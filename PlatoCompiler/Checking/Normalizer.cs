using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Normalization / desugaring pass over the *bound Symbol graph* (the output of
    /// <see cref="SymbolFactory"/>). It produces a canonical, checker-ready form that the
    /// constrain and solve passes consume.
    ///
    /// IMPORTANT CONTEXT: the parser (<c>AstNodeFactory</c>) and <see cref="SymbolFactory"/>
    /// already desugar most surface syntax before we ever see a Symbol:
    ///   * binary/unary operators  -> FunctionCall (a + b => Add(a, b), -x => Negative(x))
    ///   * member access 'a.b'      -> receiver-first FunctionCall with HasArgList=false
    ///   * UFCS 'a.b(c, d)'         -> FunctionCall b(a, c, d) with HasArgList=true
    ///   * indexer 'a[i]'           -> At(a, i)
    ///   * parentheses '(e)'        -> dropped (SymbolFactory returns the inner symbol)
    /// So this pass does NOT re-lower operators or UFCS. Its job is to (a) guarantee the
    /// invariants the constrain pass relies on (see <see cref="NormalizationInvariants"/>)
    /// and (b) perform the residual rewrites the front-end does not:
    ///
    ///   R1. Strip any residual <see cref="Parenthesized"/>:  (e) -> e
    ///   R2. Unwrap a single-element <see cref="MultiStatement"/>:  Multi(s) -> s
    ///   R3. Eta-expand a <see cref="FunctionGroupRefSymbol"/> used in *value* position
    ///       (i.e. not as the callee of a FunctionCall) into a <see cref="Lambda"/>, when
    ///       every candidate in the group shares a single non-zero arity:
    ///         g  ->  (p0, .., pN-1) => g(p0, .., pN-1)
    ///       This turns every first-class use of an overload set into an ordinary lambda,
    ///       so the constrain pass only ever meets a group reference in callee position.
    ///
    /// The pass is behavior-preserving (Plato is pure, so eta-expansion is sound) and
    /// idempotent: Normalize(Normalize(x)) is structurally equal to Normalize(x).
    ///
    /// Identity discipline: the pass NEVER clones a <see cref="DefSymbol"/> that is
    /// referenced by identity (parameters, local variable defs). It reuses parameter
    /// instances verbatim so that <see cref="ParameterRefSymbol"/>/<see cref="VariableRefSymbol"/>
    /// keep resolving. It rebuilds only Expression/Statement nodes and (nested) lambda bodies.
    /// The original Symbol graph consumed by the emitter is left untouched — the normalized
    /// form is a parallel artifact.
    /// </summary>
    public class Normalizer
    {
        private int _etaId;

        /// <summary>Normalize a function: reuse its parameters, normalize its body.</summary>
        public FunctionDef NormalizeFunction(FunctionDef f)
        {
            if (f == null)
                return null;
            var body = Normalize(f.Body);
            return new FunctionDef(f.Scope, f.Name, f.OwnerType, f.ReturnType, body,
                f.Parameters.ToArray());
        }

        private Expression Expr(Symbol s)
            => (Expression)Normalize(s);

        public Symbol Normalize(Symbol s)
        {
            switch (s)
            {
                case null:
                    return null;

                // R1: strip residual parentheses.
                case Parenthesized p:
                    return Normalize(p.Expression);

                // R3: a group reference that reaches this switch is in *value* position
                // (callee group refs are intercepted by the FunctionCall case below).
                case FunctionGroupRefSymbol g:
                    return EtaExpand(g);

                case FunctionCall fc:
                {
                    // Keep a group reference as the callee (do NOT eta-expand it); normalize
                    // any other callee expression (lambda / function-typed parameter / etc.).
                    var callee = fc.Function is FunctionGroupRefSymbol
                        ? fc.Function
                        : Expr(fc.Function);
                    var args = fc.Args.Select(Expr).ToArray();
                    return new FunctionCall(callee, fc.HasArgList, args);
                }

                case ConditionalExpression c:
                    return new ConditionalExpression(Expr(c.Condition), Expr(c.IfTrue), Expr(c.IfFalse));

                case NewExpression n:
                    // Type expression is not normalized; only the constructor arguments are.
                    return new NewExpression(n.Type, n.Args.Select(Expr).ToArray());

                case ArrayLiteral a:
                    return new ArrayLiteral(a.Expressions.Select(Expr).ToArray());

                case Assignment asg:
                    return new Assignment(Expr(asg.LValue), Expr(asg.RValue));

                case Lambda lam:
                    return new Lambda(NormalizeFunction(lam.Function));

                // Statements ---------------------------------------------------------

                case ReturnStatement r:
                    return new ReturnStatement(Normalize(r.Expression));

                case ExpressionStatement es:
                    return new ExpressionStatement(Expr(es.Expression));

                case BlockStatement b:
                    return new BlockStatement(b.Symbols.Select(Normalize).ToArray());

                case MultiStatement m:
                {
                    var syms = m.Symbols.Select(Normalize).ToArray();
                    return syms.Length == 1 ? syms[0] : new MultiStatement(syms); // R2
                }

                case IfStatement iff:
                    return new IfStatement(Normalize(iff.Condition), Normalize(iff.IfTrue), Normalize(iff.IfFalse));

                case LoopStatement l:
                    return new LoopStatement(Normalize(l.Condition), Normalize(l.Body));

                // Leaves and identity-bearing defs are returned unchanged. In particular we do
                // NOT clone VariableDef/ParameterDef (that would orphan references); a local
                // variable initializer is left as-authored (rare in a pure expression language).
                case CommentStatement:
                case Literal:
                case ParameterRefSymbol:
                case VariableRefSymbol:
                case TypeRefSymbol:
                case KeywordRefSymbol:
                case TypeExpression:
                case VariableDef:
                case DefSymbol:
                    return s;

                default:
                    // Conservative: never crash the pass on an unrecognized node.
                    return s;
            }
        }

        /// <summary>
        /// Eta-expand a group reference used as a value into a lambda that forwards to it.
        /// Only expands when every overload shares one arity N >= 1; otherwise the reference
        /// is left as-is (a zero-arity group behaves as a value; differing arities are left
        /// for the solve pass to report). Fresh type variables stand in for the lambda's
        /// parameter and return types; the checker resolves them.
        /// </summary>
        private Expression EtaExpand(FunctionGroupRefSymbol g)
        {
            var fns = g.Def?.Functions;
            if (fns == null || fns.Count == 0)
                return g;

            var arity = fns[0].NumParameters;
            if (arity < 1 || fns.Any(f => f.NumParameters != arity))
                return g;

            var id = _etaId++;
            var ps = new ParameterDef[arity];
            for (var i = 0; i < arity; i++)
            {
                var pType = TypeExpression.CreateTypeVar(null, $"$Eta{id}_{i}");
                ps[i] = new ParameterDef(null, $"_eta{id}_{i}", pType, i);
            }

            var call = new FunctionCall(g, true, ps.Select(p => (Expression)p.ToReference()).ToArray());
            var retType = TypeExpression.CreateTypeVar(null, $"$EtaR{id}");
            var fd = new FunctionDef(null, $"_etaLambda_{id}", null, retType, call, ps);
            return new Lambda(fd);
        }
    }
}
