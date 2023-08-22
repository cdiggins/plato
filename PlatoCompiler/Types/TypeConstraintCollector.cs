using System;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    // TODO:
    // I think having a constraint tree is too complex.
    // I can just create a bunch of constraints. 
    // Afterwards ... I can have a separate system for choosing functions. 
    // We basically walk the tree ... and if there is a function ... we can add new constraints. 
    // If one of those constraints don't work. 
    // Or another way of looking at it, is create a literal graph outside of this thing. 
    // This thing creates additional constraints ... based on the chosen function. 
    // If a function chosen violates the constraint chain, we throw it out. 
    // See the first thing to do is create constraints. 
    // The next thing is to identify conflicts 
    // And just before that identifying "options" is a good thing.
    // Remember that options might not change anything. 
    // Also we need to realize that in some cases the options might not change anything.
    // At the same time, there is almost no code here. 

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