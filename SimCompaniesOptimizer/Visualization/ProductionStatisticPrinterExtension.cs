using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.Visualization;

public static class ProductionStatisticPrinterExtension
{
    public static void PrintToConsole(this ProductionStatistic productionStatistic)
    {
        Console.WriteLine("Printing simulation result ...");
        Console.WriteLine($"Duration: {productionStatistic.CalculationDuration}");
        Console.WriteLine(
            $"Total Buildings {productionStatistic.CompanyParameters.GetTotalBuildings()}, Admin Overhead: {productionStatistic.CompanyParameters.AdminOverhead:F2}");

        Console.WriteLine();
        Console.WriteLine("Summary");
        Console.WriteLine("Profit per Hour | Profit per Day | Profit per Week");
        Console.WriteLine(
            $"{productionStatistic.TotalProfitPerHour:F1} | {productionStatistic.TotalProfitPerDay:F1} | {productionStatistic.TotalProfitPerWeek:F1}");
        Console.WriteLine();
        Console.WriteLine("Detailed resource statistic");
        Console.WriteLine(
            "Name | Avg. Sourcing Cost | Building Lvls | Profit per Day |  Bought Daily | Produced Daily | Sold daily ");

        foreach (var (resourceId, resourceStatistic) in productionStatistic.ResourceStatistic)
            Console.WriteLine(
                $"{resourceId} | {resourceStatistic.AveragedSourcingCost:F1} | {resourceStatistic.ProductionBuildingLevels} | {resourceStatistic.ProfitPerDay:F1} | {resourceStatistic.AmountBoughtPerDay:F1} | {resourceStatistic.AmountProducedPerDay:F1} | {resourceStatistic.UnitsToSellPerDay:F1}");
    }
}