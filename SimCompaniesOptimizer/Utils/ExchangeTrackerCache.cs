using SimCompaniesOptimizer.Database;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Utils;

public class ExchangeTrackerCache : IExchangeTrackerCache
{
    private readonly IExchangeTrackerReader _exchangeTrackerReader;
    private List<ExchangeTrackerEntry> _cache = new();
    private ExchangeTrackerEntry? _latestEntry;

    public ExchangeTrackerCache(IExchangeTrackerReader exchangeTrackerReader)
    {
        _exchangeTrackerReader = exchangeTrackerReader;
    }

    public async Task RefreshCache(CancellationToken cancellationToken)
    { 
        await using var dbContext = new SimCompaniesDbContext();
        if (dbContext.ExchangeTrackerEntries.Any())
        {
            dbContext.ExchangeTrackerEntries.RemoveRange(dbContext.ExchangeTrackerEntries.ToList());
            await dbContext.SaveChangesAsync(cancellationToken);
            _cache.Clear();
        }
    }

    public async Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(CancellationToken cancellationToken)
    {
        if (_cache.Count > 0) return _cache;

        await using var dbContext = new SimCompaniesDbContext();
        if (dbContext.ExchangeTrackerEntries.Any())
        {
            _cache = dbContext.ExchangeTrackerEntries.ToList();
        }
        else
        {
            var result = await _exchangeTrackerReader.GetAllEntriesFromExchangeApiAsync(cancellationToken);
            var exchangeTrackerEntries = result.Where(r => r.Timestamp != null && r.ExchangePrices.Any()).ToList();
            _cache = exchangeTrackerEntries;
            dbContext.ExchangeTrackerEntries.AddRange(exchangeTrackerEntries);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        _latestEntry = _cache.MaxBy(entry => entry.Timestamp);

        return _cache;
    }

    public async Task<ExchangeTrackerEntry> GetLatestEntry(CancellationToken cancellationToken)
    {
        if (_latestEntry != null) return _latestEntry;

        var entries = await GetEntries(cancellationToken);
        return _latestEntry;
    }
}