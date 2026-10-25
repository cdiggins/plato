using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace PlatoTests.Optimization;

using Number = Ara3D.Geometry.Number;
using Boolean = Ara3D.Geometry.Boolean;

[TestFixture]
public static class ExactOutputTests
{
    private delegate IReadOnlyList<Number> Probe(IReadOnlyList<Number> xs, Func<Number, Number> value,
        Func<Number, Boolean> keep, Func<Number, IReadOnlyList<Number>> callback);

    private sealed record Variant(Probe Invoke, string Text);
    private static readonly Dictionary<(string, bool), Variant> Programs = new();
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "ExactOutputFixtures", "exact-output.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static (CSharpWriter Writer, TirFunction Tir) Lower(string name, bool enabled, bool loops = true, bool consumers = false)
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-exact-output")
            { ExtensionStyle = true, LowerLoops = loops, ExactOutputAllocation = enabled, FuseConsumers = consumers };
        var definition = Compilation.Value.LibraryDefinitionsByName["ExactOutputProbe"].Functions.Single(f => f.Name == name);
        return (writer, writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null));
    }

    private static Variant Compile(string name, bool enabled, Func<TirLoweredLoop, TirLoweredLoop> change = null)
    {
        if (change == null && Programs.TryGetValue((name, enabled), out var cached)) return cached;
        var (writer, tir) = Lower(name, enabled);
        TirNode Rewrite(TirNode node)
            => node is TirLoweredLoop loop ? change(loop)
                : node.WithChildren(node.Children.Select(Rewrite).ToArray());
        if (change != null)
            tir = new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, Rewrite(tir.Body),
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class ExactProbe { "
            + "public static IReadOnlyList<Number> Run(IReadOnlyList<Number> xs, Func<Number, Number> value, "
            + "Func<Number, Boolean> keep, Func<Number, IReadOnlyList<Number>> callback) " + body + " } }";
        var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source }, LanguageVersion.Default, OptimizationLevel.Release);
        var method = assembly.GetType("Ara3D.Geometry.ExactProbe")!.GetMethod("Run")!;
        var result = new Variant(method.CreateDelegate<Probe>(), source);
        if (change == null) Programs[(name, enabled)] = result;
        return result;
    }

    private static TirLoweredLoop Copy(TirLoweredLoop loop, TirNode callback, FunctionDef sourceFunction)
        => new(loop.Kind, loop.Sources, callback, loop.Seed, loop.IncludeFirst,
            loop.TempName, loop.Id, loop.Type, loop.Origin, sourceFunction)
            { ElemType = loop.ElemType, SourceStages = loop.SourceStages, IndexSource = loop.IndexSource };

    [Test]
    public static void RealTirRetainsCanonicalCallAndExactLiteralThroughCaptureRewriting()
    {
        var (_, tir) = Lower("ExactPairs", true);
        var captured = TirLambdaCaptureRewriter.Rewrite(tir.Body);
        var loop = captured.Descendants().OfType<TirLoweredLoop>().Single();
        Assert.That(TirExactOutput.TryGetWidth(loop, Compilation.Value, out var width), Is.True);
        Assert.That(width, Is.EqualTo(2));
        Assert.That(loop.SourceFunction, Is.SameAs(Compilation.Value.LibraryDefinitionsByName["Intrinsics"].Functions.Single(f => f.Name == "FlatMap")));
        Assert.That(Compile("ExactPairs", false).Text, Does.Not.Contain("System.Array.MaxLength"));
        Assert.That(Compile("ExactPairs", true).Text, Does.Contain("System.Array.MaxLength").And.Contain(" is Number[]"));
    }

    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public static void FlagCombinationsPreserveRequirementForLoopLowering(bool loops, bool consumers)
    {
        var (writer, tir) = Lower("ExactPairs", true, loops, consumers);
        var body = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
        Assert.That(body.Contains("System.Array.MaxLength"), Is.EqualTo(loops));
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class FlagProbe { "
            + "public static IReadOnlyList<Number> Run(IReadOnlyList<Number> xs, Func<Number, Number> value) " + body + " } }";
        var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source }, LanguageVersion.Default, OptimizationLevel.Release);
        var run = assembly.GetType("Ara3D.Geometry.FlagProbe")!.GetMethod("Run")!
            .CreateDelegate<Func<IReadOnlyList<Number>, Func<Number, Number>, IReadOnlyList<Number>>>();
        Assert.That(run(new Number[] { 1, 2 }, x => x), Is.EqualTo(new Number[] { 1, 1, 2, 2 }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public static void ValuesAndLiteralEvaluationOrder(bool enabled)
    {
        var program = Compile("ExactPairs", enabled);
        foreach (var input in new[] { Array.Empty<Number>(), new Number[] { 3, 1, 3 } })
        {
            var trace = new List<Number>();
            var result = program.Invoke(input, x => { trace.Add(x); return x; }, null, null);
            var expected = input.SelectMany(x => new[] { x, x }).ToArray();
            Assert.That(result, Is.EqualTo(expected));
            Assert.That(trace, Is.EqualTo(expected));
            Assert.That(result is Number[], Is.EqualTo(enabled));
        }
        var calls = 0;
        Assert.Throws<InvalidOperationException>(() => program.Invoke(new Number[] { 1, 2 }, x =>
        {
            if (++calls == 2) throw new InvalidOperationException("second literal element");
            return x;
        }, null, null));
        Assert.That(calls, Is.EqualTo(2));
    }

    [TestCase(false)]
    [TestCase(true)]
    public static void SourceMutationAndNonArrayFallbackPreserveReads(bool enabled)
    {
        var program = Compile("ExactPairs", enabled);
        var array = new Number[] { 1, 2 };
        var changed = program.Invoke(array, x => { array[1] = 9; return x; }, null, null);
        Assert.That(changed, Is.EqualTo(new Number[] { 1, 1, 9, 9 }));
        var source = new ObservedList(3);
        var result = program.Invoke(source, x => x, null, null);
        Assert.That(result, Is.EqualTo(new Number[] { 0, 0, 1, 1, 2, 2 }));
        Assert.That(result, Is.TypeOf<List<Number>>());
        Assert.That(source.Reads, Is.EqualTo(new[] { 0, 1, 2 }));
        Assert.That(source.CountReads, Is.EqualTo(1));
        var huge = new ObservedList(int.MaxValue);
        Assert.Throws<ArithmeticException>(() => program.Invoke(huge, _ => throw new ArithmeticException(), null, null));
        Assert.That(huge.Reads, Is.EqualTo(new[] { 0 }));
    }

    [Test]
    public static void EmptyLiteralStillTraversesInput()
    {
        TirLoweredLoop Empty(TirLoweredLoop loop)
        {
            var lambda = (TirLambda)loop.Fn;
            var array = (TirArray)lambda.Body;
            return Copy(loop, new TirLambda(lambda.Parameters, new TirArray(Array.Empty<TirNode>(), array.Type, array.Origin),
                lambda.Type, lambda.Origin), loop.SourceFunction);
        }
        foreach (var enabled in new[] { false, true })
        {
            var source = new ObservedList(3);
            var result = Compile("ExactPairs", enabled, Empty).Invoke(source, x => throw new Exception("unused value"), null, null);
            Assert.That(result, Is.Empty);
            Assert.That(source.Reads, Is.EqualTo(new[] { 0, 1, 2 }));
            var exact = Compile("ExactPairs", enabled, Empty).Invoke(new Number[] { 1, 2 }, x => throw new Exception("unused value"), null, null);
            Assert.That(exact, Is.Empty);
            Assert.That(exact is Number[], Is.EqualTo(enabled));
        }
    }

    [TestCase("ExactVariable")]
    [TestCase("ExactDelegate")]
    public static void VariableAndUnknownWidthsKeepBaseline(string name)
    {
        var program = Compile(name, true);
        Assert.That(program.Text, Does.Not.Contain("System.Array.MaxLength"));
        var result = program.Invoke(new Number[] { 1, 2 }, x => x, x => (double)x == 1, x => new[] { x });
        Assert.That(result, Is.EqualTo(name == "ExactVariable" ? new Number[] { 1, 2, 2 } : new Number[] { 1, 2 }));
    }

    [Test]
    public static void MissingProvenanceAndWrappedCallbackAreRefused()
    {
        var (_, tir) = Lower("ExactPairs", true);
        var loop = tir.AllNodes.OfType<TirLoweredLoop>().Single();
        Assert.That(TirExactOutput.TryGetWidth(Copy(loop, loop.Fn, null), Compilation.Value, out _), Is.False);
        var wrapped = new TirCoerce(loop.Fn, loop.Fn.Type, loop.Fn.Type, null, null);
        Assert.That(TirExactOutput.TryGetWidth(Copy(loop, wrapped, loop.SourceFunction), Compilation.Value, out _), Is.False);
        var lambda = (TirLambda)loop.Fn;
        var wrappedBody = new TirCoerce(lambda.Body, lambda.Body.Type, lambda.Body.Type, null, null);
        var wrappedLambda = new TirLambda(lambda.Parameters, wrappedBody, lambda.Type, lambda.Origin);
        Assert.That(TirExactOutput.TryGetWidth(Copy(loop, wrappedLambda, loop.SourceFunction), Compilation.Value, out _), Is.False);
        var original = loop.SourceFunction;
        var impostor = new FunctionDef(original.Scope, original.Name, original.OwnerType, original.ReturnType, null, original.Parameters.ToArray());
        Assert.That(TirExactOutput.TryGetWidth(Copy(loop, loop.Fn, impostor), Compilation.Value, out _), Is.False);
        var array = (TirArray)lambda.Body;
        var large = new TirArray(Enumerable.Repeat(array.Elements[0], TirExactOutput.MaximumCallbackNodes).ToArray(), array.Type, array.Origin);
        var largeLambda = new TirLambda(lambda.Parameters, large, lambda.Type, lambda.Origin);
        Assert.That(TirExactOutput.TryGetWidth(Copy(loop, largeLambda, loop.SourceFunction), Compilation.Value, out _), Is.False);
        var mismatch = new TypeExpression(lambda.Type.Def, lambda.Type.TypeArgs[0], lambda.Type.TypeArgs[0]);
        var mismatchedLambda = new TirLambda(lambda.Parameters, lambda.Body, mismatch, lambda.Origin);
        Assert.That(TirExactOutput.TryGetWidth(Copy(loop, mismatchedLambda, loop.SourceFunction), Compilation.Value, out _), Is.False);
        var enabled = Compile("ExactPairs", true, l => Copy(l, l.Fn, null));
        Assert.That(enabled.Text, Does.Not.Contain("System.Array.MaxLength"));
        Assert.That(enabled.Invoke(new Number[] { 1 }, x => x, null, null), Is.EqualTo(new Number[] { 1, 1 }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public static void NullSourceAndCallbackFaultsKeepTheirOrder(bool enabled)
    {
        var program = Compile("ExactPairs", enabled);
        var calls = 0;
        Assert.Throws<NullReferenceException>(() => program.Invoke(null, x => { calls++; return x; }, null, null));
        Assert.That(calls, Is.Zero);
        Assert.That(program.Invoke(Array.Empty<Number>(), null, null, null), Is.Empty);
        Assert.Throws<NullReferenceException>(() => program.Invoke(new Number[] { 1 }, null, null, null));
    }

    [Test]
    public static void NewTemporaryNamesAvoidLocalsAndUnusedLambdaParameters()
    {
        var (writer, original) = Lower("ExactPairs", true);
        var loop = original.AllNodes.OfType<TirLoweredLoop>().Single();
        var number = loop.ElemType;
        var literal = new TirLiteral(0.0, Ara3D.Geometry.AST.LiteralTypesEnum.Number, number, null);
        var local = new VariableDef(null, $"_exactOffset{loop.Id}", number, null);
        var let = new TirLet(local, literal, number, null);
        var callbackType = original.Parameters[1].Type;
        var parameter = new ParameterDef(null, $"_exactSize{loop.Id}", number, 0);
        var lambda = new TirLambda(new[] { parameter }, literal, callbackType, null);
        var binding = new TirInlineBinding(new VariableDef(null, "unusedCallback", callbackType, null), lambda);
        var body = new TirBlock(new TirNode[] { let, binding, original.Body }, null);
        var tir = new TirFunction(original.Original, original.Parameters, original.ReturnType, body,
            original.ZonkedParameterTypes, original.ZonkedReturnType);
        var emitted = new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true).ToString();
        Assert.That(emitted, Does.Contain($"_exactOffset{loop.Id}_").And.Contain($"_exactSize{loop.Id}_"));
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class NameProbe { "
            + "public static IReadOnlyList<Number> Run(IReadOnlyList<Number> xs, Func<Number, Number> value) " + emitted + " } }";
        var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source }, LanguageVersion.Default, OptimizationLevel.Release);
        var run = assembly.GetType("Ara3D.Geometry.NameProbe")!.GetMethod("Run")!
            .CreateDelegate<Func<IReadOnlyList<Number>, Func<Number, Number>, IReadOnlyList<Number>>>();
        Assert.That(run(new Number[] { 2 }, x => x), Is.EqualTo(new Number[] { 2, 2 }));
    }

    [Test]
    public static void AllocationDropsOnRealGeneratedWrapperCode()
    {
        var off = Compile("ExactPairs", false).Invoke;
        var on = Compile("ExactPairs", true).Invoke;
        var input = Enumerable.Range(0, 4096).Select(x => (Number)x).ToArray();
        Func<Number, Number> value = x => x;
        for (var i = 0; i < 32; i++) { off(input, value, null, null); on(input, value, null, null); }
        long Measure(Probe action)
        {
            var bytes = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 8; i++)
            {
                var result = action(input, value, null, null);
                if (result.Count != input.Length * 2 || result[result.Count - 1] != input[input.Length - 1])
                    throw new Exception("wrong result");
            }
            return (GC.GetAllocatedBytesForCurrentThread() - bytes) / 8;
        }
        var ordinary = Measure(off);
        var exact = Measure(on);
        TestContext.Progress.WriteLine($"Generated wrapper allocation bytes/call: baseline={ordinary}, exact={exact}");
        Assert.That(exact, Is.LessThan(ordinary));
    }

    private sealed class ObservedList(int count) : IReadOnlyList<Number>
    {
        public readonly List<int> Reads = new();
        public int CountReads;
        public int Count { get { CountReads++; return count; } }
        public Number this[int index] { get { Reads.Add(index); return index; } }
        public IEnumerator<Number> GetEnumerator() => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
