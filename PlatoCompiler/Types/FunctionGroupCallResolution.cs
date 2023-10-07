using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionGroupCallResolution
    {
        public FunctionAnalysis Context { get; }
        public FunctionCall Callsite { get; }
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
            DistinctReturnTypes = CallableFunctions
                .Select(fa => fa.ReturnType)
                .Distinct()
                .ToList();
        }

        public bool IsConcept(IType type)
        {
            throw new NotImplementedException();
        }

        public bool IsConcreteType(IType type) { throw new NotImplementedException(); }

        public int InheritanceDepth(IType type, IType typeBase)
        {
            throw new NotImplementedException();
        }

        public int ImplementsDepth(IType type, IType concept)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 0 is a perfect fit, below 0 is a non-fit, above 0 is an increasingly bad fit. 
        /// </summary>
        public int ArgumentFit(IType typeArgument, IType typeParameter)
        {
            if (typeArgument.Equals(typeParameter))
                return 0;

            if (IsConcept(typeArgument))
            {
                if (IsConcept(typeParameter))
                {
                    return InheritanceDepth(typeArgument, typeParameter);
                }

                if (IsConcreteType(typeParameter))
                {
                    // Cannot pass a concept to a concrete type 
                    return -1;
                }

                // TODO: could be a type-variable 

                throw new Exception("Neither concrete not interface");
            }

            if (IsConcreteType(typeArgument))
            {
                if (IsConcept(typeParameter))
                {
                    return ImplementsDepth(typeArgument, typeParameter);
                }

                if (IsConcreteType(typeParameter))
                {
                    // Concrete types are different 
                    // TODO: log this
                    return -1;
                }

                // TODO: could be a type-variable 

                throw new Exception("Neither concrete not interface");
            }

            // TODO: could be a type 

            // Really don't know 
            return 9999;
        }

        
        public bool CanCall(FunctionAnalysis fa, IReadOnlyList<IType> argTypes)
        {
            if (fa.Parameters.Children.Count != argTypes.Count)
                return false;
            // TODO: check if the arguments satisfy the necessary constraints etc. 
            return true;
        }

        public IType BestReturnType()
        {
            return DistinctReturnTypes.FirstOrDefault();
        }

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