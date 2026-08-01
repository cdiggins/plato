using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Checking tests for DECLARED TYPE-PARAMETER BOUNDS (plato-382, phase B) — the phase that makes
    /// `type Tween&lt;T&gt; where T: Interpolatable` mean something. Two independent mechanisms are
    /// under test:
    ///
    ///   * <see cref="TypeConstraintChecker"/> — the instantiation-site gate. Writing
    ///     <c>Tween&lt;String&gt;</c> anywhere is CHK309, because String is not Interpolatable.
    ///   * <see cref="Solver"/>'s bound-licensed member lookup — a call on a bare <c>T</c> resolves
    ///     through a concept exactly when a declared bound supplies it. An unlicensed call still
    ///     resolves (pre-bounds behavior) but is reported CHK205, a warning.
    ///
    /// The front end these build on is covered by <see cref="TypeConstraintParsingTests"/>.
    /// </summary>
    [TestFixture]
    public static class TypeConstraintCheckingTests
    {
        // --- helpers -------------------------------------------------------------

        /// <summary>Compiles a source snippet through a temp folder, the route the other checker
        /// fixtures use.</summary>
        private static Compilation Compile(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato382b-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                var compilation = CheckerTestSupport.CompileFolder(dir);
                CollectionAssert.IsEmpty(compilation.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                    "the fixture must resolve cleanly or every assertion below would be vacuous");
                return compilation;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static string[] Codes(Compilation c)
            => new TypeConstraintChecker(c).Check().Select(d => d.Code).ToArray();

        private static string Messages(Compilation c)
            => string.Join("\n", new TypeConstraintChecker(c).Check().Select(d => d.ToString()));

        /// <summary>Every diagnostic the type checker produced for the named function.</summary>
        private static CheckDiagnostic[] DiagnosticsFor(Compilation c, string functionName)
            => new TypeChecker(c).CheckAll()
                .Where(r => r.Function?.Name == functionName)
                .SelectMany(r => r.Diagnostics)
                .ToArray();

        // --- source fixtures -----------------------------------------------------

        // Small closed world: two unrelated concepts, one type that satisfies each, and generic
        // containers that are bounded by one, bounded by the other, or unbounded.
        private const string Prelude = @"
type Number { }
type Boolean { }
type Object { }
type String { }

concept Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}

concept Measurable
{
    Magnitude(x: Self): Number;
}

// Inherits Interpolatable rather than declaring Lerp, so bound satisfaction has to walk the
// concept closure rather than match a name at depth zero.
concept Coordinate
    inherits Interpolatable
{
    Origin(x: Self): Self;
}

type Vector2D implements Coordinate
{
    X: Number;
    Y: Number;
}

type Tween<T>
    where T: Interpolatable
{
    From: T;
    To: T;
}

type Gauge<T>
    where T: Measurable
{
    Item: T;
}

type Crate<T>
{
    Item: T;
}
";

        // --- instantiation-site verification (CHK309) ----------------------------

        [Test]
        public static void SatisfyingTypeArgument_ChecksClean()
        {
            var c = Compile(Prelude + @"
type Holder { Animation: Tween<Vector2D>; }
");
            CollectionAssert.IsEmpty(Codes(c),
                "Vector2D implements Coordinate which inherits Interpolatable, so it satisfies the bound");
        }

        [Test]
        public static void UnsatisfyingTypeArgument_InAFieldType_IsRejected()
        {
            var c = Compile(Prelude + @"
type Holder { Animation: Tween<String>; }
");
            var diagnostics = new TypeConstraintChecker(c).Check();
            Assert.IsNotEmpty(diagnostics, "Tween<String> must be rejected: String is not Interpolatable");
            Assert.IsTrue(diagnostics.All(d => d.Code == "CHK309"), Messages(c));
            StringAssert.Contains("String", diagnostics[0].Message);
            StringAssert.Contains("Interpolatable", diagnostics[0].Message);
            Assert.AreEqual(DiagnosticSeverity.Error, diagnostics[0].Severity);
        }

        [Test]
        public static void UnsatisfyingTypeArgument_InALibrarySignature_IsRejected()
        {
            var c = Compile(Prelude + @"
library Ops
{
    Start(x: Tween<String>): String => x.From;
}
");
            CollectionAssert.Contains(Codes(c), "CHK309", Messages(c));
        }

        [Test]
        public static void UnsatisfyingTypeArgument_NestedInsideAnotherType_IsRejected()
        {
            var c = Compile(Prelude + @"
type Holder { Animations: Crate<Tween<String>>; }
");
            CollectionAssert.Contains(Codes(c), "CHK309",
                "the walk must recurse into type arguments, not stop at the outermost construction");
        }

        [Test]
        public static void BoundThatDoesNotNameAConcept_IsRejected()
        {
            var c = Compile(Prelude + @"
type Weighed<T>
    where T: Number
{
    Item: T;
}
");
            CollectionAssert.Contains(Codes(c), "CHK310",
                "a `where` bound naming a concrete type promises something nothing can check");
        }

        [Test]
        public static void BoundNamingASiblingParameter_IsSubstitutedBeforeItIsChecked()
        {
            // `where TPoint: Difference<TDelta>` is checked at the ACTUAL delta, so a construction
            // whose point type differs on the delta is caught rather than passing on the name alone.
            var c = Compile(Prelude + @"
concept Difference<TDelta>
{
    Between(a: Self, b: Self): TDelta;
}

type Millimetre implements Difference<Number> { Value: Number; }

type Span<TPoint, TDelta>
    where TPoint: Difference<TDelta>
{
    Min: TPoint;
    Max: TPoint;
}

type GoodHolder { S: Span<Millimetre, Number>; }
type BadHolder  { S: Span<Millimetre, String>; }
");
            var diagnostics = new TypeConstraintChecker(c).Check();
            Assert.AreEqual(1, diagnostics.Count, Messages(c));
            StringAssert.Contains("Difference<String>", diagnostics[0].Message);
        }

        // --- sum types go through the identical path (plato-079 readiness) --------

        [Test]
        public static void BoundsOnASumTypeParameter_UseTheSameCodePath()
        {
            // CHK306 still rejects the generic sum itself — that is not lifted here. The point of
            // this test is that the CONSTRAINT machinery is indifferent to IsSum: the bound resolves
            // and is verified at the construction site exactly as it is for a record type, so
            // lifting CHK306 later needs no constraint-side change.
            var c = Compile(Prelude + @"
type Maybe<T>
    where T: Interpolatable
    = Nothing | Just(Value: T);

type GoodHolder { M: Maybe<Vector2D>; }
type BadHolder  { M: Maybe<String>; }
");
            var maybe = c.TypeDefinitionsByName["Maybe"];
            Assert.IsTrue(maybe.IsSum, "the fixture must actually be a sum type");
            CollectionAssert.AreEqual(new[] { "Interpolatable" },
                maybe.TypeParameters.Single().Constraints.Select(b => b.Name).ToArray(),
                "a bound on a sum type's parameter must resolve like any other");

            var diagnostics = new TypeConstraintChecker(c).Check();
            Assert.AreEqual(1, diagnostics.Count, Messages(c));
            Assert.AreEqual("CHK309", diagnostics[0].Code);
            StringAssert.Contains("Maybe", diagnostics[0].Message);

            // And the pre-existing generic-sum rejection is untouched by any of this.
            CollectionAssert.Contains(new SumTypeChecker(c).Check().Select(d => d.Code).ToArray(), "CHK306");
        }

        [Test]
        public static void ACaseFieldTypeOfASumType_IsAConstructionSiteToo()
        {
            var c = Compile(Prelude + @"
type Shape = Empty | Moving(Path: Tween<String>);
");
            CollectionAssert.Contains(Codes(c), "CHK309",
                "a sum case's field type is walked in the same loop as a record's fields");
        }

        // --- bound-licensed member lookup (CHK205) -------------------------------

        [Test]
        public static void DeclaredBound_LicensesAMemberCallOnABareTypeParameter()
        {
            var c = Compile(Prelude + @"
library Ops
{
    Sample(x: Tween<$T>, t: Number): $T => x.From.Lerp(x.To, t);
}
");
            var diagnostics = DiagnosticsFor(c, "Sample");
            CollectionAssert.IsEmpty(diagnostics.Select(d => d.ToString()).ToArray(),
                "Lerp on a bare $T is licensed by Tween's `where T: Interpolatable` — no warning either");
        }

        [Test]
        public static void MemberCallNotSuppliedByTheDeclaredBound_IsReported()
        {
            var c = Compile(Prelude + @"
library Ops
{
    Sample(x: Gauge<$T>, y: Gauge<$T>, t: Number): $T => x.Item.Lerp(y.Item, t);
}
");
            var diagnostics = DiagnosticsFor(c, "Sample");
            var unlicensed = diagnostics.Where(d => d.Code == "CHK205").ToArray();
            Assert.AreEqual(1, unlicensed.Length,
                "Measurable does not supply Lerp: " + string.Join("\n", diagnostics.Select(d => d.ToString())));
            StringAssert.Contains("Measurable", unlicensed[0].Message);
            StringAssert.Contains("Lerp", unlicensed[0].Message);

            // A WARNING, deliberately: the call still resolves exactly as it did before bounds were
            // read, so nothing downstream of the checker changes behavior. See Solver.ResolveOverloadCore.
            Assert.AreEqual(DiagnosticSeverity.Warning, unlicensed[0].Severity);
            Assert.IsFalse(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                "an unlicensed call is a warning, not an error");
        }

        [Test]
        public static void AnUnboundedTypeParameter_StaysPermissive()
        {
            // The deliberate scope line: bounds RESTRICT where they are declared and change nothing
            // where they are not. Requiring a bound would be a language change — every generic
            // library body in the forward stdlib is written without one.
            var c = Compile(Prelude + @"
library Ops
{
    Sample(x: Crate<$T>, y: Crate<$T>, t: Number): $T => x.Item.Lerp(y.Item, t);
}
");
            CollectionAssert.IsEmpty(DiagnosticsFor(c, "Sample").Select(d => d.ToString()).ToArray(),
                "an unbounded parameter carries no promise to break");
        }

        [Test]
        public static void ABoundReachedThroughConceptInheritance_LicensesTheCall()
        {
            var c = Compile(Prelude + @"
type Path<T>
    where T: Coordinate
{
    From: T;
    To: T;
}

library Ops
{
    Sample(x: Path<$T>, t: Number): $T => x.From.Lerp(x.To, t);
}
");
            CollectionAssert.IsEmpty(DiagnosticsFor(c, "Sample").Select(d => d.ToString()).ToArray(),
                "Coordinate inherits Interpolatable, so the bound supplies Lerp transitively");
        }

        // --- propagation into library signatures ---------------------------------

        [Test]
        public static void ALibrarySignatureVariable_InheritsTheBoundOfTheTypeItAppearsIn()
        {
            var c = Compile(Prelude + @"
library Ops
{
    First(t: Tween<$T>): $T => t.From;
}
");
            var first = c.FunctionDefinitions.Single(f => f.Name == "First");
            var bounds = TypeConstraints.InheritedBounds(first);
            CollectionAssert.AreEqual(new[] { "$T" }, bounds.Keys.ToArray());
            CollectionAssert.AreEqual(new[] { "Interpolatable" }, bounds["$T"].Select(b => b.Name).ToArray());
        }

        [Test]
        public static void InheritedBounds_AreDeduplicatedAcrossRepeatedMentions()
        {
            var c = Compile(Prelude + @"
library Ops
{
    Both(a: Tween<$T>, b: Tween<$T>): $T => a.From;
}
");
            var both = c.FunctionDefinitions.Single(f => f.Name == "Both");
            CollectionAssert.AreEqual(new[] { "Interpolatable" },
                TypeConstraints.InheritedBounds(both)["$T"].Select(b => b.Name).ToArray());
        }

        [Test]
        public static void InheritedBounds_SubstituteASiblingParameterThroughToTheActualArgument()
        {
            var c = Compile(Prelude + @"
concept Difference<TDelta>
{
    Between(a: Self, b: Self): TDelta;
}

type Span<TPoint, TDelta>
    where TPoint: Difference<TDelta>
{
    Min: TPoint;
    Max: TPoint;
}

library Ops
{
    Low(s: Span<$P, Number>): $P => s.Min;
}
");
            var low = c.FunctionDefinitions.Single(f => f.Name == "Low");
            CollectionAssert.AreEqual(new[] { "Difference<Number>" },
                TypeConstraints.InheritedBounds(low)["$P"].Select(b => b.ToString()).ToArray(),
                "the bound must reach the body naming the ACTUAL delta, not the declaration's TDelta");
        }
    }
}
