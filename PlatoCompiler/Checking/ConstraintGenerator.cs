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
                    CheckReturn(e, returnType);
                    break;
                default:
                    CheckStatement(body, returnType);
                    break;
            }
        }

        /// <summary>Check an expression in RETURN position. Unlike <see cref="Check"/> this emits a
        /// coercion constraint rather than a hard equality: Plato (like the generated C#) admits an
        /// implicit conversion at the return boundary — a tuple result for a same-shape struct
        /// (<c>(a, b)</c> for a <c>Line2D</c>), a cast relation (<c>Vector3</c> for <c>Point3D</c>),
        /// or a value for an interface it implements.</summary>
        private void CheckReturn(Expression e, TypeExpression returnType)
        {
            if (IsDefaultCall(e))
            {
                System.ExprTypes[e] = returnType;
                return;
            }
            var t = Synthesize(e);
            if (t != null && returnType != null)
                System.Add(new CoercionConstraint(t, returnType, e));
        }

        private void CheckStatement(Symbol s, TypeExpression returnType)
        {
            switch (s)
            {
                case ReturnStatement r when r.Expression is Expression e:
                    CheckReturn(e, returnType);
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
                    if (v.Type != null && !IsUnannotated(v.Type))
                        System.Equate(vt, v.Type, v);
                    else
                        // `var q = ...`: the declared type is the "Any" placeholder (or absent).
                        // Record the initializer's type so references to q synthesize it, not Any.
                        _localVarTypes[v] = vt;
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
                    r = (p.Def != null && _lambdaParamTypes.TryGetValue(p.Def, out var hole) ? hole : null)
                        ?? p.Def?.Type ?? Vars.Fresh("Param");
                    break;

                case VariableRefSymbol v:
                    r = (v.Def != null && _localVarTypes.TryGetValue(v.Def, out var localType) ? localType : null)
                        ?? (v.Def?.Type != null && !IsUnannotated(v.Def.Type) ? v.Def.Type : null)
                        ?? Vars.Fresh("Var");
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

                // A type used as a value (`Self.CreateFromComponents(...)`, `Number.MinValue`) is a
                // static-access receiver: its "type" is the referenced type itself, so a concept
                // parameter can bind through it (Self satisfies any concept — the reifier decides
                // what Self is) and the member resolves against it.
                case TypeRefSymbol tr:
                    r = tr.Def?.ToTypeExpression() ?? Vars.Fresh("Type");
                    break;

                case TypeExpression te:
                    r = te;
                    break;

                // A bare reference to a unique nullary function is a CONSTANT (the writer emits
                // `Constants.<Name>`): its type is that function's declared return type. This is
                // exactly the current writer's constants rule, mirrored.
                case FunctionGroupRefSymbol g when IsUniqueNullary(g):
                    r = g.Def.Functions[0].ReturnType ?? Vars.Fresh("Const");
                    break;

                // Any other bare group reference should have been eta-expanded; a bare keyword ref
                // is only well-typed in checking position. Fall back to a fresh variable.
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
            // Unannotated lambda parameters carry the placeholder type "Any" from binding — they
            // are inference HOLES, not a concrete type. A fresh variable per parameter lets the
            // enclosing HOF's signature (Function1<$T,$T>) determine them.
            var ps = f.Parameters.Select(p =>
            {
                var t = p.Type != null && !IsUnannotated(p.Type) ? p.Type : Vars.Fresh("LP");
                if (!ReferenceEquals(t, p.Type))
                    _lambdaParamTypes[p] = t;
                return t;
            }).ToList();
            var body = f.Body is Expression be ? Synthesize(be) : Vars.Fresh("LBody");
            return Named($"Function{ps.Count}", ps.Append(body).ToArray());
        }

        /// <summary>The placeholder type given to unannotated parameters/locals during binding.</summary>
        private static bool IsUnannotated(TypeExpression t)
            => t?.Def?.Name == "Any";

        /// <summary>A function group with exactly one nullary overload — the writer's constants rule.</summary>
        private static bool IsUniqueNullary(FunctionGroupRefSymbol g)
            => g?.Def?.Functions != null
               && g.Def.Functions.Count == 1
               && g.Def.Functions[0].NumParameters == 0;

        // The fresh hole minted for each unannotated lambda parameter, so a reference to the
        // parameter inside the lambda body synthesizes the SAME hole (p.Def.Type would give "Any").
        private readonly Dictionary<ParameterDef, TypeExpression> _lambdaParamTypes
            = new Dictionary<ParameterDef, TypeExpression>();

        // The initializer type of each unannotated local (`var q = ...`), so references to the
        // local share the initializer's type instead of the "Any" placeholder.
        private readonly Dictionary<VariableDef, TypeExpression> _localVarTypes
            = new Dictionary<VariableDef, TypeExpression>();
    }
}
