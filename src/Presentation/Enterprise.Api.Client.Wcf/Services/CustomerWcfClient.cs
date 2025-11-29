using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Shared.ErrorCodes;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Proxy.Core.Wcf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// Customer WCF Service Client
/// WCF proxy ile ServerApi'ye bağlanır
/// </summary>
public class CustomerWcfClient : WcfProxyBase<ICustomerWcfService>, ICustomerWcfClient
{
    public CustomerWcfClient(
        IOptions<WcfClientOptions> options,
        ILogger<CustomerWcfClient> logger,
        ILogService logService,
        ICorrelationContext correlationContext)
        : base(
            options.Value.CustomerServiceEndpoint,
            options.Value.Binding,
            logger,
            logService,
            correlationContext)
    {
    }

    public async Task<WcfCustomerDto?> GetCustomerAsync(int id)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.GetCustomerAsync(id);

                if (!response.Success)
                {
                    throw BusinessExceptionFactory.NotFound("Customer", id);
                }

                return response.Data;
            },
            $"GetCustomer({id})");
    }

    public async Task<CustomerListResponse> GetCustomersAsync(int page, int pageSize, string? searchTerm = null)
    {
        return await ExecuteAsync(
            async client =>
            {
                var request = new CustomerListRequest
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                };

                var response = await client.GetCustomersAsync(request);

                if (!response.Success)
                {
                    throw new BusinessException(CommonErrorCodes.GeneralError, response.Message);
                }

                return response.Data ?? new CustomerListResponse();
            },
            $"GetCustomers(page={page}, pageSize={pageSize})");
    }

    public async Task<WcfCustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.CreateCustomerAsync(request);

                if (!response.Success)
                {
                    throw new BusinessException(CommonErrorCodes.GeneralError, response.Message);
                }

                return response.Data!;
            },
            "CreateCustomer");
    }

    public async Task<WcfCustomerDto> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.UpdateCustomerAsync(request);

                if (!response.Success)
                {
                    throw BusinessExceptionFactory.NotFound("Customer", request.Id);
                }

                return response.Data!;
            },
            $"UpdateCustomer({request.Id})");
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.DeleteCustomerAsync(id);

                if (!response.Success)
                {
                    throw BusinessExceptionFactory.NotFound("Customer", id);
                }

                return response.Data;
            },
            $"DeleteCustomer({id})");
    }
}

/// <summary>
/// Customer WCF Client interface
/// </summary>
public interface ICustomerWcfClient
{
    Task<WcfCustomerDto?> GetCustomerAsync(int id);
    Task<CustomerListResponse> GetCustomersAsync(int page, int pageSize, string? searchTerm = null);
    Task<WcfCustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<WcfCustomerDto> UpdateCustomerAsync(UpdateCustomerRequest request);
    Task<bool> DeleteCustomerAsync(int id);
}

/// <summary>
/// WCF Client yapılandırma seçenekleri
/// </summary>
public class WcfClientOptions
{
    public const string SectionName = "WcfClient";

    /// <summary>
    /// Customer Service WCF endpoint adresi
    /// </summary>
    public string CustomerServiceEndpoint { get; set; } = "http://localhost:5001/CustomerService.svc";

    /// <summary>
    /// Order Service WCF endpoint adresi
    /// </summary>
    public string OrderServiceEndpoint { get; set; } = "http://localhost:5001/OrderService.svc";

    /// <summary>
    /// WCF Binding tipi (BasicHttp, NetTcp, WSHttp)
    /// </summary>
    public WcfBindingType Binding { get; set; } = WcfBindingType.BasicHttp;

    /// <summary>
    /// Timeout (saniye)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Max message size (bytes)
    /// </summary>
    public int MaxMessageSize { get; set; } = 65536;
}


