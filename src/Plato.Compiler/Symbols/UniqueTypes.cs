using System.Collections.Generic;

namespace Ara3D.Geometry.Compiler.Symbols
{
    /// <summary>
    /// The effect a method has on a unique (affine) builder value.
    /// </summary>
    public enum UniqueEffect
    {
        /// <summary>Reads the builder without invalidating it (Count, At). May be used freely.</summary>
        Observe,

        /// <summary>Mutates the builder in place and returns it; call sites must rebind
        /// ("xs = xs.Add(p)") or chain. The received value is dead after the call.</summary>
        Mutate,

        /// <summary>Consumes the builder (Freeze): the backing store is handed off zero-copy
        /// and the builder is invalidated. The variable is dead after the call.</summary>
        Consume,
    }

    /// <summary>
    /// The compiler's intrinsic table for the affine builder types (roadmap Phase 6:
    /// "unique type List&lt;T&gt;" / "unique type Buffer&lt;T&gt;"). One keyword, declaration-site
    /// only: uniqueness is a property of the TYPE, never of a use site — every pass of a
    /// unique value is a move. Method effects live here, not in the source language.
    ///
    /// Enforcement today is at runtime (the handwritten Plato.Intrinsics builders carry a
    /// frozen flag; use-after-freeze throws InvalidOperationException). The static affine
    /// pass (occurrence counting per control-flow path + the structural bans, see
    /// docs/affine-types.md) rides the future type-checker workstream and will consume
    /// this table unchanged. The linter warns on the two cheap structural bans (LINT006/007).
    /// </summary>
    public static class UniqueTypes
    {
        /// <summary>The only type names allowed to carry the "unique" modifier.</summary>
        public static readonly HashSet<string> Names = new HashSet<string> { "List", "Buffer" };

        public static bool IsUniqueTypeName(string name)
            => name != null && Names.Contains(name);

        /// <summary>Method effects for unique type List&lt;T&gt; (growable builder).</summary>
        public static readonly Dictionary<string, UniqueEffect> ListEffects = new Dictionary<string, UniqueEffect>
        {
            { "Count", UniqueEffect.Observe },
            { "At", UniqueEffect.Observe },
            { "Add", UniqueEffect.Mutate },
            { "AddRange", UniqueEffect.Mutate },
            { "Set", UniqueEffect.Mutate },
            { "Freeze", UniqueEffect.Consume },
        };

        /// <summary>Method effects for unique type Buffer&lt;T&gt; (fixed-size builder).</summary>
        public static readonly Dictionary<string, UniqueEffect> BufferEffects = new Dictionary<string, UniqueEffect>
        {
            { "Count", UniqueEffect.Observe },
            { "At", UniqueEffect.Observe },
            { "Set", UniqueEffect.Mutate },
            { "Freeze", UniqueEffect.Consume },
        };

        public static Dictionary<string, UniqueEffect> EffectsOf(string typeName)
            => typeName == "List" ? ListEffects
             : typeName == "Buffer" ? BufferEffects
             : null;
    }
}
