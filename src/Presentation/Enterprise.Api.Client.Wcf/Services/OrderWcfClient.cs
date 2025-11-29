using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Core.Application.Interfaces.Logging;
using Enterprise.Core.Shared.ErrorCodes;
using Enterprise.Core.Shared.Exceptions;
using Enterprise.Proxy.Core.Wcf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// Order WCF Service Client
/// WCF proxy ile ServerApi'ye bağlanır
/// </summary>
public class OrderWcfClient : WcfProxyBase<IOrderWcfService>, IOrderWcfClient
{
    public OrderWcfClient(
        IOptions<WcfClientOptions> options,
        ILogger<OrderWcfClient> logger,
        ILogService logService,
        ICorrelationContext correlationContext)
        : base(
            options.Value.OrderServiceEndpoint,
            options.Value.Binding,
            logger,
            logService,
            correlationContext)
    {
    }

    public async Task<WcfOrderDto?> GetOrderAsync(int id)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.GetOrderAsync(id);

                if (!response.Success)
                {
                    throw BusinessExceptionFactory.NotFound("Order", id);
                }

                return response.Data;
            },
            $"GetOrder({id})");
    }

    public async Task<List<WcfOrderDto>> GetCustomerOrdersAsync(int customerId)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.GetCustomerOrdersAsync(customerId);

                if (!response.Success)
                {
                    throw new BusinessException(CommonErrorCodes.GeneralError, response.Message);
                }

                return response.Data ?? new List<WcfOrderDto>();
            },
            $"GetCustomerOrders({customerId})");
    }

    public async Task<WcfOrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.CreateOrderAsync(request);

                if (!response.Success)
                {
                    throw new BusinessException(CommonErrorCodes.GeneralError, response.Message);
                }

                return response.Data!;
            },
            "CreateOrder");
    }

    public async Task<bool> CancelOrderAsync(int id)
    {
        return await ExecuteAsync(
            async client =>
            {
                var response = await client.CancelOrderAsync(id);

                if (!response.Success)
                {
                    throw BusinessExceptionFactory.NotFound("Order", id);
                }

                return response.Data;
            },
            $"CancelOrder({id})");
    }
}

/// <summary>
/// Order WCF Client interface
/// </summary>
public interface IOrderWcfClient
{
    Task<WcfOrderDto?> GetOrderAsync(int id);
    Task<List<WcfOrderDto>> GetCustomerOrdersAsync(int customerId);
    Task<WcfOrderDto> CreateOrderAsync(CreateOrderRequest request);
    Task<bool> CancelOrderAsync(int id);
}

