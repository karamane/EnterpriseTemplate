namespace Enterprise.Core.Application.DTOs;

/// <summary>
/// Create Customer Application Response
/// </summary>
public record CreateCustomerAppResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);


