// Checks the bound CountBounds reports for the array a small body builds.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// Each function of <c>CountBoundsFixtures/count-bounds.plato</c> builds one array. The tests lower
/// the body as the writer does and ask CountBounds about the array-producing node.
/// </summary>
[TestFixture]
public static class CountBoundsTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "CountBoundsFixtures", "count-bounds.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static (TirNode Node, TirNode Body) Producer(string name)
    {
        TirLoopLowerer.NextId = 0;
        var writer = new CSharpWriter(Compilation.Value, "unused-count-bounds")
            { ExtensionStyle = true, LowerLoops = true, FuseCombinators = true };
        var definition = Compilation.Value.LibraryDefinitionsByName["CountBoundsProbe"].Functions.Single(f => f.Name == name);
        var tir = writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null);
        var node = tir.Body.Descendants()
            .First(d => d is TirArray || d is TirLoweredLoop loop && TirLoopLowerer.ProducesArray(loop.Kind));
        return (node, tir.Body);
    }

    private static CountBound Bound(string name)
    {
        var (node, body) = Producer(name);
        return CountBounds.OfArray(node, body);
    }

    [Test]
    public static void LiteralCountIsConstant()
        => Assert.That(Bound("Literal").Constant, Is.EqualTo(5));

    [Test]
    public static void ArrayLiteralLengthIsConstant()
        => Assert.That(Bound("Literals").Constant, Is.EqualTo(3));

    [Test]
    public static void LiteralArithmeticIsFolded()
        => Assert.That(Bound("Folded").Constant, Is.EqualTo(14));

    [Test]
    public static void OverflowingProductIsUnknown()
        => Assert.That(Bound("Overflowing").IsUnknown, Is.True);

    [Test]
    public static void ParameterCountPlusOneIsGuarded()
    {
        var bound = Bound("Guarded");
        Assert.That(bound.IsGuarded, Is.True);
        Assert.That(bound.Expression.ToString(), Does.Contain("xs").And.Contain("1"));
    }

    [Test]
    public static void MapOverAParameterIsGuardedByItsCount()
    {
        var bound = Bound("GuardedMap");
        Assert.That(bound.IsGuarded, Is.True);
        Assert.That(bound.Expression, Is.TypeOf<TirCall>());
    }

    [Test]
    public static void DivisionIsNotTotalSoUnknown()
        => Assert.That(Bound("Divided").IsUnknown, Is.True);

    [Test]
    public static void CallToAnotherFunctionIsUnknown()
        => Assert.That(Bound("Called").IsUnknown, Is.True);

    [Test]
    public static void AssignmentInScopeMakesAVariableUnstable()
    {
        var (node, body) = Producer("Guarded");
        var loop = (TirLoweredLoop)node;
        var parameter = loop.Sources[0].Descendants().OfType<TirParameter>().First();
        var scope = new TirAssign(parameter, loop.Sources[0], null, null);
        Assert.That(CountBounds.OfCount(loop.Sources[0], scope).IsUnknown, Is.True);
        Assert.That(CountBounds.OfCount(loop.Sources[0], body).IsGuarded, Is.True);
    }
}
