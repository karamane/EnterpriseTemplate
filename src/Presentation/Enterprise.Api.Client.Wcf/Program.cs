using Enterprise.Api.Client.Wcf.Extensions;
using Enterprise.Infrastructure.Logging.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Enterprise WCF Client API",
        Version = "v1",
        Description = "WCF servisleri üzerinden çalışan Client API (DMZ)"
    });
});

// WCF Client API services
builder.Services.RegisterWcfClientApi(builder.Configuration);

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// WCF Client API middleware
app.UseWcfClientApi();

// Exception handling
app.UseMiddleware<ExceptionLoggingMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Startup banner
PrintStartupBanner(app, "Enterprise.Api.Client.Wcf");

app.Run();

// ===========================================
// STARTUP BANNER
// ===========================================
static void PrintStartupBanner(WebApplication app, string appName)
{
    var urls = app.Urls.Any() 
        ? string.Join(", ", app.Urls) 
        : "http://localhost:5000 (default)";
    
    var env = app.Environment.EnvironmentName;
    var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled", true);
    
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                                                              ║");
    Console.WriteLine($"║  {appName,-56}  ║");
    Console.WriteLine("║  (DMZ - WCF Client API)                                      ║");
    Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
    Console.ResetColor();
    
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"║  ✓ Status      : Running                                     ║");
    Console.ResetColor();
    
    Console.WriteLine($"║  • Environment : {env,-43} ║");
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
    
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("║                                                              ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

// For integration tests
public partial class Program { }

