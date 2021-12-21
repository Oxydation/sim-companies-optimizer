using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Interfaces;

public interface IExchangeTrackerCache
{
    public Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(CancellationToken cancellationToken);
    public Task<ExchangeTrackerEntry?> GetLatestEntry(CancellationToken cancellationToken);

    Task RefreshCache(CancellationToken cancellationToken);
}