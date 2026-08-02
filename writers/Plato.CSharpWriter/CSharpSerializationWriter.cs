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
    /// Field VALUES are written and read through <c>PlatoJson</c>'s runtime dispatch, never by
    /// naming the field's type. That is what lets one uniform line per field compile for every
    /// field shape a Plato type can have — a nested struct, a scalar wrapper, an
    /// <c>IReadOnlyList</c>, a <c>System.Func</c>, an open type parameter. Shapes with no parse
    /// simply return false at run time instead of failing to compile.
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

        /// <summary>Local names for the parsed field values. Deliberately not the constructor's
        /// parameter names: those are derived from the field names and would shadow members inside
        /// a static method that also mentions them.</summary>
        private static string ParsedLocal(int i) => $"_v{i}";

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

            if (IsScalarPrimitive)
                WriteScalarTryParse();
            else
                WriteObjectTryParse();

            WriteParseTail();
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

        private void WriteObjectTryParse()
        {
            var tw = TypeWriter;
            var members = JsonMembers().ToList();
            var types = JsonMemberTypes().ToList();

            tw.WriteLine(
                $"public static bool TryParse(System.ReadOnlySpan<char> input, " +
                $"System.IFormatProvider provider, out {Name} result)");
            tw.WriteStartBlock();
            tw.WriteLine("result = default;");
            for (var i = 0; i < members.Count; ++i)
                tw.WriteLine($"{types[i]} {ParsedLocal(i)} = default;");
            tw.WriteLine("var reader = new JsonObjectReader(input);");
            tw.WriteLine("while (reader.Read())");
            tw.WriteStartBlock();
            for (var i = 0; i < members.Count; ++i)
                tw.WriteLine(
                    $"{(i == 0 ? "" : "else ")}if (reader.NameIs(\"{members[i]}\")) " +
                    $"{{ if (!PlatoJson.TryParseValue(reader.Value, provider, out {ParsedLocal(i)})) return false; }}");
            tw.WriteEndBlock();
            tw.WriteLine("if (!reader.Completed) return false;");
            tw.WriteLine(members.Count == 0
                ? $"result = default;"
                : $"result = new {Name}({Enumerable.Range(0, members.Count).Select(ParsedLocal).JoinStringsWithComma()});");
            tw.WriteLine("return true;");
            tw.WriteEndBlock();
        }

        private void WriteScalarTryParse()
        {
            var tw = TypeWriter;
            var payload = CSharpWriter.ScalarPrimitives[SimpleName];
            tw.WriteLine(
                $"public static bool TryParse(System.ReadOnlySpan<char> input, " +
                $"System.IFormatProvider provider, out {Name} result)");
            tw.WriteStartBlock();
            tw.WriteLine("result = default;");
            tw.WriteLine($"if (!PlatoJson.TryParseValue<{payload}>(input, provider, out var value)) return false;");
            tw.WriteLine($"result = new {Name}(value);");
            tw.WriteLine("return true;");
            tw.WriteEndBlock();
        }

        // The string-flavoured half of IParsable/ISpanParsable, plus the Parse throwing forms.
        // `(System.ReadOnlySpan<char>)input` uses the implicit string->span conversion; a bare
        // `input` would bind back to the string overload. A null string converts to an empty span,
        // which fails to parse — which is what IParsable.TryParse(null, ...) is required to do.
        private void WriteParseTail()
        {
            var tw = TypeWriter;
            tw.WriteLine(
                $"{Attr} public static bool TryParse(string input, System.IFormatProvider provider, out {Name} result) " +
                $"=> TryParse((System.ReadOnlySpan<char>)input, provider, out result);");
            tw.WriteLine(
                $"{Attr} public static bool TryParse(string input, out {Name} result) " +
                $"=> TryParse((System.ReadOnlySpan<char>)input, null, out result);");
            tw.WriteLine(
                $"public static {Name} Parse(System.ReadOnlySpan<char> input, System.IFormatProvider provider) " +
                $"=> TryParse(input, provider, out var result) ? result : throw PlatoJson.BadFormat(\"{SimpleName}\", input);");
            tw.WriteLine(
                $"{Attr} public static {Name} Parse(string input, System.IFormatProvider provider) " +
                $"=> Parse((System.ReadOnlySpan<char>)input, provider);");
            tw.WriteLine(
                $"{Attr} public static {Name} Parse(string input) " +
                $"=> Parse((System.ReadOnlySpan<char>)input, null);");
            tw.WriteLine(
                $"{Attr} public static {Name} FromJson(string input) " +
                $"=> Parse((System.ReadOnlySpan<char>)input, null);");
        }

        // ---------------------------------------------------------------------------------
        // Members
        // ---------------------------------------------------------------------------------

        /// <summary>The JSON member names, in declaration order. A sum type leads with its `Kind`
        /// discriminant and then carries the flattened per-case fields, exactly the shape
        /// DataContract already serializes — so the JSON round-trips without a case registry.</summary>
        private IEnumerable<string> JsonMembers()
            => ConcreteType.TypeDef.IsSum ? FieldNames.Prepend("Kind") : FieldNames;

        private IEnumerable<string> JsonMemberTypes()
            => ConcreteType.TypeDef.IsSum ? FieldTypes.Prepend("int") : FieldTypes;
    }
}
