using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Yeni müşteri oluşturma komutu
/// CQRS pattern - Command örneği
/// </summary>
public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : IRequest<Result<CreateCustomerResponse>>;

/// <summary>
/// Müşteri oluşturma yanıtı
/// </summary>
public record CreateCustomerResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);

