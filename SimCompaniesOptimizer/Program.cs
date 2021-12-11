// See https://aka.ms/new-console-template for more information

using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimCompaniesOptimizer.APIs;
using SimCompaniesOptimizer.Calculations;
using SimCompaniesOptimizer.Database;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Utils;

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
var container = containerBuilder.Build();
var serviceProvider = new AutofacServiceProvider(container);

await using (var dbContext = new SimCompaniesDbContext())
{
    dbContext.Database.Migrate();
}

var companyParameters = new CompanyParameters
{
    CooOverheadReduction = 7,
    ProductionSpeed = 1.06,
    BuildingsPerResource = new Dictionary<ResourceId, int>
    {
        { ResourceId.Batteries, 15 },
        { ResourceId.HighGradeEComps, 15 }
    }
};
var simCompaniesApi = serviceProvider.GetService<ISimCompaniesApi>();
var testrse = await simCompaniesApi.GetResourceAsync(ResourceId.Batteries, CancellationToken.None);
await simCompaniesApi.GetResourceAsync(ResourceId.HighGradeEComps, CancellationToken.None);
await simCompaniesApi.GetResourceAsync(ResourceId.Chemicals, CancellationToken.None);
await simCompaniesApi.GetResourceAsync(ResourceId.GoldenBars, CancellationToken.None);
await simCompaniesApi.GetResourceAsync(ResourceId.Transport, CancellationToken.None);
await simCompaniesApi.GetResourceAsync(ResourceId.Silicon, CancellationToken.None);

// await simCompaniesApi.GetAllResourcesAsync(CancellationToken.None, TimeSpan.FromSeconds(5));
await simCompaniesApi.UpdateExchangePriceOfResource(ResourceId.Batteries, CancellationToken.None);
await simCompaniesApi.UpdateExchangePriceOfResource(ResourceId.Chemicals, CancellationToken.None);
await simCompaniesApi.UpdateExchangePriceOfResource(ResourceId.Silicon, CancellationToken.None);
await simCompaniesApi.UpdateExchangePriceOfResource(ResourceId.GoldenBars, CancellationToken.None);
await simCompaniesApi.UpdateExchangePriceOfResource(ResourceId.Transport, CancellationToken.None);
await simCompaniesApi.UpdateExchangePriceOfResource(ResourceId.HighGradeEComps, CancellationToken.None);

var calculator = new ProfitCalculator(simCompaniesApi);
// var profit = await
//     calculator.CalculateProfitOfResourceByMarket(ResourceId.Batteries, companyParameters, CancellationToken.None);

var productionStatistic =
    await calculator.CalculateProductionStatisticForCompany(companyParameters,
        CancellationToken.None);
Console.WriteLine(testrse);
// Console.WriteLine("Profit for 1 level per hour: " + profit);
Console.WriteLine("Company profit per day " + productionStatistic);