// Which emitted types hold no arena memory (what a cached constant may be).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter
{
    public partial class CppWriter
    {
        /// <summary>
        /// True when a value of the type owns no pointer into the thread's arena: the scalars, Angle, String (a
        /// std::u16string), and a non-generic record or sum whose fields are all such. An Array, List, Buffer, Bytes,
        /// function value or generic instance is not: the arena is thread-local and is rolled back or reset, so a static
        /// that kept one would dangle after the first rollback and be unreadable from another thread.
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
