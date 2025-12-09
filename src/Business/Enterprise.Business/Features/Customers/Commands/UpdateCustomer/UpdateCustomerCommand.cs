using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Müşteri güncelleme komutu
/// </summary>
public record UpdateCustomerCommand(
    long Id,
    string FirstName,
    string LastName,
    string? PhoneNumber) : IRequest<Result<UpdateCustomerResponse>>;

/// <summary>
/// Müşteri güncelleme yanıtı
/// </summary>
public record UpdateCustomerResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime UpdatedAt);


