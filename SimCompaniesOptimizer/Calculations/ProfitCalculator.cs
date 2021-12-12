using System.Diagnostics;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Calculations;

public class ProfitCalculator
{
    private readonly ISimCompaniesApi _simCompaniesApi;

    public ProfitCalculator(ISimCompaniesApi simCompaniesApi)
    {
        _simCompaniesApi = simCompaniesApi;
    }

    public async Task<ProductionStatistic> CalculateProductionStatisticForCompany(CompanyParameters companyParameters,
        CancellationToken cancellationToken)
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
                cancellationToken);


        double totalProfitPerHour = 0;
        double totalRevenuePerHour = 0;
        double totalExpensesPerHour = 0;

        // Calc avg sourcing cost for each used resource, revenue, expenses and profits
        foreach (var (resourceId, resourceStatistic) in productionStatistic.ResourceStatistic)
        {
            resourceStatistic.AveragedSourcingCost = await CalculateAvgSourcingCostRecursive(resourceId,
                productionStatistic,
                cancellationToken);

            var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);
            var bruttoIncome = resourceStatistic.UnitsToSell * resource.CurrentExchangePrice;
            var exchangeFee = bruttoIncome * SimCompaniesConstants.ExchangeFee;
            resourceStatistic.RevenuePerHour = bruttoIncome;
            resourceStatistic.ExpensePerHour = resourceStatistic.AveragedSourcingCost * resourceStatistic.TotalUnits +
                                               exchangeFee + resourceStatistic.UnitsToSell * (resource.Transportation *
                                                   transportationResource.CurrentExchangePrice);

            totalProfitPerHour += resourceStatistic.ProfitPerHour;
            totalRevenuePerHour += resourceStatistic.RevenuePerHour;
            totalExpensesPerHour += resourceStatistic.ExpensePerHour;
        }

        productionStatistic.TotalProfitPerHour = totalProfitPerHour;
        productionStatistic.TotalRevenuePerHour = totalRevenuePerHour;
        productionStatistic.TotalExpensePerHour = totalExpensesPerHour;

        stopWatch.Stop();
        productionStatistic.CalculationDuration = stopWatch.Elapsed;
        return productionStatistic;
    }

    private async Task<double> CalculateAvgSourcingCostRecursive(ResourceId resourceId,
        ProductionStatistic productionStatistic, CancellationToken cancellationToken)
    {
        var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);

        if (resourceId == ResourceId.IonDrive)
        {
        }

        double totalSourcingCost = 0;
        var inputItemSourcingCost = 0.0;
        foreach (var inputResource in resource.ProducedFrom)
        {
            // TODO: remember already visited branches
            if (!productionStatistic.ResourceStatistic.ContainsKey(inputResource.Resource.Id)) continue;
            var inputResourceId = inputResource.Resource.Id;
            var avgCost = await CalculateAvgSourcingCostRecursive(inputResourceId, productionStatistic,
                cancellationToken);
            productionStatistic.ResourceStatistic.TryAdd(inputResourceId, new ResourceStatistic()
            {
                AveragedSourcingCost = avgCost
            });
            inputItemSourcingCost += avgCost * inputResource.Amount;
        }

        var resourceStatistic = productionStatistic.ResourceStatistic[resourceId];
        totalSourcingCost += resourceStatistic.AmountBought * resource.CurrentExchangePrice;
        totalSourcingCost += resourceStatistic.AmountProduced *
                             (resource.CalcUnitAdminCost(productionStatistic.CompanyParameters.AdminOverhead,
                                  productionStatistic.CompanyParameters.ProductionSpeed) +
                              resource.CalcUnitWorkerCost(productionStatistic.CompanyParameters.ProductionSpeed) +
                              inputItemSourcingCost);

        return totalSourcingCost / resourceStatistic.TotalUnits;
    }

    public async Task SimulateResourceProductionRecursive(ResourceId resourceId,
        CompanyParameters companyParameters, ProductionStatistic productionStatistic,
        CancellationToken cancellationToken)
    {
        var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);

        foreach (var inputResource in resource.ProducedFrom)
        {
            var inputResourceId = inputResource.Resource.Id;
            if (!productionStatistic.ResourceStatistic.ContainsKey(inputResourceId) &&
                companyParameters.BuildingsPerResource.ContainsKey(resourceId))
                await SimulateResourceProductionRecursive(inputResourceId, companyParameters, productionStatistic,
                    cancellationToken);
        }

        var resourceStatistic = new ResourceStatistic();
        if (productionStatistic.ResourceStatistic.ContainsKey(resourceId))
            resourceStatistic = productionStatistic.ResourceStatistic[resourceId];

        double sourcingCost = 0;

        // If we have a building producing desired resource, calc amount
        if (companyParameters.BuildingsPerResource.ContainsKey(resourceId))
        {
            var buildingLevel = companyParameters.BuildingsPerResource[resourceId];
            var totalUnitsProducedPerHour = resource.ProducedAnHour * buildingLevel;

            if (resourceStatistic.AmountProduced == 0 && resourceStatistic.UnusedUnits == 0)
            {
                resourceStatistic.AmountProduced += totalUnitsProducedPerHour;
                resourceStatistic.UnusedUnits += totalUnitsProducedPerHour;
            }

            foreach (var inputResource in resource.ProducedFrom)
            {
                var inputResourceStatistic = productionStatistic.ResourceStatistic[inputResource.Resource.Id];
                var inputUnitsNeededPerHour = inputResource.Amount * resource.ProducedAnHour;

                inputResourceStatistic.UnusedUnits -= inputUnitsNeededPerHour;
            }
        }
        
        productionStatistic.ResourceStatistic[resourceId] = resourceStatistic;
    }

    // /// <summary>
    // /// Calculates the profits for a resource if buying input items from market only. For one hour and a single level 1 building.
    // /// </summary>
    // /// <param name="resourceId"></param>
    // /// <param name="companyParameters"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns></returns>
    // public async Task<ResourceStatistic> CalculateProfitOfResourceByMarket(ResourceId resourceId, CompanyParameters companyParameters, CancellationToken cancellationToken)
    // {
    //     var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);
    //     var transportationResource = await _simCompaniesApi.GetResourceAsync(ResourceId.Transport, cancellationToken);
    //     double sourcingCost = 0;
    //     var exchangePrice = resource.CurrentExchangePrice;
    //
    //     foreach (var inputResource in resource.ProducedFrom)
    //     {
    //         var res = await _simCompaniesApi.GetResourceAsync(inputResource.Resource.Id, cancellationToken);
    //         sourcingCost += res.CurrentExchangePrice * inputResource.Amount;
    //     }
    //
    //     var profitPerUnit = exchangePrice - (exchangePrice * SimCompaniesConstants.ExchangeFee) -
    //                         resource.Transportation * transportationResource.CurrentExchangePrice - sourcingCost - resource.CalcUnitAdminCost(companyParameters.AdminOverhead, companyParameters.ProductionSpeed) - resource.CalcUnitWorkerCost(companyParameters.ProductionSpeed);
    //
    //     return profitPerUnit * resource.ProducedAnHour;
    // }
}