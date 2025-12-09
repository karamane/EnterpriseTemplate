namespace Enterprise.Business.DTOs;

/// <summary>
/// Customer List Business Response
/// </summary>
public record CustomerListBusinessResponse(
    IReadOnlyList<CustomerBusinessDto> Customers,
    int TotalCount);


