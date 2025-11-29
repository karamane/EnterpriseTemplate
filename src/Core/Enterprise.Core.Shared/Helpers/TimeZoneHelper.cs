namespace Enterprise.Core.Shared.Helpers;

/// <summary>
/// Türkiye saat dilimi için helper sınıf
/// </summary>
public static class TimeZoneHelper
{
    /// <summary>
    /// Türkiye saat dilimi ID
    /// </summary>
    public const string TurkeyTimeZoneId = "Turkey Standard Time";

    /// <summary>
    /// Türkiye saat dilimi (IANA formatı - Linux için)
    /// </summary>
    public const string TurkeyTimeZoneIdIana = "Europe/Istanbul";

    private static readonly Lazy<TimeZoneInfo> _turkeyTimeZone = new(() =>
    {
        try
        {
            // Windows için
            return TimeZoneInfo.FindSystemTimeZoneById(TurkeyTimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            try
            {
                // Linux/macOS için IANA formatı
                return TimeZoneInfo.FindSystemTimeZoneById(TurkeyTimeZoneIdIana);
            }
            catch
            {
                // Fallback: UTC+3 olarak manuel oluştur
                return TimeZoneInfo.CreateCustomTimeZone(
                    "Turkey",
                    TimeSpan.FromHours(3),
                    "Turkey Standard Time",
                    "Turkey Standard Time");
            }
        }
    });

    /// <summary>
    /// Türkiye TimeZoneInfo
    /// </summary>
    public static TimeZoneInfo TurkeyTimeZone => _turkeyTimeZone.Value;

    /// <summary>
    /// Şu anki Türkiye saati
    /// </summary>
    public static DateTime NowTurkey => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);

    /// <summary>
    /// Şu anki Türkiye saati (DateTimeOffset)
    /// </summary>
    public static DateTimeOffset NowTurkeyOffset => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TurkeyTimeZone);

    /// <summary>
    /// UTC'yi Türkiye saatine çevirir
    /// </summary>
    public static DateTime ToTurkeyTime(this DateTime utcDateTime)
    {
        if (utcDateTime.Kind == DateTimeKind.Local)
        {
            utcDateTime = utcDateTime.ToUniversalTime();
        }
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TurkeyTimeZone);
    }

    /// <summary>
    /// UTC'yi Türkiye saatine çevirir (DateTimeOffset)
    /// </summary>
    public static DateTimeOffset ToTurkeyTime(this DateTimeOffset utcDateTime)
    {
        return TimeZoneInfo.ConvertTime(utcDateTime, TurkeyTimeZone);
    }

    /// <summary>
    /// Türkiye saatini UTC'ye çevirir
    /// </summary>
    public static DateTime ToUtcFromTurkey(this DateTime turkeyDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(turkeyDateTime, TurkeyTimeZone);
    }

    /// <summary>
    /// Tarih formatı (Türkiye standardı)
    /// </summary>
    public static string ToTurkeyString(this DateTime dateTime, string format = "dd.MM.yyyy HH:mm:ss")
    {
        return dateTime.ToTurkeyTime().ToString(format);
    }

    /// <summary>
    /// Tarih formatı (Türkiye standardı)
    /// </summary>
    public static string ToTurkeyString(this DateTimeOffset dateTime, string format = "dd.MM.yyyy HH:mm:ss")
    {
        return dateTime.ToTurkeyTime().ToString(format);
    }
}

