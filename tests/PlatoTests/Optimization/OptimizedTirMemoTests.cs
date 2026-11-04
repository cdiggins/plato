// TKT-1152 C7: CSharpWriter.OptimizedTir memoises a function's optimized TIR and leaves the temporary counters alone.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// A later chunk inspects a callee's optimized body while the caller is being emitted. The inspection runs the
/// optimizer passes, which draw temporary names from per-thread counters (TKT-1137); it must put the counters back,
/// so that the caller's remaining temporaries are numbered as if nothing had been inspected, and a second request
/// must return the tree already built rather than build another.
/// </summary>
[TestFixture]
public static class OptimizedTirMemoTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "SharedCallsFixtures", "shared-calls.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static CSharpWriter NewWriter()
        => new(Compilation.Value, "unused-optimized-tir-memo")
            { ExtensionStyle = true, LowerLoops = true, FuseCombinators = true, ShareCalls = true };

    private static Ara3D.Geometry.Compiler.Symbols.FunctionDef Function(string name)
        => Compilation.Value.LibraryDefinitionsByName["SharedProbe"].Functions.Single(f => f.Name == name);

    // The body writer draws the remaining temporaries (the _var captures) from the counters after the passes ran.
    private static string EmitAfterPasses(CSharpWriter writer, string name, string inspectedMidway)
    {
        var tir = writer.RunOptimizerPasses(writer.TryGetStaticTir(Function(name)), null);
        if (inspectedMidway != null)
            Assert.That(writer.OptimizedTir(Function(inspectedMidway), null), Is.Not.Null);
        return new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
    }

    [TestCase("InLambda", "Make")]
    [TestCase("Twice", "InLambda")]
    [TestCase("Make", "ArrayResult")]
    public static void InspectingACalleeBetweenPassesAndBodyWriterLeavesTheCallersTextUnchanged(string caller, string callee)
    {
        var alone = EmitAfterPasses(NewWriter(), caller, null);
        var inspected = EmitAfterPasses(NewWriter(), caller, callee);
        Assert.That(inspected, Is.EqualTo(alone));
    }

    [Test]
    public static void InspectingACalleeRestoresEveryCounter()
    {
        var writer = NewWriter();
        writer.RunOptimizerPasses(writer.TryGetStaticTir(Function("InLambda")), null);
        var before = CSharpWriter.TemporaryCounters.Save();
        writer.OptimizedTir(Function("Make"), null);
        writer.OptimizedTir(Function("InLambda"), null);
        Assert.That(CSharpWriter.TemporaryCounters.Save(), Is.EqualTo(before));
    }

    [Test]
    public static void ASecondRequestReturnsTheMemoisedTree()
    {
        var writer = NewWriter();
        var first = writer.OptimizedTir(Function("InLambda"), null);
        var second = writer.OptimizedTir(Function("InLambda"), null);
        Assert.That(first, Is.Not.Null);
        Assert.That(second, Is.SameAs(first));
        Assert.That(writer.OptimizedTir(Function("Make"), null), Is.Not.SameAs(first));
    }

    [Test]
    public static void TheInspectedTreeIsTheOneAFreshWriterEmits()
    {
        var inspected = NewWriter().OptimizedTir(Function("InLambda"), null).Body.ToString();
        var writer = NewWriter();
        writer.RunOptimizerPasses(writer.TryGetStaticTir(Function("Make")), null);
        var direct = writer.RunOptimizerPasses(writer.TryGetStaticTir(Function("InLambda")), null).Body.ToString();
        Assert.That(inspected, Is.EqualTo(direct));
    }
}
