namespace Enterprise.Core.Application.Interfaces.Logging;

/// <summary>
/// Request boyunca taşınan correlation context bilgileri
/// Uçtan uca request tracking için kullanılır
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Unique correlation ID - Tüm katmanlarda aynı kalır
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Parent correlation ID - Nested call'lar için
    /// </summary>
    string? ParentCorrelationId { get; }

    /// <summary>
    /// Authenticated user ID
    /// </summary>
    string? UserId { get; set; }

    /// <summary>
    /// Client IP adresi
    /// </summary>
    string? ClientIp { get; set; }

    /// <summary>
    /// Client user agent
    /// </summary>
    string? UserAgent { get; set; }

    /// <summary>
    /// Request path
    /// </summary>
    string? RequestPath { get; set; }

    /// <summary>
    /// Server adı (multi-server deployment için)
    /// </summary>
    string ServerName { get; }

    /// <summary>
    /// Server IP
    /// </summary>
    string? ServerIp { get; }

    /// <summary>
    /// Request başlangıç zamanı
    /// </summary>
    DateTime RequestStartTime { get; }

    /// <summary>
    /// Session ID
    /// </summary>
    string? SessionId { get; set; }

    /// <summary>
    /// Ek özellikler
    /// </summary>
    Dictionary<string, object> CustomProperties { get; }

    /// <summary>
    /// Özel özellik ekler
    /// </summary>
    void SetProperty(string key, object value);

    /// <summary>
    /// Özel özellik getirir
    /// </summary>
    T? GetProperty<T>(string key);
}

