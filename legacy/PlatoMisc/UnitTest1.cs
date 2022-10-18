using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using PlatoAnalyzer;
using PlatoLib;
using Enumerable = System.Linq.Enumerable;

namespace PlatoTest
{
    public class BenchmarkResult<TArg>
    {
        public TArg Arg { get; }
        public TimeSpan Span1 { get; }
        public TimeSpan Span2 { get; }
        public bool Result { get; }
        public string Message { get; }

        public BenchmarkResult(TArg arg, TimeSpan span1, TimeSpan span2, bool result, string message)
            => (Arg, Span1, Span2, Result, Message) = (arg, span1, span2, result, message);
    }

    public class Tests
    {

        [Test]
        public void SimpleGeneratorTest()
        {
            // Create the 'input' compilation that the generator will act on
            var inputCompilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");

            // directly create an instance of the generator
            // (Note: in the compiler this is loaded from an assembly, and created via reflection at runtime)
            var generator = new HelloWorldGenerator();

            // Create the driver that will control the generation, passing in our generator
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

            // We can now assert things about the resulting compilation:
            Debug.Assert(diagnostics.IsEmpty); // there were no diagnostics created by the generators
            Debug.Assert(outputCompilation.SyntaxTrees.Count() == 2); // we have two syntax trees, the original 'user' provided one, and the one added by the generator
            Debug.Assert(outputCompilation.GetDiagnostics().IsEmpty); // verify the compilation with the added source has no diagnostics

            // Or we can look at the results directly:
            var runResult = driver.GetRunResult();

            // The runResult contains the combined results of all generators passed to the driver
            Debug.Assert(runResult.GeneratedTrees.Length == 1);
            Debug.Assert(runResult.Diagnostics.IsEmpty);

            // Or you can access the individual results on a by-generator basis
            var generatorResult = runResult.Results[0];
            Debug.Assert(generatorResult.Generator == generator);
            Debug.Assert(generatorResult.Diagnostics.IsEmpty);
            Debug.Assert(generatorResult.GeneratedSources.Length == 1);
            Debug.Assert(generatorResult.Exception is null);
        }

        public static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        public static Compilation CreateCompilationFromFile(string file)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(SourceText.From(File.OpenRead(file))) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        public static (Compilation, ImmutableArray<Diagnostic>) 
            TestGenerator<T>(Compilation compilation, T generator, GeneratorDriver driver = null)
            where T : class, ISourceGenerator, new()
        {
            driver = driver ?? CSharpGeneratorDriver.Create(generator);

            // Run the generation pass
            // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            return (outputCompilation, diagnostics);
        }

        public static ITypeSymbol GetSymbolType(ISymbol symbol)
        {
            switch (symbol)
            {
                case IAliasSymbol alias:
                case IDiscardSymbol discard:
                case IDynamicTypeSymbol dynamicType:
                case IErrorTypeSymbol errorType:
                case IEventSymbol eventSymbol:
                case IFunctionPointerTypeSymbol functionPoint:
                case ILabelSymbol label:
                case IModuleSymbol module:
                case INamespaceSymbol namespaceSymbol:
                case IPointerTypeSymbol pointerType:
                case IPreprocessingSymbol prePreprocessingSymbol:
                case IRangeVariableSymbol rangeVariable:
                case ISourceAssemblySymbol sourceAssembly:
                case IAssemblySymbol assembly:
                    return null;
                case ITypeParameterSymbol typeParameter:
                    return typeParameter;
                case IArrayTypeSymbol arrayType:
                    return arrayType;
                case INamedTypeSymbol namedType:
                    return namedType;
                case ITypeSymbol type:
                    return type;
                case IFieldSymbol field:
                    return field.Type;
                case IPropertySymbol property:
                    return property.Type;
                case IParameterSymbol parmeter:
                    return parmeter.Type;
                case ILocalSymbol local:
                    return local.Type;
                case IMethodSymbol method:
                    // TODO: this is complicated stuff!
                    return null;
            }

            return null;
        }

