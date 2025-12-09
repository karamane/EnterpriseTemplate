using AutoMapper;
using Enterprise.Api.Client.DTOs;
using Enterprise.Api.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Controllers;

/// <summary>
/// Client API - Sipariş endpoint'leri
/// Mobil uygulamalar için optimize edilmiş
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IServerApiClient _serverApiClient;
    private readonly IMapper _mapper;

    public OrdersController(
        IServerApiClient serverApiClient,
        IMapper mapper)
    {
        _serverApiClient = serverApiClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Sipariş detayını getirir
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(OrderClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var serverResponse = await _serverApiClient.GetAsync<ServerApiResponse<ServerOrderResponse>>(
            $"/api/v1/orders/{id}");

        if (serverResponse?.Data == null)
        {
            return NotFound(new { Message = "Sipariş bulunamadı" });
        }

        var clientResponse = _mapper.Map<OrderClientResponse>(serverResponse.Data);
        return Ok(clientResponse);
    }

    /// <summary>
    /// Müşterinin siparişlerini getirir
    /// </summary>
    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(OrderListClientResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(long customerId)
    {
        var serverResponse = await _serverApiClient.GetAsync<ServerApiResponse<ServerOrderListResponse>>(
            $"/api/v1/orders/customer/{customerId}");

        if (serverResponse?.Data == null)
        {
            return Ok(new OrderListClientResponse(new List<OrderClientResponse>(), 0));
        }

        var clientResponse = _mapper.Map<OrderListClientResponse>(serverResponse.Data);
        return Ok(clientResponse);
    }

    /// <summary>
    /// Yeni sipariş oluşturur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderClientRequest request)
    {
        var serverRequest = _mapper.Map<ServerCreateOrderRequest>(request);

        var serverResponse = await _serverApiClient.PostAsync<ServerCreateOrderRequest, ServerApiResponse<ServerOrderResponse>>(
            "/api/v1/orders",
            serverRequest);

        if (serverResponse?.Data == null)
        {
            return BadRequest(new { Message = serverResponse?.Message ?? "Sipariş oluşturulamadı" });
        }

        var clientResponse = _mapper.Map<OrderClientResponse>(serverResponse.Data);
        return Created($"/api/v1/orders/{clientResponse.Id}", clientResponse);
    }

    /// <summary>
    /// Siparişi iptal eder
    /// </summary>
    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(long id)
    {
        var serverResponse = await _serverApiClient.PostAsync<object, ServerApiResponse<bool>>(
            $"/api/v1/orders/{id}/cancel",
            new { });

        if (serverResponse?.Success != true)
        {
            return NotFound(new { Message = "Sipariş bulunamadı veya iptal edilemedi" });
        }

        return Ok(new { Message = "Sipariş başarıyla iptal edildi" });
    }
}


