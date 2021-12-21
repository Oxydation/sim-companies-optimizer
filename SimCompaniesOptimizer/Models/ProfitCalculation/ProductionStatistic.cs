namespace SimCompaniesOptimizer.Models.ProfitCalculation;

public class ProductionStatistic
{
    public double TotalProfitPerHour { get; set; }
    public double TotalProfitPerDay => TotalProfitPerHour * TimeSpan.FromDays(1).TotalHours;
    public double TotalProfitPerWeek => TotalProfitPerHour * TimeSpan.FromDays(7).TotalHours;
    public double TotalProfitPerMonth => TotalProfitPerHour * TimeSpan.FromDays(30).TotalHours;

    public double TotalRevenuePerHour { get; set; }
    public double TotalExpensePerHour { get; set; }

    public CompanyParameters CompanyParameters { get; set; } = new();

    public Dictionary<ResourceId, ResourceStatistic> ResourceStatistic { get; set; } = new();

    public TimeSpan CalculationDuration { get; set; }
    public ProfitHistory ProfitResultsLastTenDays { get; set; }
}

public class ResourceStatistic
{
    public double ProductionBuildingLevels { get; set; }
    public double UnusedUnits { get; set; }

    public double AmountProducedPerHour { get; set; }

    public double AmountProducedPerDay => AmountProducedPerHour * TimeSpan.FromDays(1).TotalHours;
    public double AmountBoughtPerDay => AmountBoughtPerHour * TimeSpan.FromDays(1).TotalHours;

    public double AmountBoughtPerHour => UnusedUnits < 0 ? -UnusedUnits : 0;
    public double TotalUnitsPerHour => AmountProducedPerHour + AmountBoughtPerHour;

    public double UnitsToSellPerHour => UnusedUnits > 0 ? UnusedUnits : 0;
    public double UnitsToSellPerDay => UnitsToSellPerHour * TimeSpan.FromDays(1).TotalHours;
    public double AveragedSourcingCost { get; set; }

    public double ProfitPerHour => RevenuePerHour - ExpensePerHour;

    public double ProfitPerDay => ProfitPerHour * TimeSpan.FromDays(1).TotalHours;

    public double RevenuePerHour { get; set; }
    public double ExpensePerHour { get; set; }

    public double ExchangePrice { get; set; }
    
    public double PercentageOfProfit { get; set; }
}