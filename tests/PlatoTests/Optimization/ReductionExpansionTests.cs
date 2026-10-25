using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace PlatoTests.Optimization;

using Number = Ara3D.Geometry.Number;

[TestFixture]
public static class ReductionExpansionTests
{
    private delegate Number Probe(Number seed, Func<Number, Number> value,
        Func<Number, Number, Number, Number> combine);

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
        CheckerTestSupport.CompileFolders(new[]
        {
            Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation"),
            Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "ReductionExpansionFixtures")
        }));

    private static Probe Compile(string name, bool optimize)
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-reduction-expansion")
            { Optimize = optimize, LowerLoops = true, FuseCombinators = true };
        var definition = Compilation.Value.LibraryDefinitionsByName["ReductionExpansionProbe"].Functions.Single(f => f.Name == name);
        var input = writer.TryGetStaticTir(definition);
        // Assert refusal before traversing the replacement: the old pass builds an exponential DAG.
        var expanded = TirComponentUnroller.TryUnroll(input.Body, null, writer);
        if (name == "SingleUse")
            Assert.That(expanded, Is.Not.Null);
        else
            Assert.That(expanded, Is.Null, "Computed fold operands must not be duplicated.");
        var tir = writer.RunOptimizerPasses(input, null);
        Assert.That(tir.AllNodes.Count(), Is.LessThan(1000));
        if (name != "SingleUse")
            Assert.That(tir.AllNodes.OfType<TirLoweredLoop>().Any(l => l.Kind == "Reduce"), Is.True);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class FoldProbe { "
            + "public static Number Run(Number seed, Func<Number, Number> value, Func<Number, Number, Number, Number> combine) "
            + body + " } }";
        return CSharpTestCompilation.CompileAndLoad(new[] { source }, LanguageVersion.Default, OptimizationLevel.Release)
            .GetType("Ara3D.Geometry.FoldProbe")!.GetMethod("Run")!.CreateDelegate<Probe>();
    }

    [TestCase("RepeatedAccumulator", 32)]
    [TestCase("RepeatedElement", 2)]
    public static void RepeatedOperandsKeepLinearWorkAndEagerOrder(string name, int count)
    {
        foreach (var optimize in new[] { false, true })
        {
            var run = Compile(name, optimize);
            var events = new List<string>();
            Number Value(Number x) { events.Add("value:" + (double)x); return x; }
            Number Combine(Number a, Number b, Number c) { events.Add("combine"); return (double)a + (double)b - (double)c; }
            var expected = 7.0;
            for (var i = 1; i <= count; i++)
                expected = name == "RepeatedAccumulator" ? 2 * expected - i : expected;
            Assert.That((double)run(7, Value, Combine), Is.EqualTo(expected));
            Assert.That(events, Is.EqualTo(Enumerable.Range(1, count).Select(i => "value:" + i)
                .Concat(Enumerable.Repeat("combine", count))));
            events.Clear();
            Number Fault(Number x) { events.Add("value"); throw new InvalidOperationException("producer"); }
            Assert.That(() => run(7, Fault, Combine), Throws.InvalidOperationException.With.Message.EqualTo("producer"));
            Assert.That(events, Is.EqualTo(new[] { "value" }));
        }
    }

    [Test]
    public static void SingleUseAccumulatorStillExpandsAndPreservesFoldOrder()
    {
        foreach (var optimize in new[] { false, true })
        {
            var run = Compile("SingleUse", optimize);
            Assert.That((double)run(10, x => x, (a, b, _) => (double)a - (double)b), Is.EqualTo(4));
        }
    }
}
