using Enterprise.Core.Application.Interfaces.Persistence;

namespace Enterprise.Infrastructure.Persistence.Strategies;

/// <summary>
/// ORM strategy interface
/// EF Core ve Dapper arasında switch için
/// </summary>
public interface IOrmStrategy
{
    /// <summary>
    /// Strategy adı
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Repository instance oluşturur
    /// </summary>
    IRepository<TEntity> CreateRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Repository instance oluşturur (custom ID)
    /// </summary>
    IRepository<TEntity, TId> CreateRepository<TEntity, TId>() where TEntity : class;
}

/// <summary>
/// ORM türleri
/// </summary>
public enum OrmType
{
    EfCore,
    Dapper
}

/// <summary>
/// ORM yapılandırma seçenekleri
/// </summary>
public class OrmOptions
{
    public const string SectionName = "Orm";

    /// <summary>
    /// Aktif ORM türü
    /// </summary>
    public OrmType ActiveOrm { get; set; } = OrmType.EfCore;

    /// <summary>
    /// EF Core kullanılacak mı?
    /// </summary>
    public bool UseEfCore => ActiveOrm == OrmType.EfCore;

    /// <summary>
    /// Dapper kullanılacak mı?
    /// </summary>
    public bool UseDapper => ActiveOrm == OrmType.Dapper;
}

