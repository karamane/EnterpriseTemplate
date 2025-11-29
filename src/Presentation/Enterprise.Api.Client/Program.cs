using AspNetCoreRateLimit;
using Enterprise.Api.Client.Extensions;
using Enterprise.Infrastructure.Logging.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ===========================================
// SERILOG CONFIGURATION
// ===========================================
Log.Logger = new LoggerConfiguration()
    .ConfigureSerilog(builder.Configuration, "Enterprise.Api.Client")
    .CreateLogger();

builder.Host.UseSerilog();

// ===========================================
// SERVICES - Plugin gibi tek satırda ekleme
// ===========================================

// Tüm Enterprise Client API altyapısını tek satırda ekle
// Client API tamamen izole - Server API referansı yok
builder.Services.RegisterEnterpriseClientApi(builder.Configuration);

// API Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Enterprise Client API",
        Version = "v1",
        Description = "DMZ - Public API for Mobile Applications"
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ===========================================
// MIDDLEWARE PIPELINE
// ===========================================

// Rate Limiting (DDoS Protection - en üstte)
app.UseIpRateLimiting();

// Enterprise Logging middleware'leri (tek satırda)
// - ExceptionLoggingMiddleware
// - CorrelationIdMiddleware  
// - RequestLoggingMiddleware
// - ActionLoggingMiddleware
app.UseLogging();

// Serilog
app.UseSerilogRequestLogging();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS
app.UseHttpsRedirection();

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
    PrintStartupBanner(app, "Enterprise.Api.Client");
    
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
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine($"║  {appName,-56}  ║");
    Console.WriteLine("║  (DMZ - Public API)                                          ║");
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
    
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
    
    Log.Information("🚀 {AppName} started on {Urls} ({Environment})", appName, urls, env);
}
