using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// WHERE-CLAUSE BOUNDS ON A LIBRARY FUNCTION (plato-393) — the construct that lets a function
    /// promise something about its OWN signature variables:
    ///
    ///     DeCasteljau(xs: Array&lt;$T&gt;, t: Number): $T where $T: Interpolatable =&gt; ...;
    ///
    /// Before this, a function variable could only INHERIT a bound from a constructed type in its
    /// signature (plato-382), so a body needing an operation on a bare element of an unbounded
    /// container had to be written once per element type.
    ///
    /// The clause sits after the return type and before the body — the last thing in the signature,
    /// the same slot the clause occupies on `type` and `concept` — and names the variable exactly as
    /// the signature spells it, with the `$`.
    ///
    /// Four levels are under test here: parse, symbol, checking (both the licence it grants inside
    /// the body and the obligation it imposes at every call site), and lint. The C# emission half is
    /// <see cref="FunctionConstraintCodegenTests"/>.
    /// </summary>
    [TestFixture]
    public static class FunctionConstraintTests
    {
        // --- helpers -------------------------------------------------------------

        private static AstFile ParseOk(string source)
        {
            var parser = CommonParsers.PlatoParser(new ParserInput(source, "test.plato"), Ara3D.Logging.Logger.Null);
            Assert.IsTrue(parser.Succeeded,
                $"Expected a successful parse but got errors:\n{parser.ParserErrorsString}");
            return (AstFile)parser.Cst!.ToAst();
        }

        private static Compilation Compile(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato393-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                var c = CheckerTestSupport.CompileFolder(dir);
                CollectionAssert.IsEmpty(c.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                    "the fixture must resolve cleanly or every assertion below would be vacuous");
                return c;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static CheckDiagnostic[] DiagnosticsFor(Compilation c, string functionName)
            => new TypeChecker(c).CheckAll()
                .Where(r => r.Function?.Name == functionName)
                .SelectMany(r => r.Diagnostics)
                .ToArray();

        private static string Show(CheckDiagnostic[] ds)
            => string.Join("\n", ds.Select(d => d.ToString()));

        // --- source fixture ------------------------------------------------------

        // A miniature of the de Casteljau world: an UNBOUNDED generic container (Bag stands in for
        // Array, which is a primitive and must stay unbounded), a concept its element type has to
        // supply, one type that supplies it and one that does not.
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

type Point2D implements Interpolatable
{
    X: Number;
    Y: Number;
}

type Bag<T>
{
    Item: T;
}
";

        // The shape the whole issue exists for: the bound cannot be inherited from anything, because
        // the only constructed type in the signature is the unbounded container.
        private const string BoundedMix = Prelude + @"
library Ops
{
    Mix(xs: Bag<$T>, t: Number): $T where $T: Interpolatable
        => xs.Item.Lerp(xs.Item, t);
}
";

        // --- parse level ---------------------------------------------------------

        [Test]
        public static void FunctionWhereClause_ParsesAndReachesTheAst()
        {
            var md = ParseOk(BoundedMix).GetAllDescendants().OfType<AstMethodDeclaration>()
                .Single(m => m.Name.Text == "Mix");

            CollectionAssert.AreEqual(new[] { "$T" }, md.Constraints.Select(c => c.Name.Text).ToArray(),
                "the target keeps the `$`, because that is how the signature spells the variable");
            CollectionAssert.AreEqual(new[] { "Interpolatable" },
                md.Constraints.Select(c => c.Constraint.Name.Text).ToArray());
        }

        [Test]
        public static void FunctionWhereClause_DoesNotDisturbTheSignatureOrTheBody()
        {
            var md = ParseOk(BoundedMix).GetAllDescendants().OfType<AstMethodDeclaration>()
                .Single(m => m.Name.Text == "Mix");
            CollectionAssert.AreEqual(new[] { "xs", "t" }, md.Parameters.Select(p => p.Name.Text).ToArray());
            Assert.AreEqual("$T", md.Type.Name.Text);
            Assert.IsNotNull(md.Body, "the clause must not swallow the body");
        }

        [Test]
        public static void MultipleBounds_AndSeveralVariables_AreListedOnePerBound()
        {
            var md = ParseOk(Prelude + @"
library Ops
{
    Pair(a: Bag<$A>, b: Bag<$B>): Number where $A: Interpolatable, $A: Measurable, $B: Measurable
        => b.Item.Magnitude;
}
").GetAllDescendants().OfType<AstMethodDeclaration>().Single(m => m.Name.Text == "Pair");

            CollectionAssert.AreEqual(new[] { "$A", "$A", "$B" }, md.Constraints.Select(c => c.Name.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "Interpolatable", "Measurable", "Measurable" },
                md.Constraints.Select(c => c.Constraint.Name.Text).ToArray());
        }

        [Test]
        public static void FunctionWithoutWhereClause_StillCarriesNoBounds()
        {
            var md = ParseOk(Prelude + @"
library Ops
{
    First(xs: Bag<$T>): $T => xs.Item;
}
").GetAllDescendants().OfType<AstMethodDeclaration>().Single(m => m.Name.Text == "First");
            Assert.IsEmpty(md.Constraints);
        }

        // --- symbol level --------------------------------------------------------

        [Test]
        public static void FunctionWhereClause_ReachesFunctionDefDeclaredBounds()
        {
            var mix = Compile(BoundedMix).FunctionDefinitions.Single(f => f.Name == "Mix");
            CollectionAssert.AreEqual(new[] { "$T: Interpolatable" },
                mix.DeclaredBounds.Select(b => b.ToString()).ToArray());
        }

        [Test]
        public static void DeclaredBounds_JoinTheInheritedOnesUnderOneReading()
        {
            // TypeConstraints.InheritedBounds is the single channel every consumer reads — the
            // solver's licence, the emitter's where clause, and TirEmitSource's body licence — so a
            // declared bound has to arrive through it, in the same shape.
            var mix = Compile(BoundedMix).FunctionDefinitions.Single(f => f.Name == "Mix");
            var bounds = TypeConstraints.InheritedBounds(mix);
            CollectionAssert.AreEqual(new[] { "$T" }, bounds.Keys.ToArray());
            CollectionAssert.AreEqual(new[] { "Interpolatable" }, bounds["$T"].Select(b => b.Name).ToArray());
        }

        [Test]
        public static void ADeclaredBound_IsNotDuplicatedByAnIdenticalInheritedOne()
        {
            var c = Compile(Prelude + @"
type Tween<T>
    where T: Interpolatable
{
    From: T;
    To: T;
}

library Ops
{
    Sample(x: Tween<$T>, t: Number): $T where $T: Interpolatable => x.From.Lerp(x.To, t);
}
");
            var sample = c.FunctionDefinitions.Single(f => f.Name == "Sample");
            CollectionAssert.AreEqual(new[] { "Interpolatable" },
                TypeConstraints.InheritedBounds(sample)["$T"].Select(b => b.Name).ToArray(),
                "the two sources are unioned as a SET: restating an inherited bound is legal and idempotent");
        }

        // --- what the bound licenses (inside the body) ---------------------------

        [Test]
        public static void DeclaredBound_LicensesAMemberCallOnABareSignatureVariable()
        {
            CollectionAssert.IsEmpty(DiagnosticsFor(Compile(BoundedMix), "Mix").Select(d => d.ToString()).ToArray(),
                "Lerp on a bare $T is licensed by the function's own `where $T: Interpolatable`");
        }

        [Test]
        public static void ACallTheDeclaredBoundDoesNotSupply_IsStillReported()
        {
            // The licence is exactly as narrow as what the clause says: declaring Measurable does
            // not buy Lerp. Same CHK205 an inherited bound would produce, from the same channel.
            var diagnostics = DiagnosticsFor(Compile(Prelude + @"
library Ops
{
    Mix(xs: Bag<$T>, t: Number): $T where $T: Measurable
        => xs.Item.Lerp(xs.Item, t);
}
"), "Mix");
            var unlicensed = diagnostics.Where(d => d.Code == "CHK205").ToArray();
            Assert.AreEqual(1, unlicensed.Length, Show(diagnostics));
            StringAssert.Contains("Measurable", unlicensed[0].Message);
        }

        [Test]
        public static void ABoundThatDoesNotNameAConcept_IsRejected()
        {
            var codes = new TypeConstraintChecker(Compile(Prelude + @"
library Ops
{
    Mix(xs: Bag<$T>, t: Number): $T where $T: Number => xs.Item;
}
")).Check().Select(d => d.Code).ToArray();
            CollectionAssert.Contains(codes, "CHK310",
                "`where $T: Number` promises something C# cannot express as a constraint");
        }

        // --- what the bound obligates (at every call site) -----------------------

        [Test]
        public static void ACallWhoseArgumentSatisfiesTheBound_Resolves()
        {
            var diagnostics = DiagnosticsFor(Compile(BoundedMix + @"
library Callers
{
    Use(b: Bag<Point2D>, t: Number): Point2D => b.Mix(t);
}
"), "Use");
            CollectionAssert.IsEmpty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()).ToArray(), Show(diagnostics));
        }

        [Test]
        public static void ACallWhoseArgumentViolatesTheBound_IsRejectedAtTheCallSite()
        {
            // WHERE ARGUMENT SATISFACTION IS ENFORCED for a function bound: in the solver, as a
            // viability rule on the candidate — the bound is a precondition on inference, so
            // whatever the arguments bind $T to must satisfy it or the overload is not a match.
            // Reported CHK206 rather than the misleading CHK201 "no overload matches", because the
            // signature DID match and only the bound failed.
            var diagnostics = DiagnosticsFor(Compile(BoundedMix + @"
library Callers
{
    Use(b: Bag<String>, t: Number): String => b.Mix(t);
}
"), "Use");
            var rejected = diagnostics.Where(d => d.Code == "CHK206").ToArray();
            Assert.AreEqual(1, rejected.Length, Show(diagnostics));
            Assert.AreEqual(DiagnosticSeverity.Error, rejected[0].Severity);
            StringAssert.Contains("String", rejected[0].Message);
            StringAssert.Contains("Interpolatable", rejected[0].Message);
        }

        [Test]
        public static void TheBoundedFunctionsOwnRecursiveCall_StillResolves()
        {
            // The self-call passes the very variable the clause bounds, so it is judged by the
            // bounds $T is KNOWN to carry rather than by a concrete type. Without this the
            // collapsed de Casteljau — which is recursive — could not be written at all.
            var diagnostics = DiagnosticsFor(Compile(Prelude + @"
library Ops
{
    Fold(xs: Bag<$T>, t: Number): $T where $T: Interpolatable
        => xs.Item.Lerp(xs.Fold(t), t);
}
"), "Fold");
            CollectionAssert.IsEmpty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()).ToArray(), Show(diagnostics));
        }

        [Test]
        public static void AnUnboundedFunctionVariable_StaysPermissive()
        {
            // The standing scope line, restated for the new construct: bounds restrict where they
            // are declared and change nothing where they are not.
            var diagnostics = DiagnosticsFor(Compile(Prelude + @"
library Ops
{
    Mix(xs: Bag<$T>, t: Number): $T => xs.Item.Lerp(xs.Item, t);
}

library Callers
{
    Use(b: Bag<String>, t: Number): String => b.Mix(t);
}
"), "Use");
            CollectionAssert.IsEmpty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()).ToArray(), Show(diagnostics));
        }

        // --- lint ----------------------------------------------------------------

        [Test]
        public static void ABoundOnAVariableTheSignatureNeverMentions_IsLint002()
        {
            // The same trap LINT002 already catches on a type declaration: the symbol layer matches
            // bounds to variables by name, so a target that names nothing would be dropped in
            // silence. A bound that constrains nothing is an error, not a comment.
            var c = Compile(Prelude + @"
library Ops
{
    Mix(xs: Bag<$T>, t: Number): $T where $U: Interpolatable => xs.Item;
}
");
            var all = new Linter(c).Findings.ToArray();
            var findings = all.Where(f => f.Code == "LINT002").ToArray();
            Assert.AreEqual(1, findings.Length, string.Join("\n", all.Select(f => f.ToString())));
            StringAssert.Contains("$U", findings[0].Message);
        }

        [Test]
        public static void ABoundOnAVariableTheSignatureDoesMention_IsClean()
        {
            var c = Compile(BoundedMix);
            CollectionAssert.IsEmpty(new Linter(c).Findings.Where(f => f.Code == "LINT002")
                .Select(f => f.ToString()).ToArray());
        }
    }
}
