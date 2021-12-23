using System.Collections.Concurrent;
using System.Diagnostics;
using SimCompaniesOptimizer.Calculations;
using SimCompaniesOptimizer.Extensions;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ProfitCalculation;

namespace SimCompaniesOptimizer.Optimization;

public class ProfitOptimizer : IProfitOptimizer
{
    private static readonly Array ResourceEnumValues = Enum.GetValues(typeof(ResourceId));
    private static readonly ThreadLocal<Random> Random = new(() => new Random());
    private readonly IProfitCalculator _profitCalculator;

    public ProfitOptimizer(IProfitCalculator profitCalculator)
    {
        _profitCalculator = profitCalculator;
    }

    public async Task<ProductionStatistic> OptimalBuildingLevelForHorizontalProduction(ResourceId resourceId,
        CancellationToken cancellationToken, int buildingLevelLimit = 200)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        double currentMaxProfit = 0;
        var bestProductionStatistic = new ProductionStatistic();

        Parallel.For(0, buildingLevelLimit, async (i, state) =>
        {
            var companyParams = new CompanyParameters
            {
                CooOverheadReduction = 7,
                ProductionSpeed = 1.06,
                InputResourcesFromContracts = true,
                BuildingsPerResource = new Dictionary<ResourceId, int>
                {
                    { resourceId, i }
                }
            };
            var result =
                await _profitCalculator.CalculateProductionStatisticForCompany(companyParams, cancellationToken);
            if (result.TotalProfitPerHour > currentMaxProfit)
            {
                currentMaxProfit = result.TotalProfitPerHour;
                bestProductionStatistic = result;

                Console.WriteLine($"New max profit found {currentMaxProfit}");
            }

            Console.WriteLine($"Run {i} finished in {result.CalculationDuration}");
        });

        stopWatch.Stop();
        Console.WriteLine($"Total duration: {stopWatch.Elapsed}");
        return bestProductionStatistic;
    }

    public async Task<List<ProductionStatistic>> OptimalBuildingsRandom(
        IList<ResourceId> resources, SimulationConfiguration simulationConfiguration,
        CancellationToken cancellationToken)
    {
        var usedSeed = simulationConfiguration.Seed ?? Environment.TickCount;

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        double currentFittestProfit = 0;
        var bestStatistics = new ConcurrentBag<ProductionStatistic>();

        Parallel.For(0, simulationConfiguration.Generations, new ParallelOptions { MaxDegreeOfParallelism = 1 },
            async (i, state) =>
            {
                var companyParam = new CompanyParameters
                {
                    SimulationParameters = simulationConfiguration,
                    CooOverheadReduction = simulationConfiguration.CooOverheadReduction,
                    ProductionSpeed = 1.06,
                    InputResourcesFromContracts = simulationConfiguration.ContractSelection == ContractSelection.Enable,
                    MaxBuildingPlaces = simulationConfiguration.MaxBuildingPlaces,
                    Seed = usedSeed,
                    BuildingsPerResource =
                        GenerateRandomResourceBuildingLevels(resources, Random.Value,
                            simulationConfiguration.BuildingLevelLimit, simulationConfiguration.MaxBuildingPlaces)
                };
                if (companyParam.BuildingsPerResource.Count == 0) return;

                ProductionStatistic? result;
                ProfitHistory? profitHistory;

                switch (simulationConfiguration.OptimizationObjective)
                {
                    case OptimizationObjective.MaxAvgOverLastXDays:
                        profitHistory = await _profitCalculator.CalculateProfitHistoryForCompany(companyParam,
                            simulationConfiguration.DaysIntoPast, simulationConfiguration.StepInterval,
                            cancellationToken);
                        if (!(profitHistory.AvgProfitPerHour >= currentFittestProfit)) return;
                        currentFittestProfit = profitHistory.AvgProfitPerHour;
                        result = await _profitCalculator.CalculateProductionStatisticForCompany(companyParam,
                            cancellationToken);
                        result.ProfitResultsLastTenDays = profitHistory;
                        break;
                    case OptimizationObjective.MaxForLatestMarket:
                        result = await _profitCalculator.CalculateProductionStatisticForCompany(companyParam,
                            cancellationToken);
                        if (!(result.TotalProfitPerHour >= currentFittestProfit)) return;
                        currentFittestProfit = result.TotalProfitPerHour;
                        profitHistory = await _profitCalculator.CalculateProfitHistoryForCompany(companyParam,
                            simulationConfiguration.DaysIntoPast, simulationConfiguration.StepInterval,
                            cancellationToken);
                        result.ProfitResultsLastTenDays = profitHistory;
                        break;
                    case OptimizationObjective.MinLossPercentageOverLastXDays:
                        profitHistory = await _profitCalculator.CalculateProfitHistoryForCompany(companyParam,
                            simulationConfiguration.DaysIntoPast, simulationConfiguration.StepInterval,
                            cancellationToken);
                        if (!(profitHistory.LossPercentage < currentFittestProfit)) return;
                        currentFittestProfit = profitHistory.AvgProfitPerHour;
                        result =
                            await _profitCalculator.CalculateProductionStatisticForCompany(companyParam,
                                cancellationToken);
                        result.ProfitResultsLastTenDays = profitHistory;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(simulationConfiguration.OptimizationObjective),
                            simulationConfiguration.OptimizationObjective, null);
                }

                bestStatistics.Add(result);
                Console.WriteLine(
                    $"Iteration w. optimal profit {result.CalculationDuration + result.ProfitResultsLastTenDays?.CalcDuration}. Last profit {result.TotalProfitPerHour:F0} /h | AVG 10 Days: {result.ProfitResultsLastTenDays?.AvgProfitPerHour:F1} | Loss: {result.ProfitResultsLastTenDays.LossPercentage:F3} % "); //| AVG: {result.ProfitResultsLastTenDays?.AvgProfit:F1} | MAX {result.ProfitResultsLastTenDays?.MaxProfit:F1} | MIN {result.ProfitResultsLastTenDays?.MinProfit:F1}");
            });

        var bestStatisticResult = bestStatistics.ToList();

        stopWatch.Stop();
        Console.WriteLine($"Total duration for {simulationConfiguration.Generations} generations: {stopWatch.Elapsed}");
        return bestStatisticResult;
    }


    private static Dictionary<ResourceId, int> GenerateRandomResourceBuildingLevels(IList<ResourceId> resourceIds,
        Random random, int maxBuildingLevel, int maxBuildings)
    {
        if (resourceIds.Any())
            return resourceIds.ToDictionary(resourceId => resourceId,
                _ => random.Next(maxBuildingLevel + 1));

        var amount = random.Next(maxBuildings + 1);
        if (amount == 0)
            amount = 1;
        var resourceBuildingLevels = new Dictionary<ResourceId, int>();
        for (var i = 0; i < amount; i++)
        {
            var nextResource = random.NextEnum<ResourceId>(ResourceEnumValues);
            if (!NotSellableResourceIds.NotSellableResources.Contains(nextResource) &&
                nextResource != ResourceId.AerospaceResearch)
            {
                var randomBuildingLevel = random.Next(maxBuildingLevel + 1);
                if (randomBuildingLevel > 0) resourceBuildingLevels.TryAdd(nextResource, randomBuildingLevel);
            }
            else
            {
                i--;
            }
        }

        return resourceBuildingLevels;
    }
}

public enum OptimizationObjective
{
    MaxAvgOverLastXDays,
    MaxForLatestMarket,
    MinLossPercentageOverLastXDays,
    MaxAvgProfitOverLastXDaysAndMinLossPercentage
}