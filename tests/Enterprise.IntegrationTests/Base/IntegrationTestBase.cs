using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Enterprise.IntegrationTests.Base;

/// <summary>
/// Integration test base sınıfı
/// WebApplicationFactory kullanır
/// </summary>
public abstract class IntegrationTestBase<TEntryPoint> : IClassFixture<WebApplicationFactory<TEntryPoint>>, IDisposable
    where TEntryPoint : class
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<TEntryPoint> Factory;

    protected IntegrationTestBase(WebApplicationFactory<TEntryPoint> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Test için servisleri override et
                ConfigureTestServices(services);
            });
        });

        Client = Factory.CreateClient();
    }

    /// <summary>
    /// Test servislerini yapılandır
    /// Override edilebilir
    /// </summary>
    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Varsayılan: Hiçbir şey yapma
        // Alt sınıflar mock servisler ekleyebilir
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}

