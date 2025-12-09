namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Müşteri güncelleme API Request
/// </summary>
public record UpdateCustomerApiRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);
