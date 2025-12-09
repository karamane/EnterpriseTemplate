namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Create Customer API Request
/// </summary>
public record CreateCustomerApiRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);


