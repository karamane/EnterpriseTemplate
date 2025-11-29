namespace Enterprise.Core.Domain.Entities;

/// <summary>
/// Audit bilgisi tutan entity'ler için base sınıf
/// </summary>
/// <typeparam name="TId">ID tipi</typeparam>
public abstract class AuditableEntity<TId> : BaseEntity<TId>
{
    /// <summary>
    /// Oluşturulma tarihi (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Oluşturan kullanıcı ID
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Son güncelleme tarihi (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Son güncelleyen kullanıcı ID
    /// </summary>
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Guid ID'li auditable entity
/// </summary>
public abstract class AuditableEntity : AuditableEntity<Guid>
{
}

/// <summary>
/// Soft delete destekli entity
/// </summary>
/// <typeparam name="TId">ID tipi</typeparam>
public abstract class SoftDeleteEntity<TId> : AuditableEntity<TId>
{
    /// <summary>
    /// Silinmiş mi?
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Silinme tarihi (UTC)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Silen kullanıcı ID
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Soft delete yapar
    /// </summary>
    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Soft delete'i geri alır
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}

/// <summary>
/// Guid ID'li soft delete entity
/// </summary>
public abstract class SoftDeleteEntity : SoftDeleteEntity<Guid>
{
}

