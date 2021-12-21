using SimCompaniesOptimizer.Models;
using SimCompaniesOptimizer.Models.ProfitCalculation;

namespace SimCompaniesOptimizer.Calculations;

public interface IProfitCalculator
{
    Task<ProductionStatistic> CalculateProductionStatisticForCompany(CompanyParameters companyParameters,
        CancellationToken cancellationToken);

    Task<ProfitHistory> CalculateProductionStatisticForCompany(CompanyParameters companyParameters,
        TimeSpan timeSpanIntoPast, TimeSpan stepInterval, CancellationToken cancellationToken);
}