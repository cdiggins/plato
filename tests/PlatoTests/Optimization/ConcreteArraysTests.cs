// Pins --concrete-arrays (TKT-1093): off emits the interface reads, on routes them through ConcreteArrayAccess.
using System.Reflection;
using Ara3D.Geometry.CSharpWriter;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace PlatoTests.Optimization;

using Number = Ara3D.Geometry.Number;
using Integer = Ara3D.Geometry.Integer;

[TestFixture]
public static class ConcreteArraysTests
{
    private static readonly Dictionary<string, string> Signatures = new()
    {
        ["Size"] = "Integer Size(IReadOnlyList<Number> xs)",
        ["Item"] = "Number Item(IReadOnlyList<Number> xs, Integer i)",
        ["Mapped"] = "IReadOnlyList<Number> Mapped(IReadOnlyList<Number> xs, Func<Number, Number> f)",
        ["Total"] = "Number Total(IReadOnlyList<Number> xs, Func<Number, Number, Number> f)",
    };
    private sealed record Program(CSharpWriter Writer, Assembly Assembly, string Source);
    private static readonly Dictionary<bool, Program> Programs = new();
    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        var fixture = Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "ConcreteArraysFixtures", "concrete-arrays.plato");
        var asts = Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile)
            .Append(CheckerTestSupport.ParseFile(fixture));
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null, asts.ToList());
    });

    private static Program Get(bool enabled)
    {
        if (Programs.TryGetValue(enabled, out var cached))
            return cached;
        var writer = new CSharpWriter(Compilation.Value, "unused-concrete-arrays")
            { ExtensionStyle = true, LowerLoops = true, ConcreteArrays = enabled };
        var methods = Compilation.Value.LibraryDefinitionsByName["ConcreteArraysProbe"].Functions
            .Select(f =>
            {
                var tir = writer.RunOptimizerPasses(writer.TryGetStaticTir(f), null);
                return $"public static {Signatures[f.Name]} "
                    + new TirCSharpBodyWriter(new CSharpTypeWriter(writer, null), tir, isStatic: true);
            });
        var source = "global using System; global using System.Collections.Generic; using System.Runtime.CompilerServices; "
            + "using static System.Runtime.CompilerServices.MethodImplOptions; namespace Ara3D.Geometry { public static class ConcreteProbe { "
            + string.Join("\n", methods) + " } }";
        var sources = new List<string> { source };
        if (enabled)
            sources.Add(HelperFile(true)!);
        var assembly = CSharpTestCompilation.CompileAndLoad(sources, LanguageVersion.Default, OptimizationLevel.Release);
        return Programs[enabled] = new Program(writer, assembly, source);
    }

    private static readonly Dictionary<bool, string?> Helpers = new();

    /// <summary>The helper file a whole-program write emits, or null when it emits none.</summary>
    private static string? HelperFile(bool enabled)
    {
        if (Helpers.TryGetValue(enabled, out var text))
            return text;
        var writer = new CSharpWriter(Compilation.Value, "unused-concrete-arrays")
            { ExtensionStyle = true, LowerLoops = true, ConcreteArrays = enabled };
        writer.WriteAll();
        Assert.That(writer.Files.Values.Any(f => f.ToString().Contains("Item(this IReadOnlyList<Number> xs, Integer i)")), Is.True,
            "the fixture must be emitted");
        text = writer.Files.TryGetValue($"{CSharpWriter.ConcreteArrayAccessClass}.g.cs", out var file) ? file.ToString() : null;
        if (!enabled)
            Assert.That(writer.Files.Values.Any(f => f.ToString().Contains(CSharpWriter.ConcreteArrayAccessClass)), Is.False,
                "with the switch off no file names the helper");
        return Helpers[enabled] = text;
    }

    private static object? Run(bool enabled, string name, params object[] args)
    {
        try
        {
            return Get(enabled).Assembly.GetType("Ara3D.Geometry.ConcreteProbe")!.GetMethod(name)!.Invoke(null, args);
        }
        catch (TargetInvocationException e)
        {
            return e.InnerException!.GetType();
        }
    }

    [Test]
    public static void OffEmitsNoHelperAndInterfaceReads()
    {
        Assert.That(HelperFile(false), Is.Null);
        var off = Get(false).Source;
        Assert.That(off, Does.Not.Contain(CSharpWriter.ConcreteArrayAccessClass));
        Assert.That(off, Does.Contain("xs.Count").And.Contain("xs[i]"));
    }

    [Test]
    public static void OnRoutesCountsAndReadsThroughTheHelper()
    {
        var helper = HelperFile(true);
        Assert.That(helper, Does.Contain("public static class ConcreteArrayAccess")
            .And.Contain("xs.GetType() == typeof(T[])").And.Contain("(uint)i < (uint)a.Length"));
        var on = Get(true).Source;
        Assert.That(on, Does.Contain("ConcreteArrayAccess.Count(xs)").And.Contain("ConcreteArrayAccess.At(xs, i)"));
        // The lowered Map and Reduce loops read their source through the helper too.
        Assert.That(on, Does.Contain("for ("));
        Assert.That(on, Does.Contain("ConcreteArrayAccess.Count(_s").And.Contain("ConcreteArrayAccess.At(_s"));
        Assert.That(on, Does.Not.Contain("xs.Count").And.Not.Contain("xs[i]"));
    }

    private static IEnumerable<IReadOnlyList<Number>> Inputs()
    {
        Number[] values = { 1.5, -0.0, 3.25, double.NaN };
        yield return values;
        yield return values.ToList();
        yield return Array.Empty<Number>();
        yield return new List<Number>().AsReadOnly();
    }

    private static string Show(object? value)
        => value switch
        {
            IReadOnlyList<Number> list => "[" + string.Join(",", list.Select(n => BitConverter.DoubleToInt64Bits((double)n))) + "]",
            Number n => BitConverter.DoubleToInt64Bits((double)n).ToString(),
            _ => value?.ToString() ?? "null"
        };

    [Test]
    public static void OnAndOffAgreeOnArraysOtherListsAndBadIndices()
    {
        foreach (var xs in Inputs())
        {
            var kind = xs.GetType().Name;
            Func<Number, Number> twice = x => (Number)((double)x * 2);
            Func<Number, Number, Number> add = (a, x) => (Number)((double)a + (double)x);
            Assert.That(Show(Run(true, "Size", xs)), Is.EqualTo(Show(Run(false, "Size", xs))), $"Size on {kind}");
            Assert.That(Show(Run(true, "Mapped", xs, twice)), Is.EqualTo(Show(Run(false, "Mapped", xs, twice))), $"Mapped on {kind}");
            Assert.That(Show(Run(true, "Total", xs, add)), Is.EqualTo(Show(Run(false, "Total", xs, add))), $"Total on {kind}");
            foreach (var i in new[] { -1, 0, 2, 3, 4 })
                Assert.That(Show(Run(true, "Item", xs, (Integer)i)), Is.EqualTo(Show(Run(false, "Item", xs, (Integer)i))),
                    $"Item({i}) on {kind}");
        }
        // An index out of range still throws what the interface throws.
        Assert.That(Run(true, "Item", new Number[] { 1.0 }, (Integer)1), Is.EqualTo(typeof(ArgumentOutOfRangeException)));
    }
}
