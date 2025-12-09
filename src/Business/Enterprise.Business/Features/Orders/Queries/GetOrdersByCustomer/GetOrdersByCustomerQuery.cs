using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// Müşteriye ait siparişleri sorgulama
/// </summary>
public record GetOrdersByCustomerQuery(long CustomerId) : IRequest<Result<IReadOnlyList<CustomerOrderResponse>>>;

/// <summary>
/// Müşteri siparişi yanıtı
/// </summary>
public record CustomerOrderResponse(
    long Id,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    int ItemCount);


