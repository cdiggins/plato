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
    ///
    /// SCOPE: ALL FOUR tiers, <c>stdlib/future</c> included. stdlib-377 took <c>future</c> out of
    /// the lint gate and out of C# codegen, but NOT out of parsing and type-checking — those are
    /// the properties that keep aspirational vocabulary honest, and they are enforced here.
    /// </summary>
    [TestFixture]
    public static class ForwardStdLibCheckerTests
    {
        // Measured 2026-07-29: 0 / 2106. The forward vocabulary now type-checks CLEAN.
        //
        // The last cluster was 9 IntervalsTransformsInterval functions that build an interval from a
        // TUPLE LITERAL but declared the CONCEPT as their return type (`Union(...): IntervalLike<$T>
        // => (a, b)`), each yielding CHK101 'Tuple2<$T,$T>' vs 'IntervalLike<$T>'. The checker was
        // right to reject it: a concept does not say WHICH implementer the tuple becomes, so there is
        // no sound choice to make. The author's intent was `Self` — these transforms are
        // shape-preserving — so the fix was a LIBRARY fix, not a compiler fix: the 9 declarations (plus
        // the delegating FirstHalf/SecondHalf and the Tuple2<Self,Self> of SplitAt/Split) now return
        // `Self`, matching the convention ThenTransform and the Deformable surface already use.
        //
        // No checker change was needed. `Self` already unifies permissively by design (Solver.Unify:
        // "Self is compatible with anything ... monomorphization grounds it"), which is exactly the
        // deferral this shape wants — the concrete implementer is decided when Self is bound. Note the
        // corollary: `Self` in RETURN position is not arity-checked against the receiver's fields at
        // check time, so a wrong-shape tuple body would be accepted here and only caught downstream.
        // That permissiveness is pre-existing and shared with every other `Self`-returning function;
        // tightening it belongs in the monomorphizer, not here.
        //
        // The count drifts with library growth: this is a ceiling to LOWER, never to raise. At 0 it is
        // also a floor in practice — any new diagnostic is a regression.
        // Worklist: SummarizeForwardStdLibDiagnostics.
        private const int MaxFunctionsWithDiagnostics = 0;

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
            => CheckerTestSupport.PlatoFiles(folder)
                .Select(ParseFailure)
                .Where(s => s.Length > 0)
                .ToList();

        [Test]
        public static void ForwardStdLibParsesAndCompiles()
        {
            var folder = CheckerTestSupport.FindForwardStdLib();
            var files = CheckerTestSupport.PlatoFiles(folder);
            TestContext.WriteLine($"forward stdlib: {folder}");
            TestContext.WriteLine($"files (*.plato, recursive): {files.Count}");

            // Corpus floor. Every assertion below is an IsEmpty over results derived from `files`,
            // so an empty corpus would make this whole gate pass while proving nothing — exactly
            // what a top-only enumeration did once the tier subfolders landed. Deliberately far
            // below the real count (398 at the 2026-07-30 reorg) so ordinary growth or pruning
            // never trips it; it only catches "the enumeration stopped finding the library".
            Assert.Greater(files.Count, 300,
                $"forward stdlib corpus collapsed to {files.Count} files under {folder} — "
                + "the enumeration is broken, not the library");

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

            // Green since plato-289 and held across the plato-293 re-partition: all 349 files parse
            // and compile clean. This must hold in Debug AND Release — the bug it replaced was a Debug.Assert
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

        // plato-311: every concept the forward stdlib stores in type position (an existential
        // "any C" — e.g. `Path: Curve3D` on SweptSurface) must have an object-safe surface, or it
        // has no non-generic view and no defined C# lowering. Zero is a hard gate, not a ratchet:
        // a new CHK308 means a new concept was stored existentially without any object-safe
        // member, which the writer cannot emit (see ExistentialConceptChecker / the plato-311 ADR).
        [Test]
        public static void ForwardStdLibHasNoViewlessExistentialReferences()
        {
            var comp = CheckerTestSupport.CompileForwardStdLib();
            var errors = new ExistentialConceptChecker(comp).Check()
                .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var d in errors)
                TestContext.WriteLine($"  {d.Code} {d.Message}");
            Assert.IsEmpty(errors,
                "Forward stdlib has existential references to view-less concepts (CHK308).");
        }

        // plato-382: every construction of a bounded generic type in the forward vocabulary must
        // supply an argument that satisfies the bound, and every bound must name a concept. Zero is
        // a hard gate, not a ratchet — a CHK309 means a declaration promises something its argument
        // cannot do, which is precisely the class of error bounds were added to catch.
        [Test]
        public static void ForwardStdLibSatisfiesEveryDeclaredBound()
        {
            var comp = CheckerTestSupport.CompileForwardStdLib();
            var errors = new TypeConstraintChecker(comp).Check()
                .Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            foreach (var d in errors)
                TestContext.WriteLine($"  {d.Code} {d.Message}");

            // The population, so a green here is not vacuous: bounds must actually exist to be met.
            var bounded = comp.AllTypeAndLibraryDefinitions
                .Where(t => t != null && t.TypeParameters.Any(p => p.Constraints.Count > 0))
                .ToList();
            TestContext.WriteLine($"declarations carrying bounds: {bounded.Count}");
            foreach (var t in bounded)
                TestContext.WriteLine($"  {t.Name}<{string.Join(", ", t.TypeParameters.Select(p => p.Name))}>");
            Assert.IsNotEmpty(bounded,
                "no declaration in the forward stdlib carries a bound — the gate would prove nothing");

            Assert.IsEmpty(errors, "Forward stdlib violates a declared type-parameter bound (CHK309/CHK310).");
        }

        /// <summary>
        /// plato-382, the OTHER direction: a call on a bare bounded type parameter that no declared
        /// bound supplies — a declaration that under-promises what its own bodies do. The forward
        /// stdlib now has none, which is what allowed CHK205 to be promoted from warning to ERROR
        /// (phase D). The set is asserted by name rather than counted so that a NEW one cannot hide
        /// behind a ceiling; the expected set is EMPTY and adding to it needs a stated reason.
        /// </summary>
        [Test]
        public static void ForwardStdLibUnlicensedBoundedCalls()
        {
            // Was { "IntervalsTransformsBounds.Center" } until `TPoint: Interpolatable` was declared
            // on BoundsLike (stdlib/foundation/intervals-bounds.concepts.plato): Center and Lerp
            // blend the two corners, which only Interpolatable supplies, and every actual TPoint
            // (Point2D/Point3D) is Interpolatable — the declaration simply had not said so.
            var known = new string[0];

            var results = new TypeChecker(CheckerTestSupport.CompileForwardStdLib()).CheckAll();
            var found = results
                .SelectMany(r => r.Diagnostics.Where(d => d.Code == "CHK205")
                    .Select(d => (Where: $"{r.Function?.OwnerType?.Name}.{r.Function?.Name}", d.Message)))
                .ToList();
            foreach (var f in found)
                TestContext.WriteLine($"  {f.Where}: {f.Message}");

            CollectionAssert.AreEquivalent(known, found.Select(f => f.Where).Distinct().ToList(),
                "the set of bound-unlicensed calls changed — add the missing bound to the declaration, "
                + "or record the new one here with the reason");
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
