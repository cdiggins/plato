using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The solve pass. Consumes a <see cref="ConstraintSystem"/> and produces a substitution
    /// (a binding for each unification variable) plus located diagnostics. It is:
    ///
    ///   * total — never throws; a clash / no-match / ambiguity / recursive type is a diagnostic;
    ///   * occurs-checked — a variable is never bound to a type that contains it (no infinite types);
    ///   * deferred-commitment — an overloaded call is resolved only once its argument types are
    ///     ground; the most-specific surviving candidate is committed, 0 is a no-match error, and an
    ///     unresolved tie with differing return types is reported rather than silently picked.
    ///
    /// Argument matching is tiered, mirroring Plato's own resolution primitives
    /// (<see cref="TypeExtensions.IsImplementing"/> and <see cref="Compilation.GetRelation"/>):
    ///
    ///   exact (unify) &lt; generic (bind a type variable) &lt; concept (arg implements an interface
    ///   parameter, with the return refined to the concrete arg — "Self") &lt; conversion (an implicit
    ///   cast relation exists). Lower cost wins, so a concrete overload beats a generic one and an
    ///   exact match beats a conversion.
    /// </summary>
    public class Solver
    {
        public Compilation Compilation { get; }
        public Dictionary<string, TypeExpression> Substitution { get; } = new Dictionary<string, TypeExpression>();
        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        private readonly TypeVarFactory _vars;
        private const int MaxDepth = 512;

        // Argument-match cost tiers (lower is more specific).
        private const int ExactCost = 0;
        private const int GenericCost = 2;
        private const int ConceptCost = 3;
        private const int ConversionCost = 5;

        public Solver(Compilation compilation, TypeVarFactory vars = null)
        {
            Compilation = compilation;
            _vars = vars ?? new TypeVarFactory();
        }

        public void Solve(ConstraintSystem system)
        {
            Diagnostics.AddRange(system.Diagnostics);

            // Phase 1: equalities are unconditional.
            foreach (var eq in system.Equalities)
                Unify(eq.A, eq.B, Substitution, eq.Origin, record: true);

            // Phase 2: overloads to a fixpoint. A call is resolved when its argument types are
            // ground; resolving one may ground another (its result feeds an outer call).
            var pending = system.Overloads.ToList();
            bool progress = true;
            while (progress && pending.Count > 0)
            {
                progress = false;
                var still = new List<OverloadConstraint>();
                foreach (var oc in pending)
                {
                    if (ResolveOverload(oc, force: false)) progress = true;
                    else still.Add(oc);
                }
                pending = still;
            }

            // Whatever could not be grounded is resolved on a best-effort basis and reported.
            foreach (var oc in pending)
                ResolveOverload(oc, force: true);
        }

        // --- overload resolution -------------------------------------------------

        /// <summary>An instantiated candidate: fresh generic holes, interface params replaced by
        /// fresh "concept" variables (recorded in <see cref="ConceptOf"/>), and a return type
        /// refined to share a concept variable when it names the same interface as a parameter.</summary>
        private class Candidate
        {
            public FunctionDef Function;
            public TypeExpression[] Ps;
            public TypeExpression Ret;
            public Dictionary<string, TypeExpression> ConceptOf = new Dictionary<string, TypeExpression>();
        }

        private bool ResolveOverload(OverloadConstraint oc, bool force)
        {
            var args = oc.ArgTypes.Select(a => Zonk(a, Substitution)).ToList();

            if (!force && args.Any(HasVar))
                return false; // not ground yet — defer

            // Trial each candidate on a scratch substitution; keep the viable ones with their cost.
            var viable = new List<(Candidate cand, int cost)>();
            foreach (var f in oc.Candidates)
            {
                if (f.Parameters.Count != args.Count)
                    continue;
                var cand = InstantiateCandidate(f);
                var scratch = new Dictionary<string, TypeExpression>(Substitution);
                var cost = 0;
                var ok = true;
                for (var i = 0; i < args.Count; i++)
                {
                    var m = MatchArg(args[i], cand.Ps[i], cand, scratch);
                    if (!m.ok) { ok = false; break; }
                    cost += m.cost;
                }
                if (ok)
                    viable.Add((cand, cost));
            }

            if (viable.Count == 0)
            {
                Report(DiagnosticSeverity.Error, "CHK201",
                    $"No overload of '{oc.Name}' matches argument types ({string.Join(", ", args)})", oc.Origin);
                return true;
            }

            // "Most specific wins": keep only the lowest-cost candidates.
            var best = viable.Min(v => v.cost);
            var winners = viable.Where(v => v.cost == best).Select(v => v.cand).ToList();

            if (winners.Count == 1)
            {
                CommitCandidate(winners[0], args, oc);
                return true;
            }

            // A genuine tie at the best cost: well-defined iff all winners agree on the return type.
            var rets = winners
                .Select(w => Zonk(w.Ret, new Dictionary<string, TypeExpression>(Substitution)).ToString())
                .Distinct().ToList();

            if (rets.Count == 1)
            {
                CommitCandidate(winners[0], args, oc);
                Report(DiagnosticSeverity.Info, "CHK202",
                    $"Call to '{oc.Name}' has {winners.Count} equally-specific overloads with a common return type", oc.Origin);
            }
            else
            {
                Report(DiagnosticSeverity.Error, "CHK203",
                    $"Ambiguous call to '{oc.Name}': {winners.Count} equally-specific overloads with return types {string.Join(" | ", rets)}", oc.Origin);
            }
            return true;
        }

        private void CommitCandidate(Candidate cand, IReadOnlyList<TypeExpression> args, OverloadConstraint oc)
        {
            // Re-run the matches on the real substitution to bind generic/concept variables, then
            // unify the call's result with the (possibly refined) return type.
            for (var i = 0; i < args.Count; i++)
                MatchArg(args[i], cand.Ps[i], cand, Substitution);
            Unify(oc.Result, cand.Ret, Substitution, oc.Origin, record: true);
        }

        /// <summary>
        /// Match one argument against one (instantiated) parameter, returning success and a cost.
        /// Order of preference: exact unify, generic binding, concept satisfaction, implicit cast.
        /// </summary>
        private (bool ok, int cost) MatchArg(TypeExpression argType, TypeExpression param, Candidate cand, Dictionary<string, TypeExpression> sub)
        {
            if (param == null || argType == null)
                return (true, ExactCost);

            // A concept parameter: the argument must implement the interface (or convert to it).
            if (IsVar(param) && cand.ConceptOf.TryGetValue(param.Name, out var concept))
            {
                if (Implements(argType, concept))
                {
                    Unify(param, argType, sub, null, record: false); // Self: bind the concept var to the concrete arg
                    return (true, ConceptCost);
                }
                if (HasConversion(argType, concept))
                    return (true, ConversionCost);
                return (false, 0);
            }

            var generic = IsVar(ResolveTop(param, sub));
            if (Unify(param, argType, sub, null, record: false))
                return (true, generic ? GenericCost : ExactCost);

            if (HasConversion(argType, param))
                return (true, ConversionCost);

            return (false, 0);
        }

        private static bool Implements(TypeExpression argType, TypeExpression concept)
            => argType.IsImplementing(concept);

        private bool HasConversion(TypeExpression from, TypeExpression to)
        {
            // Look up cast relations directly rather than via Compilation.GetRelation, which throws
            // when a source has multiple relations to the same destination. The solver must stay total.
            if (Compilation?.TypeRelations == null || from?.Def == null || to?.Def == null)
                return false;
            if (!Compilation.TypeRelations.RelationLookup.TryGetValue(from.Def, out var list))
                return false;
            return list.Any(rel => rel.Cast != null && rel.Dest.Equals(to.Def));
        }

        // --- instantiation -------------------------------------------------------

        /// <summary>
        /// Give a candidate's generic holes fresh unification variables, replace interface
        /// parameters with fresh "concept" variables (so a concrete argument binds through them),
        /// and refine an interface return type to the concept variable of the parameter it names.
        /// </summary>
        private Candidate InstantiateCandidate(FunctionDef f)
        {
            var map = new Dictionary<string, TypeExpression>();
            foreach (var t in f.Parameters.Select(p => p.Type).Append(f.ReturnType))
                CollectHoles(t, map);

            var cand = new Candidate { Function = f, Ps = new TypeExpression[f.Parameters.Count] };
            var conceptVarByInstance = new Dictionary<string, TypeExpression>();

            for (var i = 0; i < cand.Ps.Length; i++)
            {
                var pt = Substitute(f.Parameters[i].Type, map);
                if (pt?.Def != null && pt.Def.IsInterface())
                {
                    var v = _vars.Fresh("C");
                    cand.ConceptOf[v.Name] = pt;
                    conceptVarByInstance[pt.ToString()] = v;
                    cand.Ps[i] = v;
                }
                else
                {
                    cand.Ps[i] = pt;
                }
            }

            var ret = Substitute(f.ReturnType, map);
            if (ret != null && conceptVarByInstance.TryGetValue(ret.ToString(), out var rv))
                ret = rv; // Self-style refinement: return the concrete argument's type
            cand.Ret = ret;

            return cand;
        }

        private void CollectHoles(TypeExpression t, Dictionary<string, TypeExpression> map)
        {
            if (t == null) return;
            if (IsHole(t) && !map.ContainsKey(t.Name))
                map[t.Name] = _vars.Fresh("G");
            foreach (var a in t.TypeArgs)
                CollectHoles(a, map);
        }

        private static bool IsHole(TypeExpression t)
            => t.Def != null && (t.Def.Kind == TypeKind.TypeVariable || t.Def.Kind == TypeKind.TypeParameter);

        private TypeExpression Substitute(TypeExpression t, IReadOnlyDictionary<string, TypeExpression> map)
        {
            if (t == null) return null;
            if (map.TryGetValue(t.Name, out var replace))
                return replace;
            if (t.TypeArgs.Count == 0)
                return t;
            return new TypeExpression(t.Def, t.TypeArgs.Select(a => Substitute(a, map)).ToArray());
        }

        // --- unification ---------------------------------------------------------

        private static bool IsVar(TypeExpression t)
            => t.Def != null && t.Def.Kind == TypeKind.TypeVariable;

        public bool Unify(TypeExpression a, TypeExpression b, Dictionary<string, TypeExpression> sub, Symbol origin, bool record, int depth = 0)
        {
            if (a == null || b == null || depth > MaxDepth)
                return true;

            a = ResolveTop(a, sub);
            b = ResolveTop(b, sub);

            if (IsVar(a) && IsVar(b) && a.Name == b.Name)
                return true;
            if (IsVar(a))
                return Bind(a, b, sub, origin, record);
            if (IsVar(b))
                return Bind(b, a, sub, origin, record);

            // Both are named (or rigid type parameters): match nominally.
            if (a.Def?.Name != b.Def?.Name)
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK101", $"Cannot unify '{a}' with '{b}'", origin);
                return false;
            }
            if (a.TypeArgs.Count != b.TypeArgs.Count)
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK102", $"Type argument count mismatch between '{a}' and '{b}'", origin);
                return false;
            }

            var ok = true;
            for (var i = 0; i < a.TypeArgs.Count; i++)
                ok &= Unify(a.TypeArgs[i], b.TypeArgs[i], sub, origin, record, depth + 1);
            return ok;
        }

        private bool Bind(TypeExpression var, TypeExpression t, Dictionary<string, TypeExpression> sub, Symbol origin, bool record)
        {
            if (IsVar(t) && t.Name == var.Name)
                return true;
            if (Occurs(var.Name, t, sub))
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK103", $"Recursive type: '{var.Name}' occurs in '{Zonk(t, sub)}'", origin);
                return false;
            }
            sub[var.Name] = t;
            return true;
        }

        private bool Occurs(string name, TypeExpression t, Dictionary<string, TypeExpression> sub, int depth = 0)
        {
            if (t == null || depth > MaxDepth) return false;
            t = ResolveTop(t, sub);
            if (IsVar(t))
                return t.Name == name;
            return t.TypeArgs.Any(a => Occurs(name, a, sub, depth + 1));
        }

        private static TypeExpression ResolveTop(TypeExpression t, Dictionary<string, TypeExpression> sub)
        {
            var depth = 0;
            while (t != null && IsVar(t) && sub.TryGetValue(t.Name, out var u) && depth++ < MaxDepth)
                t = u;
            return t;
        }

        /// <summary>Fully apply the substitution (deep), yielding a variable-free type where solved.</summary>
        public TypeExpression Zonk(TypeExpression t, Dictionary<string, TypeExpression> sub = null, int depth = 0)
        {
            sub ??= Substitution;
            if (t == null || depth > MaxDepth) return t;
            t = ResolveTop(t, sub);
            if (t == null || t.TypeArgs.Count == 0)
                return t;
            return new TypeExpression(t.Def, t.TypeArgs.Select(a => Zonk(a, sub, depth + 1)).ToArray());
        }

        private static bool HasVar(TypeExpression t)
            => t != null && (IsVar(t) || t.TypeArgs.Any(HasVar));

        private void Report(DiagnosticSeverity severity, string code, string message, Symbol origin)
            => Diagnostics.Add(new CheckDiagnostic(severity, code, message, origin));

        public bool Succeeded => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
    }
}
