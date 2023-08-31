using System;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    // TODO: I think this probably needs to be removed. 
    public class TypeConstraintCollector
    {
        public FunctionDefinition FunctionDefinition { get; }
        public TypedFunction TypedFunction { get; }
        public TypeFactory TypeFactory { get; }
        public TypeConstraintCollection Constraints { get; } = new TypeConstraintCollection();

        public TypeConstraintCollector(FunctionDefinition function, TypeFactory typeFactory)
        {
            FunctionDefinition = function;
            TypeFactory = typeFactory;
            if (!TypeFactory.TypedFunctionLookup.ContainsKey(FunctionDefinition))
                throw new Exception($"No typed function was found for the function {FunctionDefinition}");
            TypedFunction = TypeFactory.TypedFunctionLookup[FunctionDefinition];
            var body = FunctionDefinition.Body;

            if (body == null)
                return;
            GatherConstraints(body);
            //var exprType = TypeFactory.GetType(body);
            //var returnType = TypedFunction.Signature.ReturnType;
            //Constraints.Add(new CastsToConstraint(exprType, returnType));
        }

        public void RegisterConstraint(TypeConstraint constraint)
        {
            Constraints.Constraints.Add(constraint);
        }

        public Type GetType(Expression expression)
            => throw new NotImplementedException();

        public void GatherConstraints(Expression expression)
        {
            // This is a recursive function. 
            if (expression == null) 
                return;

            foreach (var child in expression.Children)
                GatherConstraints(child);
            
            switch (expression)
            {
                case Argument argumentSymbol:
                    break;

                case Assignment assignmentSymbol:
                    throw new NotImplementedException("Assignment symbols are not yet supported");

                case ConditionalExpression conditionalExpressionSymbol:
                    RegisterConstraint(new IsBoolConstraint(GetType(conditionalExpressionSymbol.Condition)));
                    RegisterConstraint(new UnifiesConstraint(
                        GetType(conditionalExpressionSymbol.IfTrue),
                        GetType(conditionalExpressionSymbol.IfFalse)));
                    break;

                case FunctionCall functionCallSymbol:

                    // First thing is invoked. 
                    var argTypes = functionCallSymbol.Args.Select(GetType).ToList();

                    var funcType = GetType(functionCallSymbol.Function);
                    RegisterConstraint(new InvokedConstraint(funcType, argTypes));

                    if (functionCallSymbol.Function is FunctionGroupReference fgr)
                    {
                        foreach (var f in fgr.Definition.Functions)
                        {
                            var options = Constraints.AddOptions();

                            for (var i = 0; i < f.Parameters.Count; ++i)
                            {
                                var paramType = TypeFactory.GetType(f.Parameters[i].Type);

                                if (i < argTypes.Count)
                                {
                                    var argType = argTypes[i];
                                    var constraint = new ArgumentConstraint(argType, paramType);
                                    options.Add(constraint);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"function {functionCallSymbol.Function} is not a function group reference");
                    }

                    break;

                case Literal literalSymbol:
                    break;

                case Reference refSymbol:
                    break;

                case Lambda lambda:
                    GatherConstraints(lambda.Function.Body);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }
        }
    }
}