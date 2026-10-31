// Checks which sites the decoder-wave rewrites change, case by case of each rule (TKT-1138, TKT-1142).
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// Each function of <c>DecoderRewriteFixtures/rewrites.plato</c> is one case of a rule: the scrutinee binding of
/// <c>TirScrutineeBinding</c> (<c>--bind-scrutinee</c>), the sinking of <c>TirSinkBindings</c>
/// (<c>--sink-bindings</c>) or the wide call sharing of <c>TirSharedCalls</c> (<c>--share-calls-wide</c>). The tests
/// read the tree the optimizer returns: how many calls of <c>Cost</c>, <c>Row</c> and <c>IsKind</c> it holds, and which
/// locals stand at the top of the body. That the compiled and interpreted values equal the plain reading, and how often
/// each engine evaluates a call, is checked on the same fixture by Plato.Interpreter.Tests.DecoderRewriteProgramTests.
/// </summary>
[TestFixture]
public static class DecoderRewriteTests
{
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "DecoderRewriteFixtures", "rewrites.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static readonly Dictionary<(bool Bind, bool Sink, bool Share, bool Wide, bool Loops), CSharpWriter> Writers = new();

    private static TirFunction Lower(string name, bool bind = false, bool sink = false, bool share = false, bool wide = false, bool loops = true)
    {
        lock (Writers)
        {
            if (!Writers.TryGetValue((bind, sink, share, wide, loops), out var writer))
                Writers[(bind, sink, share, wide, loops)] = writer = new CSharpWriter(Compilation.Value, "unused-decoder-rewrites")
                {
                    ExtensionStyle = true, LowerLoops = loops, FuseCombinators = loops, BindScrutinees = bind,
                    SinkBindings = sink, ShareCalls = share, ShareCallsWide = wide
                };
            TirLoopLowerer.NextId = 0;
            var definition = Compilation.Value.LibraryDefinitionsByName["RewriteProbe"].Functions.Single(f => f.Name == name);
            return writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null);
        }
    }

    private static int Calls(TirFunction tir, string callee)
        => tir.Body.Descendants().OfType<TirCall>().Count(c => c.Name == callee);

    private static IEnumerable<string> Locals(TirFunction tir, string prefix)
        => tir.Body.Descendants().Select(n => n switch
        {
            TirLet l => l.Def?.Name,
            TirInlineBinding b => b.Def?.Name,
            _ => null
        }).Where(n => n != null && n.StartsWith(prefix));

    private static IEnumerable<string> TopLevelLocals(TirFunction tir)
        => (tir.Body as TirBlock)?.Statements.Select(n => n switch
        {
            TirLet l => l.Def?.Name,
            TirInlineBinding b => b.Def?.Name,
            _ => null
        }).Where(n => n != null) ?? Enumerable.Empty<string>();

    // --- TKT-1138: a match evaluates its scrutinee once ---------------------------------------------

    [TestCase("MatchBody", 4, 1, Description = "a test and three field reads name the scrutinee")]
    [TestCase("MatchInLocal", 4, 1, Description = "the match is the value of a local")]
    [TestCase("MatchOperand", 4, 1, Description = "the match is an operand of a call")]
    public static void ScrutineeIsBoundOnce(string name, int before, int after)
    {
        Assert.That(Calls(Lower(name), "Row"), Is.EqualTo(before));
        var bound = Lower(name, bind: true);
        Assert.That(Calls(bound, "Row"), Is.EqualTo(after));
        Assert.That(Locals(bound, "_scrut").ToList(), Has.Count.EqualTo(1));
        Assert.That(Calls(Lower(name, bind: true, share: true), "Row"), Is.EqualTo(after));
    }

    [TestCase("MatchOnParameter", Description = "a parameter costs nothing to repeat")]
    [TestCase("MatchInArm", Description = "an arm of a conditional is not evaluated on every path")]
    [TestCase("MatchInLambda", Description = "a lambda body is evaluated when the lambda is called")]
    public static void ScrutineeStaysAsWritten(string name)
    {
        Assert.That(Lower(name), Is.Not.Null, "the function has a static body without the switch");
        var bound = Lower(name, bind: true);
        Assert.That(bound, Is.Not.Null, "and with it");
        Assert.That(Locals(bound, "_scrut"), Is.Empty);
        Assert.That(Calls(bound, "Row"), Is.EqualTo(Calls(Lower(name), "Row")));
    }

    [Test]
    public static void ScrutineeBindingIsDeclaredBeforeTheStatementAndReadAfterIt()
    {
        var body = (TirBlock)Lower("MatchBody", bind: true).Body;
        Assert.That(body.Statements, Has.Count.EqualTo(2));
        Assert.That(body.Statements[0], Is.TypeOf<TirLet>());
        Assert.That(((TirLet)body.Statements[0]).Value, Is.TypeOf<TirCall>());
        Assert.That(body.Statements[1], Is.TypeOf<TirReturn>());
    }

    // --- TKT-1142: a local is evaluated on the arm that reads it -----------------------------------

    [TestCase("SunkOneArm", "x", Description = "one read, in an arm of the returned conditional")]
    [TestCase("SunkChain", "x", Description = "the read is in the innermost arm of a chain")]
    [TestCase("SunkTwoReads", "x", Description = "two reads: the return becomes an if whose arm declares it")]
    [TestCase("SunkIfBranch", "x", Description = "the read is in one branch of an if")]
    [TestCase("SunkLambdaArm", "x", Description = "a lambda in the arm reads it: the arm declares it and the lambda reads the value")]
    public static void LocalIsSunkIntoTheArm(string name, string local)
    {
        Assert.That(TopLevelLocals(Lower(name)), Does.Contain(local));
        var sunk = Lower(name, sink: true);
        Assert.That(TopLevelLocals(sunk), Does.Not.Contain(local));
        Assert.That(Calls(sunk, "Cost"), Is.EqualTo(1));
        Assert.That(InsideBranch(sunk, "Cost"), Is.True, "the call is under an arm");
    }

    [Test]
    public static void ALocalAndTheLocalItReadsBothSink()
    {
        Assert.That(TopLevelLocals(Lower("SunkChained")), Is.EqualTo(new[] { "x", "y" }));
        var sunk = Lower("SunkChained", sink: true);
        Assert.That(TopLevelLocals(sunk), Is.Empty);
        Assert.That(Calls(sunk, "Cost"), Is.EqualTo(2));
        Assert.That(InsideBranch(sunk, "Cost"), Is.True);
    }

    [TestCase("SunkKeptCondition", Description = "the condition reads it: evaluated on every path")]
    [TestCase("SunkKeptBothArms", Description = "both arms read it")]
    [TestCase("SunkKeptStrict", Description = "the read is not under a conditional")]
    [TestCase("SunkKeptLambda", Description = "a lambda is not an arm: it may run any number of times")]
    [TestCase("SunkKeptCheap", Description = "a variable is free to read")]
    public static void LocalStaysWhereItWas(string name)
    {
        var plain = Lower(name);
        var sunk = Lower(name, sink: true);
        Assert.That(TopLevelLocals(sunk), Is.EqualTo(TopLevelLocals(plain)));
        Assert.That(sunk.Body.ToString(), Is.EqualTo(plain.Body.ToString()));
    }

    [Test]
    public static void SinkingIsOffByDefaultAndTheOptionsDefaultOff()
    {
        Assert.That(new LoopOptimizationOptions().SinkBindings, Is.False);
        Assert.That(new LoopOptimizationOptions().BindScrutinees, Is.False);
        Assert.That(new LoopOptimizationOptions().ShareCallsWide, Is.False);
        Assert.That(new LoopOptimizationOptions().Elaboration.Any, Is.False);
        Assert.That(new LoopOptimizationOptions(SinkBindings: true).Elaboration.SinkBindings, Is.True);
        Assert.That(new LoopOptimizationOptions(BindScrutinees: true).Elaboration.BindMatchScrutinees, Is.True);
    }

    private static IEnumerable<string> Names(TirNode n)
        => n?.Descendants().OfType<TirCall>().Select(c => c.Name) ?? Enumerable.Empty<string>();

    private static bool InsideBranch(TirFunction tir, string callee)
        => tir.Body.Descendants().Any(n => n switch
        {
            TirConditional c => Names(c.IfTrue).Concat(Names(c.IfFalse)).Contains(callee),
            TirIf i => Names(i.IfTrue).Concat(Names(i.IfFalse)).Contains(callee),
            _ => false
        });

    // --- TKT-1142: --share-calls-wide ---------------------------------------------------------------

    [TestCase("AcrossStatements", "Cost", 2, 1, Description = "the same call in two statements")]
    [TestCase("AcrossBranch", "Cost", 2, 1, Description = "the first statement evaluates it, the return names it in an arm")]
    [TestCase("AcrossKind", "IsKind", 2, 1, Description = "a sum case written as a factory is part of the call")]
    [TestCase("InLambdaTwice", "Cost", 2, 1, Description = "a lambda body that names the call twice")]
    [TestCase("InLambdaOuter", "Cost", 2, 1, Description = "a lambda names a call of the block that the block evaluates")]
    public static void CallIsSharedAcrossStatementsAndLambdas(string name, string callee, int before, int after)
    {
        Assert.That(Calls(Lower(name, share: true, loops: false), callee), Is.EqualTo(before), "--share-calls alone");
        var wide = Lower(name, share: true, loops: false, wide: true);
        Assert.That(Calls(wide, callee), Is.EqualTo(after));
        Assert.That(Locals(wide, "_shared"), Is.Not.Empty, wide.Body.ToString());
    }

    [TestCase("AcrossKept", "Cost", Description = "the earlier statement names it only on an arm")]
    [TestCase("AcrossDifferent", "Cost", Description = "different arguments are different calls")]
    [TestCase("InLambdaKept", "Cost", Description = "only the branches of the lambda name it")]
    public static void CallStaysWhereItWas(string name, string callee)
    {
        var wide = Lower(name, share: true, loops: false, wide: true);
        Assert.That(Calls(wide, callee), Is.EqualTo(Calls(Lower(name, share: true, loops: false), callee)));
        Assert.That(Locals(wide, "_shared"), Is.Empty);
    }

    [Test]
    public static void WideSharingIsOffWithoutItsSwitch()
    {
        foreach (var name in new[] { "AcrossStatements", "AcrossBranch", "AcrossKind", "InLambdaTwice", "InLambdaOuter" })
            Assert.That(Locals(Lower(name, share: true, loops: false), "_shared"), Is.Empty, name);
    }

    [Test]
    public static void ALambdaThatNeedsALocalBecomesABlock()
    {
        var wide = Lower("InLambdaTwice", share: true, loops: false, wide: true);
        var lambda = wide.Body.Descendants().OfType<TirLambda>().Single();
        Assert.That(lambda.Body, Is.TypeOf<TirBlock>());
        Assert.That(((TirBlock)lambda.Body).Statements.Last(), Is.TypeOf<TirReturn>());
    }
}
