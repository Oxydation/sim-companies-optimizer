using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Interfaces;

public interface ISimCompaniesApi
{
    Task<IEnumerable<Resource>> GetAllResourcesAsync(CancellationToken cancellationToken,
        TimeSpan retrievalDelay = default);

    Task<Resource?> GetResourceAsync(ResourceId resourceId, CancellationToken cancellationToken, int quality = 0);

    Task UpdateExchangePriceOfResource(ResourceId resourceId, CancellationToken cancellationToken);

    Task UpdateExchangePriceOfAllResources(CancellationToken cancellationToken);
}