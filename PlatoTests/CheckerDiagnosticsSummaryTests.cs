using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Reporting fixture for the checker-completeness burn-down (assessment item 2, "declare the
    /// handwritten intrinsics"): prints how many stdlib functions still carry located diagnostics,
    /// grouped by diagnostic code and by the call name that failed to resolve — the worklist for
    /// plato-src/intrinsics.plato.
    /// </summary>
    [TestFixture]
    public static class CheckerDiagnosticsSummaryTests
    {
        [Test]
        public static void SummarizeUnresolvedStdLibDiagnostics()
        {
            var results = new TypeChecker(CheckerTestSupport.CompileStdLib()).CheckAll();
            var failing = results.Where(r => !r.Succeeded).ToList();

            TestContext.WriteLine($"functions with diagnostics: {failing.Count} / {results.Count}");

            var byCode = failing.SelectMany(r => r.Diagnostics)
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .GroupBy(d => d.Code)
                .OrderByDescending(g => g.Count());
            foreach (var g in byCode)
                TestContext.WriteLine($"  {g.Key}: {g.Count()}");

            // The unresolved-call names: which member the solver could not see.
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
    }
}
