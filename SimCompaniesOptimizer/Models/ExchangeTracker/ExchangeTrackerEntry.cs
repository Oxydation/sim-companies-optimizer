using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SimCompaniesOptimizer.Models.ExchangeTracker;

public class ExchangeTrackerEntry
{
    [JsonIgnore]
    private static readonly List<ResourceId> ResourceEnumValues = Enum.GetValues<ResourceId>().ToList();
    
    // [CsvHelper.Configuration.Attributes.Ignore]
    public string Empty { get; set; }
    public string Empty2 { get; set; }

    [Key] public DateTimeOffset? Timestamp { get; set; }

    public List<double?> ExchangePrices { get; set; }

    public double? GetPriceOfResource(ResourceId resourceId)
    {
        if (ExchangePrices.Count == 0)
        {
            return null;
        }
        return ExchangePrices[GetIndexOfResourceId(resourceId)];
    }

    public static int GetIndexOfResourceId(ResourceId resourceId)
    {
        if (NotSellableResourceIds.NotSellableResources.Contains(resourceId)) return -1;

        ResourceEnumValues.RemoveAll(x => NotSellableResourceIds.NotSellableResources.Contains(x));
        return ResourceEnumValues.IndexOf(resourceId);
    }
}