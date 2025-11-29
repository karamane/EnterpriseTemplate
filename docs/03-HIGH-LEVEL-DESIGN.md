# Enterprise .NET 10 Application - High Level Design (HLD)

**Versiyon:** 1.1  
**Tarih:** Kasım 2025  
**Framework:** .NET 10, C# 14  
**Hazırlayan:** [İsim]  
**Onaylayan:** [İsim]

---

## 1. Doküman Geçmişi

| Versiyon | Tarih | Değişiklik | Hazırlayan |
|----------|-------|------------|------------|
| 1.0 | 29.11.2024 | İlk versiyon | - |
| 1.1 | 29.11.2025 | .NET 10 geçişi, Port yapılandırması, Cache/DB switch | - |
| | | | |

---

## 2. Executive Summary

### 2.1 Proje Amacı

[Projenin amacını buraya yazın]

### 2.2 Kapsam

[Proje kapsamını buraya yazın]

### 2.3 Hedef Kitle

- Backend geliştiriciler
- Sistem mimarları
- DevOps ekibi

---

## 3. Sistem Mimarisi

### 3.1 Genel Bakış

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                   INTERNET                                   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                               DMZ (Presentation)                             │
│  ┌────────────────────────┐    ┌────────────────────────┐                   │
│  │  Enterprise.Api.Client │    │ Enterprise.Api.Client  │                   │
│  │       (REST)           │    │        (WCF)           │                   │
│  └───────────┬────────────┘    └───────────┬────────────┘                   │
└──────────────┼──────────────────────────────┼───────────────────────────────┘
               │                              │
               ▼                              ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            SECURE ZONE (Application)                         │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                       Enterprise.Api.Server                          │    │
│  │                    (Internal REST API)                               │    │
│  └──────────────────────────────┬──────────────────────────────────────┘    │
└─────────────────────────────────┼───────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BUSINESS LAYER                                  │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                       Enterprise.Business                            │    │
│  │                (MediatR Handlers, Validators)                        │    │
│  └──────────────────────────────┬──────────────────────────────────────┘    │
└─────────────────────────────────┼───────────────────────────────────────────┘
                                  │
            ┌─────────────────────┼─────────────────────┐
            ▼                     ▼                     ▼
┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│   Infrastructure  │  │       Proxy       │  │       Core        │
│  ┌─────────────┐  │  │  ┌─────────────┐  │  │  ┌─────────────┐  │
│  │  Logging    │  │  │  │  WCF Proxy  │  │  │  │   Domain    │  │
│  │  Persistence│  │  │  │  HTTP Proxy │  │  │  │   Shared    │  │
│  │  Caching    │  │  │  └─────────────┘  │  │  │ Application │  │
│  └─────────────┘  │  └───────────────────┘  │  └─────────────┘  │
└───────────────────┘                         └───────────────────┘
         │                     │
         ▼                     ▼
