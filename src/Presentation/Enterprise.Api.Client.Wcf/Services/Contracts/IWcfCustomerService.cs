using CoreWCF;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Enterprise.Api.Client.Wcf.Services.Contracts;

/// <summary>
/// WCF Customer Service Contract
/// SOAP endpoint for legacy .NET Framework clients
/// </summary>
[ServiceContract(Namespace = "http://enterprise.com/services/customer")]
public interface IWcfCustomerService
{
    /// <summary>
    /// Müşteri bilgilerini getirir
    /// </summary>
    [OperationContract]
    Task<WcfCustomerResponse> GetCustomerAsync(long id);

    /// <summary>
    /// Tüm müşterileri listeler
    /// </summary>
    [OperationContract]
    Task<WcfCustomerListResponse> GetAllCustomersAsync();

    /// <summary>
    /// Yeni müşteri oluşturur
    /// </summary>
    [OperationContract]
    Task<WcfCreateCustomerResponse> CreateCustomerAsync(WcfCreateCustomerRequest request);

    /// <summary>
    /// Müşteri bilgilerini günceller
    /// </summary>
    [OperationContract]
    Task<WcfCustomerResponse> UpdateCustomerAsync(WcfUpdateCustomerRequest request);

    /// <summary>
    /// Müşteri siler
    /// </summary>
    [OperationContract]
    Task<WcfDeleteResponse> DeleteCustomerAsync(long id);
}

#region Data Contracts

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfCustomerResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }

    [DataMember]
    public WcfCustomerData? Customer { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfCustomerData
{
    [DataMember]
    public long Id { get; set; }

    [DataMember]
    public string FirstName { get; set; } = string.Empty;

    [DataMember]
    public string LastName { get; set; } = string.Empty;

    [DataMember]
    public string Email { get; set; } = string.Empty;

    [DataMember]
    public string? PhoneNumber { get; set; }

    [DataMember]
    public bool IsActive { get; set; }

    [DataMember]
    public DateTime RegisteredAt { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfCustomerListResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }

    [DataMember]
    public List<WcfCustomerData> Customers { get; set; } = new();

    [DataMember]
    public int TotalCount { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfCreateCustomerRequest
{
    [DataMember]
    public string FirstName { get; set; } = string.Empty;

    [DataMember]
    public string LastName { get; set; } = string.Empty;

    [DataMember]
    public string Email { get; set; } = string.Empty;

    [DataMember]
    public string? PhoneNumber { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfCreateCustomerResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }

    [DataMember]
    public long CustomerId { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfUpdateCustomerRequest
{
    [DataMember]
    public long Id { get; set; }

    [DataMember]
    public string FirstName { get; set; } = string.Empty;

    [DataMember]
    public string LastName { get; set; } = string.Empty;

    [DataMember]
    public string? PhoneNumber { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfDeleteResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }
}

#endregion


