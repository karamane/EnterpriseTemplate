using AutoMapper;
using Enterprise.Api.Server.DTOs;
using Enterprise.Business.Features.Customers.Commands.CreateCustomer;
using Enterprise.Business.Features.Customers.Commands.DeleteCustomer;
using Enterprise.Business.Features.Customers.Commands.UpdateCustomer;
using Enterprise.Business.Features.Customers.Queries.GetAllCustomers;
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
    /// Müşteri listesini getirir (sayfalama destekli)
    /// </summary>
    /// <param name="pageNumber">Sayfa numarası (default: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (default: 10)</param>
    /// <param name="search">Arama terimi (ad, soyad, email)</param>
    /// <returns>Müşteri listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CustomerListApiResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = new GetAllCustomersQuery(pageNumber, pageSize, search);
        var result = await Mediator.Send(query);

        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        var apiResponse = _mapper.Map<CustomerListApiResponse>(result.Data);
        return Ok(new ApiResponse<CustomerListApiResponse>
        {
            Success = true,
            Data = apiResponse,
            CorrelationId = CorrelationContext.CorrelationId
        });
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

    /// <summary>
    /// Müşteri günceller
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <param name="request">Güncellenecek müşteri bilgileri</param>
    /// <returns>Güncellenen müşteri</returns>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<UpdateCustomerApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateCustomerApiRequest request)
    {
        var command = new UpdateCustomerCommand(id, request.FirstName, request.LastName, request.PhoneNumber);
        var result = await Mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            var apiResponse = _mapper.Map<UpdateCustomerApiResponse>(result.Data);
            return Ok(new ApiResponse<UpdateCustomerApiResponse>
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
    /// Müşteri siler (Soft delete)
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <returns>Silme sonucu</returns>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await Mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = result.Message,
                Data = true,
                CorrelationId = CorrelationContext.CorrelationId
            });
        }

        return HandleResult(result);
    }
}
