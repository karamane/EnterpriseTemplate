using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Queries.GetCustomerById;

/// <summary>
/// Müşteri sorgulama
/// CQRS pattern - Query örneği
/// </summary>
public record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerDto>>;

/// <summary>
/// Customer DTO
/// </summary>
public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt);

