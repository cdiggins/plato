using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public class TotalAnalysis 
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

            OutputFunctionAnalysis(sb);
            SaveToFile(sb, "function-analysis.txt");
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

        public static List<FunctionDef> ViableFunctions(FunctionCall fc, FunctionGroupRefSymbol fgr)
        {
            // Get all the functions that have the same number of arguments as the function call, and that aren't abstract. 
            return fgr.Def.Functions.Where(f1 => !f1.OwnerType.IsConcept() && f1.Parameters.Count == fc.Args.Count).ToList();
        }
        
        public StringBuilder OutputFunctionAnalysis(StringBuilder sb2)
        {
            var i = 0;
            
            var functionGroups = Compilation
                .FunctionAnalyses
                .Values
                //.Where(fa => fa.IsConceptFunction)
                .Where(fa => fa.IsConceptExtension || fa.IsLibraryFunction)
                .GroupBy(fa => fa.OwnerType)
                .ToList();

            sb2.AppendLine($"Found {functionGroups.Count} function groups");

            foreach (var g in functionGroups)
            {
                sb2.AppendLine();
                sb2.AppendLine($"Group {g.Key}");
                sb2.AppendLine();

                foreach (var fa in g)
                {
                    var sb = new StringBuilder();

                    var isAmb = false;

                    var typeSig = $"({string.Join(", ", fa.ParameterTypes)}) => {fa.DeclaredReturnType}";
                    sb.AppendLine($"{i++}. {fa.Def.Name}");
                    sb.AppendLine($"  Type: {typeSig}");
                    sb.AppendLine($"  Sig: {fa.Signature}");
                    sb.AppendLine($"  Body: {fa.Def.Body}");

                    var expr = fa.Def.Body;

                    if (expr is FunctionCall fc)
                    {
                        var r = new FunctionCallResolver(Compilation, fa, fc);
                        var f = fc.Function;
                        if (f is FunctionGroupRefSymbol fgr)
                        {
                            var fcr = new FunctionCallResolver(Compilation, fa, fc);
                            var funcs = ViableFunctions(fc, fgr);
                            isAmb = funcs.Count > 1;
                            var isAmbStr = isAmb ? "AMBIGUOUS" : "";
                            sb.AppendLine($"  # functions: {funcs.Count} {isAmbStr}");
                            foreach (var f1 in funcs)
                            {
                                sb.AppendLine($"    - {f1.GetSignature()} => {f1.Body}");
                            }
                        }
                        else if (f is TypeRefSymbol trs)
                        {
                            sb.AppendLine($"   _NEW_ {trs}({fc.Args.JoinStringsWithComma()});");
                        }
                        else
                        {
                            sb.AppendLine($"   NOT A FUNCTION GROUP");
                        }

                        for (var j = 0; j < fc.Args.Count; ++j)
                        {
                            var arg = fc.Args[j];
                            var argType = r.ArgTypes[j];
                            sb.AppendLine($"    _ARG_ {j}: {arg} => {argType}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"  NOT FUNCTION CALL");
                    }

                    // Only do this for the ambiguous functions
                    if (isAmb)
                        sb2.AppendLine(sb.ToString());

                    //sb.AppendLine($"  Has {fa.TypeParameterToTypeLookup.Count} Type parameters ");
                    //sb.AppendLine($"  Has type parameter in return: {fa.DeclaredReturnType.HasTypeVariable()}");
                }
            }

            return sb2;
        }

        public void OutputFunctionCallAnalysis(StringBuilder sb, FunctionCallAnalysis fca)
        {
            sb.AppendLine($"    Function = {fca.Function.Signature}");
            sb.AppendLine($"    Callable = {fca.Valid}, Has body = {fca.HasBody}, Arity Matches = {fca.ArityMatches}, # Concrete type = {fca.NumConcreteTypes}");
            if (fca.ArityMatches)
            {
                for (var i = 0; i < fca.Arguments.Count; ++i)
                    sb.AppendLine($"      Argument {i} = {fca.ArgDetails(i)}");
            }
        }
    }
}