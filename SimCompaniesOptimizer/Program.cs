// See https://aka.ms/new-console-template for more information

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
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;
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
    var exchangeTrackerCache = serviceProvider.GetService<IExchangeTrackerCache>();

//await simCompaniesApi.GetAllResourcesAsync(CancellationToken.None);
//await simCompaniesApi.UpdateExchangePriceOfAllResources(CancellationToken.None);

// var productionStatistic =
    //  await calculator.CalculateProductionStatisticForCompany(companyParameters,
    //    CancellationToken.None);

// var bestResult = await profitOptimizer.OptimalBuildingLevelForHorizontalProduction(ResourceId.IonDrive, CancellationToken.None);

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
    
    var restarts = (options.Restarts ?? 1);
    for (var run = 0; run < restarts; run++)
    {
        Console.WriteLine($"Optimization run {run} of {restarts}");
        
        var bestResults = await profitOptimizer.OptimalBuildingsForGivenResourcesRandom(
            options.Resources.Select(r => (ResourceId)r), options.Generations, CancellationToken.None,
            maxBuildingPlaces: options.MaxBuildingPlaces, seed: options.Seed);
        var veryBest = bestResults.MaxBy(b => b.TotalProfitPerHour);
        veryBest?.PrintToConsole();

        await using var fileStream =
            new StreamWriter($"{DateTime.Now.Ticks}_bestResults_{veryBest?.TotalProfitPerHour:F0}.json");
        var serializedResult = JsonSerializer.Serialize(bestResults);
        fileStream.Write(serializedResult);
        fileStream.Close();
    }
   
}