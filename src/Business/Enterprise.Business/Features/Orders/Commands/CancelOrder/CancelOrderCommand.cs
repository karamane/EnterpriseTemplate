using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Commands.CancelOrder;

/// <summary>
/// Sipariş iptal komutu
/// </summary>
public record CancelOrderCommand(long OrderId) : IRequest<Result<CancelOrderResponse>>;

/// <summary>
/// Sipariş iptal yanıtı
/// </summary>
public record CancelOrderResponse(
    long Id,
    string Status,
    DateTime CancelledAt);


