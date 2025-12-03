using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Application.Interfaces.Persistence;
using Enterprise.Core.Application.Models.Logging;
using Enterprise.Core.Domain.Entities.Sample;
using Enterprise.Core.Shared.Constants;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Core.Shared.Results;
using MediatR;

namespace Enterprise.Business.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// CreateCustomerCommand handler
/// Business logic implementasyonu örneği
/// NOT: Loglama AutoLoggingBehavior tarafından merkezi olarak yapılır
/// </summary>
public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CreateCustomerResponse>>
{
    private readonly IRepository<Customer, long> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogService _logService;

    public CreateCustomerCommandHandler(
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

    public async Task<Result<CreateCustomerResponse>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        // Business rule: Email benzersiz olmalı
        var existingCustomer = await _customerRepository.ExistsAsync(
            c => c.Email == request.Email,
            cancellationToken);

        if (existingCustomer)
        {
            throw new BusinessException(
                "Bu email adresi ile kayıtlı bir müşteri zaten mevcut.",
                "CUSTOMER_EMAIL_EXISTS")
                .WithUserMessage("Bu email adresi kullanılmaktadır. Lütfen farklı bir email adresi giriniz.");
        }

        // Entity oluştur
        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        // Kaydet
        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log (iş eventi - merkezi loglama dışında)
        await _logService.LogAuditAsync(new AuditLogEntry
        {
            CorrelationId = _correlationContext.CorrelationId,
            Layer = LogConstants.Layers.Business,
            Action = "Create",
            EntityType = nameof(Customer),
            EntityId = customer.Id.ToString(),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber
            }),
            IsSuccess = true,
            UserId = _correlationContext.UserId
        }, cancellationToken);

        var response = new CreateCustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.CreatedAt);

        return Result<CreateCustomerResponse>.Success(response, "Müşteri başarıyla oluşturuldu.");
    }
}

