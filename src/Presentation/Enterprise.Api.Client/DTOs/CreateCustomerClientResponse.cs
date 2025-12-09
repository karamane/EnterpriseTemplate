namespace Enterprise.Api.Client.DTOs;

/// <summary>
/// Create Customer Client Response - Mobil'e dönen yanıt
/// </summary>
public record CreateCustomerClientResponse(
    string Id,
    string FullName,
    string Message);

