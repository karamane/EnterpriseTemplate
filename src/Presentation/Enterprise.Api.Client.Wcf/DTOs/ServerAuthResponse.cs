namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// Server API login yanıt wrapper'ı
/// </summary>
public record ServerAuthResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public ServerLoginData? Data { get; init; }
}

/// <summary>
/// Server API login data
/// </summary>
public record ServerLoginData
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public ServerUserInfo? User { get; init; }
}

/// <summary>
/// Server API user info
/// </summary>
public record ServerUserInfo
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public IEnumerable<string> Roles { get; init; } = Array.Empty<string>();
}


