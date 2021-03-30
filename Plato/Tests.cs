using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis;

namespace Plato
{
    public class TestResult
    {
        public string Name;
        public int InputSize;
        public object ResultA;
        public object ResultB;
        public TimeSpan SpanA;
        public TimeSpan SpanB;
    }

    public static class Tests
    {
        [Test]
        public static void TestCompile()
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var ws = new AdhocWorkspace();
            //Create new solution
            var solId = SolutionId.CreateNewId();
            var solutionInfo = SolutionInfo.Create(solId, VersionStamp.Create());
            //Create new project
            var project = ws.AddProject("Sample", LanguageNames.CSharp);
            project = project.AddMetadataReference(mscorlib);
            //Add project to workspace
            ws.TryApplyChanges(project.Solution);
            string text = @"
class C
{
    void M()
    {
        M();
        M();
    }
}";
            var sourceText = SourceText.From(text);
            //Create new document
            var doc = ws.AddDocument(project.Id, "NewDoc", sourceText);
            //Get the semantic model
            var model = doc.GetSemanticModelAsync().Result;
            //Get the syntax node for the first invocation to M()
            var methodInvocation = doc.GetSyntaxRootAsync().Result.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
            var methodSymbol = model.GetSymbolInfo(methodInvocation).Symbol;
            //Finds all references to M()
            var referencesToM = SymbolFinder.FindReferencesAsync(methodSymbol, doc.Project.Solution).Result;
            foreach (var mRef in referencesToM)
            {
                var locs = string.Join(", ", mRef.Locations.Select(loc => loc.Location));
                Console.WriteLine($"Definition = {mRef.Definition}, Locations = {locs}");
            }
        }

        public static void Measure()
        {
        }

        public static float Sqr(float x)
            => x * x;

        public static string[] SqrToString(float[] vals)
        {
            var tmp = new float[vals.Length];
            var r = new string[vals.Length];
            for (var i = 0; i < vals.Length; ++i)
                tmp[i] = vals[i];
            for (var i = 0; i < vals.Length; ++i)
                r[i] = tmp[i].ToString();
            return r;
        }

        public static string[] SqrToString_WithoutTemp(float[] vals)
        {
            var r = new string[vals.Length];
            for (var i = 0; i < vals.Length; ++i)
                r[i] = (vals[i] * vals[i]).ToString();
            return r;
        }

        public static List<string> SqrToString_WithArray(float[] vals)
        {
            var r = new List<string>();
            for (var i = 0; i < vals.Length; ++i)
                r.Add((vals[i] * vals[i]).ToString());
            return r;
        }

        public static double[] MakeInputFloatArray(int count)
            => Enumerable.Range(0, count).Select(x => x / 1000.0).ToArray();

        public static TestResult Compare<T, TOutput1, TOutput2>(T input, Func<T, TOutput1> f1, Func<T, TOutput2> f2)
        {
            var r = new TestResult();
            (r.ResultA, r.SpanA) = f1.InvokeWithTiming(input);
            (r.ResultB, r.SpanB) = f2.InvokeWithTiming(input);
            return r;
        }

        public static void Output(this TestResult result)
            => Console.WriteLine(
                $"Result A = {result.ResultA}, TimeSpan A = {result.SpanA.ToFormattedString()}\n" +
                $"Result B = {result.ResultB}, TimeSpan A = {result.SpanB.ToFormattedString()}");

        public static double SumAggregate(double[] xs)
            => xs.Aggregate((acc, x) => acc + x);

        public static double SumManual(double[] x)
        {
            var r = 0.0;
            for (var i = 0; i < x.Length; ++i)
                r += x[i];
            return r;
        }

        [Test]
        public static void Test1()
        {
            var input = MakeInputFloatArray(10 * 1000 * 1000);

            Console.WriteLine("Select chaining vs single selecct");
            Compare(input,
                xs => xs.Select(Math.Sqrt).Select(x => Math.Pow(x, 2)).Select(Math.Cos).Sum(),
                xs => xs.Select(x => Math.Cos(Math.Pow(Math.Sqrt(x), 2))).Sum())
                .Output();

            Console.WriteLine("Sum manual versus sum aggregate");
            Compare(input,
                SumManual,
                SumAggregate)
                .Output();


        }
    }
}
