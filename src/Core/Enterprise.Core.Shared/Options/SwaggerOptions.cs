namespace Enterprise.Core.Shared.Options;

/// <summary>
/// Swagger yapılandırma seçenekleri
/// appsettings.json'dan okunur
/// </summary>
public class SwaggerOptions
{
    public const string SectionName = "Swagger";

    /// <summary>
    /// Swagger etkin mi?
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Swagger UI etkin mi?
    /// </summary>
    public bool EnableUI { get; set; } = true;

    /// <summary>
    /// API başlığı
    /// </summary>
    public string Title { get; set; } = "Enterprise API";

    /// <summary>
    /// API versiyonu
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// API açıklaması
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// İletişim bilgileri
    /// </summary>
    public SwaggerContactOptions? Contact { get; set; }

    /// <summary>
    /// Lisans bilgileri
    /// </summary>
    public SwaggerLicenseOptions? License { get; set; }

    /// <summary>
    /// Swagger endpoint yolu
    /// </summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>
    /// JSON endpoint yolu
    /// </summary>
    public string JsonEndpoint { get; set; } = "/swagger/v1/swagger.json";

    /// <summary>
    /// XML yorum dosyası kullan
    /// </summary>
    public bool IncludeXmlComments { get; set; } = true;

    /// <summary>
    /// Bearer token authentication ekle
    /// </summary>
    public bool EnableBearerAuth { get; set; } = true;

    /// <summary>
    /// API Key authentication ekle
    /// </summary>
    public bool EnableApiKeyAuth { get; set; } = false;

    /// <summary>
    /// Sadece belirli ortamlarda göster (boş = her zaman)
    /// </summary>
    public string[] AllowedEnvironments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Swagger UI tema (dark, light)
    /// </summary>
    public string Theme { get; set; } = "light";

    /// <summary>
    /// DocExpansion (none, list, full)
    /// </summary>
    public string DocExpansion { get; set; } = "list";

    /// <summary>
    /// Operasyonları varsayılan olarak daralt
    /// </summary>
    public bool DefaultModelsExpandDepth { get; set; } = true;
}

/// <summary>
/// Swagger iletişim bilgileri
/// </summary>
public class SwaggerContactOptions
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// Swagger lisans bilgileri
/// </summary>
public class SwaggerLicenseOptions
{
    public string? Name { get; set; }
    public string? Url { get; set; }
}

