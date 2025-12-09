using System.Net.Http.Headers;
using System.Text;
using Enterprise.Api.Client.Wcf.DTOs;
using Enterprise.Api.Client.Wcf.Services;

namespace Enterprise.Api.Client.Wcf.Middleware;

/// <summary>
/// Basic Authentication middleware
/// Basic Auth header'ını alır, Server API'den Bearer token alır ve request'e ekler
/// WCF sistemleri için uyumlu
/// </summary>
public class BasicAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<BasicAuthenticationMiddleware> _logger;

    public BasicAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<BasicAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IWcfServerApiClient serverApiClient)
    {
        // Zaten Bearer token varsa devam et
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Basic Authentication kontrolü
        if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var credentialBytes = Convert.FromBase64String(encodedCredentials);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                if (credentials.Length == 2)
                {
                    var username = credentials[0];
                    var password = credentials[1];

                    // Server API'den Bearer token al
                    var loginRequest = new LoginWcfRequest(username, password);
                    var response = await serverApiClient.AuthenticateAsync(loginRequest);

                    if (response != null)
                    {
                        // Bearer token'ı request header'ına ekle
                        context.Request.Headers.Authorization = $"Bearer {response.AccessToken}";
                        _logger.LogDebug("Basic Auth converted to Bearer token for user {Username}", username);
                    }
                    else
                    {
                        _logger.LogWarning("Basic Auth failed for user {Username}", username);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.Headers.WWWAuthenticate = "Basic realm=\"WCF API\", Bearer";
                        await context.Response.WriteAsJsonAsync(new { error = "Invalid credentials" });
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Basic Authentication");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Authentication failed" });
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Middleware extension
/// </summary>
public static class BasicAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseBasicToBearer(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<BasicAuthenticationMiddleware>();
    }
}


