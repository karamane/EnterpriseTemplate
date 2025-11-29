# Proxy Servisleri Adaptasyon Rehberi

**Versiyon:** 1.0  
**Tarih:** Kasım 2025  
**Framework:** .NET 10, C# 14

Bu doküman, Enterprise uygulamasında dış servislerin (HTTP ve WCF) nasıl entegre edileceğini ve adapte edileceğini açıklamaktadır.

## İçindekiler

1. [Genel Bakış](#1-genel-bakış)
2. [Proxy Mimarisi](#2-proxy-mimarisi)
3. [HTTP Proxy Adaptasyonu](#3-http-proxy-adaptasyonu)
4. [WCF Proxy Adaptasyonu](#4-wcf-proxy-adaptasyonu)
5. [Loglama ve Correlation ID](#5-loglama-ve-correlation-id)
6. [Hata Yönetimi ve BusinessException](#6-hata-yönetimi-ve-businessexception)
7. [Resilience Patterns](#7-resilience-patterns)
8. [Konfigürasyon](#8-konfigürasyon)
9. [Yeni Proxy Ekleme Adımları](#9-yeni-proxy-ekleme-adımları)
10. [Test Stratejileri](#10-test-stratejileri)
11. [Best Practices](#11-best-practices)

---

## 1. Genel Bakış

### 1.1 Proxy Nedir?

Proxy katmanı, uygulamanın dış servislerle iletişimini yönetir. Bu servisler şunlar olabilir:
- REST API'ler (HTTP)
- SOAP Web Servisleri (WCF)
- gRPC Servisleri
- Message Queue'lar

### 1.2 Proxy Katmanının Sorumlulukları

| Sorumluluk | Açıklama |
|------------|----------|
| **İletişim** | Dış servislerle request/response yönetimi |
| **Loglama** | Tüm request/response'ların otomatik loglanması |
| **Correlation** | End-to-end takip için Correlation ID propagasyonu |
| **Hata Yönetimi** | Exception'ların yakalanması ve BusinessException'a dönüştürülmesi |
| **Resilience** | Retry, Circuit Breaker, Timeout politikaları |
| **DTO Mapping** | Dış servis DTO'larının iç DTO'lara çevrilmesi |

### 1.3 Klasör Yapısı

```
src/Proxy/
├── Enterprise.Proxy.Core/              # Base sınıflar ve ortak bileşenler
│   ├── Base/
│   │   └── BaseHttpProxy.cs            # Generic HTTP proxy base
│   ├── Http/
│   │   └── HttpProxyBase.cs            # HTTP proxy base sınıfı
│   ├── Wcf/
│   │   ├── WcfProxyBase.cs             # WCF proxy base sınıfı
│   │   └── WcfLoggingInspector.cs      # WCF message inspector
│   └── Extensions/
│       └── RegisterProxyCoreConfiguration.cs
│
├── Enterprise.Proxy.ExternalService/   # Örnek external service proxy
│   ├── DTOs/
│   ├── Extensions/
│   │   └── RegisterExternalServiceProxyConfiguration.cs
│   └── SampleExternalServiceProxy.cs
│
├── Enterprise.Proxy.PaymentService/    # Ödeme servisi proxy (örnek)
│   ├── DTOs/
│   ├── Extensions/
│   └── PaymentServiceProxy.cs
│
└── Enterprise.Proxy.CustomerService/   # Müşteri servisi proxy (WCF örnek)
    ├── DTOs/
    ├── Contracts/
    ├── Extensions/
    └── CustomerWcfProxy.cs
```

---

## 2. Proxy Mimarisi

### 2.1 Mimari Diyagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           Business Layer                                 │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                        IPaymentProxy                             │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            Proxy Layer                                   │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                      PaymentServiceProxy                         │   │
│  │  ┌────────────────────────────────────────────────────────────┐ │   │
│  │  │                    HttpProxyBase                            │ │   │
│  │  │  - Otomatik Loglama                                        │ │   │
│  │  │  - Correlation ID Propagasyonu                             │ │   │
│  │  │  - Exception Handling                                       │ │   │
│  │  │  - Performance Tracking                                     │ │   │
│  │  └────────────────────────────────────────────────────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────┘   │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │                       Resilience Layer                           │   │
│  │  - Retry Policy (Exponential Backoff)                           │   │
│  │  - Circuit Breaker                                               │   │
│  │  - Timeout Policy                                                │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         External Services                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐                  │
│  │  REST API    │  │  SOAP/WCF    │  │    gRPC      │                  │
│  └──────────────┘  └──────────────┘  └──────────────┘                  │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Sınıf Hiyerarşisi

```
                    ┌─────────────────────┐
                    │    HttpProxyBase    │
                    │  (Abstract Class)   │
                    └─────────────────────┘
                              △
                              │
            ┌─────────────────┼─────────────────┐
            │                 │                 │
  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
  │ PaymentProxy    │ │ CustomerProxy   │ │ NotificationProxy│
  └─────────────────┘ └─────────────────┘ └─────────────────┘


                    ┌─────────────────────┐
                    │  WcfProxyBase<T>    │
                    │  (Abstract Class)   │
                    └─────────────────────┘
                              △
                              │
            ┌─────────────────┼─────────────────┐
            │                 │                 │
  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
  │ LegacyWcfProxy  │ │ BankWcfProxy    │ │ ReportWcfProxy  │
  └─────────────────┘ └─────────────────┘ └─────────────────┘
```

---

## 3. HTTP Proxy Adaptasyonu

### 3.1 HttpProxyBase Sınıfı

`HttpProxyBase` sınıfı, HTTP tabanlı dış servisler için temel işlevselliği sağlar:

```csharp
public abstract class HttpProxyBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;
    protected readonly ICorrelationContext CorrelationContext;
    protected readonly ILogService LogService;
    protected readonly string ServiceName;

    // HTTP metodları
    protected Task<TResponse?> GetAsync<TResponse>(string endpoint, ...);
    protected Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, ...);
    protected Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, ...);
    protected Task<bool> DeleteAsync(string endpoint, ...);
}
```

### 3.2 Yeni HTTP Proxy Oluşturma

#### Adım 1: Interface Tanımlama

```csharp
// Interfaces/IPaymentServiceProxy.cs
namespace Enterprise.Proxy.PaymentService;

/// <summary>
/// Ödeme servisi proxy interface'i
/// </summary>
public interface IPaymentServiceProxy
{
    /// <summary>
    /// Ödeme işlemi başlatır
    /// </summary>
    Task<PaymentResultDto?> ProcessPaymentAsync(
        ProcessPaymentRequest request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ödeme durumunu sorgular
    /// </summary>
    Task<PaymentStatusDto?> GetPaymentStatusAsync(
        string transactionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// İade işlemi yapar
    /// </summary>
    Task<RefundResultDto?> RefundPaymentAsync(
        RefundRequest request, 
        CancellationToken cancellationToken = default);
}
```

#### Adım 2: DTO'ları Tanımlama

```csharp
// DTOs/PaymentDtos.cs
namespace Enterprise.Proxy.PaymentService.DTOs;

/// <summary>
/// Ödeme işlemi request
/// </summary>
public record ProcessPaymentRequest(
    string OrderId,
    decimal Amount,
    string Currency,
    string CardNumber,
    string CardHolderName,
    string ExpiryDate,
    string Cvv);

/// <summary>
/// Ödeme sonucu
/// </summary>
public record PaymentResultDto(
    string TransactionId,
    bool IsSuccess,
    string? ErrorCode,
    string? ErrorMessage,
    DateTime ProcessedAt);

/// <summary>
/// Ödeme durumu
/// </summary>
public record PaymentStatusDto(
    string TransactionId,
    PaymentState State,
    decimal Amount,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public enum PaymentState
{
    Pending,
    Processing,
    Completed,
    Failed,
    Refunded
}

/// <summary>
/// İade request
/// </summary>
public record RefundRequest(
    string TransactionId,
    decimal? Amount,
    string Reason);

/// <summary>
/// İade sonucu
/// </summary>
public record RefundResultDto(
    string RefundId,
    bool IsSuccess,
    decimal RefundedAmount,
    string? ErrorMessage);
```

#### Adım 3: Proxy Implementasyonu

```csharp
// PaymentServiceProxy.cs
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Shared.ErrorCodes;
using Enterprise.Proxy.Core.Http;
using Enterprise.Proxy.PaymentService.DTOs;
using Microsoft.Extensions.Logging;

namespace Enterprise.Proxy.PaymentService;

/// <summary>
/// Ödeme servisi proxy implementasyonu
/// Otomatik loglama ve exception handling sağlar
/// </summary>
public class PaymentServiceProxy : HttpProxyBase, IPaymentServiceProxy
{
    // Özel hata kodları tanımlama
    private static readonly ErrorCode PaymentDeclinedError = new(
        "PAYMENT_001", 
        "Payment was declined by the bank", 
        ErrorSeverity.Warning);
    
    private static readonly ErrorCode InsufficientFundsError = new(
        "PAYMENT_002", 
        "Insufficient funds in the account", 
        ErrorSeverity.Warning);

    public PaymentServiceProxy(
        HttpClient httpClient,
        ILogger<PaymentServiceProxy> logger,
        ICorrelationContext correlationContext,
        ILogService logService)
        : base(httpClient, logger, correlationContext, logService, "PaymentService")
    {
    }

    public async Task<PaymentResultDto?> ProcessPaymentAsync(
        ProcessPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        // Özel hata kodu ile çağrı
        return await PostAsync<ProcessPaymentRequest, PaymentResultDto>(
            "/api/v1/payments",
            request,
            PaymentDeclinedError,  // Özel hata kodu
            cancellationToken);
    }

    public async Task<PaymentStatusDto?> GetPaymentStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<PaymentStatusDto>(
            $"/api/v1/payments/{transactionId}/status",
            customErrorCode: null,
            cancellationToken);
    }

    public async Task<RefundResultDto?> RefundPaymentAsync(
        RefundRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<RefundRequest, RefundResultDto>(
            "/api/v1/payments/refund",
            request,
            customErrorCode: null,
            cancellationToken);
    }
}
```

#### Adım 4: Registration Extension

```csharp
// Extensions/RegisterPaymentServiceProxyConfiguration.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace Enterprise.Proxy.PaymentService.Extensions;

/// <summary>
/// Payment Service Proxy yapılandırma sınıfı
/// Tek satırda uygulamaya dahil edilebilir
/// </summary>
public static class RegisterPaymentServiceProxyConfiguration
{
    /// <summary>
    /// Payment Service Proxy'yi register eder
    /// </summary>
    /// <example>
    /// services.RegisterPaymentServiceProxy(configuration);
    /// </example>
    public static IServiceCollection RegisterPaymentServiceProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["ExternalServices:PaymentService:BaseUrl"] 
            ?? throw new InvalidOperationException("PaymentService BaseUrl is not configured");
        
        var timeout = configuration.GetValue<int>("ExternalServices:PaymentService:TimeoutSeconds", 30);

        // Retry policy - Ödeme servisi için daha az retry
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(2, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Retry loglama (opsiyonel)
                });

        // Circuit breaker - 3 hata sonrası 30 saniye açık kalır
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    // Circuit açıldığında loglama
                },
                onReset: () =>
                {
                    // Circuit kapandığında loglama
                });

        // HttpClient registration
        services.AddHttpClient<IPaymentServiceProxy, PaymentServiceProxy>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(retryPolicy)
        .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}
```

#### Adım 5: appsettings.json Konfigürasyonu

```json
{
  "ExternalServices": {
    "PaymentService": {
      "BaseUrl": "https://payment-gateway.example.com",
      "TimeoutSeconds": 30,
      "ApiKey": "your-api-key-here"
    }
  }
}
```

#### Adım 6: Program.cs'de Kullanım

```csharp
// Program.cs
using Enterprise.Proxy.PaymentService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ... diğer servisler ...

// Payment Service Proxy - TEK SATIR
services.RegisterPaymentServiceProxy(configuration);

// ... devamı ...
```

---

## 4. WCF Proxy Adaptasyonu

### 4.1 WcfProxyBase Sınıfı

`WcfProxyBase<TChannel>` sınıfı, SOAP/WCF servisleri için temel işlevselliği sağlar:

```csharp
public abstract class WcfProxyBase<TChannel> : IDisposable where TChannel : class
{
    // Channel yönetimi
    protected TChannel CreateChannel(string endpointUrl);
    protected virtual Binding CreateBinding();

    // Operasyon çalıştırma (otomatik loglama + exception handling)
    protected Task<TResult> ExecuteAsync<TResult>(
        Func<TChannel, Task<TResult>> operation,
        string operationName,
        ErrorCode? customErrorCode = null);
}
```

### 4.2 WCF Message Inspector

`WcfLoggingInspector` sınıfı, SOAP request/response'ları otomatik olarak loglar:

```csharp
public class WcfLoggingInspector : IClientMessageInspector
{
    // Request gönderilmeden önce
    public object? BeforeSendRequest(ref Message request, IClientChannel channel);

    // Response alındıktan sonra
    public void AfterReceiveReply(ref Message reply, object correlationState);
}
```

### 4.3 Yeni WCF Proxy Oluşturma

#### Adım 1: Service Contract Tanımlama

```csharp
// Contracts/ICustomerWcfService.cs
using System.ServiceModel;

namespace Enterprise.Proxy.CustomerService.Contracts;

/// <summary>
/// Legacy Customer WCF Service Contract
/// </summary>
[ServiceContract(Namespace = "http://legacy.company.com/customer")]
public interface ICustomerWcfService
{
    [OperationContract]
    Task<CustomerResponse> GetCustomerByIdAsync(GetCustomerRequest request);

    [OperationContract]
    Task<CustomerListResponse> SearchCustomersAsync(SearchCustomerRequest request);

    [OperationContract]
    Task<UpdateCustomerResponse> UpdateCustomerAsync(UpdateCustomerRequest request);
}

// Request/Response Data Contracts
[DataContract]
public class GetCustomerRequest
{
    [DataMember]
    public string CustomerId { get; set; } = string.Empty;
}

[DataContract]
public class CustomerResponse
{
    [DataMember]
    public string CustomerId { get; set; } = string.Empty;

    [DataMember]
    public string FirstName { get; set; } = string.Empty;

    [DataMember]
    public string LastName { get; set; } = string.Empty;

    [DataMember]
    public string Email { get; set; } = string.Empty;

    [DataMember]
    public string Phone { get; set; } = string.Empty;

    [DataMember]
    public bool IsSuccess { get; set; }

    [DataMember]
    public string? ErrorCode { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }
}

// ... diğer request/response sınıfları
```

#### Adım 2: Proxy Interface Tanımlama

```csharp
// ICustomerWcfProxy.cs
namespace Enterprise.Proxy.CustomerService;

/// <summary>
/// Customer WCF Proxy Interface
/// </summary>
public interface ICustomerWcfProxy
{
    Task<CustomerDto?> GetCustomerByIdAsync(string customerId);
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
    Task<bool> UpdateCustomerAsync(UpdateCustomerDto customer);
}
```

#### Adım 3: DTO'lar (İç kullanım için)

```csharp
// DTOs/CustomerDtos.cs
namespace Enterprise.Proxy.CustomerService.DTOs;

/// <summary>
/// Customer DTO - İç kullanım için
/// </summary>
public record CustomerDto(
    string Id,
    string FullName,
    string Email,
    string Phone);

/// <summary>
/// Update Customer DTO
/// </summary>
public record UpdateCustomerDto(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Phone);
```

#### Adım 4: Proxy Implementasyonu

```csharp
// CustomerWcfProxy.cs
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Shared.ErrorCodes;
using Enterprise.Proxy.Core.Wcf;
using Enterprise.Proxy.CustomerService.Contracts;
using Enterprise.Proxy.CustomerService.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ServiceModel;

namespace Enterprise.Proxy.CustomerService;

/// <summary>
/// Legacy Customer WCF Service Proxy
/// Otomatik loglama ve exception handling sağlar
/// </summary>
public class CustomerWcfProxy : WcfProxyBase<ICustomerWcfService>, ICustomerWcfProxy
{
    private readonly CustomerWcfOptions _options;

    // Özel hata kodları
    private static readonly ErrorCode CustomerNotFoundError = new(
        "CUSTOMER_001",
        "Customer not found in legacy system",
        ErrorSeverity.Warning);

    public CustomerWcfProxy(
        ILogger<CustomerWcfProxy> logger,
        ICorrelationContext correlationContext,
        ILogService logService,
        IOptions<CustomerWcfOptions> options)
        : base(logger, correlationContext, logService, "LegacyCustomerService")
    {
        _options = options.Value;

        // Channel oluştur
        CreateChannel(_options.EndpointUrl);
    }

    protected override System.ServiceModel.Channels.Binding CreateBinding()
    {
        // BasicHttpBinding veya WSHttpBinding kullan
        var binding = new BasicHttpBinding
        {
            MaxReceivedMessageSize = 10485760, // 10MB
            ReceiveTimeout = TimeSpan.FromMinutes(_options.TimeoutMinutes),
            SendTimeout = TimeSpan.FromMinutes(_options.TimeoutMinutes),
            Security = new BasicHttpSecurity
            {
                Mode = _options.UseHttps 
                    ? BasicHttpSecurityMode.Transport 
                    : BasicHttpSecurityMode.None
            }
        };

        return binding;
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(string customerId)
    {
        var request = new GetCustomerRequest { CustomerId = customerId };

        // ExecuteAsync - otomatik loglama ve exception handling
        var response = await ExecuteAsync(
            async channel => await channel.GetCustomerByIdAsync(request),
            "GetCustomerById",
            CustomerNotFoundError);

        if (!response.IsSuccess)
        {
            // Hata logla ve null dön
            Logger.LogWarning(
                "[{CorrelationId}] Customer not found: {CustomerId} | Error: {ErrorCode} - {ErrorMessage}",
                CorrelationContext.CorrelationId,
                customerId,
                response.ErrorCode,
                response.ErrorMessage);

            return null;
        }

        // DTO'ya map et
        return new CustomerDto(
            response.CustomerId,
            $"{response.FirstName} {response.LastName}",
            response.Email,
            response.Phone);
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
    {
        var request = new SearchCustomerRequest { SearchTerm = searchTerm };

        var response = await ExecuteAsync(
            async channel => await channel.SearchCustomersAsync(request),
            "SearchCustomers");

        return response.Customers?.Select(c => new CustomerDto(
            c.CustomerId,
            $"{c.FirstName} {c.LastName}",
            c.Email,
            c.Phone)) ?? Enumerable.Empty<CustomerDto>();
    }

    public async Task<bool> UpdateCustomerAsync(UpdateCustomerDto customer)
    {
        var request = new UpdateCustomerRequest
        {
            CustomerId = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone
        };

        var response = await ExecuteAsync(
            async channel => await channel.UpdateCustomerAsync(request),
            "UpdateCustomer");

        return response.IsSuccess;
    }
}
```

#### Adım 5: Options Sınıfı

```csharp
// Options/CustomerWcfOptions.cs
namespace Enterprise.Proxy.CustomerService;

public class CustomerWcfOptions
{
    public const string SectionName = "ExternalServices:CustomerWcfService";

    public string EndpointUrl { get; set; } = "http://localhost:8080/CustomerService.svc";
    public int TimeoutMinutes { get; set; } = 2;
    public bool UseHttps { get; set; } = false;
}
```

#### Adım 6: Registration Extension

```csharp
// Extensions/RegisterCustomerWcfProxyConfiguration.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Proxy.CustomerService.Extensions;

public static class RegisterCustomerWcfProxyConfiguration
{
    public static IServiceCollection RegisterCustomerWcfProxy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Options
        services.Configure<CustomerWcfOptions>(
            configuration.GetSection(CustomerWcfOptions.SectionName));

        // Proxy - Scoped (her request için yeni channel)
        services.AddScoped<ICustomerWcfProxy, CustomerWcfProxy>();

        return services;
    }
}
```

#### Adım 7: appsettings.json Konfigürasyonu

```json
{
  "ExternalServices": {
    "CustomerWcfService": {
      "EndpointUrl": "http://legacy-server.company.com/CustomerService.svc",
      "TimeoutMinutes": 2,
      "UseHttps": false
    }
  }
}
```

---

## 5. Loglama ve Correlation ID

### 5.1 Otomatik Loglama

Proxy base sınıfları aşağıdaki logları otomatik olarak tutar:

| Log Tipi | Açıklama | Örnek |
|----------|----------|-------|
| **Request Log** | Her request'in detayları | Endpoint, Method, Body, Headers |
| **Response Log** | Her response'un detayları | Status, Body, Duration |
| **Exception Log** | Hatalar | Exception type, message, stack trace |
| **Performance Log** | Performans metrikleri | Duration, Success/Fail |

### 5.2 Log Entry Örneği

```json
{
  "Timestamp": "2025-11-29T14:30:45.123+03:00",
  "CorrelationId": "abc123-def456",
  "Level": "Information",
  "Message": "HTTP POST Success: PaymentService/api/v1/payments | Duration: 245ms",
  "ServiceName": "PaymentService",
  "HttpMethod": "POST",
  "Endpoint": "/api/v1/payments",
  "DurationMs": 245,
  "StatusCode": 200
}
```

### 5.3 Correlation ID Propagasyonu

#### HTTP Proxy'de:
```csharp
// Otomatik olarak HttpClient'a eklenir
HttpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationContext.CorrelationId);
```

#### WCF Proxy'de:
```csharp
// SOAP Header olarak eklenir
var header = MessageHeader.CreateHeader(
    "X-Correlation-ID",
    "http://enterprise.com/logging",
    _correlationContext.CorrelationId);
request.Headers.Add(header);
```

### 5.4 Sensitive Data Masking

Hassas veriler loglardan otomatik olarak maskelenir:

```csharp
// appsettings.json
"SensitiveData": {
  "SensitiveFields": ["password", "cardNumber", "cvv", "secretKey"],
  "MaskCreditCards": true,
  "MaskEmails": false
}
```

Log çıktısı:
```json
{
  "RequestBody": {
    "orderId": "ORD-123",
    "cardNumber": "***MASKED***",
    "cvv": "***MASKED***",
    "amount": 150.00
  }
}
```

---

## 6. Hata Yönetimi ve BusinessException

### 6.1 Exception Akışı

```
┌──────────────────┐
│  External Error  │
│  (HTTP/WCF)      │
└────────┬─────────┘
         │
         ▼
┌──────────────────────────────────────────┐
│  1. Exception Yakalama                    │
│     catch (HttpRequestException ex)       │
│     catch (FaultException ex)             │
│     catch (TimeoutException ex)           │
└────────┬─────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────┐
│  2. Orijinal Hatayı Loglama (KRİTİK!)    │
│     Logger.LogError(ex, "...")           │
│     LogService.LogExceptionAsync(...)     │
└────────┬─────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────┐
│  3. BusinessException'a Dönüştürme       │
│     throw new BusinessException(         │
│         errorCode,                        │
│         ex,  // Inner exception koru!    │
│         additionalData);                 │
└──────────────────────────────────────────┘
```

### 6.2 Özel Error Code Tanımlama

```csharp
// PaymentErrorCodes.cs
namespace Enterprise.Proxy.PaymentService;

public static class PaymentErrorCodes
{
    public static readonly ErrorCode PaymentDeclined = new(
        "PAYMENT_001",
        "Payment was declined by the processor",
        ErrorSeverity.Warning);

    public static readonly ErrorCode InsufficientFunds = new(
        "PAYMENT_002",
        "Insufficient funds in the account",
        ErrorSeverity.Warning);

    public static readonly ErrorCode CardExpired = new(
        "PAYMENT_003",
        "Card has expired",
        ErrorSeverity.Warning);

    public static readonly ErrorCode FraudDetected = new(
        "PAYMENT_004",
        "Transaction flagged for potential fraud",
        ErrorSeverity.Critical);

    public static readonly ErrorCode ProcessorUnavailable = new(
        "PAYMENT_005",
        "Payment processor is temporarily unavailable",
        ErrorSeverity.Error);
}
```

### 6.3 Error Code Kullanımı

```csharp
public async Task<PaymentResultDto?> ProcessPaymentAsync(ProcessPaymentRequest request)
{
    return await PostAsync<ProcessPaymentRequest, PaymentResultDto>(
        "/api/v1/payments",
        request,
        PaymentErrorCodes.PaymentDeclined);  // Özel error code
}
```

### 6.4 Exception Handling Örneği (HttpProxyBase içinde)

```csharp
private async Task HandleExceptionAsync(
    Exception ex,
    string httpMethod,
    string endpoint,
    long durationMs,
    ErrorCode errorCode,
    object? requestBody)
{
    // 1. ÖNCELİKLE mevcut hatayı logla (hata KAYBEDILMEMELI!)
    Logger.LogError(ex,
        "[{CorrelationId}] HTTP {Method} Failed: {ServiceName}{Endpoint} | Duration: {Duration}ms | Error: {ErrorMessage}",
        CorrelationContext.CorrelationId,
        httpMethod,
        ServiceName,
        endpoint,
        durationMs,
        ex.Message);

    // 2. Exception log entry oluştur (veritabanına kaydet)
    await LogService.LogExceptionAsync(new ExceptionLogEntry
    {
        ExceptionType = ex.GetType().FullName,
        ExceptionMessage = ex.Message,
        StackTrace = ex.StackTrace,
        InnerExceptionMessage = ex.InnerException?.Message,
        LayerName = "Proxy",
        Severity = errorCode.Severity.ToString(),
        // ... diğer alanlar
    });

    // 3. BusinessException fırlat - orijinal exception'ı KORU!
    throw new BusinessException(
        errorCode,
        ex,  // 👈 Inner exception olarak sakla
        new Dictionary<string, object>
        {
            ["ServiceName"] = ServiceName,
            ["OriginalError"] = ex.Message
        });
}
```

---

## 7. Resilience Patterns

### 7.1 Retry Policy

```csharp
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()  // 5xx ve 408
    .Or<TimeoutException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),  // Exponential backoff
        onRetry: (outcome, timespan, retryAttempt, context) =>
        {
            logger.LogWarning(
                "Retry {RetryAttempt} after {Delay}s for {Service}",
                retryAttempt,
                timespan.TotalSeconds,
                serviceName);
        });
```

### 7.2 Circuit Breaker

```csharp
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,   // 5 hata sonrası aç
        durationOfBreak: TimeSpan.FromSeconds(30), // 30 saniye açık kal
        onBreak: (outcome, timespan) =>
        {
            logger.LogWarning(
                "Circuit OPEN for {Service} for {Duration}s. Reason: {Reason}",
                serviceName,
                timespan.TotalSeconds,
                outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
        },
        onReset: () =>
        {
            logger.LogInformation("Circuit CLOSED for {Service}", serviceName);
        },
        onHalfOpen: () =>
        {
            logger.LogInformation("Circuit HALF-OPEN for {Service}", serviceName);
        });
```

### 7.3 Timeout Policy

```csharp
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
    TimeSpan.FromSeconds(30),
    TimeoutStrategy.Optimistic,
    onTimeoutAsync: (context, timeout, task) =>
    {
        logger.LogWarning(
            "Request timeout after {Timeout}s for {Service}",
            timeout.TotalSeconds,
            serviceName);
        return Task.CompletedTask;
    });
```

### 7.4 Policy Birleştirme

```csharp
services.AddHttpClient<IPaymentServiceProxy, PaymentServiceProxy>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy)
.AddPolicyHandler(timeoutPolicy);
```

---

## 8. Konfigürasyon

### 8.1 Genel Konfigürasyon Yapısı

```json
{
  "ExternalServices": {
    "PaymentService": {
      "BaseUrl": "https://payment.example.com",
      "TimeoutSeconds": 30,
      "RetryCount": 3,
      "CircuitBreakerThreshold": 5,
      "CircuitBreakerDurationSeconds": 30,
      "ApiKey": "xxx-api-key-xxx"
    },
    "CustomerWcfService": {
      "EndpointUrl": "http://legacy.company.com/CustomerService.svc",
      "TimeoutMinutes": 2,
      "UseHttps": false
    },
    "NotificationService": {
      "BaseUrl": "https://notify.example.com",
      "TimeoutSeconds": 10,
      "MaxRetries": 2
    }
  }
}
```

### 8.2 Options Pattern Kullanımı

```csharp
public class PaymentServiceOptions
{
    public const string SectionName = "ExternalServices:PaymentService";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
    public string ApiKey { get; set; } = string.Empty;
}

// Registration
services.Configure<PaymentServiceOptions>(
    configuration.GetSection(PaymentServiceOptions.SectionName));
```

### 8.3 Environment-Specific Konfigürasyon

```json
// appsettings.Development.json
{
  "ExternalServices": {
    "PaymentService": {
      "BaseUrl": "https://sandbox.payment.example.com",
      "ApiKey": "test-api-key"
    }
  }
}

// appsettings.Production.json
{
  "ExternalServices": {
    "PaymentService": {
      "BaseUrl": "https://api.payment.example.com",
      "ApiKey": "prod-api-key"
    }
  }
}
```

---

## 9. Yeni Proxy Ekleme Adımları

### 9.1 HTTP Proxy Checklist

| Adım | Açıklama | Dosya |
|------|----------|-------|
| 1 | Yeni proje oluştur | `Enterprise.Proxy.{ServiceName}` |
| 2 | Interface tanımla | `I{ServiceName}Proxy.cs` |
| 3 | DTO'ları tanımla | `DTOs/{ServiceName}Dtos.cs` |
| 4 | Error code'ları tanımla | `{ServiceName}ErrorCodes.cs` |
| 5 | Proxy implementasyonu | `{ServiceName}Proxy.cs` |
| 6 | Options sınıfı | `Options/{ServiceName}Options.cs` |
| 7 | Registration extension | `Extensions/Register{ServiceName}ProxyConfiguration.cs` |
| 8 | appsettings konfigürasyonu | `appsettings.json` |
| 9 | Unit test | `{ServiceName}ProxyTests.cs` |

### 9.2 WCF Proxy Checklist

| Adım | Açıklama | Dosya |
|------|----------|-------|
| 1 | Yeni proje oluştur | `Enterprise.Proxy.{ServiceName}` |
| 2 | Service contract tanımla | `Contracts/I{ServiceName}WcfService.cs` |
| 3 | Interface tanımla | `I{ServiceName}WcfProxy.cs` |
| 4 | DTO'ları tanımla | `DTOs/{ServiceName}Dtos.cs` |
| 5 | Error code'ları tanımla | `{ServiceName}ErrorCodes.cs` |
| 6 | Proxy implementasyonu | `{ServiceName}WcfProxy.cs` |
| 7 | Options sınıfı | `Options/{ServiceName}WcfOptions.cs` |
| 8 | Registration extension | `Extensions/Register{ServiceName}WcfProxyConfiguration.cs` |
| 9 | appsettings konfigürasyonu | `appsettings.json` |
| 10 | Unit test | `{ServiceName}WcfProxyTests.cs` |

### 9.3 Proje Referansları

```xml
<!-- Enterprise.Proxy.{ServiceName}.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Core\Enterprise.Core.Shared\Enterprise.Core.Shared.csproj" />
    <ProjectReference Include="..\Enterprise.Proxy.Core\Enterprise.Proxy.Core.csproj" />
  </ItemGroup>

  <!-- HTTP Proxy için -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Http.Polly" />
  </ItemGroup>

  <!-- WCF Proxy için -->
  <ItemGroup>
    <PackageReference Include="System.ServiceModel.Http" />
    <PackageReference Include="System.ServiceModel.Primitives" />
  </ItemGroup>

</Project>
```

---

## 10. Test Stratejileri

### 10.1 Unit Test - HTTP Proxy

```csharp
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Enterprise.Proxy.PaymentService.Tests;

public class PaymentServiceProxyTests
{
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly Mock<ILogger<PaymentServiceProxy>> _loggerMock;
    private readonly Mock<ICorrelationContext> _correlationMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly PaymentServiceProxy _proxy;

    public PaymentServiceProxyTests()
    {
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _loggerMock = new Mock<ILogger<PaymentServiceProxy>>();
        _correlationMock = new Mock<ICorrelationContext>();
        _logServiceMock = new Mock<ILogService>();

        _correlationMock.Setup(x => x.CorrelationId).Returns("test-correlation-id");

        var httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("https://test.payment.com")
        };

        _proxy = new PaymentServiceProxy(
            httpClient,
            _loggerMock.Object,
            _correlationMock.Object,
            _logServiceMock.Object);
    }

    [Fact]
    public async Task ProcessPaymentAsync_Success_ReturnsResult()
    {
        // Arrange
        var expectedResult = new PaymentResultDto(
            "TXN-123",
            true,
            null,
            null,
            DateTime.UtcNow);

        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedResult)
            });

        var request = new ProcessPaymentRequest(
            "ORD-123",
            100.00m,
            "TRY",
            "4111111111111111",
            "Test User",
            "12/25",
            "123");

        // Act
        var result = await _proxy.ProcessPaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal("TXN-123", result.TransactionId);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ServiceUnavailable_ThrowsBusinessException()
    {
        // Arrange
        _httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var request = new ProcessPaymentRequest(...);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(
            () => _proxy.ProcessPaymentAsync(request));

        Assert.Contains("external", exception.ErrorCode.Code.ToLower());
    }
}
```

### 10.2 Unit Test - WCF Proxy

```csharp
using Moq;
using Xunit;

namespace Enterprise.Proxy.CustomerService.Tests;

public class CustomerWcfProxyTests
{
    private readonly Mock<ILogger<CustomerWcfProxy>> _loggerMock;
    private readonly Mock<ICorrelationContext> _correlationMock;
    private readonly Mock<ILogService> _logServiceMock;
    private readonly IOptions<CustomerWcfOptions> _options;

    public CustomerWcfProxyTests()
    {
        _loggerMock = new Mock<ILogger<CustomerWcfProxy>>();
        _correlationMock = new Mock<ICorrelationContext>();
        _logServiceMock = new Mock<ILogService>();

        _correlationMock.Setup(x => x.CorrelationId).Returns("test-correlation-id");

        _options = Options.Create(new CustomerWcfOptions
        {
            EndpointUrl = "http://localhost:8080/CustomerService.svc",
            TimeoutMinutes = 1
        });
    }

    // Not: WCF proxy testleri için genellikle Integration Test tercih edilir
    // çünkü WCF channel mock'lamak zordur
}
```

### 10.3 Integration Test

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace Enterprise.Proxy.PaymentService.IntegrationTests;

public class PaymentServiceProxyIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WireMockServer _mockServer;
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentServiceProxyIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _mockServer = WireMockServer.Start();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["ExternalServices:PaymentService:BaseUrl"] = _mockServer.Urls[0]
                });
            });
        });
    }

    [Fact]
    public async Task ProcessPayment_Integration_Success()
    {
        // Arrange - Mock server setup
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/v1/payments")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    transactionId = "TXN-INT-123",
                    isSuccess = true
                }));

        using var scope = _factory.Services.CreateScope();
        var proxy = scope.ServiceProvider.GetRequiredService<IPaymentServiceProxy>();

        var request = new ProcessPaymentRequest(...);

        // Act
        var result = await proxy.ProcessPaymentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TXN-INT-123", result.TransactionId);
    }

    public void Dispose()
    {
        _mockServer.Stop();
    }
}
```

---

## 11. Best Practices

### 11.1 Genel Kurallar

| Kural | Açıklama |
|-------|----------|
| ✅ **Base sınıfları kullan** | Her zaman `HttpProxyBase` veya `WcfProxyBase` extend et |
| ✅ **Interface tanımla** | Her proxy için interface oluştur (testability için) |
| ✅ **Error code kullan** | Her servis için özel error code'lar tanımla |
| ✅ **Options pattern kullan** | Konfigürasyonları type-safe options ile al |
| ✅ **Resilience ekle** | Retry, Circuit Breaker, Timeout ekle |
| ❌ **HttpClient'ı manuel oluşturma** | IHttpClientFactory kullan |
| ❌ **Exception'ları yutma** | Her zaman logla, sonra BusinessException fırlat |
| ❌ **Hardcoded URL kullanma** | appsettings'ten oku |

### 11.2 Performans İpuçları

```csharp
// ✅ HttpClient'ı singleton olarak kullan (IHttpClientFactory ile)
services.AddHttpClient<IPaymentProxy, PaymentProxy>();

