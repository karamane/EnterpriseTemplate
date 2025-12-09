namespace Enterprise.Infrastructure.CrossCutting.Options;

/// <summary>
/// JWT Authentication yapılandırma seçenekleri
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Token imzalama anahtarı (minimum 32 karakter)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Token yayıncısı (issuer)
    /// </summary>
    public string Issuer { get; set; } = "Enterprise.Api.Server";

    /// <summary>
    /// Token alıcısı (audience)
    /// </summary>
    public string Audience { get; set; } = "Enterprise.Api.Client";

    /// <summary>
    /// Access token geçerlilik süresi (dakika)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token geçerlilik süresi (gün)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Token'ı doğrula
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Audience'ı doğrula
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Token ömrünü doğrula
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// İmzalama anahtarını doğrula
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;
}


