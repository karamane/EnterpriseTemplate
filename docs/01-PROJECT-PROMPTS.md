# .NET 10 Enterprise Uygulama - Geliştirme Promptları

**Versiyon:** 5.0  
**Tarih:** Kasım 2025  
**Framework:** .NET 10, C# 14

---

## İçindekiler

1. [Proje Yapısı](#1-proje-yapısı)
2. [DTO ve Mapping Stratejisi](#2-dto-ve-mapping-stratejisi)
3. [Registration Configuration](#3-registration-configuration)
4. [Hata Kodu Sistemi](#4-hata-kodu-sistemi)
5. [Middleware Tabanlı Loglama](#5-middleware-tabanlı-loglama)
6. [Proxy Katmanı (WCF & HTTP)](#6-proxy-katmanı-wcf--http)
7. [Client API İzolasyonu](#7-client-api-izolasyonu)
8. [Veritabanı Provider Switch](#8-veritabanı-provider-switch)
9. [Cache Provider Switch](#9-cache-provider-switch)
10. [Sensitive Data Maskeleme](#10-sensitive-data-maskeleme)
11. [WCF Client API](#11-wcf-client-api)

---

## 1. Proje Yapısı

### 1.1 Klasör Yapısı

```
Enterprise/
├── src/
│   ├── Core/
│   │   ├── Enterprise.Core.Domain/              # Entity'ler
│   │   ├── Enterprise.Core.Application/         # Interfaces, DTOs, Behaviors
│   │   └── Enterprise.Core.Shared/              # Utilities, ErrorCodes, Exceptions
│   ├── Infrastructure/
│   │   ├── Enterprise.Infrastructure.Logging/   # Middleware tabanlı loglama
│   │   ├── Enterprise.Infrastructure.Persistence/
│   │   ├── Enterprise.Infrastructure.Caching/   # Redis & Memory switch
│   │   └── Enterprise.Infrastructure.CrossCutting/
│   ├── Proxy/
│   │   ├── Enterprise.Proxy.Core/               # WCF & HTTP base sınıfları
│   │   └── Enterprise.Proxy.ExternalService/
│   ├── Business/
│   │   └── Enterprise.Business/
│   ├── Application/
│   │   └── Enterprise.Api.Server/               # Secure Zone API
│   └── Presentation/
│       ├── Enterprise.Api.Client/               # DMZ - REST Client (Tamamen izole)
│       └── Enterprise.Api.Client.Wcf/           # DMZ - WCF Client (Tamamen izole)
├── tests/
└── docs/
```

### 1.2 Katman Sorumlulukları

| Katman | Sorumluluk |
|--------|------------|
| **Presentation (Client API)** | DMZ, Mobil uygulamalar için, **tamamen izole** |
| **Presentation (WCF Client API)** | DMZ, WCF servisleri için, **tamamen izole** |
| **Application (Server API)** | Secure Zone, Internal API |
| **Business** | MediatR handlers, iş kuralları |
| **Infrastructure** | Logging, Persistence, Caching |
| **Proxy** | Dış servisler (WCF, HTTP) |
| **Core** | Domain entities, ErrorCodes, Exceptions |

### 1.3 Port Yapılandırması

| API | HTTP | HTTPS | Swagger URL |
|-----|------|-------|-------------|
| **ClientApi (REST)** | 5000 | 5001 | https://localhost:5001/swagger |
| **ClientApi (WCF)** | 5010 | 5011 | https://localhost:5011/swagger |
| **ServerApi** | 5100 | 5101 | https://localhost:5101/swagger |

---

## 2. DTO ve Mapping Stratejisi

### 2.1 Katman Bazlı DTO'lar

Her katman **kendi DTO'larına** sahiptir:

```
Mobile → ClientDTO → ServerDTO → BusinessCommand → Entity → Database
```

| Katman | Namespace | Örnek |
|--------|-----------|-------|
| **ClientApi** | `Enterprise.Api.Client.DTOs` | `CustomerClientResponse` |
| **WcfClientApi** | `Enterprise.Api.Client.Wcf.DTOs` | `WcfCustomerResponse` |
| **ServerApi** | `Enterprise.Api.Server.DTOs` | `CustomerApiResponse` |
| **Business** | `Enterprise.Business.DTOs` | `CustomerBusinessDto` |

### 2.2 Client API İzolasyonu

Client API **hiçbir internal referans** içermez:

```csharp
// Client API kendi Server API contract'larını tanımlar
public record ServerCreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email);

// Mapping Client DTO -> Server Contract
CreateMap<CreateCustomerClientRequest, ServerCreateCustomerRequest>();
```

---

## 3. Registration Configuration

### 3.1 Yeni İsimlendirme Standardı

Tüm registration sınıfları `RegisterXXXConfiguration` formatında isimlendirilir:

| Eski İsim | Yeni İsim |
|-----------|-----------|
| `LoggingServiceExtensions` | `RegisterLoggingConfiguration` |
| `CachingServiceExtensions` | `RegisterCachingConfiguration` |
| `PersistenceServiceExtensions` | `RegisterPersistenceConfiguration` |
| `ApplicationServiceExtensions` | `RegisterApplicationConfiguration` |
| `BusinessServiceExtensions` | `RegisterBusinessConfiguration` |
| `ClientApiServiceExtensions` | `RegisterClientApiConfiguration` |
| `ServerApiServiceExtensions` | `RegisterServerApiConfiguration` |
| `WcfClientApiServiceExtensions` | `RegisterWcfClientApiConfiguration` |
| `CrossCuttingServiceExtensions` | `RegisterCrossCuttingConfiguration` |
| `ProxyCoreServiceExtensions` | `RegisterProxyCoreConfiguration` |
| `ExternalServiceProxyExtensions` | `RegisterExternalServiceProxyConfiguration` |

### 3.2 Method İsimlendirme

| Eski Method | Yeni Method |
|-------------|-------------|
| `AddEnterpriseLogging()` | `RegisterLogging()` |
| `AddEnterpriseCaching()` | `RegisterCaching()` |
| `AddEnterprisePersistence()` | `RegisterPersistence()` |
| `AddApplicationServices()` | `RegisterApplication()` |
| `AddBusinessServices()` | `RegisterBusiness()` |
| `AddEnterpriseServerApi()` | `RegisterEnterpriseServerApi()` |
| `AddEnterpriseClientApi()` | `RegisterEnterpriseClientApi()` |
| `AddProxyCoreServices()` | `RegisterProxyCore()` |
| `AddExternalServiceProxy()` | `RegisterExternalServiceProxy()` |

### 3.3 Plugin Mimarisi

Her proje **tek satırda** uygulamaya dahil edilebilir:

```csharp
// Server API - Tüm bağımlılıklar dahil
services.RegisterEnterpriseServerApi(configuration);

// Client API - Tamamen izole
services.RegisterEnterpriseClientApi(configuration);

// WCF Client API
services.RegisterWcfClientApi(configuration);

// Sadece ihtiyaç duyulan katmanlar
services.RegisterEnterpriseBusiness();
services.RegisterLogging(configuration);
services.RegisterPersistence(configuration);
services.RegisterCaching(configuration);
```

### 3.4 Middleware Registration

```csharp
// Logging middleware
app.UseLogging();

// WCF Client API middleware
app.UseWcfClientApi();
```

---

## 4. Hata Kodu Sistemi

### 4.1 ErrorCode Tanımlama

Developer'lar kolayca hata kodu tanımlayabilir:

```csharp
public static class CustomerErrorCodes
{
    public static readonly ErrorCode CustomerNotFound = new(
        "CUST-001",
        "Müşteri bulunamadı",
        "Customer not found",
        404,
        ErrorCategory.NotFound);

    public static readonly ErrorCode EmailAlreadyExists = new(
        "CUST-002",
        "Bu email adresi zaten kayıtlı",
        "Email already exists",
        409,
        ErrorCategory.Conflict);
}
```

### 4.2 BusinessException Kullanımı

```csharp
// Basit kullanım
throw new BusinessException(CustomerErrorCodes.CustomerNotFound);

// Orijinal hatayı koruyarak
try
{
    await externalService.CallAsync();
}
catch (Exception ex)
{
    // Önce logla, sonra business exception fırlat
    throw new BusinessException(
        CommonErrorCodes.ExternalServiceError,
        ex,  // Orijinal exception korunur
        new Dictionary<string, object>
        {
            ["ServiceName"] = "PaymentService",
            ["OriginalError"] = ex.Message
        });
}

// Factory ile
throw BusinessExceptionFactory.NotFound("Customer", customerId);
throw BusinessExceptionFactory.Duplicate("Email", email);
```

---

## 5. Middleware Tabanlı Loglama

### 5.1 Otomatik Loglama

Metod bazlı log yazmaya **gerek yok** - middleware otomatik loglar:

```csharp
// Program.cs
app.UseLogging(); // Tek satır!

// Bu middleware şunları otomatik loglar:
// - Request/Response (body dahil)
// - Action başlangıç/bitiş
// - Exception'lar
// - Performance metrikleri
```

### 5.2 Türkiye Saat Dilimi

Tüm loglar **Türkiye saati (UTC+3)** ile kaydedilir:

```
[29.11.2024 15:30:45 INF] [abc123] Request started: GET /api/customers
```

### 5.3 Middleware Pipeline Sırası

```
Request
   │
   ▼
┌─────────────────────────────┐
│ ExceptionLoggingMiddleware  │ ← Tüm hataları yakalar
├─────────────────────────────┤
│ CorrelationIdMiddleware     │ ← Her request'e ID atar
├─────────────────────────────┤
│ RequestLoggingMiddleware    │ ← Request/Response loglar
├─────────────────────────────┤
│ ActionLoggingMiddleware     │ ← Controller action loglar
└─────────────────────────────┘
   │
   ▼
Controller → Business → Domain
```

---

## 6. Proxy Katmanı (WCF & HTTP)

### 6.1 WCF Proxy (Dispatcher ile Loglama)

```csharp
public class PaymentServiceProxy : WcfProxyBase<IPaymentService>
{
    public PaymentServiceProxy(...)
        : base(logger, correlationContext, logService, "PaymentService")
    {
        CreateChannel("https://payment.example.com/service.svc");
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        return await ExecuteAsync(
            channel => channel.ProcessPaymentAsync(request),
            "ProcessPayment",
            PaymentErrorCodes.PaymentFailed);
    }
}
```

### 6.2 HTTP Proxy

```csharp
public class InventoryServiceProxy : HttpProxyBase
{
    public async Task<StockInfo?> GetStockAsync(string productId)
    {
        return await GetAsync<StockInfo>(
            $"/api/stock/{productId}",
            InventoryErrorCodes.StockCheckFailed);
    }
}
```

---

## 7. Client API İzolasyonu

### 7.1 Referans Yapısı

```xml
<!-- Client API sadece bunlara referans verir -->
<ProjectReference Include="Enterprise.Core.Shared" />
<ProjectReference Include="Enterprise.Infrastructure.Logging" />
<ProjectReference Include="Enterprise.Infrastructure.CrossCutting" />

<!-- Server API referansı YOK! -->
```

---

## 8. Veritabanı Provider Switch

### 8.1 Yapılandırma

```json
{
  "Database": {
    "Provider": "SqlServer",  // SqlServer, Oracle, PostgreSql, MySql, SQLite
    "ConnectionString": "...",
    "OrmType": "EfCore"       // EfCore, Dapper
  }
}
```

### 8.2 Desteklenen Provider'lar

| Provider | EF Core Paketi | Dapper Paketi |
|----------|---------------|---------------|
| **SqlServer** | Dahil | Microsoft.Data.SqlClient |
| **Oracle** | Oracle.EntityFrameworkCore | Oracle.ManagedDataAccess.Core |
| **PostgreSql** | Npgsql.EntityFrameworkCore.PostgreSQL | Npgsql |
| **MySql** | Pomelo.EntityFrameworkCore.MySql | MySqlConnector |
| **SQLite** | Dahil | Microsoft.Data.Sqlite |

---

## 9. Cache Provider Switch

### 9.1 Yapılandırma

```json
{
  "Cache": {
    "Provider": "Redis",  // Memory, Redis, Hybrid
    "ConnectionString": "localhost:6379",
    "InstanceName": "Enterprise_",
    "DefaultExpirationMinutes": 30,
    "FallbackToMemory": true
  }
}
```

### 9.2 Provider Tipleri

| Provider | Açıklama | Kullanım |
|----------|----------|----------|
| **Memory** | In-process cache | Tek sunucu, development |
| **Redis** | Distributed cache | Multi-server, production |
| **Hybrid** | L1: Memory, L2: Redis | Maksimum performans |

### 9.3 Kullanım

```csharp
// DI ile inject
private readonly ICacheService _cache;

// Get/Set
var customer = await _cache.GetOrSetAsync(
    $"customer:{id}",
    () => _repository.GetByIdAsync(id),
    TimeSpan.FromMinutes(30));

// Remove
await _cache.RemoveAsync($"customer:{id}");
```

---

## 10. Sensitive Data Maskeleme

### 10.1 Konfigürasyon

```json
{
  "SensitiveData": {
    "SensitiveFields": [
      "password", "token", "secret", "creditCard", "cvv", "tckn"
    ],
    "MaskCreditCards": true,
    "MaskEmails": true,
    "MaskPhoneNumbers": true,
    "MaskIbans": true,
    "MaskedText": "***MASKED***"
  }
}
```

### 10.2 Otomatik Maskeleme

| Veri Tipi | Girdi | Çıktı |
|-----------|-------|-------|
| **Kredi Kartı** | 4111-1111-1111-1234 | ****-****-****-1234 |
| **Email** | user@example.com | us***@example.com |
| **Telefon** | 05321234567 | ***4567 |
| **IBAN** | TR330006100519786457841326 | TR33***1326 |

---

## 11. WCF Client API

### 11.1 Yapı

WCF servisleri üzerinden çalışan alternatif Client API:

```
Enterprise.Api.Client.Wcf/
├── DTOs/
│   ├── WcfClientDtos.cs        # Client'a özgü DTO'lar
│   └── WcfServiceContracts.cs  # WCF service contract'ları
├── Services/
│   ├── IWcfServiceContracts.cs # WCF service interface'leri
│   ├── CustomerWcfClient.cs    # Customer WCF client
│   └── OrderWcfClient.cs       # Order WCF client
├── Controllers/
│   ├── CustomersController.cs
│   └── OrdersController.cs
└── Extensions/
    └── RegisterWcfClientApiConfiguration.cs
```

### 11.2 Yapılandırma

```json
{
  "WcfClient": {
    "CustomerServiceEndpoint": "http://localhost:5001/CustomerService.svc",
    "OrderServiceEndpoint": "http://localhost:5001/OrderService.svc",
    "Binding": "BasicHttp",
    "TimeoutSeconds": 30
  }
}
```

### 11.3 Kullanım

```csharp
// Program.cs
services.RegisterWcfClientApi(configuration);
app.UseWcfClientApi();
```

---

---

## 12. Swagger Yapılandırması

### 12.1 Konfigürasyon

```json
{
  "Swagger": {
    "Enabled": true,
    "EnableUI": true,
    "Title": "Enterprise API",
    "Version": "v1",
    "Description": "API Description",
    "EnableBearerAuth": true,
    "EnableApiKeyAuth": false,
    "IncludeXmlComments": true,
    "RoutePrefix": "swagger",
    "Theme": "light",
    "DocExpansion": "list",
    "AllowedEnvironments": ["Development", "Staging"]
  }
}
```

### 12.2 Özellikler

| Özellik | Açıklama |
|---------|----------|
| `Enabled` | Swagger tamamen açık/kapalı |
| `EnableUI` | Swagger UI açık/kapalı |
| `EnableBearerAuth` | JWT Bearer token desteği |
| `EnableApiKeyAuth` | API Key desteği |
| `AllowedEnvironments` | Sadece belirli ortamlarda göster (boş = hepsinde) |
| `Theme` | light veya dark tema |

### 12.3 Ortam Bazlı Kontrol

```json
// Production'da Swagger kapalı
{
  "Swagger": {
    "Enabled": true,
    "AllowedEnvironments": ["Development", "Staging"]
  }
}
```

---

## 13. Konsol Startup Banner

API'ler başlatıldığında konsolda bilgilendirici banner gösterilir:

```
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║  Enterprise.Api.Server                                       ║
║                                                              ║
╠══════════════════════════════════════════════════════════════╣
║  ✓ Status      : Running                                     ║
║  • Environment : Development                                 ║
║  • Version     : 1.0.0                                       ║
║  • URLs        : https://localhost:7001                      ║
║  • Swagger     : https://localhost:7001/swagger              ║
║  • Health      : /health                                     ║
║  • Started     : 29.11.2024 15:30:45                         ║
╚══════════════════════════════════════════════════════════════╝
```

---

## Özet

| Özellik | Açıklama |
|---------|----------|
| **Registration Pattern** | `RegisterXXXConfiguration` formatı |
| **Client API İzolasyonu** | Server API referansı yok, kendi contract'ları var |
| **WCF Client API** | Alternatif WCF tabanlı client |
| **Middleware Loglama** | Metod bazlı log yok, otomatik |
| **ErrorCode Sistemi** | Developer-friendly hata kodu tanımlama |
| **Türkiye Saati** | Tüm loglar UTC+3 ile |
| **Database Switch** | SqlServer / Oracle / PostgreSql / MySql / SQLite |
| **Cache Switch** | Memory / Redis / Hybrid |
| **Sensitive Data** | Konfigüratif maskeleme |
| **Swagger** | Parametrik, ortam bazlı kontrol |
| **Startup Banner** | Konsol bilgilendirme |
