using Enterprise.Core.Shared.Constants;

namespace Enterprise.Core.Application.Models.Logging;

/// <summary>
/// Business exception log entry
/// İş kuralı ihlalleri için özel log
/// </summary>
public class BusinessExceptionLogEntry : ExceptionLogEntry
{
    public BusinessExceptionLogEntry()
    {
        LogType = LogConstants.LogTypes.BusinessException;
        LogLevel = "Warning";
        ExceptionCategory = LogConstants.ExceptionCategories.Business;
    }

    #region Business Context

    /// <summary>
    /// İş operasyonu adı
    /// </summary>
    public string? BusinessOperation { get; set; }

    /// <summary>
    /// İş hatası kodu
    /// </summary>
    public string? BusinessErrorCode { get; set; }

    /// <summary>
    /// İş hatası mesajı
    /// </summary>
    public string? BusinessErrorMessage { get; set; }

    /// <summary>
    /// Etkilenen entity tipi
    /// </summary>
    public string? AffectedEntity { get; set; }

    /// <summary>
    /// Etkilenen entity ID
    /// </summary>
    public string? AffectedEntityId { get; set; }

    #endregion

    #region Kullanıcı Mesajları

    /// <summary>
    /// Kullanıcı dostu mesaj
    /// </summary>
    public string? UserFriendlyMessage { get; set; }

    /// <summary>
    /// Önerilen aksiyon
    /// </summary>
    public string? SuggestedAction { get; set; }

    #endregion

    #region İş Kuralı Detayları

    /// <summary>
    /// Kural adı
    /// </summary>
    public string? RuleName { get; set; }

    /// <summary>
    /// Kural açıklaması
    /// </summary>
    public string? RuleDescription { get; set; }

    /// <summary>
    /// Validation hataları
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    #endregion

    /// <summary>
    /// BusinessException'dan log entry oluşturur
    /// </summary>
    public static BusinessExceptionLogEntry FromBusinessException(
        Shared.Exceptions.BusinessException ex,
        string correlationId,
        string layer)
    {
        return new BusinessExceptionLogEntry
        {
            CorrelationId = correlationId,
            Layer = layer,
            ExceptionType = ex.GetType().FullName,
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace,
            BusinessErrorCode = ex.ErrorCode,
            BusinessErrorMessage = ex.Message,
            UserFriendlyMessage = ex.UserFriendlyMessage,
            SuggestedAction = ex.SuggestedAction,
            ValidationErrors = ex.ValidationErrors
        };
    }
}

