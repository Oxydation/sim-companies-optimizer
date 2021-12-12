using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.APIs;

public class ExchangeTrackerApi : IExchangeTrackerApi
{
    private readonly IExchangeTrackerCache _cache;

    public ExchangeTrackerApi(IExchangeTrackerCache cache)
    {
        _cache = cache;
    }

    public async Task<Price?> GetCurrentPrice(ResourceId resourceId, CancellationToken cancellationToken)
    {
        var index = GetIndex(resourceId);
        if (index== -1)
        {
            return null;
        }
        var latestEntry = await _cache.GetLatestEntry(cancellationToken);
        var value = latestEntry.ExchangePrices[index];
        if (!value.HasValue) return null;
        return new Price
        {
            Timestamp = latestEntry.Timestamp ?? DateTimeOffset.MinValue,
            Value = value
        };
    }

    public async Task<Price> GetAveragedPrice(ResourceId resourceId, TimeSpan timeSpan,
        CancellationToken cancellationToken)
    {
        var index = GetIndex(resourceId);
        if (index== -1)
        {
            return null;
        }
        var entries = await _cache.GetEntries(cancellationToken);
        var avg = entries.Average(x => x.ExchangePrices[index]);
        return new Price
        {
            Timestamp = null,
            Value = avg
        };
    }

    public async Task<Price> GetMinPrice(ResourceId resourceId, TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        var index = GetIndex(resourceId);
        if (index== -1)
        {
            return null;
        }
        var entries = await _cache.GetEntries(cancellationToken);
        var min = entries.Min(x => x.ExchangePrices[index]);
        return new Price
        {
            Timestamp = null,
            Value = min
        };
    }

    public async Task<Price> GetMaxPrice(ResourceId resourceId, TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        var index = GetIndex(resourceId);
        if (index == -1)
        {
            return null;
        }
        var entries = await _cache.GetEntries(cancellationToken);
        var max = entries.Max(x => x.ExchangePrices[index]);
        return new Price
        {
            Timestamp = null,
            Value = max
        };
    }

    public async Task<PriceCard> GetPriceDetails(ResourceId resourceId, TimeSpan timeSpan,
        CancellationToken cancellationToken)
    {
        var priceCard = new PriceCard
        {
            Current = await GetCurrentPrice(resourceId, cancellationToken),
            Average = await GetAveragedPrice(resourceId, timeSpan, cancellationToken),
            Min = await GetMinPrice(resourceId, timeSpan, cancellationToken),
            Max = await GetMaxPrice(resourceId, timeSpan, cancellationToken)
        };
        return priceCard;
    }

    private int GetIndex(ResourceId resourceId)
    {
        return _cache.GetIndexOfResourceId(resourceId);
    }
}