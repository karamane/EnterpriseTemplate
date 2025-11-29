namespace Enterprise.Core.Application.DTOs;

/// <summary>
/// Application katmanı Customer DTO'ları
/// Business ve Infrastructure katmanları arasında kullanılır
/// </summary>

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

/// <summary>
/// Create Customer Application Request
/// </summary>
public record CreateCustomerAppRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

/// <summary>
/// Create Customer Application Response
/// </summary>
public record CreateCustomerAppResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);

/// <summary>
/// Update Customer Application Request
/// </summary>
public record UpdateCustomerAppRequest(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber);

/// <summary>
/// Paged Customer List Response
/// </summary>
public record PagedCustomerAppResponse(
    IReadOnlyList<CustomerAppDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

