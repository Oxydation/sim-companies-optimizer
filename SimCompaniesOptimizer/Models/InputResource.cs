using System.Text.Json.Serialization;

namespace SimCompaniesOptimizer.Models;

public class InputResource
{
    [JsonPropertyName("resource")] public Resource Resource { get; set; }

    [JsonPropertyName("amount")] public double Amount { get; set; }
}