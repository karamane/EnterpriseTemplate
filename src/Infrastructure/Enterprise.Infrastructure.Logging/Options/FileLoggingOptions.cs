namespace Enterprise.Infrastructure.Logging.Options;

/// <summary>
/// Dosya bazlı loglama yapılandırması
/// </summary>
public class FileLoggingOptions
{
    public const string SectionName = "Logging:File";

    /// <summary>
    /// Dosya loglaması aktif mi?
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Ana log klasörü (varsayılan: logs)
    /// </summary>
    public string BasePath { get; set; } = "logs";

    /// <summary>
    /// Uygulama adını alt klasör olarak kullan
    /// </summary>
    public bool UseApplicationSubfolder { get; set; } = true;

    /// <summary>
    /// Log dosyası tutma süresi (gün)
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Maksimum dosya boyutu (MB) - aşılırsa yeni dosya oluşturulur
    /// </summary>
    public int MaxFileSizeMB { get; set; } = 100;

    /// <summary>
    /// Dosya sıkıştırma (eski dosyalar için)
    /// </summary>
    public bool CompressOldFiles { get; set; } = false;

    /// <summary>
    /// JSON formatında log yaz
    /// </summary>
    public bool UseJsonFormat { get; set; } = false;

    /// <summary>
    /// Ayrı log dosyaları yapılandırması
    /// </summary>
    public SeparateLogFilesOptions SeparateFiles { get; set; } = new();

    /// <summary>
    /// Rolling interval (günlük, saatlik vs.)
    /// </summary>
    public LogRollingInterval RollingInterval { get; set; } = LogRollingInterval.Day;
}

/// <summary>
/// Ayrı log dosyaları yapılandırması
/// </summary>
public class SeparateLogFilesOptions
{
    /// <summary>
    /// Tüm logları tek dosyaya yaz
    /// </summary>
    public bool AllLogs { get; set; } = true;

    /// <summary>
    /// Error ve üstü logları ayrı dosyaya yaz
    /// </summary>
    public bool ErrorLogs { get; set; } = true;

    /// <summary>
    /// Request/Response loglarını ayrı dosyaya yaz
    /// </summary>
    public bool RequestLogs { get; set; } = true;

    /// <summary>
    /// Performance loglarını ayrı dosyaya yaz
    /// </summary>
    public bool PerformanceLogs { get; set; } = true;

    /// <summary>
    /// Business exception loglarını ayrı dosyaya yaz
    /// </summary>
    public bool BusinessLogs { get; set; } = true;

    /// <summary>
    /// Security/Audit loglarını ayrı dosyaya yaz
    /// </summary>
    public bool SecurityLogs { get; set; } = true;
}

/// <summary>
/// Log dosyası rolling aralığı
/// </summary>
public enum LogRollingInterval
{
    /// <summary>
    /// Sınırsız (tek dosya)
    /// </summary>
    Infinite,

    /// <summary>
    /// Yılda bir
    /// </summary>
    Year,

    /// <summary>
    /// Ayda bir
    /// </summary>
    Month,

    /// <summary>
    /// Günde bir
    /// </summary>
    Day,

    /// <summary>
    /// Saatte bir
    /// </summary>
    Hour,

    /// <summary>
    /// Dakikada bir (test için)
    /// </summary>
    Minute
}

