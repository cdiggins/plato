using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter
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

            sb.Clear();
            OutputTypeRelations(sb);
            SaveToFile(sb, "type-relations.txt");
        }

        public StringBuilder OutputTypeRelations(StringBuilder sb)
        {
            foreach (var kv in Compilation.TypeRelations.RelationLookup)
            {
                sb.AppendLine($"{kv.Key}");

                foreach (var r in kv.Value)
                {
                    // TODO: write it to the console.
                    sb.AppendLine($"{r.Source} => {r.Dest}, Depth = {r.Depth}, Cast = {r.Cast}, Expr = {r.Expr}");
                }
            }

            return sb;
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

        public StringBuilder AnalyzeBody(FunctionAnalysis fa, FunctionCall fc)
        {
            var sb = new StringBuilder();
            var r = new FunctionGroupCallAnalysis(fa, fc);
            var f = fc.Function;
            if (f is FunctionGroupRefSymbol fgr)
            {
                var fcr = new FunctionGroupCallAnalysis(fa, fc);
                var funcs = fcr.ViableFunctions;
                var isAmb = funcs.Count > 1;
                var isAmbStr = isAmb ? "AMBIGUOUS" : "";
                sb.AppendLine($"  # functions: {funcs.Count} {isAmbStr}");
                foreach (var f1 in funcs)
                {
                    sb.AppendLine($"    - {f1.FunctionDef.GetSignature()} => {f1.FunctionDef.Body}");
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

            return sb;
        }

        public StringBuilder OutputFunctionAnalysis(StringBuilder sb)
        {
            var i = 0;
            
            var functionGroups = Compilation
                .FunctionAnalyses
                .Values
                //.Where(fa => fa.IsConceptFunction)
                .Where(fa => fa.IsConceptExtension || fa.IsLibraryFunction)
                .GroupBy(fa => fa.OwnerType)
                .ToList();

            sb.AppendLine($"Found {functionGroups.Count} function groups");

            foreach (var g in functionGroups)
            {
                sb.AppendLine();
                sb.AppendLine($"Group {g.Key}");
                sb.AppendLine();

                foreach (var fa in g)
                {
                    try
                    {
                        var inliner = new FunctionInliner(fa);
                        if (inliner.InlinedBody == null)
                            continue;
                        sb.AppendLine($"{i++}. {fa.Def.Name}");
                        //var typeSig = $"({string.Join(", ", fa.ParameterTypes)}) => {fa.DeclaredReturnType}";
                        //sb.AppendLine($"  Type   : {typeSig}");
                        sb.AppendLine($"  Signature : {fa.Signature}");
                        sb.AppendLine($"  Body      : {fa.Def.Body}");
                        sb.AppendLine($"  Inline    : {inliner.InlinedBody}");
                        //sb.AppendLine($"  Has {fa.TypeParameterToTypeLookup.Count} Type parameters ");
                        //sb.AppendLine($"  Has type parameter in return: {fa.DeclaredReturnType.HasTypeVariable()}");
                    }
                    catch (Exception e)
                    {
                        sb.AppendLine($"Error {e.Message} occurred during analysis of {fa.Def}");
                    }
                }
            }

            return sb;
        }
    }
}