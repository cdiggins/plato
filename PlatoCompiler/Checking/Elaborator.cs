using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The elaborate pass: walks a normalized function body plus the solver's recorded decisions
    /// (<see cref="TypeCheckResult.ResolvedCalls"/>) and produces a fully-typed, fully-resolved
    /// <see cref="TirFunction"/> (see <see cref="Tir"/>). This is where the emit-time heuristics
    /// start to die: a resolved call becomes a <see cref="TirCall"/> carrying its winning callee, its
    /// instantiated signature, and an <see cref="EmissionKind"/>; an argument that matched via an
    /// implicit conversion becomes an explicit <see cref="TirCoerce"/> node.
    ///
    /// It is TOTAL: a call the solver could not resolve becomes a <see cref="TirUnresolved"/> node
    /// plus an <c>ELB001</c> info diagnostic — the pass never throws. It runs in shadow mode; the
    /// TIR it produces feeds no writer yet.
    /// </summary>
    public class Elaborator
    {
        public Compilation Compilation { get; }
        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        private TypeCheckResult _result;

        public Elaborator(Compilation compilation)
            => Compilation = compilation;

        /// <summary>Elaborate one solved function into TIR. Reuses the normalized body the solver
        /// checked, so the recorded call decisions line up by identity.</summary>
        public TirFunction Elaborate(TypeCheckResult result)
        {
            _result = result;
            var f = result.Normalized ?? result.Function;
            var body = ElaborateStatement(f?.Body);
            return new TirFunction(f, f?.Parameters, f?.ReturnType, body);
        }

        private TypeExpression TypeOf(Expression e)
            => _result?.TypeOf(e);

        // --- statements ----------------------------------------------------------

        public TirNode ElaborateStatement(Symbol s)
        {
            switch (s)
            {
                case null:
                    return null;

                case Expression e:
                    return ElaborateExpr(e);

                case ReturnStatement r:
                    return new TirReturn(ElaborateStatement(r.Expression), r);

                case ExpressionStatement es:
                    return ElaborateExpr(es.Expression);

                case BlockStatement b:
                    return new TirBlock(b.Symbols.Select(ElaborateStatement).Where(n => n != null).ToList(), b);

                case MultiStatement m:
                    return new TirBlock(m.Symbols.Select(ElaborateStatement).Where(n => n != null).ToList(), m);

                case IfStatement iff:
                    return new TirIf(ElaborateStatement(iff.Condition), ElaborateStatement(iff.IfTrue),
                        ElaborateStatement(iff.IfFalse), iff);

                case LoopStatement l:
                    return new TirLoop(ElaborateStatement(l.Condition), ElaborateStatement(l.Body), l);

                case CommentStatement:
                    return null;

                case VariableDef vd:
                    // A local variable definition (rare in a pure expression language). Keep the
                    // initializer walkable under an unresolved wrapper.
                    return new TirUnresolved(vd, "local variable declaration",
                        vd.Value != null ? new List<TirNode> { ElaborateExpr(vd.Value) } : null);

                default:
                    return new TirUnresolved(s, $"unhandled statement {s.GetType().Name}");
            }
        }

        // --- expressions ---------------------------------------------------------

        public TirNode ElaborateExpr(Expression e)
        {
            if (e == null)
                return null;

            var type = TypeOf(e);
            switch (e)
            {
                case Literal lit:
                    return new TirLiteral(lit.Value, lit.TypeEnum, type, lit);

                case ParameterRefSymbol p:
                    return new TirParameter(p.Def, type ?? p.Def?.Type, p);

                case VariableRefSymbol v:
                    return new TirVariable(v.Def, type ?? v.Def?.Type, v);

                case TypeRefSymbol t:
                    return new TirTypeRef(t.Def, type, t);

                case FunctionCall fc:
                    return ElaborateCall(fc);

                case ConditionalExpression c:
                    return new TirConditional(ElaborateExpr(c.Condition), ElaborateExpr(c.IfTrue),
                        ElaborateExpr(c.IfFalse), type, c);

                case NewExpression n:
                    return new TirNew(n.Type, n.Args.Select(ElaborateExpr).ToList(), type ?? n.Type, n);

                case ArrayLiteral arr:
                    return new TirArray(arr.Expressions.Select(ElaborateExpr).ToList(), type, arr);

                case Assignment asg:
                    return new TirAssign(ElaborateExpr(asg.LValue), ElaborateExpr(asg.RValue), type, asg);

                case Lambda lam:
                    return new TirLambda(lam.Function?.Parameters, ElaborateStatement(lam.Function?.Body), type, lam);

                case KeywordRefSymbol k:
                    // `default` in value position; its type is contextual.
                    return new TirDefault(type, k);

                case FunctionGroupRefSymbol g:
                    // A bare group reference left in value position (a zero/mixed-arity group the
                    // normalizer does not eta-expand — behaves as a value/constant).
                    return new TirUnresolved(g, "bare function-group reference in value position");

                default:
                    return new TirUnresolved(e, $"unhandled expression {e.GetType().Name}");
            }
        }

        private TirNode ElaborateCall(FunctionCall fc)
        {
            var type = TypeOf(fc);

            // The `default` keyword desugars to a nullary call on a KeywordRefSymbol.
            if (fc.Function is KeywordRefSymbol)
                return new TirDefault(type, fc);

            // Applying a function VALUE (function-typed parameter/variable, a lambda, or another
            // call's result) — not a named overload, so there is no callee/EmissionKind.
            if (!(fc.Function is FunctionGroupRefSymbol))
            {
                var target = ElaborateExpr(fc.Function);
                var invokeArgs = fc.Args.Select(ElaborateExpr).ToList();
                return new TirInvoke(target, invokeArgs, type, fc);
            }

            // A resolved named overload: emit a typed call, wrapping any conversion argument.
            if (_result != null && _result.ResolvedCalls.TryGetValue(fc, out var rc) && rc.Callee != null)
            {
                var kind = DeriveEmissionKind(rc.Callee, fc);
                var args = new List<TirNode>(fc.Args.Count);
                for (var i = 0; i < fc.Args.Count; i++)
                {
                    var inner = ElaborateExpr(fc.Args[i]);
                    if (inner != null && rc.ArgKinds != null && i < rc.ArgKinds.Count
                        && rc.ArgKinds[i] == ArgMatchKind.Conversion)
                    {
                        var toType = i < rc.ParameterTypes.Count ? rc.ParameterTypes[i] : null;
                        var conv = rc.ArgConversions != null && i < rc.ArgConversions.Count ? rc.ArgConversions[i] : null;
                        inner = new TirCoerce(inner, inner.Type, toType, conv, fc.Args[i]);
                    }
                    args.Add(inner);
                }
                return new TirCall(rc.Callee, kind, rc.ParameterTypes, rc.ReturnType ?? type, args, type ?? rc.ReturnType, fc);
            }

            // Unresolved: stay total — a partial node plus a located diagnostic.
            Diagnostics.Add(new CheckDiagnostic(DiagnosticSeverity.Info, "ELB001",
                $"Call to '{fc.Function?.Name}' was not resolved by the solver; emitting a partial TIR node", fc));
            var children = fc.Args.Select(ElaborateExpr).Where(n => n != null).ToList();
            return new TirUnresolved(fc, "unresolved overload", children);
        }

        // --- emission-kind derivation -------------------------------------------

        /// <summary>
        /// Classify how a resolved call should be emitted, from the callee's <see cref="FunctionType"/>
        /// and the call's shape. First match wins:
        ///   Constructor; Cast/type-named-1-arg → Conversion; Field → Property; operator-named →
        ///   Operator; Intrinsic; no-arg member access → Property; else 0-param → StaticMethod,
        ///   ≥1-param → InstanceMethod.
        /// </summary>
        private EmissionKind DeriveEmissionKind(FunctionDef f, FunctionCall call)
        {
            switch (f.FunctionType)
            {
                case FunctionType.Constructor:
                    return EmissionKind.Constructor;
                case FunctionType.Cast:
                    return EmissionKind.Conversion;
                case FunctionType.Field:
                    return EmissionKind.Property;
            }

            // A type-named single-argument call is a conversion (Vector3(0.0), Point3D(v)) — the
            // callee's name is a generated type. Mirrors the writer's isConversionProperty rule.
            if (call != null && call.Args.Count == 1 && IsTypeName(f.Name))
                return EmissionKind.Conversion;

            // A receiver-only member access written as `a.b` (no arg list) reads as a PROPERTY, and
            // this shape must be decided BEFORE the Operator/Intrinsic classification. The writer
            // keys property-vs-method purely on the member-access shape (HasArgList), so a no-arg
            // intrinsic/operator member (`v.Sqrt`, `v.Negative` from `-v`) is emitted with property
            // syntax, not a `()` call. Deciding Operator/Intrinsic first loses that distinction and
            // makes the TIR writer emit `v.Sqrt()`. Also catches computed no-arg library/concept
            // members (`v.Magnitude`) that are not Field getters.
            if (call != null && f.NumParameters == 1 && !call.HasArgList)
                return EmissionKind.Property;

            if (IsOperatorName(f.Name))
                return EmissionKind.Operator;

            if (f.FunctionType == FunctionType.Intrinsic)
                return EmissionKind.Intrinsic;

            return f.NumParameters == 0 ? EmissionKind.StaticMethod : EmissionKind.InstanceMethod;
        }

        private bool IsTypeName(string name)
            => name != null && Compilation?.GetTypeDef(name) != null;

        private static bool IsOperatorName(string name)
            => name != null
               && (Operators.NamesToBinaryOperators.ContainsKey(name)
                   || Operators.NamesToUnaryOperators.ContainsKey(name));
    }
}
