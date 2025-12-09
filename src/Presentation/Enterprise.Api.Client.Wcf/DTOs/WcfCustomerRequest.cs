namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Customer request DTO
/// </summary>
public class WcfCustomerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}
