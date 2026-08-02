using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// LINT016 + ConceptHierarchy: redundant interface-to-interface inherits, and the ASCII
    /// spanning forest used by <c>Plato.ContextExport --hierarchy</c> (plato-371).
    /// </summary>
    [TestFixture]
    public static class ConceptHierarchyTests
    {
        // C inherits A redundantly: B already reaches A.
        private const string RedundantLattice = @"
interface A { }
interface B inherits A { }
interface C inherits A, B { }
";

        private const string CleanLattice = @"
interface A { }
interface B inherits A { }
interface C inherits B { }
interface D { }
interface Diamond inherits B, D { }
";

        private static LintFinding[] Lint(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-lint016-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                return new Linter(CheckerTestSupport.CompileFolder(dir)).Findings.ToArray();
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static string Hierarchy(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-hier-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                var compilation = CheckerTestSupport.CompileFolder(dir);
                Assert.That(compilation.CompletedCompilation, Is.True);
                return ConceptHierarchy.FormatAscii(compilation);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        [Test]
        public static void RedundantInheritIsReportedAsLint016()
        {
            var findings = Lint(RedundantLattice).Where(f => f.Code == "LINT016").ToArray();
            Assert.That(findings.Length, Is.EqualTo(1),
                "exactly one redundant inherits; got: " +
                string.Join(" | ", findings.Select(f => f.Message)));
            Assert.That(findings[0].Severity, Is.EqualTo(LintSeverity.Info));
            Assert.That(findings[0].Message, Does.Contain("'C'"));
            Assert.That(findings[0].Message, Does.Contain("'A'"));
            Assert.That(findings[0].Message, Does.Contain("'B'"));
        }

        [Test]
        public static void CleanLatticeHasNoLint016()
        {
            var findings = Lint(CleanLattice).Where(f => f.Code == "LINT016").ToArray();
            Assert.That(findings, Is.Empty,
                string.Join(" | ", findings.Select(f => f.Message)));
        }

        [Test]
        public static void AsciiForestShowsTreeAndRedundantSection()
        {
            var text = Hierarchy(RedundantLattice);
            Assert.That(text, Does.Contain("interface hierarchy"));
            Assert.That(text, Does.Contain("A"));
            Assert.That(text, Does.Contain("`-- B").Or.Contain("+-- B"));
            Assert.That(text, Does.Contain("## Redundant inherits (1)"));
            Assert.That(text, Does.Contain("C inherits A"));
            Assert.That(text, Does.Contain("already via B"));
        }

        [Test]
        public static void AsciiForestNotesMultiParent()
        {
            var text = Hierarchy(CleanLattice);
            Assert.That(text, Does.Contain("## Multi-parent interfaces"));
            Assert.That(text, Does.Contain("Diamond inherits"));
            Assert.That(text, Does.Contain("## Redundant inherits (0)"));
        }
    }
}
