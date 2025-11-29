namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Client API katmanı Customer DTO'ları
/// Mobil uygulamalar için optimize edilmiş modeller
/// </summary>

/// <summary>
/// Customer Client Response - Mobil için optimize edilmiş
/// </summary>
public record CustomerClientResponse(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive);

/// <summary>
/// Create Customer Client Request - Mobil'den gelen istek
/// </summary>
public record CreateCustomerClientRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

/// <summary>
/// Create Customer Client Response - Mobil'e dönen yanıt
/// </summary>
public record CreateCustomerClientResponse(
    string Id,
    string FullName,
    string Message);

/// <summary>
/// Customer List Client Response - Mobil liste görünümü için
/// </summary>
public record CustomerListClientResponse(
    IReadOnlyList<CustomerClientResponse> Customers,
    int TotalCount,
    bool HasMore);

/// <summary>
/// Customer Detail Client Response - Mobil detay görünümü için
/// </summary>
public record CustomerDetailClientResponse(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    string RegisteredAt,
    string CreatedAt);

