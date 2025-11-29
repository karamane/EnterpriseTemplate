using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Enterprise.Core.Shared.Extensions;

/// <summary>
/// String extension metodları
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    /// String'in null veya boş olup olmadığını kontrol eder
    /// </summary>
    public static bool IsNullOrEmpty(this string? value)
        => string.IsNullOrEmpty(value);

    /// <summary>
    /// String'in null, boş veya sadece whitespace olup olmadığını kontrol eder
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// String'i güvenli şekilde truncate eder
    /// </summary>
    public static string Truncate(this string? value, int maxLength, string suffix = "...")
    {
        if (value.IsNullOrEmpty())
            return string.Empty;

        if (value!.Length <= maxLength)
            return value;

        return value[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// String'i SHA256 hash'ine çevirir
    /// </summary>
    public static string ToSha256Hash(this string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// String'i Base64'e encode eder
    /// </summary>
    public static string ToBase64(this string value)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

    /// <summary>
    /// Base64 string'i decode eder
    /// </summary>
    public static string FromBase64(this string value)
        => Encoding.UTF8.GetString(Convert.FromBase64String(value));

    /// <summary>
    /// Newline karakterlerini temizler (Log injection koruması)
    /// </summary>
    public static string SanitizeForLogging(this string? value)
    {
        if (value.IsNullOrEmpty())
            return string.Empty;

        // Newline karakterlerini temizle
        var sanitized = value!
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ");

        // ANSI escape sequence'leri temizle
        sanitized = AnsiEscapeRegex().Replace(sanitized, "");

        // Control karakterleri temizle
        sanitized = new string(sanitized.Where(c => !char.IsControl(c) || c == ' ').ToArray());

        return sanitized;
    }

    /// <summary>
    /// Email formatını kontrol eder
    /// </summary>
    public static bool IsValidEmail(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return false;

        return EmailRegex().IsMatch(value!);
    }

    /// <summary>
    /// Telefon numarasını formatlar
    /// </summary>
    public static string FormatPhoneNumber(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return string.Empty;

        // Sadece rakamları al
        var digits = new string(value!.Where(char.IsDigit).ToArray());

        return digits.Length switch
        {
            10 => $"({digits[..3]}) {digits[3..6]}-{digits[6..]}",
            11 when digits.StartsWith('1') => $"+1 ({digits[1..4]}) {digits[4..7]}-{digits[7..]}",
            _ => digits
        };
    }

    /// <summary>
    /// Kredi kartı numarasını maskeler
    /// </summary>
    public static string MaskCreditCard(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return string.Empty;

        var digits = new string(value!.Where(char.IsDigit).ToArray());
        
        if (digits.Length < 4)
            return "****";

        return $"****-****-****-{digits[^4..]}";
    }

    /// <summary>
    /// Email adresini maskeler
    /// </summary>
    public static string MaskEmail(this string? value)
    {
        if (value.IsNullOrWhiteSpace())
            return string.Empty;

        var parts = value!.Split('@');
        if (parts.Length != 2)
            return "***@***";

        var localPart = parts[0];
        var domain = parts[1];

        var maskedLocal = localPart.Length > 2
            ? $"{localPart[..2]}***"
            : "***";

        return $"{maskedLocal}@{domain}";
    }

    [GeneratedRegex(@"\x1B\[[0-9;]*[a-zA-Z]")]
    private static partial Regex AnsiEscapeRegex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}

