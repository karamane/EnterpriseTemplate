namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Müşteri listesi API Response
/// </summary>
public record CustomerListApiResponse(
    IReadOnlyList<CustomerListItemApiResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Liste için müşteri item'ı
/// </summary>
public record CustomerListItemApiResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt);


