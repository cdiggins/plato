// Checks that --lower-more lowers MakeArray2D, MakeArray3D, CartesianProduct and Filter call sites to
// loops with the callback inline, computes the same values in the same order, and refuses a call whose
// element type is a type variable.
using System.Reflection;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace PlatoTests.Optimization;

using Number = Ara3D.Geometry.Number;
using Boolean = Ara3D.Geometry.Boolean;
using Integer = Ara3D.Geometry.Integer;

[TestFixture]
public static class LowerMoreTests
{
    // The library methods the probes call when a site is not lowered, plus the probes themselves.
    private static readonly string[] Kept =
    {
        "MakeArray2D", "MakeArray3D", "CartesianProduct", "Filter", "Take",
        "Grid", "Volume", "Product", "Kept", "Hoisted", "GenericGrid",
    };

    // The grid records as the runtime declares them (fields, constructor, tuple conversion); the
    // generated _Array2D.g.cs needs the whole concept-interface surface, which is not under test.
    private const string Grids = @"
namespace Ara3D.Geometry
{
    public readonly struct Array2D<T>
    {
        public readonly IReadOnlyList<T> Elements; public readonly Integer ColumnCount; public readonly Integer RowCount;
        public Array2D(IReadOnlyList<T> elements, Integer columnCount, Integer rowCount) { Elements = elements; ColumnCount = columnCount; RowCount = rowCount; }
        public static implicit operator Array2D<T>((IReadOnlyList<T>, Integer, Integer) t) => new(t.Item1, t.Item2, t.Item3);
    }
    public readonly struct Array3D<T>
    {
        public readonly IReadOnlyList<T> Elements; public readonly Integer ColumnCount; public readonly Integer RowCount; public readonly Integer LayerCount;
        public Array3D(IReadOnlyList<T> elements, Integer columnCount, Integer rowCount, Integer layerCount) { Elements = elements; ColumnCount = columnCount; RowCount = rowCount; LayerCount = layerCount; }
        public static implicit operator Array3D<T>((IReadOnlyList<T>, Integer, Integer, Integer) t) => new(t.Item1, t.Item2, t.Item3, t.Item4);
    }
    // The derived Integer surface the stdlib's own MakeArray2D/3D bodies call (generated into _Integer.g.cs, not under test).
    public static class IntegerSurface
    {
        public static Integer Multiply(this Integer a, Integer b) => (Integer)((int)a * (int)b);
        public static Integer Divide(this Integer a, Integer b) => (Integer)((int)a / (int)b);
        public static Integer Modulo(this Integer a, Integer b) => (Integer)((int)a % (int)b);
        public static Number Multiply(this Number a, Number b) => (Number)((double)a * (double)b);
    }
}";

    private sealed record Program(Assembly Assembly, string Source);
    private static readonly Dictionary<(bool, bool), Program> Programs = new();

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "LowerMoreFixtures", "lowermore.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static string KeepMethods(string source)
    {
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();
        return root.RemoveNodes(root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(m => !Kept.Contains(m.Identifier.Text)), SyntaxRemoveOptions.KeepNoTrivia)!.ToFullString();
    }

    private static Program Compile(bool enabled, bool hoist = false)
    {
        if (Programs.TryGetValue((enabled, hoist), out var cached))
            return cached;
        var writer = new CSharpWriter(Compilation.Value, "unused-lower-more")
            { ExtensionStyle = true, LowerLoops = true, LowerMore = enabled, HoistInvariants = hoist };
        writer.WriteAll();
        // The Primitives library's generic functions land in Primitives.g.cs and Extensions.g.cs, the
        // probes in the file named after their library.
        var sources = new[] { "global using System; global using System.Collections.Generic;", Grids }
            .Concat(new[] { "Primitives.g.cs", "Extensions.g.cs", "LowerMoreProbe.g.cs" }.Select(f => KeepMethods(writer.Files[f].ToString()))).ToArray();
        var assembly = CSharpTestCompilation.CompileAndLoad(sources, LanguageVersion.CSharp12, OptimizationLevel.Release);
        return Programs[(enabled, hoist)] = new Program(assembly, string.Join("\n", sources.Skip(2)));
    }

    private static object Run(bool enabled, string name, params object[] arguments)
        => Run(enabled, false, name, arguments);

    private static object Run(bool enabled, bool hoist, string name, params object[] arguments)
    {
        try
        {
            return Compile(enabled, hoist).Assembly.GetTypes().SelectMany(t => t.GetMethods()).Single(m => m.Name == name)
                .Invoke(null, arguments)!;
        }
        catch (TargetInvocationException e) when (e.InnerException != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }
    }

    private static string Method(bool enabled, string name, bool hoist = false)
    {
        var source = Compile(enabled, hoist).Source;
        var start = source.IndexOf(" " + name + "(", StringComparison.Ordinal);
        if (start < 0)
            start = source.IndexOf(" " + name + "<", StringComparison.Ordinal); // a generic probe
        var next = source.IndexOf("public static", start, StringComparison.Ordinal);
        return next < 0 ? source[start..] : source[start..next];
    }

