namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Order response DTO
/// </summary>
public class WcfOrderResponse
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public List<WcfOrderItemResponse> Items { get; set; } = new();
}

/// <summary>
/// WCF Client API - Order item response DTO
/// </summary>
public class WcfOrderItemResponse
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
