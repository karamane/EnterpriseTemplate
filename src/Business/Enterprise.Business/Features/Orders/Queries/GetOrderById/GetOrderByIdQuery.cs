using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Queries.GetOrderById;

/// <summary>
/// Sipariş detayı sorgulama
/// </summary>
public record GetOrderByIdQuery(long OrderId) : IRequest<Result<OrderDetailResponse>>;

/// <summary>
/// Sipariş detay yanıtı
/// </summary>
public record OrderDetailResponse(
    long Id,
    long CustomerId,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    string? Notes,
    DateTime OrderDate,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDetailResponse> Items);

/// <summary>
/// Sipariş kalemi detay yanıtı
/// </summary>
public record OrderItemDetailResponse(
    long Id,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);


