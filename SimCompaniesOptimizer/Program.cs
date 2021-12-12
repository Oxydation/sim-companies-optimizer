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
        { ResourceId.HighGradeEComps, 15 },
        { ResourceId.Minerals, 13 },
        { ResourceId.Chemicals, 6 }
    }
};
var simCompaniesApi = serviceProvider.GetService<ISimCompaniesApi>();
//await simCompaniesApi.GetAllResourcesAsync(CancellationToken.None);

//await simCompaniesApi.UpdateExchangePriceOfAllResources(CancellationToken.None);
var calculator = new ProfitCalculator(simCompaniesApi);
var productionStatistic =
    await calculator.CalculateProductionStatisticForCompany(companyParameters,
        CancellationToken.None);

// Console.WriteLine("Profit for 1 level per hour: " + profit);
Console.WriteLine(
    $"Company profit per day {+productionStatistic.TotalProfitPerDay}. Duration: {productionStatistic.CalculationDuration}");