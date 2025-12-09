namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Customer list response DTO
/// </summary>
public class WcfCustomerListResponse
{
    public List<WcfCustomerResponse> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
