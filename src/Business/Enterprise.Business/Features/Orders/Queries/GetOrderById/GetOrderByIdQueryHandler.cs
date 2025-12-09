using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Queries.GetOrderById;

/// <summary>
/// GetOrderByIdQuery handler
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDetailResponse>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithDetailsAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            throw BusinessExceptionFactory.NotFound("Order", request.OrderId);
        }

        var items = order.Items.Select(i => new OrderItemDetailResponse(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice)).ToList();

        var response = new OrderDetailResponse(
            order.Id,
            order.CustomerId,
            order.Customer != null ? $"{order.Customer.FirstName} {order.Customer.LastName}" : string.Empty,
            order.TotalAmount,
            order.Status,
            order.Notes,
            order.OrderDate,
            order.CreatedAt,
            items);

        return Result<OrderDetailResponse>.Success(response);
    }
}
