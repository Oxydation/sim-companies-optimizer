using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ProfitCalculation;
using SimCompaniesOptimizer.Optimization;

namespace SimCompaniesOptimizer.Interfaces;

public interface IProfitOptimizer
{
    public Task<ProductionStatistic> OptimalBuildingLevelForHorizontalProduction(ResourceId resourceId,
        CancellationToken cancellationToken, int buildingLevelLimit = 200);

    public Task<List<ProductionStatistic>> OptimalBuildingsForGivenResourcesRandom(IList<ResourceId> resources,
        SimulationConfiguration simulationConfiguration, CancellationToken cancellationToken);
}