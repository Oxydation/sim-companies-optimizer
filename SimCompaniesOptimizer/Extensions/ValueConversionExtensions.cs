using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SimCompaniesOptimizer.Extensions;

public static class ValueConversionExtensions
{
    public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class
    {
        var converter = new ValueConverter<T, string>
        (
            v => JsonSerializer.Serialize(v,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
            v => JsonSerializer.Deserialize<T>(v,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })
        );

        var comparer = new ValueComparer<T>
        (
            (l, r) => JsonSerializer.Serialize(l, new JsonSerializerOptions()) ==
                      JsonSerializer.Serialize(r, new JsonSerializerOptions()),
            v => v == null ? 0 : JsonSerializer.Serialize(v, new JsonSerializerOptions()).GetHashCode(),
            v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                new JsonSerializerOptions())
        );

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueConverter(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        propertyBuilder.HasColumnType("jsonb");

        return propertyBuilder;
    }
}