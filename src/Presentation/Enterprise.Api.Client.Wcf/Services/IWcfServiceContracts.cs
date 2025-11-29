using System.ServiceModel;
using Enterprise.Api.Client.Wcf.DTOs;

namespace Enterprise.Api.Client.Wcf.Services;

/// <summary>
/// WCF Customer Service Contract
/// ServerApi'deki WCF endpoint'i ile iletişim için
/// </summary>
[ServiceContract(Namespace = "http://enterprise.com/services/customer")]
public interface ICustomerWcfService
{
    [OperationContract]
    Task<WcfServiceResponse<WcfCustomerDto>> GetCustomerAsync(int id);

    [OperationContract]
    Task<WcfServiceResponse<CustomerListResponse>> GetCustomersAsync(CustomerListRequest request);

    [OperationContract]
    Task<WcfServiceResponse<WcfCustomerDto>> CreateCustomerAsync(CreateCustomerRequest request);

    [OperationContract]
    Task<WcfServiceResponse<WcfCustomerDto>> UpdateCustomerAsync(UpdateCustomerRequest request);

    [OperationContract]
    Task<WcfServiceResponse<bool>> DeleteCustomerAsync(int id);
}

/// <summary>
/// WCF Order Service Contract
/// ServerApi'deki WCF endpoint'i ile iletişim için
/// </summary>
[ServiceContract(Namespace = "http://enterprise.com/services/order")]
public interface IOrderWcfService
{
    [OperationContract]
    Task<WcfServiceResponse<WcfOrderDto>> GetOrderAsync(int id);

    [OperationContract]
    Task<WcfServiceResponse<List<WcfOrderDto>>> GetCustomerOrdersAsync(int customerId);

    [OperationContract]
    Task<WcfServiceResponse<WcfOrderDto>> CreateOrderAsync(CreateOrderRequest request);

    [OperationContract]
    Task<WcfServiceResponse<bool>> CancelOrderAsync(int id);
}

