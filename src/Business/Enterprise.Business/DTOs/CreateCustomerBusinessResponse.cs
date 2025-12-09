namespace Enterprise.Business.DTOs;

/// <summary>
/// Create Customer Business Response
/// </summary>
public record CreateCustomerBusinessResponse(
    Guid Id,
    string FullName,
    string Email,
    DateTime CreatedAt);


