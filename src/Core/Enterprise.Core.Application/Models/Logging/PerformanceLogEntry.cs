using Enterprise.Core.Shared.Constants;

namespace Enterprise.Core.Application.Models.Logging;

/// <summary>
/// Performance log entry
/// Performans metrikleri için
/// </summary>
public class PerformanceLogEntry : BaseLogEntry
{
    public PerformanceLogEntry()
    {
        LogType = LogConstants.LogTypes.Performance;
    }

    /// <summary>
    /// İşlem adı
    /// </summary>
    public string? OperationName { get; set; }

    /// <summary>
    /// Toplam süre (ms)
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Yavaş request eşiği aşıldı mı?
    /// </summary>
    public bool IsSlowRequest { get; set; }

    /// <summary>
    /// Yavaş request eşiği (ms)
    /// </summary>
    public long? SlowRequestThresholdMs { get; set; }

    /// <summary>
    /// İşlem tipi (HttpAction, MediatR, Database, Cache, ExternalService)
    /// </summary>
    public string? OperationType { get; set; }

    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Ek metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    #region Detaylı Metrikler

    /// <summary>
    /// CPU süresi (ms)
    /// </summary>
    public long? CpuTimeMs { get; set; }

    /// <summary>
    /// Bellek kullanımı (bytes)
    /// </summary>
    public long? MemoryUsedBytes { get; set; }

    /// <summary>
    /// Allocation sayısı
    /// </summary>
    public long? AllocationCount { get; set; }

    /// <summary>
    /// Database süreleri
    /// </summary>
    public DatabaseMetrics? Database { get; set; }

    /// <summary>
    /// Cache süreleri
    /// </summary>
    public CacheMetrics? Cache { get; set; }

    /// <summary>
    /// External service süreleri
    /// </summary>
    public ExternalServiceMetrics? ExternalServices { get; set; }

    #endregion
}

/// <summary>
/// Database performans metrikleri
/// </summary>
public class DatabaseMetrics
{
    /// <summary>
    /// Toplam sorgu sayısı
    /// </summary>
    public int QueryCount { get; set; }

    /// <summary>
    /// Toplam sorgu süresi (ms)
    /// </summary>
    public long TotalDurationMs { get; set; }

    /// <summary>
    /// En yavaş sorgu süresi (ms)
    /// </summary>
    public long? SlowestQueryMs { get; set; }

    /// <summary>
    /// Connection wait süresi (ms)
    /// </summary>
    public long? ConnectionWaitMs { get; set; }
}

/// <summary>
/// Cache performans metrikleri
/// </summary>
public class CacheMetrics
{
    /// <summary>
    /// Cache hit sayısı
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    /// Cache miss sayısı
    /// </summary>
    public int MissCount { get; set; }

    /// <summary>
    /// Hit ratio
    /// </summary>
    public double HitRatio => HitCount + MissCount > 0 
        ? (double)HitCount / (HitCount + MissCount) 
        : 0;

    /// <summary>
    /// Toplam cache süresi (ms)
    /// </summary>
    public long TotalDurationMs { get; set; }
}

/// <summary>
/// External service performans metrikleri
/// </summary>
public class ExternalServiceMetrics
{
    /// <summary>
    /// Toplam çağrı sayısı
    /// </summary>
    public int CallCount { get; set; }

    /// <summary>
    /// Toplam süre (ms)
    /// </summary>
    public long TotalDurationMs { get; set; }

    /// <summary>
    /// Başarısız çağrı sayısı
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Retry sayısı
    /// </summary>
    public int RetryCount { get; set; }
}

