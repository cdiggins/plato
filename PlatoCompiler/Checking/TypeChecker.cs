using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Orchestrates the checker passes for a single function:
    ///   normalize (Compilation.GetNormalizedFunction) -> constrain (ConstraintGenerator)
    ///   -> solve (Solver).
    /// Produces a <see cref="TypeCheckResult"/>. The generator and solver share one
    /// <see cref="TypeVarFactory"/> so every unification variable in the run is uniquely named.
    ///
    /// This runs in "shadow mode": it computes a fully-typed view of a function and reports
    /// diagnostics, but it does not (yet) feed elaboration or code generation.
    /// </summary>
    public class TypeChecker
    {
        public Compilation Compilation { get; }

        public TypeChecker(Compilation compilation)
            => Compilation = compilation;

        public TypeCheckResult Check(FunctionDef f)
        {
            var normalized = Compilation.GetNormalizedFunction(f);
            var vars = new TypeVarFactory();
            var generator = new ConstraintGenerator(Compilation, vars);
            var system = generator.Generate(normalized);
            var solver = new Solver(Compilation, vars);
            solver.Solve(system);
            return new TypeCheckResult(f, normalized, system, solver);
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

        /// <summary>The solved type of an expression in the normalized body, if one was assigned.</summary>
        public TypeExpression TypeOf(Expression e)
            => System.ExprTypes.TryGetValue(e, out var t) ? Solver.Zonk(t) : null;
    }
}
