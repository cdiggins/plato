using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Wave-2 semantic checks for sum types + match (plato-232). Drives the plato-test-sum corpus:
    /// each negative fixture must raise its <c>// EXPECT: CHKnnn</c> diagnostic (asserting on the
    /// code AND that the message names the offending/missing cases), and each positive fixture must
    /// compile clean. option.plato is a generics-stance negative (CHK306) per the v1 restriction.
    /// </summary>
    [TestFixture]
    public static class SumTypeCheckingTests
    {
        private static string SumDir()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var c = Path.Combine(dir.FullName, "plato-test-sum");
                if (Directory.Exists(c)) return c;
            }
            throw new DirectoryNotFoundException("plato-test-sum not found above " + AppContext.BaseDirectory);
        }

        private static (Compilation Comp, System.Collections.Generic.IReadOnlyList<CheckDiagnostic> Diags)
            CheckFixture(string relativePath)
        {
            var path = Path.Combine(SumDir(), relativePath);
            var ast = CheckerTestSupport.ParseFile(path);
            var comp = new Compilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var diags = new SumTypeChecker(comp).Check();
            return (comp, diags);
        }

        /// <summary>The CHK code on a negative fixture's first `// EXPECT:` line.</summary>
        private static string ExpectedCode(string relativePath)
        {
            var first = File.ReadLines(Path.Combine(SumDir(), relativePath)).First();
            var m = Regex.Match(first, @"CHK\d{3}");
            Assert.IsTrue(m.Success, $"fixture {relativePath} has no EXPECT CHK code on its first line");
            return m.Value;
        }

        // --- positives: compile clean --------------------------------------------

        [TestCase("pathsegment.plato")]
        [TestCase("fillrule.plato")]
        [TestCase("match-expression.plato")]
        [TestCase("nested-match.plato")]
        public static void Positive_CompilesWithoutDiagnostics(string file)
        {
            var (comp, diags) = CheckFixture(file);
            Assert.IsEmpty(comp.SymbolFactory.Errors,
                $"{file} produced symbol-resolution errors:\n{string.Join("\n", comp.SymbolFactory.Errors.Select(e => e.Message))}");
            Assert.IsEmpty(comp.SemanticErrors, $"{file} produced semantic errors:\n{string.Join("\n", comp.SemanticErrors)}");
            Assert.IsEmpty(diags, $"{file} produced sum-type diagnostics:\n{string.Join("\n", diags.Select(d => d.ToString()))}");
        }

        // --- negatives: each raises its EXPECT diagnostic ------------------------

        [TestCase("negatives/missing-case.plato")]
        [TestCase("negatives/unknown-case.plato")]
        [TestCase("negatives/duplicate-case.plato")]
        [TestCase("negatives/wrong-binder-arity.plato")]
        [TestCase("negatives/match-non-sum.plato")]
        [TestCase("negatives/duplicate-case-names.plato")]
        public static void Negative_RaisesExpectedDiagnostic(string file)
        {
            var expected = ExpectedCode(file);
            var (_, diags) = CheckFixture(file);
            Assert.IsTrue(diags.Any(d => d.Code == expected && d.Severity == DiagnosticSeverity.Error),
                $"{file} expected {expected}; got: {string.Join(" | ", diags.Select(d => d.Code + ": " + d.Message))}");
            // A negative fixture isolates a single failure mode: exactly one diagnostic.
            Assert.AreEqual(1, diags.Count, $"{file} expected exactly one diagnostic; got {diags.Count}");
        }

        // Each diagnostic must NAME the offending/missing case(s) and the sum type.
        [Test]
        public static void Diagnostics_NameTheOffendingCases()
        {
            string Msg(string file, string code)
            {
                var (_, diags) = CheckFixture(file);
                var d = diags.FirstOrDefault(x => x.Code == code);
                Assert.NotNull(d, $"{file}: no {code}");
                return d.Message;
            }

            var missing = Msg("negatives/missing-case.plato", "CHK300");
            Assert.That(missing, Does.Contain("PathSegment2D"));
            Assert.That(missing, Does.Contain("Cubic").And.Contain("Arc").And.Contain("Close"));

            var unknown = Msg("negatives/unknown-case.plato", "CHK301");
            Assert.That(unknown, Does.Contain("Curve").And.Contain("Shape"));

            var dup = Msg("negatives/duplicate-case.plato", "CHK302");
            Assert.That(dup, Does.Contain("Circle").And.Contain("Shape"));

            var arity = Msg("negatives/wrong-binder-arity.plato", "CHK303");
            Assert.That(arity, Does.Contain("Quadratic").And.Contain("Control").And.Contain("EndPoint"));

            var nonSum = Msg("negatives/match-non-sum.plato", "CHK304");
            Assert.That(nonSum, Does.Contain("Point2D"));

            var dupName = Msg("negatives/duplicate-case-names.plato", "CHK305");
            Assert.That(dupName, Does.Contain("Bad").And.Contain("Move"));
        }

        // --- generics stance: option.plato is restricted (CHK306) ----------------

        [Test]
        public static void GenericSum_IsRestricted_CHK306()
        {
            var (_, diags) = CheckFixture("option.plato");
            Assert.IsTrue(diags.Any(d => d.Code == "CHK306" && d.Message.Contains("Option")),
                $"option.plato expected CHK306; got: {string.Join(" | ", diags.Select(d => d.Code + ": " + d.Message))}");
        }
    }
}
