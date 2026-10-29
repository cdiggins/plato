// Checks that --hoist-invariants moves only unconditional, element-free callback work out of lowered loops.
using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace PlatoTests.Optimization;

using Number = Ara3D.Geometry.Number;
using Boolean = Ara3D.Geometry.Boolean;
using Integer = Ara3D.Geometry.Integer;

[TestFixture]
public static class HoistInvariantsTests
{
    private const string Delegates = "Func<Number, Number> value, Func<Number, Number, Number> f";

    private static readonly Dictionary<string, string> Signatures = new()
    {
        ["Invariant"] = $"IReadOnlyList<Number> Invariant(IReadOnlyList<Number> xs, Number a, {Delegates})",
        ["Guarded"] = "IReadOnlyList<Number> Guarded(IReadOnlyList<Number> xs, IReadOnlyList<Number> ys, Integer k, Number a, "
            + "Func<Number, Number> value, Func<Number, Boolean> keep)",
        ["Dependent"] = $"IReadOnlyList<Number> Dependent(IReadOnlyList<Number> xs, Number a, {Delegates})",
        ["Scaled"] = $"Number Scaled(this Number x, Number a, {Delegates})",
        ["Exposed"] = $"IReadOnlyList<Number> Exposed(IReadOnlyList<Number> xs, Number a, {Delegates})",
    };

    private sealed record Program(Assembly Assembly, string Source, CSharpWriter Writer);
    private static readonly Dictionary<bool, Program> Programs = new();

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "HoistInvariantsFixtures", "hoist.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static TirFunction Lower(CSharpWriter writer, string name)
    {
        var definition = Compilation.Value.LibraryDefinitionsByName["HoistProbe"].Functions.Single(f => f.Name == name);
        return writer.RunOptimizerPasses(writer.TryGetStaticTir(definition), null);
    }

