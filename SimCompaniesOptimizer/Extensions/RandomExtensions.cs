namespace SimCompaniesOptimizer.Extensions;

public static class RandomExtensions
{
    public static T NextEnum<T>(this Random random)
    {
        var values = Enum.GetValues(typeof(T));
        return (T)values.GetValue(random.Next(values.Length));
    }

    public static T NextEnum<T>(this Random random, Array values)
    {
        return (T)values.GetValue(random.Next(values.Length));
    }
}