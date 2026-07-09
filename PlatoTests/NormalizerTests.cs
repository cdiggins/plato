using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Synthetic (parser-free) unit tests for the normalization pass and its invariant verifier.
    /// These construct small Symbol trees by hand so they exercise the rewrite rules in isolation.
    /// The stdlib-level "runs over real code" check lives in <see cref="CheckerIntegrationTests"/>.
    /// </summary>
    [TestFixture]
    public static class NormalizerTests
    {
        // --- small symbol-graph builders ----------------------------------------

        private static TypeDef ConcreteType(string name)
            => new TypeDef(null, TypeKind.ConcreteType, name);

        private static TypeExpression Type(string name)
            => ConcreteType(name).ToTypeExpression();

        private static Literal Int(int v)
            => new Literal(LiteralTypesEnum.Integer, v);

        /// <summary>A library function group of one intrinsic function with the given arity.</summary>
        private static FunctionGroupRefSymbol Group(string name, int arity)
        {
            var lib = new TypeDef(null, TypeKind.Library, "Lib");
            var ret = Type("Number");
            var ps = Enumerable.Range(0, arity)
                .Select(i => new ParameterDef(null, $"p{i}", Type("Number"), i))
                .ToArray();
            var f = new FunctionDef(null, name, lib, ret, null, ps);
            var g = new FunctionGroupDef(null, ret, new[] { f }, name);
            return (FunctionGroupRefSymbol)g.ToReference();
        }

        private static string Shape(Symbol s)
            => string.Join(",", s.GetSymbolTree().Select(x => x.GetType().Name));

        // --- R1: parentheses -----------------------------------------------------

        [Test]
        public static void StripsParentheses()
        {
            var inner = Int(1);
            var expr = new Parenthesized(inner);
            var n = new Normalizer().Normalize(expr);
            Assert.IsInstanceOf<Literal>(n);
            Assert.IsEmpty(NormalizationInvariants.Check(n));
        }

        [Test]
        public static void NestedParenthesesFullyStripped()
        {
            var expr = new Parenthesized(new Parenthesized(new Parenthesized(Int(1))));
            var n = new Normalizer().Normalize(expr);
            Assert.IsInstanceOf<Literal>(n);
        }

        // --- R2: single-element MultiStatement -----------------------------------

        [Test]
        public static void UnwrapsSingletonMultiStatement()
        {
            var ret = new ReturnStatement(Int(1));
            var multi = new MultiStatement(ret);
            var n = new Normalizer().Normalize(multi);
            Assert.IsInstanceOf<ReturnStatement>(n);
            Assert.IsEmpty(NormalizationInvariants.Check(n));
        }

        [Test]
        public static void KeepsMultiElementMultiStatement()
        {
            var multi = new MultiStatement(new ReturnStatement(Int(1)), new ReturnStatement(Int(2)));
            var n = new Normalizer().Normalize(multi);
            Assert.IsInstanceOf<MultiStatement>(n);
        }

        // --- R3: eta-expansion of function-group values --------------------------

        [Test]
        public static void EtaExpandsGroupRefInValuePosition()
        {
            var g = Group("Square", 1);
            var n = new Normalizer().Normalize(g);

            Assert.IsInstanceOf<Lambda>(n, "a bare group reference should become a lambda");
            var lam = (Lambda)n;
            Assert.AreEqual(1, lam.Function.NumParameters);

            // the lambda body must forward to the same group as a callee
            var call = lam.Function.Body as FunctionCall;
            Assert.NotNull(call);
            Assert.IsInstanceOf<FunctionGroupRefSymbol>(call.Function);
            Assert.IsEmpty(NormalizationInvariants.Check(n));
        }

        [Test]
        public static void DoesNotEtaExpandCalleePosition()
        {
            var g = Group("Square", 1);
            var call = new FunctionCall(g, true, Int(3));
            var n = (FunctionCall)new Normalizer().Normalize(call);

            Assert.IsInstanceOf<FunctionGroupRefSymbol>(n.Function,
                "a group reference in callee position must stay a group reference");
            Assert.IsEmpty(NormalizationInvariants.Check(n));
        }

        [Test]
        public static void EtaExpandsGroupRefUsedAsArgument()
        {
            // Higher-order call: Map(xs, Square)  where Square is a bare group reference.
            var map = Group("Map", 2);
            var square = Group("Square", 1);
            var xs = Int(0); // stand-in receiver/collection
            var call = new FunctionCall(map, true, xs, square);

            var n = (FunctionCall)new Normalizer().Normalize(call);
            Assert.IsInstanceOf<FunctionGroupRefSymbol>(n.Function); // Map stays the callee
            Assert.IsInstanceOf<Lambda>(n.Args[1]);                  // Square becomes a lambda
            Assert.IsEmpty(NormalizationInvariants.Check(n));
        }

        [Test]
        public static void ZeroArityGroupNotEtaExpanded()
        {
            // A zero-parameter group behaves as a value (e.g. a constant/property), not a function.
            var g = Group("Zero", 0);
            var n = new Normalizer().Normalize(g);
            Assert.IsInstanceOf<FunctionGroupRefSymbol>(n);
        }

        // --- idempotence & invariant contract ------------------------------------

        [Test]
        public static void NormalizationIsIdempotent()
        {
            var g = Group("Square", 1);
            var tree = new FunctionCall(Group("Map", 2), true,
                new Parenthesized(Int(0)),
                g);

            var norm = new Normalizer();
            var once = norm.Normalize(tree);
            var twice = new Normalizer().Normalize(once);
            Assert.AreEqual(Shape(once), Shape(twice), "second normalization pass must be a no-op");
            Assert.IsEmpty(NormalizationInvariants.Check(once));
        }

        [Test]
        public static void VerifierFlagsUnnormalizedTree()
        {
            var withParen = new Parenthesized(Int(1));
            var violations = NormalizationInvariants.Check(withParen);
            Assert.IsNotEmpty(violations);
            Assert.AreEqual("NORM1", violations[0].Code);
        }
    }
}
