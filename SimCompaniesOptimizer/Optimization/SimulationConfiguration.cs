using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Optimization;

public class SimulationConfiguration
{
    public double CooOverheadReduction { get; set; } = 0;

    public int Generations { get; set; } = 1000;
    public int BuildingLevelLimit { get; set; } = 30;

    public ContractSelection ContractSelection { get; set; } = ContractSelection.Enable;
    public int MaxBuildingPlaces { get; set; } = 12;
    public int? Seed { get; set; }
    public bool CalculateProfitHistoryForAllNewMaxProfits { get; set; }
}