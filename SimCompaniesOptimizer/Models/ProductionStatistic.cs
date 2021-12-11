namespace SimCompaniesOptimizer.Models;

public class ProductionStatistic
{
    public double TotalProfitPerHour { get; set; }
    public double TotalProfitPerDay => TotalProfitPerHour * TimeSpan.FromDays(1).TotalHours;
    public double TotalProfitPerWeek => TotalProfitPerHour * TimeSpan.FromDays(7).TotalHours;
    public double TotalProfitPerMonth => TotalProfitPerHour * TimeSpan.FromDays(30).TotalHours;

    public double TotalRevenuePerHour { get; set; }
    public double TotalExpensePerHour { get; set; }

    public CompanyParameters UsedCompanyParameters { get; set; } = new();

    public Dictionary<ResourceId, ResourceStatistic> ResourceStatistic { get; set; } = new();
}

public class ResourceStatistic
{
    public double UnusedUnits { get; set; }

    public double AmountProduced { get; set; }
    public double AmountBought => UnusedUnits < 0 ? -UnusedUnits : 0;
    public double TotalUnits => AmountProduced + AmountBought;

    public double AveragedSourcingCost { get; set; }
    public double PercentageContributionToProfit { get; set; }
    public double ProfitPerHour { get; set; }
    public double RevenuePerHour { get; set; }
    public double ExpensePerHour { get; set; }
}