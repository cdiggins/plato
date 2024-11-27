using System.Collections.Generic;
using System.Linq;
using Plato.Compiler.Symbols;

namespace Plato.Compiler.Types
{
    public class FunctionCallResolver
    {
        public Compilation Compilation { get; }
        public FunctionAnalysis Context { get; }
        public FunctionCall FunctionCall { get; }
        public List<FunctionCallAnalysis> Options { get; }
        public IReadOnlyList<IType> ArgTypes { get; }
        public IReadOnlyList<IType> ResultTypes { get; }
        public IType FinalResultType { get; }
        public bool Success { get; }
    
        public FunctionCallResolver(Compilation compilation, FunctionAnalysis context, FunctionCall fc)
        {
            Compilation = compilation;
            Context = context;
            FunctionCall = fc;

            ArgTypes = fc.Args.Select(GetTypeOfExpr).ToList();
            
            var funcs = FunctionCall.Function is FunctionGroupRefSymbol fgr 
                ? fgr.Def.Functions 
                : new List<FunctionDef>();
            
            Options = funcs
                .Select(Compilation.GetProcessedFunctionAnalysis)
                .Select(f => new FunctionCallAnalysis(f, ArgTypes))
                .ToList();

            ResultTypes = Options
                .Select(f => f.DeterminedReturnType)
                .Distinct()
                .ToList();
            
            Success = ResultTypes.Count == 1;

            // TODO: order the results.
            
            FinalResultType = ResultTypes.FirstOrDefault();
        }

        public IType GetTypeOfExpr(Expression expr)
            => Compilation.GetExpressionType(expr);
    }
}