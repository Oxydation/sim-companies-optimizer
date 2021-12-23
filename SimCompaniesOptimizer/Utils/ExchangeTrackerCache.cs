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

    public async Task DeleteAll(CancellationToken cancellationToken)
    {
        await using var dbContext = new SimCompaniesDbContext();
        if (dbContext.ExchangeTrackerEntries.Any())
        {
            dbContext.ExchangeTrackerEntries.RemoveRange(dbContext.ExchangeTrackerEntries.ToList());
            await dbContext.SaveChangesAsync(cancellationToken);
            _cache.Clear();
        }
    }

    public async Task SyncNewExchangeEntries(CancellationToken cancellationToken)
    {
        var result = await _exchangeTrackerReader.GetAllEntriesFromExchangeApiAsync(cancellationToken);
        var exchangeTrackerEntries = result.Where(r => r.Timestamp != null && r.ExchangePrices.Any()).ToList();
        await using var dbContext = new SimCompaniesDbContext();
        foreach (var exchangeTrackerEntry in exchangeTrackerEntries.Where(exchangeTrackerEntry =>
                     dbContext.ExchangeTrackerEntries.FirstOrDefault(x =>
                         x.Timestamp == exchangeTrackerEntry.Timestamp) ==
                     null))
            dbContext.Add(exchangeTrackerEntry);

        await dbContext.SaveChangesAsync(cancellationToken);
        _cache = dbContext.ExchangeTrackerEntries.ToList();
    }

    public async Task<IEnumerable<ExchangeTrackerEntry>> GetEntriesByInterval(TimeSpan? stepInterval,
        TimeSpan timeSpanIntoPast, CancellationToken cancellationToken)
    {
        var entries = await GetEntries(timeSpanIntoPast, cancellationToken);
        if (stepInterval == null) return entries;

        // Get only by interval https://newbedev.com/linq-aggregate-and-group-by-periods-of-time
        return entries.GroupBy(s => s.Timestamp.Value.Ticks / TimeSpan.FromHours(1).Ticks)
            .Select(s => s.First()).ToList();
    }

    public async Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(
        CancellationToken cancellationToken)

    {
        return await GetEntries(null, cancellationToken);
    }

    public async Task<IEnumerable<ExchangeTrackerEntry>> GetEntries(TimeSpan? timeSpanIntoPast,
        CancellationToken cancellationToken)
    {
        if (_cache.Count > 0) return _cache;

        await using var dbContext = new SimCompaniesDbContext();
        if (dbContext.ExchangeTrackerEntries.Any())
        {
            if (timeSpanIntoPast != null)
            {
                var latestEntry = await GetLatestEntry(cancellationToken);
                var maxOldestEntryTimeStamp = latestEntry.Timestamp.Value.Subtract(timeSpanIntoPast.Value);
                _cache = dbContext.ExchangeTrackerEntries.Where(x => x.Timestamp.HasValue)
                    .Where(x => x.Timestamp >= maxOldestEntryTimeStamp).ToList();
            }
            else
            {
                _cache = dbContext.ExchangeTrackerEntries.ToList();
            }
        }
        else
        {
            await SyncNewExchangeEntries(cancellationToken);
        }

        _latestEntry = _cache.MaxBy(entry => entry.Timestamp);

        return _cache;
    }

    public async Task<ExchangeTrackerEntry> GetLatestEntry(CancellationToken cancellationToken)
    {
        if (_latestEntry != null) return _latestEntry;

        await using var dbContext = new SimCompaniesDbContext();
        _latestEntry = dbContext.ExchangeTrackerEntries.Where(x => x.Timestamp != null)
            .OrderByDescending(x => (DateTime)(object)x.Timestamp).FirstOrDefault();

        return _latestEntry;
    }
}