using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    public enum DiagnosticSeverity { Info, Warning, Error }

    /// <summary>
    /// A located type-checking diagnostic. The checker is *total*: instead of throwing on a
    /// no-match / ambiguity / clash, it records one of these against the originating Symbol.
    /// This is the whole ROI of the checker — it turns "mysterious downstream error" into
    /// "the checker points at the expression".
    /// </summary>
    public class CheckDiagnostic
    {
        public DiagnosticSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public Symbol Origin { get; }

        public CheckDiagnostic(DiagnosticSeverity severity, string code, string message, Symbol origin)
            => (Severity, Code, Message, Origin) = (severity, code, message, origin);

        public override string ToString()
            => $"[{Severity}] {Code}: {Message}" + (Origin != null ? $" (at {Origin.GetType().Name} #{Origin.Id})" : "");
    }

    /// <summary>Mints uniquely-named unification type variables ($V0, $Ret1, ...).</summary>
    public class TypeVarFactory
    {
        private int _n;
        public TypeExpression Fresh(string hint = "V")
            => TypeExpression.CreateTypeVar(null, $"${hint}{_n++}");
    }

    public abstract class Constraint
    {
        public Symbol Origin { get; }
        protected Constraint(Symbol origin) => Origin = origin;
    }

    /// <summary>The two types must unify.</summary>
    public class EqualityConstraint : Constraint
    {
        public TypeExpression A { get; }
        public TypeExpression B { get; }

        public EqualityConstraint(TypeExpression a, TypeExpression b, Symbol origin) : base(origin)
            => (A, B) = (a, b);

        public override string ToString() => $"{A} ~ {B}";
    }

    /// <summary>
    /// A call to an overload set must resolve to exactly one candidate: the argument types must
    /// match some candidate's (freshly instantiated) parameter types, and <see cref="Result"/>
    /// unifies with that candidate's return type. This is overload resolution as a constraint —
    /// the solver defers it until the argument types are known, then commits or reports.
    /// </summary>
    public class OverloadConstraint : Constraint
    {
        public FunctionCall Call { get; }
        public string Name { get; }
        public TypeExpression Result { get; }
        public IReadOnlyList<TypeExpression> ArgTypes { get; }
        public IReadOnlyList<FunctionDef> Candidates { get; }

        public OverloadConstraint(FunctionCall call, string name, TypeExpression result,
            IReadOnlyList<TypeExpression> argTypes, IReadOnlyList<FunctionDef> candidates, Symbol origin)
            : base(origin)
            => (Call, Name, Result, ArgTypes, Candidates) = (call, name, result, argTypes, candidates);

        public override string ToString()
            => $"{Name}({string.Join(", ", ArgTypes)}) -> {Result} [{Candidates.Count} candidates]";
    }

    /// <summary>
    /// The output of the constrain pass: the constraints to solve, the type assigned to every
    /// expression (a variable until solved), and any generation-time diagnostics.
    /// </summary>
    public class ConstraintSystem
    {
        public List<Constraint> Constraints { get; } = new List<Constraint>();
        public Dictionary<Expression, TypeExpression> ExprTypes { get; } = new Dictionary<Expression, TypeExpression>();
        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        public IEnumerable<EqualityConstraint> Equalities => Constraints.OfType<EqualityConstraint>();
        public IEnumerable<OverloadConstraint> Overloads => Constraints.OfType<OverloadConstraint>();

        public void Equate(TypeExpression a, TypeExpression b, Symbol origin)
        {
            if (a != null && b != null)
                Constraints.Add(new EqualityConstraint(a, b, origin));
        }

        public void Add(Constraint c) => Constraints.Add(c);
    }
}
