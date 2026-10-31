// Checks which calls the --share-calls pass (TKT-1136) evaluates once, case by case of its rule.
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// Each function of <c>SharedCallsFixtures/shared-calls.plato</c> is one case of the rule in
/// <c>writers/Plato.CSharpWriter/Optimization/TirSharedCalls.cs</c>. The tests count the calls of
/// <c>Cost</c>, <c>Row</c> and <c>Make</c> that the tree the pass returns still holds: a shared call is
/// bound once and read through a local, an unshared one stays where it was written. That the compiled value
/// equals the interpreted one is checked on the same fixture by Plato.Interpreter.Tests.SharedCallsProgramTests.
/// </summary>
[TestFixture]
public static class SharedCallsTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "SharedCallsFixtures", "shared-calls.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static TirFunction Lower(string name, bool share, bool loops = true)
    {
        TirLoopLowerer.NextId = 0;
        var writer = new CSharpWriter(Compilation.Value, "unused-shared-calls")
            { ExtensionStyle = true, LowerLoops = loops, FuseCombinators = true, ShareCalls = share };
        var definition = Compilation.Value.LibraryDefinitionsByName["SharedProbe"].Functions.Single(f => f.Name == name);
        return writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null);
    }

    private static int Calls(string name, string callee, bool share)
        => Lower(name, share).Body.Descendants().OfType<TirCall>().Count(c => c.Name == callee);

    private static int Locals(string name)
        => Lower(name, true).Body.Descendants().OfType<TirInlineBinding>().Count(b => b.Def.Name.StartsWith("_shared"));

    // The call is evaluated on every path, so it is bound once.

    [TestCase("MatchOnCall", "Row", 4, 1, Description = "a test and three field reads name the scrutinee")]
    [TestCase("Twice", "Cost", 2, 1, Description = "the same call twice")]
    [TestCase("InConditionAndBranch", "Cost", 3, 1, Description = "the condition is always evaluated")]
    [TestCase("Nested", "Cost", 4, 2, Description = "the largest call is bound; its argument stays inside it")]
    [TestCase("NestedAndInner", "Cost", 5, 2, Description = "the inner call is shared too")]
    [TestCase("TwoKinds", "Cost", 4, 2, Description = "two different calls, each twice")]
    [TestCase("InBlock", "Cost", 3, 2, Description = "one statement at a time: the second statement shares nothing")]
    public static void SharedCallIsEvaluatedOnce(string name, string callee, int before, int after)
    {
        Assert.That(Calls(name, callee, false), Is.EqualTo(before));
        Assert.That(Calls(name, callee, true), Is.EqualTo(after));
        Assert.That(Locals(name), Is.GreaterThan(0));
    }

    // Each clause that keeps an expression as written.

    [TestCase("OnlyInBranches", "Cost", Description = "no occurrence is evaluated on every path")]
    [TestCase("Different", "Cost", Description = "different arguments are different calls")]
    [TestCase("ArrayResult", "Make", Description = "an array result is left to the loop passes")]
    [TestCase("InLambda", "Cost", Description = "a lambda body is another expression")]
    public static void UnsharedCallStaysWhereItWas(string name, string callee)
    {
        Assert.That(Calls(name, callee, true), Is.EqualTo(Calls(name, callee, false)));
        Assert.That(Locals(name), Is.Zero);
    }

    [Test]
    public static void FlagOffAddsNoLocal()
    {
        foreach (var name in new[] { "MatchOnCall", "Twice", "Nested", "NestedAndInner", "TwoKinds", "InBlock" })
            Assert.That(Lower(name, false).Body.Descendants().OfType<TirInlineBinding>().Where(b => b.Def.Name.StartsWith("_shared")), Is.Empty, name);
    }

    [Test]
    public static void ASharedCallIsBoundBeforeItsUsesAndInnermostFirst()
    {
        Assert.That(Locals("Nested"), Is.EqualTo(1));
        var body = (TirBlock)Lower("NestedAndInner", true).Body;
        var bindings = body.Statements.OfType<TirInlineBinding>().ToList();
        Assert.That(bindings, Has.Count.EqualTo(2));
        // The outer call reads the first local.
        Assert.That(bindings[1].Value.Descendants().OfType<TirVariable>().Select(v => v.Def.Name), Does.Contain(bindings[0].Def.Name));
        Assert.That(body.Statements.Last(), Is.TypeOf<TirReturn>());
    }

    [Test]
    public static void OptionRecordCarriesTheSwitch()
    {
        Assert.That(new LoopOptimizationOptions().ShareCalls, Is.False);
        Assert.That(new LoopOptimizationOptions(ShareCalls: true).ShareCalls, Is.True);
    }
}
