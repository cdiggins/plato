using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plato.Compiler.Types
{

    /// <summary>
    /// At any point in time we have an expression, and a set of choices for the possible types
    /// for that expression. As we walk through the input, we make choices, and generate constraints.
    /// Choices are ordered: the first choice is presumed to be better according to a set of heuristics. 
    /// </summary>
    public class ResolverInput
    {
        public ResolverInput Previous { get; }
        public ExpressionSymbol Expression { get; }
        public IReadOnlyList<Type> Choices { get; }

        public ResolverInput(ResolverInput previous, ExpressionSymbol expression, IReadOnlyList<Type> choices)
        {
            Expression = expression;
            Previous = previous;
            Choices = choices;
        }
    }

    /// <summary>
    /// As we walk the input each choice generates a new list of constraints.
    /// </summary>
    public class ResolverOutput
    {
        public ResolverInput Input { get; }
        public ILists<TypeConstraint> Constraints { get; }

        public ResolverOutput(ResolverInput input, ILists<TypeConstraint> constraints)
        {
            if (input.Choices.Count != constraints.Count)
                throw new Exception();

            Input = input;
            Constraints = constraints;
        }
    }

    /// <summary>
    /// Represents a choice made on the resolver input/output chain.
    /// Used to gather constraints, and identify and locate conflicts.
    /// </summary>
    public class ResolverChoice
    {
        public ResolverChoice Parent { get; }
        public ResolverInput Input { get; }
        public ResolverOutput Output { get; }
        public int Choice { get; }
        public int MaxChoice => Input.Choices.Count;
        public ExpressionSymbol Expression => Input.Expression;
        public Type Type => Input.Choices[Choice];
        public IReadOnlyList<TypeConstraint> Constraints => Output.Constraints[Choice];

        public ResolverChoice(ResolverChoice parent, ResolverInput input, ResolverOutput output, int choice)
        {
            Parent = parent;
            Input = input;
            Output = output;
            Choice = choice;
        }

        public ResolverChoice GetNextChoice()
        {
            var choice = Choice + 1;
            if (choice == MaxChoice)
            {
                var newParent = Parent?.GetNextChoice();
                if (newParent == null) return null;
                return new ResolverChoice(newParent, Input, Output, 0);
            }
            return new ResolverChoice(Parent, Input, Output, choice);
        }

        public ResolverChoice GetNextChoice(Predicate<ResolverChoice> predicate)
        {
            if (predicate(this))
                return GetNextChoice();
            return new ResolverChoice(Parent.GetNextChoice(predicate), Input, Output, 0);
        }
    }

    /// <summary>
    /// A conflict means that two or more constraints don't work together.
    /// The last one in the chain suggests that we need to make a new choice
    /// </summary>
    public class ConstraintConflict
    {
        public IReadOnlyList<TypeConstraint> Constraints { get; }
        public string Reason { get; }

        public ConstraintConflict(IReadOnlyList<TypeConstraint> constraints, string reason)
        {
            if (constraints.Count < 2)
                throw new Exception("Constraint conflicts must have at least 2 constraints");
            Constraints = constraints;
            Reason = reason;
        }
    }
    /// <summary>
    /// Go to the end of the first choice chain. Find the location of the conflict.
    /// Choose a new chain. 
    /// </summary>
    public class ConflictResolver
    {
        public ConstraintConflict Conflict { get; }
        public ResolverInput Input { get; }
        public ResolverOutput Output { get; }
        public ResolverChoice PreviousChoice { get; }
        public ResolverChoice NewChoice { get; }

        public bool HasConflict => Conflict != null;

        public static bool IsConflictLocation(ResolverChoice choice, ConstraintConflict conflict)
            => choice.Constraints.Any(conflict.Constraints.Contains);

        public ResolverChoice ComputeNewChoice()
        {
            throw new NotImplementedException();
        }
    }
}