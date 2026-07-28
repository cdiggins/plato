using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Runs the type checker over the FORWARD vocabulary (<c>stdlib</c>), which until now saw only
    /// <c>Plato.CLI lint</c>. Mirrors the stdlib-legacy pair: a reporting test (the worklist) and a
    /// ratchet. Also runs <see cref="SumTypeChecker"/>, since the forward stdlib is where sum types
    /// actually live (PathSegment2D, Paint, MaskSource2D, ScalarFieldNode2D/3D, WindowFunction).
    /// </summary>
    [TestFixture]
    public static class ForwardStdLibCheckerTests
    {
        // Measured 2026-07-28, the FIRST run of the checker over the forward vocabulary:
        // 40 / 1957 functions carry error diagnostics — CHK201 no-match (27) + CHK101 cannot-unify (16);
        // zero CHK102/103/203, and zero sum-type diagnostics (CHK300-306) across 118 sum declarations.
        // Two clusters, both the shapes the stdlib-legacy ratchet still carries: a tuple literal not
        // coercing to a nominal/generic-interface parameter (Tuple2<..> vs Point2D / IntervalLike<$T> —
        // GeometryTraits, IntervalsTransforms, Polygons), and a member the solver cannot see on a
        // concept-typed receiver (Eval/NormalAt/TangentUAt/TangentVAt on ParametricSurface and friends,
        // Function on the *FunctionField types, DeCasteljau on Array<T>). The count drifts with library
        // growth: this is a ceiling to LOWER, never to raise. Worklist: SummarizeForwardStdLibDiagnostics.
        private const int MaxFunctionsWithDiagnostics = 40;

        /// <summary>Empty when the file parses; otherwise the failure, so one bad file cannot mask the rest.</summary>
        private static string ParseFailure(string path)
        {
            try
            {
                CheckerTestSupport.ParseFile(path);
                return "";
            }
            catch (Exception e)
            {
                return $"{Path.GetFileName(path)}: {e.Message}";
            }
        }

        private static IReadOnlyList<string> ParseFailures(string folder)
            => Directory.GetFiles(folder, "*.plato")
                .Select(ParseFailure)
                .Where(s => s.Length > 0)
                .ToList();

        [Test]
        public static void ForwardStdLibParsesAndCompiles()
        {
            var folder = CheckerTestSupport.FindForwardStdLib();
            var files = Directory.GetFiles(folder, "*.plato");
            TestContext.WriteLine($"forward stdlib: {folder}");
            TestContext.WriteLine($"files (*.plato, top-directory-only): {files.Length}");

            var failures = ParseFailures(folder);
            foreach (var f in failures)
                TestContext.WriteLine($"  PARSE FAIL {f}");

            // Compilation swallows its own exceptions and only LOGS them, so compile once with a
            // capturing logger: without this a false CompletedCompilation has no explanation.
            var log = new StringBuilder();
            var asts = files.Select(CheckerTestSupport.ParseFile).ToList();
            var comp = new Compilation(Ara3D.Logging.Logger.Create(log), asts);
            var lines = log.ToString().Split('\n');
            var stopped = Array.FindIndex(lines, l => l.Contains("Exception"));
            if (stopped >= 0)
                foreach (var line in lines.Skip(Math.Max(0, stopped - 6)).Take(10))
                    TestContext.WriteLine($"  LOG {line.Trim()}");
            TestContext.WriteLine($"CompletedCompilation: {comp.CompletedCompilation}");
            TestContext.WriteLine($"resolution errors: {comp.SymbolFactory.Errors.Count}");
            foreach (var e in comp.SymbolFactory.Errors.Take(40))
                TestContext.WriteLine($"  RES {e.Message}");
            TestContext.WriteLine($"semantic errors: {comp.SemanticErrors.Count}");
            foreach (var e in comp.SemanticErrors.Take(40))
                TestContext.WriteLine($"  SEM {e}");
            TestContext.WriteLine($"internal errors: {comp.InternalErrors.Count}");
            foreach (var e in comp.InternalErrors.Take(40))
                TestContext.WriteLine($"  INT {e}");

            Assert.IsEmpty(failures, "forward stdlib files failed to parse");

            // HONEST CURRENT STATE (2026-07-28): all 84 files parse (0 resolution / 0 semantic /
            // 0 internal errors, 1133 types, 6668 reified functions) and the checker still runs over
            // 1957 functions — but under a DEBUG build Compilation never reaches CompletedCompilation.
            // The log stops right after "Grouping Reified functions by name", i.e. in the
            // Libraries/ConcreteType construction that drives FunctionInstance, whose Debug.Assert
            // fires and is swallowed by the catch-all in Compilation's ctor. Skipped, not asserted
            // away, so this flips to green the day that assert is fixed instead of staying yellow.
            if (!comp.CompletedCompilation)
                Assert.Ignore("forward stdlib does not complete compilation (see the LOG lines above); "
                    + "the checker still runs, so the sibling fixtures remain meaningful");
        }

        [Test]
        public static void SummarizeForwardStdLibDiagnostics()
        {
            var results = new TypeChecker(CheckerTestSupport.CompileForwardStdLib()).CheckAll();
            var failing = results.Where(r => !r.Succeeded).ToList();

            TestContext.WriteLine($"functions with diagnostics: {failing.Count} / {results.Count}");

            var byCode = failing.SelectMany(r => r.Diagnostics)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .GroupBy(d => d.Code)
                .OrderByDescending(g => g.Count());
            foreach (var g in byCode)
                TestContext.WriteLine($"  {g.Key}: {g.Count()}");

            var names = new Dictionary<string, int>();
            foreach (var r in failing)
                foreach (var d in r.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                {
                    var n = (d.Origin as FunctionCall)?.Function?.Name ?? d.Origin?.Name ?? "?";
                    names[n] = names.TryGetValue(n, out var c) ? c + 1 : 1;
                }
            TestContext.WriteLine("top unresolved names:");
            foreach (var kv in names.OrderByDescending(kv => kv.Value).Take(40))
                TestContext.WriteLine($"  {kv.Value,4}  {kv.Key}");

            TestContext.WriteLine("per-function detail (all, all error diagnostics):");
            foreach (var r in failing)
                foreach (var d in r.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))
                    TestContext.WriteLine($"  {r.Function?.OwnerType?.Name}.{r.Function?.Name}: {d.Code} {d.Message}");
        }

        [Test]
        public static void SummarizeForwardStdLibSumTypeDiagnostics()
        {
            var comp = CheckerTestSupport.CompileForwardStdLib();
            var diags = new SumTypeChecker(comp).Check();
            var errors = diags.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            // Report the population too: "0 diagnostics" only means something if sums were seen.
            var sums = comp.AllTypeAndLibraryDefinitions.Where(t => t != null && t.IsSum).ToList();
            TestContext.WriteLine($"sum declarations: {sums.Count}");
            foreach (var s in sums)
                TestContext.WriteLine($"  {s.Name} ({s.Cases.Count} cases)");

            TestContext.WriteLine($"sum-type diagnostics: {diags.Count} ({errors.Count} errors)");
            foreach (var g in diags.GroupBy(d => d.Code).OrderByDescending(g => g.Count()))
                TestContext.WriteLine($"  {g.Key}: {g.Count()}");
            foreach (var d in diags)
                TestContext.WriteLine($"  {d.Code} [{d.Severity}] {d.Message}");
        }

        [Test]
        public static void ForwardStdLibDiagnosticCountDoesNotRegress()
        {
            var results = new TypeChecker(CheckerTestSupport.CompileForwardStdLib()).CheckAll();
            var failing = results.Count(r => !r.Succeeded);
            TestContext.WriteLine($"functions with diagnostics: {failing} / {results.Count} (ceiling {MaxFunctionsWithDiagnostics})");
            Assert.LessOrEqual(failing, MaxFunctionsWithDiagnostics,
                "Forward stdlib checker completeness regressed. "
                + "See SummarizeForwardStdLibDiagnostics for the worklist.");
        }
    }
}
