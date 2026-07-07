using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Types
{
    public class FunctionGroupCallAnalysis
    {
        public Compilation Compilation => Context.Compilation;
        public FunctionAnalysis Context { get; }
        public FunctionCall FunctionCall { get; }
        public IReadOnlyList<FunctionCallAnalysis> AllFunctions { get; }
        public IReadOnlyList<FunctionCallAnalysis> ViableFunctions { get; }
        public IReadOnlyList<FunctionCallAnalysis> OrderedFunctions { get; }
        public IReadOnlyList<FunctionCallAnalysis> BestFunctions { get; }
        public IReadOnlyList<TypeExpression> ArgTypes { get; }
        public IReadOnlyList<TypeExpression> ResultTypes { get; }
        public TypeExpression FinalResultType { get; }
        public bool Success { get; }
    
        public FunctionGroupCallAnalysis(FunctionAnalysis context, FunctionCall fc)
        {
            Context = context;
            FunctionCall = fc;

            ArgTypes = fc.Args.Select(Context.TypeResolver.GetType).ToList();
                    
            var funcs = FunctionCall.Function is FunctionGroupRefSymbol fgr 
                ? fgr.Def.Functions 
                : new List<FunctionDef>();

            AllFunctions = funcs
                .Select(f => new FunctionCallAnalysis(Compilation, f, ArgTypes))
                .ToList();

            ViableFunctions = AllFunctions
                .Where(f => f.Valid)
                .ToList();

            if (ViableFunctions.Count == 0)
                throw new Exception("No viable functions found");

            OrderedFunctions = ViableFunctions.OrderBy(x => x).ToList();
            var bestFunction = OrderedFunctions[0];
            BestFunctions = OrderedFunctions.TakeWhile(f => f.CompareTo(bestFunction) == 0).ToList();

            ResultTypes = BestFunctions
                .Select(f => f.DeterminedReturnType)
                .Distinct()
                .ToList();

            Success = ResultTypes.Count == 1;

            //if (!Success) throw new Exception($"Could not find a definitive return type for function call {fc}");
            
            FinalResultType = ResultTypes.FirstOrDefault();
        }

       
    }
}