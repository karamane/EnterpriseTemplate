using AutoMapper;
using Enterprise.Business.DTOs;
using Enterprise.Business.Features.Customers.Commands.CreateCustomer;
using Enterprise.Business.Features.Customers.Queries.GetCustomerById;
using Enterprise.Core.Application.DTOs;
using Enterprise.Core.Domain.Entities.Sample;

namespace Enterprise.Business.Mapping;

/// <summary>
/// Business katmanı AutoMapper profili
/// Application DTO <-> Business DTO <-> Domain Entity mapping
/// </summary>
public class BusinessMappingProfile : Profile
{
    public BusinessMappingProfile()
    {
        // Application -> Business mappings
        CreateMap<CreateCustomerAppRequest, CreateCustomerBusinessRequest>();
        CreateMap<CreateCustomerAppRequest, CreateCustomerCommand>();

        // Business -> Application mappings
        CreateMap<CreateCustomerBusinessResponse, CreateCustomerAppResponse>();
        CreateMap<CustomerBusinessDto, CustomerAppDto>();
        CreateMap<CreateCustomerResponse, CreateCustomerAppResponse>();
        CreateMap<CustomerDto, CustomerAppDto>();

        // Domain -> Business mappings
        CreateMap<Customer, CustomerBusinessDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        CreateMap<Customer, CreateCustomerBusinessResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

        // Business -> Domain mappings
        CreateMap<CreateCustomerBusinessRequest, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.RegisteredAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.DomainEvents, opt => opt.Ignore());
    }
}

