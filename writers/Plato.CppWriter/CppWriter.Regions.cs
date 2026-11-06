// Which values can point into the arena: the analysis behind scalar regions (TKT-1211) and, on its branch, stack literals (TKT-1188).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter
{
    public partial class CppWriter
    {
        /// <summary>
        /// True when no value of <paramref name="type"/> can hold a pointer into the arena: the Cpu primitives, Angle,
        /// String (a <c>std::u16string</c> that owns its heap text), and non-generic records and sums whose fields are all
        /// such types. An Array, Bytes, list, buffer or function value holds one, as does any record containing one, and
        /// so does any type the dialect cannot spell (a type variable, an interface).
        /// </summary>
        public bool HoldsNoArenaMemory(TypeExpression type)
            => type != null && HoldsNoArenaMemory(TypeInstance.Create(type), new HashSet<string>());

        private bool HoldsNoArenaMemory(TypeInstance ti, HashSet<string> visiting)
        {
            var cpp = CppTypeNameOrNull(ti);
            if (cpp == null)
                return false;
            if (CpuPrimitives.ContainsValue(cpp) || cpp == "Angle" || cpp == "String")
                return true;
            if (ti.Args.Count > 0 || !StructNames.Contains(ti.Name))
                return false;
            var c = ConcreteNamed(ti.Name);
            if (c == null || c.TypeDef.TypeParameters.Count > 0)
                return false;
            if (!visiting.Add(ti.Name))
                return true;
            try
            {
                var types = c.TypeDef.IsSum ? c.TypeDef.Cases.SelectMany(k => k.Fields).Select(f => f.Type) : c.TypeDef.Fields.Select(f => f.Type);
                return types.All(t => HoldsNoArenaMemory(TypeInstance.Create(t), visiting));
            }
            finally
            {
                visiting.Remove(ti.Name);
            }
        }
    }
}
