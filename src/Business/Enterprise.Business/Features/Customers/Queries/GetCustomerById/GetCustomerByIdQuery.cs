using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Müşteri sorgulama
/// CQRS pattern - Query örneği
/// </summary>
public record GetCustomerByIdQuery(long Id) : IRequest<Result<CustomerDto>>;

/// <summary>
/// Customer DTO
/// </summary>
public record CustomerDto(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt);

