using AutoMapper;
using Enterprise.Api.Server.DTOs;
using Enterprise.Business.Features.Customers.Commands.CreateCustomer;
using Enterprise.Business.Features.Customers.Queries.GetCustomerById;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Server.Controllers;

/// <summary>
/// Müşteri API'si
/// Örnek CRUD operasyonları - DTO mapping ile
/// </summary>
public class CustomersController : BaseApiController
{
    private readonly IMapper _mapper;

    public CustomersController(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Müşteri bilgisi getirir
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <returns>Müşteri bilgisi</returns>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id));

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        // Business DTO -> API DTO mapping
        var apiResponse = _mapper.Map<CustomerApiResponse>(result.Data);
        return Ok(new ApiResponse<CustomerApiResponse>
        {
            Success = true,
            Data = apiResponse,
            CorrelationId = CorrelationContext.CorrelationId
        });
    }

    /// <summary>
    /// Yeni müşteri oluşturur
    /// </summary>
    /// <param name="request">Müşteri bilgileri (API DTO)</param>
    /// <returns>Oluşturulan müşteri</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateCustomerApiResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerApiRequest request)
    {
        // API DTO -> Business Command mapping
        var command = _mapper.Map<CreateCustomerCommand>(request);
        var result = await Mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            // Business Response -> API Response mapping
            var apiResponse = _mapper.Map<CreateCustomerApiResponse>(result.Data);
            
            return Created(
                $"/api/v1/customers/{apiResponse.Id}",
                new ApiResponse<CreateCustomerApiResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = apiResponse,
                    CorrelationId = CorrelationContext.CorrelationId
                });
        }

        return HandleResult(result);
    }
}
