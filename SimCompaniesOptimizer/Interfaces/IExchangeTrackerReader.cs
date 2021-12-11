using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Interfaces;

public interface IExchangeTrackerReader
{
    Task<IEnumerable<ExchangeTrackerEntry>> GetAllEntriesFromExchangeApiAsync(CancellationToken cancellationToken);
}