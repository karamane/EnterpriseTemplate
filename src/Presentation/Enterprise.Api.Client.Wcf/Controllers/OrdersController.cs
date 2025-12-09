using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Wcf.Controllers;

/// <summary>
/// WCF Client - Order API Controller
/// Server API üzerinden sipariş işlemleri (DMZ kuralı: Sadece Server API tüketilir)
/// </summary>
[Authorize]
public class OrdersController : BaseWcfApiController
{
    private readonly IWcfServerApiClient _serverApiClient;

    public OrdersController(
        IWcfServerApiClient serverApiClient,
        ICorrelationContext correlationContext)
        : base(correlationContext)
    {
        _serverApiClient = serverApiClient;
    }

    /// <summary>
    /// Sipariş detayını getirir
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(WcfApiResponse<WcfOrderResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<WcfOrderResponse>>> GetOrder(long id)
    {
        var result = await _serverApiClient.GetAsync<OrderServerResponse>($"api/v1/orders/{id}");

        if (result?.Success != true || result.Data == null)
        {
            return NotFound(Error<WcfOrderResponse>("Sipariş bulunamadı"));
        }

        var response = MapToWcfOrderResponse(result.Data);
        return Success(response);
    }

    /// <summary>
    /// Müşterinin siparişlerini getirir
    /// </summary>
    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(WcfApiResponse<List<WcfOrderResponse>>), 200)]
    public async Task<ActionResult<WcfApiResponse<List<WcfOrderResponse>>>> GetCustomerOrders(long customerId)
    {
        var result = await _serverApiClient.GetAsync<OrderListServerResponse>($"api/v1/orders/customer/{customerId}");

        if (result?.Success != true)
        {
            return BadRequest(Error<List<WcfOrderResponse>>(result?.Message ?? "İstek başarısız"));
        }

        var response = result.Data?.Items?.Select(MapToWcfOrderResponse).ToList() ?? new List<WcfOrderResponse>();
        return Success(response);
    }

    /// <summary>
    /// Yeni sipariş oluşturur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WcfApiResponse<WcfOrderResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<WcfApiResponse<WcfOrderResponse>>> CreateOrder(
        [FromBody] WcfOrderRequest request)
    {
        var serverRequest = new
        {
            customerId = request.CustomerId,
            items = request.Items?.Select(i => new
            {
                productId = i.ProductId,
                productName = i.ProductName,
                quantity = i.Quantity,
                unitPrice = i.UnitPrice
            }).ToList()
        };

        var result = await _serverApiClient.PostAsync<object, OrderServerResponse>(
            "api/v1/orders", serverRequest);

        if (result?.Success != true || result.Data == null)
        {
            return BadRequest(Error<WcfOrderResponse>(result?.Message ?? "Sipariş oluşturulamadı"));
        }

        var response = MapToWcfOrderResponse(result.Data);
        return Created(response);
    }

    /// <summary>
    /// Siparişi iptal eder
    /// </summary>
    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(typeof(WcfApiResponse<object>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<object>>> CancelOrder(long id)
    {
        var result = await _serverApiClient.PostAsync<object, OrderServerResponse>(
            $"api/v1/orders/{id}/cancel", new { });

        if (result?.Success != true)
        {
            return NotFound(Error<object>(result?.Message ?? "Sipariş iptal edilemedi"));
        }

        return Success<object>(null!, "Sipariş iptal edildi");
    }

    private static WcfOrderResponse MapToWcfOrderResponse(OrderData data)
    {
        return new WcfOrderResponse
        {
            Id = data.Id,
            CustomerId = data.CustomerId,
            OrderDate = data.OrderDate,
            Status = data.Status,
            TotalAmount = data.TotalAmount,
            Items = data.Items?.Select(i => new WcfOrderItemResponse
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList() ?? new List<WcfOrderItemResponse>()
        };
    }
}

#region Server API Response Types

internal class OrderServerResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public OrderData? Data { get; set; }
}

internal class OrderListServerResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public OrderListData? Data { get; set; }
}

internal class OrderData
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<OrderItemData>? Items { get; set; }
}

internal class OrderItemData
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

internal class OrderListData
{
    public List<OrderData>? Items { get; set; }
    public int TotalCount { get; set; }
}

#endregion
