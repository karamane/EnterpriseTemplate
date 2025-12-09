namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Create Customer Client Request - Mobil'den gelen istek
/// </summary>
public record CreateCustomerClientRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);


