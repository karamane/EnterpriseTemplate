using AutoMapper;
using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Wcf.Controllers;

/// <summary>
/// WCF Client - Order API Controller
/// WCF servisleri üzerinden sipariş işlemleri
/// </summary>
public class OrdersController : BaseWcfApiController
{
    private readonly IOrderWcfClient _orderClient;
    private readonly IMapper _mapper;

    public OrdersController(
        IOrderWcfClient orderClient,
        IMapper mapper,
        ICorrelationContext correlationContext)
        : base(correlationContext)
    {
        _orderClient = orderClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Sipariş detayını getirir (WCF üzerinden)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WcfApiResponse<WcfOrderResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<WcfOrderResponse>>> GetOrder(int id)
    {
        var order = await _orderClient.GetOrderAsync(id);

        if (order == null)
        {
            return NotFound(new WcfApiResponse<WcfOrderResponse>
            {
                Success = false,
                Message = "Sipariş bulunamadı",
                CorrelationId = CorrelationContext.CorrelationId
            });
        }

        var response = _mapper.Map<WcfOrderResponse>(order);

        return Success(response);
    }

    /// <summary>
    /// Müşterinin siparişlerini getirir (WCF üzerinden)
    /// </summary>
    [HttpGet("customer/{customerId:int}")]
    [ProducesResponseType(typeof(WcfApiResponse<List<WcfOrderResponse>>), 200)]
    public async Task<ActionResult<WcfApiResponse<List<WcfOrderResponse>>>> GetCustomerOrders(int customerId)
    {
        var orders = await _orderClient.GetCustomerOrdersAsync(customerId);

        var response = _mapper.Map<List<WcfOrderResponse>>(orders);

        return Success(response);
    }

    /// <summary>
    /// Yeni sipariş oluşturur (WCF üzerinden)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WcfApiResponse<WcfOrderResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<WcfApiResponse<WcfOrderResponse>>> CreateOrder(
        [FromBody] WcfOrderRequest request)
    {
        var wcfRequest = _mapper.Map<CreateOrderRequest>(request);

        var order = await _orderClient.CreateOrderAsync(wcfRequest);

        var response = _mapper.Map<WcfOrderResponse>(order);

        return Created(response);
    }

    /// <summary>
    /// Siparişi iptal eder (WCF üzerinden)
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(WcfApiResponse<object>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<object>>> CancelOrder(int id)
    {
        await _orderClient.CancelOrderAsync(id);

        return Success<object>(null!, "Sipariş iptal edildi");
    }
}

