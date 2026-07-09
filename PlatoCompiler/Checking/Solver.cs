using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The solve pass. Consumes a <see cref="ConstraintSystem"/> and produces a substitution
    /// (a binding for each unification variable) plus located diagnostics. It is:
    ///
    ///   * total — never throws; a clash / no-match / ambiguity / recursive type is a diagnostic;
    ///   * occurs-checked — a variable is never bound to a type that contains it (no infinite types);
    ///   * deferred-commitment — an overloaded call is resolved only once its argument types are
    ///     ground; a unique surviving candidate is committed, 0 is a no-match error, and >1 with
    ///     differing return types is reported as an ambiguity rather than silently picking the first.
    ///
    /// Unification is over the nominal <see cref="TypeExpression"/> representation: only
    /// $-prefixed <c>TypeVariable</c>s are flexible; declared <c>TypeParameter</c>s (rigid) and
    /// named types match by name. This first cut has no implicit conversions or concept
    /// satisfaction — those calls are *reported*, which is exactly the intended shadow-mode
    /// behavior.
    /// </summary>
    public class Solver
    {
        public Compilation Compilation { get; }
        public Dictionary<string, TypeExpression> Substitution { get; } = new Dictionary<string, TypeExpression>();
        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        private readonly TypeVarFactory _vars;
        private const int MaxDepth = 512;

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

        private bool ResolveOverload(OverloadConstraint oc, bool force)
        {
            var args = oc.ArgTypes.Select(a => Zonk(a, Substitution)).ToList();

            if (!force && args.Any(HasVar))
                return false; // not ground yet — defer

            // Trial each candidate on a scratch substitution; keep the ones that unify.
            var viable = new List<(TypeExpression[] ps, TypeExpression ret)>();
            foreach (var f in oc.Candidates)
            {
                if (f.Parameters.Count != args.Count)
                    continue;
                var inst = Instantiate(f);
                var scratch = new Dictionary<string, TypeExpression>(Substitution);
                var ok = true;
                for (var i = 0; i < args.Count; i++)
                {
                    if (!Unify(inst.ps[i], args[i], scratch, oc.Origin, record: false))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    viable.Add(inst);
            }

            if (viable.Count == 0)
            {
                Report(DiagnosticSeverity.Error, "CHK201",
                    $"No overload of '{oc.Name}' matches argument types ({string.Join(", ", args)})", oc.Origin);
                return true;
            }

            if (viable.Count == 1)
            {
                Commit(viable[0], args, oc);
                return true;
            }

            // Multiple candidates survive. If they all yield the same return type, the call is
            // still well-defined (bind the result); otherwise it is a genuine ambiguity.
            var rets = viable
                .Select(v => Zonk(v.ret, new Dictionary<string, TypeExpression>(Substitution)).ToString())
                .Distinct().ToList();

            if (rets.Count == 1)
            {
                Unify(oc.Result, viable[0].ret, Substitution, oc.Origin, record: true);
                Report(DiagnosticSeverity.Info, "CHK202",
                    $"Call to '{oc.Name}' has {viable.Count} matching overloads with a common return type", oc.Origin);
            }
            else
            {
                Report(DiagnosticSeverity.Error, "CHK203",
                    $"Ambiguous call to '{oc.Name}': {viable.Count} overloads match with return types {string.Join(" | ", rets)}", oc.Origin);
            }
            return true;
        }

        private void Commit((TypeExpression[] ps, TypeExpression ret) cand, IReadOnlyList<TypeExpression> args, OverloadConstraint oc)
        {
            for (var i = 0; i < args.Count; i++)
                Unify(cand.ps[i], args[i], Substitution, oc.Origin, record: true);
            Unify(oc.Result, cand.ret, Substitution, oc.Origin, record: true);
        }

        // --- instantiation -------------------------------------------------------

        /// <summary>
        /// Give a candidate's generic holes (its $-type-variables and declared type parameters)
        /// fresh unification variables, so each use of an overload is independent.
        /// </summary>
        private (TypeExpression[] ps, TypeExpression ret) Instantiate(FunctionDef f)
        {
            var map = new Dictionary<string, TypeExpression>();
            foreach (var t in f.Parameters.Select(p => p.Type).Append(f.ReturnType))
                CollectHoles(t, map);
            var ps = f.Parameters.Select(p => Substitute(p.Type, map)).ToArray();
            var ret = Substitute(f.ReturnType, map);
            return (ps, ret);
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
