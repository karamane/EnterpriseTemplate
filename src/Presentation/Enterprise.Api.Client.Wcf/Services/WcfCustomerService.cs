using System.Net.Http.Json;
using System.Text.Json;
using Enterprise.Api.Client.Wcf.Services.Contracts;
using Enterprise.Core.Application.Interfaces.Logging;
using Microsoft.AspNetCore.Http;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// WCF Customer Service Implementation
/// Server API'ye proxy yapar
/// </summary>
public class WcfCustomerService : IWcfCustomerService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICorrelationContext _correlationContext;
    private readonly ILogger<WcfCustomerService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WcfCustomerService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ICorrelationContext correlationContext,
        ILogger<WcfCustomerService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _correlationContext = correlationContext;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<WcfCustomerResponse> GetCustomerAsync(long id)
    {
        try
        {
            PrepareRequest();
            var response = await _httpClient.GetAsync($"api/v1/customers/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[{CorrelationId}] GetCustomer failed for ID {Id}: {StatusCode}",
                    _correlationContext.CorrelationId, id, response.StatusCode);

                return new WcfCustomerResponse
                {
                    Success = false,
                    ErrorMessage = $"Müşteri bulunamadı (ID: {id})"
                };
            }

            var serverResponse = await response.Content.ReadFromJsonAsync<ServerApiResponse<ServerCustomerData>>(_jsonOptions);

            if (serverResponse?.Success != true || serverResponse.Data == null)
            {
                return new WcfCustomerResponse
                {
                    Success = false,
                    ErrorMessage = serverResponse?.Message ?? "Müşteri bulunamadı"
                };
            }

            return new WcfCustomerResponse
            {
                Success = true,
                Customer = MapToWcfCustomer(serverResponse.Data)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] GetCustomer error for ID {Id}",
                _correlationContext.CorrelationId, id);

            return new WcfCustomerResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    public async Task<WcfCustomerListResponse> GetAllCustomersAsync()
    {
        try
        {
            PrepareRequest();
            var response = await _httpClient.GetAsync("api/v1/customers");

            if (!response.IsSuccessStatusCode)
            {
                return new WcfCustomerListResponse
                {
                    Success = false,
                    ErrorMessage = "Müşteri listesi alınamadı"
                };
            }

            var serverResponse = await response.Content.ReadFromJsonAsync<ServerApiResponse<ServerCustomerListData>>(_jsonOptions);

            if (serverResponse?.Success != true)
            {
                return new WcfCustomerListResponse
                {
                    Success = false,
                    ErrorMessage = serverResponse?.Message ?? "Müşteri listesi alınamadı"
                };
            }

            return new WcfCustomerListResponse
            {
                Success = true,
                Customers = serverResponse.Data?.Items?.Select(MapToWcfCustomer).ToList() ?? new List<WcfCustomerData>(),
                TotalCount = serverResponse.Data?.TotalCount ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] GetAllCustomers error", _correlationContext.CorrelationId);

            return new WcfCustomerListResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    public async Task<WcfCreateCustomerResponse> CreateCustomerAsync(WcfCreateCustomerRequest request)
    {
        try
        {
            PrepareRequest();
            var serverRequest = new
            {
                firstName = request.FirstName,
                lastName = request.LastName,
                email = request.Email,
                phoneNumber = request.PhoneNumber
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/customers", serverRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("[{CorrelationId}] CreateCustomer failed: {StatusCode} - {Error}",
                    _correlationContext.CorrelationId, response.StatusCode, errorContent);

                return new WcfCreateCustomerResponse
                {
                    Success = false,
                    ErrorMessage = "Müşteri oluşturulamadı"
                };
            }

            var serverResponse = await response.Content.ReadFromJsonAsync<ServerApiResponse<ServerCreateCustomerData>>(_jsonOptions);

            return new WcfCreateCustomerResponse
            {
                Success = serverResponse?.Success ?? false,
                CustomerId = serverResponse?.Data?.Id ?? 0,
                ErrorMessage = serverResponse?.Success != true ? serverResponse?.Message : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] CreateCustomer error", _correlationContext.CorrelationId);

            return new WcfCreateCustomerResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    public async Task<WcfCustomerResponse> UpdateCustomerAsync(WcfUpdateCustomerRequest request)
    {
        try
        {
            PrepareRequest();
            var serverRequest = new
            {
                firstName = request.FirstName,
                lastName = request.LastName,
                phoneNumber = request.PhoneNumber
            };

            var response = await _httpClient.PutAsJsonAsync($"api/v1/customers/{request.Id}", serverRequest, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return new WcfCustomerResponse
                {
                    Success = false,
                    ErrorMessage = "Müşteri güncellenemedi"
                };
            }

            // Güncellenmiş müşteriyi getir
            return await GetCustomerAsync(request.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] UpdateCustomer error for ID {Id}",
                _correlationContext.CorrelationId, request.Id);

            return new WcfCustomerResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    public async Task<WcfDeleteResponse> DeleteCustomerAsync(long id)
    {
        try
        {
            PrepareRequest();
            var response = await _httpClient.DeleteAsync($"api/v1/customers/{id}");

            return new WcfDeleteResponse
            {
                Success = response.IsSuccessStatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? null : "Müşteri silinemedi"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{CorrelationId}] DeleteCustomer error for ID {Id}",
                _correlationContext.CorrelationId, id);

            return new WcfDeleteResponse
            {
                Success = false,
                ErrorMessage = "Beklenmeyen bir hata oluştu"
            };
        }
    }

    private void PrepareRequest()
    {
        // Correlation ID propagation
        if (!string.IsNullOrEmpty(_correlationContext.CorrelationId))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", _correlationContext.CorrelationId);
        }

        // Authorization header propagation (SOAP header'dan gelen token)
        var token = WcfAuthHeaderExtractor.ExtractBearerToken(_httpContextAccessor.HttpContext);
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }

    private static WcfCustomerData MapToWcfCustomer(ServerCustomerData data)
    {
        return new WcfCustomerData
        {
            Id = data.Id,
            FirstName = data.FirstName,
            LastName = data.LastName,
            Email = data.Email,
            PhoneNumber = data.PhoneNumber,
            IsActive = data.IsActive,
            RegisteredAt = data.RegisteredAt
        };
    }
}

#region Server API Response Types

internal class ServerApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

internal class ServerCustomerData
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
}

internal class ServerCustomerListData
{
    public List<ServerCustomerData>? Items { get; set; }
    public int TotalCount { get; set; }
}

internal class ServerCreateCustomerData
{
    public long Id { get; set; }
}

#endregion

