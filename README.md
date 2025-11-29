# Enterprise .NET 10 Application

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-239120)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

Kurumsal düzeyde, **Onion Architecture** tabanlı **.NET 10** uygulaması. Plugin mimarisi, middleware tabanlı loglama, switchable cache/database provider ve kapsamlı hata yönetimi özellikleri içerir.

## 🏗️ Mimari

```
┌─────────────────────────────────────────────────────────────┐
│                         INTERNET                             │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼                               ▼
┌─────────────────────────┐     ┌─────────────────────────┐
│   ClientApi (REST)      │     │   ClientApi (WCF)       │
│   Port: 5000/5001       │     │   Port: 5010/5011       │
│   DMZ - Tamamen İzole   │     │   DMZ - Tamamen İzole   │
└───────────┬─────────────┘     └───────────┬─────────────┘
            │                               │
            └───────────────┬───────────────┘
                            ▼
            ┌─────────────────────────────────┐
            │       ServerApi (Internal)      │
            │       Port: 5100/5101           │
            │       Secure Zone               │
            └───────────────┬─────────────────┘
                            │
            ┌───────────────┼───────────────┐
            ▼               ▼               ▼
    ┌───────────┐   ┌───────────┐   ┌───────────┐
    │  Business │   │   Proxy   │   │    Core   │
    │   Layer   │   │   Layer   │   │   Layer   │
    └─────┬─────┘   └─────┬─────┘   └───────────┘
          │               │
          ▼               ▼
    ┌───────────┐   ┌───────────┐
    │ Database  │   │ External  │
    │ SQL/Oracle│   │ Services  │
    └───────────┘   └───────────┘
```

## 📁 Proje Yapısı

```
Enterprise/
├── src/
│   ├── Core/
│   │   ├── Enterprise.Core.Domain/              # Entity'ler
│   │   ├── Enterprise.Core.Application/         # Interfaces, DTOs, Behaviors
│   │   └── Enterprise.Core.Shared/              # ErrorCodes, Exceptions
│   ├── Infrastructure/
│   │   ├── Enterprise.Infrastructure.Logging/   # Middleware tabanlı loglama
│   │   ├── Enterprise.Infrastructure.Persistence/ # EF Core / Dapper
│   │   ├── Enterprise.Infrastructure.Caching/   # Redis / Memory / Hybrid
│   │   └── Enterprise.Infrastructure.CrossCutting/
│   ├── Proxy/
│   │   ├── Enterprise.Proxy.Core/               # WCF & HTTP base sınıfları
│   │   └── Enterprise.Proxy.ExternalService/
│   ├── Business/
│   │   └── Enterprise.Business/
│   ├── Application/
│   │   └── Enterprise.Api.Server/               # Secure Zone API
│   └── Presentation/
│       ├── Enterprise.Api.Client/               # DMZ - REST Client
│       └── Enterprise.Api.Client.Wcf/           # DMZ - WCF Client
├── tests/
│   ├── Enterprise.UnitTests/
│   └── Enterprise.IntegrationTests/
└── docs/
```

## 🚀 Özellikler

### ✅ Plugin Mimarisi (RegisterXXXConfiguration)

```csharp
// Tek satırda tüm bağımlılıklar
services.RegisterEnterpriseServerApi(configuration);
services.RegisterEnterpriseClientApi(configuration);
services.RegisterWcfClientApi(configuration);
```

### ✅ Middleware Tabanlı Loglama

```csharp
app.UseLogging(); // Tek satır - otomatik Request/Response/Exception loglama
```

### ✅ Hata Kodu Sistemi

```csharp
public static readonly ErrorCode CustomerNotFound = new(
    "CUST-001", "Müşteri bulunamadı", "Customer not found", 404, ErrorCategory.NotFound);

throw new BusinessException(CustomerNotFound);
```

### ✅ Switchable Database Provider

```json
{
  "Database": {
    "Provider": "SqlServer",  // SqlServer, Oracle
    "OrmType": "EfCore"       // EfCore, Dapper
  }
}
```

### ✅ Switchable Cache Provider

```json
{
  "Cache": {
    "Provider": "Redis"  // Memory, Redis, Hybrid
  }
}
```

### ✅ Sensitive Data Masking

```json
{
  "SensitiveData": {
    "SensitiveFields": ["password", "token", "creditCard", "tckn"],
    "MaskCreditCards": true,
    "MaskEmails": true
  }
}
```

