namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Müşteri güncelleme API Response
/// </summary>
public record UpdateCustomerApiResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime UpdatedAt);


