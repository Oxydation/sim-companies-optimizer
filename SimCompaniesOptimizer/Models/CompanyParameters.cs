namespace SimCompaniesOptimizer.Models;

public class CompanyParameters
{
    public double CooOverheadReduction { get; set; } = 0;

    public double AdminOverhead =>
        GetTotalBuildings() * 100 * SimCompaniesConstants.AdminOverheadFactor - CooOverheadReduction;

    public double ProductionSpeed { get; set; }

    public Dictionary<ResourceId, int> BuildingsPerResource { get; set; } = new();

    public bool InputResourcesFromContracts { get; set; }

    public int MaxBuildingPlaces { get; set; }

    public int GetTotalBuildings()
    {
        return BuildingsPerResource.Sum(x => x.Value);
    }
}