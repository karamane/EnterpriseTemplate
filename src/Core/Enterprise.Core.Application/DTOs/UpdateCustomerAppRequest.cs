namespace Enterprise.Core.Application.DTOs;

/// <summary>
/// Update Customer Application Request
/// </summary>
public record UpdateCustomerAppRequest(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber);


