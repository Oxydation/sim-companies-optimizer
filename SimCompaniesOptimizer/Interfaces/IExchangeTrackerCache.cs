using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Interfaces;

public interface IExchangeTrackerCache
{
    public Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(CancellationToken cancellationToken);
    public Task<ExchangeTrackerEntry?> GetLatestEntry(CancellationToken cancellationToken);

    public int GetIndexOfResourceId(ResourceId resourceId)
    {
        return Array.IndexOf(Enum.GetValues<ResourceId>(), resourceId);
    }
}