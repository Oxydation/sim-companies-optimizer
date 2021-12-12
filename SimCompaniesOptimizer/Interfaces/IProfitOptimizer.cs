using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Interfaces;

public interface IProfitOptimizer
{
    public Task<ProductionStatistic> OptimalBuildingLevelForHorizontalProduction(ResourceId resourceId,
        CancellationToken cancellationToken, int buildingLevelLimit = 200);

    public Task<List<ProductionStatistic>> OptimalBuildingsForGivenResourcesRandom(IEnumerable<ResourceId> resources,
        int generations, CancellationToken cancellationToken, int buildingLevelLimit = 30,
        int maxBuildingPlaces = 12);
}