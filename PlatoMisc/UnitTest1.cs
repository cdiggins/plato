using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PlatoAnalyzer;
using PlatoLib;

namespace PlatoTest
{
    public record BenchmarkResult<TArg>(TArg Arg, TimeSpan Span1, TimeSpan Span2, bool Result, string Message);

    // Need to compare:

    public class GeneratorTests
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

        private static Compilation CreateCompilation(string source)
            => CSharpCompilation.Create("compilation",
                new[] { CSharpSyntaxTree.ParseText(source) },
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        public static readonly int KB = 1024;
        public static readonly int MB = KB * KB;
        public static readonly int GB = KB * MB;
        public static readonly int[] DefaultSizes = new[] {100 * KB, MB, 10 * MB};

        [Test]
        public void Test1()
        {
            
            var a = new[] {1, 2, 3, 4};
            var p = a.ToPlato();
            foreach (var x in p)
                Console.WriteLine(x);

            OutputResults(CompareSelect(DefaultSizes, RandomIntegers, x => Math.Sqrt(x)));
        }

        [Test]
        public void Test2()
        {
            Console.WriteLine("Test 2");
            //HelloWorld.SayHello();
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
                System.Linq.Enumerable.SequenceEqual
            ));

            r.AddRange(Benchmark(
                "Selection compare ToList",
                sizes,
                genInput,
                x => x.ToPlato().Select(f).ToList(),
                x => x.Select(f).ToList(),
                System.Linq.Enumerable.SequenceEqual
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
                x => x.ToPlato().Select(f).Aggregate((acc, x) => acc + x),
                x => x.Select(f).Aggregate((acc, x) => acc + x),
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