// ✅ Async/await doğru kullan
public async Task<Result> GetDataAsync()
{
    return await _httpClient.GetFromJsonAsync<Result>("/api/data");
}

// ❌ Blocking call yapma
public Result GetData()
{
    return _httpClient.GetFromJsonAsync<Result>("/api/data").Result; // YANLIŞ!
}

// ✅ CancellationToken kullan
public async Task<Result> GetDataAsync(CancellationToken ct = default)
{
    return await _httpClient.GetFromJsonAsync<Result>("/api/data", ct);
}
```

### 11.3 Güvenlik İpuçları

```csharp
// ✅ API Key'leri environment variable'dan al
var apiKey = configuration["ExternalServices:PaymentService:ApiKey"];

// ✅ HTTPS kullan
client.BaseAddress = new Uri("https://secure-api.example.com");

// ✅ Certificate validation
services.AddHttpClient<ISecureProxy, SecureProxy>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
        {
            // Custom validation logic
            return errors == SslPolicyErrors.None;
        }
    });

// ✅ Hassas verileri loglamadan önce maskele
// (Otomatik olarak SensitiveDataMasker tarafından yapılır)
```

### 11.4 Monitoring ve Alerting

```csharp
// Health check endpoint
services.AddHealthChecks()
    .AddUrlGroup(
        new Uri("https://payment.example.com/health"),
        name: "payment-service",
        failureStatus: HealthStatus.Degraded,
        timeout: TimeSpan.FromSeconds(5));

// Metrics
services.AddMeter("Enterprise.Proxy.Metrics");

// Custom metrics in proxy
private readonly Counter<long> _requestCounter;

public PaymentServiceProxy(...)
{
    _requestCounter = meter.CreateCounter<long>("proxy.requests");
}

protected override async Task<T> ExecuteAsync<T>(...)
{
    _requestCounter.Add(1, new KeyValuePair<string, object?>("service", ServiceName));
    // ...
}
```

---

## Sonuç

Bu rehber, Enterprise uygulamasında HTTP ve WCF proxy'lerinin nasıl oluşturulacağını, yapılandırılacağını ve test edileceğini kapsamlı bir şekilde açıklamaktadır.

Önemli noktalar:
- Her zaman base sınıfları kullanın
- Otomatik loglama ve exception handling'den faydalanın
- Resilience pattern'larını ekleyin
- Error code'larla yapılandırılmış hata yönetimi yapın
- Unit ve integration testler yazın

Sorularınız için: architecture-team@company.com

