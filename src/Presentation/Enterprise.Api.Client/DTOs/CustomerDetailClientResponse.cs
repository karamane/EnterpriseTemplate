namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Customer Detail Client Response - Mobil detay görünümü için
/// </summary>
public record CustomerDetailClientResponse(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    string RegisteredAt,
    string CreatedAt);

