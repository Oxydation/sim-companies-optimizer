using CommandLine;

namespace SimCompaniesOptimizer;

public class ParameterOptions
{
    [Option('g', "generations", HelpText = "Amount of generations to find optimum.", Required = true)]
    public int Generations { get; set; } = 30;

    [Option('b', "maxbuildinglevel", HelpText = "The max building level of one resource. Default is 30.",
        Required = false)]
    public int MaxBuildingLevel { get; set; } = 30;

    [Option('r', "resources",
        HelpText = "The resources which can be produced. If not set, resources will be randomly selected",
        Required = false)]
    public IEnumerable<int> Resources { get; set; } = new List<int>();

    [Option('p', "maxbuildingplaces",
        HelpText =
            "The amount of building places available. Defines max. variations of produced resources. Default is 12.",
        Required = false)]
    public int MaxBuildingPlaces { get; set; } = 12;
}