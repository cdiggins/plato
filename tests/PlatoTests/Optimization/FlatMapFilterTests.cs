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

    private sealed record Program(CSharpWriter Writer, Assembly Assembly, string Text, IReadOnlyList<string> Sources);
    private static readonly Dictionary<(bool, bool, bool), Program> Programs = new();
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

    private static Assembly Compile(IReadOnlyList<string> sources)
        => CSharpTestCompilation.CompileAndLoad(
            sources.Prepend("global using System; global using System.Collections.Generic;").ToArray(),
            LanguageVersion.CSharp12, OptimizationLevel.Release);

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

    private static Program GetProgram(bool loops, bool fuse, bool inline = false)
    {
        if (Programs.TryGetValue((loops, fuse, inline), out var program))
            return program;
        var writer = new CSharpWriter(Compilation.Value, "unused-filter")
            { ExtensionStyle = true, LowerLoops = loops, FuseCombinators = fuse, InlineCalls = inline };
        writer.WriteAll();
        // Use actual generated reference bodies and records, plus the test assembly's real runtime.
        var sources = new[]
        {
            writer.Files["_ProbeUv.g.cs"].ToString(),
            KeepMethods(writer.Files["Extensions.g.cs"].ToString(), n => n.StartsWith("F1") || n.StartsWith("F3") || n.StartsWith("F4") || n is "Candidates" or "Take" or "Filter"),
            RangeReference(writer)
        };
        var text = string.Join("\n", sources);
        Directory.CreateDirectory(Evidence);
        File.WriteAllText(Path.Combine(Evidence, $"generated-loops-{loops}-fuse-{fuse}-inline-{inline}.cs"), text);
        program = new Program(writer, Compile(sources), text, sources);
        Programs[(loops, fuse, inline)] = program;
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
        Assert.That(body, Does.Contain("if ((bool)(").And.Not.Contain(".Take(").And.Not.Contain("MakeArray<ProbeUv>"), body);
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

    private const string SelectParameters = "IReadOnlyList<Number> xs, Func<Number, Boolean> keep, "
        + "Func<Number, Number> value, Func<Number, IReadOnlyList<Number>> fallback";

    private static (Filter Invoke, string Body) Variant(string name, Func<TirLoweredLoop, TirLoweredLoop> change)
        => Variant<Filter>(name, "F1Select", SelectParameters, change);

    private static (T Invoke, string Body) Variant<T>(string name, string function, string parameters,
        Func<TirLoweredLoop, TirLoweredLoop> change) where T : Delegate
    {
        var writer = GetProgram(true, true).Writer;
        var tir = Lowered(writer, function);
        TirNode Rewrite(TirNode node)
        {
            if (node is TirLoweredLoop loop)
                return change(loop);
            var children = node.Children.ToArray();
            return children.Length == 0 ? node : node.WithChildren(children.Select(Rewrite).ToArray());
        }
        var rewritten = new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, Rewrite(tir.Body),
            tir.ZonkedParameterTypes, tir.ZonkedReturnType);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), rewritten, isStatic: true).ToString();
        var source = "namespace Ara3D.Geometry { public static class Variant { "
            + "public static IReadOnlyList<Number> Run(" + parameters + ") " + body + " } }";
        File.WriteAllText(Path.Combine(Evidence, name + ".cs"), source);
        // The program's own Take, Range and records come along: a variant may still call them.
        return (Method(Compile(GetProgram(true, true).Sources.Append(source).ToArray()), "Run").CreateDelegate<T>(), body);
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

    private delegate IReadOnlyList<Number> Select3(IReadOnlyList<Number> xs, Func<Number, Boolean> keep, Func<Number, Number> value);
    private delegate IReadOnlyList<Number> Mixed(IReadOnlyList<Number> xs, Func<Number, Boolean> keep, Func<Number, IReadOnlyList<Number>> fallback);
    private delegate IReadOnlyList<Number> Indexed(IReadOnlyList<Number> xs, IReadOnlyList<Number> ys, Func<Integer, Boolean> keep);
    private delegate IReadOnlyList<Number> Counted(IReadOnlyList<Number> xs, Func<Number, Boolean> keep, Integer count);
    private delegate IReadOnlyList<Number> Unary(IReadOnlyList<Number> xs, Func<Number, Boolean> keep);
    private delegate IReadOnlyList<Number> Faulting(IReadOnlyList<Number> xs, Func<Number, Boolean> keep,
        Func<Number, Number> accept, Func<Number, Number> reject);

    private const string Select3Parameters = "IReadOnlyList<Number> xs, Func<Number, Boolean> keep, Func<Number, Number> value";
    private const string FaultingParameters = "IReadOnlyList<Number> xs, Func<Number, Boolean> keep, "
        + "Func<Number, Number> accept, Func<Number, Number> reject";

    private static readonly Number[][] Inputs = { new Number[] { 3, 1, 2, 1 }, Array.Empty<Number>() };

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void EmptyTakeArmAppendsNothing(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        foreach (var (name, keptWhen) in new[] { ("F3EmptyArm", true), ("F3EmptyFirst", false) })
        {
            var select = Method(program.Assembly, name).CreateDelegate<Select3>();
            foreach (var input in Inputs)
            foreach (var mode in new[] { "all", "none", "mixed" })
            {
                var trace = new List<string>();
                bool Accept(Number x) => mode == "all" || mode == "mixed" && (double)x <= 2;
                var actual = select(input, x => { trace.Add($"p{x}"); return Accept(x); },
                    x => { trace.Add($"v{x}"); return (Number)((double)x * 10); });
                var kept = input.Where(x => Accept(x) == keptWhen).ToArray();
                Assert.That(actual, Is.EqualTo(kept.Select(x => (Number)((double)x * 10))), $"{name} {mode}");
                Assert.That(trace, Is.EqualTo(input.SelectMany(x => Accept(x) == keptWhen ? new[] { $"p{x}", $"v{x}" } : new[] { $"p{x}" })));
            }
            if (!loops)
                continue;
            var body = Body(program, name);
            Assert.That(body, Does.Not.Contain("MakeArray").And.Not.Contain("Take(").And.Not.Contain("for (var _j")
                .And.Not.Contain("else").And.Contain(keptWhen ? "if ((bool)(" : "if (!(bool)("), body);
        }
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void EmptyTakeArmBesideACopiedArmAndOverFieldReads(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        var mixed = Method(program.Assembly, "F3Mixed").CreateDelegate<Mixed>();
        var calls = new List<Number>();
        Func<Number, IReadOnlyList<Number>> twice = x => { calls.Add(x); return new[] { x, x }; };
        Assert.That(mixed(new Number[] { 3, 1, 2 }, x => (double)x != 1, twice), Is.EqualTo(new Number[] { 3, 3, 2, 2 }));
        Assert.That(calls, Is.EqualTo(new Number[] { 3, 2 }));
        if (loops)
            Assert.That(Body(program, "F3Mixed"), Does.Not.Contain("MakeArray").And.Not.Contain("Take(").And.Not.Contain("else")
                .And.Contain("for (var _j"));

        var fields = Method(program.Assembly, "F3Fields");
        var uv = program.Assembly.GetType("Ara3D.Geometry.ProbeUv")!;
        var records = Array.CreateInstance(uv, 3);
        for (var i = 0; i < 3; i++)
            records.SetValue(Activator.CreateInstance(uv, (Number)(i + 1), (Number)(10 * (i + 1))), i);
        var result = (System.Collections.IEnumerable)fields.Invoke(null,
            new object[] { records, (Func<Number, Boolean>)(u => (double)u != 2) })!;
        Assert.That(result.Cast<object>().Select(r => (double)(Number)uv.GetField("U")!.GetValue(r)!), Is.EqualTo(new[] { 1d, 3d }));
        if (loops)
            Assert.That(Body(program, "F3Fields"), Does.Not.Contain("MakeArray").And.Not.Contain("Take(").And.Not.Contain("else"));
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void EmptyTakeReceiverThatMayFaultIsStillEvaluated(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        var indexed = Method(program.Assembly, "F3Index").CreateDelegate<Indexed>();
        var xs = new Number[] { 1, 2 };
        Assert.That(indexed(xs, new Number[] { 5, 6 }, k => (int)k == 0), Is.EqualTo(new Number[] { 1 }));
        Assert.That(indexed(xs, Array.Empty<Number>(), _ => true), Is.EqualTo(xs), "a kept element never reads the dropped arm's receiver");
        Assert.That(() => indexed(xs, new Number[] { 5 }, k => (int)k == 0), Throws.TypeOf<ArgumentOutOfRangeException>(),
            "the dropped element's receiver ys[1] must still be read");

        var calls = new List<Number>();
        var rejecting = Method(program.Assembly, "F3Call").CreateDelegate<Mixed>();
        Assert.That(rejecting(new Number[] { 3, 1, 2 }, x => (double)x == 1, x => { calls.Add(x); return Array.Empty<Number>(); }),
            Is.EqualTo(new Number[] { 1 }));
        Assert.That(calls, Is.EqualTo(new Number[] { 3, 2 }));
        if (loops)
            foreach (var name in new[] { "F3Index", "F3Call" })
                Assert.That(Body(program, name), Does.Contain("IReadOnlyList<Number> _p").And.Not.Contain("for (var _j"), name);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void TakeOtherThanTheLiteralZeroIsCopied(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        var counted = Method(program.Assembly, "F3Count").CreateDelegate<Counted>();
        var xs = new Number[] { 1, 2 };
        Assert.That(counted(xs, x => (double)x == 1, (Integer)0), Is.EqualTo(new Number[] { 1 }));
        Assert.That(counted(xs, x => (double)x == 1, (Integer)1), Is.EqualTo(new Number[] { 1, 2 }));
        if (loops)
            Assert.That(Body(program, "F3Count"), Does.Contain("for (var _j").And.Contain("Take("));

        var parameter = Method(program.Assembly, "F3Parameter").CreateDelegate<Unary>();
        Assert.That(parameter(xs, x => (double)x == 1), Is.EqualTo(new Number[] { 1 }));
        if (loops && !fuse)
            Assert.That(Body(program, "F3Parameter"), Does.Not.Contain("Take(").And.Not.Contain("for (var _j"));
    }

    [TestCase("callee")]
    [TestCase("count")]
    [TestCase("coerced")]
    public static void TakeCallsThatAreNotTheStandardEmptyTakeKeepTheirCopy(string shape)
    {
        var skip = Compilation.Value.LibraryDefinitionsByName["Primitives"].Functions.First(f => f.Name == "Skip");
        var variant = Variant<Faulting>("take-refused-" + shape, "F1FaultTake", FaultingParameters, loop =>
        {
            var lambda = (TirLambda)loop.Fn;
            var conditional = (TirConditional)lambda.Body;
            var take = (TirCall)conditional.IfFalse;
            TirNode arm = shape switch
            {
                // A call named Take whose callee is another function.
                "callee" => new TirCall(skip, take.EmissionKind, take.ParameterTypes, take.ReturnType, take.Args, take.Type, take.Origin, "Take"),
                "count" => take.WithChildren(new[] { take.Args[0], new TirLiteral(1, Ara3D.Geometry.AST.LiteralTypesEnum.Integer, take.Args[1].Type, null) }),
                _ => Wrapped(take),
            };
            return WithCallback(loop, new TirLambda(lambda.Parameters,
                new TirConditional(conditional.Condition, conditional.IfTrue, arm, conditional.Type, conditional.Origin), lambda.Type, lambda.Origin));
        });
        Assert.That(variant.Body, Does.Contain("MakeArray<Number>").And.Contain("for (var _j"));
        Assert.That(variant.Invoke(new Number[] { 1, 2 }, x => (double)x == 1, x => x, x => (Number)((double)x * 10)),
            Is.EqualTo(shape == "count" ? new Number[] { 1, 20 } : new Number[] { 1 }));
    }

    [Test]
    public static void EmptyTakeAllocationIsRemovedAgainstTheSameLoweredPath()
    {
        var optimized = Variant<Select3>("take-allocation-optimized", "F3EmptyArm", Select3Parameters, loop => loop);
        var ordinary = Variant<Select3>("take-allocation-ordinary", "F3EmptyArm", Select3Parameters,
            loop => WithCallback(loop, Wrapped(loop.Fn)));
        Assert.That(optimized.Body, Does.Not.Contain("MakeArray<Number>"));
        Assert.That(ordinary.Body, Does.Contain("MakeArray<Number>"));
        Func<Number, Boolean> dropAll = _ => false;
        Func<Number, Number> identity = x => x;
        foreach (var size in new[] { 8, 64, 512 })
        {
            var input = Enumerable.Range(1, size).Select(x => (Number)x).ToArray();
            foreach (var filter in new[] { optimized.Invoke, ordinary.Invoke })
                for (var i = 0; i < 10000; i++) filter(input, dropAll, identity);
            long Measure(Select3 filter)
            {
                var count = 0;
                var before = GC.GetAllocatedBytesForCurrentThread();
                for (var i = 0; i < 1000; i++)
                    count += filter(input, dropAll, identity).Count;
                var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(count, Is.EqualTo(0));
                return bytes;
            }
            var fast = Measure(optimized.Invoke);
            var slow = Measure(ordinary.Invoke);
            TestContext.WriteLine($"{size},{fast / 1000},{slow / 1000},{(slow - fast) / 1000}");
            Assert.That(slow - fast, Is.GreaterThanOrEqualTo(size * 24L * 1000), "one T[1] per dropped element");
        }
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void NamedFilterAgreesWithTheIdiomItStandsFor(bool loops, bool fuse)
    {
        var program = GetProgram(loops, fuse);
        foreach (var name in new[] { "F4Filter", "F4Lambda", "F4Idiom" })
        {
            var filter = Method(program.Assembly, name).CreateDelegate<Unary>();
            foreach (var input in Inputs)
            foreach (var mode in new[] { "all", "none", "mixed" })
            {
                var trace = new List<string>();
                bool Accept(Number x) => mode == "all" || mode == "mixed" && (double)x <= 2;
                var actual = filter(input, x => { trace.Add($"p{x}"); return Accept(x); });
                Assert.That(actual, Is.EqualTo(input.Where(Accept)), $"{name} {mode}");
                Assert.That(trace, Is.EqualTo(input.Select(x => $"p{x}")), $"{name} {mode}: one call per element, in order");
            }
        }
    }

    /// <summary>Under --fuse a Filter call becomes the idiom its stdlib body states, so the loop lowerer
    /// emits one loop that appends kept elements: no Filter call, no per-element array, no Take.</summary>
    [TestCase("F4Filter")]
    [TestCase("F4Lambda")]
    public static void FilterCallIsOneLoopUnderFuse(string name)
    {
        var body = Body(GetProgram(true, true), name);
        Assert.That(System.Text.RegularExpressions.Regex.Matches(body, @"for \(var _i").Count, Is.EqualTo(1), body);
        Assert.That(body, Does.Contain(".Add(").And.Contain("if ((bool)(").And.Not.Contain(".Filter(")
            .And.Not.Contain("MakeArray").And.Not.Contain(".Take("), body);
    }

    /// <summary>Without --fuse the call stays: the rule is fusion's, not the lowerer's.</summary>
    [Test]
    public static void WithoutFuseTheFilterCallRemains()
        => Assert.That(Body(GetProgram(true, false), "F4Filter"), Does.Contain(".Filter("));
}
