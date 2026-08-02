using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The JSON surface every generated struct funnels through. `ToString`, `ToString(format,
    /// provider)`, `TryFormat`, `Parse` and `TryParse` on a generated struct are thin shells over
    /// these helpers, so the wire format and its culture-invariance are decided in ONE place
    /// instead of being baked into emitted text.
    ///
    /// Writing is done here; READING is System.Text.Json (<see cref="TryDeserialize{T}"/>), against
    /// the [JsonInclude] / [JsonConstructor] contract the generated structs carry.
    ///
    /// Two properties the emitted code relies on:
    ///   * Invariant by default. A null <c>IFormatProvider</c> means <see cref="Invariant"/>, so
    ///     a Number is `1.5` under de-DE, never `1,5`.
    ///   * Round-trippable floats ("R"). The non-finite values have no JSON literal, so they are
    ///     written as the quoted names System.Text.Json uses under
    ///     <c>JsonNumberHandling.AllowNamedFloatingPointLiterals</c> ("NaN", "Infinity",
    ///     "-Infinity") and read back from the same spelling.
    /// </summary>
    public static class PlatoJson
    {
        public static readonly IFormatProvider Invariant = CultureInfo.InvariantCulture;

        public static IFormatProvider Culture(IFormatProvider provider)
            => provider ?? Invariant;

        /// <summary>`"name":value` — the caller writes the separating commas and braces.</summary>
        public static StringBuilder WriteMember<T>(StringBuilder sb, string name, T value, string format, IFormatProvider provider)
            => WriteValue(WriteString(sb, name).Append(':'), value, format, provider);

        /// <summary>
        /// One JSON value, dispatched on the RUNTIME type so a nested generated struct renders as
        /// a nested object (it is <c>IFormattable</c>, and its <c>ToString(format, provider)</c> is
        /// its JSON), a collection renders as an array, and everything else falls back to a quoted
        /// string. Dispatching here rather than in generated code is what lets the writer emit one
        /// uniform line per field regardless of the field's type.
        /// </summary>
        public static StringBuilder WriteValue<T>(StringBuilder sb, T value, string format, IFormatProvider provider)
        {
            switch (value)
            {
                case null: return sb.Append("null");
                case string s: return WriteString(sb, s);
                case char c: return WriteString(sb, c.ToString());
                case bool b: return sb.Append(b ? "true" : "false");
                case float f: return WriteFloat(sb, f, format, provider);
                case double d: return WriteDouble(sb, d, format, provider);
                case IFormattable x: return sb.Append(x.ToString(format, Culture(provider)));
                case IEnumerable e: return WriteArray(sb, e, format, provider);
                default: return WriteString(sb, value.ToString());
            }
        }

        public static StringBuilder WriteArray(StringBuilder sb, IEnumerable items, string format, IFormatProvider provider)
        {
            sb.Append('[');
            var first = true;
            foreach (var item in items)
            {
                if (!first) sb.Append(',');
                first = false;
                WriteValue(sb, item, format, provider);
            }
            return sb.Append(']');
        }

        // A float must be formatted AS a float: widening 0.1f to double first would round-trip as
        // 0.10000000149011612.
        private static StringBuilder WriteFloat(StringBuilder sb, float f, string format, IFormatProvider provider)
            => float.IsFinite(f)
                ? sb.Append(f.ToString(format ?? "R", Culture(provider)))
                : WriteString(sb, NonFiniteName(f));

        private static StringBuilder WriteDouble(StringBuilder sb, double d, string format, IFormatProvider provider)
            => double.IsFinite(d)
                ? sb.Append(d.ToString(format ?? "R", Culture(provider)))
                : WriteString(sb, NonFiniteName(d));

        public static string NonFiniteName(double d)
            => double.IsNaN(d) ? "NaN" : d > 0 ? "Infinity" : "-Infinity";

        public static StringBuilder WriteString(StringBuilder sb, string s)
        {
            if (s == null)
                return sb.Append("null");
            sb.Append('"');
            foreach (var c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                            sb.Append("\\u").Append(((int)c).ToString("x4", Invariant));
                        else
                            sb.Append(c);
                        break;
                }
            }
            return sb.Append('"');
        }

        /// <summary>The <c>ISpanFormattable</c> tail: generated structs format into a string and
        /// copy. Correct and allocating, rather than fast and hand-written per type.</summary>
        public static bool TryFormatString(string s, Span<char> destination, out int charsWritten)
        {
            if (s != null && s.AsSpan().TryCopyTo(destination))
            {
                charsWritten = s.Length;
                return true;
            }
            charsWritten = 0;
            return false;
        }

        /// <summary>
        /// The options the generated <c>Parse</c>/<c>TryParse</c> read with. Pass them to
        /// <c>JsonSerializer</c> to get the same tolerance on any hand-written call site.
        ///
        /// Two departures from the defaults:
        ///   * Case-INSENSITIVE member matching. System.Text.Json matches case-sensitively by
        ///     default, and an object whose members match NOTHING still deserializes — every field
        ///     simply takes its default. So `{"x":1,"y":2,"z":3}` read as a Point3D silently
        ///     produced the origin rather than failing. Casing is the one difference a producer
        ///     realistically introduces (a camelCase naming policy at the far end), so accepting it
        ///     turns the most likely silent-wrong-answer back into a correct read.
        ///   * Named floating-point literals. JSON has no non-finite number, and
        ///     <see cref="WriteValue{T}"/> writes those as the quoted names, so the reader has to
        ///     accept them back.
        ///
        /// Unmapped members are still IGNORED, deliberately: tolerating a member you do not know
        /// is what lets a document written by a newer version of a type be read by an older one.
        /// `UnmappedMemberHandling.Disallow` here would trade that away for strictness.
        /// </summary>
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        };

        /// <summary>
        /// The generated <c>TryParse</c>. System.Text.Json signals bad input by throwing, so the
        /// try/catch IS the Try-pattern here — which means a REJECTED parse costs an exception.
        /// That is the price of not maintaining a second JSON reader, and it is paid only on the
        /// failure path; validating untrusted input in a tight loop wants
        /// <c>JsonDocument</c>/<c>Utf8JsonReader</c> directly.
        ///
        /// Only <see cref="JsonException"/> is caught. A shape the serializer cannot bind at all
        /// (a constructor parameter matching no member) raises <c>InvalidOperationException</c>,
        /// and that is a defect in the emitted type rather than bad input — it must surface, not
        /// turn into <c>false</c>.
        /// </summary>
        public static bool TryDeserialize<T>(ReadOnlySpan<char> json, out T result)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(json, Options);
                return true;
            }
            catch (JsonException)
            {
                result = default;
                return false;
            }
        }

        /// <summary>The throwing half. <c>System.IParsable.Parse</c> is specified to raise
        /// <see cref="FormatException"/>, so the JsonException becomes one (kept as the inner
        /// exception, since its message carries the line and position).</summary>
        public static T Deserialize<T>(ReadOnlySpan<char> json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json, Options);
            }
            catch (JsonException e)
            {
                throw new FormatException($"Input is not valid JSON for {typeof(T)}", e);
            }
        }
    }
}
