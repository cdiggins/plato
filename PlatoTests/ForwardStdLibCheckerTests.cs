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
        // Measured 2026-07-29 after the tuple-coercion checker fixes (Solver.cs): the return-position
        // coercion no longer hard-binds a not-yet-resolved tuple result var to the declared struct
        // (fixed Tuple{N} -> concrete-struct returns), and MatchArg now accepts a same-shape tuple
        // literal for a struct PARAMETER (fixed Eval/NormalAt/TangentU/VAt((u,v)) and the Scale((x,y,z))
        // family). Both are real generated-C# tuple-constructor implicit operators, so no checker was
        // weakened. That took the forward count 45 -> 27 (and the stdlib-legacy ratchet 32 -> 26).
        // 27 / 1978 remain — CHK201 no-match (21) + CHK101 cannot-unify (13). Three residual clusters,
        // each needing a fix out of scope of the tuple work:
        //   * tuple -> GENERIC-INTERFACE returns (Tuple2<$T,$T> vs IntervalLike<$T> — IntervalsTransforms
        //     x9): the checker cannot soundly pick the concrete implementer a tuple becomes; a library
        //     redesign (return a concrete type / Self) is the right fix. Mirrors the legacy IInterval/
        //     IBounds residue.
        //   * sum-type CASE references (plato-288, in progress): `Axis3D.Y` types as Number and
        //     `SignedAxis3D.NegX` resolves as a call — nullary sum-case member access is unimplemented
        //     (Axes/Axes2D/SignedAxes.Opposite/Transforms.EulerAngles' ZXY).
        //   * function-typed FIELD invocation (`self.Function(point)` on *FunctionField x8) and element
        //     covariance (`Array<Point2D>` for `Array<Interpolatable>` in DeCasteljau x6).
        // The count drifts with library growth: this is a ceiling to LOWER, never to raise.
        // Worklist: SummarizeForwardStdLibDiagnostics.
        private const int MaxFunctionsWithDiagnostics = 27;

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

            // Compilation catches its own exceptions; it now records them in InternalErrors, but
            // capture the log too so a failure here reads as a story rather than one line.
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

            // Green since plato-289: all 84 files parse and compile clean (1133 types, 6668 reified
            // functions). This must hold in Debug AND Release — the bug it replaced was a Debug.Assert
            // in FunctionInstance that fired only under Debug and was then swallowed.
            Assert.IsEmpty(comp.SymbolFactory.Errors, "forward stdlib has symbol resolution errors");
            Assert.IsEmpty(comp.SemanticErrors, "forward stdlib has semantic errors");
            Assert.IsEmpty(comp.InternalErrors, "forward stdlib has internal errors");
            Assert.IsTrue(comp.CompletedCompilation, "forward stdlib did not complete compilation");
        }

        /// <summary>
        /// The compiler must never fail silently: a false <c>CompletedCompilation</c> always comes
        /// with at least one internal error to act on. Uses the cheapest failing input there is.
        /// </summary>
        [Test]
        public static void IncompleteCompilationAlwaysReportsAnInternalError()
        {
            var comp = new Compilation(Ara3D.Logging.Logger.Create(new StringBuilder()),
                Enumerable.Empty<Ara3D.Geometry.AST.AstNode>());
            Assert.IsFalse(comp.CompletedCompilation);
            Assert.IsNotEmpty(comp.InternalErrors,
                "CompletedCompilation == false must imply at least one InternalErrors entry");
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
