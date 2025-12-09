using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Commands.CreateOrder;

/// <summary>
/// CreateOrderCommand handler
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IRepository<Order, long> _orderRepository;
    private readonly IRepository<Customer, long> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IRepository<Order, long> orderRepository,
        IRepository<Customer, long> customerRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateOrderResponse>> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // Müşteri kontrolü
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw BusinessExceptionFactory.NotFound("Customer", request.CustomerId);
        }

        // Sipariş oluştur
        var order = Order.Create(request.CustomerId, request.Notes);

        // Sipariş kalemlerini ekle
        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var itemResponses = order.Items.Select(i => new CreateOrderItemResponse(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice)).ToList();

        var response = new CreateOrderResponse(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Status,
            order.OrderDate,
            itemResponses);

        return Result<CreateOrderResponse>.Success(response, "Sipariş başarıyla oluşturuldu");
    }
}


