namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Server API ile iletişim için kullanılan contract DTO'ları
/// Client API tamamen izole - Server API referansı yok
/// Bu DTO'lar Server API'nin beklediği/döndürdüğü format ile uyumlu olmalı
/// </summary>

#region Request Contracts (Client -> Server)

/// <summary>
/// Server API'ye gönderilecek müşteri oluşturma isteği
/// </summary>
public record ServerCreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);

/// <summary>
/// Server API'ye gönderilecek müşteri güncelleme isteği
/// </summary>
public record ServerUpdateCustomerRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);

#endregion

#region Response Contracts (Server -> Client)

/// <summary>
/// Server API'den dönen müşteri yanıtı
/// </summary>
public record ServerCustomerResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime RegisteredAt,
    DateTime CreatedAt);

/// <summary>
/// Server API'den dönen müşteri oluşturma yanıtı
/// </summary>
public record ServerCreateCustomerResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt);

/// <summary>
/// Server API'den dönen sayfalanmış müşteri listesi
/// </summary>
public record ServerPagedCustomerResponse(
    IReadOnlyList<ServerCustomerResponse> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Server API generic response wrapper
/// </summary>
public record ServerApiResponse<T>(
    bool Success,
    string? Message,
    string? ErrorCode,
    T? Data,
    List<string>? Errors,
    string? CorrelationId,
    DateTime Timestamp);

#endregion

