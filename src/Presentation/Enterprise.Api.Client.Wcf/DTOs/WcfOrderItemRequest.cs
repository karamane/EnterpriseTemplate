namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Order item request DTO
/// </summary>
public class WcfOrderItemRequest
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
