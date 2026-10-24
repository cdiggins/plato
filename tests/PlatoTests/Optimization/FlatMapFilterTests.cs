using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace PlatoTests.Optimization;

using Boolean = Ara3D.Geometry.Boolean;
using Integer = Ara3D.Geometry.Integer;
using Number = Ara3D.Geometry.Number;

[TestFixture]
public static class FlatMapFilterTests
{
    private delegate IReadOnlyList<Number> Filter(IReadOnlyList<Number> xs, Func<Number, Boolean> keep,
        Func<Number, Number> value, Func<Number, IReadOnlyList<Number>> fallback);

    private sealed record Program(CSharpWriter Writer, Assembly Assembly, string Text);
    private static readonly Dictionary<(bool, bool), Program> Programs = new();
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "FlatMapFilterFixtures", "filter.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static string Evidence
        => Path.Combine(Path.GetDirectoryName(CheckerTestSupport.FindForwardStdLib())!, ".temp", "compiler-482");

    private static Assembly Compile(IEnumerable<string> sources, string name)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator).Where(File.Exists)
            .Where(p => !string.Equals(Path.GetFileName(p), "Ara3D.Geometry.dll", StringComparison.OrdinalIgnoreCase))
            .Select(p => MetadataReference.CreateFromFile(p));
        var trees = sources.Prepend("global using System; global using System.Collections.Generic;")
            .Select(s => CSharpSyntaxTree.ParseText(s, new CSharpParseOptions(LanguageVersion.CSharp12)));
        var compilation = CSharpCompilation.Create(name + Guid.NewGuid().ToString("N"), trees, references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.That(result.Success, Is.True, string.Join("\n", result.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }

    private static string KeepMethods(string source, Func<string, bool> keep)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        return root.RemoveNodes(root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => !keep(m.Identifier.Text)), SyntaxRemoveOptions.KeepNoTrivia)!.ToFullString();
    }

    private static string RangeReference(CSharpWriter writer)
    {
        var definition = Compilation.Value.LibraryDefinitionsByName["Primitives"].Functions.Single(f => f.Name == "Range");
        var tir = writer.RunOptimizerPasses(writer.TryGetGroundTirByTypeName(definition, "Integer"), null);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true);
        return "namespace Ara3D.Geometry { public static class RangeReference { public static IReadOnlyList<Integer> Range(this Integer self) " + body + " } }";
    }

    private static Program GetProgram(bool loops, bool fuse)
    {
        if (Programs.TryGetValue((loops, fuse), out var program))
            return program;
        var writer = new CSharpWriter(Compilation.Value, "unused-filter")
            { ExtensionStyle = true, LowerLoops = loops, FuseCombinators = fuse, InlineCalls = false };
        writer.WriteAll();
        // Use actual generated reference bodies and records, plus the test assembly's real runtime.
        var sources = new[]
        {
            writer.Files["_ProbeUv.g.cs"].ToString(),
            KeepMethods(writer.Files["Extensions.g.cs"].ToString(), n => n.StartsWith("F1") || n is "Candidates" or "Take"),
            RangeReference(writer)
        };
        var text = string.Join("\n", sources);
        Directory.CreateDirectory(Evidence);
        File.WriteAllText(Path.Combine(Evidence, $"generated-loops-{loops}-fuse-{fuse}.cs"), text);
        program = new Program(writer, Compile(sources, "FlatMapFilter_"), text);
        Programs[(loops, fuse)] = program;
        return program;
    }

    private static MethodInfo Method(Assembly assembly, string name)
        => assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == name);

    private static Filter GetFilter(Program program, string name = "F1Select")
        => Method(program.Assembly, name).CreateDelegate<Filter>();

    private static string Body(Program program, string name)
        => CSharpSyntaxTree.ParseText(program.Text).GetRoot().DescendantNodes()
            .OfType<MethodDeclarationSyntax>().Single(m => m.Identifier.Text == name).ToString();

    private static TirFunction Lowered(CSharpWriter writer, string name)
    {
        var definition = Compilation.Value.LibraryDefinitionsByName["FlatMapProbe"].Functions.Single(f => f.Name == name);
        var input = writer.TryGetStaticTir(definition);
        Assert.That(input, Is.Not.Null, name);
        return writer.RunOptimizerPasses(input, null);
    }

    [Test]
    public static void ProjectionCandidatePreflight()
    {
        var program = GetProgram(true, true);
        var captured = TirLambdaCaptureRewriter.Rewrite(Lowered(program.Writer, "Candidates").Body);
        var loop = captured.Descendants().OfType<TirLoweredLoop>().Single(n => n.Kind == "FlatMap");
        Assert.That(loop.Fn, Is.TypeOf<TirLambda>());
        var lambda = (TirLambda)loop.Fn;
        Assert.That(lambda.Body, Is.TypeOf<TirConditional>());
        var conditional = (TirConditional)lambda.Body;
        Assert.That(conditional.IfTrue, Is.TypeOf<TirArray>());
        var array = (TirArray)conditional.IfTrue;
        Assert.That(array.Elements, Has.Count.EqualTo(1));
        Assert.That(array.Type, Is.EqualTo(conditional.Type));
        Assert.That(conditional.IfFalse.Type, Is.EqualTo(conditional.Type));
        Assert.That(lambda.Type.TypeArgs.Last(), Is.EqualTo(conditional.Type));
        Assert.That(array.Type.TypeArgs.Single(), Is.EqualTo(loop.ElemType));
        Assert.That(array.Elements.Single().Type, Is.EqualTo(loop.ElemType));
        var body = Body(program, "Candidates");
        Assert.That(body, Does.Contain("if ((bool)(").And.Contain(".Take("));
        Assert.That(body.Split("Intrinsics.MakeArray<ProbeUv>(").Length - 1, Is.EqualTo(1), body);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void ValuesCountsAndCaptures(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        foreach (var input in new[] { new Number[] { 3, 1, 2, 1 }, Array.Empty<Number>() })
        foreach (var mode in new[] { "all", "none", "mixed" })
        foreach (var reversed in new[] { false, true })
        {
            var trace = new List<string>();
            bool Accept(Number x) => mode == "all" || mode == "mixed" && (double)x <= 2;
            Func<Number, Boolean> predicate = x => { trace.Add($"p{x}"); return Accept(x); };
            Func<Number, Number> value = x => { trace.Add($"v{x}"); return x; };
            Func<Number, IReadOnlyList<Number>> fallback = x => { trace.Add($"f{x}"); return new[] { (Number)9, (Number)9 }; };
            var actual = GetFilter(program, reversed ? "F1Reversed" : "F1Select")(input, predicate, value, fallback);
            var expected = input.SelectMany(x => Accept(x) != reversed ? new[] { x } : new[] { (Number)9, (Number)9 });
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(trace, Is.EqualTo(input.SelectMany(x => new[] { $"p{x}", $"{(Accept(x) != reversed ? "v" : "f")}{x}" })));
        }

        Func<Number, Boolean> keep = x => (double)x <= 2;
        Func<Number, Number> identity = x => x;
        Func<Number, IReadOnlyList<Number>> empty = _ => Array.Empty<Number>();
        var values = new Number[] { 3, 1, 2, 1 };
        foreach (var enabled in new[] { false, true })
        {
            var nested = Method(program.Assembly, "F1Nested").Invoke(null, new object[] { values, keep, identity, empty, (Boolean)enabled });
            Assert.That(nested, Is.EqualTo(enabled ? new Number[] { 1, 2, 1 } : Array.Empty<Number>()));
        }
        Func<Integer, Boolean> gridMinimum = k => values[(int)k] <= (Number)2;
        var records = (System.Collections.IEnumerable)Method(program.Assembly, "Candidates")
            .Invoke(null, new object[] { values, gridMinimum })!;
        Assert.That(records.Cast<object>().Select(v => (Number)v.GetType().GetField("U")!.GetValue(v)!),
            Is.EqualTo(new Number[] { 1, 2, 1 }));
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void SelectedFaultsAndTakeReceiverArePreserved(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        var filter = GetFilter(program);
        var input = new Number[] { 1 };
        Number Fault(string message) => throw new InvalidOperationException(message);
        Func<Number, Number> good = x => x;
        Func<Number, Number> bad = _ => Fault("value");
        Func<Number, IReadOnlyList<Number>> empty = _ => Array.Empty<Number>();
        Func<Number, IReadOnlyList<Number>> badFallback = _ => throw new InvalidOperationException("fallback");
        Func<Number, Boolean> yes = _ => true;
        Func<Number, Boolean> no = _ => false;
        Assert.That(() => filter(input, _ => throw new InvalidOperationException("predicate"), bad, badFallback),
            Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("predicate"));
        Assert.That(() => filter(input, yes, bad, badFallback), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("value"));
        Assert.That(() => filter(input, no, bad, badFallback), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("fallback"));
        Assert.That(filter(input, no, bad, empty), Is.Empty);
        Assert.That(filter(input, yes, good, badFallback), Is.EqualTo(input));
        var faultTake = Method(program.Assembly, "F1FaultTake")
            .CreateDelegate<Func<IReadOnlyList<Number>, Func<Number, Boolean>, Func<Number, Number>, Func<Number, Number>, IReadOnlyList<Number>>>();
        Assert.That(faultTake(input, yes, good, bad), Is.EqualTo(input));
        Assert.That(() => faultTake(input, no, good, bad), Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("value"));
        var calls = 0;
        Assert.That(faultTake(input, no, good, x => { calls++; return x; }), Is.Empty);
        Assert.That(calls, Is.EqualTo(1), "Take(0) must still evaluate its receiver's element");
        var both = Method(program.Assembly, "F1Both")
            .CreateDelegate<Func<IReadOnlyList<Number>, Func<Number, Boolean>, Func<Number, Number>, Func<Number, Number>, IReadOnlyList<Number>>>();
        Assert.That(both(input, yes, good, bad), Is.EqualTo(input));
        Assert.That(both(input, no, bad, good), Is.EqualTo(input));
        if (loops)
            Assert.That(Body(program, "F1Both"), Does.Not.Contain("MakeArray<Number>").And.Not.Contain("for (var _j"));
    }

    [Test]
    public static void UnsupportedSourceShapesKeepTheirCopyLoop()
    {
        var program = GetProgram(true, true);
        Func<Number, Boolean> yes = _ => true;
        Func<Number, Number> identity = x => x;
        Func<Number, IReadOnlyList<Number>> empty = _ => Array.Empty<Number>();
        var input = new Number[] { 1, 2 };
        foreach (var name in new[] { "F1Multi", "F1NestedArray", "F1Delegate" })
            Assert.That(Body(program, name), Does.Not.Contain("if ((bool)(").And.Contain("for (var _j"));
        Assert.That(GetFilter(program, "F1Multi")(input, yes, identity, empty), Is.EqualTo(new Number[] { 1, 1, 2, 2 }));
        Assert.That(GetFilter(program, "F1NestedArray")(input, yes, identity, empty), Is.EqualTo(input));
        var callback = Method(program.Assembly, "F1Delegate")
            .CreateDelegate<Func<IReadOnlyList<Number>, Func<Number, IReadOnlyList<Number>>, IReadOnlyList<Number>>>();
        Assert.That(callback(input, x => new[] { x, x }), Is.EqualTo(new Number[] { 1, 1, 2, 2 }));
    }

    private static TirCoerce Wrapped(TirNode node)
        => new(node, node.Type, node.Type, null, node.Origin);

    private static (Filter Invoke, string Body) Variant(string name, Func<TirLoweredLoop, TirLoweredLoop> change)
    {
        var writer = GetProgram(true, true).Writer;
        var tir = Lowered(writer, "F1Select");
        TirNode Rewrite(TirNode node)
        {
            if (node is TirLoweredLoop loop)
                return change(loop);
            var children = node.Children.ToArray();
            return children.Length == 0 ? node : node.WithChildren(children.Select(Rewrite).ToArray());
        }
        var function = new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, Rewrite(tir.Body),
            tir.ZonkedParameterTypes, tir.ZonkedReturnType);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), function, isStatic: true).ToString();
        var source = "namespace Ara3D.Geometry { public static class Variant { "
            + "public static IReadOnlyList<Number> Run(IReadOnlyList<Number> xs, Func<Number, Boolean> keep, "
            + "Func<Number, Number> value, Func<Number, IReadOnlyList<Number>> fallback) " + body + " } }";
        File.WriteAllText(Path.Combine(Evidence, name + ".cs"), source);
        return (Method(Compile(new[] { source }, name), "Run").CreateDelegate<Filter>(), body);
    }

    private static TirLoweredLoop WithCallback(TirLoweredLoop loop, TirNode callback)
        => new(loop.Kind, loop.Sources, callback, loop.Seed, loop.IncludeFirst, loop.TempName, loop.Id, loop.Type, loop.Origin)
            { ElemType = loop.ElemType, SourceStages = loop.SourceStages, IndexSource = loop.IndexSource };

    [TestCase("callback")]
    [TestCase("conditional")]
    [TestCase("singleton")]
    [TestCase("fallback")]
    [TestCase("mismatched-result")]
    [TestCase("unknown-result")]
    public static void WrappedAndUncertainShapesAreRefused(string shape)
    {
        var variant = Variant("refused-" + shape, loop =>
        {
            var lambda = (TirLambda)loop.Fn;
            var conditional = (TirConditional)lambda.Body;
            TirNode body = conditional;
            var type = lambda.Type;
            if (shape == "callback")
                return WithCallback(loop, Wrapped(lambda));
            if (shape == "conditional") body = Wrapped(body);
            if (shape == "singleton") body = new TirConditional(conditional.Condition, Wrapped(conditional.IfTrue), conditional.IfFalse, conditional.Type, conditional.Origin);
            if (shape == "fallback") body = new TirConditional(conditional.Condition, conditional.IfTrue, Wrapped(conditional.IfFalse), conditional.Type, conditional.Origin);
            if (shape == "mismatched-result") type = new TypeExpression(type.Def, type.TypeArgs[0], type.TypeArgs[0]);
            if (shape == "unknown-result") type = null;
            return WithCallback(loop, new TirLambda(lambda.Parameters, body, type, lambda.Origin));
        });
        Assert.That(variant.Body, Does.Not.Contain("if ((bool)(").And.Contain("Intrinsics.MakeArray<Number>"));
        Assert.That(variant.Invoke(new Number[] { 1, 2 }, x => (double)x == 1, x => x, _ => Array.Empty<Number>()),
            Is.EqualTo(new Number[] { 1 }));
    }

    [Test]
    public static void ElementCoercionKeepsItsImplicitWrapperConversion()
    {
        TirLoweredLoop ConvertElement(TirLoweredLoop loop)
        {
            var lambda = (TirLambda)loop.Fn;
            var conditional = (TirConditional)lambda.Body;
            var array = (TirArray)conditional.IfTrue;
            var integer = new TypeExpression(Compilation.Value.TypeDefinitions.Single(t => t.Name == "Integer"));
            var literal = new TirLiteral(7, Ara3D.Geometry.AST.LiteralTypesEnum.Integer, integer, null);
            var converted = new TirCoerce(literal, integer, loop.ElemType, null, null);
            var arm = new TirArray(new TirNode[] { converted }, array.Type, array.Origin);
            return WithCallback(loop, new TirLambda(lambda.Parameters,
                new TirConditional(conditional.Condition, arm, conditional.IfFalse, conditional.Type, conditional.Origin), lambda.Type, lambda.Origin));
        }
        var optimized = Variant("element-conversion", ConvertElement);
        var ordinary = Variant("element-conversion-ordinary", loop =>
        {
            var converted = ConvertElement(loop);
            return WithCallback(converted, Wrapped(converted.Fn));
        });
        Assert.That(optimized.Body, Does.Contain("if ((bool)(").And.Not.Contain("MakeArray<Number>"));
        foreach (var filter in new[] { optimized.Invoke, ordinary.Invoke })
            Assert.That(filter(new Number[] { 1, 2 }, _ => true, x => x, _ => Array.Empty<Number>()), Is.EqualTo(new Number[] { 7, 7 }));
    }

    [Test]
    public static void SingletonAllocationIsRemovedAgainstTheSameLoweredPath()
    {
        var optimized = Variant("allocation-optimized", loop => loop);
        var ordinary = Variant("allocation-ordinary", loop => WithCallback(loop, Wrapped(loop.Fn)));
        Assert.That(optimized.Body, Does.Not.Contain("MakeArray<Number>"));
        Assert.That(ordinary.Body, Does.Contain("MakeArray<Number>"));
        Func<Number, Boolean> keep = _ => true;
        Func<Number, Number> identity = x => x;
        Func<Number, IReadOnlyList<Number>> empty = _ => Array.Empty<Number>();
        var lines = new List<string> { "cpu64; net8; C#12; Release; loops=true; fuse=true; inline=false; materialized Number[] source", "Only change: a same-type TIR callback wrapper forces the ordinary emitter; it emits no conversion.", "Setup, delegate creation, reflection, and assertions outside measurement; warm 10000 calls/version; measure 1000 calls; consume count/last value.", "size,optimized bytes/call,ordinary bytes/call,saved bytes/call" };
        foreach (var size in new[] { 8, 64, 512 })
        {
            var input = Enumerable.Range(1, size).Select(x => (Number)x).ToArray();
            foreach (var filter in new[] { optimized.Invoke, ordinary.Invoke })
                for (var i = 0; i < 10000; i++) filter(input, keep, identity, empty);
            long Measure(Filter filter)
            {
                var checksum = 0d;
                var before = GC.GetAllocatedBytesForCurrentThread();
                for (var i = 0; i < 1000; i++)
                {
                    var result = filter(input, keep, identity, empty);
                    checksum += result.Count + (double)result[^1];
                }
                var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(checksum, Is.EqualTo(size * 2000d));
                return bytes;
            }
            Assert.That(optimized.Invoke(input, keep, identity, empty), Is.EqualTo(ordinary.Invoke(input, keep, identity, empty)));
            var fast = Measure(optimized.Invoke);
            var slow = Measure(ordinary.Invoke);
            lines.Add($"{size},{fast / 1000},{slow / 1000},{(slow - fast) / 1000}");
            Assert.That(slow - fast, Is.GreaterThanOrEqualTo(size * 16L * 1000));
        }
        File.WriteAllLines(Path.Combine(Evidence, "allocation.csv"), lines);
        TestContext.WriteLine(string.Join("\n", lines));
    }
}
