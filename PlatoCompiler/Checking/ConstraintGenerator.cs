using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The constrain pass. Walks a *normalized* function body and produces a
    /// <see cref="ConstraintSystem"/> — a type (often a fresh variable) for every expression,
    /// plus the equality/overload constraints relating them.
    ///
    /// It is bidirectional: <see cref="Synthesize"/> infers a type upward from an expression,
    /// while <see cref="Check"/> pushes an expected type downward (into conditional branches,
    /// return positions, and — crucially — the <c>default</c> keyword, whose type is entirely
    /// determined by context). Overloaded calls become <see cref="OverloadConstraint"/>s that
    /// the solver resolves once the argument types are known.
    /// </summary>
    public class ConstraintGenerator
    {
        public Compilation Compilation { get; }
        public TypeVarFactory Vars { get; }
        public ConstraintSystem System { get; } = new ConstraintSystem();

        public ConstraintGenerator(Compilation compilation, TypeVarFactory vars = null)
        {
            Compilation = compilation;
            Vars = vars ?? new TypeVarFactory();
        }

        public ConstraintSystem Generate(FunctionDef f)
        {
            // The body's result must be the declared return type.
            CheckBody(f.Body, f.ReturnType);
            return System;
        }

        // --- named-type helpers --------------------------------------------------

        private TypeExpression Named(string name)
        {
            var td = Compilation?.GetTypeDef(name);
            return td != null ? td.ToTypeExpression() : Vars.Fresh(name);
        }

        private TypeExpression Named(string name, params TypeExpression[] args)
        {
            var td = Compilation?.GetTypeDef(name);
            return td != null ? new TypeExpression(td, args) : Vars.Fresh(name);
        }

        private TypeExpression Bool() => Named("Boolean");

        private TypeExpression LiteralType(LiteralTypesEnum e)
        {
            switch (e)
            {
                case LiteralTypesEnum.Integer: return Named("Integer");
                case LiteralTypesEnum.Number: return Named("Number");
                case LiteralTypesEnum.Boolean: return Named("Boolean");
                case LiteralTypesEnum.String: return Named("String");
                default: return Vars.Fresh("Lit");
            }
        }

        private static bool IsDefaultCall(Expression e)
            => e is FunctionCall fc && fc.Function is KeywordRefSymbol k && k.Keyword == "default";

        // --- statements ----------------------------------------------------------

        private void CheckBody(Symbol body, TypeExpression returnType)
        {
            switch (body)
            {
                case null:
                    break;
                case Expression e: // expression-bodied function
                    Check(e, returnType);
                    break;
                default:
                    CheckStatement(body, returnType);
                    break;
            }
        }

        private void CheckStatement(Symbol s, TypeExpression returnType)
        {
            switch (s)
            {
                case ReturnStatement r when r.Expression is Expression e:
                    Check(e, returnType);
                    break;
                case ReturnStatement r:
                    CheckBody(r.Expression, returnType);
                    break;
                case ExpressionStatement es:
                    Synthesize(es.Expression);
                    break;
                case BlockStatement b:
                    foreach (var x in b.Symbols) CheckStatement(x, returnType);
                    break;
                case MultiStatement m:
                    foreach (var x in m.Symbols) CheckStatement(x, returnType);
                    break;
                case IfStatement iff:
                    if (iff.Condition is Expression ce) Check(ce, Bool());
                    CheckStatement(iff.IfTrue, returnType);
                    CheckStatement(iff.IfFalse, returnType);
                    break;
                case LoopStatement l:
                    if (l.Condition is Expression lc) Check(lc, Bool());
                    CheckStatement(l.Body, returnType);
                    break;
                case VariableDef v when v.Value != null:
                    var vt = Synthesize(v.Value);
                    if (v.Type != null) System.Equate(vt, v.Type, v);
                    break;
                case Expression expr:
                    Synthesize(expr);
                    break;
            }
        }

        // --- expressions ---------------------------------------------------------

        /// <summary>Push an expected type into an expression (checking mode).</summary>
        private void Check(Expression e, TypeExpression expected)
        {
            // `default` takes its type entirely from context; nothing else to infer.
            if (IsDefaultCall(e))
            {
                System.ExprTypes[e] = expected;
                return;
            }

            var t = Synthesize(e);
            System.Equate(t, expected, e);
        }

        /// <summary>Infer a type upward from an expression (synthesis mode).</summary>
        public TypeExpression Synthesize(Expression e)
        {
            if (e == null)
                return Vars.Fresh("Null");
            if (System.ExprTypes.TryGetValue(e, out var cached))
                return cached;

            TypeExpression r;
            switch (e)
            {
                case Literal lit:
                    r = LiteralType(lit.TypeEnum);
                    break;

                case ParameterRefSymbol p:
                    r = p.Def?.Type ?? Vars.Fresh("Param");
                    break;

                case VariableRefSymbol v:
                    r = v.Def?.Type ?? Vars.Fresh("Var");
                    break;

                case FunctionCall fc:
                    r = SynthesizeCall(fc);
                    break;

                case ConditionalExpression c:
                {
                    if (c.Condition != null) Check(c.Condition, Bool());
                    var result = Vars.Fresh("Cond");
                    if (c.IfTrue != null) Check(c.IfTrue, result);
                    if (c.IfFalse != null) Check(c.IfFalse, result);
                    r = result;
                    break;
                }

                case NewExpression n:
                    foreach (var a in n.Args) Synthesize(a);
                    r = n.Type;
                    break;

                case ArrayLiteral arr:
                {
                    var elem = Vars.Fresh("Elem");
                    foreach (var x in arr.Expressions) Check(x, elem);
                    r = Named("Array", elem);
                    break;
                }

                case Lambda lam:
                    r = LambdaType(lam);
                    break;

                case Assignment asg:
                {
                    var rv = Synthesize(asg.RValue);
                    var lv = Synthesize(asg.LValue);
                    System.Equate(lv, rv, asg);
                    r = rv;
                    break;
                }

                case Parenthesized par: // defensive; normalization removes these
                    r = Synthesize(par.Expression);
                    break;

                case TypeRefSymbol _:
                    r = Named("Type");
                    break;

                // A bare group reference should have been eta-expanded; a bare keyword ref is
                // only well-typed in checking position. Fall back to a fresh variable.
                case FunctionGroupRefSymbol _:
                case KeywordRefSymbol _:
                default:
                    r = Vars.Fresh("Expr");
                    break;
            }

            System.ExprTypes[e] = r;
            return r;
        }

        private TypeExpression SynthesizeCall(FunctionCall fc)
        {
            var argTypes = fc.Args.Select(Synthesize).ToList();

            switch (fc.Function)
            {
                case FunctionGroupRefSymbol g:
                {
                    var candidates = g.Def?.Functions?.ToList() ?? new List<FunctionDef>();
                    var result = Vars.Fresh("Ret");
                    System.Add(new OverloadConstraint(fc, g.Name, result, argTypes, candidates, fc));
                    return result;
                }

                case KeywordRefSymbol _: // `default` in call position; type comes from context
                    return Vars.Fresh("Default");

                case Lambda lam:
                    return ApplyFunctionType(LambdaType(lam), argTypes, fc);

                case ParameterRefSymbol p:
                    return ApplyFunctionType(p.Def?.Type, argTypes, fc);

                case VariableRefSymbol v:
                    return ApplyFunctionType(v.Def?.Type, argTypes, fc);

                default:
                    return Vars.Fresh("Call");
            }
        }

        /// <summary>Apply a value of function type to arguments, yielding the return type.</summary>
        private TypeExpression ApplyFunctionType(TypeExpression funcType, IReadOnlyList<TypeExpression> argTypes, Symbol origin)
        {
            var n = argTypes.Count;
            var ret = Vars.Fresh("Apply");
            var expected = Named($"Function{n}", argTypes.Append(ret).ToArray());
            if (funcType != null)
                System.Equate(funcType, expected, origin);
            return ret;
        }

        private TypeExpression LambdaType(Lambda lam)
        {
            var f = lam.Function;
            var ps = f.Parameters.Select(p => p.Type ?? Vars.Fresh("LP")).ToList();
            var body = f.Body is Expression be ? Synthesize(be) : Vars.Fresh("LBody");
            return Named($"Function{ps.Count}", ps.Append(body).ToArray());
        }
    }
}
