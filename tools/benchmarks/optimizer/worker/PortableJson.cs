using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ara3D.StepBrep;
using Type = System.Type;

namespace Plato.OptimizerBenchmark;

public static class PortableJson
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };
        options.Converters.Add(new GeneratedConverterFactory());
        return options;
    }

    public static byte[] Bytes<T>(T value)
        => JsonSerializer.SerializeToUtf8Bytes(value, Options);

    public static string Hash(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    sealed class GeneratedConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type type)
            => type.Assembly == typeof(BrepModel).Assembly && type.IsValueType && !type.IsEnum
                && !(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Length == 0
                    && type.GetCustomAttribute<JsonConverterAttribute>() is not null);

        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
            => (JsonConverter)Activator.CreateInstance(typeof(GeneratedConverter<>).MakeGenericType(type))!;
    }

    // Generated sums have private all-fields constructors. Ordinary deserialization can
    // silently default their discriminant or wrapper values when constructor binding changes.
    sealed class GeneratedConverter<T> : JsonConverter<T> where T : struct
    {
        public GeneratedConverter() { }

        static readonly FieldInfo[] Fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(f => f.Name, StringComparer.Ordinal).ToArray();
        static readonly ConstructorInfo Constructor = FindConstructor();

        static ConstructorInfo FindConstructor()
        {
            var matches = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(c => c.GetParameters().Length == Fields.Length)
                .Where(c => Enumerable.All(c.GetParameters(), p => Enumerable.Any(Fields,
                    f => string.Equals(f.Name, p.Name, StringComparison.OrdinalIgnoreCase) && f.FieldType == p.ParameterType)))
                .ToArray();
            if (matches.Length != 1)
                throw new NotSupportedException($"Portable fixture type {typeof(T).FullName} has {matches.Length} unambiguous field constructors; "
                    + $"expected one covering public fields [{string.Join(", ", Fields.Select(f => f.Name + ":" + f.FieldType.FullName))}]. "
                    + "Types with private storage require an explicit JsonConverterAttribute.");
            return matches[0];
        }

        public override T Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var values = Fields.ToDictionary(f => f.Name,
                f => document.RootElement.GetProperty(f.Name).Deserialize(f.FieldType, options), StringComparer.OrdinalIgnoreCase);
            return (T)Constructor.Invoke(Constructor.GetParameters().Select(p => values[p.Name!]).ToArray());
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var field in Fields)
            {
                writer.WritePropertyName(field.Name);
                JsonSerializer.Serialize(writer, field.GetValue(value), field.FieldType, options);
            }
            writer.WriteEndObject();
        }
    }
}
