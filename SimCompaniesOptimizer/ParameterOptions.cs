using CommandLine;

namespace SimCompaniesOptimizer;

public class ParameterOptions
{
    [Option('g', "generations", HelpText = "Amount of generations to find optimum.", Required = true)]
    public int Generations { get; set; } = 30;
    
    [Option('t', "restarts", HelpText = "Amount of restarts with a new random seed. Defaulting to one run", Required = false)]
    public int? Restarts { get; set; } = 1;

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

    [Option('s', "seed",
        HelpText =
            "The seed to use for the random values. Default is Environment.TickCount.",
        Required = false)]
    public int? Seed { get; set; }
    
    [Option('e', "exsync",
        HelpText =
            "Forces to sync the exchange tracker with current values.",
        Required = false)]
    public bool ForceExchangeTrackerSync { get; set; }
}