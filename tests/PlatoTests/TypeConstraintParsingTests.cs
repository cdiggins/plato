using System;
using System.IO;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Front-end tests for declared bounds on a CONCRETE type's parameters (plato-382):
    /// "type Tween&lt;T&gt; where T: Interpolatable implements TimeVarying&lt;T&gt; { ... }".
    /// The `where` clause sits between the type parameters and `implements`, matching the position
    /// it already occupies on `concept`.
    ///
    /// Scope is the front end only: these assert that the clause PARSES, reaches
    /// <see cref="AstTypeDeclaration.Constraints"/>, and is copied into
    /// <c>TypeParameterDef.Constraints</c> by the symbol factory. What the checker then DOES with
    /// the bound — reject an unsatisfying type argument, license a member call on a bare parameter —
    /// is <see cref="TypeConstraintCheckingTests"/>.
    /// </summary>
    [TestFixture]
    public static class TypeConstraintParsingTests
    {
        // --- helpers -------------------------------------------------------------

        private static Parser Parse(string source)
            => CommonParsers.PlatoParser(new ParserInput(source, "test.plato"), Ara3D.Logging.Logger.Null);

        private static AstFile ParseOk(string source)
        {
            var parser = Parse(source);
            Assert.IsTrue(parser.Succeeded,
                $"Expected a successful parse but got errors:\n{parser.ParserErrorsString}");
            return (AstFile)parser.Cst!.ToAst();
        }

        private static AstTypeDeclaration FindType(AstNode root, string name)
            => root.GetAllDescendants().OfType<AstTypeDeclaration>().Single(t => t.Name.Text == name);

        /// <summary>Compiles a source snippet through the symbol factory, the same temp-folder route
        /// the linter fixtures use, and hands back the resulting <see cref="Compilation"/>.</summary>
        private static Compilation Compile(string source)
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato382-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), source);
                return CheckerTestSupport.CompileFolder(dir);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        // --- source fixtures -----------------------------------------------------

        // The motivating shape: one bound parameter, a bound, and an implements list after it.
        private const string TweenSource = @"
type Number { }
type Boolean { }
type Object { }
type Duration { }

concept Interpolatable
{
    Lerp(a: Self, b: Self, t: Number): Self;
}

concept TimeVarying<TValue>
{
    Sample(x: Self, time: Duration): TValue;
}

type Tween<T>
    where T: Interpolatable
    implements TimeVarying<T>
{
    From: T;
    To: T;
}
";

        // Two parameters, and one parameter carrying two bounds — the comma-separated form the
        // ConstraintList rule already accepts for concepts.
        private const string MultiConstraintSource = @"
type Number { }
type Boolean { }
type Object { }

concept Interpolatable { Lerp(a: Self, b: Self, t: Number): Self; }
concept Measurable { Magnitude(x: Self): Number; }
concept Orderable { Compare(a: Self, b: Self): Number; }

type Pair<A, B>
    where A: Interpolatable, A: Measurable, B: Orderable
{
    First: A;
    Second: B;
}
";

        // --- parse level ---------------------------------------------------------

        [Test]
        public static void TypeWhereClause_ParsesAndReachesTheAst()
        {
            var t = FindType(ParseOk(TweenSource), "Tween");

            CollectionAssert.AreEqual(new[] { "T" }, t.TypeParameters.Select(p => p.Name.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "T" }, t.Constraints.Select(c => c.Name.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "Interpolatable" },
                t.Constraints.Select(c => c.Constraint.Name.Text).ToArray());
        }

        [Test]
        public static void TypeWhereClause_DoesNotDisturbTheImplementsList()
        {
            var t = FindType(ParseOk(TweenSource), "Tween");
            CollectionAssert.AreEqual(new[] { "TimeVarying" }, t.Implements.Select(i => i.Name.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "From", "To" }, t.Members.Select(m => m.Name.Text).ToArray());
        }

        [Test]
        public static void TypeWithoutWhereClause_StillCarriesNoConstraints()
        {
            var t = FindType(ParseOk("type Point2D implements Value { X: Number; Y: Number; }"), "Point2D");
            Assert.IsEmpty(t.Constraints, "a type declared without a where clause has no bounds");
        }

        [Test]
        public static void MultipleConstraints_AreListedOnePerBound()
        {
            var t = FindType(ParseOk(MultiConstraintSource), "Pair");

            // One AstConstraint per (parameter, bound) pair — a parameter with two bounds appears twice.
            CollectionAssert.AreEqual(new[] { "A", "A", "B" }, t.Constraints.Select(c => c.Name.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "Interpolatable", "Measurable", "Orderable" },
                t.Constraints.Select(c => c.Constraint.Name.Text).ToArray());
        }

        // --- symbol level --------------------------------------------------------

        [Test]
        public static void TypeWhereClause_ReachesTypeParameterDefConstraints()
        {
            var compilation = Compile(TweenSource);
            CollectionAssert.IsEmpty(compilation.SymbolFactory.Errors.Select(e => e.ToString()).ToArray(),
                "the fixture must resolve cleanly or the assertion below would pass vacuously");

            var tp = compilation.TypeDefinitionsByName["Tween"].TypeParameters.Single();
            Assert.AreEqual("T", tp.Name);
            CollectionAssert.AreEqual(new[] { "Interpolatable" },
                tp.Constraints.Select(c => c.Name).ToArray());
        }

        [Test]
        public static void MultipleConstraints_AreGroupedOntoTheirOwnTypeParameter()
        {
            var compilation = Compile(MultiConstraintSource);
            var typeParameters = compilation.TypeDefinitionsByName["Pair"].TypeParameters;

            CollectionAssert.AreEqual(new[] { "A", "B" }, typeParameters.Select(p => p.Name).ToArray());
            CollectionAssert.AreEqual(new[] { "Interpolatable", "Measurable" },
                typeParameters[0].Constraints.Select(c => c.Name).ToArray());
            CollectionAssert.AreEqual(new[] { "Orderable" },
                typeParameters[1].Constraints.Select(c => c.Name).ToArray());
        }
    }
}
