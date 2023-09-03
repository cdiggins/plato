using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plato.Compiler.Ast;
using Plato.Compiler.Symbols;
using Plato.Compiler.Utilities;
using Tuple = Plato.Compiler.Symbols.Tuple;

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

    // This input/output model would work. 
    // This gives us a graph of outputs, but it does not answer the question of "which choice is the best".
    // In theory it is more of a question, which choice is valid. 
    // At the same time, the question of which choice is best, is influenced by which choice is valid. 
    // See: in some cases there are three choices, and if one is invalid we have to choose again the better one
    // of the remaining two. If choices are ordered by: how good of a choice is it? Then we can simplify things. 
    // Sometimes two choices are almost identical. In that case it doesn't really matter. 
    // There is also an open question of "what is a constraint"? Is it just a relationship between two types? 
    // or is it also a relationship between an expression and a type? 
    // The idea is that I want to be able to make the best choices possible. 

    // At the end I can constuct a constraint chain. From the end to the beginning. Each one making a choice. 
    // Starting with the first choice. Once the chain is complete, I have to look for conflicts. Are there any? 
    // If a conflict is found, I have to make another choice. 
    // The question then becomes ... which choice lead to the conflict? 
    // A conflict is the result of two constraints that are incompatible. I think they might be able to happen anywhere. 
    // Some constraints are the result of a choice, some aren't. 

    /// <summary>
    /// This class is primarily responsible for assigning types to expressions.
    /// It can have multiple children, if there are multiple interpretations.
    /// For example if a function is overloaded. In the future this might be
    /// generalized tow what if two branches of a conditiona are chosen.
    /// It also might be used to figure out if a "Union" is better as A or B.
    ///
    /// The problem is that a choice might n ot be a good one, it might affect that other constraints.
    /// In that case, we need to back up and try a new one. This is not possible
    /// at the current moment.
    ///
    /// Its like a need a way of representing choices ... so that any violation can go back
    /// and try the other set. Two things happen:
    /// 1. we go back to that list of constraints, easy.
    /// 2. we go back to that list of choices and remove one.
    /// 3. we go back to the types worked out so far.
    /// 4. we start the resolution procedure where it was
    ///
    /// So remembering the state of the resolver as it walks the chain is an interesting problem.
    /// There is no clear tree or list of possibilities. I could transform the input however into one. 
    /// </summary>
    public class TypeResolver
    {
        public TypeResolver(
            TypeFactory factory,  
            TypeResolver parent, 
            Constraints parentConstraints,
            FunctionGroupReference group, 
            FunctionDefinition function)
        {
            Factory = factory;
            Group = group;
            Parent = parent;
            Group = group;
            Function = function;
            ExpressionTypes = parent?.ExpressionTypes;

            // Creating a type resolver creates a list of new initial constraints. 
            // And it also creates a final constraint. 
            Constraints = parentConstraints;

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

        public bool IsRoot => Parent == null;
        public string Message { get; private set; }
        public bool Success { get; set; }
        public TypeFactory Factory { get; }
        public TypeResolver Parent { get; }
        public FunctionGroupReference Group { get; }
        public FunctionDefinition Function { get; }
        public List<TypeResolver> Children { get; } = new List<TypeResolver>();
        public Constraints Constraints { get; set; }
        public ExpressionTypes ExpressionTypes { get; private set;  }
        public Type ReturnType { get; }
        public Type BodyType { get; }

        public void Fail(string reason)
        {
            Message = reason;
        }

        public void AddConstraint(TypeConstraint constraint)
        {
            Constraints = new Constraints(Constraints, constraint);
        }

        public IEnumerable<ExpressionTypes> GetExpressionTypes()
        {
            for (var r = ExpressionTypes; r != null; r = r.Parent)
                yield return r;
        }

        public bool CheckCast(Type from, Type to)
        {
            AddConstraint(new CastsToConstraint(from, to));
            
            if (from.CanCastTo(to))
                return true;

            Fail($"Can't cast from {from} to {to}");
            return false;
        }

        public Type Unify(Type typeA, Type typeB)
        {
            // TODO: Add a constraint, and choose between the best, and if not successful, fail. 
            // NOTE: similarly this could work by choosing between both options. 
            // However, let's say I choose "A", but then how do I evaluate a separate branch where it binds to "B". 
            return typeA; 
        }

        public IReadOnlyList<TypeResolver> GetViableChildren()
            => Children.Where(c => c.Success).ToList();

        public Constraints GatherConstraints(IReadOnlyList<Type> argTypes, IReadOnlyList<Type> paramTypes)
        {
            var constraints = Constraints;
            for (var i = 0; i < argTypes.Count; ++i)
            {
                var paramType = paramTypes[i];
                var argType = argTypes[i];
                var constraint = new CastsToConstraint(argType, paramType);
                constraints = new Constraints(constraints, constraint);
            }
            return constraints;
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
                var fx2 = Reduce(fgr, argTypes); 
                foreach (var f in fx2.Definition.Functions)
                {
                    var tf = Factory.GetTypedFunction(f);
                    var constraints = GatherConstraints(argTypes, tf.Parameters);
                    Children.Add(new TypeResolver(Factory, this, constraints, fgr, f));
                }

                var tmp = GetViableChildren();
                if (tmp.Count == 0)
                {
                    throw new Exception("No viable child");
                }


                if (tmp.Count == 1)
                {
                    return tmp[0].ReturnType;
                }

                // TODO: write an algorithm to choose the best child

                // TODO: assure that the functions have the same type. 

                Debug.WriteLine("Multiple viable children: should choose the best");
                return tmp[0].ReturnType;
            }
            else
            {
                var paramTypes = Enumerable.Range(0, nArgs).Select(_ => Factory.CreateAny()).ToList();
                var returnType = Factory.CreateAny();
                Constraints = GatherConstraints(argTypes, paramTypes);
                return returnType;
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