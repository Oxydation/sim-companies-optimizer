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
        var productionStatistic = new ProductionStatistic
        {
            UsedCompanyParameters = companyParameters
        };
        //
        // double totalProfitPerHour = 0;
        // double totalRevenuePerHour = 0;
        // double totalExpensesPerHour = 0;
        foreach (var (resourceId, buildingCount) in companyParameters.BuildingsPerResource)
            await SimulateResourceProductionRecursive(resourceId, companyParameters, productionStatistic,
                cancellationToken);

        // var resourceStatistic = 
        // if (!productionStatistic.ResourceStatistic.TryAdd(resourceId, resourceStatistic))
        // {
        //     productionStatistic.ResourceStatistic[resourceId] = resourceStatistic;
        // }
        //
        // totalProfitPerHour += resourceStatistic.ProfitPerHour;
        // totalRevenuePerHour += resourceStatistic.RevenuePerHour;
        // totalExpensesPerHour += resourceStatistic.ExpensePerHour;

        // productionStatistic.TotalProfitPerHour = totalProfitPerHour;
        // productionStatistic.TotalRevenuePerHour = totalRevenuePerHour;
        // productionStatistic.TotalExpensePerHour = totalExpensesPerHour;
        return productionStatistic;
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

        var transportationResource = await _simCompaniesApi.GetResourceAsync(ResourceId.Transport, cancellationToken);
        double sourcingCost = 0;
        var exchangePrice = resource.CurrentExchangePrice;

        // If we have a building producing desired resource, calc amount
        if (companyParameters.BuildingsPerResource.ContainsKey(resourceId))
        {
            var buildingLevel = companyParameters.BuildingsPerResource[resourceId];
            var totalUnitsProducedPerHour = resource.ProducedAnHour * buildingLevel;
            resourceStatistic.AmountProduced = totalUnitsProducedPerHour;
            resourceStatistic.UnusedUnits = totalUnitsProducedPerHour;
            //sourcingCost = resource.CalcUnitAdminCost(companyParameters.AdminOverhead, companyParameters.ProductionSpeed) + resource.CalcUnitWorkerCost(companyParameters.ProductionSpeed);


            foreach (var inputResource in resource.ProducedFrom)
            {
                var res = await _simCompaniesApi.GetResourceAsync(inputResource.Resource.Id, cancellationToken);
                var inputResourceStatistic = productionStatistic.ResourceStatistic[inputResource.Resource.Id];
                var inputUnitsNeededPerHour = inputResource.Amount * resource.ProducedAnHour;

                inputResourceStatistic.UnusedUnits -= inputUnitsNeededPerHour;

                // sourcingCost += inputResourceStatistic.UnusedUnits<0 ? res.CurrentExchangePrice * inputResourceStatistic.AmountProduced * inputResource.Amount;
            }
        }

        // var profitPerUnit = exchangePrice - (exchangePrice * SimCompaniesConstants.ExchangeFee) -
        //                     resource.Transportation * transportationResource.CurrentExchangePrice - sourcingCost - resource.CalcUnitAdminCost(companyParameters.AdminOverhead, companyParameters.ProductionSpeed) - resource.CalcUnitWorkerCost(companyParameters.ProductionSpeed);
        //
        // resourceStatistic.ProfitPerHour = profitPerUnit;
        //     
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