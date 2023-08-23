using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Tuple = Plato.Compiler.Symbols.Tuple;

namespace Plato.Compiler.Types
{
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
            if (!TypeFactory.Functions.ContainsKey(FunctionDefinition))
                throw new Exception($"No typed function was found for the function {FunctionDefinition}");
            TypedFunction = TypeFactory.Functions[FunctionDefinition];
            GatherConstraints(FunctionDefinition.Body);
        }

        public void RegisterConstraint(TypeConstraint constraint)
        {
            Constraints.Constraints.Add(constraint);
        }

        public Type GetType(Expression expression)
        {
            // 
            throw new NotImplementedException();
        }

        public void GatherConstraints(Expression expression)
        {
            // This is a recursive function. 

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
                        // We are going to be in an interesting situiation. 
                        // Depending one which one is chosen, we have to fork the constraint tree.
                        // We in theory have a list of type arguments for each possibility. 
                        // Those types 
                    }
                    else
                    {
                    }

                    break;
                case Literal literalSymbol:
                    break;
                case Reference refSymbol:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(expression));
            }

            foreach (var c in expression.Children)
                GatherConstraints(c);
        }
    }
}