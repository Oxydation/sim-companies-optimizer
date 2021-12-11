namespace SimCompaniesOptimizer.Models.ExchangeTracker;

public class Price
{
    public double? Value { get; set; }
    public DateTimeOffset? Timestamp { get; set; }

    public override string ToString()
    {
        return $"{Timestamp} ${Value}";
    }
}