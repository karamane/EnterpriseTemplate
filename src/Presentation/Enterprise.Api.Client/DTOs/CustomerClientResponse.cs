namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Customer Client Response - Mobil için optimize edilmiş
/// </summary>
public record CustomerClientResponse(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive);

