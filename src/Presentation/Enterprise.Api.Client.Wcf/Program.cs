using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Enterprise.Api.Client.Wcf.Extensions;
using Enterprise.Api.Client.Wcf.Services;
using Enterprise.Api.Client.Wcf.Services.Contracts;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===========================================
// SERVICES
// ===========================================

// WCF Client API services (logging, auth, etc.)
builder.Services.RegisterWcfClientApi(builder.Configuration);

// CoreWCF Services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

// Controllers (REST Client API ile aynı metotlar)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Enterprise WCF Client API",
        Version = "v1",
        Description = "SOAP/WCF endpoints + REST fallback for legacy clients\n\n" +
                      "**SOAP Endpoints (WSDL):**\n" +
                      "- `/AuthService.svc` - Authentication\n" +
                      "- `/CustomerService.svc` - Customer operations\n\n" +
                      "**REST Endpoints (bu Swagger UI):**\n" +
                      "- `/api/wcf/auth/*` - Authentication\n" +
                      "- `/api/wcf/customers/*` - Customer operations\n\n" +
                      "**Not:** SOAP için WSDL kullanın, REST için bu Swagger UI'ı kullanın."
    });

    // JWT Bearer Authentication for Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token giriniz. Örnek: eyJhbGciOiJIUzI1NiIs..."
    });

    // [AllowAnonymous] olan endpoint'ler için kilit gösterme
    options.OperationFilter<Enterprise.Core.Shared.Extensions.AuthorizeCheckOperationFilter>();
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ===========================================
// MIDDLEWARE PIPELINE
// ===========================================

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// WCF Client API middleware (Basic Auth → Bearer, Logging)
app.UseWcfClientApi();

// Authentication & Authorization (REST Controllers için)
app.UseAuthentication();
app.UseAuthorization();

// ===========================================
// COREWCF SOAP ENDPOINTS
// ===========================================

app.UseServiceModel(serviceBuilder =>
{
    // Auth Service - SOAP endpoint
    serviceBuilder.AddService<WcfAuthService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = app.Environment.IsDevelopment();
    });
    serviceBuilder.AddServiceEndpoint<WcfAuthService, IWcfAuthService>(
        new BasicHttpBinding
        {
            MaxReceivedMessageSize = 65536,
            MaxBufferSize = 65536,
            Security = new BasicHttpSecurity { Mode = BasicHttpSecurityMode.None }
        },
        "/AuthService.svc");

    // Customer Service - SOAP endpoint
    serviceBuilder.AddService<WcfCustomerService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = app.Environment.IsDevelopment();
    });
    serviceBuilder.AddServiceEndpoint<WcfCustomerService, IWcfCustomerService>(
        new BasicHttpBinding
        {
            MaxReceivedMessageSize = 65536,
            MaxBufferSize = 65536,
            Security = new BasicHttpSecurity { Mode = BasicHttpSecurityMode.None }
        },
        "/CustomerService.svc");

    // WSDL metadata
    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;
    serviceMetadataBehavior.HttpsGetEnabled = true;
});

// REST Controllers
app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// ===========================================
// STARTUP BANNER
// ===========================================
PrintStartupBanner(app);

app.Run();

// ===========================================
// STARTUP BANNER
// ===========================================
static void PrintStartupBanner(WebApplication app)
{
    var urls = app.Urls.Any()
        ? string.Join(", ", app.Urls)
        : "http://localhost:5000 (default)";

    var env = app.Environment.EnvironmentName;

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("║  Enterprise.Api.Client.Wcf                                   ║");
    Console.WriteLine("║  (DMZ - WCF/SOAP Client API)                                 ║");
    Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("║  ✓ Status      : Running                                     ║");
    Console.ResetColor();

    Console.WriteLine($"║  • Environment : {env,-43} ║");
    Console.WriteLine($"║  • URLs        : {urls,-43} ║");

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("║  SOAP Endpoints:                                             ║");
    Console.WriteLine("║  • /AuthService.svc     - Authentication                     ║");
    Console.WriteLine("║  • /CustomerService.svc - Customer operations                ║");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("║  REST Endpoints:                                             ║");
    Console.WriteLine("║  • /api/wcf/auth/*      - Authentication                     ║");
    Console.WriteLine("║  • /api/wcf/customers/* - Customer operations                ║");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("║  WSDL:                                                       ║");
    Console.WriteLine("║  • /AuthService.svc?wsdl                                     ║");
    Console.WriteLine("║  • /CustomerService.svc?wsdl                                 ║");
    Console.ResetColor();

    Console.WriteLine($"║  • Health      : /health                                     ║");
    Console.WriteLine($"║  • Swagger     : /swagger                                    ║");
    Console.WriteLine($"║  • Started     : {DateTime.Now:dd.MM.yyyy HH:mm:ss}                          ║");

    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

// For integration tests
public partial class Program { }
