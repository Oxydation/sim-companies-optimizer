using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Interfaces;

public interface IExchangeTrackerCache
{
    public Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(CancellationToken cancellationToken);
    public Task<ExchangeTrackerEntry?> GetLatestEntry(CancellationToken cancellationToken);

    public int GetIndexOfResourceId(ResourceId resourceId)
    {
        if (NotSellableResourceIds.NotSellableResources.Contains(resourceId)) return -1;

        var values = Enum.GetValues<ResourceId>().ToList();
        values.RemoveAll(x => NotSellableResourceIds.NotSellableResources.Contains(x));
        return values.IndexOf(resourceId);
        // return Array.IndexOf(, resourceId);
    }
}