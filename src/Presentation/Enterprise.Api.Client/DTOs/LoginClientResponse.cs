namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Client API login yanıtı
/// </summary>
public record LoginClientResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Username,
    string[] Roles);


