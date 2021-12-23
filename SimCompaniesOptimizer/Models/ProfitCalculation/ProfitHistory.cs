namespace SimCompaniesOptimizer.Models.ProfitCalculation;

public class ProfitHistory
{
    public List<Profit> Profits { get; set; }

    public double MaxProfitPerHour { get; set; }

    public double MinProfitPerHour { get; set; }

    public double AvgProfitPerHour { get; set; }
    public int CountIterationsWithLoss { get; set; }
    public int CountIterationsWithProfit { get; set; }
    public double LossPercentage { get; set; }

    public TimeSpan CalcDuration { get; set; }
}