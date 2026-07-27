using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Parse-level tests for the sum-type (tagged-union) declaration form and the match expression.
    /// These assert on AST node shapes only; semantic analysis (exhaustiveness, binder arity, type
    /// resolution) is wave-2 work and is intentionally NOT exercised here. Field/case type names such
    /// as Point2D or Angle need not be declared: parsing does not resolve types.
    /// </summary>
    [TestFixture]
    public static class SumTypeParsingTests
    {
        // --- helpers -------------------------------------------------------------

        private static Parser Parse(string source)
            => CommonParsers.PlatoParser(new ParserInput(source, "test.plato"), Logger.Null);

        private static AstFile ParseOk(string source)
        {
            var parser = Parse(source);
            Assert.IsTrue(parser.Succeeded,
                $"Expected a successful parse but got errors:\n{parser.ParserErrorsString}");
            return (AstFile)parser.Cst!.ToAst();
        }

        private static AstTypeDeclaration FindType(AstNode root, string name)
            => root.GetAllDescendants().OfType<AstTypeDeclaration>().Single(t => t.Name.Text == name);

        private static AstMatch FindMatch(AstNode root)
            => root.GetAllDescendants().OfType<AstMatch>().Single();

        // --- source fixtures -----------------------------------------------------

        private const string PathSegment2D = @"
type PathSegment2D implements Value
  = Move(EndPoint: Point2D)
  | Line(EndPoint: Point2D)
  | Quadratic(Control: Point2D, EndPoint: Point2D)
  | Cubic(Control1: Point2D, Control2: Point2D, EndPoint: Point2D)
  | Arc(Radii: Vector2, AxisRotation: Angle, LargeArc: Boolean, Sweep: Boolean, EndPoint: Point2D)
  | Close;
";

        private const string FillRule = "type FillRule = NonZero | EvenOdd;";

        private const string OptionT = "type Option<T> = None | Some(Value: T);";

        // A library method whose body is the spec's match expression over a PathSegment2D.
        private const string MatchLibrary = @"
library PathOps {
    Process(segment: PathSegment2D): Integer => match (segment) {
        Move(p) => doMove(p);
        Line(p) => doLine(p);
        Quadratic(c, e) => f(c, e);
        Cubic(c1, c2, e) => g(c1, c2, e);
        Arc(radii, rot, large, sweep, e) => h(e);
        Close => k();
    };
}
";

        // A match expression used as a sub-expression (a function argument), proving it composes.
        private const string MatchNested = @"
library OptionOps {
    Unwrap(o: Option): Integer => use(match (o) { None => 0; Some(v) => v; });
}
";

        // --- sum-type declaration: PathSegment2D ---------------------------------

        [Test]
        public static void PathSegment2D_ParsesAllCasesWithCorrectArity()
        {
            var ast = ParseOk(PathSegment2D);
            var t = FindType(ast, "PathSegment2D");

            // Sum type stays a ConcreteType; cases are carried on the Cases list; no braced fields.
            Assert.AreEqual(TypeKind.ConcreteType, t.Kind);
            Assert.IsEmpty(t.Members, "a sum type has no braced field members");

            var names = t.Cases.Select(c => c.Name.Text).ToArray();
            CollectionAssert.AreEqual(
                new[] { "Move", "Line", "Quadratic", "Cubic", "Arc", "Close" }, names);

            var arity = t.Cases.ToDictionary(c => c.Name.Text, c => c.Fields.Count);
            Assert.AreEqual(1, arity["Move"]);
            Assert.AreEqual(1, arity["Line"]);
            Assert.AreEqual(2, arity["Quadratic"]);
            Assert.AreEqual(3, arity["Cubic"]);
            Assert.AreEqual(5, arity["Arc"]);
            Assert.AreEqual(0, arity["Close"], "a no-payload case has zero fields");
        }

        [Test]
        public static void PathSegment2D_CaseFieldsCarryNamesAndTypes()
        {
            var ast = ParseOk(PathSegment2D);
            var t = FindType(ast, "PathSegment2D");

            var quad = t.Cases.Single(c => c.Name.Text == "Quadratic");
            CollectionAssert.AreEqual(
                new[] { "Control", "EndPoint" }, quad.Fields.Select(f => f.Name.Text).ToArray());
            CollectionAssert.AreEqual(
                new[] { "Point2D", "Point2D" }, quad.Fields.Select(f => f.Type.Name.Text).ToArray());

            var arc = t.Cases.Single(c => c.Name.Text == "Arc");
            Assert.AreEqual("Boolean", arc.Fields.Single(f => f.Name.Text == "Sweep").Type.Name.Text);
        }

        [Test]
        public static void PathSegment2D_KeepsImplementsList()
        {
            var ast = ParseOk(PathSegment2D);
            var t = FindType(ast, "PathSegment2D");
            CollectionAssert.AreEqual(new[] { "Value" }, t.Implements.Select(i => i.Name.Text).ToArray());
        }

        // --- sum-type declaration: enum form (no payloads) -----------------------

        [Test]
        public static void FillRule_ParsesAsPayloadlessCases()
        {
            var ast = ParseOk(FillRule);
            var t = FindType(ast, "FillRule");
            CollectionAssert.AreEqual(new[] { "NonZero", "EvenOdd" }, t.Cases.Select(c => c.Name.Text).ToArray());
            Assert.IsTrue(t.Cases.All(c => c.Fields.Count == 0));
        }

        // --- sum-type declaration: generic ---------------------------------------

        [Test]
        public static void OptionT_ParsesGenericSumTypeWithTypeParameter()
        {
            var ast = ParseOk(OptionT);
            var t = FindType(ast, "Option");

            CollectionAssert.AreEqual(new[] { "T" }, t.TypeParameters.Select(p => p.Name.Text).ToArray());
            CollectionAssert.AreEqual(new[] { "None", "Some" }, t.Cases.Select(c => c.Name.Text).ToArray());

            var some = t.Cases.Single(c => c.Name.Text == "Some");
            Assert.AreEqual("Value", some.Fields.Single().Name.Text);
            Assert.AreEqual("T", some.Fields.Single().Type.Name.Text);
            Assert.AreEqual(0, t.Cases.Single(c => c.Name.Text == "None").Fields.Count);
        }

        // --- backward compatibility: ordinary braced type still parses -----------

        [Test]
        public static void BracedFieldType_StillParsesWithEmptyCases()
        {
            var ast = ParseOk("type Point2D implements Value { X: Number; Y: Number; }");
            var t = FindType(ast, "Point2D");
            Assert.IsEmpty(t.Cases, "an ordinary braced type carries no sum-type cases");
            CollectionAssert.AreEqual(new[] { "X", "Y" }, t.Members.Select(m => m.Name.Text).ToArray());
        }

        // --- match expression ----------------------------------------------------

        [Test]
        public static void Match_ParsesArmsWithPositionalBinders()
        {
            var ast = ParseOk(MatchLibrary);
            var m = FindMatch(ast);

            // Scrutinee is unwrapped from its parentheses (same as if/while conditions).
            Assert.IsInstanceOf<AstIdentifier>(m.Scrutinee);
            Assert.AreEqual("segment", ((AstIdentifier)m.Scrutinee).Text);

            var caseNames = m.Arms.Select(a => a.CaseName.Text).ToArray();
            CollectionAssert.AreEqual(
                new[] { "Move", "Line", "Quadratic", "Cubic", "Arc", "Close" }, caseNames);

            var binders = m.Arms.ToDictionary(a => a.CaseName.Text, a => a.Binders.Count);
            Assert.AreEqual(1, binders["Move"]);
            Assert.AreEqual(2, binders["Quadratic"]);
            Assert.AreEqual(3, binders["Cubic"]);
            Assert.AreEqual(5, binders["Arc"]);
            Assert.AreEqual(0, binders["Close"], "a no-payload arm has no binders");

            // Binder identifiers are captured in order.
            var cubic = m.Arms.Single(a => a.CaseName.Text == "Cubic");
            CollectionAssert.AreEqual(new[] { "c1", "c2", "e" }, cubic.Binders.Select(b => b.Text).ToArray());

            // Each arm body is a real expression (here, a call).
            Assert.IsTrue(m.Arms.All(a => a.Body is AstInvoke));
        }

        [Test]
        public static void Match_ComposesAsSubExpression()
        {
            var ast = ParseOk(MatchNested);
            // The match sits inside the argument list of an invocation.
            var m = FindMatch(ast);
            Assert.AreEqual(2, m.Arms.Count);
            CollectionAssert.AreEqual(new[] { "None", "Some" }, m.Arms.Select(a => a.CaseName.Text).ToArray());
            Assert.IsTrue(ast.GetAllDescendants().OfType<AstInvoke>().Any(),
                "the enclosing call should still be present");
        }

        // --- negative tests ------------------------------------------------------

        [Test]
        public static void EmptyCaseList_FailsToParse()
        {
            Assert.IsFalse(Parse("type X = ;").Succeeded,
                "an empty case list must be rejected");
        }

        [Test]
        public static void MissingTerminatingSemicolon_FailsToParse()
        {
            Assert.IsFalse(Parse("type FillRule = NonZero | EvenOdd").Succeeded,
                "a case list not terminated by ';' must be rejected");
        }
    }
}
