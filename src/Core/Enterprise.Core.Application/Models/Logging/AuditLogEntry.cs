using Enterprise.Core.Shared.Constants;

namespace Enterprise.Core.Application.Models.Logging;

/// <summary>
/// Audit log entry
/// Denetim logları için
/// </summary>
public class AuditLogEntry : BaseLogEntry
{
    public AuditLogEntry()
    {
        LogType = LogConstants.LogTypes.Audit;
    }

    #region Audit Bilgileri

    /// <summary>
    /// Yapılan işlem (Create, Read, Update, Delete, Login, Logout, etc.)
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Entity tipi
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Entity ID
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Eski değerler (JSON)
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Yeni değerler (JSON)
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Değişiklik detayları
    /// </summary>
    public List<PropertyChange>? Changes { get; set; }

    #endregion

    #region İşlem Detayları

    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Başarısızlık nedeni
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// İşlem süresi (ms)
    /// </summary>
    public long? DurationMs { get; set; }

    #endregion

    /// <summary>
    /// Değişiklik ekler
    /// </summary>
    public void AddChange(string propertyName, object? oldValue, object? newValue)
    {
        Changes ??= new List<PropertyChange>();
        Changes.Add(new PropertyChange
        {
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue
        });
    }
}

/// <summary>
/// Property değişiklik bilgisi
/// </summary>
public class PropertyChange
{
    /// <summary>
    /// Property adı
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Eski değer
    /// </summary>
    public object? OldValue { get; set; }

    /// <summary>
    /// Yeni değer
    /// </summary>
    public object? NewValue { get; set; }
}

