namespace Enterprise.Core.Domain.Entities.Auth;

/// <summary>
/// Refresh token entity
/// </summary>
public class RefreshToken : BaseEntity<long>
{
    /// <summary>
    /// User ID (FK)
    /// </summary>
    public long UserId { get; private set; }

    /// <summary>
    /// Token değeri
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Token son kullanma tarihi
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Token oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Token oluşturan IP adresi
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// Token iptal edilme tarihi
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Token iptal eden IP adresi
    /// </summary>
    public string? RevokedByIp { get; private set; }

    /// <summary>
    /// Yerine geçen token (rotation için)
    /// </summary>
    public string? ReplacedByToken { get; private set; }

    /// <summary>
    /// İptal nedeni
    /// </summary>
    public string? RevokedReason { get; private set; }

    /// <summary>
    /// User navigation property
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Token süresi doldu mu?
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Token iptal edildi mi?
    /// </summary>
    public bool IsRevoked => RevokedAt != null;

    /// <summary>
    /// Token aktif mi? (iptal edilmemiş ve süresi dolmamış)
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Constructor (EF Core için)
    /// </summary>
    protected RefreshToken() { }

    /// <summary>
    /// Yeni refresh token oluşturur
    /// </summary>
    public static RefreshToken Create(
        long userId,
        string token,
        DateTime expiresAt,
        string? createdByIp = null)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };
    }

    /// <summary>
    /// Token'ı iptal eder
    /// </summary>
    public void Revoke(string? revokedByIp = null, string? reason = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
    }
}


