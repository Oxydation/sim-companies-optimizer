using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SimCompaniesOptimizer.Models.ExchangeTracker;

public class ExchangeTrackerEntry
{
    [JsonIgnore] private static readonly List<ResourceId> ResourceEnumValues = Enum.GetValues<ResourceId>().ToList();

    // [CsvHelper.Configuration.Attributes.Ignore]
    [NotMapped] public string Empty { get; set; }

    [NotMapped] public string Empty2 { get; set; }

    [Key] public DateTimeOffset? Timestamp { get; set; }

    public List<double?> ExchangePrices { get; set; }

    public double? GetPriceOfResource(ResourceId resourceId)
    {
        if (GetIndexOfResourceId(resourceId) >= ExchangePrices.Count || GetIndexOfResourceId(resourceId) < 0)
            Console.WriteLine("Error");

        return ExchangePrices.Count == 0 ? null : ExchangePrices[GetIndexOfResourceId(resourceId)];
    }

    public static int GetIndexOfResourceId(ResourceId resourceId)
    {
        if (NotSellableResourceIds.NotSellableResources.Contains(resourceId)) return -1;

        // TODO: problem: Aearospace reasearch can be sold but for production it needs unsellable stuff.
        ResourceEnumValues.RemoveAll(x => NotSellableResourceIds.NotSellableResources.Contains(x));
        return ResourceEnumValues.IndexOf(resourceId);
    }
}