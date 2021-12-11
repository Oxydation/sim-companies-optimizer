using System.Text.Json.Serialization;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Models;

public class Resource
{
    [JsonPropertyName("name")] public string Name { get; set; }

    // public int Id { get; set; }

    [JsonPropertyName("db_letter")] public ResourceId Id { get; set; }

    [JsonPropertyName("transportation")] public double Transportation { get; set; }

    [JsonPropertyName("producedAnHour")] public double ProducedAnHour { get; set; }

    [JsonPropertyName("baseSalary")] public double BaseSalary { get; set; }

    [JsonPropertyName("producedFrom")] public IEnumerable<InputResource> ProducedFrom { get; set; }

    [JsonIgnore] public double CurrentExchangePrice { get; set; }

    [JsonIgnore] public PriceCard? PriceCard { get; set; }
    //
    // [JsonIgnore]
    // public DateTimeOffset LastSync { get; set; }

    public double CalcUnitWorkerCost(double productionSpeed)
    {
        return BaseSalary / (ProducedAnHour * productionSpeed);
    }

    public double CalcUnitAdminCost(double adminOverhead, double productionSpeed)
    {
        return CalcUnitWorkerCost(productionSpeed) * adminOverhead / 100.0;
    }

    public override string ToString()
    {
        return $"{Id}, {PriceCard}";
    }
}