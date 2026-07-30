using System.Diagnostics;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace Ara3D.Geometry.Navigation.Tests;

/// <summary>The measurement the plato_check plan (docs/plato-mcp-improvements-plan-2026-07-30.md,
/// P1 "first task") demands before any tool code: how long does a WARM check iteration cost on the
/// full forward stdlib — Compilation from cached ASTs, Linter, TypeChecker.CheckAll, plus the two
/// auxiliary checkers. Explicit because it is a benchmark, not a gate: it prints numbers and
/// asserts only that the corpus compiled.</summary>
[TestFixture]
public static class CheckTimingTests
{
    [Test, Explicit("Benchmark, not a gate. Run by name to (re)measure warm-check latency.")]
    public static void MeasureWarmCheckOnForwardStdLib()
    {
        var stdlib = Corpus.RepoRoot.RelativeFolder("stdlib");
        if (!stdlib.Exists())
            Assert.Ignore("No stdlib folder above the test binary.");

        var cache = new ParseCache();
        var snapshot = SourceSnapshot.FromDirectories(new[] { stdlib });
        TestContext.WriteLine($"files: {snapshot.Files.Count}");

        // Cold: every file goes through the parser once, then the full front-end.
        var timer = Stopwatch.StartNew();
        var parsed = CheckSupport.Parse(snapshot, cache);
        var coldParse = timer.ElapsedMilliseconds;
        TestContext.WriteLine($"cold parse: {coldParse} ms ({cache.Misses} parsed)");

        Measure("cold", snapshot, cache);

        // Warm: the iteration an agent actually pays after editing zero files — all parse hits.
        Measure("warm", snapshot, cache);

        // Warm after a one-file edit: reparse just that file, then the full front-end again.
        var first = snapshot.Files[0];
        var edited = snapshot.Files
            .Select(f => f == first ? new SourceFile(f.Id, f.Path, f.Text + "\n") : f)
            .ToList();
        Measure("warm+1 edit", new SourceSnapshot(edited), cache);
    }

    private static void Measure(string label, SourceSnapshot snapshot, ParseCache cache)
    {
        var timer = Stopwatch.StartNew();
        var (hits, misses) = (cache.Hits, cache.Misses);
        var parsed = CheckSupport.Parse(snapshot, cache);
        var parseMs = timer.ElapsedMilliseconds;

        timer.Restart();
        var comp = CheckSupport.Compile(parsed);
        var compileMs = timer.ElapsedMilliseconds;

        timer.Restart();
        var lint = new Linter(comp);
        var lintMs = timer.ElapsedMilliseconds;

        timer.Restart();
        var results = new TypeChecker(comp).CheckAll();
        var checkMs = timer.ElapsedMilliseconds;

        timer.Restart();
        var sums = new SumTypeChecker(comp).Check();
        var existentials = new ExistentialConceptChecker(comp).Check();
        var auxMs = timer.ElapsedMilliseconds;

        TestContext.WriteLine(
            $"{label}: parse {parseMs} ms ({cache.Misses - misses} parsed, {cache.Hits - hits} cached), "
            + $"compile {compileMs} ms, lint {lintMs} ms, "
            + $"checkAll {checkMs} ms ({results.Count} functions, {results.Count(r => !r.Succeeded)} failing), "
            + $"aux {auxMs} ms ({sums.Count + existentials.Count} findings), "
            + $"total {parseMs + compileMs + lintMs + checkMs + auxMs} ms");

        Assert.IsTrue(comp.CompletedCompilation, $"{label}: stdlib did not complete compilation");
    }
}
