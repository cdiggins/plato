using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionGroupCallResolution
    {                                                                                                                                                                                                                                                                                                                                                                                                                                                   
        public FunctionAnalysis Context { get; }
        public FunctionCall Callsite { get; }
        public Compilation Compilation => Context.Compilation;
        public FunctionGroupRefSymbol RefSymbol { get; }
        public IReadOnlyList<IType> ArgTypes { get; }
        public List<FunctionCallAnalysis> Functions { get; }
        public List<FunctionCallAnalysis> CallableFunctions { get; }
        public List<FunctionCallAnalysis> BestFunctions { get; }
        public List<IType> DistinctReturnTypes { get; }

        public FunctionGroupCallResolution(FunctionCall callsite, FunctionAnalysis context, FunctionGroupRefSymbol refSymbol, IReadOnlyList<IType> argTypes)
        {
            Callsite = callsite;
            Context = context;
            RefSymbol = refSymbol;
            ArgTypes = argTypes;

            Functions = refSymbol.Def.Functions
                .Select(Context.Compilation.GetProcessedFunctionAnalysis)
                .Select(fa => fa.AnalyzeCall(argTypes))
                .ToList();
            
            CallableFunctions = Functions
                .Where(fca => fca.Callable)
                .ToList();

            BestFunctions = CallableFunctions;

            if (BestFunctions.Count > 1)
            {
                BestFunctions = CallableFunctions
                    .GroupBy(AssignScore)
                    .OrderBy(g => g.Key)
                    .First().ToList();
            }

            DistinctReturnTypes = BestFunctions
                .Select(fca => fca.DeterminedReturnType)
                .Distinct()
                .ToList();
        }

        public static int AssignScore(FunctionCallAnalysis fca)
        {
            if (fca.PerfectFit)
                return 0;
            var score = 0;
            var firstParam = fca.Parameters.FirstOrDefault();
            if (firstParam == null)
                return 1;
            if (firstParam.IsConcept())
                score += 100;
            if (firstParam.IsTypeVariable())
                score += 200;
            score += fca.ArgFits.Any(FunctionCallAnalysis.IsNotFit) ? 100000 : 0;
            score += fca.ArgFits.Any(FunctionCallAnalysis.IsMaybeFit) ? 1000 : 0;
            score += fca.ArgFits.Count(af => af == FunctionCallAnalysis.ArgFit.FitWithCast) * 5;
            return score;
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