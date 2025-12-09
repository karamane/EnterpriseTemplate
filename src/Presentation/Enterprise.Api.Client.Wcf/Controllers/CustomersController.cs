using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Client.Wcf.Controllers;

/// <summary>
/// WCF Client - Customer API Controller
/// Server API üzerinden müşteri işlemleri (DMZ kuralı: Sadece Server API tüketilir)
/// </summary>
[Authorize]
public class CustomersController : BaseWcfApiController
{
    private readonly IWcfServerApiClient _serverApiClient;

    public CustomersController(
        IWcfServerApiClient serverApiClient,
        ICorrelationContext correlationContext)
        : base(correlationContext)
    {
        _serverApiClient = serverApiClient;
    }

    /// <summary>
    /// Müşteri listesini getirir
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerListResponse>), 200)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerListResponse>>> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var endpoint = $"api/v1/customers?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search))
        {
            endpoint += $"&search={Uri.EscapeDataString(search)}";
        }

        var result = await _serverApiClient.GetAsync<ServerApiResponse<CustomerListData>>(endpoint);

        if (result?.Success != true)
        {
            return BadRequest(Error<WcfCustomerListResponse>(result?.Message ?? "İstek başarısız"));
        }

        var response = new WcfCustomerListResponse
        {
            Items = result.Data?.Items?.Select(c => new WcfCustomerResponse
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive,
                RegisteredAt = c.RegisteredAt
            }).ToList() ?? new List<WcfCustomerResponse>(),
            TotalCount = result.Data?.TotalCount ?? 0,
            Page = page,
            PageSize = pageSize
        };

        return Success(response);
    }

    /// <summary>
    /// Müşteri detayını getirir
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerResponse>>> GetCustomer(long id)
    {
        var result = await _serverApiClient.GetAsync<ServerApiResponse<CustomerData>>($"api/v1/customers/{id}");

        if (result?.Success != true || result.Data == null)
        {
            return NotFound(Error<WcfCustomerResponse>("Müşteri bulunamadı"));
        }

        var response = new WcfCustomerResponse
        {
            Id = result.Data.Id,
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            Email = result.Data.Email,
            PhoneNumber = result.Data.PhoneNumber,
            IsActive = result.Data.IsActive,
            RegisteredAt = result.Data.RegisteredAt
        };

        return Success(response);
    }

    /// <summary>
    /// Yeni müşteri oluşturur
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerResponse>>> CreateCustomer(
        [FromBody] WcfCustomerRequest request)
    {
        var serverRequest = new
        {
            firstName = request.FirstName,
            lastName = request.LastName,
            email = request.Email,
            phoneNumber = request.PhoneNumber
        };

        var result = await _serverApiClient.PostAsync<object, ServerApiResponse<CustomerData>>(
            "api/v1/customers", serverRequest);

        if (result?.Success != true || result.Data == null)
        {
            return BadRequest(Error<WcfCustomerResponse>(result?.Message ?? "Müşteri oluşturulamadı"));
        }

        var response = new WcfCustomerResponse
        {
            Id = result.Data.Id,
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            Email = result.Data.Email,
            PhoneNumber = result.Data.PhoneNumber,
            IsActive = result.Data.IsActive,
            RegisteredAt = result.Data.RegisteredAt
        };

        return Created(response);
    }

    /// <summary>
    /// Müşteri günceller
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(WcfApiResponse<WcfCustomerResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<WcfCustomerResponse>>> UpdateCustomer(
        long id,
        [FromBody] WcfCustomerRequest request)
    {
        var serverRequest = new
        {
            firstName = request.FirstName,
            lastName = request.LastName,
            phoneNumber = request.PhoneNumber
        };

        var result = await _serverApiClient.PutAsync<object, ServerApiResponse<CustomerData>>(
            $"api/v1/customers/{id}", serverRequest);

        if (result?.Success != true || result.Data == null)
        {
            return BadRequest(Error<WcfCustomerResponse>(result?.Message ?? "Müşteri güncellenemedi"));
        }

        var response = new WcfCustomerResponse
        {
            Id = result.Data.Id,
            FirstName = result.Data.FirstName,
            LastName = result.Data.LastName,
            Email = result.Data.Email,
            PhoneNumber = result.Data.PhoneNumber,
            IsActive = result.Data.IsActive,
            RegisteredAt = result.Data.RegisteredAt
        };

        return Success(response, "Müşteri güncellendi");
    }

    /// <summary>
    /// Müşteri siler
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(WcfApiResponse<object>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<WcfApiResponse<object>>> DeleteCustomer(long id)
    {
        var result = await _serverApiClient.DeleteAsync($"api/v1/customers/{id}");

        if (!result)
        {
            return NotFound(Error<object>("Müşteri bulunamadı veya silinemedi"));
        }

        return Deleted();
    }
}

#region Server API Response Types

internal class ServerApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

internal class CustomerData
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}

internal class CustomerListData
{
    public List<CustomerData>? Items { get; set; }
    public int TotalCount { get; set; }
}

#endregion
