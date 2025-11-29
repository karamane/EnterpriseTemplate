using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enterprise.Business.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// GetCustomerByIdQuery handler
/// </summary>
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly ILogger<GetCustomerByIdQueryHandler> _logger;
    private readonly ICorrelationContext _correlationContext;

    public GetCustomerByIdQueryHandler(
        IRepository<Customer> customerRepository,
        ILogger<GetCustomerByIdQueryHandler> logger,
        ICorrelationContext correlationContext)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _correlationContext = correlationContext;
    }

    public async Task<Result<CustomerDto>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "[{CorrelationId}] Getting customer by ID: {CustomerId}",
            _correlationContext.CorrelationId, request.Id);

        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning(
                "[{CorrelationId}] Customer not found: {CustomerId}",
                _correlationContext.CorrelationId, request.Id);

            throw new NotFoundException(nameof(Customer), request.Id.ToString());
        }

        var dto = new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.IsActive,
            customer.RegisteredAt,
            customer.CreatedAt);

        return Result<CustomerDto>.Success(dto);
    }
}

