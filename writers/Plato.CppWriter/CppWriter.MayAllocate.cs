// Which functions can touch the arena: the transitive may-allocate analysis behind scalar regions (TKT-1211, condition d).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CppWriter
{
    public partial class CppWriter
    {
        /// <summary>The prelude array templates and builders that allocate arena storage (plato_array.hpp, plato_builders.hpp);
        /// Slice, Take, Skip, At, Count, Reduce, Any, All, Contains and IndexOf do not.</summary>
        internal static readonly HashSet<string> AllocatingPreludeNames = new HashSet<string>
        {
            "Map", "Zip", "MapRange", "Range", "FlatMap", "Append", "Prepend", "Concatenate", "Reverse", "Freeze", "EmptyList", "AddRange",
        };

        private readonly Dictionary<FunctionDef, bool> _mayAllocate = new Dictionary<FunctionDef, bool>();
        private readonly HashSet<FunctionDef> _mayAllocateVisiting = new HashSet<FunctionDef>();
        private readonly Dictionary<FunctionDef, bool> _reachesHeavy = new Dictionary<FunctionDef, bool>();
        private readonly HashSet<FunctionDef> _reachesHeavyVisiting = new HashSet<FunctionDef>();

        /// <summary>
        /// True when a call to <paramref name="def"/> may allocate in the arena: its TIR body (before the C++ passes) holds a
        /// non-empty array literal, a construction of a type holding arena memory, a call to an allocating prelude name, a
        /// function-value invocation, an unresolved node, or a call to a function that may allocate (a fixpoint by memoised
        /// depth-first search; recursion and a function without TIR count as allocating). Bodiless intrinsics allocate only
        /// when their name is an allocating prelude name. A wrong "no" here loses a region, never correctness.
        /// </summary>
        public bool MayAllocate(FunctionDef def)
        {
            if (def == null)
                return true;
            if (_mayAllocate.TryGetValue(def, out var known))
                return known;
            if (!_mayAllocateVisiting.Add(def))
                return true;
            bool result;
            try
            {
                if (def.Body == null)
                    result = AllocatingPreludeNames.Contains(def.Name);
                else
                {
                    var tir = TryGetStaticTir(def);
                    result = tir?.Body == null || tir.Body.Descendants().Any(MayAllocateNode);
                }
            }
            finally
            {
                _mayAllocateVisiting.Remove(def);
            }
            _mayAllocate[def] = result;
            return result;
        }

        /// <summary>
        /// True when <paramref name="def"/> is one of <paramref name="heavy"/> (the names of the functions a tally showed to
        /// allocate most) or calls, transitively, a function that is: the size-threshold form of <see cref="MayAllocate"/>
        /// (--cpp-scalar-region-heavy=A,B,...). Recursion and a function without TIR count as reaching.
        /// </summary>
        public bool ReachesHeavyAllocator(FunctionDef def, IReadOnlyCollection<string> heavy)
        {
            if (def == null)
                return true;
            if (heavy.Contains(def.Name))
                return true;
            if (_reachesHeavy.TryGetValue(def, out var known))
                return known;
            if (!_reachesHeavyVisiting.Add(def))
                return true;
            bool result;
            try
            {
                if (def.Body == null)
                    result = false;
                else
                {
                    var tir = TryGetStaticTir(def);
                    result = tir?.Body == null || tir.Body.Descendants().Any(n =>
                        n is TirInvoke || n is TirUnresolved
                        || n is TirCall c && (c.Name != null && heavy.Contains(c.Name) || c.Callee != null && ReachesHeavyAllocator(c.Callee, heavy)));
                }
            }
            finally
            {
                _reachesHeavyVisiting.Remove(def);
            }
            _reachesHeavy[def] = result;
            return result;
        }

        private bool MayAllocateNode(TirNode n)
        {
            switch (n)
            {
                case TirArray a:
                    return a.Elements.Count > 0;
                case TirInvoke:
                case TirUnresolved:
                    return true;
                case TirNew nw:
                    return !HoldsNoArenaMemory(nw.NewType ?? nw.Type);
                case TirCall c:
                    if (c.Name != null && AllocatingPreludeNames.Contains(c.Name))
                        return true;
                    if (c.Callee != null)
                        return MayAllocate(c.Callee);
                    return c.EmissionKind != EmissionKind.Property && c.EmissionKind != EmissionKind.Conversion
                        && c.EmissionKind != EmissionKind.Constructor && c.EmissionKind != EmissionKind.Operator;
                default:
                    return false;
            }
        }
    }
}
