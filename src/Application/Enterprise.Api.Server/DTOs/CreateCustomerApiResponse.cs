namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Create Customer API Response
/// </summary>
public record CreateCustomerApiResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);


