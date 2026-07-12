using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Mechanically verifies that the Typed IR the writers emit from is internally type-consistent —
    /// the precise, testable definition of "the emitter can trust the TIR" that the type-directed
    /// scalar-lowering endgame (docs/plato-tir-scalar-lowering-plan) depends on. It re-runs
    /// Elaborate → Monomorphize (the same selection <see cref="TirEmitSource"/> exposes: fully-ground
    /// member-instance bodies plus the generic elaborated static bodies) and walks every node,
    /// reporting violations grouped by <see cref="TirVerifyRule"/>:
    ///
    ///   R1 NullType         — a VALUE node carries no solved type (statements are exempt).
    ///   R2 Unresolved       — a <see cref="TirUnresolved"/> reached emission.
    ///   R3 SyntacticCall    — a <see cref="TirCall"/> with no <see cref="TirCall.Callee"/> (the
    ///                         handwritten-intrinsic / unseen-target census; the allowlist to shrink).
    ///   R4 ArgParamMismatch — a ground-concrete argument type differs from the callee's ground
    ///                         declared parameter type WITHOUT an explicit <see cref="TirCoerce"/>
    ///                         (the elaborator's contract is that every implicit conversion is a node).
    ///   R5 CoerceInconsistent — a <see cref="TirCoerce"/> whose FromType ≠ Inner.Type or Type ≠ ToType.
    ///   R6 ResidualTypeVar  — a fully-ground member body still contains an unbound type VARIABLE
    ///                         (the solver's permissive Self / type-parameter residue is reported
    ///                         separately as informational, not a hard violation).
    ///
    /// It NEVER changes emitted output; it is a gate/measurement only. Total (never throws).
    /// </summary>
    public class TirTypeVerifier
    {
        public enum TirVerifyRule
        {
            R1_NullType,
            R2_Unresolved,
            R3_SyntacticCall,
            R4_ArgParamMismatch,
            R5_CoerceInconsistent,
            R6_ResidualTypeVar,
        }

        public readonly struct Violation
        {
            public readonly TirVerifyRule Rule;
            public readonly string Scope;   // "OwnerType.Function" the node lives in
            public readonly string Detail;  // the offending node / type pair
            public Violation(TirVerifyRule rule, string scope, string detail)
                => (Rule, Scope, Detail) = (rule, scope, detail);
        }

        private readonly Compilation _compilation;
        private readonly List<Violation> _violations = new List<Violation>();

        // Informational, not counted as hard violations: bodies whose residue is only a Self type or
        // a rigid type parameter (permissive-by-design), and the distinct null-callee names seen.
        public int SelfResidueBodies { get; private set; }
        public int TypeParameterResidueBodies { get; private set; }
        public HashSet<string> SyntacticCallNames { get; } = new HashSet<string>();

        public IReadOnlyList<Violation> Violations => _violations;

        public TirTypeVerifier(Compilation compilation) => _compilation = compilation;

        /// <summary>Run the verifier over the whole compilation's emitted TIR and return the
        /// violations (also stored on the instance for the informational counters).</summary>
        public IReadOnlyList<Violation> Verify()
        {
            var mono = new Monomorphizer(_compilation);
            var elaborated = mono.ElaborateAll();

            // Static bodies: generic elaborated TIR, emitted by name+shape — residual type PARAMETERS
            // and Self are legitimate here, so R6 does not apply; the rest does.
            foreach (var kv in elaborated)
                if (kv.Value?.Body != null && !kv.Value.AllNodes.Any(n => n is TirUnresolved))
                    VerifyBody(kv.Value, ScopeOf(kv.Key), groundMember: false);

            // Member-instance bodies: only the fully-ground ones are emitted; these must carry no
            // unbound type variable (R6).
            var seen = new HashSet<(FunctionDef, string)>();
            foreach (var m in mono.MonomorphizeAll(elaborated))
            {
                if (!m.HasBody || !m.IsFullyGround)
                    continue;
                var key = (m.Original, m.ConcreteType?.Name);
                if (m.Original == null || key.Item2 == null || !seen.Add(key))
                    continue;
                VerifyBody(m.Tir, $"{key.Item2}.{m.Original.Name}", groundMember: true);
            }

            return _violations;
        }

        private static string ScopeOf(FunctionDef f)
            => $"{f?.OwnerType?.Name}.{f?.Name}";

        private void VerifyBody(TirFunction tir, string scope, bool groundMember)
        {
            var sawTypeVar = false;
            var sawSelf = false;
            var sawTypeParam = false;

            foreach (var n in tir.AllNodes)
            {
                if (n == null)
                    continue;

                switch (n)
                {
                    case TirUnresolved u:
                        Add(TirVerifyRule.R2_Unresolved, scope, u.Original?.Name ?? "?");
                        break;

                    case TirCall call when call.Callee == null && call.Args.Count > 0:
                        // Zero-arg calls (bare constant groups) legitimately carry a null callee and
                        // a name; only argument-bearing null-callee calls are the syntactic-intrinsic
                        // census this rule tracks.
                        Add(TirVerifyRule.R3_SyntacticCall, scope, call.Name ?? "?");
                        SyntacticCallNames.Add(call.Name ?? "?");
                        break;

                    case TirCoerce co:
                        if (!TypeNameEq(co.FromType, co.Inner?.Type) || !TypeNameEq(co.Type, co.ToType))
                            Add(TirVerifyRule.R5_CoerceInconsistent, scope,
                                $"coerce<{co.FromType}→{co.ToType}> over inner:{co.Inner?.Type}");
                        break;
                }

                if (n is TirCall c2 && c2.Callee != null)
                    VerifyCallArgs(c2, scope);

                if (IsValueNode(n) && n.Type == null)
                    Add(TirVerifyRule.R1_NullType, scope, n.GetType().Name);

                foreach (var t in TypesOf(n))
                {
                    if (ContainsKind(t, TypeKind.TypeVariable)) sawTypeVar = true;
                    if (ContainsKind(t, TypeKind.SelfType)) sawSelf = true;
                    if (ContainsKind(t, TypeKind.TypeParameter)) sawTypeParam = true;
                }
            }

            if (groundMember && sawTypeVar)
                Add(TirVerifyRule.R6_ResidualTypeVar, scope, "unbound type variable in ground body");
            if (groundMember && sawSelf) SelfResidueBodies++;
            if (groundMember && sawTypeParam) TypeParameterResidueBodies++;
        }

        private void VerifyCallArgs(TirCall call, string scope)
        {
            var ps = call.ParameterTypes;
            if (ps == null)
                return;
            for (var i = 0; i < call.Args.Count && i < ps.Count; i++)
            {
                var arg = call.Args[i];
                if (arg is TirCoerce)
                    continue; // conversion is explicit — the contract is satisfied
                var pt = ps[i];
                var at = arg?.Type;
                if (pt == null || at == null)
                    continue;
                // Only ground-concrete-vs-ground-concrete NAME mismatches are hard: a var / Self /
                // interface / type-parameter on either side is a legitimate wildcard the reifier or
                // C#'s implicit conversions absorb.
                if (IsGroundConcrete(pt) && IsGroundConcrete(at) && !TypeNameEq(pt, at))
                    Add(TirVerifyRule.R4_ArgParamMismatch, scope, $"{call.Name}: arg {at} vs param {pt}");
            }
        }

        // --- node/type helpers ---------------------------------------------------

        // Value nodes must carry a type. Statements (block/return/if/loop) and pure declarations
        // (let) do not; TirName's type is documented best-effort, so it is exempt from R1.
        private static bool IsValueNode(TirNode n)
            => !(n is TirBlock || n is TirReturn || n is TirIf || n is TirLoop
                 || n is TirLet || n is TirName || n is TirUnresolved);

        private static IEnumerable<TypeExpression> TypesOf(TirNode n)
        {
            if (n.Type != null) yield return n.Type;
            if (n is TirCall c)
            {
                if (c.ReturnType != null) yield return c.ReturnType;
                if (c.ParameterTypes != null)
                    foreach (var p in c.ParameterTypes)
                        if (p != null) yield return p;
            }
            if (n is TirCoerce co)
            {
                if (co.FromType != null) yield return co.FromType;
                if (co.ToType != null) yield return co.ToType;
            }
        }

        private static bool ContainsKind(TypeExpression t, TypeKind kind)
        {
            if (t?.Def == null) return false;
            if (t.Def.Kind == kind) return true;
            return t.TypeArgs.Any(a => ContainsKind(a, kind));
        }

        private static bool IsGroundConcrete(TypeExpression t)
            => t?.Def != null
               && (t.Def.Kind == TypeKind.ConcreteType || t.Def.Kind == TypeKind.Primitive)
               && !ContainsKind(t, TypeKind.TypeVariable)
               && !ContainsKind(t, TypeKind.TypeParameter)
               && !ContainsKind(t, TypeKind.SelfType);

        private static bool TypeNameEq(TypeExpression a, TypeExpression b)
            => (a?.ToString() ?? "∅") == (b?.ToString() ?? "∅");

        private void Add(TirVerifyRule rule, string scope, string detail)
            => _violations.Add(new Violation(rule, scope, detail));
    }
}
