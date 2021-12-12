using System.Collections.Concurrent;
using System.Diagnostics;
using SimCompaniesOptimizer.Calculations;
using SimCompaniesOptimizer.Extensions;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Optimization;

public class ProfitOptimizer : IProfitOptimizer
{
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

    public async Task<List<ProductionStatistic>> OptimalBuildingsForGivenResourcesRandom(
        IEnumerable<ResourceId> resources,
        int generations, CancellationToken cancellationToken,
        int buildingLevelLimit = 30,
        int maxBuildingPlaces = 12,
        int? seed = null)
    {
        var usedSeed = seed ?? Environment.TickCount;
        var random = new Random(usedSeed);

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        double currentMaxProfit = 0;
        var bestProductionStatistic = new ProductionStatistic();
        var bestStatistics = new ConcurrentBag<ProductionStatistic>();

        // var pregeneratedCompanyParameters = new ConcurrentBag<CompanyParameters>();
        // Parallel.For(0, generations, (i, state) =>
        // {
        //     var companyParams = new CompanyParameters
        //     {
        //         CooOverheadReduction = 7,
        //         ProductionSpeed = 1.06,
        //         InputResourcesFromContracts = true,
        //         MaxBuildingPlaces = maxBuildingPlaces,
        //         Seed = usedSeed,
        //         BuildingsPerResource =
        //             GenerateRandomResourceBuildingLevels(resources, random, buildingLevelLimit, maxBuildingPlaces)
        //     };
        //
        //     pregeneratedCompanyParameters.Add(companyParams);
        // });

        // Console.WriteLine($"Pre-generated {generations} company parameters in {stopWatch.Elapsed}.");
        Parallel.For(0, generations, async (i, state) =>
        {
            var companyParam = new CompanyParameters
            {
                CooOverheadReduction = 7,
                ProductionSpeed = 1.06,
                InputResourcesFromContracts = true,
                MaxBuildingPlaces = maxBuildingPlaces,
                Seed = usedSeed,
                BuildingsPerResource =
                    GenerateRandomResourceBuildingLevels(resources, random, buildingLevelLimit, maxBuildingPlaces)
            };
            var result =
                await _profitCalculator.CalculateProductionStatisticForCompany(companyParam, cancellationToken);
            if (result.TotalProfitPerHour > currentMaxProfit)
            {
                currentMaxProfit = result.TotalProfitPerHour;
                bestProductionStatistic = result;
                bestStatistics.Add(result);
                // Console.WriteLine($"New max profit found {currentMaxProfit:F0}");
                Console.WriteLine(
                    $"Iteration with new max profit finished in {result.CalculationDuration}. {result.TotalProfitPerHour:F0} profit/h");
            }
        });

        stopWatch.Stop();
        Console.WriteLine($"Total duration for {generations} generations: {stopWatch.Elapsed}");
        return bestStatistics.ToList();
    }

    private static Dictionary<ResourceId, int> GenerateRandomResourceBuildingLevels(IEnumerable<ResourceId> resourceIds,
        Random random, int maxBuildingLevel, int maxBuildings)
    {
        var enumerable = resourceIds.ToList();
        if (enumerable.Any())
            return enumerable.ToDictionary(resourceId => resourceId,
                _ => (int)(random.NextDouble() * maxBuildingLevel));

        var amount = (int)(random.NextDouble() * maxBuildings);
        var selectedResources = new List<ResourceId>();
        for (var i = 0; i < amount; i++)
        {
            var nextResource = random.NextEnum<ResourceId>();
            if (!NotSellableResourceIds.NotSellableResources.Contains(nextResource))
                selectedResources.Add(nextResource);
            else
                i--;
        }

        return selectedResources.Distinct()
            .ToDictionary(resourceId => resourceId, _ => (int)(random.NextDouble() * maxBuildingLevel));
    }
}