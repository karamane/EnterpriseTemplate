namespace Enterprise.Core.Domain.Entities.Sample;

/// <summary>
/// Sipariş kalemi entity
/// </summary>
public class OrderItem : AuditableEntity<long>
{
    /// <summary>
    /// Sipariş ID
    /// </summary>
    public long OrderId { get; private set; }

    /// <summary>
    /// Sipariş (navigation property)
    /// </summary>
    public Order? Order { get; private set; }

    /// <summary>
    /// Ürün ID
    /// </summary>
    public int ProductId { get; private set; }

    /// <summary>
    /// Ürün adı
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    /// Miktar
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// Birim fiyat
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// Toplam fiyat (hesaplanan)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

    /// <summary>
    /// Constructor (EF Core için)
    /// </summary>
    protected OrderItem() { }

    /// <summary>
    /// Yeni sipariş kalemi oluşturur
    /// </summary>
    public static OrderItem Create(
        long orderId,
        int productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar 0'dan büyük olmalı", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Birim fiyat negatif olamaz", nameof(unitPrice));

        return new OrderItem
        {
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Miktarı günceller
    /// </summary>
    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar 0'dan büyük olmalı", nameof(quantity));

        Quantity = quantity;
    }
}


