namespace Enterprise.Core.Domain.Entities.Auth;

/// <summary>
/// Kullanıcı entity
/// </summary>
public class User : SoftDeleteEntity<long>
{
    /// <summary>
    /// Kullanıcı adı (unique)
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// Şifre hash (BCrypt)
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Tam ad
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// Roller (comma-separated: "Admin,User")
    /// </summary>
    public string? Roles { get; private set; }

    /// <summary>
    /// Kullanıcı aktif mi?
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Son giriş tarihi
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Refresh token'ları (navigation property)
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    /// <summary>
    /// Constructor (EF Core için)
    /// </summary>
    protected User() { }

    /// <summary>
    /// Yeni kullanıcı oluşturur
    /// </summary>
    public static User Create(
        string username,
        string passwordHash,
        string email,
        string? fullName = null,
        string? roles = null)
    {
        return new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            FullName = fullName,
            Roles = roles,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Son giriş tarihini günceller
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Şifreyi günceller
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rolleri günceller
    /// </summary>
    public void UpdateRoles(string roles)
    {
        Roles = roles;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kullanıcıyı deaktif eder
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Kullanıcıyı aktif eder
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rolleri array olarak döner
    /// </summary>
    public string[] GetRolesArray()
    {
        if (string.IsNullOrEmpty(Roles))
            return Array.Empty<string>();

        return Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}


