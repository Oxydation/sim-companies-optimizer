using System.ComponentModel.DataAnnotations;

namespace SimCompaniesOptimizer.Models.ExchangeTracker;

public class ExchangeTrackerEntry
{
    // [CsvHelper.Configuration.Attributes.Ignore]
    public string Empty { get; set; }
    public string Empty2 { get; set; }

    [Key] public DateTimeOffset? Timestamp { get; set; }

    public List<double?> ExchangePrices { get; set; }
}