// Checks the scalar-only inlining mode the scalar lowering asks for (ITirInlineHost.InlineScalarBodies).
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// In-proc TIR-shape assertions for scalar-only inlining (plato-brep TKT-1129). The fixture under
/// <c>ScalarBodyInliningFixtures</c> is compiled once; a writer with <c>LowerScalars</c> on (so
/// <see cref="CSharpWriter.InlineScalarBodies"/> is true) and <c>InlineCalls</c> off fetches the
/// ground TIR of each probe and runs <see cref="TirInliner.Inline"/> directly. The emitted C# is
/// not compiled here: the body writer's scalar lowering is a separate change, and the inliner's
/// decision is visible in the TIR and in the <see cref="InlineReport"/>.
/// </summary>
[TestFixture]
public static class ScalarBodyInliningTests
{
    private static readonly System.Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var folder = System.IO.Path.Combine(CheckerTestSupport.FindFolder("PlatoTests"), "Optimization", "ScalarBodyInliningFixtures");
        var compilation = CheckerTestSupport.CompileFolder(folder);
        Assert.That(compilation.SymbolFactory.Errors, Is.Empty,
            string.Join("\n", compilation.SymbolFactory.Errors.Select(e => e.Message)));
        return compilation;
    });

    private static CSharpWriter Writer(bool inlineCalls, bool lowerScalars)
        => new CSharpWriter(Compilation.Value, "unused-scalar-body-inlining")
        {
            ExtensionStyle = true, InlineCalls = inlineCalls, LowerScalars = lowerScalars,
            InlineReport = new InlineReport(),
        };

    private static TirFunction GroundTir(CSharpWriter w, string type, string fn)
    {
        var tir = w.TestGetGroundTir(type, fn);
        Assert.NotNull(tir, $"no ground TIR for {type}.{fn}");
        return tir;
    }

    private static int Calls(TirNode body, string name)
        => body.Descendants().OfType<TirCall>().Count(c => c.Name == name);

    [Test]
    public static void ScalarOnlyModeExpandsAnAllScalarHelper()
    {
        // Entry(a, b) => a.GreaterThanOrEquals(b), and GreaterThanOrEquals(a, b) => b.LessThanOrEquals(a)
        // is all Integer/Boolean: with InlineCalls off and InlineScalarBodies on, the helper
        // expands and only the intrinsic remains.
        var w = Writer(inlineCalls: false, lowerScalars: true);
        Assert.That(w.InlineScalarBodies, Is.True);
        var input = GroundTir(w, "Integer", "Entry");
        var result = TirInliner.Inline(input, w, "Integer", out var inlined);

        Assert.That(inlined, Is.EqualTo(1));
        Assert.That(Calls(result.Body, "GreaterThanOrEquals"), Is.Zero, "the all-scalar helper should be expanded");
        Assert.That(Calls(result.Body, "LessThanOrEquals"), Is.EqualTo(1), "the intrinsic it wraps should remain");
        Assert.That(w.InlineReport.CallsInlined, Is.EqualTo(1));
        Assert.That(w.InlineReport.RefusalCount(InlineRefusal.NotScalarBody), Is.Zero);
    }

    [Test]
    public static void ScalarOnlyModeRefusesANonScalarParameter()
    {
        // PairEntry(p) => p.Sum, with Sum(p: Pair): Integer: the parameter is a two-field struct,
        // so scalar-only mode leaves the call and records NotScalarBody.
        var w = Writer(inlineCalls: false, lowerScalars: true);
        var input = GroundTir(w, "Pair", "PairEntry");
        var result = TirInliner.Inline(input, w, "Pair", out var inlined);

        Assert.That(inlined, Is.Zero);
        Assert.That(ReferenceEquals(result, input), Is.True, "an untouched body is returned as is");
        Assert.That(Calls(result.Body, "Sum"), Is.EqualTo(1));
        Assert.That(w.InlineReport.RefusalCount("PairEntry", "Sum", InlineRefusal.NotScalarBody), Is.EqualTo(1));
    }

    [Test]
    public static void ScalarOnlyModeRefusesANonScalarReturn()
    {
        // ReturnEntry(a) => a.PairOf, with PairOf(a: Integer): Pair: the parameter is scalar but
        // the return is a struct, so scalar-only mode refuses with NotScalarBody.
        var w = Writer(inlineCalls: false, lowerScalars: true);
        var input = GroundTir(w, "Integer", "ReturnEntry");
        var result = TirInliner.Inline(input, w, "Integer", out var inlined);

        Assert.That(inlined, Is.Zero);
        Assert.That(Calls(result.Body, "PairOf"), Is.EqualTo(1));
        Assert.That(w.InlineReport.RefusalCount("ReturnEntry", "PairOf", InlineRefusal.NotScalarBody), Is.EqualTo(1));
    }

    [Test]
    public static void GeneralModeIsNotGatedByScalarSignatures()
    {
        // With InlineCalls on, the struct-parameter helper is subject only to the ordinary rules:
        // NotScalarBody is never recorded, whatever LowerScalars says.
        var w = Writer(inlineCalls: true, lowerScalars: true);
        TirInliner.Inline(GroundTir(w, "Pair", "PairEntry"), w, "Pair", out _);
        TirInliner.Inline(GroundTir(w, "Integer", "ReturnEntry"), w, "Integer", out _);
        TirInliner.Inline(GroundTir(w, "Integer", "Entry"), w, "Integer", out var scalarInlined);

        Assert.That(w.InlineReport.RefusalCount(InlineRefusal.NotScalarBody), Is.Zero);
        Assert.That(scalarInlined, Is.EqualTo(1));
    }

    [Test]
    public static void BothSwitchesOffInlineNothing()
    {
        // Neither InlineCalls nor InlineScalarBodies: the inliner returns its input unchanged and
        // records nothing.
        var w = Writer(inlineCalls: false, lowerScalars: false);
        Assert.That(w.InlineScalarBodies, Is.False);
        foreach (var (type, fn) in new[] { ("Integer", "Entry"), ("Pair", "PairEntry"), ("Integer", "ReturnEntry") })
        {
            var input = GroundTir(w, type, fn);
            var result = TirInliner.Inline(input, w, type, out var inlined);
            Assert.That(inlined, Is.Zero, $"{type}.{fn}");
            Assert.That(ReferenceEquals(result, input), Is.True, $"{type}.{fn} should be returned as is");
        }
        Assert.That(w.InlineReport.CallsInlined, Is.Zero);
        Assert.That(w.InlineReport.ToTable(), Does.Not.Contain("NotScalarBody"));
    }
}
