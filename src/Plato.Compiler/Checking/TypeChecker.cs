// Runs normalization, constraint solving, and elaboration for compiled functions.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Orchestrates the checker passes for a single function:
    ///   normalize (Compilation.GetNormalizedFunction) -> constrain (ConstraintGenerator)
    ///   -> solve (Solver) -> the sealed-construction pass (SealedConstruction, CHK311).
    /// Produces a <see cref="TypeCheckResult"/>. The generator and solver share one
    /// <see cref="TypeVarFactory"/> so every unification variable in the run is uniquely named.
    ///
    /// This runs in "shadow mode": it computes a fully-typed view of a function and reports
    /// diagnostics, but it does not (yet) feed elaboration or code generation.
    /// </summary>
    public class TypeChecker
    {
        public Compilation Compilation { get; }

        /// <summary>Whether any type is "sealed by" its constructors, so the sealed-construction pass
        /// has anything to look for (TKT-444).</summary>
        private readonly bool _anySealed;

        public TypeChecker(Compilation compilation)
        {
            Compilation = compilation;
            _anySealed = compilation.TypeDefinitions?.Any(t => t.IsSealed) == true;
        }

        public TypeCheckResult Check(FunctionDef f)
        {
            var normalized = Compilation.GetNormalizedFunction(f);
            var vars = new TypeVarFactory();
            var generator = new ConstraintGenerator(Compilation, vars);
            var system = generator.Generate(normalized);
            var solver = new Solver(Compilation, vars)
            {
                RigidVars = SignatureVars(f),
                // The bounds this signature's variables inherit from the constructed types it
                // mentions (`x: Tween<$T>` with `Tween<T> where T: Interpolatable`). They license
                // member lookup on a bare `$T` in the body — see Solver.BoundsPermit (plato-382).
                VarBounds = TypeConstraints.InheritedBounds(f),
            };
            solver.Solve(system);
            if (_anySealed)
                solver.Diagnostics.AddRange(SealedConstruction.Check(f, system, solver));
            return new TypeCheckResult(f, normalized, system, solver);
        }

        /// <summary>The names of a function's own signature type variables ($T, $D) — the ones that
        /// are universally quantified in its body and must stay rigid during overload matching.</summary>
        private static HashSet<string> SignatureVars(FunctionDef f)
        {
            var set = new HashSet<string>();
            void Collect(TypeExpression t)
            {
                if (t?.Def == null) return;
                if (t.Def.Kind == TypeKind.TypeVariable) set.Add(t.Name);
                foreach (var a in t.TypeArgs) Collect(a);
            }
            foreach (var p in f.Parameters ?? Enumerable.Empty<ParameterDef>())
                Collect(p.Type);
            Collect(f.ReturnType);
            return set;
        }

        /// <summary>Check every function with a body and return the per-function results.</summary>
        public IReadOnlyList<TypeCheckResult> CheckAll()
            => (Compilation.FunctionDefinitions ?? Enumerable.Empty<FunctionDef>())
                .Where(f => f.Body != null)
                .Select(Check)
                .ToList();
    }

    public class TypeCheckResult
    {
        public FunctionDef Function { get; }
        public FunctionDef Normalized { get; }
        public ConstraintSystem System { get; }
        public Solver Solver { get; }

        public TypeCheckResult(FunctionDef function, FunctionDef normalized, ConstraintSystem system, Solver solver)
            => (Function, Normalized, System, Solver) = (function, normalized, system, solver);

        public IReadOnlyList<CheckDiagnostic> Diagnostics => Solver.Diagnostics;
        public IEnumerable<CheckDiagnostic> Errors => Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
        public bool Succeeded => Solver.Succeeded;

        /// <summary>The overload decision the solver committed for each cleanly-resolved call in the
        /// normalized body — the input to the elaborate pass.</summary>
        public IReadOnlyDictionary<FunctionCall, ResolvedCall> ResolvedCalls => Solver.ResolvedCalls;

        /// <summary>The solved type of an expression in the normalized body, if one was assigned.</summary>
        public TypeExpression TypeOf(Expression e)
            => System.ExprTypes.TryGetValue(e, out var t) ? Solver.Zonk(t) : null;
    }
}
