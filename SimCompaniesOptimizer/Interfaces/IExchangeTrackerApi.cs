using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Interfaces;

public interface IExchangeTrackerApi
{
    public Task<Price?> GetCurrentPrice(ResourceId resourceId, CancellationToken cancellationToken);
    public Task<Price> GetAveragedPrice(ResourceId resourceId, TimeSpan timeSpan, CancellationToken cancellationToken);
    public Task<Price> GetMinPrice(ResourceId resourceId, TimeSpan timeSpan, CancellationToken cancellationToken);
    public Task<Price> GetMaxPrice(ResourceId resourceId, TimeSpan timeSpan, CancellationToken cancellationToken);

    public Task<PriceCard> GetPriceDetails(ResourceId resourceId, TimeSpan timeSpan,
        CancellationToken cancellationToken);
}