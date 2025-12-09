namespace Enterprise.Infrastructure.CrossCutting.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Kullanıcı girişi yapar ve token döner
    /// </summary>
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh token ile yeni access token alır
    /// </summary>
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Token'ı geçersiz kılar (logout)
    /// </summary>
    Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Token'ın geçerli olup olmadığını kontrol eder
    /// </summary>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

/// <summary>
/// Authentication sonucu
/// </summary>
public record AuthenticationResult
{
    public bool Success { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? ErrorMessage { get; init; }
    public UserInfo? User { get; init; }

    public static AuthenticationResult Succeeded(string accessToken, string refreshToken, DateTime expiresAt, UserInfo user)
        => new()
        {
            Success = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = user
        };

    public static AuthenticationResult Failed(string errorMessage)
        => new()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
}

/// <summary>
/// Kullanıcı bilgileri
/// </summary>
public record UserInfo
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}


