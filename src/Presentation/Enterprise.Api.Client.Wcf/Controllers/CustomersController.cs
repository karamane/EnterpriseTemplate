using AutoMapper;
using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Wcf.Controllers;

/// <summary>
/// WCF Client - Customer API Controller
/// WCF servisleri üzerinden müşteri işlemleri
/// </summary>
public class CustomersController : BaseWcfApiController
{
    private readonly ICustomerWcfClient _customerClient;
    private readonly IMapper _mapper;

    public CustomersController(
        ICustomerWcfClient customerClient,
        IMapper mapper,
        ICorrelationContext correlationContext)
        : base(correlationContext)
    {
        _customerClient = customerClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Müşteri listesini getirir (WCF üzerinden)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerListResponse>), 200)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerListResponse>>> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _customerClient.GetCustomersAsync(page, pageSize, search);

        var response = _mapper.Map<WcfCustomerListResponse>(result);

        return Success(response);
    }

    /// <summary>
    /// Müşteri detayını getirir (WCF üzerinden)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerResponse>>> GetCustomer(int id)
    {
        var customer = await _customerClient.GetCustomerAsync(id);

        if (customer == null)
        {
            return NotFound(new WcfApiResponse<WcfCustomerResponse>
            {
                Success = false,
                Message = "Müşteri bulunamadı",
                CorrelationId = CorrelationContext.CorrelationId
            });
        }

        var response = _mapper.Map<WcfCustomerResponse>(customer);

        return Success(response);
    }

    /// <summary>
    /// Yeni müşteri oluşturur (WCF üzerinden)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerResponse>>> CreateCustomer(
        [FromBody] WcfCustomerRequest request)
    {
        var wcfRequest = _mapper.Map<CreateCustomerRequest>(request);

        var customer = await _customerClient.CreateCustomerAsync(wcfRequest);

        var response = _mapper.Map<WcfCustomerResponse>(customer);

        return Created(response);
    }

    /// <summary>
    /// Müşteri günceller (WCF üzerinden)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerResponse>>> UpdateCustomer(
        int id,
        [FromBody] WcfCustomerRequest request)
    {
        var wcfRequest = _mapper.Map<UpdateCustomerRequest>(request);
        wcfRequest.Id = id;

        var customer = await _customerClient.UpdateCustomerAsync(wcfRequest);

        var response = _mapper.Map<WcfCustomerResponse>(customer);

        return Success(response, "Müşteri güncellendi");
    }

    /// <summary>
    /// Müşteri siler (WCF üzerinden)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(WcfApiResponse<object>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<object>>> DeleteCustomer(int id)
    {
        await _customerClient.DeleteCustomerAsync(id);

        return Deleted();
    }
}

