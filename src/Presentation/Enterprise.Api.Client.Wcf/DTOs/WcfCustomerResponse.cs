namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Client API - Customer response DTO
/// </summary>
public class WcfCustomerResponse
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime RegisteredAt { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}