    private static Program Compile(bool enabled)
    {
        if (Programs.TryGetValue(enabled, out var cached))
            return cached;
        var writer = new CSharpWriter(Compilation.Value, "unused-hoist-invariants")
            { ExtensionStyle = true, LowerLoops = true, HoistInvariants = enabled };
        var methods = Signatures.Select(kv => "public static " + kv.Value + " "
            + new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), Lower(writer, kv.Key), isStatic: true));
        var source = "global using System; global using System.Collections.Generic; namespace Ara3D.Geometry { public static class HoistProbe { "
            + string.Join("\n", methods) + " } }";
        var assembly = CSharpTestCompilation.CompileAndLoad(new[] { source }, LanguageVersion.Default, OptimizationLevel.Release);
        return Programs[enabled] = new Program(assembly, source, writer);
    }

    private static object Run(bool enabled, string name, params object[] arguments)
    {
        try
        {
            return Compile(enabled).Assembly.GetType("Ara3D.Geometry.HoistProbe")!.GetMethod(name)!.Invoke(null, arguments);
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static string Method(bool enabled, string name)
    {
        var source = Compile(enabled).Source;
        var start = source.IndexOf(" " + name + "(", StringComparison.Ordinal);
        var next = source.IndexOf("public static", start, StringComparison.Ordinal);
        return next < 0 ? source[start..] : source[start..next];
    }

    // A method's text with the run-wide name counters (_var{N}, loop ids) taken out.
    private static string Shape(bool enabled, string name)
        => System.Text.RegularExpressions.Regex.Replace(Method(enabled, name), @"\d+", "#");

    private static long[] Bits(object list)
        => ((IReadOnlyList<Number>)list).Select(n => BitConverter.DoubleToInt64Bits((double)n)).ToArray();

    private static IReadOnlyList<Number> Numbers(params double[] values) => values.Select(v => (Number)v).ToArray();

    private static readonly double[][] Inputs =
    {
        System.Array.Empty<double>(), new[] { 1.5 }, new[] { -0.0, 0.0, double.NaN, double.PositiveInfinity, 1e-300, -7.25 },
    };

    [Test]
    public static void InvariantDelegateCallMovesAheadOfTheLoopAndRunsOncePerLoop()
    {
        Assert.That(Method(false, "Invariant"), Does.Not.Contain("_inv"));
        Assert.That(Method(true, "Invariant"), Does.Contain("_inv").And.Contain("> 0)"));
        foreach (var input in Inputs)
        foreach (var a in new[] { 0.0, -0.0, 3.0, double.NaN })
        {
            var counts = new int[2];
            Number Value(int which, Number x) { counts[which]++; return ((double)x * 0.5 + 1.0); }
            Number F(Number x, Number y) => (double)x * (double)y - 2.0;
            var off = Run(false, "Invariant", Numbers(input), (Number)a, (Func<Number, Number>)(x => Value(0, x)), (Func<Number, Number, Number>)F);
            var on = Run(true, "Invariant", Numbers(input), (Number)a, (Func<Number, Number>)(x => Value(1, x)), (Func<Number, Number, Number>)F);
            Assert.That(Bits(on), Is.EqualTo(Bits(off)));
            Assert.That(counts[0], Is.EqualTo(input.Length));
            Assert.That(counts[1], Is.EqualTo(input.Length == 0 ? 0 : 1));
        }
    }

    [Test]
    public static void ZeroTripLoopEvaluatesNoHoistedExpression()
    {
        Func<Number, Number> fault = _ => throw new InvalidOperationException("hoisted on a zero-trip loop");
        var result = Run(true, "Invariant", Numbers(), (Number)1.0, fault, (Func<Number, Number, Number>)((x, y) => x));
        Assert.That((IReadOnlyList<Number>)result, Is.Empty);
        Assert.Throws<InvalidOperationException>(() =>
            Run(true, "Invariant", Numbers(2.0), (Number)1.0, fault, (Func<Number, Number, Number>)((x, y) => x)));
    }

    [Test]
    public static void GuardedExpressionsStayUnderTheirCondition()
    {
        Assert.That(Method(true, "Guarded"), Does.Not.Contain("_inv"));
        Assert.That(Shape(true, "Guarded"), Is.EqualTo(Shape(false, "Guarded")));
        foreach (var enabled in new[] { false, true })
        {
            var calls = 0;
            Func<Number, Number> value = x => { calls++; return x; };
            // ys is empty, so ys.At(k) faults if it runs; keep is false for every element, so it never does.
            var result = Run(enabled, "Guarded", Numbers(1, 2, 3), Numbers(), (Integer)0, (Number)4.0, value,
                (Func<Number, Boolean>)(_ => false));
            Assert.That(Bits(result), Is.EqualTo(Bits(Numbers(4, 4, 4))));
            Assert.That(calls, Is.EqualTo(3));
            // keep is true for every element, so value(a) never runs.
            calls = 0;
            Run(enabled, "Guarded", Numbers(1, 2), Numbers(9), (Integer)0, (Number)4.0, value, (Func<Number, Boolean>)(_ => true));
            Assert.That(calls, Is.EqualTo(0));
        }
    }

    [Test]
    public static void ElementDependentExpressionStaysInTheLoop()
    {
        Assert.That(Method(true, "Dependent"), Does.Not.Contain("_inv"));
        Assert.That(Shape(true, "Dependent"), Is.EqualTo(Shape(false, "Dependent")));
        var calls = 0;
        var result = Run(true, "Dependent", Numbers(1, 2, 3), (Number)0.25,
            (Func<Number, Number>)(x => { calls++; return (double)x * 2.0; }), (Func<Number, Number, Number>)((x, y) => (double)x + (double)y));
        Assert.That(Bits(result), Is.EqualTo(Bits(Numbers(2.5, 4.5, 6.5))));
        Assert.That(calls, Is.EqualTo(3));
    }

    [Test]
    public static void ForwardingCalleeExpandsOnlyToExposeAnInvariant()
    {
        Assert.That(Method(false, "Exposed"), Does.Contain(".Scaled(").And.Not.Contain("_inv"));
        Assert.That(Method(true, "Exposed"), Does.Contain("_inv").And.Not.Contain(".Scaled("));
        Assert.That(Compile(true).Writer.HoistExposingInlines, Is.GreaterThan(0));
        foreach (var input in Inputs)
        {
            var calls = 0;
            Func<Number, Number> value = x => { calls++; return (double)x - 1.0; };
            Func<Number, Number, Number> f = (x, y) => (double)x / (double)y;
            var off = Run(false, "Exposed", Numbers(input), (Number)3.0, value, f);
            var expected = calls;
            calls = 0;
            var on = Run(true, "Exposed", Numbers(input), (Number)3.0, value, f);
            Assert.That(Bits(on), Is.EqualTo(Bits(off)));
            Assert.That(expected, Is.EqualTo(input.Length));
            Assert.That(calls, Is.EqualTo(input.Length == 0 ? 0 : 1));
        }
    }
}
