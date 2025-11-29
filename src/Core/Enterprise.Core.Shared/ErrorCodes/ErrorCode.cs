namespace Enterprise.Core.Shared.ErrorCodes;

/// <summary>
/// Hata kodu tanımlama sistemi
/// Developer'ların kolayca hata kodu tanımlayabilmesini sağlar
/// </summary>
public record ErrorCode
{
    /// <summary>
    /// Hata kodu (ör: CUST-001, ORD-002, PAY-003)
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Kullanıcıya gösterilecek mesaj
    /// </summary>
    public string UserMessage { get; init; }

    /// <summary>
    /// Teknik/detaylı mesaj (log için)
    /// </summary>
    public string TechnicalMessage { get; init; }

    /// <summary>
    /// HTTP status code (varsayılan 400)
    /// </summary>
    public int HttpStatusCode { get; init; }

    /// <summary>
    /// Hata kategorisi
    /// </summary>
    public ErrorCategory Category { get; init; }

    /// <summary>
    /// Hata şiddeti
    /// </summary>
    public ErrorSeverity Severity { get; init; }

    public ErrorCode(
        string code,
        string userMessage,
        string? technicalMessage = null,
        int httpStatusCode = 400,
        ErrorCategory category = ErrorCategory.Business,
        ErrorSeverity severity = ErrorSeverity.Error)
    {
        Code = code;
        UserMessage = userMessage;
        TechnicalMessage = technicalMessage ?? userMessage;
        HttpStatusCode = httpStatusCode;
        Category = category;
        Severity = severity;
    }

    public override string ToString() => $"[{Code}] {UserMessage}";
}

/// <summary>
/// Hata kategorileri
/// </summary>
public enum ErrorCategory
{
    /// <summary>
    /// Validasyon hatası
    /// </summary>
    Validation,

    /// <summary>
    /// İş kuralı hatası
    /// </summary>
    Business,

    /// <summary>
    /// Yetkilendirme hatası
    /// </summary>
    Authorization,

    /// <summary>
    /// Kimlik doğrulama hatası
    /// </summary>
    Authentication,

    /// <summary>
    /// Kaynak bulunamadı
    /// </summary>
    NotFound,

    /// <summary>
    /// Conflict (çakışma)
    /// </summary>
    Conflict,

    /// <summary>
    /// Dış servis hatası
    /// </summary>
    ExternalService,

    /// <summary>
    /// Sistem hatası
    /// </summary>
    System
}

/// <summary>
/// Hata şiddeti
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Bilgi - işlem devam edebilir
    /// </summary>
    Info,

    /// <summary>
    /// Uyarı - dikkat edilmeli
    /// </summary>
    Warning,

    /// <summary>
    /// Hata - işlem durdu
    /// </summary>
    Error,

    /// <summary>
    /// Kritik - sistem seviyesi hata
    /// </summary>
    Critical
}

