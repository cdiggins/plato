// The current sum layout: an int32_t Kind and every case's fields flattened as Case_Field.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Lays out a sum as CSharpConcreteTypeWriter.WriteSumType does: an <c>int32_t Kind</c> (0-based,
    /// declaration order) and every case's fields as <c>Case_Field</c>, all present whichever case is active.
    /// </summary>
    public class FlattenedKindSumLayout : ISumLayout
    {
        public IReadOnlyList<CppField> Fields(IReadOnlyList<SumCaseDef> cases)
        {
            var list = new List<CppField> { new CppField("Kind", null, "int32_t") };
            list.AddRange(cases.SelectMany(cs => cs.Fields).Select(f => new CppField(f.FlatName, f.Type)));
            return list;
        }

        public string TagDeclarations(IReadOnlyList<SumCaseDef> cases)
        {
            var sb = new StringBuilder();
            foreach (var cs in cases)
                sb.Append($"    static constexpr int32_t {cs.TagConstName} = {cs.Tag};\n");
            return sb.ToString();
        }

        public string Construct(string typeName, SumCaseDef c, IReadOnlyList<string> values)
        {
            var sets = c.Fields.Select((f, i) => $" r.{CppWriter.EscapeName(f.FlatName)} = {values[i]};").JoinStrings("");
            return $" {{ {typeName} r{{}}; r.Kind = {typeName}::{c.TagConstName};{sets} return r; }}";
        }

        public string EqualsCondition(string typeName, IReadOnlyList<SumCaseDef> cases)
        {
            var terms = new List<string> { "a.Kind == b.Kind" };
            foreach (var cs in cases.Where(c => c.Fields.Count > 0))
            {
                var eq = cs.Fields.Select(f => $"eq_(a.{CppWriter.EscapeName(f.FlatName)}, b.{CppWriter.EscapeName(f.FlatName)})").JoinStrings(" && ");
                terms.Add($"(a.Kind != {typeName}::{cs.TagConstName} || ({eq}))");
            }
            return string.Join(" && ", terms);
        }

        public string HashBody(string typeName, IReadOnlyList<SumCaseDef> cases, Func<CppField, string> hashOf)
        {
            var sb = new StringBuilder(" { int32_t h = hash_(a.Kind);");
            foreach (var cs in cases.Where(c => c.Fields.Count > 0))
            {
                sb.Append($" if (a.Kind == {typeName}::{cs.TagConstName}) {{");
                foreach (var f in cs.Fields)
                    sb.Append($" h = mix_(h, {hashOf(new CppField(f.FlatName, f.Type))});");
                sb.Append(" }");
            }
            sb.Append(" return h; }");
            return sb.ToString();
        }

        public string PredicateBody(string typeName, SumCaseDef c)
            => $" {{ return s.Kind == {typeName}::{c.TagConstName}; }}";
    }
}
