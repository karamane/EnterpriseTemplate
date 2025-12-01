# Dosya Bazlı Loglama Yapısı

**Versiyon:** 1.0  
**Tarih:** Kasım 2025  
**Framework:** .NET 10, Serilog

---

## 📁 Klasör Yapısı

```
logs/
├── enterprise-api-server/           # Uygulama adı (küçük harf, tire ile)
│   ├── all/                         # Tüm log seviyeleri
│   │   ├── log-20251129.txt         # Günlük rolling
│   │   ├── log-20251128.txt
│   │   └── ...
│   │
│   ├── errors/                      # Sadece Error ve üstü (Error, Fatal)
│   │   ├── error-20251129.txt
│   │   └── ...
│   │
│   ├── requests/                    # HTTP Request/Response logları
│   │   ├── request-20251129.txt
│   │   └── ...
│   │
│   ├── performance/                 # Performans metrikleri
│   │   ├── perf-20251129.txt
│   │   └── ...
│   │
│   ├── business/                    # Business exception logları
│   │   ├── business-20251129.txt
│   │   └── ...
│   │
│   └── security/                    # Security/Audit logları
│       ├── audit-20251129.txt
│       └── ...
│
├── enterprise-api-client/           # Client API logları
│   └── (aynı yapı)
│
└── enterprise-api-client-wcf/       # WCF Client API logları
    └── (aynı yapı)
```

---

## 📋 Log Dosyası İçerik Formatları

### Standart Format (Text)

```
29.11.2025 14:30:45.123 +03:00 [INF] [abc123-def456] [CustomersController] [SERVER-01] Customer retrieved successfully: Id=123
29.11.2025 14:30:46.456 +03:00 [ERR] [abc123-def456] [CustomerService] [SERVER-01] Failed to update customer
Enterprise.Core.Shared.Exceptions.BusinessException: Customer not found
   at Enterprise.Business.Customers.GetCustomerQueryHandler.Handle(...) in ...
```

### JSON Format

```json
{
  "timestamp": "2025-11-29T14:30:45.123+03:00",
  "level": "Information",
  "correlationId": "abc123-def456",
  "source": "CustomersController",
  "machine": "SERVER-01",
  "message": "Customer retrieved successfully: Id=123",
  "exception": null
}
```

---

## ⚙️ Yapılandırma

### appsettings.json

```json
{
  "Logging": {
    "ApplicationName": "Enterprise.Api.Server",
    "File": {
      "Enabled": true,
      "BasePath": "logs",
      "UseApplicationSubfolder": true,
      "RetentionDays": 30,
      "MaxFileSizeMB": 100,
      "CompressOldFiles": false,
      "UseJsonFormat": false,
      "RollingInterval": "Day",
      "SeparateFiles": {
        "AllLogs": true,
        "ErrorLogs": true,
        "RequestLogs": true,
        "PerformanceLogs": true,
        "BusinessLogs": true,
        "SecurityLogs": true
      }
    }
  }
}
```

### Yapılandırma Parametreleri

| Parametre | Varsayılan | Açıklama |
|-----------|------------|----------|
| `Enabled` | `true` | Dosya loglaması aktif/pasif |
| `BasePath` | `logs` | Ana log klasörü |
| `UseApplicationSubfolder` | `true` | Uygulama adını alt klasör olarak kullan |
| `RetentionDays` | `30` | Log dosyası saklama süresi (gün) |
| `MaxFileSizeMB` | `100` | Max dosya boyutu (MB) |
| `CompressOldFiles` | `false` | Eski dosyaları sıkıştır |
| `UseJsonFormat` | `false` | JSON formatında log yaz |
| `RollingInterval` | `Day` | Rolling aralığı |

### Rolling Interval Seçenekleri

| Değer | Açıklama | Dosya Adı Örneği |
|-------|----------|------------------|
| `Infinite` | Tek dosya (rolling yok) | `log.txt` |
| `Year` | Yıllık | `log-2025.txt` |
| `Month` | Aylık | `log-202511.txt` |
| `Day` | Günlük | `log-20251129.txt` |
| `Hour` | Saatlik | `log-2025112914.txt` |
| `Minute` | Dakikalık (test için) | `log-202511291430.txt` |

---

## 📊 Log Türleri ve Dosyaları

### 1. All Logs (`/all/`)

Tüm log seviyelerini içerir:
- Verbose
- Debug
- Information
- Warning
- Error
- Fatal

### 2. Error Logs (`/errors/`)

Sadece hata loglarını içerir:
- Error seviyesi
- Fatal seviyesi

**Retention:** Varsayılan sürenin 2 katı (önemli veriler)

### 3. Request Logs (`/requests/`)

HTTP Request/Response loglarını içerir:
- Request body
- Response body
- Headers (maskelenmiş)
- Duration
- Status code

**Filtre:** `LogType = "Request" OR LogType = "Response"`

### 4. Performance Logs (`/performance/`)

Performans metriklerini içerir:
- Operation duration
- Slow queries
- Cache hit/miss
- External service calls

**Filtre:** `LogType = "Performance"`