┌───────────────────┐  ┌───────────────────┐
│     Database      │  │  External Services│
│  SqlServer/Oracle │  │   WCF / REST      │
└───────────────────┘  └───────────────────┘
```

### 3.2 Bileşen Listesi

| Bileşen | Tip | Port (HTTP/HTTPS) | Açıklama |
|---------|-----|-------------------|----------|
| Enterprise.Api.Client | Web API | 5000 / 5001 | DMZ'de çalışan REST API |
| Enterprise.Api.Client.Wcf | Web API | 5010 / 5011 | DMZ'de çalışan WCF Client |
| Enterprise.Api.Server | Web API | 5100 / 5101 | Secure Zone'da çalışan internal API |
| Enterprise.Business | Class Library | - | İş mantığı katmanı |
| Enterprise.Infrastructure.* | Class Library | - | Altyapı servisleri |
| Enterprise.Proxy.* | Class Library | Dış servis entegrasyonları |
| Enterprise.Core.* | Class Library | Domain ve paylaşılan yapılar |

---

## 4. Teknoloji Stack

### 4.1 Backend

| Teknoloji | Versiyon | Kullanım |
|-----------|----------|----------|
| .NET | 9.0+ | Runtime |
| ASP.NET Core | 9.0+ | Web framework |
| Entity Framework Core | 9.0+ | ORM |
| Dapper | 2.x | Micro ORM |
| MediatR | 12.x | CQRS pattern |
| FluentValidation | 11.x | Validation |
| AutoMapper | 13.x | Object mapping |
| Serilog | 4.x | Logging |
| Polly | 8.x | Resilience |

### 4.2 Veritabanı

| Teknoloji | Kullanım |
|-----------|----------|
| SQL Server | Primary database |
| Oracle | Alternative database |
| Redis | Distributed cache |

### 4.3 Araçlar

| Araç | Kullanım |
|------|----------|
| Swagger | API documentation |
| ELK Stack | Log aggregation |
| Docker | Containerization |

---

## 5. Katman Detayları

### 5.1 Presentation Layer (DMZ)

**Projeler:**
- `Enterprise.Api.Client` - REST tabanlı client API
- `Enterprise.Api.Client.Wcf` - WCF tabanlı client API

**Sorumluluklar:**
- Dış dünyadan gelen istekleri karşılama
- Rate limiting ve DDoS koruması
- Request/Response mapping
- Validation

**Güvenlik:**
- Tamamen izole (Server API referansı yok)
- Kendi DTO'larını tanımlar
- CORS yapılandırması

### 5.2 Application Layer (Secure Zone)

**Projeler:**
- `Enterprise.Api.Server`

**Sorumluluklar:**
- İş mantığını tetikleme
- Authentication/Authorization
- DTO dönüşümleri

### 5.3 Business Layer

**Projeler:**
- `Enterprise.Business`

**Sorumluluklar:**
- İş kurallarının uygulanması
- CQRS pattern (MediatR)
- Validation

**Pattern:**
```
Command/Query → Validator → Handler → Response
```

### 5.4 Infrastructure Layer

**Projeler:**
- `Enterprise.Infrastructure.Logging`
- `Enterprise.Infrastructure.Persistence`
- `Enterprise.Infrastructure.Caching`
- `Enterprise.Infrastructure.CrossCutting`

### 5.5 Proxy Layer

**Projeler:**
- `Enterprise.Proxy.Core`
- `Enterprise.Proxy.ExternalService`

**Sorumluluklar:**
- Dış servis entegrasyonları
- WCF ve HTTP client management
- Retry/Circuit breaker policies

**Registration:**
```csharp
services.RegisterProxyCore();
services.RegisterExternalServiceProxy(configuration);
```

### 5.6 Core Layer

**Projeler:**
- `Enterprise.Core.Domain`
- `Enterprise.Core.Application`
- `Enterprise.Core.Shared`

---

## 6. Veri Akışı

### 6.1 REST API Akışı

```
Mobile App
    │
    ▼ HTTP Request
┌─────────────────┐
│ Client API      │ DTOs: ClientRequest/Response
│ (DMZ)           │
└────────┬────────┘
         │ HTTP Request
         ▼
┌─────────────────┐
│ Server API      │ DTOs: ServerRequest/Response
│ (Secure Zone)   │
└────────┬────────┘
         │ MediatR Command/Query
         ▼
┌─────────────────┐
│ Business        │ DTOs: BusinessDto
│ (Handlers)      │
└────────┬────────┘
         │ Repository calls
         ▼
┌─────────────────┐
│ Domain          │ Entities
│ (EF Core/Dapper)│
└────────┬────────┘
         │
         ▼
    Database
```

### 6.2 WCF Client API Akışı

```
Mobile App
    │
    ▼ HTTP Request
┌─────────────────┐
│ WCF Client API  │ DTOs: WcfClientRequest/Response
│ (DMZ)           │
└────────┬────────┘
         │ SOAP/WCF Request
         ▼
┌─────────────────┐
│ Server WCF      │ DTOs: WcfServiceContracts
│ Endpoint        │
└─────────────────┘
```

---

## 7. Cross-Cutting Concerns

### 7.1 Logging

**Yaklaşım:** Middleware tabanlı otomatik loglama

**Pipeline:**
```
ExceptionLoggingMiddleware
    ↓
CorrelationIdMiddleware
    ↓
RequestLoggingMiddleware
    ↓
ActionLoggingMiddleware
    ↓
Controller
```

**Özellikler:**
- Correlation ID tracking
- Request/Response body logging
- Sensitive data masking
- Türkiye saati (UTC+3)
- Multi-sink (Console, File, Database, ELK)

### 7.2 Exception Handling

**Yapı:**
```
Exception
    │
    ▼ LogService.LogException()
┌─────────────────┐
│ Exception Log   │ (Orijinal hata korunur)
└────────┬────────┘
         │
         ▼ BusinessException fırlatılır
┌─────────────────┐
│ ErrorCode       │ (Developer-friendly)
└────────┬────────┘
         │
         ▼ API Response
