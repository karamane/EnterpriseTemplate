using Enterprise.Core.Shared.Helpers;

namespace Enterprise.Core.Application.Models.Logging;

/// <summary>
/// Tüm log entry'lerin base sınıfı
/// </summary>
public abstract class BaseLogEntry
{
    /// <summary>
    /// Unique log ID
    /// </summary>
    public string LogId { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Correlation ID - Request tracking için
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Parent correlation ID
    /// </summary>
    public string? ParentCorrelationId { get; set; }

    /// <summary>
    /// Log timestamp (Türkiye saati)
    /// </summary>
    public DateTime Timestamp { get; set; } = TimeZoneHelper.NowTurkey;

    /// <summary>
    /// Log timestamp UTC (karşılaştırma için)
    /// </summary>
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Formatlanmış Türkiye saati
    /// </summary>
    public string TimestampFormatted => Timestamp.ToString("dd.MM.yyyy HH:mm:ss.fff");

    /// <summary>
    /// Log seviyesi
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// Log türü
    /// </summary>
    public string LogType { get; set; } = string.Empty;

    /// <summary>
    /// Log mesajı
    /// </summary>
    public string Message { get; set; } = string.Empty;

    #region Context Bilgileri

    /// <summary>
    /// Sunucu adı
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Sunucu IP
    /// </summary>
    public string? ServerIp { get; set; }

    /// <summary>
    /// Client IP
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// Kullanıcı ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Session ID
    /// </summary>
    public string? SessionId { get; set; }

    #endregion

    #region Katman Bilgileri

    /// <summary>
    /// Logun oluşturulduğu katman
    /// </summary>
    public string Layer { get; set; } = string.Empty;

    /// <summary>
    /// Class adı
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Method adı
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Namespace
    /// </summary>
    public string? Namespace { get; set; }

    #endregion

    #region Uygulama Bilgileri

    /// <summary>
    /// Uygulama adı
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Uygulama versiyonu
    /// </summary>
    public string? ApplicationVersion { get; set; }

    /// <summary>
    /// Ortam (Development, Staging, Production)
    /// </summary>
    public string? Environment { get; set; }

    #endregion

    /// <summary>
    /// Ek veriler (JSON olarak serialize edilir)
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }

    /// <summary>
    /// Ek veri ekler
    /// </summary>
    public void AddData(string key, object value)
    {
        AdditionalData ??= new Dictionary<string, object>();
        AdditionalData[key] = value;
    }
}
