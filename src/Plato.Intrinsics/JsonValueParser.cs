using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Ara3D.Geometry
{
    /// <summary>
    /// Resolves, once per <typeparamref name="T"/>, how a raw JSON value becomes a
    /// <typeparamref name="T"/>. Runtime dispatch rather than emitted static calls is deliberate:
    /// a generated struct has fields of every shape — other generated structs, scalars, arrays,
    /// interface interfaces, <c>System.Func</c>, open type parameters — and only some of those have
    /// a parse. Emitting <c>PlatoJson.TryParseValue&lt;TField&gt;</c> uniformly always COMPILES;
    /// the types with no parse simply return false at run time.
    ///
    /// <see cref="TryParse"/> is null when nothing can parse this type.
    /// </summary>
    public static class JsonValueParser<T>
    {
        public delegate bool Fn(ReadOnlySpan<char> raw, IFormatProvider provider, out T value);

        public static readonly Fn TryParse = Build();

        private static Fn Build()
        {
            var t = typeof(T);

            if (t == typeof(string)) return ParseString;
            if (t == typeof(char)) return ParseChar;
            if (t == typeof(bool)) return ParseBool;
            if (t == typeof(float)) return ParseFloat;
            if (t == typeof(double)) return ParseDouble;

            var element = ElementType(t);
            if (element != null)
                return BuildCollection(element);

            // Anything with the ISpanParsable shape: every generated struct, and the BCL integer
            // and decimal types.
            var m = t.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, null,
                new[] { typeof(ReadOnlySpan<char>), typeof(IFormatProvider), t.MakeByRefType() }, null);
            return m == null ? null : (Fn)m.CreateDelegate(typeof(Fn));
        }

        private static System.Type ElementType(System.Type t)
        {
            if (t.IsArray && t.GetArrayRank() == 1)
                return t.GetElementType();
            if (!t.IsGenericType)
                return null;
            var def = t.GetGenericTypeDefinition();
            return def == typeof(IReadOnlyList<>) || def == typeof(IReadOnlyCollection<>)
                   || def == typeof(IList<>) || def == typeof(ICollection<>)
                   || def == typeof(IEnumerable<>) || def == typeof(List<>)
                ? t.GetGenericArguments()[0]
                : null;
        }

        private static Fn BuildCollection(System.Type element)
            => (Fn)typeof(JsonValueParser<T>)
                .GetMethod(nameof(ParseCollection), BindingFlags.NonPublic | BindingFlags.Static)
                .MakeGenericMethod(element)
                .CreateDelegate(typeof(Fn));

        private static bool ParseCollection<TElement>(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            value = default;
            var items = new List<TElement>();
            var r = new JsonArrayReader(raw);
            while (r.Read())
            {
                if (!PlatoJson.TryParseValue<TElement>(r.Value, provider, out var item))
                    return false;
                items.Add(item);
            }
            if (!r.Completed)
                return false;
            // A List<TElement> satisfies every supported interface; only a real array needs a copy.
            value = (T)(object)(typeof(T).IsArray ? items.ToArray() : (object)items);
            return true;
        }

        private static bool ParseString(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            value = default;
            if (!JsonScan.Unquote(raw, out var s))
                return false;
            value = (T)(object)s;
            return true;
        }

        private static bool ParseChar(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            value = default;
            if (!JsonScan.Unquote(raw, out var s) || s.Length != 1)
                return false;
            value = (T)(object)s[0];
            return true;
        }

        private static bool ParseBool(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            value = default;
            if (raw.SequenceEqual("true".AsSpan()))
                value = (T)(object)true;
            else if (raw.SequenceEqual("false".AsSpan()))
                value = (T)(object)false;
            else
                return false;
            return true;
        }

        private static bool ParseFloat(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            value = default;
            if (!TryNumber(raw, provider, out var d))
                return false;
            value = (T)(object)(float)d;
            return true;
        }

        private static bool ParseDouble(ReadOnlySpan<char> raw, IFormatProvider provider, out T value)
        {
            value = default;
            if (!TryNumber(raw, provider, out var d))
                return false;
            value = (T)(object)d;
            return true;
        }

        // JSON has no non-finite number literal, so those arrive as the quoted names PlatoJson
        // writes; everything else is a bare invariant number token.
        private static bool TryNumber(ReadOnlySpan<char> raw, IFormatProvider provider, out double result)
        {
            if (raw.Length > 0 && raw[0] == '"')
            {
                result = 0;
                if (!JsonScan.Unquote(raw, out var s))
                    return false;
                switch (s)
                {
                    case "NaN": result = double.NaN; return true;
                    case "Infinity": result = double.PositiveInfinity; return true;
                    case "-Infinity": result = double.NegativeInfinity; return true;
                    default: return false;
                }
            }
            return double.TryParse(raw, NumberStyles.Float, PlatoJson.Culture(provider), out result);
        }
    }
}
