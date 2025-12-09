namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Order request DTO
/// </summary>
public class WcfOrderRequest
{
    public long CustomerId { get; set; }
    public List<WcfOrderItemRequest>? Items { get; set; }
    public string? Notes { get; set; }
}
