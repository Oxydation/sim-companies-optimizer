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
using SimCompaniesOptimizer.Visualization;

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
    InputResourcesFromContracts = false,
    BuildingsPerResource = new Dictionary<ResourceId, int>
    {
        { ResourceId.IonDrive, 60 }
    }
};
var simCompaniesApi = serviceProvider.GetService<ISimCompaniesApi>();
//await simCompaniesApi.GetAllResourcesAsync(CancellationToken.None);

//await simCompaniesApi.UpdateExchangePriceOfAllResources(CancellationToken.None);
var calculator = new ProfitCalculator(simCompaniesApi);
var productionStatistic =
    await calculator.CalculateProductionStatisticForCompany(companyParameters,
        CancellationToken.None);

productionStatistic.PrintToConsole();