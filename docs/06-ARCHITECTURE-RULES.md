# Enterprise Template - Architecture Rules

## Layer Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        INTERNET                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     DMZ (Demilitarized Zone)                     │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │           Enterprise.Api.Client                            │  │
│  │  • Mobile/Web client istekleri                            │  │
│  │  • ❌ Database erişimi YOK                                 │  │
│  │  • ✅ Sadece FILE loglama                                  │  │
│  │  • ✅ Server API'ye HTTP ile iletişim                      │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                       (HTTP/HTTPS)
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                     INTERNAL ZONE                                │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │           Enterprise.Api.Server                            │  │
│  │  • Business logic                                          │  │
│  │  • ✅ Database erişimi                                     │  │
│  │  • ✅ FILE + DATABASE loglama                              │  │
│  │  • ✅ Merkezi log yönetimi                                 │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │               DATABASE (Oracle / SqlServer)                │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Project References

### Client API (DMZ) - Minimal Dependencies
```xml
<!-- Enterprise.Api.Client.csproj -->
<ProjectReference Include="Enterprise.Core.Shared" />
<ProjectReference Include="Enterprise.Infrastructure.Logging" />
<ProjectReference Include="Enterprise.Infrastructure.CrossCutting" />

<!-- ❌ YASAK REFERANSLAR -->
<!-- Enterprise.Infrastructure.Persistence -->
<!-- Enterprise.Core.Domain -->
<!-- Enterprise.Business -->
```

### Server API (Internal) - Full Access
```xml
<!-- Enterprise.Api.Server.csproj -->
<ProjectReference Include="Enterprise.Business" />
<ProjectReference Include="Enterprise.Infrastructure.Persistence" />
<ProjectReference Include="Enterprise.Infrastructure.Logging" />
<ProjectReference Include="Enterprise.Infrastructure.CrossCutting" />
```

## Logging Architecture

### Central Logging Principle
- ❌ CommandHandler içinde `_logger.LogInformation()` **YAPMA**
- ❌ Business logic içinde log **YAZMA**
- ✅ `AutoLoggingBehavior` MediatR pipeline'da otomatik loglar
- ✅ `RequestLoggingMiddleware` HTTP request/response loglar
- ✅ `ExceptionLoggingMiddleware` hataları merkezi loglar

### Doğru Kullanım
```csharp
// ❌ YANLIŞ - Handler içinde log
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result>
{
    public async Task<Result> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating customer: {Email}", request.Email);  // ❌
        // ...
    }
}

// ✅ DOĞRU - AutoLoggingBehavior otomatik loglar
// Handler'a log eklemeye gerek yok, behavior halleder
```

### Log Tipi Konfigürasyonu
```json
{
  "Logging": {
    "File": {
      "Enabled": true,
      "SeparateFiles": {
        "AllLogs": true,
        "ErrorLogs": true,
        "RequestLogs": true,
        "PerformanceLogs": true,
        "BusinessLogs": true,
        "SecurityLogs": true
      }
    },
    "Database": {
      "Enabled": true,
      "LogTypes": {
        "RequestLogs": true,
        "ResponseLogs": true,
        "ExceptionLogs": true,
        "BusinessExceptionLogs": true,
        "AuditLogs": true,
        "PerformanceLogs": true,
        "SecurityLogs": true
      }
    }
  }
}
```

### Client API vs Server API Logging

| Özellik | Client API (DMZ) | Server API (Internal) |
|---------|------------------|----------------------|
| File Logging | ✅ Aktif | ✅ Aktif |
| Database Logging | ❌ Devre dışı | ✅ Aktif |
| ELK Logging | Opsiyonel | ✅ Aktif |
| Request/Response Body | ❌ Hassas veri | ✅ Masked |

## Database Provider Support

### Multi-Provider Configuration
```json
{
  "Database": {
    "Provider": "Oracle",
    "ConnectionStrings": {
      "SqlServer": "Server=.;Database=EnterpriseDb;...",
      "Oracle": "Data Source=localhost:1521/ORCL;..."
    },
    "OrmType": "EfCore"
  }
}
```

### Provider Değiştirme
```json
// SqlServer'a geçmek için sadece Provider değiştir
"Provider": "SqlServer"

// Oracle'a geçmek için
"Provider": "Oracle"
```

