namespace Enterprise.Core.Application.DTOs;

/// <summary>
/// Customer Application DTO
/// </summary>
public record CustomerAppDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);


