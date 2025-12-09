namespace Enterprise.Core.Application.DTOs;

/// <summary>
/// Create Customer Application Request
/// </summary>
public record CreateCustomerAppRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);


