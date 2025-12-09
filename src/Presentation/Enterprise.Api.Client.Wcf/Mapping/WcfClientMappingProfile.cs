using AutoMapper;
using Enterprise.Api.Client.Wcf.DTOs;

namespace Enterprise.Api.Client.Wcf.Mapping;

/// <summary>
/// WCF Client API mapping profili
/// Controller'larda inline mapping kullanıldığı için minimal profil
/// </summary>
public class WcfClientMappingProfile : Profile
{
    public WcfClientMappingProfile()
    {
        // WCF Client API artık doğrudan Server API tüketiyor
        // Mapping işlemleri controller'larda inline yapılıyor
        // Bu profil FluentValidation assembly scanning için korunuyor
    }
}
