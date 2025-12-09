using AutoMapper;
using Enterprise.Api.Client.DTOs;

namespace Enterprise.Api.Client.Mapping;

/// <summary>
/// Client API katmanı AutoMapper profili
/// Client DTO - Server Contract DTO mapping
/// Client API tamamen izole - hiçbir internal referans yok
/// </summary>
public class ClientApiMappingProfile : Profile
{
    public ClientApiMappingProfile()
    {
        // ========== Customer Mappings ==========

        // Client Request -> Server Request mappings
        CreateMap<CreateCustomerClientRequest, ServerCreateCustomerRequest>();
        CreateMap<UpdateCustomerClientRequest, ServerUpdateCustomerRequest>();

        // Server Response -> Client Response mappings (ConstructUsing for records)
        CreateMap<ServerCustomerResponse, CustomerClientResponse>()
            .ConstructUsing(src => new CustomerClientResponse(
                src.Id.ToString(),
                $"{src.FirstName} {src.LastName}",
                src.Email,
                src.PhoneNumber,
                src.IsActive));

        CreateMap<ServerCreateCustomerResponse, CreateCustomerClientResponse>()
            .ConstructUsing(src => new CreateCustomerClientResponse(
                src.Id.ToString(),
                $"{src.FirstName} {src.LastName}",
                "Müşteri başarıyla oluşturuldu"));

        CreateMap<ServerUpdateCustomerResponse, UpdateCustomerClientResponse>()
            .ConstructUsing(src => new UpdateCustomerClientResponse(
                src.Id.ToString(),
                $"{src.FirstName} {src.LastName}",
                src.Email,
                src.PhoneNumber,
                src.IsActive,
                src.UpdatedAt));

        CreateMap<ServerCustomerResponse, CustomerDetailClientResponse>()
            .ConstructUsing(src => new CustomerDetailClientResponse(
                src.Id.ToString(),
                src.FirstName,
                src.LastName,
                $"{src.FirstName} {src.LastName}",
                src.Email,
                src.PhoneNumber,
                src.IsActive,
                src.RegisteredAt.ToString("dd.MM.yyyy HH:mm"),
                src.CreatedAt.ToString("dd.MM.yyyy HH:mm")));

        // List response mapping
        CreateMap<ServerPagedCustomerResponse, CustomerListClientResponse>()
            .ForMember(dest => dest.Customers, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.HasMore, opt => opt.MapFrom(src => src.PageNumber < src.TotalPages));

        // ========== Order Mappings ==========

        // Client Request -> Server Request mappings
        CreateMap<CreateOrderClientRequest, ServerCreateOrderRequest>();
        CreateMap<OrderItemClientRequest, ServerOrderItemRequest>();

        // Server Response -> Client Response mappings
        CreateMap<ServerOrderResponse, OrderClientResponse>()
            .ConstructUsing(src => new OrderClientResponse(
                src.Id.ToString(),
                src.CustomerId.ToString(),
                src.CustomerName,
                src.TotalAmount,
                src.Status,
                src.OrderDate,
                src.Items != null
                    ? src.Items.Select(i => new OrderItemClientResponse(
                        i.ProductId.ToString(),
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        i.TotalPrice)).ToList()
                    : new List<OrderItemClientResponse>()));

        CreateMap<ServerOrderListResponse, OrderListClientResponse>()
            .ConstructUsing((src, ctx) => new OrderListClientResponse(
                src.Items.Select(o => ctx.Mapper.Map<OrderClientResponse>(o)).ToList(),
                src.TotalCount));
    }
}