┌─────────────────┐
│ ApiResponse     │ (User-friendly message)
└─────────────────┘
```

### 7.3 Caching

**Provider'lar:**

| Provider | Kullanım |
|----------|----------|
| Memory | Development, tek sunucu |
| Redis | Production, multi-server |
| Hybrid | L1: Memory, L2: Redis |

### 7.4 Security

**Önlemler:**
- Input sanitization
- XSS protection
- CORS configuration
- Rate limiting
- Sensitive data masking

---

## 8. Veritabanı Tasarımı

### 8.1 Provider Switch

```json
{
  "Database": {
    "Provider": "SqlServer",
    "OrmType": "EfCore"
  }
}
```

### 8.2 Log Tabloları

| Tablo | Açıklama |
|-------|----------|
| RequestLogs | HTTP request logları |
| ResponseLogs | HTTP response logları |
| ExceptionLogs | Exception logları |
| BusinessExceptionLogs | Business exception logları |
| AuditLogs | Audit trail |

### 8.3 ERD

[Buraya ER diagram eklenebilir]

---

## 9. Deployment

### 9.1 Ortamlar

| Ortam | URL | Açıklama |
|-------|-----|----------|
| Development | localhost | Geliştirme |
| Test | test.example.com | Test ortamı |
| Staging | staging.example.com | Pre-production |
| Production | api.example.com | Canlı ortam |

### 9.2 Deployment Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        Load Balancer                         │
└─────────────────────────────┬───────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│  Client API   │     │  Client API   │     │  Client API   │
│   Server 1    │     │   Server 2    │     │   Server N    │
└───────┬───────┘     └───────┬───────┘     └───────┬───────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
┌─────────────────────────────┼───────────────────────────────┐
│                    Internal Load Balancer                    │
└─────────────────────────────┬───────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│  Server API   │     │  Server API   │     │  Server API   │
│   Server 1    │     │   Server 2    │     │   Server N    │
└───────┬───────┘     └───────┬───────┘     └───────┬───────┘
        │                     │                     │
        └─────────────────────┼─────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│  SQL Server   │     │    Redis      │     │     ELK       │
│   Primary     │     │   Cluster     │     │    Stack      │
└───────────────┘     └───────────────┘     └───────────────┘
```

---

## 10. Integration Points

### 10.1 Internal Systems

| Sistem | Protokol | Açıklama |
|--------|----------|----------|
| [Sistem Adı] | REST/WCF | [Açıklama] |
| | | |

### 10.2 External Systems

| Sistem | Protokol | Açıklama |
|--------|----------|----------|
| [Sistem Adı] | REST/WCF | [Açıklama] |
| | | |

---

## 11. Non-Functional Requirements

### 11.1 Performance

| Metrik | Hedef |
|--------|-------|
| Response Time (p95) | < 500ms |
| Throughput | > 1000 req/sec |
| Availability | 99.9% |

### 11.2 Scalability

- Horizontal scaling (load balancer ile)
- Distributed cache (Redis)
- Database read replicas

### 11.3 Security

- [Güvenlik gereksinimleri]

---

## 12. Monitoring & Alerting

### 12.1 Health Checks

```
GET /health
GET /health/ready
GET /health/live
```

### 12.2 Metrics

- Request count
- Response time
- Error rate
- CPU/Memory usage

### 12.3 Alerts

| Alert | Koşul | Severity |
|-------|-------|----------|
| High Error Rate | > 5% errors | Critical |
| Slow Response | p95 > 1s | Warning |
| | | |

---

## 13. Açık Konular & Riskler

### 13.1 Açık Konular

| # | Konu | Sorumlu | Hedef Tarih |
|---|------|---------|-------------|
| 1 | | | |
| 2 | | | |

### 13.2 Riskler

| Risk | Etki | Olasılık | Azaltma |
|------|------|----------|---------|
| | | | |

---

## 14. Onay

| Rol | İsim | İmza | Tarih |
|-----|------|------|-------|
| Proje Yöneticisi | | | |
| Teknik Lider | | | |
| Mimar | | | |

---

## Ekler

### Ek A: Glossary

| Terim | Açıklama |
|-------|----------|
| DMZ | Demilitarized Zone |
| CQRS | Command Query Responsibility Segregation |
| | |

### Ek B: Referanslar

- [Referans 1]
- [Referans 2]

### Ek C: Değişiklik Geçmişi

[Değişiklik detayları]

