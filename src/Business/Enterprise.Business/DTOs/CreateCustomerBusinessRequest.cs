namespace Enterprise.Business.DTOs;

/// <summary>
/// Create Customer Business Request
/// </summary>
public record CreateCustomerBusinessRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);


