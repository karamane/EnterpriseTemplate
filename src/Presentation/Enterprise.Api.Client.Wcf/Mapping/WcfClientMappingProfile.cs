using AutoMapper;
using Enterprise.Api.Client.Wcf.DTOs;

namespace Enterprise.Api.Client.Wcf.Mapping;

/// <summary>
/// WCF Client API mapping profili
/// WCF DTOları ile Client DTOları arasında dönüşüm
/// </summary>
public class WcfClientMappingProfile : Profile
{
    public WcfClientMappingProfile()
    {
        // WCF DTO -> Client Response
        CreateMap<WcfCustomerDto, WcfCustomerResponse>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"));

        CreateMap<CustomerListResponse, WcfCustomerListResponse>()
            .ForMember(d => d.Customers, opt => opt.MapFrom(s => s.Customers));

        CreateMap<WcfOrderDto, WcfOrderResponse>()
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.TotalAmount))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status));

        // Client Request -> WCF DTO
        CreateMap<WcfCustomerRequest, CreateCustomerRequest>();

        CreateMap<WcfCustomerRequest, UpdateCustomerRequest>();

        CreateMap<WcfOrderRequest, CreateOrderRequest>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items));

        CreateMap<WcfOrderItemRequest, CreateOrderItemRequest>();
    }
}

