using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// THE concept-closure walk for the checker: every concept instance a type implements or
    /// inherits, transitively, with each level's type arguments substituted through
    /// (<c>List&lt;Number&gt;</c> yields <c>IArray&lt;Number&gt;</c>, not <c>IArray&lt;T&gt;</c>;
    /// <c>Vector3 : IVectorLike : IArrayLike&lt;Number&gt;</c> resolves through both levels).
    /// Shared by the solver's concept satisfaction and monomorphization's element grounding so
    /// there is exactly one answer to "which instance of concept C does type T carry".
    ///
    /// (Deliberately separate from <see cref="TypeExtensions.IsImplementing"/>, which the legacy
    /// writers depend on and which has different — coarser — matching rules.)
    /// </summary>
    public static class ConceptClosure
    {
        /// <summary>Every concept instance of <paramref name="t"/>, per-level substituted,
        /// pre-order, cycle-capped by depth.</summary>
        public static IEnumerable<TypeExpression> InstancesOf(TypeExpression t, int depth = 0)
        {
            if (t?.Def == null || depth > 8)
                yield break;
            var map = new Dictionary<string, TypeExpression>();
            var tps = t.Def.TypeParameters;
            for (var i = 0; i < tps.Count && i < t.TypeArgs.Count; i++)
                map[tps[i].Name] = t.TypeArgs[i];
            foreach (var impl in t.Def.Implements.Concat(t.Def.Inherits))
            {
                if (impl?.Def == null)
                    continue;
                var inst = impl.Replace(x => x != null && map.TryGetValue(x.ToString(), out var r) ? r : x);
                yield return inst;
                foreach (var deeper in InstancesOf(inst, depth + 1))
                    yield return deeper;
            }
        }

        /// <summary>The instance of the named concept that <paramref name="t"/> is or carries
        /// (itself, or via the closure), or null.</summary>
        public static TypeExpression FindInstance(TypeExpression t, string conceptName)
        {
            if (t?.Def == null || conceptName == null)
                return null;
            if (t.Def.Name == conceptName)
                return t;
            return InstancesOf(t).FirstOrDefault(i => i?.Def != null && i.Def.Name == conceptName);
        }
    }
}
