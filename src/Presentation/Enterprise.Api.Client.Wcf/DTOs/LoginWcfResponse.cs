namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API login yanıtı
/// </summary>
public record LoginWcfResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string Username,
    string[] Roles);


