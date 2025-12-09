namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Customer List Client Response - Mobil liste görünümü için
/// </summary>
public class CustomerListClientResponse
{
    public IReadOnlyList<CustomerClientResponse> Customers { get; set; } = new List<CustomerClientResponse>();
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }

    public CustomerListClientResponse() { }

    public CustomerListClientResponse(IReadOnlyList<CustomerClientResponse> customers, int totalCount, bool hasMore)
    {
        Customers = customers;
        TotalCount = totalCount;
        HasMore = hasMore;
    }
}
