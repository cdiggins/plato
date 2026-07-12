using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The writer-facing view of the checker pipeline: one Normalize → Constrain → Solve →
    /// Elaborate → Monomorphize run over a compilation, exposed as two lookups —
    /// the fully-ground monomorphized TIR per (source function, concrete type name) for
    /// member-instance bodies, and the GENERIC elaborated TIR per function for static bodies
    /// (constants, the IArray library functions), which are emitted unspecialized.
    /// Shared by the C#, TypeScript and Rust writers.
    /// </summary>
    public class TirEmitSource
    {
        private readonly Dictionary<(FunctionDef, string), TirFunction> _groundTirByKey
            = new Dictionary<(FunctionDef, string), TirFunction>();
        private readonly Dictionary<FunctionDef, TirFunction> _staticTirByFn
            = new Dictionary<FunctionDef, TirFunction>();

        public TirEmitSource(Compilation compilation)
        {
            // One checker+elaborator run feeds both maps; MonomorphizeAll is total (never throws).
            var mono = new Monomorphizer(compilation);
            var elaborated = mono.ElaborateAll();

            // Static bodies are emitted from name + shape, so residual generic types are fine;
            // an unresolved CALL is not (its emission would be a guess).
            foreach (var kv in elaborated)
                if (kv.Value?.Body != null && !kv.Value.AllNodes.Any(n => n is TirUnresolved))
                    _staticTirByFn[kv.Key] = kv.Value;

            foreach (var m in mono.MonomorphizeAll(elaborated))
            {
                if (!m.HasBody || !m.IsFullyGround)
                    continue;
                var key = (m.Original, m.ConcreteType?.Name);
                if (m.Original != null && key.Item2 != null && !_groundTirByKey.ContainsKey(key))
                    _groundTirByKey[key] = m.Tir;
            }
        }

        /// <summary>The fully-ground monomorphized TIR body for a (source function, concrete type),
        /// or null when none exists (non-ground / unresolved / not bodied).</summary>
        public TirFunction TryGetGroundTir(FunctionDef original, TypeDef concreteType)
            => TryGetGroundTir(original, concreteType?.Name);

        /// <summary>Name-keyed variant, for callers that know the receiver's concrete type only as
        /// the solved type NAME (e.g. the inliner looking up a callee's body).</summary>
        public TirFunction TryGetGroundTir(FunctionDef original, string concreteTypeName)
            => original != null && concreteTypeName != null
                && _groundTirByKey.TryGetValue((original, concreteTypeName), out var tir)
                    ? tir
                    : null;

        /// <summary>The generic (unspecialized) TIR for a STATIC body, or null when the
        /// function's elaboration has unresolved nodes.</summary>
        public TirFunction TryGetStaticTir(FunctionDef original)
            => original != null && _staticTirByFn.TryGetValue(original, out var tir) ? tir : null;
    }
}
