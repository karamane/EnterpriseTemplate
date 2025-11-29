using System.Runtime.Serialization;
using System.ServiceModel;

namespace Enterprise.Api.Client.Wcf.DTOs;

/// <summary>
/// WCF Service ile iletişim için kullanılan contract DTOları
/// ServerApi WCF endpoint'leri için
/// </summary>

#region Customer Service Contracts

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class WcfCustomerDto
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string FirstName { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string LastName { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public string? Phone { get; set; }

    [DataMember(Order = 6)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 7)]
    public DateTime? UpdatedAt { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class CreateCustomerRequest
{
    [DataMember(Order = 1)]
    public string FirstName { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string LastName { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string? Phone { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class UpdateCustomerRequest
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string FirstName { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string LastName { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public string? Phone { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class CustomerListRequest
{
    [DataMember(Order = 1)]
    public int Page { get; set; } = 1;

    [DataMember(Order = 2)]
    public int PageSize { get; set; } = 10;

    [DataMember(Order = 3)]
    public string? SearchTerm { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/customer")]
public class CustomerListResponse
{
    [DataMember(Order = 1)]
    public List<WcfCustomerDto> Customers { get; set; } = new();

    [DataMember(Order = 2)]
    public int TotalCount { get; set; }

    [DataMember(Order = 3)]
    public int Page { get; set; }

    [DataMember(Order = 4)]
    public int PageSize { get; set; }
}

#endregion

#region Order Service Contracts

[DataContract(Namespace = "http://enterprise.com/services/order")]
public class WcfOrderDto
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public int CustomerId { get; set; }

    [DataMember(Order = 3)]
    public string CustomerName { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public decimal TotalAmount { get; set; }

    [DataMember(Order = 5)]
    public string Status { get; set; } = string.Empty;

    [DataMember(Order = 6)]
    public DateTime OrderDate { get; set; }

    [DataMember(Order = 7)]
    public List<WcfOrderItemDto> Items { get; set; } = new();
}

[DataContract(Namespace = "http://enterprise.com/services/order")]
public class WcfOrderItemDto
{
    [DataMember(Order = 1)]
    public int ProductId { get; set; }

    [DataMember(Order = 2)]
    public string ProductName { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public int Quantity { get; set; }

    [DataMember(Order = 4)]
    public decimal UnitPrice { get; set; }

    [DataMember(Order = 5)]
    public decimal TotalPrice { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/order")]
public class CreateOrderRequest
{
    [DataMember(Order = 1)]
    public int CustomerId { get; set; }

    [DataMember(Order = 2)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();

    [DataMember(Order = 3)]
    public string? Notes { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/order")]
public class CreateOrderItemRequest
{
    [DataMember(Order = 1)]
    public int ProductId { get; set; }

    [DataMember(Order = 2)]
    public int Quantity { get; set; }

    [DataMember(Order = 3)]
    public decimal UnitPrice { get; set; }
}

#endregion

#region Service Response Wrapper

[DataContract(Namespace = "http://enterprise.com/services/common")]
public class WcfServiceResponse<T>
{
    [DataMember(Order = 1)]
    public bool Success { get; set; }

    [DataMember(Order = 2)]
    public string? Message { get; set; }

    [DataMember(Order = 3)]
    public T? Data { get; set; }

    [DataMember(Order = 4)]
    public string? ErrorCode { get; set; }

    [DataMember(Order = 5)]
    public string? CorrelationId { get; set; }
}

#endregion

