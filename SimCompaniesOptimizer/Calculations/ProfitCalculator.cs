using System.Collections.Concurrent;
using System.Diagnostics;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ExchangeTracker;
using SimCompaniesOptimizer.Models.ProfitCalculation;

namespace SimCompaniesOptimizer.Calculations;

public class ProfitCalculator : IProfitCalculator
{
    private readonly IExchangeTrackerCache _exchangeTrackerCache;
    private readonly ISimCompaniesApi _simCompaniesApi;

    public ProfitCalculator(ISimCompaniesApi simCompaniesApi, IExchangeTrackerCache exchangeTrackerCache)
    {
        _simCompaniesApi = simCompaniesApi;
        _exchangeTrackerCache = exchangeTrackerCache;
    }


    public async Task<ProductionStatistic> CalculateProductionStatisticForCompany(CompanyParameters companyParameters,
        CancellationToken cancellationToken)
    {
        return await CalculateProductionStatisticForCompany(companyParameters, null, cancellationToken);
    }

    public async Task<ProfitHistory> CalculateProfitHistoryForCompany(CompanyParameters companyParameters,
        TimeSpan timeSpanIntoPast,
        TimeSpan stepInterval, CancellationToken cancellationToken)
    {
        var exchangeTrackerEntries = await _exchangeTrackerCache.GetEntries(cancellationToken);
        // TOdo get given timespan and only by interval: https://newbedev.com/linq-aggregate-and-group-by-periods-of-time

        var result = new ProfitHistory();
        var profits = new ConcurrentBag<Profit>();

        Parallel.ForEach(exchangeTrackerEntries.Where(x => x.Timestamp.HasValue), async (entry, state) =>
        {
            var productionStatistic =
                await CalculateProductionStatisticForCompany(companyParameters, entry, cancellationToken);
            profits.Add(new Profit
            {
                Timestamp = entry.Timestamp.Value,
                Value = productionStatistic.TotalProfitPerHour
            });
        });

        result.Profits = profits.ToList();
        result.AvgProfit = result.Profits.Average(x => x.Value);
        result.MaxProfit = result.Profits.Max(x => x.Value);
        result.MinProfit = result.Profits.Min(x => x.Value);
        result.CountIterationsWithLoss = result.Profits.Count(x => x.Value <= 0);
        result.CountIterationsWithProfit = result.Profits.Count(x => x.Value > 0);
        result.LossPercentage = result.CountIterationsWithLoss * 1.0 / result.Profits.Count * 100;
        return result;
    }

    private static double GetExchangePriceOfResource(ResourceId resourceId, Resource resource,
        ExchangeTrackerEntry? exchangeTrackerEntry)
    {
        return exchangeTrackerEntry?.GetPriceOfResource(resourceId) ?? resource.CurrentExchangePrice;
    }

    private async Task<ProductionStatistic> CalculateProductionStatisticForCompany(CompanyParameters companyParameters,
        ExchangeTrackerEntry exchangeTrackerEntry, CancellationToken cancellationToken)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var transportationResource = await _simCompaniesApi.GetResourceAsync(ResourceId.Transport, cancellationToken);

        var productionStatistic = new ProductionStatistic
        {
            CompanyParameters = companyParameters
        };
        // Calculate amount produced and used resources
        foreach (var (resourceId, buildingCount) in companyParameters.BuildingsPerResource)
            await SimulateResourceProductionRecursive(resourceId, companyParameters, productionStatistic,
                exchangeTrackerEntry,
                cancellationToken);

        double totalProfitPerHour = 0;
        double totalRevenuePerHour = 0;
        double totalExpensesPerHour = 0;

        // Calc avg sourcing cost for each used resource, revenue, expenses and profits
        foreach (var (resourceId, resourceStatistic) in productionStatistic.ResourceStatistic)
        {
            resourceStatistic.AveragedSourcingCost = await CalculateAvgSourcingCostRecursive(resourceId,
                productionStatistic,
                companyParameters.InputResourcesFromContracts,
                exchangeTrackerEntry,
                cancellationToken);

            var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);
            var grossIncome = resourceStatistic.UnitsToSellPerHour *
                              GetExchangePriceOfResource(resourceId, resource, exchangeTrackerEntry);
            var exchangeFee = grossIncome * SimCompaniesConstants.ExchangeFee;
            resourceStatistic.RevenuePerHour = grossIncome;
            resourceStatistic.ExpensePerHour = exchangeFee + resourceStatistic.UnitsToSellPerHour *
                (resourceStatistic.AveragedSourcingCost +
                 +(resource.Transportation *
                   GetExchangePriceOfResource(transportationResource.Id, transportationResource,
                       exchangeTrackerEntry)));

