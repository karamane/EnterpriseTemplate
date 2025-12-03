using AutoMapper;
using Enterprise.Api.Client.DTOs;

namespace Enterprise.Api.Client.Mapping;

/// <summary>
/// Client API katmanı AutoMapper profili
/// Client DTO <-> Server Contract DTO mapping
/// Client API tamamen izole - hiçbir internal referans yok
/// </summary>
public class ClientApiMappingProfile : Profile
{
    public ClientApiMappingProfile()
    {
        // Client Request -> Server Request mappings
        CreateMap<CreateCustomerClientRequest, ServerCreateCustomerRequest>();

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
    }
}