## 🔧 Kurulum

### Gereksinimler

- .NET 10 SDK
- SQL Server veya Oracle (opsiyonel)
- Redis (opsiyonel - cache için)

### 1. Clone

```bash
git clone https://github.com/YOUR_USERNAME/Enterprise.git
cd Enterprise
```

### 2. Veritabanı (Opsiyonel)

```sql
CREATE DATABASE EnterpriseDb;
CREATE DATABASE EnterpriseLogs;
```

### 3. Redis (Opsiyonel)

```bash
docker run -d -p 6379:6379 redis:7-alpine
```

### 4. Çalıştırma

```bash
# Tüm projeleri derle
dotnet build Enterprise.sln

# Server API (Port: 5100/5101)
cd src/Application/Enterprise.Api.Server
dotnet run

# Client API - REST (Port: 5000/5001)
cd src/Presentation/Enterprise.Api.Client
dotnet run

# Client API - WCF (Port: 5010/5011)
cd src/Presentation/Enterprise.Api.Client.Wcf
dotnet run
```

## 🌐 Port Yapılandırması

| API | HTTP | HTTPS | Swagger |
|-----|------|-------|---------|
| **ClientApi (REST)** | 5000 | 5001 | https://localhost:5001/swagger |
| **ClientApi (WCF)** | 5010 | 5011 | https://localhost:5011/swagger |
| **ServerApi** | 5100 | 5101 | https://localhost:5101/swagger |

## 🛠️ Teknoloji Stack

| Kategori | Teknoloji |
|----------|-----------|
| Framework | .NET 10, C# 14 |
| ORM | Entity Framework Core 10 / Dapper |
| Caching | Redis / MemoryCache / Hybrid |
| Logging | Serilog + ELK (opsiyonel) |
| Validation | FluentValidation |
| CQRS | MediatR |
| API Docs | Swagger/OpenAPI |
| Testing | xUnit, Moq, FluentAssertions |
| Database | SQL Server / Oracle (switchable) |
| Resilience | Polly (Retry, Circuit Breaker) |

## ⚙️ Yapılandırma

```json
{
  "Database": {
    "Provider": "SqlServer",
    "OrmType": "EfCore"
  },
  "Cache": {
    "Provider": "Memory"
  },
  "Logging": {
    "ApplicationName": "Enterprise.Api.Server",
    "Elk": {
      "Enabled": false
    }
  },
  "SensitiveData": {
    "SensitiveFields": ["password", "token", "creditCard"]
  }
}
```

## 📋 Registration Methods

| Method | Açıklama |
|--------|----------|
| `RegisterEnterpriseServerApi(config)` | Server API + tüm bağımlılıklar |
| `RegisterEnterpriseClientApi(config)` | Client API (REST, izole) |
| `RegisterWcfClientApi(config)` | Client API (WCF, izole) |
| `RegisterEnterpriseBusiness()` | Business katmanı |
| `RegisterLogging(config)` | Middleware loglama |
| `RegisterCaching(config)` | Redis/Memory cache |
| `RegisterPersistence(config)` | EF Core/Dapper |
| `UseLogging()` | Middleware pipeline |

## 📚 Dokümantasyon

| Doküman | Açıklama |
|---------|----------|
| [01-PROJECT-PROMPTS.md](docs/01-PROJECT-PROMPTS.md) | Geliştirme promptları |
| [02-LOGGING-ARCHITECTURE-REPORT.md](docs/02-LOGGING-ARCHITECTURE-REPORT.md) | Loglama mimarisi |
| [03-HIGH-LEVEL-DESIGN.md](docs/03-HIGH-LEVEL-DESIGN.md) | High Level Design (HLD) |
| [04-UNIT-TEST-GUIDE.md](docs/04-UNIT-TEST-GUIDE.md) | Unit Test Kılavuzu |
| [05-PROXY-ADAPTATION-GUIDE.md](docs/05-PROXY-ADAPTATION-GUIDE.md) | Proxy Adaptasyon Rehberi |

## 🧪 Test

```bash
# Unit tests
dotnet test tests/Enterprise.UnitTests/Enterprise.UnitTests.csproj

# Integration tests
dotnet test tests/Enterprise.IntegrationTests/Enterprise.IntegrationTests.csproj

# Tüm testler
dotnet test Enterprise.sln
```

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📧 İletişim

Sorularınız için issue açabilirsiniz.