### Entity Configuration (Oracle Uyumlu)
```csharp
// ✅ DOĞRU - Oracle uyumlu
builder.ToTable("CUSTOMERS");  // Schema yok, büyük harf
builder.Property(c => c.Id)
    .HasColumnName("ID")
    .ValueGeneratedOnAdd();

// ❌ YANLIŞ - Sadece SqlServer
builder.ToTable("Customers", "dbo");  // Oracle'da dbo yok
builder.HasIndex(c => c.Email)
    .HasFilter("[IsDeleted] = 0");  // Oracle syntax değil
```

## SOLID Principles

### Single Responsibility
```
CommandHandler   → Sadece orchestration
Validator        → Sadece validation
Repository       → Sadece data access
Service          → Sadece business logic
Middleware       → Sadece cross-cutting concerns
```

### Open/Closed
- Yeni provider eklerken mevcut kodu değiştirme
- Strategy pattern ile provider desteği

### Liskov Substitution
- `IRepository<T>` interface'i tüm entity'ler için çalışır
- `IDbConnection` SqlServer ve Oracle için çalışır

### Interface Segregation
- `ILogSink` → Database, File, ELK ayrı implementasyonlar
- `IRepository<T>` → Generic, filtrelenmiş veri erişimi

### Dependency Inversion
- Business layer → Interfaces (IRepository, IUnitOfWork)
- Infrastructure → Concrete implementations

## Unit Testing Rules

### Test Coverage Targets

| Component | Min Coverage | Test Types |
|-----------|-------------|------------|
| CommandHandler | 90% | Success, Failure, NotFound, Duplicate, Validation |
| QueryHandler | 90% | Success, NotFound, Mapping |
| Validator | 100% | AllFieldsValid, EmptyFields, MaxLength, InvalidFormat |
| Controller | 80% | 200, 201, 400, 404, 422, 500 |
| Repository | 80% | CRUD operations |

### Test Naming Convention
```
{MethodName}_Should{ExpectedBehavior}_When{Condition}
```

### Test File Structure
```
tests/Enterprise.UnitTests/
├── Base/
│   └── TestBase.cs          # AutoFixture + Moq altyapısı
├── Business/
│   └── Customers/
│       ├── CreateCustomerCommandHandlerTests.cs
│       ├── GetCustomerByIdQueryHandlerTests.cs
│       └── CreateCustomerCommandValidatorTests.cs
└── Infrastructure/
    └── Logging/
        └── SensitiveDataMaskerTests.cs
```

### Handler Test Template
```csharp
public class CreateXxxCommandHandlerTests : TestBase
{
    [Fact] public async Task Handle_ShouldCreateEntity_WhenValidCommand()
    [Fact] public async Task Handle_ShouldThrowBusinessException_WhenDuplicate()
    [Fact] public async Task Handle_ShouldCallRepository_WithCorrectEntity()
    [Fact] public async Task Handle_ShouldSaveChanges_AfterCreate()
}
```

### Validator Test Template
```csharp
public class XxxValidatorTests
{
    [Fact] public async Task Validate_ShouldPass_WhenAllFieldsValid()
    [Fact] public async Task Validate_ShouldFail_WhenFieldIsEmpty()
    [Fact] public async Task Validate_ShouldFail_WhenFieldExceedsMaxLength()
    [Theory] public async Task Validate_ShouldFail_WhenFormatIsInvalid(string value)
}
```

## Change Checklist

Her kod değişikliğinde:

1. [ ] Layer separation kurallarına uygun mu?
2. [ ] Logging merkezi mi (handler'da değil)?
3. [ ] Oracle uyumlu mu (büyük harf, schema yok)?
4. [ ] SOLID prensiplerine uygun mu?
5. [ ] docs/ klasörü güncellendi mi?
6. [ ] DDL script'leri güncellendi mi?
7. [ ] Unit test eklendi mi?
8. [ ] Test coverage yeterli mi? (Handler: 90%, Validator: 100%)

## Last Updated

- **Tarih:** 3 Aralık 2024
- **Değişiklikler:**
  - Handler'lardan doğrudan loglama kaldırıldı (AutoLoggingBehavior kullanılıyor)
  - Unit test coverage artırıldı (GetCustomerByIdQueryHandler, Validator testleri eklendi)
  - 57 unit test geçiyor