    private static IReadOnlyList<Number> Numbers(params double[] values) => values.Select(v => (Number)v).ToArray();

    private static long[] Bits(IEnumerable<Number> list) => list.Select(n => BitConverter.DoubleToInt64Bits((double)n)).ToArray();

    // A grid record's parts, whichever assembly declared the record.
    private static (long[] Elements, int[] Extents) Parts(object grid)
    {
        var type = grid.GetType();
        var elements = (IReadOnlyList<Number>)type.GetField("Elements")!.GetValue(grid)!;
        var extents = new[] { "ColumnCount", "RowCount", "LayerCount" }
            .Select(f => type.GetField(f)).Where(f => f != null).Select(f => (int)(Integer)f!.GetValue(grid)!).ToArray();
        return (Bits(elements), extents);
    }

    // The body (after the signature, whose delegate parameters are Func types) declares no lambda and
    // keeps no FlatMap / Take(0) idiom; the probe's own delegate parameters are still called.
    private static void AssertNoLambda(string method)
        => Assert.That(method[method.IndexOf('{')..], Does.Not.Contain("=>").And.Not.Contain("Func<")
            .And.Not.Contain("FlatMap").And.Not.Contain("Take("));

    private static readonly (int Cols, int Rows)[] Extents2D = { (0, 0), (0, 3), (3, 0), (1, 1), (3, 2), (2, 5), (-1, 2), (2, -1) };

    [Test]
    public static void MakeArray2DIsNestedLoopsOverOnePresizedArray()
    {
        var on = Method(true, "Grid");
        Assert.That(Method(false, "Grid"), Does.Contain(".MakeArray2D("));
        Assert.That(on, Does.Not.Contain(".MakeArray2D(").And.Contain("new Array2D<Number>(").And.Contain("_cols").And.Contain("_rows"));
        AssertNoLambda(on);
        foreach (var (cols, rows) in Extents2D)
        {
            var calls = new List<(int, int)>[2];
            Func<Integer, Integer, Number> F(int which) => (c, r) => { calls[which].Add(((int)c, (int)r)); return (double)(int)c * 10 + (double)(int)r; };
            calls[0] = new(); calls[1] = new();
            var off = Parts(Run(false, "Grid", (Integer)cols, (Integer)rows, F(0)));
            var lowered = Parts(Run(true, "Grid", (Integer)cols, (Integer)rows, F(1)));
            Assert.That(lowered.Elements, Is.EqualTo(off.Elements), $"{cols}x{rows}");
            Assert.That(lowered.Extents, Is.EqualTo(off.Extents).And.EqualTo(new[] { cols, rows }));
            Assert.That(calls[1], Is.EqualTo(calls[0]), $"call order {cols}x{rows}");
            Assert.That(calls[1].Count, Is.EqualTo(Math.Max(0, cols) * Math.Max(0, rows)));
        }
    }

    [Test]
    public static void MakeArray3DIsThreeNestedLoops()
    {
        var on = Method(true, "Volume");
        Assert.That(Method(false, "Volume"), Does.Contain(".MakeArray3D("));
        Assert.That(on, Does.Not.Contain(".MakeArray3D(").And.Contain("new Array3D<Number>(").And.Contain("_layers"));
        AssertNoLambda(on);
        foreach (var (cols, rows, layers) in new[] { (0, 0, 0), (2, 3, 0), (1, 1, 1), (3, 2, 4), (2, -1, 3) })
        {
            var calls = new List<(int, int, int)>[2];
            Func<Integer, Integer, Integer, Number> F(int which) => (c, r, l) => { calls[which].Add(((int)c, (int)r, (int)l)); return (double)(int)c + 10.0 * (int)r + 100.0 * (int)l; };
            calls[0] = new(); calls[1] = new();
            var off = Parts(Run(false, "Volume", (Integer)cols, (Integer)rows, (Integer)layers, F(0)));
            var lowered = Parts(Run(true, "Volume", (Integer)cols, (Integer)rows, (Integer)layers, F(1)));
            Assert.That(lowered.Elements, Is.EqualTo(off.Elements), $"{cols}x{rows}x{layers}");
            Assert.That(lowered.Extents, Is.EqualTo(new[] { cols, rows, layers }));
            Assert.That(calls[1], Is.EqualTo(calls[0]));
        }
    }

