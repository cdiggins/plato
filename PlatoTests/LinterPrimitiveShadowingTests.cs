using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Types;
using NUnit.Framework;
using Ara3D.Geometry.CSharpWriter;

namespace PlatoTests
{
    /// <summary>
    /// LINT015: a type declaration whose NAME is a writer primitive and which declares fields.
    /// The writer decides primitiveness by string match and emits the runtime type for such a
    /// name, discarding the declared shape — so the declaration silently is not the authority
    /// it looks like (plato-365).
    ///
    /// These tests pin three things: the rule fires on a shadowing declaration at ERROR severity
    /// (so `lint --strict` fails on it), it does NOT fire on the fieldless declarations the stdlib
    /// uses to state an intrinsic's concept surface, and the compiler's copy of the primitive-name
    /// list still agrees exactly with the C# writer's table.
    /// </summary>
    [TestFixture]
    public static class LinterPrimitiveShadowingTests
    {
        // Self-contained: only the primitives the compiler needs, plus one shadowing declaration
        // and one innocent same-shaped type.
        private const string Source = @"
// Fieldless declarations of primitive names: the sanctioned idiom. NOT flagged.
type Number { }
type Integer { }
type Boolean { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }

// SHADOWING: 'Quaternion' is on the writer's primitive list, so these fields are discarded
// and the generated type is System.Numerics.Quaternion instead.
type Quaternion
{
    X: Number;
    Y: Number;
    Z: Number;
    W: Number;
}

// NOT shadowing: same shape, a name the writer knows nothing about.
type Rotor
{
    X: Number;
    Y: Number;
    Z: Number;
    W: Number;
}
";

        // The prevention case: a primitive name that is NOT grandfathered. 'Integer' is on the
        // writer's list and is NOT on KnownShadowedByStdlib, so giving it a shape is exactly the
        // mistake a future contributor would make, and it must red `lint --strict`.
        private const string ShadowingANonGrandfatheredPrimitive = @"
type Number { }
type Boolean { }
type Object { }

type Integer
{
    Value: Number;
}
";

        private static LintFinding[] Lint(string source = Source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-lint015-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                var findings = new Linter(CheckerTestSupport.CompileFolder(dir)).Findings;
                foreach (var f in findings)
                    TestContext.WriteLine(f.ToString());
                return findings.ToArray();
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static LintFinding[] Shadowings()
            => Lint().Where(f => f.Code == "LINT015").ToArray();

        [Test]
        public static void ShadowingDeclarationIsReported()
        {
            var findings = Shadowings();
            Assert.That(findings.Length, Is.EqualTo(1),
                "exactly the one field-bearing primitive-named type collides; got: " +
                string.Join(" | ", findings.Select(f => f.Message)));
            Assert.That(findings[0].Message, Does.Contain("'Quaternion'"),
                "the finding must name the shadowing type");
            Assert.That(findings[0].Message, Does.Contain("X: Number"),
                "the finding must show the shape that gets discarded");
        }

        [Test]
        public static void FieldlessPrimitiveDeclarationsAreNotReported()
            => Assert.That(
                string.Join(" | ", Shadowings().Select(f => f.Message)),
                Does.Not.Contain("'Number'").And.Not.Contain("'Boolean'").And.Not.Contain("'Function0'"),
                "declaring an intrinsic's concept surface with no fields discards nothing and is " +
                "how stdlib/foundation/primitives.types.plato is written");

        [Test]
        public static void UnrelatedTypeOfTheSameShapeIsNotReported()
            => Assert.That(string.Join(" | ", Shadowings().Select(f => f.Message)),
                Does.Not.Contain("'Rotor'"),
                "the rule is about the NAME being on the writer's primitive list, not about shape");

        [Test]
        public static void ShadowingIsAnErrorSoItFailsLintStrict()
        {
            // 'Quaternion' IS on KnownShadowedByStdlib (the forward stdlib declares it today), so
            // this synthetic corpus would report a Warning. Assert the severity function directly
            // for a name that is NOT grandfathered — the prevention case, which is the whole point:
            // any type that starts shadowing from now on reds `lint --strict`.
            var grandfathered = WriterPrimitiveNames.KnownShadowedByStdlib;
            Assert.That(grandfathered, Does.Not.Contain("Integer"),
                "the corpus below relies on 'Integer' being a non-grandfathered primitive");

            var fresh = Lint(ShadowingANonGrandfatheredPrimitive)
                .Where(f => f.Code == "LINT015")
                .ToArray();
            Assert.That(fresh.Length, Is.EqualTo(1));
            Assert.That(fresh[0].Severity, Is.EqualTo(LintSeverity.Error),
                "a type that starts shadowing a primitive from now on must be an ERROR: only Errors " +
                "gate `lint --strict` (Plato.CLI Program.Lint returns 1 iff ErrorCount > 0)");

            Assert.That(Shadowings().Single().Severity, Is.EqualTo(LintSeverity.Warning),
                "'Quaternion' is grandfathered (plato-365 increment c) and must not red the gate yet");

            // The grandfather list only ever shrinks, and only names that are actually primitive
            // can be on it — a stale entry would silently un-gate a name the writer no longer owns.
            Assert.That(grandfathered.All(n => WriterPrimitiveNames.All.Contains(n)), Is.True,
                "WriterPrimitiveNames.KnownShadowedByStdlib has a stale entry: it names a type that " +
                "is no longer a writer primitive. Delete that entry — the declaration is now " +
                "authoritative and needs no exemption.");
        }

        /// <summary>
        /// The enforcing half of the duplication. WriterPrimitiveNames.All lives in the compiler so
        /// the linter can run without depending on any backend; CSharpWriter.PrimitiveTypes holds
        /// the same names with their runtime mappings. Divergence in either direction is a defect:
        /// a name only in the writer escapes LINT015 entirely (the silent shadowing this rule
        /// exists to kill), and a name only in the compiler makes LINT015 fire on a declaration
        /// that is in fact authoritative.
        /// </summary>
        [Test]
        public static void WriterPrimitiveTableMatchesTheCompilerCopy()
        {
            var writer = CSharpWriter.PrimitiveTypes.Keys.OrderBy(x => x).ToArray();
            var compiler = WriterPrimitiveNames.All.OrderBy(x => x).ToArray();

            var onlyInWriter = writer.Except(compiler).ToArray();
            var onlyInCompiler = compiler.Except(writer).ToArray();

            Assert.That(onlyInWriter, Is.Empty,
                "CSharpWriter.PrimitiveTypes has name(s) missing from " +
                "PlatoCompiler/Types/WriterPrimitiveNames.All: [" + string.Join(", ", onlyInWriter) +
                "]. Add them there, or LINT015 cannot see that a stdlib declaration of that name " +
                "is silently overridden.");
            Assert.That(onlyInCompiler, Is.Empty,
                "WriterPrimitiveNames.All has name(s) the C# writer no longer treats as primitive: [" +
                string.Join(", ", onlyInCompiler) + "]. Deleting an entry from " +
                "CSharpWriter.PrimitiveTypes (plato-365) means deleting it from WriterPrimitiveNames.All " +
                "and from KnownShadowedByStdlib too — otherwise LINT015 fires on a declaration that " +
                "is now the real authority.");
        }
    }
}
