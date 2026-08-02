using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter
{
    /// <summary>
    /// The JSON / formatting / parsing surface every generated struct carries.
    ///
    /// Why it exists: the writer used to emit `ToString()` as
    /// <c>$"{{ \"X\" = {X}, ... }}"</c> — `=` instead of `:`, so not JSON — and every scalar in it
    /// went through the CURRENT culture, so a Number rendered as `1,5` under de-DE. The replacement
    /// is real JSON, invariant unless the caller passes a provider, produced by the handwritten
    /// <c>PlatoJson</c> runtime rather than by string interpolation in emitted text.
    ///
    /// The emitted members and the interfaces they discharge:
    ///   ToString()                              — JSON (the object form for a struct with fields)
    ///   ToString(format, provider)              — System.IFormattable
    ///   TryFormat(dest, out n, format, provider)— System.ISpanFormattable
    ///   Parse / TryParse (string and span)      — System.IParsable&lt;T&gt; / System.ISpanParsable&lt;T&gt;
    ///   ToJson / FromJson / AppendJson          — the convenience surface callers actually reach for
    ///
    /// The two directions are NOT symmetric in how they get there, deliberately:
    ///   * WRITING is emitted — one <c>PlatoJson.WriteMember</c> line per field, dispatched on the
    ///     runtime type of the value, so it compiles for every field shape a Plato type can have
    ///     (nested struct, scalar wrapper, <c>IReadOnlyList</c>, <c>System.Func</c>, open type
    ///     parameter) and needs no serializer options to produce the canonical text.
    ///   * READING hands the input to System.Text.Json, against the
    ///     <c>[JsonInclude]</c>/<c>[JsonConstructor]</c> contract these structs already carry.
    ///     Every field shape is the serializer's problem rather than the writer's, and the parse
    ///     cannot disagree with <c>JsonSerializer</c> about a type's shape because it IS
    ///     <c>JsonSerializer</c>. The two directions agreeing is pinned by a test that sweeps every
    ///     generated type (JsonSurfaceTests).
    /// </summary>
    public partial class CSharpConcreteTypeWriter
    {
        /// <summary>The four System interfaces the surface discharges, as written in the struct's
        /// base list. ISpanFormattable/ISpanParsable already imply their non-span bases; both are
        /// named anyway so the declaration says what the type supports without a lookup.</summary>
        public string SerializationInterfaces
            => $"System.IFormattable, System.ISpanFormattable, "
               + $"System.IParsable<{Name}>, System.ISpanParsable<{Name}>";

        /// <summary>Whether this type gets the surface at all. Excluded: the primitives with no
        /// payload (`Type`, the `FunctionN` arities) — they wrap a `System.Type` / `System.Func`
        /// that has no serializable value, and their ToString is already their own name.</summary>
        public bool HasSerializationSurface
            => !IsPrimitive || CSharpWriter.ScalarPrimitives.ContainsKey(SimpleName);

        private bool IsScalarPrimitive
            => IsPrimitive && CSharpWriter.ScalarPrimitives.ContainsKey(SimpleName);

        public void WriteSerializationSurface()
        {
            if (!HasSerializationSurface)
                return;

            var tw = TypeWriter;
            tw.WriteLine("// JSON, formatting and parsing. Invariant unless a provider is passed.");

            if (IsScalarPrimitive)
                WriteScalarJson();
            else
                WriteObjectJson();

            tw.WriteLine($"{Attr} public string ToJson() => ToString(null, null);");
            tw.WriteLine(
                $"{Attr} public bool TryFormat(System.Span<char> destination, out int charsWritten, " +
                $"System.ReadOnlySpan<char> format, System.IFormatProvider provider) " +
                $"=> PlatoJson.TryFormatString(ToString(format.Length == 0 ? null : format.ToString(), provider), " +
                $"destination, out charsWritten);");
            tw.WriteLine();

            WriteParseSurface();
            tw.WriteLine();
        }

        // ---------------------------------------------------------------------------------
        // Writing
        // ---------------------------------------------------------------------------------

        private void WriteObjectJson()
        {
            var tw = TypeWriter;
            var members = JsonMembers().ToList();

            tw.WriteLine($"{Attr} public override string ToString() => ToString(null, null);");
            tw.WriteLine(
                "public string ToString(string format, System.IFormatProvider provider) " +
                "=> AppendJson(new System.Text.StringBuilder(), format, provider).ToString();");
            tw.WriteLine(
                "public System.Text.StringBuilder AppendJson(System.Text.StringBuilder sb, " +
                "string format = null, System.IFormatProvider provider = null)");
            tw.WriteStartBlock();
            tw.WriteLine("sb.Append('{');");
            for (var i = 0; i < members.Count; ++i
                )
            {
                var comma = i < members.Count - 1 ? ".Append(',')" : "";
                tw.WriteLine(
                    $"PlatoJson.WriteMember(sb, \"{members[i]}\", {members[i]}, format, provider){comma};");
            }
            tw.WriteLine("return sb.Append('}');");
            tw.WriteEndBlock();
        }

        // A scalar wrapper (Number/Integer/Boolean/Character/String) serializes as the JSON value
        // of its payload, not as an object: `1.5`, `true`, `"a"`.
        //
        // ToString() stays the payload's own invariant text — UNQUOTED for String and Character.
        // Quoting there would be a trap: `$"{someString}"` and every log line would start showing
        // quotes. The IFormattable overload is the JSON one, and it is the overload PlatoJson calls
        // when this value is a field of another struct, so nested JSON is still correct.
        private void WriteScalarJson()
        {
            var tw = TypeWriter;
            tw.WriteLine(
                $"{Attr} public override string ToString() " +
                $"=> System.Convert.ToString(Value, PlatoJson.Invariant);");
            tw.WriteLine(
                "public string ToString(string format, System.IFormatProvider provider) " +
                "=> AppendJson(new System.Text.StringBuilder(), format, provider).ToString();");
            tw.WriteLine(
                "public System.Text.StringBuilder AppendJson(System.Text.StringBuilder sb, " +
                "string format = null, System.IFormatProvider provider = null) " +
                "=> PlatoJson.WriteValue(sb, Value, format, provider);");
        }

        // ---------------------------------------------------------------------------------
        // Reading
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// The whole parse surface, identical for a product, a sum and a scalar wrapper: hand the
        /// text to System.Text.Json.
        ///
        /// This used to be a per-field loop over a hand-rolled scanner, with the field VALUES
        /// resolved by reflection because a Plato field can be any shape. All of that is what
        /// System.Text.Json already does, and better: it is the same reader that produced the
        /// [JsonInclude] / [JsonConstructor] contract these structs already carry, so the parse and
        /// the serializer cannot disagree about a type's shape.
        ///
        /// `provider` is accepted (System.IParsable requires it) and ignored. JSON numbers are
        /// culture-invariant by specification, so there is no reading of them a provider could
        /// change; the parameter exists for the interface, not for behaviour.
        /// </summary>
        private void WriteParseSurface()
        {
            var tw = TypeWriter;
            tw.WriteLine(
                $"{Attr} public static bool TryParse(System.ReadOnlySpan<char> input, " +
                $"System.IFormatProvider provider, out {Name} result) " +
                $"=> PlatoJson.TryDeserialize(input, out result);");
            // `(System.ReadOnlySpan<char>)input` takes the implicit string->span conversion; a bare
            // `input` would bind back to this same string overload. A null string converts to an
            // empty span, which fails to parse — what IParsable.TryParse(null, ...) must do.
            tw.WriteLine(
                $"{Attr} public static bool TryParse(string input, System.IFormatProvider provider, out {Name} result) " +
                $"=> PlatoJson.TryDeserialize((System.ReadOnlySpan<char>)input, out result);");
            tw.WriteLine(
                $"{Attr} public static bool TryParse(string input, out {Name} result) " +
                $"=> PlatoJson.TryDeserialize((System.ReadOnlySpan<char>)input, out result);");
            tw.WriteLine(
                $"{Attr} public static {Name} Parse(System.ReadOnlySpan<char> input, System.IFormatProvider provider) " +
                $"=> PlatoJson.Deserialize<{Name}>(input);");
            tw.WriteLine(
                $"{Attr} public static {Name} Parse(string input, System.IFormatProvider provider) " +
                $"=> PlatoJson.Deserialize<{Name}>((System.ReadOnlySpan<char>)input);");
            tw.WriteLine(
                $"{Attr} public static {Name} Parse(string input) " +
                $"=> PlatoJson.Deserialize<{Name}>((System.ReadOnlySpan<char>)input);");
            tw.WriteLine(
                $"{Attr} public static {Name} FromJson(string input) " +
                $"=> PlatoJson.Deserialize<{Name}>((System.ReadOnlySpan<char>)input);");
        }

        // ---------------------------------------------------------------------------------
        // Members
        // ---------------------------------------------------------------------------------

        /// <summary>The JSON member names, in declaration order. A sum type leads with its `Kind`
        /// discriminant and then carries the flattened per-case fields, exactly the shape
        /// DataContract already serializes — so the JSON round-trips without a case registry.</summary>
        private IEnumerable<string> JsonMembers()
            => ConcreteType.TypeDef.IsSum ? FieldNames.Prepend("Kind") : FieldNames;
    }
}
