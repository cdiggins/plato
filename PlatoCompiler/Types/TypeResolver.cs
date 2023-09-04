using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public TypeResolver(
            TypeFactory factory,  
            FunctionDefinition function)
        {
            Factory = factory;
            Function = function;

            try
            {
                if (function != null)
                {
                    var tf = Factory.GetTypedFunction(function);
                    ReturnType = tf.ReturnType;
                    BodyType = Resolve(Function.Body);

                    if (BodyType != null && ReturnType != null)
                        if (!CheckCast(BodyType, ReturnType))
                            return;
                }

                Success = true;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                Success = false;
            }
        }

        public bool Success { get; }
        public string Message { get; private set; }
        public TypeFactory Factory { get; }
        public FunctionDefinition Function { get; }
        public ExpressionTypes ExpressionTypes { get; private set;  }
        public Type ReturnType { get; }
        public Type BodyType { get; }

        public void Fail(string reason)
            => Message = reason;

        public IEnumerable<ExpressionTypes> GetExpressionTypes()
        {
            for (var r = ExpressionTypes; r != null; r = r.Parent)
                yield return r;
        }

        public bool CheckCast(Type from, Type to)
        {
            if (from.CanCastTo(to))
                return true;

            Fail($"Can't cast from {from} to {to}");
            return false;
        }

        public Type Unify(Type typeA, Type typeB)
        {
            // TODO: Choose between the best, and if not successful, fail. 
            return typeA; 
        }

        public bool IsFunctionCallable(FunctionDefinition f, IReadOnlyList<Type> argTypes)
        {
            var ft = Factory.GetTypedFunction(f);

            if (ft.Parameters.Count != argTypes.Count)
                return false;

            for (var i = 0; i < ft.Parameters.Count; ++i)
            {
                var paramType = ft.Parameters[i];
                var argType = argTypes[i];
                if (!argType.CanCastTo(paramType))
                    return false;
            }

            return true;
        }

        public FunctionGroupReference Reduce(FunctionGroupReference fgr, IReadOnlyList<Type> argTypes)
        {
            if (fgr.Definition.Functions.Count == 0)
                throw new Exception("Function group found with no functions");
            var funcs = fgr
                .Definition 
                .Functions
                .Where(f => IsFunctionCallable(f, argTypes))
                .ToList();
            var fgd = new FunctionGroupDefinition(funcs, fgr.Name);
            return new FunctionGroupReference(fgd);
        }

        public Type ResolveFunctionCall(FunctionCall fc)
        {
            var fx = fc.Function;
            var nArgs = fc.Args.Count;
            var argTypes = fc.Args.Select(Resolve).ToList();

            if (fx is FunctionGroupReference fgr)
            {
                if (fgr.Definition.Functions.Count == 0)
                    throw new Exception("No functions");

                var fx2 = Reduce(fgr, argTypes);

                var typedFunctions = fx2.Definition.Functions.Select(Factory.GetTypedFunction).ToList();

                if (typedFunctions.Count == 0)
                {
                    throw new Exception("No viable child");
                }

                if (typedFunctions.Count == 1)
                {
                    return typedFunctions[0].ReturnType;
                }

                // TODO: write an algorithm to choose the best child
                // TODO: assure that the functions have the same type. 

                Debug.WriteLine("Multiple viable children: should choose the best");

                return typedFunctions[0].ReturnType;
            }
            else
            {
                return Factory.CreateAny();
            }
        }

        public Type Resolve(ExpressionSymbol expression)
        {
            if (expression == null)
                return null;

            Type r = null;

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
                    r = Factory.FindType(PrimitiveTypeDefinitions.Function.Name);
                    break;

                case Lambda lambda:
                    r = CreateType(lambda);
                    break;

                case Literal literal:
                    switch (literal.TypeEnum)
                    {
                        case LiteralTypesEnum.Integer:
                            r = Factory.FindType("Integer");
                            break;
                        case LiteralTypesEnum.Number:
                            r = Factory.FindType("Number");
                            break;
                        case LiteralTypesEnum.Boolean:
                            r = Factory.FindType("Boolean");
                            break;
                        case LiteralTypesEnum.String:
                            r = Factory.FindType("String");
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;

                case ParameterReference parameterReference:
                    r = Factory.GetType(parameterReference.Type);
                    break;

                case Parenthesized parenthesized:
                    r = Resolve(parenthesized.Expression);
                    break;

                case PredefinedReference predefinedReference:
                    r = Factory.GetType(predefinedReference.Definition.Type);
                    break;

                case Reference reference:
                    r = Factory.GetType(reference.Definition.Type);
                    break;

                case Tuple tuple:
                    r = CreateType(tuple);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }

            ExpressionTypes = new ExpressionTypes(ExpressionTypes, expression, r);
            return r;
        }

        public Type CreateType(Lambda lambda)
        {
            var r = Factory.GetTypedFunction(lambda.Function);
            Resolve(lambda.Function.Body);
            return r.FunctionType;
        }

        public Type CreateType(Tuple tuple)
        {
            var args = tuple.Children.Select(Resolve).ToArray();
            return Factory.CreateTuple(args);
        }
    }
}