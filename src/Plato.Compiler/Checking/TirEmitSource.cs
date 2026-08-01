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
        private readonly Dictionary<(FunctionDef, string), TirFunction> _openGenericTirByKey
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
                if (!m.HasBody)
                    continue;
                var key = (m.Original, m.ConcreteType?.Name);
                if (m.Original == null || key.Item2 == null)
                    continue;
                if (m.IsFullyGround)
                {
                    if (!_groundTirByKey.ContainsKey(key))
                        _groundTirByKey[key] = m.Tir;
                }
                else if (IsOpenGenericEmittable(m.Tir) && !_openGenericTirByKey.ContainsKey(key))
                {
                    _openGenericTirByKey[key] = m.Tir;
                }
            }
        }

        /// <summary>
        /// Whether a specialized-but-not-ground TIR body is TYPE-AGNOSTIC in its residual type
        /// variables, and so can be emitted ONCE as an open-generic member instead of a throwing
        /// stub (plato: the "no ground instantiation" degradation for library functions over a
        /// generic concrete type — <c>Map(xs: Array2D&lt;$T&gt;, f)</c> and friends, which the
        /// monomorphizer never stamps because the compiled tier holds no call site at a concrete T).
        ///
        /// Type-agnostic means the body only MOVES values of the residual variable — indexes,
        /// stores, passes them to a function value, constructs an <c>Array2D&lt;T&gt;</c> — and
        /// never performs an operation ON one. The test is conservative and structural:
        ///   * no unresolved elaboration node (its emission would be a guess), and
        ///   * no call that DISPATCHES on a residual type variable, i.e. whose receiver argument's
        ///     type is BARE and abstract (a type variable / type parameter / Self, with no type
        ///     arguments of its own). That is the case the C# type parameter cannot satisfy — a
        ///     body needing <c>Subtract</c> or <c>Lerp</c> on a bare T, e.g.
        ///     <c>TimeVaryingValues.Change</c> — and it stays a throwing stub.
        /// A residual variable in any other position (element of an array, argument of a passed-in
        /// function, type argument of the receiver or of a constructed type) is exactly the type
        /// parameter the emitted signature already declares, so the body is valid C# as written.
        /// </summary>
        // Public so FallbackDiagnosticsTests can mirror the emit path's coverage rule exactly.
        public static bool IsOpenGenericEmittable(TirFunction tir)
        {
            if (tir?.Body == null)
                return false;
            foreach (var n in tir.AllNodes)
            {
                if (n is TirUnresolved)
                    return false;
                if (n is TirCall c && c.Args.Count > 0 && DispatchesOnAbstractReceiver(c))
                    return false;
            }
            return true;
        }

        private static bool DispatchesOnAbstractReceiver(TirCall c)
        {
            var recv = c.Args[0];
            while (recv is TirCoerce coerce)
                recv = coerce.Inner;
            var t = recv?.Type ?? (c.ParameterTypes.Count > 0 ? c.ParameterTypes[0] : null);
            // A bare abstract head with no type arguments: the receiver IS the unknown type.
            return t?.Def != null && t.TypeArgs.Count == 0 && !TypeSubstitution.IsGround(t);
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

        /// <summary>The OPEN-GENERIC monomorphized TIR body for a (source function, concrete type):
        /// specialized as far as the concrete receiver allows, with the type variables the emitted
        /// C# signature declares as its own type parameters left open. Null unless the body is
        /// type-agnostic in them (see <see cref="IsOpenGenericEmittable"/>). Callers use it as the
        /// fallback when <see cref="TryGetGroundTir(FunctionDef, TypeDef)"/> finds nothing.</summary>
        public TirFunction TryGetOpenGenericTir(FunctionDef original, TypeDef concreteType)
            => TryGetOpenGenericTir(original, concreteType?.Name);

        public TirFunction TryGetOpenGenericTir(FunctionDef original, string concreteTypeName)
            => original != null && concreteTypeName != null
                && _openGenericTirByKey.TryGetValue((original, concreteTypeName), out var tir)
                    ? tir
                    : null;

        /// <summary>The generic (unspecialized) TIR for a STATIC body, or null when the
        /// function's elaboration has unresolved nodes.</summary>
        public TirFunction TryGetStaticTir(FunctionDef original)
            => original != null && _staticTirByFn.TryGetValue(original, out var tir) ? tir : null;

        /// <summary>Test helper: the ground TIR for a (concrete type name, function name) pair,
        /// looked up by NAME rather than by <see cref="FunctionDef"/> identity — for in-proc TIR
        /// shape tests that pin flagship functions (M0 optimizer instrumentation). Returns the
        /// first match; null when none exists.</summary>
        public TirFunction TryGetGroundTirByNames(string typeName, string functionName)
            => _groundTirByKey
                .Where(kv => kv.Key.Item2 == typeName && kv.Key.Item1?.Name == functionName)
                .Select(kv => kv.Value)
                .FirstOrDefault();
    }
}
