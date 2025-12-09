using CoreWCF;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Enterprise.Api.Client.Wcf.Services.Contracts;

/// <summary>
/// WCF Authentication Service Contract
/// SOAP endpoint for legacy .NET Framework clients
/// </summary>
[ServiceContract(Namespace = "http://enterprise.com/services/auth")]
public interface IWcfAuthService
{
    /// <summary>
    /// Kullanıcı girişi yapar ve token döner
    /// </summary>
    [OperationContract]
    Task<WcfLoginResponse> LoginAsync(WcfLoginRequest request);

    /// <summary>
    /// Token yeniler
    /// </summary>
    [OperationContract]
    Task<WcfLoginResponse> RefreshTokenAsync(WcfRefreshTokenRequest request);

    /// <summary>
    /// Çıkış yapar
    /// </summary>
    [OperationContract]
    Task<WcfLogoutResponse> LogoutAsync(WcfLogoutRequest request);

    /// <summary>
    /// Token doğrular
    /// </summary>
    [OperationContract]
    Task<WcfValidateTokenResponse> ValidateTokenAsync(WcfValidateTokenRequest request);
}

#region Data Contracts

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfLoginRequest
{
    [DataMember]
    public string Username { get; set; } = string.Empty;

    [DataMember]
    public string Password { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfLoginResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }

    [DataMember]
    public string? AccessToken { get; set; }

    [DataMember]
    public string? RefreshToken { get; set; }

    [DataMember]
    public DateTime? ExpiresAt { get; set; }

    [DataMember]
    public string? Username { get; set; }

    [DataMember]
    public string[]? Roles { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfRefreshTokenRequest
{
    [DataMember]
    public string RefreshToken { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfLogoutRequest
{
    [DataMember]
    public string AccessToken { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfLogoutResponse
{
    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? Message { get; set; }
}

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfValidateTokenRequest
{
    [DataMember]
    public string AccessToken { get; set; } = string.Empty;
}

[DataContract(Namespace = "http://enterprise.com/services/auth")]
public class WcfValidateTokenResponse
{
    [DataMember]
    public bool IsValid { get; set; }

    [DataMember]
    public string? Username { get; set; }

    [DataMember]
    public string[]? Roles { get; set; }
}

#endregion


