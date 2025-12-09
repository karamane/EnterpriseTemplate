using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Constants;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// UpdateCustomerCommand handler
/// NOT: Loglama AutoLoggingBehavior tarafından merkezi olarak yapılır
/// </summary>
public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<UpdateCustomerResponse>>
{
    private readonly IRepository<Customer, long> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogService _logService;

    public UpdateCustomerCommandHandler(
        IRepository<Customer, long> customerRepository,
        IUnitOfWork unitOfWork,
        ICorrelationContext correlationContext,
        ILogService logService)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _correlationContext = correlationContext;
        _logService = logService;
    }

    public async Task<Result<UpdateCustomerResponse>> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // Müşteriyi bul
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
        {
            throw BusinessExceptionFactory.NotFound("Customer", request.Id);
        }

        // Eski değerleri sakla (audit için)
        var oldValues = new
        {
            customer.FirstName,
            customer.LastName,
            customer.PhoneNumber
        };

        // Güncelle
        customer.Update(request.FirstName, request.LastName, request.PhoneNumber);

        // Kaydet
        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        await _logService.LogAuditAsync(new AuditLogEntry
        {
            CorrelationId = _correlationContext.CorrelationId,
            Layer = LogConstants.Layers.Business,
            Action = "Update",
            EntityType = nameof(Customer),
            EntityId = customer.Id.ToString(),
            OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                customer.FirstName,
                customer.LastName,
                customer.PhoneNumber
            }),
            IsSuccess = true,
            UserId = _correlationContext.UserId
        }, cancellationToken);

        var response = new UpdateCustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.PhoneNumber,
            customer.IsActive,
            customer.UpdatedAt ?? DateTime.UtcNow);

        return Result<UpdateCustomerResponse>.Success(response, "Müşteri başarıyla güncellendi.");
    }
}


