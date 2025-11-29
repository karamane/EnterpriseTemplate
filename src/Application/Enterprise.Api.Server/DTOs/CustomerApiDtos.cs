namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Server API katmanı Customer DTO'ları
/// API request/response modelleri
/// </summary>

/// <summary>
/// Customer API Response
/// </summary>
public record CustomerApiResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt);

/// <summary>
/// Create Customer API Request
/// </summary>
public record CreateCustomerApiRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

/// <summary>
/// Create Customer API Response
/// </summary>
public record CreateCustomerApiResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);

/// <summary>
/// Update Customer API Request
/// </summary>
public record UpdateCustomerApiRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);

/// <summary>
/// Paged Customer List API Response
/// </summary>
public record PagedCustomerApiResponse(
    IReadOnlyList<CustomerApiResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Customer List Query Parameters
/// </summary>
public record CustomerListQueryParams(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null);

