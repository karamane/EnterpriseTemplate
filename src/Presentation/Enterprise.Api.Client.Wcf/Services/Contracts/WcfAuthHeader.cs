using System.Runtime.Serialization;

namespace Enterprise.Api.Client.Wcf.Services.Contracts;

/// <summary>
/// SOAP header for authentication credentials
/// WCF client'lar bu header'ı göndererek kimlik doğrulama yapar
/// </summary>
[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfAuthHeader
{
    /// <summary>
    /// Bearer token (login sonrası alınan access token)
    /// </summary>
    [DataMember(Order = 1)]
    public string? AccessToken { get; set; }

    /// <summary>
    /// Username (login sırasında kullanılır)
    /// </summary>
    [DataMember(Order = 2)]
    public string? Username { get; set; }

    /// <summary>
    /// Password (login sırasında kullanılır)
    /// </summary>
    [DataMember(Order = 3)]
    public string? Password { get; set; }
}

/// <summary>
/// SOAP mesajlarından auth header okumak için yardımcı sınıf
/// </summary>
public static class WcfAuthHeaderExtractor
{
    /// <summary>
    /// HttpContext'ten Bearer token'ı çıkarır
    /// </summary>
    public static string? ExtractBearerToken(Microsoft.AspNetCore.Http.HttpContext? httpContext)
    {
        if (httpContext == null)
            return null;

        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
            return null;

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authHeader.Substring(7);
        }

        return null;
    }
}


