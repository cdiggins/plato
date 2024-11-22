using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class Analysis
    {
        public int Id = NextId++;
        public static int NextId = 0;
        public virtual string Name { get; }
    }
    
    public class TypeAnalysis : Analysis
    {
    }

    public class ParameterAnalysis : Analysis
    {
        public string Name;
        public TypeAnalysis Type;
    }
    
    public class ExpressionAnalysis : Analysis
    {
        public string Type; 
        public Expression Expression;
        public FunctionAnalysis Function;
        public List<ExpressionAnalysis> Children  = new List<ExpressionAnalysis>();
    }

    public class TotalAnalysis : Analysis
    {
        public List<TypeInfo> TypeInfos { get; } = new List<TypeInfo>();
        public List<FunctionAnalysis> Functions { get; } = new List<FunctionAnalysis>();

        public DirectoryPath Directory;
        public Compiler.Compilation Compilation;

        public void SaveToFile(StringBuilder sb, string fileName)
        {
            Directory.RelativeFile(fileName).WriteAllText(sb.ToString());
            sb.Clear();
        }

        public TotalAnalysis(Compiler.Compilation c, DirectoryPath d)
        {
            Compilation = c;
            Directory = d;

            var sb = new StringBuilder();
            WriteReifiedTypes(sb);
            SaveToFile(sb, "reified-types.txt");

            /*
            var noResults = c.FunctionGroupCalls.Values.Where(fgc => fgc.DistinctReturnTypes.Count == 0).ToList();
            sb.AppendLine($"Function group call unresolved: no results {noResults.Count}");
            foreach (var fgc in noResults)
                sb.AppendLine(fgc.ToString());

            var ambResults = c.FunctionGroupCalls.Values.Where(fgc => fgc.DistinctReturnTypes.Count > 1).ToList();
            sb.AppendLine($"Function group call unresolved: ambiguous {ambResults.Count}");
            foreach (var fgc in ambResults)
                sb.AppendLine(fgc.ToString());
            SaveToFile(sb, "Function group calls");
            */

            OutputFunctionAnalysis(sb);
            SaveToFile(sb, "function-analysis.txt");

            OutputFunctionCallAnalysis(sb);
            SaveToFile(sb, "function-call-analysis.txt");
        }

        public StringBuilder WriteReifiedTypes(StringBuilder sb)
        {
            foreach (var kv in Compilation.ReifiedTypes)
            {
                var rt = kv.Value;
                sb.AppendLine($"Reified type {rt.Name}");
                var funcGroups = rt.Functions.GroupBy(f => f.OwnerType);
                foreach (var group in funcGroups)
                {
                    sb.AppendLine($"  Reified functions for group {group.Key}");
                    foreach (var f in group)
                    {
                        sb.AppendLine($"    {f}");
                    }
                }
            }

            return sb;
        }

        public StringBuilder OutputFunctionCallAnalysis(StringBuilder sb)
        {
            var results = Compilation.FunctionGroupCalls.Values.Where(fgc => fgc.BestFunctions.Count != 1).ToList();
            //var results = FunctionGroupCalls.Values.ToList();

            var i = 0;
            sb.AppendLine($"Function call analysis");
            foreach (var fgc in results)
            {
                sb.AppendLine($"{i++}.");
                sb.AppendLine($"  Callsite: {fgc.Callsite}");
                sb.AppendLine($"  Args: {fgc.ArgString}");
                sb.AppendLine($"  Possible Return Types: {string.Join(", ", fgc.DistinctReturnTypes)}");
                sb.AppendLine($"  Callable function count: {fgc.CallableFunctions.Count}");
                sb.AppendLine($"  Best function count: {fgc.BestFunctions.Count}");
                foreach (var f in fgc.CallableFunctions)
                    OutputFunctionCallAnalysis(sb, f);
            }

            return sb;
        }

        public StringBuilder OutputFunctionAnalysis(StringBuilder sb)
        {
            var i = 0;
            foreach (var fa in Compilation.FunctionAnalyses.Values)
            {
                var typeSig = $"({string.Join(", ", fa.ParameterTypes)}) => {fa.DeclaredReturnType}";
                sb.AppendLine($"{i++}. {fa.Def.Name}");
                sb.AppendLine($"  Type: {typeSig}");
                sb.AppendLine($"  Sig: {fa.Signature}");
                sb.AppendLine($"  Body: {fa.Def.Body}");
                //sb.AppendLine($"  Has {fa.TypeParameterToTypeLookup.Count} Type parameters ");
                //sb.AppendLine($"  Has type parameter in return: {fa.DeclaredReturnType.HasTypeVariable()}");
            }

            return sb;
        }

        public void OutputFunctionCallAnalysis(StringBuilder sb, FunctionCallAnalysis fca)
        {
            sb.AppendLine($"    Function = {fca.Function.Signature}");
            sb.AppendLine($"    Callable = {fca.Callable}, Has body = {fca.HasBody}, Arity Matches = {fca.ArityMatches}, # Concrete type = {fca.NumConcreteTypes}");
            if (fca.ArityMatches)
            {
                for (var i = 0; i < fca.Arguments.Count; ++i)
                    sb.AppendLine($"      Argument {i} = {fca.ArgDetails(i)}");
            }
        }
    }
}