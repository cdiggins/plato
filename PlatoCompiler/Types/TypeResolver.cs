using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Tuple = Plato.Compiler.Symbols.Tuple;

namespace Plato.Compiler.Types
{
    /// <summary>
    /// This class is primarily responsible for assigning types to expressions.
    /// </summary>
    public class TypeResolver
    {
        public bool Success { get; }
        public string Message { get; private set; }
        public Compiler Compiler { get; }
        public Dictionary<ExpressionSymbol, TypeExpressionSymbol> Types => Compiler.ExpressionTypes;
        public FunctionDefinition Function { get; }
        public List<FunctionCallResolver> FunctionCalls { get; } = new List<FunctionCallResolver>();

        public TypeResolver(
            Compiler compiler,  
            FunctionDefinition function)
        {
            Compiler = compiler;

            Function = function;

            try
            {
                if (function != null)
                {
                    Resolve(Function.Body);
                }

                Success = true;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                Success = false;
            }
        }

        public void Fail(string reason)
            => Message = reason;


        public TypeExpressionSymbol Unify(TypeExpressionSymbol typeA, TypeExpressionSymbol typeB)
        {
            // TODO: Choose between the best, and if not successful, fail. 
            return typeA; 
        }

        public TypeExpressionSymbol ResolveFunctionCall(FunctionCall fc)
        {
            var fx = fc.Function;
            var argTypes = fc.Args.Select(Resolve).ToList();

            if (fx is FunctionGroupReference fgr)
            {
                var fcr = new FunctionCallResolver(Compiler, fgr, argTypes);
                FunctionCalls.Add(fcr);
                return fcr.ResultType;
            }

            return CreateAny();
        }

        public TypeExpressionSymbol Resolve(ExpressionSymbol expression)
        {
            if (expression == null)
                return null;

            TypeExpressionSymbol r = null;

            switch (expression)
            {
                case Argument argument:
                    r = Resolve(argument.Expression);
                    break;

                case Assignment assignment:
                    r = Resolve(assignment.LValue);
                    break;

                case ConditionalExpression conditionalExpression:
                    r = Unify(
                        Resolve(conditionalExpression.IfTrue),
                        Resolve(conditionalExpression.IfFalse));
                    break;

                case FunctionCall functionCall:
                    r = ResolveFunctionCall(functionCall);
                    break;

                case FunctionGroupReference functionGroupReference:
                    r = Compiler.GetTypeExpression(PrimitiveTypeDefinitions.Function.Name);
                    break;

                case Lambda lambda:
                    r = CreateType(lambda);
                    break;

                case Literal literal:
                    switch (literal.TypeEnum)
                    {
                        case LiteralTypesEnum.Integer:
                            r = Compiler.GetTypeExpression("Integer");
                            break;
                        case LiteralTypesEnum.Number:
                            r = Compiler.GetTypeExpression("Number");
                            break;
                        case LiteralTypesEnum.Boolean:
                            r = Compiler.GetTypeExpression("Boolean");
                            break;
                        case LiteralTypesEnum.String:
                            r = Compiler.GetTypeExpression("String");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case ParameterReference parameterReference:
                    r = parameterReference.Type;
                    break;

                case Parenthesized parenthesized:
                    r = Resolve(parenthesized.Expression);
                    break;

                case PredefinedReference predefinedReference:
                    r = predefinedReference.Definition.Type;
                    break;

                case Reference reference:
                    r = reference.Definition.Type;
                    break;

                case Tuple tuple:
                    r = CreateType(tuple);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }

            if (r == null)
                throw new Exception($"No type determined for {expression}");
            Types.Add(expression, r);
            return r;
        }

        public TypeExpressionSymbol CreateAny()
        {
            return new TypeExpressionSymbol(Compiler.GetTypeDefinition("Any"));
        }

        public TypeExpressionSymbol CreateType(Lambda lambda)
        {
            return CreateType(lambda.Function);
        }

        public TypeExpressionSymbol CreateType(FunctionDefinition function)
        {
            var args = function.Parameters.Select(p => p.Type).Append(function.ReturnType).ToArray();
            // TODO: maybe choose one of the numbered functions
            return new TypeExpressionSymbol(Compiler.GetTypeDefinition("Function"), args);
        }

        public TypeExpressionSymbol CreateType(Tuple tuple)
        {
            var args = tuple.Children.Select(Resolve).ToArray();
            // TODO: maybe choose one of the numbered tuples
            return new TypeExpressionSymbol(Compiler.GetTypeDefinition("Tuple"), args);
        }
    }
}