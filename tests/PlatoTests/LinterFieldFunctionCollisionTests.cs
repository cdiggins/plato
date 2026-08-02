using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// LINT014: a name that is BOTH a struct field and a no-arg library function on an unrelated
    /// receiver. Such a name couples the two types in the generated C#: the writer must demote the
    /// library function out of its extension class and back into every struct, because a C# instance
    /// member silently hides a same-name extension method
    /// (<c>ExtensionStylePlan.DemoteMovedNames</c>). It used to be worse — call-site syntax was
    /// decided globally by name too, which produced 915 x CS0119 from the single field
    /// <c>Histogram.Range</c> and 110 x CS0030 from a field named <c>Amount</c>; that half is fixed
    /// receiver-aware (plato-323 clusters 1 and item 2, <c>CSharpWriter.IsStructSurfaceProperty</c>).
    ///
    /// These tests pin the predicate's two halves: the collision IS reported, and the two shapes
    /// that are design rather than defect (a field on the receiver itself, and a field discharging
    /// an obligation of the receiver interface) are NOT.
    /// </summary>
    [TestFixture]
    public static class LinterFieldFunctionCollisionTests
    {
        // Self-contained, in the style of CollectionFieldAtCountTests: only the primitives the
        // compiler needs, and no System.Numerics-backed names.
        private const string Source = @"
type Number { }
type Integer { }
type Boolean { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }

// The obligation a field may legitimately discharge.
interface HasWidth
{
    Width(x: Self): Number;
}

// COLLISION: 'Range' is a field here, and a no-arg library function over Integer.
type Histogram
{
    Range: Number;
}

// NOT a collision: 'Width' is a field here AND the library function's receiver is the
// interface this type implements - the field IS the implementation (field forwarding).
type Box implements HasWidth
{
    Width: Number;
}

// NOT a collision: 'Depth' is a field here and the library function's receiver is this
// very type (a derived accessor over its own field).
type Slab
{
    Depth: Number;
}

library Surface
{
    // Unrelated receiver: Integer has no 'Range' field.
    Range(n: Integer): Number;

    // Receiver interface that the field owner implements.
    Width(x: HasWidth): Number;

    // Receiver IS the field owner.
    Depth(s: Slab): Number;

    // '_' receiver: emitted as a static, so no property-syntax hazard even though
    // 'Range' is a field name.
    Range(_: Boolean): Number;
}
";

        private static LintFinding[] Lint()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-lint014-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
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

        private static string[] Collisions()
            => Lint().Where(f => f.Code == "LINT014").Select(f => f.Message).ToArray();

        [Test]
        public static void UnrelatedReceiverIsReported()
        {
            var messages = Collisions();
            Assert.That(messages.Length, Is.EqualTo(1),
                "exactly one of the four no-arg functions collides; got: " + string.Join(" | ", messages));
            Assert.That(messages[0], Does.Contain("'Histogram.Range'").And.Contain("Range(Integer)"),
                "the finding must name both sides: the field owner and the function's receiver");
            Assert.That(messages[0], Does.Contain("'Surface'"), "the finding must name the library");
        }

        [Test]
        public static void FieldForwardingToAnImplementedConceptIsNotReported()
            => Assert.That(string.Join(" | ", Collisions()), Does.Not.Contain("Width"),
                "a field discharging an obligation of the receiver interface is the intended design, " +
                "not a collision");

        [Test]
        public static void FunctionOnTheFieldOwnerItselfIsNotReported()
            => Assert.That(string.Join(" | ", Collisions()), Does.Not.Contain("Depth"),
                "a derived accessor whose receiver IS the field owner renders as a property correctly");

        [Test]
        public static void TypeLevelReceiverIsNotReported()
            => Assert.That(string.Join(" | ", Collisions()), Does.Not.Contain("Range(Boolean)"),
                "a '_' receiver emits as a static member, which is never rendered with property syntax");

        [Test]
        public static void FindingIsInfoSoItNeverGatesStrictOrTheRatchet()
        {
            var lint014 = Lint().Where(f => f.Code == "LINT014").ToArray();
            Assert.That(lint014, Is.Not.Empty);
            Assert.That(lint014.All(f => f.Severity == LintSeverity.Info), Is.True,
                "LINT014 fires on the existing corpus; raising it above Info would break " +
                "`lint --strict` and the lint ratchet (the LINT003 precedent)");
        }
    }
}