            totalProfitPerHour += resourceStatistic.ProfitPerHour;
            totalRevenuePerHour += resourceStatistic.RevenuePerHour;
            totalExpensesPerHour += resourceStatistic.ExpensePerHour;

            // the avg cost of the products to sell includes all previous expenses for the product. so you cannot accumulate those 
        }

        productionStatistic.TotalProfitPerHour = totalProfitPerHour;
        productionStatistic.TotalRevenuePerHour = totalRevenuePerHour;
        productionStatistic.TotalExpensePerHour = totalExpensesPerHour;

        stopWatch.Stop();
        productionStatistic.CalculationDuration = stopWatch.Elapsed;
        return productionStatistic;
    }

    private async Task<double> CalculateAvgSourcingCostRecursive(ResourceId resourceId,
        ProductionStatistic productionStatistic, bool inputResourceFromContracts,
        ExchangeTrackerEntry exchangeTrackerEntry, CancellationToken cancellationToken)
    {
        var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);

        double totalSourcingCost = 0;
        var inputItemSourcingCost = 0.0;
        foreach (var inputResource in resource.ProducedFrom)
        {
            if (!productionStatistic.ResourceStatistic.ContainsKey(inputResource.Resource.Id)) continue;
            var inputResourceId = inputResource.Resource.Id;
            var avgCost = await CalculateAvgSourcingCostRecursive(inputResourceId, productionStatistic,
                inputResourceFromContracts,
                exchangeTrackerEntry,
                cancellationToken);
            productionStatistic.ResourceStatistic.TryAdd(inputResourceId, new ResourceStatistic
            {
                AveragedSourcingCost = avgCost
            });
            inputItemSourcingCost += avgCost * inputResource.Amount;
        }

        var resourceStatistic = productionStatistic.ResourceStatistic[resourceId];
        totalSourcingCost += resourceStatistic.AmountBoughtPerHour *
                             GetExchangePriceOfResource(resourceId, resource, exchangeTrackerEntry) *
                             (inputResourceFromContracts ? 1 - SimCompaniesConstants.ExchangeFee : 1);
        totalSourcingCost += resourceStatistic.AmountProducedPerHour *
                             (resource.CalcUnitAdminCost(productionStatistic.CompanyParameters.AdminOverhead,
                                  productionStatistic.CompanyParameters.ProductionSpeed) +
                              resource.CalcUnitWorkerCost(productionStatistic.CompanyParameters.ProductionSpeed) +
                              inputItemSourcingCost);

        var averageCalculation = totalSourcingCost / resourceStatistic.TotalUnitsPerHour;
        if (double.IsNaN(averageCalculation)) return 0;

        return averageCalculation;
    }

    private async Task SimulateResourceProductionRecursive(ResourceId resourceId,
        CompanyParameters companyParameters, ProductionStatistic productionStatistic,
        ExchangeTrackerEntry exchangeTrackerEntry,
        CancellationToken cancellationToken)
    {
        var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);

        foreach (var inputResource in resource.ProducedFrom)
        {
            var inputResourceId = inputResource.Resource.Id;
            if (!productionStatistic.ResourceStatistic.ContainsKey(inputResourceId) &&
                companyParameters.BuildingsPerResource.ContainsKey(resourceId))
                await SimulateResourceProductionRecursive(inputResourceId, companyParameters, productionStatistic,
                    exchangeTrackerEntry,
                    cancellationToken);
        }

        var resourceStatistic = new ResourceStatistic();
        if (productionStatistic.ResourceStatistic.ContainsKey(resourceId))
            resourceStatistic = productionStatistic.ResourceStatistic[resourceId];

        // If we have a building producing desired resource, calc amount
        if (companyParameters.BuildingsPerResource.TryGetValue(resourceId, out var buildingLevel) && buildingLevel > 0)
        {
            var totalUnitsProducedPerHour = resource.ProducedAnHour * buildingLevel;
            resourceStatistic.ProductionBuildingLevels = buildingLevel;
            resourceStatistic.ExchangePrice = GetExchangePriceOfResource(resourceId, resource, exchangeTrackerEntry);

            if (resourceStatistic.AmountProducedPerHour == 0 && resourceStatistic.UnusedUnits == 0)
            {
                resourceStatistic.AmountProducedPerHour += totalUnitsProducedPerHour;
                resourceStatistic.UnusedUnits += totalUnitsProducedPerHour;


                foreach (var inputResource in resource.ProducedFrom)
                {
                    var inputResourceStatistic = productionStatistic.ResourceStatistic[inputResource.Resource.Id];
                    var inputUnitsNeededPerHour = inputResource.Amount * totalUnitsProducedPerHour;

                    inputResourceStatistic.UnusedUnits -= inputUnitsNeededPerHour;
                }
            }
        }

        productionStatistic.ResourceStatistic[resourceId] = resourceStatistic;
    }
}