using AutoMapper;
using Enterprise.Api.Server.DTOs;
using Enterprise.Business.Features.Orders.Commands.CancelOrder;
using Enterprise.Business.Features.Orders.Commands.CreateOrder;
using Enterprise.Business.Features.Orders.Queries.GetOrderById;
using Enterprise.Business.Features.Orders.Queries.GetOrdersByCustomer;
using Enterprise.Core.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Server.Controllers;

/// <summary>
/// Sipariş API'si
/// Order CRUD operasyonları
/// </summary>
public class OrdersController : BaseApiController
{
    private readonly IMapper _mapper;

    public OrdersController(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Sipariş detayını getirir
    /// </summary>
    /// <param name="id">Sipariş ID</param>
    /// <returns>Sipariş detayı</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<OrderApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await Mediator.Send(new GetOrderByIdQuery(id));

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        var apiResponse = _mapper.Map<OrderApiResponse>(result.Data);
        return Ok(new ApiResponse<OrderApiResponse>
        {
            Success = true,
            Data = apiResponse,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Müşteriye ait siparişleri getirir
    /// </summary>
    /// <param name="customerId">Müşteri ID</param>
    /// <returns>Sipariş listesi</returns>
    [HttpGet("customer/{customerId:long}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerOrderApiResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCustomer(long customerId)
    {
        var result = await Mediator.Send(new GetOrdersByCustomerQuery(customerId));

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        var apiResponse = _mapper.Map<IReadOnlyList<CustomerOrderApiResponse>>(result.Data);
        return Ok(new ApiResponse<IReadOnlyList<CustomerOrderApiResponse>>
        {
            Success = true,
            Data = apiResponse,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Yeni sipariş oluşturur
    /// </summary>
    /// <param name="request">Sipariş bilgileri</param>
    /// <returns>Oluşturulan sipariş</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderApiResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateOrderApiRequest request)
    {
        var command = _mapper.Map<CreateOrderCommand>(request);
        var result = await Mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            var apiResponse = _mapper.Map<OrderApiResponse>(result.Data);

            return Created(
                $"/api/v1/orders/{apiResponse.Id}",
                new ApiResponse<OrderApiResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = apiResponse,
                    CorrelationId = CorrelationContext.CorrelationId
                });
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Siparişi iptal eder
    /// </summary>
    /// <param name="id">Sipariş ID</param>
    /// <returns>İptal sonucu</returns>
    [HttpPost("{id:long}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(long id)
    {
        var result = await Mediator.Send(new CancelOrderCommand(id));

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = result.Message,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }
}


