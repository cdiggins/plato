using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

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
        public FunctionAnalysis Context { get; }
        public FunctionCall Callsite { get; }
        public Compiler Compiler => Context.Compiler;
        public FunctionGroupReference Reference { get; }
        public IReadOnlyList<IType> ArgTypes { get; }
        public List<FunctionCallAnalysis> CallableFunctions { get; }
        public List<IType> DistinctReturnTypes { get; }

        public FunctionGroupCallResolution(FunctionCall callsite, FunctionAnalysis context, FunctionGroupReference reference, IReadOnlyList<IType> argTypes)
        {
            Callsite = callsite;
            Context = context;
            Reference = reference;
            ArgTypes = argTypes;

            var functions = reference.Definition.Functions
                .Select(Context.Compiler.GetProcessedFunctionAnalysis)
                .Select(fa => fa.AnalyzeCall(argTypes))
                .ToList();
            
            CallableFunctions = functions
                .Where(fca => fca.Callable)
                .ToList();  

            // Find the best fit. 
            var groups = CallableFunctions
                .GroupBy(fca => fca.FinalScore)
                .OrderBy(g => g.Key);

            var group0 = groups.First().ToList();
            CallableFunctions = group0;

            DistinctReturnTypes = CallableFunctions
                .Select(fca => fca.DeterminedReturnType)
                .Distinct()
                .ToList();
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