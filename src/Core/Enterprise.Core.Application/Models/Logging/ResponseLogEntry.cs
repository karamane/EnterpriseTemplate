using Enterprise.Core.Shared.Constants;

namespace Enterprise.Core.Application.Models.Logging;

/// <summary>
/// HTTP Response log entry
/// </summary>
public class ResponseLogEntry : BaseLogEntry
{
    public ResponseLogEntry()
    {
        LogType = LogConstants.LogTypes.Response;
    }

    #region HTTP Response Bilgileri

    /// <summary>
    /// HTTP Status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Status description
    /// </summary>
    public string? StatusDescription { get; set; }

    /// <summary>
    /// Response body (maskelenmiş ve truncate edilmiş)
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Response header'ları
    /// </summary>
    public Dictionary<string, string>? ResponseHeaders { get; set; }

    /// <summary>
    /// Content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Content length
    /// </summary>
    public long? ContentLength { get; set; }

    #endregion

    #region Performans Metrikleri

    /// <summary>
    /// Toplam işlem süresi (ms)
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Veritabanı sorgu sayısı
    /// </summary>
    public int? DbQueryCount { get; set; }

    /// <summary>
    /// Veritabanı sorgu süresi (ms)
    /// </summary>
    public long? DbQueryDurationMs { get; set; }

    /// <summary>
    /// Cache hit sayısı
    /// </summary>
    public int? CacheHitCount { get; set; }

    /// <summary>
    /// Cache miss sayısı
    /// </summary>
    public int? CacheMissCount { get; set; }

    /// <summary>
    /// External service call count
    /// </summary>
    public int? ExternalCallCount { get; set; }

    /// <summary>
    /// External service call duration (ms)
    /// </summary>
    public long? ExternalCallDurationMs { get; set; }

    #endregion

    /// <summary>
    /// İlişkili request log ID
    /// </summary>
    public string? RequestLogId { get; set; }
}

