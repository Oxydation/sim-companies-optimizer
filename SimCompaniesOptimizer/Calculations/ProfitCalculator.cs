using System.Diagnostics;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Calculations;

public class ProfitCalculator : IProfitCalculator
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
                companyParameters.InputResourcesFromContracts,
                cancellationToken);

            var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);
            var grossIncome = resourceStatistic.UnitsToSellPerHour * resource.CurrentExchangePrice;
            var exchangeFee = grossIncome * SimCompaniesConstants.ExchangeFee;
            resourceStatistic.RevenuePerHour = grossIncome;
            resourceStatistic.ExpensePerHour = exchangeFee + resourceStatistic.UnitsToSellPerHour *
                (resourceStatistic.AveragedSourcingCost +
                 +(resource.Transportation *
                   transportationResource.CurrentExchangePrice));

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
        ProductionStatistic productionStatistic, bool inputResourceFromContracts, CancellationToken cancellationToken)
    {
        var resource = await _simCompaniesApi.GetResourceAsync(resourceId, cancellationToken);

        double totalSourcingCost = 0;
        var inputItemSourcingCost = 0.0;
        foreach (var inputResource in resource.ProducedFrom)
        {
            // TODO: remember already visited branches
            if (!productionStatistic.ResourceStatistic.ContainsKey(inputResource.Resource.Id)) continue;
            var inputResourceId = inputResource.Resource.Id;
            var avgCost = await CalculateAvgSourcingCostRecursive(inputResourceId, productionStatistic,
                inputResourceFromContracts,
                cancellationToken);
            productionStatistic.ResourceStatistic.TryAdd(inputResourceId, new ResourceStatistic
            {
                AveragedSourcingCost = avgCost
            });
            inputItemSourcingCost += avgCost * inputResource.Amount;
        }

        var resourceStatistic = productionStatistic.ResourceStatistic[resourceId];
        totalSourcingCost += resourceStatistic.AmountBoughtPerHour * resource.CurrentExchangePrice *
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

        // If we have a building producing desired resource, calc amount
        if (companyParameters.BuildingsPerResource.ContainsKey(resourceId))
        {
            var buildingLevel = companyParameters.BuildingsPerResource[resourceId];
            var totalUnitsProducedPerHour = resource.ProducedAnHour * buildingLevel;
            resourceStatistic.ProductionBuildingLevels = buildingLevel;
            resourceStatistic.ExchangePrice = resource.CurrentExchangePrice;

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