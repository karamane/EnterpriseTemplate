namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Customer request DTO
/// </summary>
public record WcfCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone);

/// <summary>
/// WCF Client API - Customer response DTO
/// </summary>
public record WcfCustomerResponse(
    int Id,
    string FullName,
    string Email,
    string? Phone,
    DateTime CreatedAt);

/// <summary>
/// WCF Client API - Customer list response DTO
/// </summary>
public record WcfCustomerListResponse(
    IReadOnlyList<WcfCustomerResponse> Customers,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// WCF Client API - Order request DTO
/// </summary>
public record WcfOrderRequest(
    int CustomerId,
    IReadOnlyList<WcfOrderItemRequest> Items,
    string? Notes);

/// <summary>
/// WCF Client API - Order item request DTO
/// </summary>
public record WcfOrderItemRequest(
    int ProductId,
    int Quantity,
    decimal UnitPrice);

/// <summary>
/// WCF Client API - Order response DTO
/// </summary>
public record WcfOrderResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime OrderDate);

