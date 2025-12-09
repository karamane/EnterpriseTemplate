using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Commands.DeleteCustomer;

/// <summary>
/// Müşteri silme komutu (Soft delete)
/// </summary>
public record DeleteCustomerCommand(long Id) : IRequest<Result<bool>>;


