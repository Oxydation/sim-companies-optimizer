using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Interfaces;

public interface IExchangeTrackerCache
{
    public Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(CancellationToken cancellationToken);
    public Task<ExchangeTrackerEntry?> GetLatestEntry(CancellationToken cancellationToken);

    Task DeleteAll(CancellationToken cancellationToken);

    Task<IEnumerable<ExchangeTrackerEntry>> GetEntriesByInterval(TimeSpan? stepInterval, TimeSpan timeSpanIntoPast,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Syncs only the new records.
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task SyncNewExchangeEntries(CancellationToken cancellationToken);

    Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(TimeSpan? timeSpanIntoPast, CancellationToken cancellationToken);
}