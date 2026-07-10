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

    /// <summary>How one argument matched its parameter during overload resolution, in order of
    /// preference. Recorded per argument on a <see cref="ResolvedCall"/> so the elaborator can
    /// make the implicit-conversion class an explicit IR node (see <c>TirCoerce</c>).</summary>
    public enum ArgMatchKind { Exact, Generic, Concept, Conversion }

    /// <summary>
    /// The decision the solver committed for one resolved <see cref="FunctionCall"/>: which
    /// candidate won, its instantiated (zonked) signature, and how each argument matched. This is
    /// what the elaborate pass consumes so the writers stop re-deriving callee/signature/conversion
    /// semantics at emit time. Produced only for cleanly-committed calls (a unique winner, or a
    /// CHK202 common-return tie); ambiguous / no-match calls are not recorded.
    /// </summary>
    public class ResolvedCall
    {
        public FunctionCall Call { get; }
        public FunctionDef Callee { get; }
        /// <summary>The instantiated (zonked) parameter types — the declared signature with generic
        /// holes filled and interface (concept) parameters kept as the interface.</summary>
        public IReadOnlyList<TypeExpression> ParameterTypes { get; }
        public TypeExpression ReturnType { get; }
        public IReadOnlyList<ArgMatchKind> ArgKinds { get; }
        /// <summary>The cast function for each argument, non-null only where the match kind is
        /// <see cref="ArgMatchKind.Conversion"/> and a cast relation was found.</summary>
        public IReadOnlyList<IFunction> ArgConversions { get; }

        public ResolvedCall(FunctionCall call, FunctionDef callee,
            IReadOnlyList<TypeExpression> parameterTypes, TypeExpression returnType,
            IReadOnlyList<ArgMatchKind> argKinds, IReadOnlyList<IFunction> argConversions)
            => (Call, Callee, ParameterTypes, ReturnType, ArgKinds, ArgConversions)
                = (call, callee, parameterTypes, returnType, argKinds, argConversions);

        public override string ToString()
            => $"{Callee?.Name}({string.Join(", ", ParameterTypes)}) -> {ReturnType} [{string.Join(",", ArgKinds)}]";
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
    /// A soft equality for RETURN positions: <see cref="From"/> must unify with — or implicitly
    /// convert to — <see cref="To"/>. Plato's generated C# admits an implicit conversion at the
    /// return boundary (tuple → same-shape struct, cast relation, value → implemented interface),
    /// so a hard unify would reject bodies the emitter compiles fine.
    /// </summary>
    public class CoercionConstraint : Constraint
    {
        public TypeExpression From { get; }
        public TypeExpression To { get; }

        public CoercionConstraint(TypeExpression from, TypeExpression to, Symbol origin) : base(origin)
            => (From, To) = (from, to);

        public override string ToString() => $"{From} ~> {To}";
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
        public IEnumerable<CoercionConstraint> Coercions => Constraints.OfType<CoercionConstraint>();

        public void Equate(TypeExpression a, TypeExpression b, Symbol origin)
        {
            if (a != null && b != null)
                Constraints.Add(new EqualityConstraint(a, b, origin));
        }

        public void Add(Constraint c) => Constraints.Add(c);
    }
}
