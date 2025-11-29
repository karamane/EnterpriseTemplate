using Enterprise.Core.Application.Models.Logging;

namespace Enterprise.Infrastructure.Logging.Sinks;

/// <summary>
/// Log sink interface
/// Farklı hedeflere log yazmak için
/// </summary>
public interface ILogSink
{
    /// <summary>
    /// Sink adı
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Sink aktif mi?
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Tek bir log yazar
    /// </summary>
    Task WriteAsync(BaseLogEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch log yazar
    /// </summary>
    Task WriteBatchAsync(IEnumerable<BaseLogEntry> entries, CancellationToken cancellationToken = default);
}

/// <summary>
/// Log sink manager interface
/// </summary>
public interface ILogSinkManager
{
    /// <summary>
    /// Tüm aktif sink'lere yazar
    /// </summary>
    Task WriteAsync(IEnumerable<BaseLogEntry> entries, CancellationToken cancellationToken = default);

    /// <summary>
    /// Belirli bir sink'e yazar
    /// </summary>
    Task WriteToSinkAsync(string sinkName, IEnumerable<BaseLogEntry> entries, CancellationToken cancellationToken = default);
}

