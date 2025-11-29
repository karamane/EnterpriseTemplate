namespace Enterprise.Business.DTOs;

/// <summary>
/// Business katmanı Customer DTO'ları
/// Business logic işlemleri için kullanılır
/// </summary>

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

/// <summary>
/// Create Customer Business Request
/// </summary>
public record CreateCustomerBusinessRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

/// <summary>
/// Create Customer Business Response
/// </summary>
public record CreateCustomerBusinessResponse(
    Guid Id,
    string FullName,
    string Email,
    DateTime CreatedAt);

/// <summary>
/// Customer List Business Response
/// </summary>
public record CustomerListBusinessResponse(
    IReadOnlyList<CustomerBusinessDto> Customers,
    int TotalCount);

