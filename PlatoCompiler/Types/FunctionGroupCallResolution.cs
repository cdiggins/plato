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