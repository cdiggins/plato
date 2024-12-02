using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class FunctionInliner
    {
        public Compilation Compilation => Context.Compilation;
        public readonly FunctionAnalysis Context;
        public readonly List<string> FailedInlines = new List<string>();
        public readonly TypedExpression InlinedBody;
        public readonly IType BooleanType;

        public FunctionInliner(FunctionAnalysis fa)
        {
            BooleanType = fa.ToSimpleType("Boolean");
            Context = fa;
            var body = fa.Def.Body as Expression;
            InlinedBody = body == null 
                ? null 
                : InlineExpression(
                    body, 
                    new ParameterReplacements(null, null, null), 
                    fa.DeclaredReturnType);
        }

        public TypedExpression InlineExpression(Expression expr, ParameterReplacements replacements, IType usage = null)
        {
            var type = Compilation.GetExpressionType(expr);
            var r = new TypedExpression(expr, type, usage);

            switch (expr)
            {
                case ArrayLiteral arrayLiteral:
                    return new TypedExpression(
                        new ArrayLiteral(
                            arrayLiteral.Expressions.Select(x => InlineExpression(x, replacements)).ToArray()),
                        type, usage);

                case Assignment assignment:
                    return r;
                
                case ConditionalExpression conditionalExpression:
                    return new TypedExpression(
                        new ConditionalExpression(
                            InlineExpression(conditionalExpression.Condition, replacements, BooleanType),
                            InlineExpression(conditionalExpression.IfTrue, replacements, null),
                            // TODO: the second branch might need to be cast to the first (or vice versa)
                            InlineExpression(conditionalExpression.IfFalse, replacements, null)),
                        type,
                        usage);
                
                case FunctionCall functionCall:
                    return InlineCall(functionCall, replacements, usage);
                
                case FunctionGroupRefSymbol functionGroupRefSymbol:
                    return r;
                
                case Lambda lambda:
                    return r;
                
                case Literal literal:
                    return r;
                
                case ParameterRefSymbol parameterRefSymbol:
                    // TODO: if a parameter is used in multiple places, we want to replace this with a variable. 
                    return replacements.Replace(parameterRefSymbol.Def) ?? r;

                case VariableRefSymbol variableRefSymbol:
                    // TODO: in some cases, it might make more sense to inline the variable definition as well. 
                    // It depends on how often it is used. 
                    return r;

                case Parenthesized parenthesized:
                    return r;

                case TypeRefSymbol typeRef:
                    return r;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expr));
            }
        }

        public TypedExpression InlineCall(FunctionCall fc, ParameterReplacements replacements, IType usage)
        {
            var determined = Compilation.GetExpressionType(fc);
            var args = fc.Args.Select(a => InlineExpression(a, replacements)).ToList();
            var r = new TypedExpression(
                new FunctionCall(fc.Function, fc.HasArgList, args.Cast<Expression>().ToArray()), determined, usage);

            if (fc.Function is TypeRefSymbol trs)
            {
                if (!trs.Name.StartsWith("Tuple"))
                    throw new Exception($"Expected type refs used as function calls to be ony Tuples not {trs.Name}");

                // TODO: resolve tuples into actual constructors of the type. 
            }
            else if (fc.Function is FunctionGroupRefSymbol fg)
            {
                var fcr = new FunctionGroupCallAnalysis(Compilation, Context, fc);
                var funcs = fcr.ViableFunctions;
                if (funcs.Count > 1)
                {
                    FailedInlines.Add($"Too many functions for {fc}");
                    return r.With(fcr);
                }

                if (funcs.Count == 0)
                {
                    FailedInlines.Add($"No valid function found for {fc}");
                    return r.With(fcr);
                }

                // TODO: inject and expand type casts where appropriate. 

                var f = funcs[0];
                var fd = f.FunctionDef;
                var fa = Compilation.GetProcessedFunctionAnalysis(fd);

                var args2 = new List<TypedExpression>();
                for (var i = 0; i < fc.Args.Count; ++i)
                {
                    var parameterType = fa.ParameterTypes[i];
                    var arg = InlineExpression(fc.Args[i], replacements, parameterType);
                    args2.Add(arg);
                }

                // TODO: there is going to be some additional type information that we should leverage as well. 

                if (fd.Body is Expression body)
                    return InlineExpression(body, replacements.AddRange(fd.Parameters, args2)).With(fcr);

                FailedInlines.Add($"Can't inline function with non-expression body");
                return r.With(fcr);
            }
            else  
            {
                // TODO: what if we find it in the parameter replacements?
            }

            return r;
        }
    }
}