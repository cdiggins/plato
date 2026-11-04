// Checks the facts TirOwnership reports for the array bindings of small bodies.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// Each function of <c>OwnershipFixtures/ownership.plato</c> holds one binding of interest. The tests
/// lower the body as the writer does (loops lowered, nothing rewritten in place) and read the facts.
/// </summary>
[TestFixture]
public static class OwnershipTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "OwnershipFixtures", "ownership.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static (TirNode Body, TirOwnership Facts) Analyze(string name)
    {
        TirLoopLowerer.NextId = 0;
        var writer = new CSharpWriter(Compilation.Value, "unused-ownership")
            { ExtensionStyle = true, LowerLoops = true, FuseCombinators = true };
        var definition = Compilation.Value.LibraryDefinitionsByName["OwnershipProbe"].Functions.Single(f => f.Name == name);
        var tir = writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null);
        return (tir.Body, TirOwnership.Analyze(tir.Body, new TirArrayIdentity(Compilation.Value)));
    }

    private static BindingFacts Variable(string function, string variable)
    {
        var (body, facts) = Analyze(function);
        var def = body.Descendants().OfType<TirLet>().Select(l => l.Def).First(d => d?.Name == variable);
        return facts.Of(def);
    }

    [Test]
    public static void FreshTemporaryOfALoweredLoop()
    {
        var (body, facts) = Analyze("OfTemp");
        var loop = body.Descendants().OfType<TirLoweredLoop>().Single(l => TirLoopLowerer.ProducesArray(l.Kind));
        var temp = facts.OfTemp(loop.TempName);
        Assert.That(temp.Fresh, Is.True);
        Assert.That(temp.Declarations, Is.EqualTo(1));
        Assert.That(temp.Captured, Is.False);
        Assert.That(temp.Uses.Count, Is.EqualTo(1));
        Assert.That(facts.OfTemp("no such temp"), Is.Null);
    }

    [Test]
    public static void FreshVariableIsOwnedAndBorrowed()
    {
        var a = Variable("Fresh", "a");
        Assert.That(a.Fresh && a.Declarations == 1, Is.True);
        Assert.That(a.Aliased || a.Captured || a.LoopCarried, Is.False);
        Assert.That(a.Uses.Select(u => u.Kind), Is.All.EqualTo(TirUpdateFlow.UseKind.Borrow));
    }

    [Test]
    public static void AliasedBindings()
    {
        var a = Variable("Aliased", "a");
        Assert.That(a.Uses.Any(u => u.Kind == TirUpdateFlow.UseKind.Escape), Is.True, "handed to b and c");
        Assert.That(Variable("Aliased", "b").Fresh, Is.False, "b = a names an existing array");
        var c = Variable("Aliased", "c");
        Assert.That(c.Fresh, Is.True);
        Assert.That(c.Aliased, Is.True, "c is later assigned a");
    }

    [Test]
    public static void RealLambdaCapturesALoopCallbackDoesNot()
    {
        Assert.That(Variable("RealLambda", "a").Captured, Is.True);
        Assert.That(Variable("LoopCallback", "a").Captured, Is.False);
    }

    [Test]
    public static void LoopCarriedAgainstPerPass()
    {
        var xs = Variable("Carried", "xs");
        Assert.That(xs.LoopCarried, Is.True);
        Assert.That(xs.AssignedLoops, Is.Not.Empty);
        Assert.That(Variable("PerPass", "row").LoopCarried, Is.False);
    }

    [Test]
    public static void ParameterIsNeverFreshAndMayBeCaptured()
    {
        var (body, facts) = Analyze("OfParameter");
        var def = body.Descendants().OfType<TirParameter>().Select(p => p.Def).First(d => d.Name == "a");
        var a = facts.OfParameter(def);
        Assert.That(a.Fresh, Is.False);
        Assert.That(a.Captured, Is.True);
    }

    [Test]
    public static void NonArrayBindingsHaveNoFacts()
    {
        var (_, facts) = Analyze("Fresh");
        Assert.That(facts.Of(null), Is.Null);
        Assert.That(facts.OfParameter(null), Is.Null);
    }
}
