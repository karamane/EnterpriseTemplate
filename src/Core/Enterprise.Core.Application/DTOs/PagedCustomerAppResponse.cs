namespace Enterprise.Core.Application.DTOs;

/// <summary>
/// Paged Customer List Response
/// </summary>
public record PagedCustomerAppResponse(
    IReadOnlyList<CustomerAppDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);


