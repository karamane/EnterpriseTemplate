namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Order Client Request - Sipariş oluşturma
/// </summary>
public record CreateOrderClientRequest(
    int CustomerId,
    List<OrderItemClientRequest> Items,
    string? Notes);

/// <summary>
/// Order Item Client Request
/// </summary>
public record OrderItemClientRequest(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// Order Client Response
/// </summary>
public record OrderClientResponse(
    string Id,
    string CustomerId,
    string? CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    List<OrderItemClientResponse> Items);

/// <summary>
/// Order Item Client Response
/// </summary>
public record OrderItemClientResponse(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

/// <summary>
/// Order List Client Response
/// </summary>
public record OrderListClientResponse(
    List<OrderClientResponse> Items,
    int TotalCount);


