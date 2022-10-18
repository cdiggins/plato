using Microsoft.CodeAnalysis;
using PlatoRoslynSyntaxAnalyzer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlatoIR;

namespace PlatoGenerator
{
    public class RoslynToIR
    {
        public IEnumerable<TypeDeclarationIR> BuildIr(Compilation compilation)
        {
            var types = compilation.SyntaxTrees.GetPlatoTypes();

            var builder = new IRBuilder();
            builder = SyntaxToIR.BuildIR(builder, compilation, types);
            return builder
                .GetTypes()
                // TEMP: ignore stuff that is included manually 
                .Where(td => td.Name != "Program" && td.Name != "Benchmarks")
                .ToList();
        }

        // TODO: this should be moved elsewhere ... when it is more robust
        public static IEnumerable<TypeDeclarationIR> InlineMethods(IEnumerable<TypeDeclarationIR> typeDecls)
        { 
            // Inline methods 
            var inliner = new IRMethodInliner();
            foreach (var type in typeDecls)
            {
                var inlineFuncs = new List<MethodDeclarationIR>();
                foreach (var method in type.Methods)
                {
                    var inline = inliner.GetOrComputeInlined(method);
                    if (inline != null && inline.Name.StartsWith("_inlined_"))
                    {
                        inlineFuncs.Add(inline);
                    }
                }
                type.Methods.AddRange(inlineFuncs);
            }

            return typeDecls;
        }
    }
}
