namespace Enterprise.Core.Shared.Options;

/// <summary>
/// Hosting yapılandırma seçenekleri
/// Konsol veya Windows Service olarak çalıştırma
/// </summary>
public class HostingOptions
{
    public const string SectionName = "Hosting";

    /// <summary>
    /// Windows Service olarak çalıştır
    /// </summary>
    public bool RunAsWindowsService { get; set; } = false;

    /// <summary>
    /// Windows Service adı
    /// </summary>
    public string ServiceName { get; set; } = "EnterpriseApi";

    /// <summary>
    /// Windows Service açıklaması
    /// </summary>
    public string ServiceDisplayName { get; set; } = "Enterprise API Service";

    /// <summary>
    /// Windows Service açıklaması
    /// </summary>
    public string ServiceDescription { get; set; } = "Enterprise .NET API Service";

    /// <summary>
    /// Graceful shutdown timeout (saniye)
    /// </summary>
    public int ShutdownTimeoutSeconds { get; set; } = 30;
}

