using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Queries.GetAllCustomers;

/// <summary>
/// GetAllCustomersQuery handler
/// NOT: Loglama AutoLoggingBehavior tarafından merkezi olarak yapılır
/// </summary>
public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, Result<GetAllCustomersResponse>>
{
    private readonly IRepository<Customer, long> _customerRepository;

    public GetAllCustomersQueryHandler(IRepository<Customer, long> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<GetAllCustomersResponse>> Handle(
        GetAllCustomersQuery request,
        CancellationToken cancellationToken)
    {
        // Tüm müşterileri getir (filtreleme opsiyonel)
        var allCustomers = await _customerRepository.GetAllAsync(cancellationToken);

        // Search filtresi uygula
        var query = allCustomers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLowerInvariant().Contains(searchLower) ||
                c.LastName.ToLowerInvariant().Contains(searchLower) ||
                c.Email.ToLowerInvariant().Contains(searchLower));
        }

        // Toplam sayı
        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        // Sayfalama uygula
        var pagedCustomers = query
            .OrderByDescending(c => c.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // DTO'ya dönüştür
        var items = pagedCustomers.Select(c => new CustomerListItemResponse(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.PhoneNumber,
            c.IsActive,
            c.RegisteredAt)).ToList();

        var response = new GetAllCustomersResponse(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize,
            totalPages);

        return Result<GetAllCustomersResponse>.Success(response);
    }
}