### 5. Business Logs (`/business/`)

İş kuralı ihlallerini içerir:
- BusinessException
- Validation errors
- Domain events

**Filtre:** `LogType = "Business"`
**Retention:** Varsayılan sürenin 2 katı

### 6. Security Logs (`/security/`)

Güvenlik ve audit loglarını içerir:
- Authentication attempts
- Authorization failures
- Data access logs
- Admin operations

**Filtre:** `LogType = "Security" OR LogType = "Audit"`
**Retention:** Varsayılan sürenin 3 katı (compliance için)

---

## 🔍 Log Arama Örnekleri (PowerShell)

### Son Hataları Bul

```powershell
# Son 10 hatayı göster
Get-Content .\logs\enterprise-api-server\errors\error-*.txt -Tail 10

# Belirli bir Correlation ID ile ara
Select-String -Path .\logs\enterprise-api-server\all\*.txt -Pattern "abc123-def456"
```

### Tarih Aralığında Ara

```powershell
# Bugünün loglarını ara
Select-String -Path .\logs\enterprise-api-server\all\log-$(Get-Date -Format 'yyyyMMdd').txt -Pattern "ERROR"
```

### Performance Sorunlarını Bul

```powershell
# Yavaş istekleri bul (1000ms üstü)
Select-String -Path .\logs\enterprise-api-server\performance\*.txt -Pattern "Duration.*[1-9][0-9]{3,}ms"
```

---

## 💡 Best Practices

### 1. Retention Politikası

```json
{
  "SeparateFiles": {
    "AllLogs": true,      // 30 gün
    "ErrorLogs": true,    // 60 gün (2x)
    "SecurityLogs": true  // 90 gün (3x) - Compliance
  }
}
```

### 2. Disk Alanı Yönetimi

```json
{
  "MaxFileSizeMB": 100,         // Dosya 100MB'ı aşınca yeni dosya
  "RetentionDays": 30,          // 30 günden eski dosyalar silinir
  "CompressOldFiles": true      // Eski dosyaları sıkıştır (opsiyonel)
}
```

### 3. Production için Öneriler

```json
{
  "File": {
    "Enabled": true,
    "BasePath": "D:\\Logs",     // Ayrı disk
    "RetentionDays": 90,        // Daha uzun saklama
    "MaxFileSizeMB": 50,        // Daha küçük dosyalar
    "UseJsonFormat": true,      // JSON format (analiz için)
    "SeparateFiles": {
      "AllLogs": false,         // Disable (disk tasarrufu)
      "ErrorLogs": true,
      "RequestLogs": true,
      "PerformanceLogs": true,
      "BusinessLogs": true,
      "SecurityLogs": true
    }
  }
}
```

### 4. Development için Öneriler

```json
{
  "File": {
    "Enabled": true,
    "RetentionDays": 7,         // Kısa saklama
    "MaxFileSizeMB": 10,        // Küçük dosyalar
    "UseJsonFormat": false,     // Okunabilir format
    "SeparateFiles": {
      "AllLogs": true,
      "ErrorLogs": true,
      "RequestLogs": false,     // Gereksiz
      "PerformanceLogs": false,
      "BusinessLogs": false,
      "SecurityLogs": false
    }
  }
}
```

---

## 🔄 Log Rotation ve Temizlik

### Otomatik Temizlik

Serilog, `RetentionDays` parametresine göre eski dosyaları otomatik olarak siler.

### Manuel Temizlik (PowerShell)

```powershell
# 30 günden eski log dosyalarını sil
Get-ChildItem -Path .\logs -Recurse -File | 
    Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | 
    Remove-Item -Force

# Dosya boyutunu kontrol et
Get-ChildItem -Path .\logs -Recurse -File | 
    Measure-Object -Property Length -Sum | 
    Select-Object @{N='TotalSizeMB';E={[math]::Round($_.Sum/1MB,2)}}
```

### Scheduled Task ile Temizlik

```powershell
# Windows Scheduled Task oluştur
$action = New-ScheduledTaskAction -Execute 'PowerShell.exe' -Argument '-File C:\Scripts\CleanLogs.ps1'
$trigger = New-ScheduledTaskTrigger -Daily -At 2am
Register-ScheduledTask -TaskName "CleanEnterpriseLogs" -Action $action -Trigger $trigger
```

---

## 📈 Monitoring

### Disk Kullanımı İzleme

```powershell
# Log klasörü boyutunu izle
$logPath = ".\logs"
$threshold = 5GB

$size = (Get-ChildItem -Path $logPath -Recurse | Measure-Object -Property Length -Sum).Sum
if ($size -gt $threshold) {
    Write-Warning "Log folder size exceeded threshold: $([math]::Round($size/1GB,2)) GB"
}
```

### Hata Sayısı İzleme

```powershell
# Bugünkü hata sayısını kontrol et
$errorCount = (Select-String -Path .\logs\*\errors\error-$(Get-Date -Format 'yyyyMMdd').txt -Pattern "\[ERR\]" | Measure-Object).Count
Write-Host "Today's error count: $errorCount"
```

