namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Müşteri güncelleme Client Response - Mobil için
/// </summary>
public record UpdateCustomerClientResponse(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime UpdatedAt);


