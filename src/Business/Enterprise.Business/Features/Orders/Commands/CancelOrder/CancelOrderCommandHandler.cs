using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Commands.CancelOrder;

/// <summary>
/// CancelOrderCommand handler
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<CancelOrderResponse>>
{
    private readonly IRepository<Order, long> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IRepository<Order, long> orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CancelOrderResponse>> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
            throw BusinessExceptionFactory.NotFound("Order", request.OrderId);
        }

        order.Cancel();
        
        await _orderRepository.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CancelOrderResponse(
            order.Id,
            order.Status,
            order.UpdatedAt ?? DateTime.UtcNow);

        return Result<CancelOrderResponse>.Success(response, "Sipariş başarıyla iptal edildi");
    }
}


