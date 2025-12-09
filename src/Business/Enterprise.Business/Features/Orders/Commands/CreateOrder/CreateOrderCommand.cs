using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Commands.CreateOrder;

/// <summary>
/// Sipariş oluşturma komutu
/// </summary>
public record CreateOrderCommand(
    long CustomerId,
    IReadOnlyList<CreateOrderItemCommand> Items,
    string? Notes) : IRequest<Result<CreateOrderResponse>>;

/// <summary>
/// Sipariş kalemi komutu
/// </summary>
public record CreateOrderItemCommand(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// Sipariş oluşturma yanıtı
/// </summary>
public record CreateOrderResponse(
    long Id,
    long CustomerId,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    IReadOnlyList<CreateOrderItemResponse> Items);

/// <summary>
/// Sipariş kalemi yanıtı
/// </summary>
public record CreateOrderItemResponse(
    long Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);