        /*
        public static void OutputCommon<T>(AnalyticalPart<T> part) where T: PlatoSyntaxNode
        {
            Console.WriteLine($"Analysis Type {part.GetType().Name}, Plato Syntax {part.PlatoSyntax.GetType()}");
            Console.WriteLine($"C# Syntax Kind = {part.CSharpNode?.Kind()}, C# Syntax Type = {part.CSharpNode?.GetType().Name}");
            Console.WriteLine($"Type = {part.CSharpType}, Has Value = {part.HasValue}, Value = {part.Value}");
            Console.WriteLine($"Operation = {part.CSharpOperation}, Kind = {part.CSharpOperation?.Kind}, Type = {part.CSharpOperation?.GetType().Name}");
            Console.WriteLine($"Symbol = {part.CSharpSymbol}, Kind = {part.CSharpSymbol?.Kind}, Location = {part.CSharpLocation}, Declaration = {part.Declaration}");
        }

        public static void Output(ClassAnalysis ca)
        {
            Console.WriteLine($"Class {ca.PlatoSyntax.Name}");
            OutputCommon(ca);
            Console.WriteLine("Fields");
            foreach (var f in ca.Fields)
                Output(f);
            Console.WriteLine("Properties");
            foreach (var p in ca.Properties)
                Output(p);
            Console.WriteLine("Functions");
            foreach (var f in ca.Functions)
                Output(f);
        }

        public static void Output(PlatoSemanticMapping mapping, IMethodSymbol ms)
        {
            Console.WriteLine($"Reciever type = {ms.ReceiverType}");
            Console.WriteLine($"Method type arity = {ms.Arity}");
            Console.WriteLine($"Method kind = {ms.MethodKind}");
            Console.WriteLine($"Associated symbol = {ms.AssociatedSymbol}");
            Console.WriteLine($"Constructed from = {ms.ConstructedFrom}");
            Console.WriteLine($"Original definotion = {ms.OriginalDefinition}");
            Console.WriteLine($"Parameters = {string.Join(", ", ms.Parameters)}");
            Console.WriteLine($"Return type = {ms.ReturnType}");
            Console.WriteLine($"Type parameters = {string.Join(", ", ms.TypeParameters)}");
            foreach (var decl in ms.DeclaringSyntaxReferences)
            {
                var node = decl.GetSyntax();
                var plato = mapping.GetPlatoNode(node);
                Console.WriteLine($"Declaring Syntax Node {node.Kind()}, Plato Node {plato?.GetType().Name}");
            }
        }

        public static void Output(ExpressionAnalysis ea)
        {
            Console.WriteLine($"Expression {ea.CSharpNode}");
            OutputCommon(ea);
            foreach (var child in ea.Expressions)
                Console.WriteLine($"child = {child.CSharpNode}, type = {child.CSharpType}");

            if (ea.PlatoSyntax is PlatoInvoke pi)
            {
                if (!(ea.CSharpSymbol is IMethodSymbol ms))
                    throw new Exception("Expected a method symbol");
                Output(ea.Mapping, ms);
            }
        }

        public static void Output(FunctionAnalysis fa)
        {
            Console.WriteLine($"Function {fa.PlatoSyntax.Name}, Args {string.Join(", ", fa.PlatoSyntax.Parameters.Parameters)}");
            OutputCommon(fa);
            Output(fa.Statement);
        }

        public static void Output(PropertyAnalysis pa)
        {
            Console.WriteLine($"Property {pa.PlatoSyntax.Name}");
            OutputCommon(pa);
        }

        public static void Output(FieldAnalysis fa)
        {
            Console.WriteLine($"Field {fa.PlatoSyntax.Name}");
            OutputCommon(fa);
        }

        public static void Output(StatementAnalysis sa)
        {
            Console.WriteLine($"Statement: {sa.PlatoSyntax.GetType().Name}");
            OutputCommon(sa);
            Console.WriteLine("Child expressions");
            foreach (var child in sa.Expressions)
                Output(child);
            Console.WriteLine("Child statements");
            foreach (var child in sa.Statements)
                Output(child);
        }

        public static void TestAnalyzer(PlatoAnalyzer.PlatoAnalyzer analyzer)
        {
            Console.WriteLine($"Mapping children = {analyzer.Mapping.Children.Count}, Plato lookup = {analyzer.Mapping.PlatoLookup.Count}, C# lookup = {analyzer.Mapping.CSharpLookup.Count}");

            foreach (var cls in analyzer.Mapping.GetPlatoSyntaxNodes<PlatoClass>())
            {
                var ca = cls.ToAnalysis(analyzer.Mapping);
                Output(ca);
            }

            foreach (var exp in analyzer.Mapping.GetPlatoSyntaxNodes<PlatoInvoke>())
            {
                var ea = exp.ToAnalysis(analyzer.Mapping);
                Output(ea);
            } 
        }
    */


        [Test]
        public static void AnalyzerTest()
        {
            var mgr = new AnalyzerManager(@"C:\Users\Acer\source\repos\Plato\Plato\Plato.sln");
            foreach (var kv in mgr.Projects)
            {
                Console.WriteLine($"Project {kv.Key} = {kv.Value.ProjectFile.Name}");
            }

            var proj = mgr.Projects[@"C:\Users\Acer\source\repos\Plato\PlatoLib\PlatoLib.csproj"];
            var compilation = proj.GetWorkspace().CurrentSolution.Projects.First().GetCompilationAsync().Result;
            Console.WriteLine($"Syntax trees {compilation?.SyntaxTrees.Count()}");

            /*
            var generator = new PlatoGenerator();
            var diagnostics = TestGenerator(compilation, generator).Item2;
            Console.WriteLine("Begin Diagnostics");
            foreach (var d in diagnostics)
            {
                Console.WriteLine(d);
            }
            */

            var analyzer = new PlatoAnalyzer.PlatoAnalyzer();
            analyzer.Analyze(compilation);
            //TestAnalyzer(analyzer);
            TestRewriter(analyzer);
        }

