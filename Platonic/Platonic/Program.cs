using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PlatoAnalyzer;
using Ptarmigan.Utils.Roslyn;
using Compilation = Ptarmigan.Utils.Roslyn.Compilation;

namespace Platonic
{
    public static class Program
    {
        //public static bool ReferencesAMutableO  

        public static bool IsImmutable(ITypeSymbol symbol, List<string> reasons)
        {
            // All fields are readonly 
            foreach (var m in symbol.GetDeclaredAndBaseMembers())
            {
                if (m is IFieldSymbol fs)
                {
                    if (!fs.IsReadOnly && !fs.IsConst)
                    {
                        //if (fs.IsPublicMember())
                        reasons.Add($"Type has a public field {fs.Name} that is not readonly or const");
                    }
                }
            }
                
            return reasons.Count == 0;
        }

        public static void AnalyzeClass(TypeDeclarationSyntax typeDecl, INamedTypeSymbol symbol)
        {
            // Is this a mutable class or not? 

            // If it is mutable, is it used in a non-mutable context 
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting compilation ... ");
            var inputFolder = @"C:\Users\cdigg\git\plato\Platonic\TestInput";
            var inputFiles = Directory.GetFiles(inputFolder, "*.cs");
            var compilation = Compilation.Create(inputFiles);
            Console.WriteLine($"Compilation completed {compilation.EmitResult}");

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
                // Console.WriteLine($"{td.Keyword} {sym.Name} {sym.Kind}");
                d[sym] = 0;
            }

            var namedTypeSymbols = compilation
                .GetExpressionAndTypeSymbols()
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
                //Console.WriteLine($"{kv.Key} = {kv.Value}");

                if (kv.Key is ITypeSymbol ts)
                {
                    var reasons = new List<string>();
                    var mut = IsImmutable(ts, reasons);
                    Console.WriteLine($"{ts.Name} is immutable {mut}");
                    foreach (var reason in reasons)
                    {
                        Console.WriteLine($"  {reason}");
                    }
                }
            }

            /*
            Console.WriteLine("Undeclared symbols");
            foreach (var kv in d2)
            {
                Console.WriteLine($"{kv.Key} = {kv.Value}");
            }*/

            // TODO: get references to members. 
            // TODO: properly handle generics 
            // TODO: 
        }
    }
}