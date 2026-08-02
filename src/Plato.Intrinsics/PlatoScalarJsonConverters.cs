using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ara3D.Geometry
{
    // System.Text.Json serializes a type by walking its PROPERTIES, which is wrong for the five
    // scalar wrappers twice over: the wrapped value is a field, and the interface members spelled as
    // properties on the handwritten half (Number.Hash returns an Integer, whose Hash returns an
    // Integer, ...) make the walk infinitely deep. Each wrapper is one JSON scalar, so each gets a
    // converter that says exactly that — matching what the generated ToJson writes, so the two
    // paths agree byte for byte.
    //
    // Composite structs need none of this: their fields carry [JsonInclude] and their all-fields
    // constructor carries [JsonConstructor], so the default object converter handles them and
    // still honours the caller's JsonSerializerOptions (naming policy and friends).

    public sealed class NumberJsonConverter : JsonConverter<Number>
    {
        public override Number Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.String
                ? (float)NonFinite(reader.GetString())
                : reader.GetSingle();

        public override void Write(Utf8JsonWriter writer, Number value, JsonSerializerOptions options)
        {
            if (float.IsFinite(value.Value))
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteStringValue(PlatoJson.NonFiniteName(value.Value));
        }

        // JSON has no non-finite number literal; PlatoJson writes the same quoted names
        // System.Text.Json uses under JsonNumberHandling.AllowNamedFloatingPointLiterals.
        internal static double NonFinite(string s)
        {
            switch (s)
            {
                case "NaN": return double.NaN;
                case "Infinity": return double.PositiveInfinity;
                case "-Infinity": return double.NegativeInfinity;
                default: throw new JsonException($"Expected a number, got \"{s}\"");
            }
        }
    }

    public sealed class IntegerJsonConverter : JsonConverter<Integer>
    {
        public override Integer Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options)
            => reader.GetInt32();

        public override void Write(Utf8JsonWriter writer, Integer value, JsonSerializerOptions options)
            => writer.WriteNumberValue(value.Value);
    }

    public sealed class BooleanJsonConverter : JsonConverter<Boolean>
    {
        public override Boolean Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options)
            => reader.GetBoolean();

        public override void Write(Utf8JsonWriter writer, Boolean value, JsonSerializerOptions options)
            => writer.WriteBooleanValue(value.Value);
    }

    public sealed class CharacterJsonConverter : JsonConverter<Character>
    {
        public override Character Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options)
        {
            var s = reader.GetString();
            if (s == null || s.Length != 1)
                throw new JsonException($"Expected a one-character string, got \"{s}\"");
            return s[0];
        }

        public override void Write(Utf8JsonWriter writer, Character value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Value.ToString());
    }

    public sealed class StringJsonConverter : JsonConverter<String>
    {
        public override String Read(ref Utf8JsonReader reader, System.Type type, JsonSerializerOptions options)
            => reader.GetString();

        public override void Write(Utf8JsonWriter writer, String value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Value);
    }
}
