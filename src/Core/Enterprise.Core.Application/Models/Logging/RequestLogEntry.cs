using Enterprise.Core.Shared.Constants;

namespace Enterprise.Core.Application.Models.Logging;

/// <summary>
/// HTTP Request log entry
/// </summary>
public class RequestLogEntry : BaseLogEntry
{
    public RequestLogEntry()
    {
        LogType = LogConstants.LogTypes.Request;
    }

    #region HTTP Request Bilgileri

    /// <summary>
    /// HTTP Method (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Request path
    /// </summary>
    public string? RequestPath { get; set; }

    /// <summary>
    /// Query string
    /// </summary>
    public string? QueryString { get; set; }

    /// <summary>
    /// Request body (maskelenmiş)
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Request header'ları
    /// </summary>
    public Dictionary<string, string>? RequestHeaders { get; set; }

    /// <summary>
    /// Content type
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// Content length
    /// </summary>
    public long? ContentLength { get; set; }

    #endregion

    #region Client Bilgileri

    /// <summary>
    /// User agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Referer
    /// </summary>
    public string? Referer { get; set; }

    /// <summary>
    /// Origin
    /// </summary>
    public string? Origin { get; set; }

    #endregion

    #region Security Bilgileri

    /// <summary>
    /// Authorization türü (Bearer, Basic, etc.)
    /// </summary>
    public string? AuthorizationType { get; set; }

    /// <summary>
    /// Authenticated mi?
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Kullanıcı rolleri
    /// </summary>
    public string[]? UserRoles { get; set; }

    #endregion
}

