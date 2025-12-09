using Enterprise.Api.Server.Extensions;
using Enterprise.Api.Server.Middleware;
using Enterprise.Infrastructure.Logging.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===========================================
// SERILOG CONFIGURATION
// ===========================================
Log.Logger = new LoggerConfiguration()
    .ConfigureSerilog(builder.Configuration, "Enterprise.Api.Server")
    .CreateLogger();

builder.Host.UseSerilog();

// ===========================================
// SERVICES - Plugin gibi tek satırda ekleme
// ===========================================

// Tüm Enterprise altyapısını tek satırda ekle
builder.Services.RegisterEnterpriseServerApi(builder.Configuration);

// API Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Enterprise Server API",
        Version = "v1",
        Description = "Secure Zone - Internal API"
    });

    // JWT Bearer Authentication for Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\n\n" +
                      "Enter your token in the text input below.\n\n" +
                      "Example: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    // [AllowAnonymous] olan endpoint'ler için kilit gösterme
    options.OperationFilter<Enterprise.Core.Shared.Extensions.AuthorizeCheckOperationFilter>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientApi", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-Correlation-ID", "X-Server-Name");
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ===========================================
// MIDDLEWARE PIPELINE
// Sıralama kritik: Exception -> Correlation -> Logging -> Business Logic
// ===========================================

// Global exception handling (en üstte)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enterprise Logging middleware'leri (tek satırda)
// - ExceptionLoggingMiddleware
// - CorrelationIdMiddleware  
// - RequestLoggingMiddleware
// - ActionLoggingMiddleware
app.UseLogging();

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
    };
});

// CORS
app.UseCors("AllowClientApi");

// Swagger (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS Redirection
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Health Check
app.MapHealthChecks("/health");

// ===========================================
// RUN
// ===========================================
try
{
    // Startup banner
    PrintStartupBanner(app, "Enterprise.Api.Server");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ===========================================
// STARTUP BANNER
// ===========================================
static void PrintStartupBanner(WebApplication app, string appName)
{
    var urls = app.Urls.Any() 
        ? string.Join(", ", app.Urls) 
        : "http://localhost:5000 (default)";
    
    var env = app.Environment.EnvironmentName;
    var version = app.Configuration["Logging:ApplicationVersion"] ?? "1.0.0";
    var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled", true);
    
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine($"║  {appName,-56}  ║");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
    Console.ResetColor();
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"║  ✓ Status      : Running                                     ║");
    Console.ResetColor();
    
    Console.WriteLine($"║  • Environment : {env,-43} ║");
    Console.WriteLine($"║  • Version     : {version,-43} ║");
    Console.WriteLine($"║  • URLs        : {urls,-43} ║");
    
    if (swaggerEnabled)
    {
        var swaggerUrl = app.Urls.FirstOrDefault() ?? "http://localhost:5000";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"║  • Swagger     : {swaggerUrl}/swagger                         ║");
        Console.ResetColor();
    }
    
    Console.WriteLine($"║  • Health      : /health                                     ║");
    Console.WriteLine($"║  • Started     : {DateTime.Now:dd.MM.yyyy HH:mm:ss}                          ║");
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
    
    Log.Information("🚀 {AppName} started on {Urls} ({Environment})", appName, urls, env);
}
