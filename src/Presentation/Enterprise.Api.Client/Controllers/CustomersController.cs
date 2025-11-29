using AutoMapper;
using Enterprise.Api.Client.DTOs;
using Enterprise.Api.Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Controllers;

/// <summary>
/// Client API - Müşteri endpoint'leri
/// Mobil uygulamalar için optimize edilmiş
/// Client API tamamen izole - hiçbir internal referans yok
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly IServerApiClient _serverApiClient;
    private readonly IMapper _mapper;

    public CustomersController(
        IServerApiClient serverApiClient,
        IMapper mapper)
    {
        _serverApiClient = serverApiClient;
        _mapper = mapper;
    }

    /// <summary>
    /// Müşteri bilgisi getirir (Mobil için optimize edilmiş)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDetailClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Server API'ye istek gönder
        var serverResponse = await _serverApiClient.GetAsync<ServerApiResponse<ServerCustomerResponse>>(
            $"/api/v1/customers/{id}");

        if (serverResponse?.Data == null)
        {
            return NotFound(new { Message = "Müşteri bulunamadı" });
        }

        // Server Response -> Client Response mapping
        var clientResponse = _mapper.Map<CustomerDetailClientResponse>(serverResponse.Data);
        return Ok(clientResponse);
    }

    /// <summary>
    /// Yeni müşteri oluşturur (Mobil'den gelen istek)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCustomerClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerClientRequest request)
    {
        // Client Request -> Server Request mapping
        var serverRequest = _mapper.Map<ServerCreateCustomerRequest>(request);

        // Server API'ye istek gönder
        var serverResponse = await _serverApiClient.PostAsync<ServerCreateCustomerRequest, ServerApiResponse<ServerCreateCustomerResponse>>(
            "/api/v1/customers",
            serverRequest);

        if (serverResponse?.Data == null)
        {
            return BadRequest(new { Message = serverResponse?.Message ?? "İşlem başarısız" });
        }

        // Server Response -> Client Response mapping
        var clientResponse = _mapper.Map<CreateCustomerClientResponse>(serverResponse.Data);
        return Created($"/api/v1/customers/{clientResponse.Id}", clientResponse);
    }

    /// <summary>
    /// Müşteri listesi getirir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerListClientResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var serverResponse = await _serverApiClient.GetAsync<ServerApiResponse<ServerPagedCustomerResponse>>(
            $"/api/v1/customers?pageNumber={page}&pageSize={pageSize}");

        if (serverResponse?.Data == null)
        {
            return Ok(new CustomerListClientResponse(new List<CustomerClientResponse>(), 0, false));
        }

        var clientResponse = _mapper.Map<CustomerListClientResponse>(serverResponse.Data);
        return Ok(clientResponse);
    }
}
