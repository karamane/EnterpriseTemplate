using Microsoft.Extensions.Options;

namespace Enterprise.Infrastructure.Logging.Helpers;

/// <summary>
/// SensitiveDataMasker için static helper class
/// DI olmadan kullanım için (eski kod uyumluluğu)
/// </summary>
public static class SensitiveDataMaskerStatic
{
    private static readonly Lazy<SensitiveDataMasker> _instance = new(() =>
        new SensitiveDataMasker(Microsoft.Extensions.Options.Options.Create(new SensitiveDataOptions())));

    /// <summary>
    /// JSON içindeki hassas alanları maskeler
    /// </summary>
    public static string? MaskJson(string? json) => _instance.Value.MaskJson(json);

    /// <summary>
    /// JSON içindeki hassas alanları maskeler (ek alanlar ile)
    /// </summary>
    public static string? MaskJson(string? json, string[]? additionalSensitiveFields) 
        => _instance.Value.MaskJson(json);

    /// <summary>
    /// Text içindeki hassas verileri maskeler
    /// </summary>
    public static string MaskText(string text) => _instance.Value.MaskText(text);

    /// <summary>
    /// Log injection saldırılarını önler
    /// </summary>
    public static string SanitizeForLogging(string? input) => _instance.Value.SanitizeForLogging(input);

    /// <summary>
    /// Dictionary içindeki hassas alanları maskeler
    /// </summary>
    public static Dictionary<string, string> MaskDictionary(Dictionary<string, string>? dictionary) 
        => _instance.Value.MaskDictionary(dictionary);
}

