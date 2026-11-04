// A sum laid out as an int32_t Kind and one union of per-case structs (the largest case sets the size).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// <c>int32_t Kind</c>, then the fields of every case that may share storage as nested structs
    /// (<c>Circle_Data As_Circle</c>) in one anonymous union, so a value is the size of its largest case instead of the
    /// sum of all. The first member of the union is a zero-filled byte array as large as the biggest case, so a
    /// value-initialized sum is all zeros, as the flattened one is, and the sum stays trivially copyable. A case that may
    /// not share storage (it holds a String, a function value, ...) keeps flat <c>Case_Field</c> members next to Kind.
    /// JSON is unchanged: the object lists every field of every case; an inactive union case writes the zero value.
    /// </summary>
    public sealed class CompactUnionSumLayout : FlattenedKindSumLayout, ISumLayout
    {
        private readonly Func<SumCaseDef, bool> _mayShare;
        private readonly bool _switchDispatch;

        public CompactUnionSumLayout(bool switchDispatch, Func<SumCaseDef, bool> mayShareStorage = null)
        {
            _switchDispatch = switchDispatch;
            _mayShare = mayShareStorage ?? (_ => false);
        }

        public string SwitchOn => _switchDispatch ? ".Kind" : null;

        public ISumLayout BindStorage(Func<SumCaseDef, bool> mayShareStorage)
            => new CompactUnionSumLayout(_switchDispatch, mayShareStorage);

        private static string Member(SumCaseDef c) => "As_" + c.Name;
        private static string Nested(SumCaseDef c) => c.Name + "_Members";

        private bool InUnion(SumCaseDef c) => c.Fields.Count > 0 && _mayShare(c);

        private string Path(SumCaseDef c, SumCaseField f)
            => InUnion(c) ? $"{Member(c)}.{CppWriter.EscapeName(f.Name)}" : CppWriter.EscapeName(f.FlatName);

        public override IReadOnlyList<CppField> Fields(IReadOnlyList<SumCaseDef> cases)
        {
            var list = new List<CppField> { new CppField("Kind", null, "int32_t") };
            list.AddRange(cases.SelectMany(c => c.Fields.Select(f => new CppField(f.FlatName, f.Type, null, Path(c, f)))));
            return list;
        }

        public override string FieldAccess(SumCaseDef c, SumCaseField f) => Path(c, f);

        public override string MemberDeclarations(string typeName, IReadOnlyList<SumCaseDef> cases, Func<CppField, string> cppType)
        {
            var shared = cases.Where(InUnion).ToList();
            var sb = new StringBuilder();
            foreach (var c in shared)
            {
                sb.Append($"    struct {Nested(c)} {{ ");
                foreach (var f in c.Fields)
                    sb.Append($"{cppType(new CppField(f.Name, f.Type))} {CppWriter.EscapeName(f.Name)}; ");
                sb.Append("};\n");
            }
            for (var i = 0; i < shared.Count; i++)
                sb.Append(i == 0
                    ? $"    static constexpr size_t Storage0_ = sizeof({Nested(shared[0])});\n"
                    : $"    static constexpr size_t Storage{i}_ = Storage{i - 1}_ > sizeof({Nested(shared[i])}) ? Storage{i - 1}_ : sizeof({Nested(shared[i])});\n");
            sb.Append("    int32_t Kind;\n");
            foreach (var c in cases.Where(c => !InUnion(c)))
                foreach (var f in c.Fields)
                    sb.Append($"    {cppType(new CppField(f.FlatName, f.Type))} {CppWriter.EscapeName(f.FlatName)};\n");
            if (shared.Count > 0)
            {
                sb.Append("    union\n    {\n");
                sb.Append($"        {shared.Select(c => $"alignas({Nested(c)}) ").JoinStrings("")}unsigned char Zero_[Storage{shared.Count - 1}_] = {{}};\n");
                foreach (var c in shared)
                    sb.Append($"        {Nested(c)} {Member(c)};\n");
                sb.Append("    };\n");
            }
            return sb.ToString();
        }

        public override string Construct(string typeName, SumCaseDef c, IReadOnlyList<string> values)
        {
            var sets = c.Fields.Select((f, i) => $" r.{Path(c, f)} = {values[i]};").JoinStrings("");
            return $" {{ {typeName} r{{}}; r.Kind = {typeName}::{c.TagConstName};{sets} return r; }}";
        }

        public override string EqualsCondition(string typeName, IReadOnlyList<SumCaseDef> cases)
        {
            var terms = new List<string> { "a.Kind == b.Kind" };
            foreach (var cs in cases.Where(c => c.Fields.Count > 0))
            {
                var eq = cs.Fields.Select(f => $"eq_(a.{Path(cs, f)}, b.{Path(cs, f)})").JoinStrings(" && ");
                terms.Add($"(a.Kind != {typeName}::{cs.TagConstName} || ({eq}))");
            }
            return string.Join(" && ", terms);
        }

        public override string HashBody(string typeName, IReadOnlyList<SumCaseDef> cases, Func<CppField, string> hashOf)
        {
            var sb = new StringBuilder(" { int32_t h = hash_(a.Kind);");
            foreach (var cs in cases.Where(c => c.Fields.Count > 0))
            {
                sb.Append($" if (a.Kind == {typeName}::{cs.TagConstName}) {{");
                foreach (var f in cs.Fields)
                    sb.Append($" h = mix_(h, {hashOf(new CppField(f.FlatName, f.Type, null, Path(cs, f)))});");
                sb.Append(" }");
            }
            sb.Append(" return h; }");
            return sb.ToString();
        }

        public override string JsonRead(string typeName, IReadOnlyList<SumCaseDef> cases)
        {
            var sb = new StringBuilder("    from_json_member(j, \"Kind\", v.Kind);\n");
            foreach (var c in cases)
            {
                if (InUnion(c))
                    sb.Append($"    if (v.Kind == {typeName}::{c.TagConstName})\n    {{\n");
                foreach (var f in c.Fields)
                    sb.Append($"{(InUnion(c) ? "    " : "")}    from_json_member(j, \"{f.FlatName}\", v.{Path(c, f)});\n");
                if (InUnion(c))
                    sb.Append("    }\n");
            }
            return sb.ToString();
        }

        public override string JsonWrite(string typeName, IReadOnlyList<SumCaseDef> cases)
        {
            var sb = new StringBuilder("    j.set(\"Kind\", to_json(v.Kind));\n");
            foreach (var c in cases)
                foreach (var f in c.Fields)
                {
                    var path = $"v.{Path(c, f)}";
                    sb.Append(InUnion(c)
                        ? $"    j.set(\"{f.FlatName}\", v.Kind == {typeName}::{c.TagConstName} ? to_json({path}) : to_json(decltype({path}){{}}));\n"
                        : $"    j.set(\"{f.FlatName}\", to_json({path}));\n");
                }
            return sb.ToString();
        }
    }
}
