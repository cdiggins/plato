// TKT-1137: the temporaries a body needs are numbered from zero inside that body.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// Two functions emitted one after the other must name their temporaries (<c>_lp</c>, <c>_n</c>,
/// <c>_i</c>, <c>_shared</c>, <c>_var</c>) independently, so that editing or adding the first leaves
/// the second's text unchanged and a library edit changes only the generated files it touches.
/// </summary>
[TestFixture]
public static class TemporaryNumberingTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "SharedCallsFixtures", "shared-calls.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static string Emit(CSharpWriter writer, string name)
    {
        var definition = Compilation.Value.LibraryDefinitionsByName["SharedProbe"].Functions.Single(f => f.Name == name);
        var tir = writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null);
        return new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
    }

    private static CSharpWriter NewWriter()
        => new(Compilation.Value, "unused-temporary-numbering")
            { ExtensionStyle = true, LowerLoops = true, FuseCombinators = true, ShareCalls = true };

    [TestCase("InLambda")]
    [TestCase("Make")]
    [TestCase("Twice")]
    public static void AFunctionEmitsTheSameTextWhateverWasEmittedBefore(string name)
    {
        var alone = Emit(NewWriter(), name);

        var writer = NewWriter();
        Emit(writer, "Make");
        Emit(writer, "InLambda");
        Emit(writer, "ArrayResult");
        var after = Emit(writer, name);

        Assert.That(after, Is.EqualTo(alone));
    }

    [Test]
    public static void EveryBodyStartsItsLoopTemporariesAtZero()
    {
        var writer = NewWriter();
        var first = Emit(writer, "Make");
        var second = Emit(writer, "Make");
        Assert.That(first, Does.Contain("_lp0").Or.Contain("_n0"));
        Assert.That(second, Is.EqualTo(first));
    }
}
