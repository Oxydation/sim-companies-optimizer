using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Calculations;

public interface IProfitCalculator
{
    Task<ProductionStatistic> CalculateProductionStatisticForCompany(CompanyParameters companyParameters,
        CancellationToken cancellationToken);
}