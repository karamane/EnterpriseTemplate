namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Login isteği
/// </summary>
public record LoginRequest(
    string Username,
    string Password);


