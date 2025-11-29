namespace Enterprise.Infrastructure.Caching.Options;

/// <summary>
/// Cache yapılandırma seçenekleri
/// Redis ve MemoryCache arasında switch yapılabilir
/// </summary>
public class CacheOptions
{
    public const string SectionName = "Cache";

    /// <summary>
    /// Cache provider tipi (Redis, Memory)
    /// </summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>
    /// Redis connection string (Provider = Redis ise)
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Redis instance adı
    /// </summary>
    public string InstanceName { get; set; } = "Enterprise_";

    /// <summary>
    /// Varsayılan TTL (dakika)
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Sliding expiration varsayılan (dakika)
    /// </summary>
    public int SlidingExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Parametrelerin TTL (dakika)
    /// </summary>
    public int ParameterExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Memory cache size limit (MB) - Provider = Memory ise
    /// </summary>
    public int MemoryCacheSizeLimitMb { get; set; } = 100;

    /// <summary>
    /// Memory cache compaction percentage (0-1)
    /// </summary>
    public double CompactionPercentage { get; set; } = 0.25;

    /// <summary>
    /// Redis retry count
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Redis connect timeout (ms)
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Redis sync timeout (ms)
    /// </summary>
    public int SyncTimeoutMs { get; set; } = 1000;

    /// <summary>
    /// Fallback to memory cache on Redis failure
    /// </summary>
    public bool FallbackToMemory { get; set; } = true;
}

/// <summary>
/// Cache provider tipleri
/// </summary>
public enum CacheProvider
{
    /// <summary>
    /// In-memory cache (tek sunucu için)
    /// </summary>
    Memory,

    /// <summary>
    /// Redis distributed cache (multi-server için)
    /// </summary>
    Redis,

    /// <summary>
    /// Hybrid: Memory + Redis (performans için)
    /// L1: Memory, L2: Redis
    /// </summary>
    Hybrid
}
