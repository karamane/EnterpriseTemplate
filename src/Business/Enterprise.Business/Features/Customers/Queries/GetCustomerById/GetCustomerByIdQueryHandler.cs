using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// GetCustomerByIdQuery handler
/// NOT: Loglama AutoLoggingBehavior tarafından merkezi olarak yapılır
/// </summary>
public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly IRepository<Customer, long> _customerRepository;

    public GetCustomerByIdQueryHandler(IRepository<Customer, long> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerDto>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
        {
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

