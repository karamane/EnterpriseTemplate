namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Paged Customer List API Response
/// </summary>
public record PagedCustomerApiResponse(
    IReadOnlyList<CustomerApiResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);


