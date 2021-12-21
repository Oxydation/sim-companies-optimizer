using System.Text.Json.Serialization;

namespace SimCompaniesOptimizer.Models.ProfitCalculation;

public class Profit
{
    public double Value { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    
    [JsonIgnore]
    public ProductionStatistic ProductionStatistic { get; set; } // TODO: remove?
}