using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// Müşteri listesi sorgusu (sayfalama destekli)
/// </summary>
public record GetAllCustomersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<Result<GetAllCustomersResponse>>;

/// <summary>
/// Müşteri listesi yanıtı
/// </summary>
public record GetAllCustomersResponse(
    IReadOnlyList<CustomerListItemResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Liste için müşteri item'ı
/// </summary>
public record CustomerListItemResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt);


