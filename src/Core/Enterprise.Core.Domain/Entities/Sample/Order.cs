namespace Enterprise.Core.Domain.Entities.Sample;

/// <summary>
/// Sipariş entity
/// </summary>
public class Order : SoftDeleteEntity<long>
{
    /// <summary>
    /// Müşteri ID
    /// </summary>
    public long CustomerId { get; private set; }

    /// <summary>
    /// Müşteri (navigation property)
    /// </summary>
    public Customer? Customer { get; private set; }

    /// <summary>
    /// Toplam tutar
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Sipariş durumu
    /// </summary>
    public string Status { get; private set; } = OrderStatus.Pending;

    /// <summary>
    /// Sipariş notları
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Sipariş tarihi
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    /// Sipariş kalemleri (navigation property)
    /// </summary>
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    /// <summary>
    /// Constructor (EF Core için)
    /// </summary>
    protected Order() { }

    /// <summary>
    /// Yeni sipariş oluşturur
    /// </summary>
    public static Order Create(
        long customerId,
        string? notes = null)
    {
        return new Order
        {
            CustomerId = customerId,
            TotalAmount = 0,
            Status = OrderStatus.Pending,
            Notes = notes,
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Sipariş kalemi ekler
    /// </summary>
    public void AddItem(int productId, string productName, int quantity, decimal unitPrice)
    {
        var item = OrderItem.Create(Id, productId, productName, quantity, unitPrice);
        Items.Add(item);
        RecalculateTotal();
    }

    /// <summary>
    /// Toplam tutarı yeniden hesaplar
    /// </summary>
    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.TotalPrice);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Siparişi onaylar
    /// </summary>
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen siparişler onaylanabilir");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Siparişi kargoya verir
    /// </summary>
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Sadece onaylanmış siparişler kargoya verilebilir");

        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Siparişi teslim eder
    /// </summary>
    public void Deliver()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException("Sadece kargodaki siparişler teslim edilebilir");

        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Siparişi iptal eder
    /// </summary>
    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Teslim edilmiş siparişler iptal edilemez");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Notları günceller
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Sipariş durumları
/// </summary>
public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Shipped = "Shipped";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}


