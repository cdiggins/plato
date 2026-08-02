using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The JSON surface every generated struct funnels through. `ToString`, `ToString(format,
    /// provider)`, `TryFormat`, `Parse` and `TryParse` on a generated struct are thin shells over
    /// these helpers, so the wire format and its culture-invariance are decided in ONE place
    /// instead of being baked into emitted text.
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

        /// <summary>Parse one raw JSON value into <typeparamref name="T"/>, dispatched at runtime
        /// (see <see cref="JsonValueParser{T}"/>). Returns false — never throws, never fails to
        /// compile — for a field type with no parse, which is what lets the writer emit the parse
        /// surface for every struct including those with function- or interface-typed fields.</summary>
        public static bool TryParseValue<T>(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            var fn = JsonValueParser<T>.TryParse;
            if (fn != null)
                return fn(raw, provider, out value);
            value = default;
            return false;
        }

        public static T ParseValue<T>(ReadOnlySpan<char> raw, IFormatProvider provider)
            => TryParseValue<T>(raw, provider, out var v)
                ? v
                : throw new FormatException($"Not valid JSON for {typeof(T)}: {raw.ToString()}");

        /// <summary>The exception an <c>IParsable</c> Parse throws on bad input.</summary>
        public static FormatException BadFormat(string typeName, ReadOnlySpan<char> s)
            => new FormatException($"Input is not valid JSON for {typeName}: {s.ToString()}");
    }
}
