using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Runs the checker passes over the real standard library. These are the "works on real code"
    /// proofs: normalization invariants must hold for every stdlib function body, and the pass
    /// must be idempotent there.
    /// </summary>
    [TestFixture]
    public static class CheckerIntegrationTests
    {
        private static Compilation _compilation;

        [OneTimeSetUp]
        public static void Setup()
            => _compilation = CheckerTestSupport.CompileStdLib();

        private static IEnumerable<FunctionDef> BodiedFunctions
            => _compilation.FunctionDefinitions.Where(f => f.Body != null);

        private static string Shape(Symbol s)
            => s == null ? "" : string.Join(",", s.GetSymbolTree().Select(x => x.GetType().Name));

        [Test]
        public static void StdLibCompiles()
        {
            Assert.IsTrue(_compilation.CompletedCompilation,
                "stdlib must compile for the checker integration tests to be meaningful");
            Assert.IsNotEmpty(_compilation.FunctionDefinitions);
        }

        [Test]
        public static void NormalizationHoldsInvariantsAcrossStdLib()
        {
            // Exercise the INTEGRATED path: Compilation.NormalizedFunctions, not a local Normalizer.
            var offenders = new List<string>();
            var checkedCount = 0;

            foreach (var fd in BodiedFunctions)
            {
                var normalized = _compilation.GetNormalizedFunction(fd);
                var violations = NormalizationInvariants.Check(normalized);
                checkedCount++;
                if (violations.Count > 0)
                    offenders.Add($"{fd.Name}: {string.Join("; ", violations)}");
            }

            Assert.Greater(checkedCount, 0, "expected to check some function bodies");
            Assert.IsEmpty(offenders,
                $"{offenders.Count} function(s) violated normalization invariants:\n" +
                string.Join("\n", offenders.Take(25)));
        }

        [Test]
        public static void NormalizationIsIdempotentAcrossStdLib()
        {
            var norm = new Normalizer();
            var mismatches = new List<string>();

            foreach (var fd in BodiedFunctions)
            {
                var once = norm.NormalizeFunction(fd);
                var twice = new Normalizer().NormalizeFunction(once);
                if (Shape(once.Body) != Shape(twice.Body))
                    mismatches.Add(fd.Name);
            }

            Assert.IsEmpty(mismatches,
                "second normalization pass changed structure for: " + string.Join(", ", mismatches.Take(25)));
        }

        // --- solve pass over the stdlib -----------------------------------------

        [Test]
        public static void SolverIsTotalAndDiagnosticsAreLocated()
        {
            // The whole run must complete without throwing (totality), and every diagnostic must
            // carry an origin symbol so it can point at a source location.
            var results = new TypeChecker(_compilation).CheckAll();
            Assert.IsNotEmpty(results);

            foreach (var r in results)
                foreach (var d in r.Diagnostics)
                    Assert.NotNull(d.Origin, $"unlocated diagnostic in '{r.Function.Name}': {d}");
        }

        [Test]
        public static void SolverResolvesSomeStdLibFunctionsCleanly()
        {
            var results = new TypeChecker(_compilation).CheckAll();
            var clean = results.Count(r => r.Succeeded);
            TestContext.WriteLine(
                $"Checker: {clean}/{results.Count} stdlib functions resolved with no errors " +
                $"(concept satisfaction, implicit casts, and generic-concept element inference in).");
            Assert.Greater(clean, 0,
                "expected the first-cut solver to fully resolve at least some real functions");
        }
    }
}
