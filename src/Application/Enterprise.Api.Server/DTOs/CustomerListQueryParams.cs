namespace Enterprise.Api.Server.DTOs;

/// <summary>
/// Customer List Query Parameters
/// </summary>
public record CustomerListQueryParams(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null);


