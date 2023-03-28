using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PlatoAnalyzer;
using Ptarmigan.Utils.Roslyn;
using Compilation = Ptarmigan.Utils.Roslyn.Compilation;

namespace Platonic
{
    public static class Program
    {
        public static bool IsImmutable(ITypeSymbol symbol, List<string> reasons)
        {
            // All fields are readonly 
            foreach (var m in symbol.GetDeclaredAndBaseMembers())
            {
                if (m is IFieldSymbol fs)
                {
                    if (!fs.IsReadOnly && !fs.IsConst)
                    {
                        var node = fs.GetSyntax();
                        if (node != null)
                            if (node.IsPublicMember())
                                reasons.Add($"Type has a public field {fs.Name} that is not readonly or const");
                    }
                }
            }
                
            return reasons.Count == 0;
        }

        /// <summary>
        /// Checks if a method is platonic.
        /// Check that any mutable types aren't copied. 
        /// 1) They aren't assigned to anything (including arrays, and fields, and local variables)
        /// 2) That they aren't captured quietly in a lambda
        /// </summary>
        public static bool IsMethodPlatonic(SemanticModel model, MethodDeclarationSyntax method, HashSet<ITypeSymbol> immutableTypes, List<string> reasons)
        {
            if (method == null)
            {
                reasons.Add("Could not find method");
                return false;
            }

            var sym = model.GetDeclaredSymbol(method) as IMethodSymbol;
            if (sym == null)
            {
                reasons.Add("Could not find symbol");
                return false;
            }

            var allParametersAreImmutable = sym.Parameters.Select(p => p.Type).All(t => immutableTypes.Contains(t));

            var op = model.GetOperation(method.Body);

            var assignments = op.DescendantsAndSelf().OfType<IAssignmentOperation>();

            foreach (var ass in assignments)
            {
                if (ass.Target is IFieldReferenceOperation fro)
                {
                    // We are assigning to a field. 
                    // Is it inside of this class? Or another one? 
                    reasons.Add($"Assignment to {fro.Field.Name} occurs");
                }

                var t = ass.Value.Type;

                if (t == null)
                {
                    reasons.Add($"Could not determine type of assigned value {ass.Value}");
                }
                else
                {
                    if (!immutableTypes.Contains(t))
                    {
                        reasons.Add($"The type {t} of the assigned value {ass.Value} is not immutable: you can't copy mutable types!");
                    }
                }
            }

            // Check that lambdas don't capture mutable types
            var lambdas = method.GetLambdas();
            foreach (var lambda in lambdas)
            {
                foreach (var v in model.GetCapturedVariables(lambda))
                {
                    var t = model.GetTypeSymbol(v);
                    if (!immutableTypes.Contains(t))
                    {
                        reasons.Add($"A captured variable {v} has a mutable type {t}");
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
            var immutableTypes = new HashSet<ITypeSymbol>();

            foreach (var kv in d)
            {
                //Console.WriteLine($"{kv.Key} = {kv.Value}");

                if (kv.Key is ITypeSymbol ts)
                {
                    var reasons = new List<string>();
                    var pure = IsImmutable(ts, reasons);
                    Console.WriteLine($"{ts.Name} is immutable {pure}");
                    foreach (var reason in reasons)
                    {
                        Console.WriteLine($"  {reason}");
                    }

                    if (pure)
                    {
                        immutableTypes.Add(ts);
                    }
                }
            }

            // TODO: add built-in immutable types.
            // Like string, int, float, etc. 
                
            /*
            Console.WriteLine("Undeclared symbols");
            foreach (var kv in d2)
            {
                Console.WriteLine($"{kv.Key} = {kv.Value}");
            }*/

            // TODO: get references to members. 
            // TODO: properly handle generics 
            // TODO: 

            foreach (var (semanticModel, syntaxTree) in compilation.GetModelsAndTrees())
            {
                foreach (var md in syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<MethodDeclarationSyntax>())
                {
                    var reasons = new List<string>();
                    var result = IsMethodPlatonic(semanticModel, md, immutableTypes, reasons);

                    var sym = semanticModel.GetDeclaredSymbol(md);

                    Console.WriteLine($"Method {md.Identifier} {sym?.Name} is Platonic {result}");
                    foreach (var r in reasons)
                    {
                        Console.WriteLine($"Reason {r}");
                    }
                }
            }
        }
    }
}