using System;
using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionGroupCallAnalysis
    {
        public Compilation Compilation { get; }
        public FunctionAnalysis Context { get; }
        public FunctionCall FunctionCall { get; }
        public IReadOnlyList<FunctionCallAnalysis> AllFunctions { get; }
        public IReadOnlyList<FunctionCallAnalysis> ViableFunctions { get; }
        public IReadOnlyList<TypeExpression> ArgTypes { get; }
        public IReadOnlyList<TypeExpression> ResultTypes { get; }
        public TypeExpression FinalResultType { get; }
        public bool Success { get; }
    
        public FunctionGroupCallAnalysis(Compilation compilation, FunctionAnalysis context, FunctionCall fc)
        {
            Compilation = compilation;
            Context = context;
            FunctionCall = fc;

            ArgTypes = fc.Args.Select(Compilation.GetType).ToList();
            
            var funcs = FunctionCall.Function is FunctionGroupRefSymbol fgr 
                ? fgr.Def.Functions 
                : new List<FunctionDef>();

            AllFunctions = funcs
                .Select(f => new FunctionCallAnalysis(Compilation, f, ArgTypes))
                .ToList();

            ViableFunctions = AllFunctions
                .Where(f => f.Valid)
                .ToList(); 
            
            ResultTypes = ViableFunctions
                .Select(f => f.DeterminedReturnType)
                .Distinct()
                .ToList();
            
            Success = ResultTypes.Count == 1;

            if (!Success)
                throw new Exception($"Could not find a definitive return type for function call {fc}");
            
            FinalResultType = ResultTypes.FirstOrDefault();
        }
    }
}