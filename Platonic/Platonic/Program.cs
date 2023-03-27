using System.Net.Http.Headers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PlatoAnalyzer;
using Ptarmigan.Utils.Roslyn;
using Compilation = Ptarmigan.Utils.Roslyn.Compilation;

namespace Platonic
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var inputFolder = @"C:\Users\cdigg\git\plato\Platonic\TestInput";
            var inputFiles = Directory.GetFiles(inputFolder, "*.cs");

            var compilation = Compilation.Create(inputFiles);

            //PlatoToCSharp.OutputTypes(compilation.Compiler, outputFile);
            /*
            var analyzer = new PlatoAnalyzer.PlatoAnalyzer(compilation.Compiler);
            var writer = new PlatoDebugWriter();

            foreach (var c in analyzer.Mapping.GetClasses())
            {
                writer.Add(c);
            }

            var text = writer.ToString();
            var outputFile = @"C:\Users\cdigg\git\plato\Platonic\output.txt";
            File.WriteAllText(outputFile, text);
            */

            /*
            foreach (var t in compilation.GetAllLinkedTypes().OrderBy(s => s.Name))
            {
                Console.WriteLine($"{t.Name} {t.TypeKind}");
            }
            */


            var d = new Dictionary<ISymbol, int>();
            var d2 = new Dictionary<ISymbol, int>();

            foreach (var (td, sym) in compilation.GetTypeDeclarationsWithSymbols())
            {
                Console.WriteLine($"{td.Keyword} {sym.Name} {sym.Kind}");
                d[sym] = 0;
            }

            var namedTypeSymbols = compilation
                .GetAllSymbols()
                .OfType<INamedTypeSymbol>();
            

            var distinctNamedTypeSymbols = namedTypeSymbols
                .Distinct();

            foreach (var nts in namedTypeSymbols)
            {
                if (d.ContainsKey(nts))
                {
                    d[nts] += 1;
                }
                else
                {
                    if (d2.ContainsKey(nts))
                        d2[nts] += 1;
                    else
                        d2[nts] = 1;
                }
            }

            Console.WriteLine("Declared symbols");
            foreach (var kv in d)
            {
                Console.WriteLine($"{kv.Key} = {kv.Value}");
            }

            Console.WriteLine("Undeclared symbols");
            foreach (var kv in d2)
            {
                Console.WriteLine($"{kv.Key} = {kv.Value}");
            }

            // TODO: get references to members. 
            // TODO: properly handle generics 
            // TODO: 
        }
    }
}