﻿using System.Collections.Concurrent;
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

    public async Task<List<ProductionStatistic>> OptimalBuildingsForGivenResourcesRandom(
        IList<ResourceId> resources, SimulationConfiguration simulationConfiguration,
        CancellationToken cancellationToken)
    {
        var usedSeed = simulationConfiguration.Seed ?? Environment.TickCount;
        //var random = new Random(usedSeed);


        var stopWatch = new Stopwatch();
        stopWatch.Start();

        double currentMaxProfit = 0;
        var bestStatistics = new ConcurrentBag<ProductionStatistic>();

        Parallel.For(0, simulationConfiguration.Generations, async (i, state) =>
        {
            var companyParam = new CompanyParameters
            {
                SimulationParameters = simulationConfiguration,
                CooOverheadReduction = 7,
                ProductionSpeed = 1.06,
                InputResourcesFromContracts = simulationConfiguration.ContractSelection == ContractSelection.Enable,
                MaxBuildingPlaces = simulationConfiguration.MaxBuildingPlaces,
                Seed = usedSeed,
                BuildingsPerResource =
                    GenerateRandomResourceBuildingLevels(resources, Random.Value,
                        simulationConfiguration.BuildingLevelLimit, simulationConfiguration.MaxBuildingPlaces)
            };
            if (companyParam.BuildingsPerResource.Count == 0) return;

            var result =
                await _profitCalculator.CalculateProductionStatisticForCompany(companyParam, cancellationToken);
            if (!(result.TotalProfitPerHour > currentMaxProfit)) return;

           
            currentMaxProfit = result.TotalProfitPerHour;
            bestStatistics.Add(result);
            // Console.WriteLine($"New max profit found {currentMaxProfit:F0}");
            Console.WriteLine(
                $"Iteration w. max. profit {result.CalculationDuration}. {result.TotalProfitPerHour:F0} /h"); //| AVG: {result.ProfitResultsLastTenDays?.AvgProfit:F1} | MAX {result.ProfitResultsLastTenDays?.MaxProfit:F1} | MIN {result.ProfitResultsLastTenDays?.MinProfit:F1}");
        });

        var bestStatisticResult = bestStatistics.ToList();
        // if (simulationConfiguration.CalculateProfitHistoryForAllNewMaxProfits)
        // {
        //     foreach (var productionStatistic in bestStatisticResult)
        //     {     var profitResultsForTheLastTenDays =
        //             await _profitCalculator.CalculateProductionStatisticForCompany(productionStatistic.CompanyParameters,
        //                 TimeSpan.FromDays(5), TimeSpan.FromMinutes(30),
        //                 cancellationToken);
        //         productionStatistic.ProfitResultsLastTenDays = profitResultsForTheLastTenDays;
        //         Console.WriteLine(
        //             $"Iteration w. max. profit {productionStatistic.CalculationDuration}. {productionStatistic.TotalProfitPerHour:F0} /h | AVG: {productionStatistic.ProfitResultsLastTenDays?.AvgProfit:F1} | MAX {productionStatistic.ProfitResultsLastTenDays?.MaxProfit:F1} | MIN {productionStatistic.ProfitResultsLastTenDays?.MinProfit:F1}");
        //
        //     }
        //
        // }

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
            if (!NotSellableResourceIds.NotSellableResources.Contains(nextResource))
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