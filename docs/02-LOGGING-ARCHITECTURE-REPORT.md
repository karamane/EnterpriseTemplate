# .NET 10 Enterprise Uygulama - Loglama Mimarisi Raporu

**Versiyon:** 4.0  
**Tarih:** Kasım 2025  
**Framework:** .NET 10, C# 14

---

## İçindekiler

1. [Yönetici Özeti](#1-yönetici-özeti)
2. [Middleware Tabanlı Loglama](#2-middleware-tabanlı-loglama)
3. [Hata Kodu Sistemi](#3-hata-kodu-sistemi)
4. [Proxy Loglama (WCF & HTTP)](#4-proxy-loglama-wcf--http)
5. [Correlation ID Mekanizması](#5-correlation-id-mekanizması)
6. [Log Türleri](#6-log-türleri)
7. [Veritabanı ve ELK](#7-veritabanı-ve-elk)

---

## 1. Yönetici Özeti

### 1.1 Temel Özellikler

| Özellik | Açıklama |
|---------|----------|
| **Middleware Loglama** | Metod bazlı log yok, tamamen otomatik |
| **ErrorCode Sistemi** | Developer-friendly hata kodu tanımlama |
| **WCF Dispatcher** | SOAP request/response otomatik loglama |
| **HTTP Proxy** | REST çağrıları otomatik loglama |
| **Correlation ID** | Uçtan uca request tracking |
| **Multi-Sink** | Database, ELK, File |

### 1.2 Mimari Değişiklikler

- ✅ **Metod bazlı loglama kaldırıldı** → Middleware kullanılıyor
- ✅ **Server API** → Application katmanına taşındı
- ✅ **Client API** → Tamamen izole, Server API referansı yok
- ✅ **ErrorCode sistemi** → Developer'lar kolayca hata tanımlayabilir
- ✅ **WCF Dispatcher** → SOAP loglaması otomatik

---

## 2. Middleware Tabanlı Loglama

### 2.1 Neden Middleware?

**Eski Yaklaşım (Kötü):**
```csharp
public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request)
{
    _logger.LogInformation("CreateCustomer started"); // Her metoda yazılıyor!
    try
    {
        var result = await _service.CreateAsync(request);
        _logger.LogInformation("CreateCustomer completed"); // Tekrar!
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "CreateCustomer failed"); // Tekrar!
        throw;
    }
}
```

**Yeni Yaklaşım (İyi):**
```csharp
public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request)
{
    // Hiç log kodu yok - middleware otomatik hallediyor!
    var result = await _service.CreateAsync(request);
    return Ok(result);
}
```

### 2.2 Middleware Pipeline

```
app.UseEnterpriseLogging(); // Tek satır!
```

Bu tek satır şunları yapar:
1. `ExceptionLoggingMiddleware` - Tüm hataları yakalar
2. `CorrelationIdMiddleware` - Her request'e ID atar
3. `RequestLoggingMiddleware` - Request/Response loglar
4. `ActionLoggingMiddleware` - Controller action loglar

### 2.3 MediatR Auto-Logging

```csharp
// AutoLoggingBehavior otomatik register edilir
public class AutoLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, ...)
    {
        // Başlangıç logu
        _logger.LogInformation("[{CorrelationId}] MediatR START: {RequestName}", ...);
        
        var response = await next();
        
        // Bitiş logu
        _logger.LogInformation("[{CorrelationId}] MediatR END: {RequestName} | Duration: {Duration}ms", ...);
        
        return response;
    }
}
```

---

## 3. Hata Kodu Sistemi

### 3.1 ErrorCode Yapısı

```csharp
public record ErrorCode
{
    public string Code { get; init; }           // "CUST-001"
    public string UserMessage { get; init; }    // "Müşteri bulunamadı"
    public string TechnicalMessage { get; init; } // "Customer not found"
    public int HttpStatusCode { get; init; }    // 404
    public ErrorCategory Category { get; init; } // NotFound
    public ErrorSeverity Severity { get; init; } // Error
}
```

### 3.2 Modül Bazlı Hata Kodları

```csharp
// Her modül kendi ErrorCodes sınıfını tanımlar
public static class CustomerErrorCodes
{
    public static readonly ErrorCode CustomerNotFound = new(
        "CUST-001", "Müşteri bulunamadı", "Customer not found", 404);

    public static readonly ErrorCode EmailAlreadyExists = new(
        "CUST-002", "Bu email zaten kayıtlı", "Email exists", 409);
}

public static class PaymentErrorCodes
{
    public static readonly ErrorCode PaymentFailed = new(
        "PAY-001", "Ödeme işlemi başarısız", "Payment failed", 422);
}
```

### 3.3 BusinessException

```csharp
// Basit kullanım
throw new BusinessException(CustomerErrorCodes.CustomerNotFound);

// Orijinal hatayı koruyarak (KRİTİK!)
catch (Exception ex)
{
    // 1. Önce mevcut hatayı logla
    // 2. BusinessException fırlat (ex innerException olarak)
    throw new BusinessException(errorCode, ex, additionalData);
}
```

---

## 4. Proxy Loglama (WCF & HTTP)

### 4.1 WCF Dispatcher ile Loglama

```
WCF Request
    │
    ▼
┌──────────────────────────────────┐
│ WcfLoggingInspector              │
│ (IClientMessageInspector)        │
├──────────────────────────────────┤
│ BeforeSendRequest:               │
│  - SOAP body logla               │
│  - Correlation ID header ekle    │
│  - Başlangıç zamanı kaydet       │
├──────────────────────────────────┤
│ AfterReceiveReply:               │
│  - SOAP response logla           │
│  - Duration hesapla              │
│  - Fault kontrolü                │
│  - Performance log               │
└──────────────────────────────────┘
    │
    ▼
WCF Service
```

### 4.2 HTTP Proxy Loglama

```csharp
// HttpProxyBase otomatik şunları yapar:
// 1. Request başlangıcını loglar
// 2. Correlation ID'yi header'a ekler
// 3. Response'u loglar
// 4. Exception'ları yakalar ve BusinessException'a çevirir
// 5. Performance metriklerini loglar

protected async Task<TResponse?> GetAsync<TResponse>(
    string endpoint,
    ErrorCode? customErrorCode = null)
{
    // Tüm loglama otomatik!
}
```

### 4.3 Exception Handling Akışı

```
1. Dış servisten exception gelir
   │
   ▼
2. ÖNCE orijinal exception loglanır (KRİTİK!)
   _logger.LogError(ex, "...");
   │
   ▼
3. ExceptionLogEntry oluşturulur
   - ExceptionType, Message, StackTrace
   - ServiceName, Endpoint
   - CorrelationId
   - AdditionalData
   │
   ▼
4. BusinessException fırlatılır
   - ErrorCode ile (CUST-001, PAY-001, vs.)
   - Orijinal exception innerException olarak
   - Ek metadata ile
```

---

## 5. Correlation ID Mekanizması

### 5.1 Akış

```
Mobile App
    │ X-Correlation-ID: abc123
    ▼
Client API (DMZ)
    │ Header'dan alır veya yeni üretir
    │ X-Correlation-ID: abc123
    ▼
Server API (Secure Zone)
    │ Header'dan propagate eder
    │ X-Correlation-ID: abc123
    ▼
Business Layer
    │ ICorrelationContext'ten alır
    │ abc123
    ▼
WCF/HTTP Proxy
    │ SOAP Header / HTTP Header olarak gönderir
    │ X-Correlation-ID: abc123
    ▼
External Service
```

### 5.2 Format

```
{timestamp}-{guid}-{servername}
20241129-a1b2c3d4-SRV01
```

---

## 6. Log Türleri

| Log Türü | Middleware | İçerik |
|----------|------------|--------|
| **RequestLog** | RequestLoggingMiddleware | Method, Path, Body, Headers |
| **ResponseLog** | RequestLoggingMiddleware | StatusCode, Body, Duration |
| **ExceptionLog** | ExceptionLoggingMiddleware | Type, Message, StackTrace, LayerName |
| **PerformanceLog** | ActionLoggingMiddleware | OperationName, Duration, Success |
| **AuditLog** | Manuel | Entity changes, User actions |
| **BusinessExceptionLog** | Manual/Middleware | ErrorCode, UserMessage |

---

## 7. Veritabanı ve ELK

### 7.1 Yapılandırma

```json
{
  "Logging": {
    "Database": {
      "Enabled": true,
      "ConnectionString": "..."
    },
    "Elk": {
      "Enabled": false,  // Parametre ile açılır
      "ElasticsearchUrl": "http://localhost:9200",
      "IndexFormat": "enterprise-{0:yyyy.MM.dd}"
    }
  }
}
```

### 7.2 Multi-Sink Yapısı

```
Log Entry
    │
    ├─────────────────────────────────────┐
    │                                     │
    ▼                                     ▼
Database Sink                        ELK Sink
(Her zaman aktif)                   (Parametre ile)
    │                                     │
    ▼                                     ▼
SQL Server                          Elasticsearch
(Kritik veriler)                    (Analitik)
```

---

## Özet

### Değişiklikler

1. ✅ **Middleware Loglama** - Metod bazlı log yazılmıyor
2. ✅ **ErrorCode Sistemi** - Developer-friendly hata kodları
3. ✅ **BusinessException** - Orijinal hatayı koruyan exception
4. ✅ **WCF Dispatcher** - SOAP otomatik loglama
5. ✅ **HTTP Proxy** - REST otomatik loglama
6. ✅ **Client API İzolasyonu** - Server API referansı yok

### Avantajlar

| Özellik | Avantaj |
|---------|---------|
| **Middleware** | Kod tekrarı yok, tutarlı loglama |
| **ErrorCode** | Standart hata mesajları, kolay hata takibi |
| **Proxy Loglama** | Dış servis çağrıları izlenebilir |
| **Correlation ID** | Uçtan uca request tracking |
| **Database Switch** | SqlServer / Oracle arası kolay geçiş |

---

## 8. Veritabanı Provider Switch

### 8.1 Desteklenen Provider'lar

- **SqlServer** - Microsoft SQL Server
- **Oracle** - Oracle Database
- **PostgreSql** - PostgreSQL
- **MySql** - MySQL / MariaDB
- **SQLite** - SQLite (test için)

### 8.2 Yapılandırma

```json
{
  "Database": {
    "Provider": "SqlServer",
    "ConnectionString": "...",
    "OrmType": "EfCore"
  }
}
```

### 8.3 Oracle'a Geçiş

1. Paketleri ekle:
   ```bash
   dotnet add package Oracle.EntityFrameworkCore
   ```

2. Yapılandırmayı değiştir:
   ```json
   {
     "Database": {
       "Provider": "Oracle",
       "ConnectionString": "Data Source=...;User Id=...;Password=...;"
     }
   }
   ```

3. Yeniden başlat - Kod değişikliği gerekmez!

---

## 9. Türkiye Saat Dilimi

### 9.1 Tüm Loglar Türkiye Saatinde

Uygulama UTC yerine **Türkiye saati (UTC+3)** kullanır:

```csharp
// TimeZoneHelper kullanımı
var now = TimeZoneHelper.NowTurkey;           // DateTime
var nowOffset = TimeZoneHelper.NowTurkeyOffset; // DateTimeOffset

// Extension method
var turkeyTime = utcDateTime.ToTurkeyTime();
var formatted = dateTime.ToTurkeyString();    // "29.11.2024 15:30:45"
```

### 9.2 Log Formatı

```
[29.11.2024 15:30:45 INF] [abc123] Customer created successfully
```

### 9.3 Veritabanında

- `Timestamp` - Türkiye saati (UTC+3)
- `TimestampUtc` - UTC (karşılaştırma için)

---

## 10. Konfigüratif Sensitive Data Maskeleme

### 10.1 Yapılandırma

```json
{
  "SensitiveData": {
    "SensitiveFields": [
      "password",
      "token",
      "secret",
      "creditCard",
      "cvv",
      "tckn",
      "identityNumber"
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

| Veri Tipi | Örnek Girdi | Maskelenmiş |
|-----------|-------------|-------------|
| **Kredi Kartı** | 4111-1111-1111-1234 | ****-****-****-1234 |
| **Email** | user@example.com | us***@example.com |
| **Telefon** | 05321234567 | ***4567 |
| **IBAN** | TR330006100519786457841326 | TR33***1326 |

### 10.3 Field Bazlı Maskeleme

```json
// Girdi
{
  "username": "john",
  "password": "secret123",
  "email": "john@example.com"
}

// Log'da görünüm
{
  "username": "john",
  "password": "***MASKED***",
  "email": "jo***@example.com"
}
```

### 10.4 Kullanım

```csharp
// DI ile inject
private readonly ISensitiveDataMasker _masker;

// JSON maskeleme
var maskedJson = _masker.MaskJson(requestBody);

// Text maskeleme
var maskedText = _masker.MaskText(logMessage);

// Dictionary maskeleme
var maskedHeaders = _masker.MaskDictionary(headers);
```
