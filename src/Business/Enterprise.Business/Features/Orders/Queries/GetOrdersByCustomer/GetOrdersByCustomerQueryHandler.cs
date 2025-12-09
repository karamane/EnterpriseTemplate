using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Orders.Queries.GetOrdersByCustomer;

/// <summary>
/// GetOrdersByCustomerQuery handler
/// </summary>
public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, Result<IReadOnlyList<CustomerOrderResponse>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRepository<Enterprise.Core.Domain.Entities.Sample.Customer, long> _customerRepository;

    public GetOrdersByCustomerQueryHandler(
        IOrderRepository orderRepository,
        IRepository<Enterprise.Core.Domain.Entities.Sample.Customer, long> customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
    }

    public async Task<Result<IReadOnlyList<CustomerOrderResponse>>> Handle(
        GetOrdersByCustomerQuery request,
        CancellationToken cancellationToken)
    {
        // Müşteri var mı kontrol et
        var customerExists = await _customerRepository.ExistsAsync(
            c => c.Id == request.CustomerId,
            cancellationToken);

        if (!customerExists)
        {
            throw BusinessExceptionFactory.NotFound("Customer", request.CustomerId);
        }

        var orders = await _orderRepository.GetByCustomerIdWithDetailsAsync(
            request.CustomerId,
            cancellationToken);

        var response = orders.Select(o => new CustomerOrderResponse(
            o.Id,
            o.TotalAmount,
            o.Status,
            o.OrderDate,
            o.Items.Count)).ToList();

        return Result<IReadOnlyList<CustomerOrderResponse>>.Success(response);
    }
}
