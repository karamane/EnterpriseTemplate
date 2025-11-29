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

        // Server Response -> Client Response mappings
        CreateMap<ServerCustomerResponse, CustomerClientResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<ServerCreateCustomerResponse, CreateCustomerClientResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "Müşteri başarıyla oluşturuldu"));

        CreateMap<ServerCustomerResponse, CustomerDetailClientResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => src.RegisteredAt.ToString("dd.MM.yyyy HH:mm")))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt.ToString("dd.MM.yyyy HH:mm")));

        // List response mapping
        CreateMap<ServerPagedCustomerResponse, CustomerListClientResponse>()
            .ForMember(dest => dest.Customers, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.HasMore, opt => opt.MapFrom(src => src.PageNumber < src.TotalPages));
    }
}
