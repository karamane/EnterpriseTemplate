namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Login yanıtı
/// </summary>
public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfoResponse User);

/// <summary>
/// Kullanıcı bilgileri yanıtı
/// </summary>
public record UserInfoResponse(
    string UserId,
    string Username,
    string Email,
    string? FullName,
    IReadOnlyList<string> Roles);


