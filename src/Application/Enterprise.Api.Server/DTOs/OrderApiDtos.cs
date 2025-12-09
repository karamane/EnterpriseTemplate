namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Sipariş oluşturma API isteği
/// </summary>
public record CreateOrderApiRequest(
    long CustomerId,
    IReadOnlyList<CreateOrderItemApiRequest> Items,
    string? Notes);

/// <summary>
/// Sipariş kalemi oluşturma API isteği
/// </summary>
public record CreateOrderItemApiRequest(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// Sipariş API yanıtı
/// </summary>
public record OrderApiResponse(
    long Id,
    long CustomerId,
    string? CustomerName,
    decimal TotalAmount,
    string Status,
    string? Notes,
    DateTime OrderDate,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemApiResponse> Items);

/// <summary>
/// Sipariş kalemi API yanıtı
/// </summary>
public record OrderItemApiResponse(
    long Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

/// <summary>
/// Müşteri siparişi listesi API yanıtı
/// </summary>
public record CustomerOrderApiResponse(
    long Id,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    int ItemCount);


