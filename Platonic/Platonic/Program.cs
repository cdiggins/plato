using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Ptarmigan.Utils;
using Ptarmigan.Utils.Roslyn;
using Compilation = Ptarmigan.Utils.Roslyn.Compilation;

namespace Platonic
{
    public static class Program
    {
        public static string Describe(ISymbol symbol)
        {
            return $"(Name={symbol.Name} Kind={symbol.Kind})";
        }

        public static string Describe(this IEnumerable<ISymbol> symbols)
        {
            return string.Join(", ", symbols.Select(Describe));
        }

        public static void OutputAnnotatedFiles(this Compilation compilation, string outputFolder)
        {
            foreach (var st in compilation.SyntaxTrees)
            {
                var fileName = Path.GetFileName(st.FilePath);
                if (string.IsNullOrWhiteSpace(fileName))
                    throw new Exception("Expected non-null file path");
                var model = compilation.Compiler.GetSemanticModel(st);

                var cb = new CodeBuilder();
                foreach (var n in st.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>())
                {
                    var ta = new PlatoTypeAnalysis(model, n);

                    cb.WriteLine($"// ");
                    cb.WriteLine($"public {ta.TypeSymbol.Kind} {ta.TypeSymbol.Name}");
                    cb.WriteLine("{").Indent();
                    foreach (var m in ta.Members)
                    {
                        var isPublic = m.IsPublic ? "public" : "private";
                        var isStatic = m.IsStatic ? "static" : "instance";
                        var memberType = m is PlatonicMethodAnalysis ? "method"
                            : m is PlatonicFieldAnalysis ? "field"
                            : m is PlatonicPropertyAnalysis ? "property"
                            : "member";

                        cb.WriteLine($"// It has a {isPublic} {isStatic} {memberType} named {m.Name} with a type {m.MemberType}");
                        if (m.Operation != null)
                        {
                            cb.WriteLine($"// operation kind is {m.Operation?.Kind} and type {m.Operation?.Type}");
                        }
                        else
                        {
                            cb.WriteLine($"// No associated operation");
                        }

                        if (m.DataFlow != null)
                        {
                            cb.WriteLine($"// Written symbols are {m.DataFlow.WrittenInside.Describe()}");
                            cb.WriteLine($"// Read symbols are {m.DataFlow.ReadInside.Describe()}");
                            cb.WriteLine($"// Captured symbols are {m.DataFlow.Captured.Describe()}");
                            cb.WriteLine($"// Variables declared are {m.DataFlow.VariablesDeclared}");
                        }
                        else
                        {
                            cb.WriteLine($"// No data-flow analysis could be created");
                        }
                        cb.WriteLine(m.Node.ToFullString());
                    }
                    cb.WriteLine("}");
                }

                var outputFile = Path.Combine(outputFolder, fileName);
                File.WriteAllText(outputFile, cb.ToString());
            }
        }

        public static IEnumerable<PlatoTypeAnalysis> AnalyzeTypes(this Compilation compilation)
        {
            foreach (var st in compilation.SyntaxTrees)
            {
                var model = compilation.Compiler.GetSemanticModel(st);
                foreach (var n in st.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>())
                {
                    yield return new PlatoTypeAnalysis(model, n);
                }
            }
        }

        public static void OutputMemberInfo(PlatoMemberAnalysis m)
        {
            var isPublic = m.IsPublic ? "public" : "private";
            var isStatic = m.IsStatic ? "static" : "instance";
            var memberType = m is PlatonicMethodAnalysis ? "method"
                : m is PlatonicFieldAnalysis ? "field"
                : m is PlatonicPropertyAnalysis ? "property"
                : "member";
            Console.WriteLine($"  It has a {isPublic} {isStatic} {memberType} named {m.Name} with a type {m.MemberType}");
            Console.WriteLine($"     There is {m.Operation != null} an associated operation of kind {m.Operation?.Kind} and type {m.Operation?.Type}");
            Console.WriteLine($"     The syntax node is {m.Node.Kind()} and the associated statement/expression is {m.StatementOrExpression?.Kind()}");

            if (m.DataFlow != null)
            {
                var writtenTo = string.Join(", ", m.DataFlow.WrittenInside);
                var usedFuncs = string.Join(", ", m.DataFlow.UsedLocalFunctions);
                var readInside = string.Join(", ", m.DataFlow.ReadInside);
                Console.WriteLine($"    Written variables are {writtenTo}, used functions {usedFuncs}, variables read inside {readInside}");
            }
        }
        
        // TODO: Output the code.
        // TODO: make sure there are no modifiers on classes
        // TODO: probably going to use the code builder? 
        // TODO: 

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting compilation ... ");
            var inputFolder = @"C:\Users\cdigg\git\plato\Platonic\TestInput";
            var inputFiles = Directory.GetFiles(inputFolder, "*.cs");
            var compilation = Compilation.Create(inputFiles);
            Console.WriteLine($"Compilation completed {compilation.EmitResult}");
            var ta = compilation.AnalyzeTypes();
            foreach (var t in ta)
            {
                Console.WriteLine($"Found a type {t.TypeSymbol?.Name} of kind {t.TypeSymbol?.TypeKind}");

                foreach (var m in t.Members)
                {
                    OutputMemberInfo(m);
                }
            }
        }

        public static void OldMain(string[] args)
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
                    var pure = PlatonicAnalysis.IsImmutable(ts, reasons);
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
                    var result = PlatonicAnalysis.IsMethodPlatonic(semanticModel, md, immutableTypes, reasons);

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