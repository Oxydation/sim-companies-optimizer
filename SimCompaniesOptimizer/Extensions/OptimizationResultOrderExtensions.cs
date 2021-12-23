using SimCompaniesOptimizer.Models.ProfitCalculation;
using SimCompaniesOptimizer.Optimization;

namespace SimCompaniesOptimizer.Extensions;

public static class OptimizationResultOrderExtensions
{
    public static IEnumerable<ProductionStatistic> OrderByOptimizationObjective(
        this IEnumerable<ProductionStatistic> results,
        OptimizationObjective optimizationObjective)
    {
        switch (optimizationObjective)
        {
            case OptimizationObjective.MaxAvgOverLastXDays:
                return results.OrderByDescending(p => p.ProfitResultsLastTenDays.AvgProfitPerHour);
                break;
            case OptimizationObjective.MaxForLatestMarket:
                return results.OrderByDescending(p => p.TotalProfitPerDay);
                break;
            case OptimizationObjective.MinLossPercentageOverLastXDays:
                return results.OrderBy(p => p.ProfitResultsLastTenDays.LossPercentage);
                break;
            case OptimizationObjective.MaxAvgProfitOverLastXDaysAndMinLossPercentage:
                return results.OrderBy(p => p.ProfitResultsLastTenDays.LossPercentage)
                    .ThenByDescending(p => p.ProfitResultsLastTenDays.AvgProfitPerHour);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(optimizationObjective), optimizationObjective, null);
        }
    }

    public static ProductionStatistic MaxByOptimizationObjective(this IEnumerable<ProductionStatistic> results,
        OptimizationObjective optimizationObjective)
    {
        if (results == null || !results.Any()) return null;
        return optimizationObjective switch
        {
            OptimizationObjective.MaxAvgOverLastXDays =>
                results.MaxBy(p => p.ProfitResultsLastTenDays.AvgProfitPerHour),
            OptimizationObjective.MaxForLatestMarket => results.MaxBy(p => p.TotalProfitPerDay),
            OptimizationObjective.MinLossPercentageOverLastXDays => results.MaxBy(p =>
                p.ProfitResultsLastTenDays.LossPercentage),
            OptimizationObjective.MaxAvgProfitOverLastXDaysAndMinLossPercentage => results.MaxBy(p =>
                p.ProfitResultsLastTenDays.AvgProfitPerHour),
            _ => throw new ArgumentOutOfRangeException(nameof(optimizationObjective), optimizationObjective, null)
        };
    }
}