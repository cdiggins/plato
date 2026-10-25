using System.Reflection;
using System.Diagnostics;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using RuntimeBoolean = Ara3D.Geometry.Boolean;
using RuntimeInteger = Ara3D.Geometry.Integer;
using RuntimeNumber = Ara3D.Geometry.Number;

namespace PlatoTests.Optimization;

[TestFixture]
public static class MultiConsumerFusionTests
{
    private sealed record Program(CSharpWriter Writer, Assembly Assembly, string Source);
    private static readonly Dictionary<(bool, bool, bool), Program> Programs = new();
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "MultiConsumerFusionFixtures", "folds.plato");
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
                .Append(CheckerTestSupport.ParseFile(fixture)).ToList());
    });

    private static Program GetProgram(bool enabled, bool loops = true, bool combined = false)
    {
        if (Programs.TryGetValue((enabled, loops, combined), out var result))
            return result;
        var writer = new CSharpWriter(Compilation.Value, "unused-multi-consumer")
        {
            ExtensionStyle = true, LowerLoops = loops, FuseConsumers = enabled,
            ExactOutputAllocation = combined, FuseCombinators = combined,
            InlineCalls = combined, InlineStatements = combined
        };
        writer.WriteAll();
        var names = Compilation.Value.LibraryDefinitionsByName["MultiConsumerProbe"].Functions.Select(f => f.Name).ToHashSet();
        var methods = writer.Files.Where(file => file.Key is "MultiConsumerProbe.g.cs" or "Extensions.g.cs")
            .SelectMany(file => CSharpSyntaxTree.ParseText(file.Value.ToString()).GetRoot()
            .DescendantNodes().OfType<MethodDeclarationSyntax>())
            .Where(m => names.Contains(m.Identifier.Text) && m.Modifiers.Any(SyntaxKind.StaticKeyword)).ToList();
        Assert.That(methods, Has.Count.EqualTo(names.Count), "Every fixture must be emitted exactly once.");
        var source = "using System; using System.Collections.Generic; using System.Runtime.CompilerServices; "
            + "using static System.Runtime.CompilerServices.MethodImplOptions; namespace Ara3D.Geometry { public static class GeneratedFolds { "
            + string.Join("\n", methods.Select(m => m.ToString()).Concat(NumberOperatorAdapters(writer))) + " } }";
        var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source }, LanguageVersion.Default, OptimizationLevel.Release);
        result = new Program(writer, assembly, source);
        Programs.Add((enabled, loops, combined), result);
        return result;
    }

    private static IEnumerable<string> NumberOperatorAdapters(CSharpWriter writer)
    {
        // Partial structs cannot extend the runtime in another assembly. Preserve the generated
        // operator bodies, adapting only their instance receiver to a test-local extension receiver.
        var methods = CSharpSyntaxTree.ParseText(writer.Files["_Number.g.cs"].ToString()).GetRoot()
            .DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text is "Add" or "Multiply"
                && !m.Modifiers.Any(SyntaxKind.StaticKeyword) && m.ParameterList.Parameters.Count == 1
                && m.ParameterList.Parameters[0].Type!.ToString() == "Number").ToList();
        Assert.That(methods, Has.Count.EqualTo(2));
        var receiver = SyntaxFactory.Parameter(SyntaxFactory.Identifier("self"))
            .WithType(SyntaxFactory.ParseTypeName("Number"))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)));
        return methods.Select(method => method
            .ReplaceNodes(method.DescendantNodes().OfType<ThisExpressionSyntax>(), (_, _) => SyntaxFactory.IdentifierName("self"))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
            .WithParameterList(method.ParameterList.WithParameters(method.ParameterList.Parameters.Insert(0, receiver)))
            .NormalizeWhitespace().ToFullString());
    }

    private static MethodInfo Method(Program program, string name)
        => program.Assembly.GetType("Ara3D.Geometry.GeneratedFolds", true)!.GetMethod(name)!;

    private static RuntimeNumber Run(Program program, string name, params object[] arguments)
        => (RuntimeNumber)Method(program, name).Invoke(null, arguments)!;

    private static long Bits(RuntimeNumber number)
        => BitConverter.DoubleToInt64Bits((double)number);

    private static string Body(Program program, string name)
        => CSharpSyntaxTree.ParseText(program.Source).GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == name).ToString();

    private static int LoopCount(Program program, string name)
        => CSharpSyntaxTree.ParseText(Body(program, name)).GetRoot().DescendantNodes().OfType<ForStatementSyntax>().Count();

    private static TirFunction Input(CSharpWriter writer, string name)
        => writer.TryGetStaticTir(Compilation.Value.LibraryDefinitionsByName["MultiConsumerProbe"].Functions.Single(f => f.Name == name));

    [Test]
    public static void ActualTirContainsResolvedAdjacentReductionsAndBothModesExecuteBitwise()
    {
        var off = GetProgram(false);
        var on = GetProgram(true);
        var input = Input(on.Writer, "Pair");
        Assert.That(input.Body, Is.TypeOf<TirBlock>());
        Assert.That(TirMultiConsumerFusion.Rewrite(input, on.Writer).AllNodes.OfType<TirMultiReduction>().Count(), Is.EqualTo(1));
        Assert.That(Body(on, "Pair"), Does.Contain("_mc0_source"));
        Assert.That(Body(off, "Pair"), Does.Not.Contain("_mc"));
        Assert.That(LoopCount(off, "Pair"), Is.EqualTo(3));
        Assert.That(LoopCount(on, "Pair"), Is.EqualTo(2));
        foreach (var n in new[] { 0, 1, 2, 37, 1024 })
        foreach (var value in new[] { 0d, -0d, 0.1, -2.5, 1e200, double.PositiveInfinity, double.NegativeInfinity, double.NaN })
        foreach (var first in new[] { false, true })
        {
            var args = new object[] { (RuntimeInteger)n, (RuntimeNumber)value, (RuntimeNumber)0.25, (RuntimeBoolean)first };
            Assert.That(Bits(Run(on, "Pair", args)), Is.EqualTo(Bits(Run(off, "Pair", args))), $"n={n}, value={value}, first={first}");
        }
        Assert.That(Body(on, "Names"), Does.Contain("_mc1_source"));
        Assert.That(Body(on, "UnusedLambdaName"), Does.Contain("_mc1_source"));
        Assert.That(Bits(Run(on, "Names", (RuntimeInteger)3, (RuntimeNumber)2)), Is.EqualTo(Bits(Run(off, "Names", (RuntimeInteger)3, (RuntimeNumber)2))));
    }

    [Test]
    public static void ProducerRunsOnceBeforeFoldsAndPreservesItsFault()
    {
        foreach (var enabled in new[] { false, true })
        {
            var program = GetProgram(enabled);
            var trace = new List<int>();
            Func<RuntimeInteger, RuntimeNumber> produce = i => { trace.Add((int)i); return (RuntimeNumber)((int)i + 0.5); };
            Assert.That((double)Run(program, "Producer", (RuntimeInteger)4, produce, (RuntimeBoolean)true), Is.EqualTo(8d));
            Assert.That(trace, Is.EqualTo(new[] { 0, 1, 2, 3 }));
            trace.Clear();
            Func<RuntimeInteger, RuntimeNumber> fail = i => { trace.Add((int)i); throw new InvalidOperationException("producer"); };
            var error = Assert.Throws<TargetInvocationException>(() => Run(program, "Producer", (RuntimeInteger)4, fail, (RuntimeBoolean)false));
            Assert.That(error!.InnerException!.Message, Is.EqualTo("producer"));
            Assert.That(trace, Is.EqualTo(new[] { 0 }));
        }
    }

    [TestCase("Foreign")]
    [TestCase("DependentSeed")]
    [TestCase("DependentCallback")]
    [TestCase("UnknownCallback")]
    [TestCase("Interrupted")]
    [TestCase("MutableCapture")]
    public static void UnsafeOrUnprovedShapesRetainSeparateFolds(string name)
    {
        var program = GetProgram(true);
        var input = Input(program.Writer, name);
        Assert.That(TirMultiConsumerFusion.Rewrite(input, program.Writer), Is.SameAs(input));
        Assert.That(Body(program, name), Does.Not.Contain("_mc"));
    }

    [Test]
    public static void ArrayLiteralsAndIndependentFlagControlWork()
    {
        var off = GetProgram(false);
        var on = GetProgram(true);
        var withoutLoops = GetProgram(true, false);
        Assert.That(Body(on, "Literal"), Does.Contain("_mc0_source"));
        Assert.That(LoopCount(off, "Literal"), Is.EqualTo(2));
        Assert.That(LoopCount(on, "Literal"), Is.EqualTo(1));
        Assert.That(Body(withoutLoops, "Pair"), Does.Not.Contain("_mc"));
        foreach (var first in new[] { false, true })
        foreach (var value in new[] { -0d, 2.5, double.PositiveInfinity, double.NaN })
            Assert.That(Bits(Run(on, "Literal", (RuntimeNumber)value, (RuntimeBoolean)first)),
                Is.EqualTo(Bits(Run(off, "Literal", (RuntimeNumber)value, (RuntimeBoolean)first))));
    }

    [Test]
    public static void MarkerRebuildAndUntrustedResolvedTargetRefusal()
    {
        var program = GetProgram(true);
        var input = Input(program.Writer, "Pair");
        var output = TirMultiConsumerFusion.Rewrite(input, program.Writer);
        var marker = output.AllNodes.OfType<TirMultiReduction>().Single();
        var clone = (TirMultiReduction)marker.WithChildren(marker.Children.ToList());
        Assert.That(clone.Source, Is.SameAs(marker.Source));
        Assert.That(clone.First, Is.SameAs(marker.First));
        Assert.That(clone.Second, Is.SameAs(marker.Second));
        var call = (TirCall)marker.First.Value;
        var lambda = (TirLambda)call.Args[2];
        var add = (TirCall)lambda.Body;
        var impostor = Compilation.Value.LibraryDefinitionsByName["MultiConsumerProbe"].Functions.First();
        var forged = new TirCall(impostor, add.EmissionKind, add.ParameterTypes, add.ReturnType,
            add.Args, add.Type, add.Origin, "Add");
        var coercion = new TirCoerce(lambda.Body, lambda.Body.Type, lambda.Body.Type, null, lambda.Body.Origin);
        foreach (var unsafeBody in new TirNode[] { forged, coercion })
        {
            var args = call.Args.ToArray();
            args[2] = new TirLambda(lambda.Parameters, unsafeBody, lambda.Type, lambda.Origin);
            var changedCall = call.WithChildren(args);
            var body = (TirBlock)input.Body;
            var statements = body.Statements.ToList();
            statements[1] = new TirLet(marker.First.Def, changedCall, marker.First.Type, marker.First.Origin);
            var changed = new TirFunction(input.Original, input.Parameters, input.ReturnType,
                new TirBlock(statements, body.Origin), input.ZonkedParameterTypes, input.ZonkedReturnType);
            Assert.That(TirMultiConsumerFusion.Rewrite(changed, program.Writer), Is.SameAs(changed));
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public static void OtherFlagsAndDumpingKeepTheSameRecognizedProgram(bool statements)
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-multi-consumer-options")
        {
            ExtensionStyle = true, LowerLoops = true, FuseConsumers = true,
            FuseCombinators = true, InlineCalls = true, ExactOutputAllocation = true,
            InlineStatements = statements
        };
        var input = Input(writer, "Pair");
        TirLoopLowerer.NextId = 0;
        var normal = writer.RunOptimizerPasses(input, null);
        writer.TirDumpDir = "unused-no-flush";
        TirLoopLowerer.NextId = 0;
        var dumped = writer.RunOptimizerPasses(input, null);
        Assert.That(normal.AllNodes.OfType<TirMultiReduction>().Count(), Is.EqualTo(1));
        Assert.That(dumped.AllNodes.OfType<TirMultiReduction>().Count(), Is.EqualTo(1));
        Assert.That(dumped.Body.ToString(), Is.EqualTo(normal.Body.ToString()));
    }

    [Test]
    public static void CombinedFlagsCompileAndExecute()
    {
        var reference = GetProgram(false);
        var combined = GetProgram(true, combined: true);
        Assert.That(LoopCount(combined, "Pair"), Is.EqualTo(2));
        foreach (var first in new[] { false, true })
        {
            var args = new object[] { (RuntimeInteger)37, (RuntimeNumber)0.1, (RuntimeNumber)0.25, (RuntimeBoolean)first };
            Assert.That(Bits(Run(combined, "Pair", args)), Is.EqualTo(Bits(Run(reference, "Pair", args))));
        }
    }

    [Test, Explicit("Non-gating generated Release-code timing; select this test explicitly.")]
    public static void GeneratedReleaseTraversalTiming()
    {
        var off = Method(GetProgram(false), "Pair")
            .CreateDelegate<Func<RuntimeInteger, RuntimeNumber, RuntimeNumber, RuntimeBoolean, RuntimeNumber>>();
        var on = Method(GetProgram(true), "Pair")
            .CreateDelegate<Func<RuntimeInteger, RuntimeNumber, RuntimeNumber, RuntimeBoolean, RuntimeNumber>>();
        for (var i = 0; i < 100; i++)
        {
            off(10000, 0.1, 0.25, false);
            on(10000, 0.1, 0.25, false);
        }
        var expected = Bits(off(10000, 0.1, 0.25, false));
        double Measure(Func<RuntimeInteger, RuntimeNumber, RuntimeNumber, RuntimeBoolean, RuntimeNumber> run)
        {
            RuntimeNumber result = 0;
            var timer = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++) result = run(10000, 0.1, 0.25, false);
            timer.Stop();
            Assert.That(Bits(result), Is.EqualTo(expected));
            return timer.Elapsed.TotalMilliseconds;
        }
        var separate = new double[5];
        var fused = new double[5];
        for (var round = 0; round < separate.Length; round++)
        {
            if (round % 2 == 0)
            {
                separate[round] = Measure(off);
                fused[round] = Measure(on);
            }
            else
            {
                fused[round] = Measure(on);
                separate[round] = Measure(off);
            }
        }
        TestContext.WriteLine($"Generated Release calls, 100 calls/round, 10000 values/call, alternating order; includes source allocation.");
        TestContext.WriteLine($"Separate ms: {string.Join(", ", separate.Select(t => t.ToString("F3")))}; median {separate.OrderBy(t => t).ElementAt(2):F3}.");
        TestContext.WriteLine($"Fused ms: {string.Join(", ", fused.Select(t => t.ToString("F3")))}; median {fused.OrderBy(t => t).ElementAt(2):F3}.");
    }
}
