namespace SimCompaniesOptimizer.Models.ProfitCalculation;

public class ProfitHistory
{
    public List<Profit> Profits { get; set; }

    public double MaxProfit { get; set; }

    public double MinProfit { get; set; }

    public double AvgProfit { get; set; }
    public int CountIterationsWithLoss { get; set; }
    public int CountIterationsWithProfit { get; set; }
    public double LossPercentage { get; set; }
}