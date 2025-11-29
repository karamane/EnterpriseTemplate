namespace Enterprise.Core.Shared.ErrorCodes;

/// <summary>
/// Ortak hata kodları
/// Her modül kendi ErrorCodes sınıfını oluşturabilir
/// </summary>
public static class CommonErrorCodes
{
    #region Validation Errors (VAL-XXX)

    public static readonly ErrorCode ValidationFailed = new(
        "VAL-001",
        "Girilen bilgiler geçersiz",
        "Validation failed",
        400,
        ErrorCategory.Validation);

    public static readonly ErrorCode RequiredFieldMissing = new(
        "VAL-002",
        "Zorunlu alan eksik",
        "Required field is missing",
        400,
        ErrorCategory.Validation);

    public static readonly ErrorCode InvalidFormat = new(
        "VAL-003",
        "Geçersiz format",
        "Invalid format",
        400,
        ErrorCategory.Validation);

    #endregion

    #region Authentication Errors (AUTH-XXX)

    public static readonly ErrorCode Unauthorized = new(
        "AUTH-001",
        "Oturum açmanız gerekiyor",
        "Authentication required",
        401,
        ErrorCategory.Authentication);

    public static readonly ErrorCode InvalidCredentials = new(
        "AUTH-002",
        "Kullanıcı adı veya şifre hatalı",
        "Invalid credentials",
        401,
        ErrorCategory.Authentication);

    public static readonly ErrorCode TokenExpired = new(
        "AUTH-003",
        "Oturumunuz sona erdi, lütfen tekrar giriş yapın",
        "Token expired",
        401,
        ErrorCategory.Authentication);

    #endregion

    #region Authorization Errors (AUTHZ-XXX)

    public static readonly ErrorCode Forbidden = new(
        "AUTHZ-001",
        "Bu işlem için yetkiniz bulunmuyor",
        "Access forbidden",
        403,
        ErrorCategory.Authorization);

    public static readonly ErrorCode InsufficientPermissions = new(
        "AUTHZ-002",
        "Yetersiz yetki",
        "Insufficient permissions",
        403,
        ErrorCategory.Authorization);

    #endregion

    #region NotFound Errors (NF-XXX)

    public static readonly ErrorCode ResourceNotFound = new(
        "NF-001",
        "Kaynak bulunamadı",
        "Resource not found",
        404,
        ErrorCategory.NotFound);

    public static readonly ErrorCode EntityNotFound = new(
        "NF-002",
        "Kayıt bulunamadı",
        "Entity not found",
        404,
        ErrorCategory.NotFound);

    #endregion

    #region Business Errors (BUS-XXX)

    public static readonly ErrorCode BusinessRuleViolation = new(
        "BUS-001",
        "İş kuralı ihlali",
        "Business rule violation",
        422,
        ErrorCategory.Business);

    public static readonly ErrorCode OperationNotAllowed = new(
        "BUS-002",
        "Bu işlem şu an gerçekleştirilemiyor",
        "Operation not allowed",
        422,
        ErrorCategory.Business);

    public static readonly ErrorCode DuplicateEntry = new(
        "BUS-003",
        "Bu kayıt zaten mevcut",
        "Duplicate entry",
        409,
        ErrorCategory.Conflict);

    #endregion

    #region External Service Errors (EXT-XXX)

    public static readonly ErrorCode ExternalServiceError = new(
        "EXT-001",
        "Dış servis hatası, lütfen daha sonra tekrar deneyin",
        "External service error",
        502,
        ErrorCategory.ExternalService,
        ErrorSeverity.Error);

    public static readonly ErrorCode ExternalServiceTimeout = new(
        "EXT-002",
        "Dış servis yanıt vermedi, lütfen daha sonra tekrar deneyin",
        "External service timeout",
        504,
        ErrorCategory.ExternalService,
        ErrorSeverity.Warning);

    public static readonly ErrorCode ExternalServiceUnavailable = new(
        "EXT-003",
        "Dış servis şu an kullanılamıyor",
        "External service unavailable",
        503,
        ErrorCategory.ExternalService,
        ErrorSeverity.Warning);

    #endregion

    #region System Errors (SYS-XXX)

    public static readonly ErrorCode GeneralError = new(
        "SYS-000",
        "Bir hata oluştu",
        "General error occurred",
        500,
        ErrorCategory.System,
        ErrorSeverity.Error);

    public static readonly ErrorCode InternalError = new(
        "SYS-001",
        "Beklenmeyen bir hata oluştu, lütfen daha sonra tekrar deneyin",
        "Internal server error",
        500,
        ErrorCategory.System,
        ErrorSeverity.Critical);

    public static readonly ErrorCode ServiceUnavailable = new(
        "SYS-002",
        "Servis şu an kullanılamıyor",
        "Service unavailable",
        503,
        ErrorCategory.System,
        ErrorSeverity.Critical);

    public static readonly ErrorCode DatabaseError = new(
        "SYS-003",
        "Veritabanı hatası",
        "Database error",
        500,
        ErrorCategory.System,
        ErrorSeverity.Critical);

    #endregion
}

/// <summary>
/// Müşteri modülü hata kodları örneği
/// </summary>
public static class CustomerErrorCodes
{
    public static readonly ErrorCode CustomerNotFound = new(
        "CUST-001",
        "Müşteri bulunamadı",
        "Customer not found",
        404,
        ErrorCategory.NotFound);

    public static readonly ErrorCode EmailAlreadyExists = new(
        "CUST-002",
        "Bu email adresi zaten kayıtlı",
        "Email already exists",
        409,
        ErrorCategory.Conflict);

    public static readonly ErrorCode CustomerInactive = new(
        "CUST-003",
        "Müşteri hesabı aktif değil",
        "Customer account is inactive",
        422,
        ErrorCategory.Business);

    public static readonly ErrorCode InvalidPhoneNumber = new(
        "CUST-004",
        "Geçersiz telefon numarası",
        "Invalid phone number format",
        400,
        ErrorCategory.Validation);
}

