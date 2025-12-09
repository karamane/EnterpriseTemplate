namespace Enterprise.Business.DTOs;

/// <summary>
/// Business katmanı Customer DTO
/// </summary>
public record CustomerBusinessDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt);


