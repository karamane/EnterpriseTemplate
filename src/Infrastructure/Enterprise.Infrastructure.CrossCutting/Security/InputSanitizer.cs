using System.Text.RegularExpressions;
using System.Web;
using Ganss.Xss;

namespace Enterprise.Infrastructure.CrossCutting.Security;

/// <summary>
/// Input sanitization servisi
/// XSS, SQL Injection ve diğer injection saldırılarını önler
/// </summary>
public interface IInputSanitizer
{
    /// <summary>
    /// HTML'i sanitize eder (XSS prevention)
    /// </summary>
    string SanitizeHtml(string? input);

    /// <summary>
    /// Tüm HTML tag'lerini temizler
    /// </summary>
    string StripHtml(string? input);

    /// <summary>
    /// SQL injection'a karşı temizler
    /// </summary>
    string SanitizeSql(string? input);

    /// <summary>
    /// Path traversal'a karşı temizler
    /// </summary>
    string SanitizePath(string? input);

    /// <summary>
    /// Genel amaçlı sanitization
    /// </summary>
    string Sanitize(string? input, SanitizeOptions? options = null);
}

/// <summary>
/// Input sanitizer implementasyonu
/// </summary>
public partial class InputSanitizer : IInputSanitizer
{
    private readonly HtmlSanitizer _htmlSanitizer;

    public InputSanitizer()
    {
        _htmlSanitizer = new HtmlSanitizer();

        // Güvenli tag'ler
        _htmlSanitizer.AllowedTags.Clear();
        _htmlSanitizer.AllowedTags.Add("p");
        _htmlSanitizer.AllowedTags.Add("br");
        _htmlSanitizer.AllowedTags.Add("b");
        _htmlSanitizer.AllowedTags.Add("i");
        _htmlSanitizer.AllowedTags.Add("u");
        _htmlSanitizer.AllowedTags.Add("strong");
        _htmlSanitizer.AllowedTags.Add("em");
        _htmlSanitizer.AllowedTags.Add("ul");
        _htmlSanitizer.AllowedTags.Add("ol");
        _htmlSanitizer.AllowedTags.Add("li");

        // Tehlikeli attribute'ları kaldır
        _htmlSanitizer.AllowedAttributes.Clear();
        _htmlSanitizer.AllowedAttributes.Add("class");
    }

    public string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return _htmlSanitizer.Sanitize(input);
    }

    public string StripHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Tüm HTML tag'lerini kaldır
        var stripped = HtmlTagRegex().Replace(input, string.Empty);

        // HTML entity'leri decode et
        stripped = HttpUtility.HtmlDecode(stripped);

        return stripped.Trim();
    }

    public string SanitizeSql(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Tehlikeli SQL karakterlerini escape et
        var sanitized = input
            .Replace("'", "''")
            .Replace("--", "")
            .Replace(";", "")
            .Replace("/*", "")
            .Replace("*/", "")
            .Replace("xp_", "")
            .Replace("EXEC", "", StringComparison.OrdinalIgnoreCase)
            .Replace("EXECUTE", "", StringComparison.OrdinalIgnoreCase)
            .Replace("DROP", "", StringComparison.OrdinalIgnoreCase)
            .Replace("DELETE", "", StringComparison.OrdinalIgnoreCase)
            .Replace("UPDATE", "", StringComparison.OrdinalIgnoreCase)
            .Replace("INSERT", "", StringComparison.OrdinalIgnoreCase);

        return sanitized;
    }

    public string SanitizePath(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Path traversal karakterlerini temizle
        var sanitized = input
            .Replace("..", "")
            .Replace("~", "")
            .Replace("//", "/");

        // Sadece güvenli karakterlere izin ver
        sanitized = PathUnsafeCharsRegex().Replace(sanitized, "");

        return sanitized;
    }

    public string Sanitize(string? input, SanitizeOptions? options = null)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        options ??= new SanitizeOptions();
        var result = input;

        if (options.StripHtml)
        {
            result = StripHtml(result);
        }
        else if (options.SanitizeHtml)
        {
            result = SanitizeHtml(result);
        }

        if (options.PreventSqlInjection)
        {
            result = SanitizeSql(result);
        }

        if (options.PreventPathTraversal)
        {
            result = SanitizePath(result);
        }

        if (options.TrimWhitespace)
        {
            result = result.Trim();
        }

        if (options.MaxLength > 0 && result.Length > options.MaxLength)
        {
            result = result[..options.MaxLength];
        }

        return result;
    }

    [GeneratedRegex("<[^>]*>")]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9_\-\./\\]")]
    private static partial Regex PathUnsafeCharsRegex();
}

/// <summary>
/// Sanitization seçenekleri
/// </summary>
public class SanitizeOptions
{
    /// <summary>
    /// HTML'i sanitize et
    /// </summary>
    public bool SanitizeHtml { get; set; }

    /// <summary>
    /// Tüm HTML tag'lerini kaldır
    /// </summary>
    public bool StripHtml { get; set; }

    /// <summary>
    /// SQL injection'a karşı koru
    /// </summary>
    public bool PreventSqlInjection { get; set; }

    /// <summary>
    /// Path traversal'a karşı koru
    /// </summary>
    public bool PreventPathTraversal { get; set; }

    /// <summary>
    /// Whitespace'i trim et
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Maksimum uzunluk (0 = sınırsız)
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Varsayılan güvenli seçenekler
    /// </summary>
    public static SanitizeOptions Default => new()
    {
        StripHtml = true,
        PreventSqlInjection = true,
        PreventPathTraversal = true,
        TrimWhitespace = true
    };
}

