// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimCompaniesOptimizer;
using SimCompaniesOptimizer.APIs;
using SimCompaniesOptimizer.Calculations;
using SimCompaniesOptimizer.Database;
using SimCompaniesOptimizer.Extensions;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ProfitCalculation;
using SimCompaniesOptimizer.Optimization;
using SimCompaniesOptimizer.Utils;
using SimCompaniesOptimizer.Visualization;

Parser.Default.ParseArguments<ParameterOptions>(args).WithParsed(RunOptions);

static async void RunOptions(ParameterOptions options)
{
    Console.WriteLine($"Starting sim companies optimizer {Assembly.GetExecutingAssembly().GetName().Version}");

    var serviceCollection = new ServiceCollection();
    serviceCollection.AddLogging();
    var containerBuilder = new ContainerBuilder();
    containerBuilder.Populate(serviceCollection);
    containerBuilder.RegisterType<SimCompaniesApi>().As<ISimCompaniesApi>();
    containerBuilder.RegisterType<ExchangeTrackerApi>().As<IExchangeTrackerApi>();
    containerBuilder.RegisterType<ExchangeTrackerCache>().As<IExchangeTrackerCache>();
    containerBuilder.RegisterType<ExchangeTrackerReader>().As<IExchangeTrackerReader>();
    containerBuilder.RegisterType<SimCompaniesApi>().As<ISimCompaniesApi>();
    containerBuilder.RegisterType<ProfitOptimizer>().As<IProfitOptimizer>();
    containerBuilder.RegisterType<ProfitCalculator>().As<IProfitCalculator>();
    var container = containerBuilder.Build();
    var serviceProvider = new AutofacServiceProvider(container);

    await using (var dbContext = new SimCompaniesDbContext())
    {
        dbContext.Database.Migrate();
    }

    var simCompaniesApi = serviceProvider.GetService<ISimCompaniesApi>();
    var profitOptimizer = serviceProvider.GetService<IProfitOptimizer>();
    var profitCalculator = serviceProvider.GetService<IProfitCalculator>();
    var exchangeTrackerCache = serviceProvider.GetService<IExchangeTrackerCache>();

//await simCompaniesApi.GetAllResourcesAsync(CancellationToken.None);
//await simCompaniesApi.UpdateExchangePriceOfAllResources(CancellationToken.None);


    // new List<ResourceId>
    // {
    //     ResourceId.Minerals, ResourceId.Chemicals, ResourceId.Batteries, ResourceId.HighGradeEComps,
    //     ResourceId.GoldenBars, ResourceId.GoldOre, ResourceId.IonDrive
    // }

    if (options.ForceExchangeTrackerSync)
    {
        await exchangeTrackerCache.RefreshCache(CancellationToken.None);
        await simCompaniesApi.UpdateExchangePriceOfAllResources(CancellationToken.None);
    }

    var restarts = options.Restarts ?? 1;
    var optimizationRunsResults = new ConcurrentBag<ProductionStatistic>();
    var stopWatch = new Stopwatch();
    stopWatch.Start();
    var simulationParams = new SimulationConfiguration
    {
        CooOverheadReduction = 7,
        Generations = options.Generations,
        Seed = options.Seed,
        ContractSelection = options.UseContracts ? ContractSelection.Enable : ContractSelection.Disable,
        BuildingLevelLimit = options.MaxBuildingLevel,
        MaxBuildingPlaces = options.MaxBuildingPlaces,
        CalculateProfitHistoryForAllNewMaxProfits = true,
        OptimizationObjective = OptimizationObjective.MaxAvgOverLastXDays
    };

    Parallel.For(0, restarts, new ParallelOptions { MaxDegreeOfParallelism = options.CountConcurrentRuns ?? 1 },
        async (run, state) =>
        {
            Console.WriteLine($"Optimization run {run + 1} of {restarts}");

            var bestResults = await profitOptimizer.OptimalBuildingsRandom(
                options.Resources.Select(r => (ResourceId)r).ToList(), simulationParams, CancellationToken.None);
            
            var bestOfRun = bestResults.MaxByOptimizationObjective(simulationParams.OptimizationObjective);
            optimizationRunsResults.Add(bestOfRun);
            bestOfRun?.PrintToConsole(false);
            Console.WriteLine($"Optimization run {run + 1} finished.");
        });

    var veryBest = optimizationRunsResults.MaxByOptimizationObjective(simulationParams.OptimizationObjective);
    stopWatch.Stop();
    Console.WriteLine(
        $"{restarts} optimization runs with {options.Generations} generations finished within {stopWatch.Elapsed}. Total best profit per hour {veryBest.TotalProfitPerHour}");
    Console.WriteLine("Profit for best result over the last ten days.");
    Console.WriteLine(
        $"AVG: {veryBest.ProfitResultsLastTenDays?.AvgProfitPerHour:F1} | MAX {veryBest.ProfitResultsLastTenDays?.MaxProfitPerHour:F1} | MIN {veryBest.ProfitResultsLastTenDays?.MinProfitPerHour:F1}");

    var orderedResults = optimizationRunsResults.OrderByOptimizationObjective(simulationParams.OptimizationObjective).ToList();

    await using var fileStream =
        new StreamWriter($"{DateTime.Now.Ticks}_{simulationParams.OptimizationObjective}_{veryBest?.TotalProfitPerHour:F0}.json");
    var serializedResult = JsonSerializer.Serialize(orderedResults);
    fileStream.Write(serializedResult);
    fileStream.Close();
}