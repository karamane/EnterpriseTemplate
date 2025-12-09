using AutoMapper;
using Enterprise.Api.Server.DTOs;
using Enterprise.Business.Features.Customers.Commands.CreateCustomer;
using Enterprise.Business.Features.Customers.Commands.UpdateCustomer;
using Enterprise.Business.Features.Customers.Queries.GetAllCustomers;
using Enterprise.Business.Features.Customers.Queries.GetCustomerById;
using Enterprise.Business.Features.Orders.Commands.CreateOrder;
using Enterprise.Business.Features.Orders.Queries.GetOrderById;
using Enterprise.Business.Features.Orders.Queries.GetOrdersByCustomer;
using Enterprise.Core.Application.DTOs;

namespace Enterprise.Api.Server.Mapping;

/// <summary>
/// Server API katmanı AutoMapper profili
/// API DTO - Application/Business DTO mapping
/// </summary>
public class ServerApiMappingProfile : Profile
{
    public ServerApiMappingProfile()
    {
        // API Request -> Application/Business Request mappings
        CreateMap<CreateCustomerApiRequest, CreateCustomerCommand>();
        CreateMap<CreateCustomerApiRequest, CreateCustomerAppRequest>();

        // Application/Business Response -> API Response mappings
        CreateMap<CreateCustomerResponse, CreateCustomerApiResponse>();
        CreateMap<CreateCustomerAppResponse, CreateCustomerApiResponse>();

        // Customer DTO mappings
        CreateMap<CustomerDto, CustomerApiResponse>();
        CreateMap<CustomerAppDto, CustomerApiResponse>();

        // Paged response mapping
        CreateMap<PagedCustomerAppResponse, PagedCustomerApiResponse>()
            .ForMember(dest => dest.TotalPages,
                opt => opt.MapFrom(src => (int)Math.Ceiling((double)src.TotalCount / src.PageSize)));

        // Update Customer mappings
        CreateMap<UpdateCustomerResponse, UpdateCustomerApiResponse>();

        // Customer list mappings
        CreateMap<GetAllCustomersResponse, CustomerListApiResponse>();
        CreateMap<CustomerListItemResponse, CustomerListItemApiResponse>();

        // Order mappings
        CreateMap<CreateOrderApiRequest, CreateOrderCommand>();
        CreateMap<CreateOrderItemApiRequest, CreateOrderItemCommand>();

        // Order response mappings
        CreateMap<CreateOrderResponse, OrderApiResponse>()
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore());
        CreateMap<CreateOrderItemResponse, OrderItemApiResponse>();

        // Order query response mappings
        CreateMap<OrderDetailResponse, OrderApiResponse>();
        CreateMap<OrderItemDetailResponse, OrderItemApiResponse>();
        CreateMap<CustomerOrderResponse, CustomerOrderApiResponse>();
    }
}

