namespace SimCompaniesOptimizer.Models.ExchangeTracker;

public class PriceCard
{
    public Price? Current { get; set; }
    public Price Average { get; set; }
    public Price Min { get; set; }
    public Price Max { get; set; }

    public override string ToString()
    {
        return $"{Current}, Avg: {Average}, Min: {Min}, Max: {Max}";
    }
}