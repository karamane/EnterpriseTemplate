namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Customer API Response
/// </summary>
public record CustomerApiResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt);


