namespace Enterprise.Api.Client.DTOs;

// Server API ile iletişim için kullanılan contract DTO'ları
// Client API tamamen izole - Server API referansı yok
// Bu DTO'lar Server API'nin beklediği/döndürdüğü format ile uyumlu olmalı

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
/// Server API'den dönen müşteri güncelleme yanıtı
/// </summary>
public record ServerUpdateCustomerResponse(
    long Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime UpdatedAt);

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

#region Order Contracts

/// <summary>
/// Server API'ye gönderilecek sipariş oluşturma isteği
/// </summary>
public record ServerCreateOrderRequest(
    int CustomerId,
    List<ServerOrderItemRequest> Items,
    string? Notes);

/// <summary>
/// Server API'ye gönderilecek sipariş item'ı
/// </summary>
public record ServerOrderItemRequest(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// Server API'den dönen sipariş yanıtı
/// </summary>
public record ServerOrderResponse(
    long Id,
    long CustomerId,
    string? CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate,
    List<ServerOrderItemResponse>? Items);

/// <summary>
/// Server API'den dönen sipariş item yanıtı
/// </summary>
public record ServerOrderItemResponse(
    long ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

/// <summary>
/// Server API'den dönen sipariş listesi
/// </summary>
public record ServerOrderListResponse(
    List<ServerOrderResponse> Items,
    int TotalCount);

#endregion

