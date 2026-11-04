// What the sum layouts ask of the writer: which cases may share storage, where a field is read, and the JSON members.
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter
{
    public partial class CppWriter
    {
        private Dictionary<string, ConcreteType> _concreteByName;

        private ConcreteType ConcreteNamed(string name)
        {
            _concreteByName ??= Compilation.ConcreteTypes.GroupBy(c => c.TypeDef.Name).ToDictionary(g => g.Key, g => g.First());
            return _concreteByName.TryGetValue(name, out var c) ? c : null;
        }

        /// <summary>
        /// True when every field of <paramref name="c"/> is trivially copyable and trivially destructible (what a union member
        /// needs) and no field is named like a type the case spells (a nested struct's member may not change the meaning
        /// of a name it has already used). Arrays are views and Bytes a pointer and a count; a String, a function value or
        /// any record holding one is not.
        /// </summary>
        private bool MayShareSumStorage(SumCaseDef c)
        {
            if (c.Fields.Count == 0)
                return false;
            var spelled = new HashSet<string>();
            foreach (var f in c.Fields)
            {
                if (!IsUnionSafe(TypeInstance.Create(f.Type), new HashSet<string>()))
                    return false;
                spelled.Add(CppTypeNameOrNull(f.Type));
            }
            return c.Fields.All(f => !spelled.Contains(EscapeName(f.Name)));
        }

        private bool IsUnionSafe(TypeInstance ti, HashSet<string> visiting)
        {
            var cpp = CppTypeNameOrNull(ti);
            if (cpp == null)
                return false;
            if (CpuPrimitives.ContainsValue(cpp) || cpp == "Angle" || cpp == "Bytes" || cpp.StartsWith("Array<"))
                return true;
            var c = StructNames.Contains(ti.Name) ? ConcreteNamed(ti.Name) : null;
            if (c == null)
                return false;
            if (!visiting.Add(ti.Name))
                return true;
            try
            {
                if (!ti.Args.All(a => IsUnionSafe(a, visiting)))
                    return false;
                var parameters = c.TypeDef.TypeParameters.Select(p => p.Name).ToHashSet();
                var types = c.TypeDef.IsSum ? c.TypeDef.Cases.SelectMany(k => k.Fields).Select(f => f.Type) : c.TypeDef.Fields.Select(f => f.Type);
                return types.All(t =>
                {
                    var inner = TypeInstance.Create(t);
                    return parameters.Contains(inner.Name) || IsUnionSafe(inner, visiting);
                });
            }
            finally
            {
                visiting.Remove(ti.Name);
            }
        }

        private Dictionary<string, Dictionary<string, string>> _sumAccess;

        /// <summary>The member path of a flattened sum field (<c>Circle_Radius</c>) on a value of the named sum type.</summary>
        private bool TryGetSumFieldAccess(string platoType, string flatName, out string access)
        {
            access = null;
            if (platoType == null)
                return false;
            _sumAccess ??= new Dictionary<string, Dictionary<string, string>>();
            if (!_sumAccess.TryGetValue(platoType, out var map))
            {
                var cases = SumCasesOf(platoType);
                if (cases == null)
                {
                    var generic = ConcreteNamed(platoType);
                    cases = generic != null && generic.TypeDef.IsSum ? generic.TypeDef.Cases : null;
                }
                map = cases?.SelectMany(c => c.Fields.Select(f => (f.FlatName, Path: Sums.FieldAccess(c, f)))).ToDictionary(x => x.FlatName, x => x.Path);
                _sumAccess[platoType] = map;
            }
            return map != null && map.TryGetValue(flatName, out access);
        }

        private string JsonReadMembers((string Name, List<CppField> Fields, ConcreteType Type) s)
            => s.Type != null && s.Type.TypeDef.IsSum
                ? Sums.JsonRead(s.Name, s.Type.TypeDef.Cases)
                : string.Concat(s.Fields.Select(f => $"    from_json_member(j, \"{f.Name}\", v.{EscapeName(f.Name)});\n"));

        private string JsonWriteMembers((string Name, List<CppField> Fields, ConcreteType Type) s)
            => s.Type != null && s.Type.TypeDef.IsSum
                ? Sums.JsonWrite(s.Name, s.Type.TypeDef.Cases)
                : string.Concat(s.Fields.Select(f => $"    j.set(\"{f.Name}\", to_json(v.{EscapeName(f.Name)}));\n"));
    }
}
