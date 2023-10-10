using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using Plato.Compiler.Symbols;
using Ptarmigan.Utils;

namespace Plato.Compiler.Types
{
    /// <summary>
    /// TODO: I may need to update this so that it tells us more about the process.
    /// And it might be useful to tell us about the kinds of casts that are required,
    /// or the constraints that are created. For example: when passing a type-variable
    /// to a concrete type. 
    /// </summary>
    public class FunctionGroupCallResolution
    {
        public const int CantPassGenericToConcreteFit = -6;
        public const int DoesntImplementConceptFit = -5;
        public const int MismatchedTypeFit = -4;
        public const int CantPassConceptToConcreteFit = -3;
        public const int DoesntMatchDeclaredConstraintFit = -2;
        public const int NoFit = -1;
        public const int PerfectFit = 0;
        public const int GenericFit = 10 * 1000; 
        public const int AlmostNotAFit = 1000 * 1000;
        public const int InheritsFitPenalty = 10;
        public const int ImplementsFitPenalty = 100;
        public const int CastPenalty = 1000;

        public FunctionAnalysis Context { get; }
        public FunctionCall Callsite { get; }
        public Compiler Compiler => Context.Compiler;
        public FunctionGroupReference Reference { get; }
        public IReadOnlyList<IType> ArgTypes { get; }
        public List<FunctionAnalysis> CallableFunctions { get; }
        public List<IType> DistinctReturnTypes { get; }

        public FunctionGroupCallResolution(FunctionCall callsite, FunctionAnalysis context, FunctionGroupReference reference, IReadOnlyList<IType> argTypes)
        {
            Callsite = callsite;
            Context = context;
            Reference = reference;
            ArgTypes = argTypes;
            CallableFunctions = reference.Definition.Functions
                .Select(Context.Compiler.GetProcessedFunctionAnalysis)
                .Where(fa => CanCall(fa, ArgTypes))
                .ToList();

            // Find the best fit. 
            if (CallableFunctions.Count > 0 && argTypes.Count > 0)
            {
                var arg0 = argTypes[0];
                var groups = CallableFunctions.GroupBy(cf => ArgumentFit(arg0, cf.Parameters[0]));
                var group0 = groups.First().ToList();
                CallableFunctions = group0;
            }

            DistinctReturnTypes = CallableFunctions
                .Select(fa => fa.ReturnType)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// below 0 is a non-fit, 0 is a perfect fit, above 0 is an increasingly bad fit. 
        /// </summary>
        public int ArgumentFit(IType typeArgument, IType typeParameter)
        {
            if (typeArgument == null)
                return NoFit;

            if (typeArgument is TypeVariable tv0)
            {
                typeArgument = tv0.Constraint;
            }

            if (typeArgument.Equals(typeParameter))
                return PerfectFit;

            if (typeParameter is TypeVariable tv)
            {
                if (typeArgument.IsConcrete())
                {
                    // Check that the type argument implements all of the constraints of the type variable
                    if (!typeArgument.Implements(tv.Constraint))
                        return DoesntMatchDeclaredConstraintFit;

                    // All of the declared constraints are satisfied.
                    // So we are a match, but anything else would match better 
                    return GenericFit;
                }

                if (typeArgument.IsConcept())
                {
                    // Check that the type argument inherits all of the constraints of the type variable
                    if (!typeArgument.Inherits(tv.Constraint))
                        return DoesntMatchDeclaredConstraintFit;

                    // All of the declared constraints are satisfied.
                    // So we are a match, but anything else would match better 
                    return GenericFit;
                }

                throw new InvalidOperationException("Should not be reachable");
            }

            if (typeParameter.IsConcept())
            {
                if (typeArgument.IsConcept())
                {
                    var r = typeArgument.InheritsDepth(typeParameter) + InheritsFitPenalty;
                    return r >= 0 ? r : DoesntImplementConceptFit;
                }

                if (typeArgument.IsConcrete())
                {
                    var r = typeArgument.ImplementsDepth(typeParameter) + ImplementsFitPenalty;
                    return r >= 0 ? r : DoesntImplementConceptFit;
                }

                throw new InvalidOperationException("Should not be reachable");
            }

            if (typeParameter.IsConcrete())
            {
                var cast = Context.Compiler.FindCast(typeArgument, typeParameter);
                if (cast != null)
                {
                    return CastPenalty;
                }

                if (typeArgument.IsConcept())
                {
                    // Cannot pass a concept to a concrete type 
                    return CantPassConceptToConcreteFit;
                }

                if (typeParameter.IsConcrete())
                {
                    // Already verified above that this is not true
                    return MismatchedTypeFit;
                }

                throw new InvalidOperationException("Should not be reachable");
            }

            throw new InvalidOperationException("Should not be reachable");
        }

        public bool CanCall(FunctionAnalysis fa, IReadOnlyList<IType> argTypes)
        {
            if (fa.Parameters.Count != argTypes.Count) 
                return false;
            if (fa.Parameters.Count > 0)
                return ArgumentFit(argTypes[0], fa.Parameters[0]) >= 0;
            return true;
        }

        public IType BestReturnType()
            => DistinctReturnTypes.FirstOrDefault();
        

        public string ArgString
            => string.Join(", ", ArgTypes);

        public string ReturnTypesString
            => string.Join(", ", DistinctReturnTypes);

        public override string ToString()
        {
            return $"{Callsite} with ({ArgString}) has {DistinctReturnTypes.Count} possible return types {ReturnTypesString}";
        }
    }
}