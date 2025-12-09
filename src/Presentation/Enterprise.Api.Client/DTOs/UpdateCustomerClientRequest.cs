namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Müşteri güncelleme Client Request - Mobil için
/// </summary>
public record UpdateCustomerClientRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);