    [Test]
    public static void CartesianProductIsNestedLoopsOverBothLists()
    {
        var on = Method(true, "Product");
        Assert.That(Method(false, "Product"), Does.Contain(".CartesianProduct("));
        Assert.That(on, Does.Not.Contain(".CartesianProduct(").And.Contain("new Array2D<Number>("));
        AssertNoLambda(on);
        foreach (var (xs, ys) in new[] { (Numbers(), Numbers()), (Numbers(1, 2), Numbers()), (Numbers(1, 2, 3), Numbers(10, 20)), (Numbers(-0.0), Numbers(0.0, double.NaN)) })
        {
            var calls = new List<(double, double)>[2];
            Func<Number, Number, Number> F(int which) => (x, y) => { calls[which].Add(((double)x, (double)y)); return (double)x + (double)y; };
            calls[0] = new(); calls[1] = new();
            var off = Parts(Run(false, "Product", xs, ys, F(0)));
            var lowered = Parts(Run(true, "Product", xs, ys, F(1)));
            Assert.That(lowered.Elements, Is.EqualTo(off.Elements));
            Assert.That(lowered.Extents, Is.EqualTo(new[] { xs.Count, ys.Count }));
            Assert.That(calls[1], Is.EqualTo(calls[0]));
        }
    }

    [Test]
    public static void FilterIsAPresizedLoopTrimmedOnce()
    {
        var on = Method(true, "Kept");
        Assert.That(Method(false, "Kept"), Does.Contain(".Filter("));
        Assert.That(on, Does.Not.Contain(".Filter(").And.Contain("System.Array.Resize("));
        AssertNoLambda(on);
        var inputs = new[] { Numbers(), Numbers(1.5), Numbers(-0.0, 0.0, double.NaN, 3, -7.25, double.PositiveInfinity) };
        var keeps = new Func<Number, Boolean>[] { _ => true, _ => false, x => (double)x > 0, x => double.IsNaN((double)x) || (double)x < 0 };
        foreach (var input in inputs)
        foreach (var keep in keeps)
        {
            var off = (IReadOnlyList<Number>)Run(false, "Kept", input, keep);
            var lowered = (IReadOnlyList<Number>)Run(true, "Kept", input, keep);
            Assert.That(Bits(lowered), Is.EqualTo(Bits(off)));
            Assert.That(lowered.Count, Is.EqualTo(input.Count(x => (bool)keep(x))));
        }
    }

    [Test]
    public static void InvariantInAGridCallbackHoistsAheadOfTheLoopNestAndNeverRunsOnAnEmptyGrid()
    {
        Assert.That(Method(true, "Hoisted"), Does.Not.Contain("_inv"));
        var hoisted = Method(true, "Hoisted", hoist: true);
        Assert.That(hoisted, Does.Contain("_inv").And.Contain("> 0)"));
        AssertNoLambda(hoisted);
        foreach (var (cols, rows) in Extents2D)
        {
            var counts = new int[2];
            Func<Number, Number> Value(int which) => x => { counts[which]++; return (double)x * 0.5; };
            Func<Integer, Integer, Number> f = (c, r) => (double)(int)c - (double)(int)r;
            var off = Parts(Run(true, false, "Hoisted", (Integer)cols, (Integer)rows, (Number)3.0, Value(0), f));
            var on = Parts(Run(true, true, "Hoisted", (Integer)cols, (Integer)rows, (Number)3.0, Value(1), f));
            Assert.That(on.Elements, Is.EqualTo(off.Elements));
            var cells = Math.Max(0, cols) * Math.Max(0, rows);
            Assert.That(counts[0], Is.EqualTo(cells));
            Assert.That(counts[1], Is.EqualTo(cells == 0 ? 0 : 1));
        }
        Func<Number, Number> fault = _ => throw new InvalidOperationException("hoisted on an empty grid");
        Run(true, true, "Hoisted", (Integer)0, (Integer)4, (Number)1.0, fault, (Func<Integer, Integer, Number>)((c, r) => 0.0));
        Assert.Throws<InvalidOperationException>(() =>
            Run(true, true, "Hoisted", (Integer)1, (Integer)1, (Number)1.0, fault, (Func<Integer, Integer, Number>)((c, r) => 0.0)));
    }

    [Test]
    public static void TypeVariableElementIsRefused()
    {
        // The generic probe's element type is $T: `new T[n]` has no C# spelling, so the call stays a call.
        Assert.That(Method(true, "GenericGrid"), Does.Contain(".MakeArray2D("));
        Assert.That(Method(true, "GenericGrid"), Is.EqualTo(Method(false, "GenericGrid")));
    }

    [Test]
    public static void SwitchOffLeavesEveryProbeAsBefore()
    {
        var writer = new CSharpWriter(Compilation.Value, "unused-lower-more-off") { ExtensionStyle = true, LowerLoops = true };
        writer.WriteAll();
        var text = KeepMethods(writer.Files["LowerMoreProbe.g.cs"].ToString()) + KeepMethods(writer.Files["Extensions.g.cs"].ToString());
        Assert.That(text, Does.Contain(".MakeArray2D(").And.Contain(".MakeArray3D(").And.Contain(".CartesianProduct(").And.Contain(".Filter("));
        Assert.That(text, Does.Not.Contain("_cols").And.Not.Contain("System.Array.Resize("));
    }
}
