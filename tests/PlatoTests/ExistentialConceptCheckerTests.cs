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
    /// CHK308 (plato-311, Option A): an existential (type-position) reference to an interface with an
    /// empty object-safe surface has no non-generic view and therefore no defined C# lowering — the
    /// checker must reject it instead of letting Plato.CSharpWriter improvise. Drives the
    /// plato-test-existential corpus with the same fixture conventions as SumTypeCheckingTests:
    /// negatives carry an <c>// EXPECT: CHKnnn</c> first line, positives must check clean.
    /// </summary>
    [TestFixture]
    public static class ExistentialConceptCheckerTests
    {
        private static string FixtureDir()
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
            {
                var c = Path.Combine(dir.FullName, "tests", "plato-test-existential");
                if (Directory.Exists(c)) return c;
                c = Path.Combine(dir.FullName, "plato-test-existential");
                if (Directory.Exists(c)) return c;
            }
            throw new DirectoryNotFoundException("plato-test-existential not found above " + AppContext.BaseDirectory);
        }

        private static (Compilation Comp, System.Collections.Generic.IReadOnlyList<CheckDiagnostic> Diags)
            CheckFixture(string relativePath)
        {
            var path = Path.Combine(FixtureDir(), relativePath);
            var ast = CheckerTestSupport.ParseFile(path);
            var comp = new Compilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var diags = new ExistentialConceptChecker(comp).Check();
            return (comp, diags);
        }

        private static string ExpectedCode(string relativePath)
        {
            var first = File.ReadLines(Path.Combine(FixtureDir(), relativePath)).First();
            var m = Regex.Match(first, @"CHK\d{3}");
            Assert.IsTrue(m.Success, $"fixture {relativePath} has no EXPECT CHK code on its first line");
            return m.Value;
        }

        [TestCase("stored-interface-ok.plato")]
        public static void Positive_CompilesWithoutDiagnostics(string file)
        {
            var (comp, diags) = CheckFixture(file);
            Assert.IsEmpty(comp.SymbolFactory.Errors,
                $"{file} produced symbol-resolution errors:\n{string.Join("\n", comp.SymbolFactory.Errors.Select(e => e.Message))}");
            Assert.IsEmpty(diags, $"{file} produced diagnostics:\n{string.Join("\n", diags.Select(d => d.ToString()))}");
        }

        [TestCase("negatives/viewless-interface-field.plato")]
        public static void Negative_RaisesExpectedDiagnostic(string file)
        {
            var expected = ExpectedCode(file);
            var (_, diags) = CheckFixture(file);
            Assert.IsTrue(diags.Any(d => d.Code == expected && d.Severity == DiagnosticSeverity.Error),
                $"{file} expected {expected}; got: {string.Join(" | ", diags.Select(d => d.Code + ": " + d.Message))}");
        }
    }
}