        public static void TestRewriter(PlatoAnalyzer.PlatoAnalyzer analyzer)
        {
            foreach (var f in analyzer.Mapping.GetFunctions())
            {
                Console.WriteLine($"Function {f.ReturnType} {f.Name} {f.Parameters}");

                Console.WriteLine("Old body");
                Console.WriteLine(f.Body.ToFormattedString());

                var body = f.Body.Rewrite(x => x.NormalizeExpressions());

                Console.WriteLine("New body");
                Console.WriteLine(body.ToFormattedString());

                var inlinedBody = body.Rewrite(st => analyzer.Mapping.InlineFunctions(st));

                Console.WriteLine("New body with inlined functions");
                Console.WriteLine(inlinedBody.ToFormattedString());
            }
        }

        
        [SetUp]
        public void Setup()
        {
        }

        public static readonly int KB = 1024;
        public static readonly int MB = KB * KB;
        public static readonly int GB = KB * MB;
        public static readonly int[] DefaultSizes = {100 * KB, MB, 10 * MB};

        [Test]
        public void LinqTests()
        {
            OutputResults(CompareSelect(DefaultSizes, RandomIntegers, x => Math.Sqrt(x)));
        }

        [Test]
        public void CSharpAnalyzerTest()
        {

        }

        public static int[] RandomIntegers(int size)
        {
            var rng = new Random();
            var r = new int[size];
            for (var i = 0; i < size; ++i)
                r[i] = rng.Next();
            return r;
        }

        public static List<BenchmarkResult<int>> CompareSelect<T>(int[] sizes, Func<int, T[]> genInput, Func<T, double> f)
        {
            var r = new List<BenchmarkResult<int>>();

            r.AddRange(Benchmark(
                "Selection compare ToArray",
                sizes,
                genInput,
                x => x.ToPlato().Select(f).ToArray(),
                x => x.Select(f).ToArray(),
                Enumerable.SequenceEqual
            ));

            r.AddRange(Benchmark(
                "Selection compare ToList",
                sizes,
                genInput,
                x => x.ToPlato().Select(f).ToList(),
                x => x.Select(f).ToList(),
                Enumerable.SequenceEqual
            ));

            r.AddRange(Benchmark(
                "Selection compare Count",
                sizes,
                genInput,
                x => x.ToPlato().Select(f).Count(),
                x => x.Select(f).Count(),
                (r1, r2) => r1.Equals(r2)
            ));

            r.AddRange(Benchmark(
                "Selection compare Sum",
                sizes,
                genInput,
                x => x.ToPlato().Select(f).Aggregate((acc, x2) => acc + x2),
                x => x.Select(f).Aggregate((acc, x2) => acc + x2),
                (r1, r2) => r1.Equals(r2)
            ));

            return r;
        }

        public static void OutputResults<TArg>(System.Collections.Generic.IEnumerable<BenchmarkResult<TArg>> results)
        {
            foreach (var x in results)
            {
                Console.WriteLine(x.Message);
                Console.WriteLine($"Result A = {(int)x.Span1.TotalMilliseconds}");
                Console.WriteLine($"Result B = {(int)x.Span2.TotalMilliseconds}");
            }
        }

        public static List<BenchmarkResult<TArg>> Benchmark<TArg, TInput, TResult1, TResult2>(
            string testName, 
            TArg[] args, 
            Func<TArg, TInput> genInput, 
            Func<TInput, TResult1> func1, 
            Func<TInput, TResult2> func2, 
            Func<TResult1, TResult2, bool> compare)
        {
            var results = new List<BenchmarkResult<TArg>>();

            foreach (var arg in args)
            {
                try
                {
                    var input = genInput(arg);

                    var sw = Stopwatch.StartNew();
                    var result1 = func1(input);
                    var elapsed1 = sw.Elapsed;

                    sw = Stopwatch.StartNew();
                    var result2 = func2(input);
                    var elapsed2 = sw.Elapsed;

                    var result = compare(result1, result2);
                    results.Add(new BenchmarkResult<TArg>(arg, elapsed1, elapsed2, result, $"Result {result} for {testName} with {arg}"));

                }
                catch (Exception e)
                {
                    results.Add(new BenchmarkResult<TArg>(arg, TimeSpan.MinValue, TimeSpan.MinValue, false, $"Error {e.Message} for {testName} with {arg}"));
                }
            }
            return results;
        }
    }
}