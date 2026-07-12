using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.AST;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Unit tests for the shared TIR structural primitives (<see cref="TirRewrite"/>): parameter
    /// substitution and β-reduction of an immediately-applied lambda. These build synthetic TIR
    /// directly — no compilation — to pin the substitution mechanics that the inliner and the
    /// upcoming loop-body / delegate specializers all rest on.
    /// </summary>
    [TestFixture]
    public static class TirRewriteTests
    {
        // --- tiny synthetic-TIR builders (types/origins are irrelevant to structural rewriting) ---

        private static ParameterDef Param(string name, int index = 0)
            => new ParameterDef(null, name, null, index);

        private static TirParameter Ref(ParameterDef def)
            => new TirParameter(def, null, null);

        private static TirLiteral Int(int v)
            => new TirLiteral(v, LiteralTypesEnum.Integer, null, null);

        // A non-cheap node (an opaque call): substituting it more than once would duplicate work.
        private static TirCall Call(string name, params TirNode[] args)
            => new TirCall(null, EmissionKind.StaticMethod, null, null, args.ToList(), null, null, name);

        private static TirLambda Lambda(TirNode body, params ParameterDef[] ps)
            => new TirLambda(ps.ToList(), body, null, null);

        private static TirInvoke Invoke(TirNode target, params TirNode[] args)
            => new TirInvoke(target, args.ToList(), null, null);

        // --- Substitute -------------------------------------------------------

        [Test]
        public static void Substitute_ReplacesMatchingParameterAndLeavesOthers()
        {
            var x = Param("x");
            var y = Param("y");
            var arg = Int(42);
            var body = Call("f", Ref(x), Ref(y));

            var result = (TirCall)TirRewrite.Substitute(body, new Dictionary<ParameterDef, TirNode> { [x] = arg });

            Assert.AreSame(arg, result.Args[0], "x should be replaced by the argument node");
            Assert.IsInstanceOf<TirParameter>(result.Args[1]);
            Assert.AreSame(y, ((TirParameter)result.Args[1]).Def, "y is not in the map and stays");
        }

        [Test]
        public static void Substitute_RecursesIntoNestedStructure()
        {
            var x = Param("x");
            var arg = Int(7);
            var body = Call("outer", Call("inner", Ref(x)));

            var result = (TirCall)TirRewrite.Substitute(body, new Dictionary<ParameterDef, TirNode> { [x] = arg });

            var inner = (TirCall)result.Args[0];
            Assert.AreSame(arg, inner.Args[0]);
        }

        [Test]
        public static void Substitute_EmptyMapReturnsBodyUnchanged()
        {
            var body = Call("f", Int(1));
            Assert.AreSame(body, TirRewrite.Substitute(body, new Dictionary<ParameterDef, TirNode>()));
        }

        // --- TryBetaReduce ----------------------------------------------------

        [Test]
        public static void BetaReduce_AppliesLambdaToArgument()
        {
            var x = Param("x");
            var lam = Lambda(Ref(x), x);             // (x) => x
            var arg = Int(99);

            Assert.IsTrue(TirRewrite.TryBetaReduce(Invoke(lam, arg), out var reduced));
            Assert.AreSame(arg, reduced, "(x => x)(99) reduces to 99");
        }

        [Test]
        public static void BetaReduce_CheapArgUsedTwiceIsAllowed()
        {
            var x = Param("x");
            var lam = Lambda(Call("add", Ref(x), Ref(x)), x);   // (x) => add(x, x)
            var arg = Int(3);                                    // cheap literal: dup is free

            Assert.IsTrue(TirRewrite.TryBetaReduce(Invoke(lam, arg), out var reduced));
            var call = (TirCall)reduced;
            Assert.AreSame(arg, call.Args[0]);
            Assert.AreSame(arg, call.Args[1]);
        }

        [Test]
        public static void BetaReduce_NonCheapArgUsedTwiceIsRefused()
        {
            var x = Param("x");
            var lam = Lambda(Call("add", Ref(x), Ref(x)), x);   // uses x twice
            var arg = Call("expensive");                        // non-cheap: would duplicate work

            Assert.IsFalse(TirRewrite.TryBetaReduce(Invoke(lam, arg), out var reduced));
            Assert.IsNull(reduced);
        }

        [Test]
        public static void BetaReduce_NonCheapArgUsedOnceIsAllowed()
        {
            var x = Param("x");
            var lam = Lambda(Call("wrap", Ref(x)), x);          // uses x once
            var arg = Call("expensive");

            Assert.IsTrue(TirRewrite.TryBetaReduce(Invoke(lam, arg), out var reduced));
            Assert.AreSame(arg, ((TirCall)reduced).Args[0]);
        }

        [Test]
        public static void BetaReduce_SeesThroughCoercedLambdaTarget()
        {
            var x = Param("x");
            var lam = Lambda(Ref(x), x);
            var coerced = new TirCoerce(lam, null, null, null, null);   // delegate target-typing
            var arg = Int(5);

            Assert.IsTrue(TirRewrite.TryBetaReduce(Invoke(coerced, arg), out var reduced));
            Assert.AreSame(arg, reduced);
        }

        [Test]
        public static void BetaReduce_NonLambdaTargetIsRefused()
        {
            var f = Param("f");
            Assert.IsFalse(TirRewrite.TryBetaReduce(Invoke(Ref(f), Int(1)), out var reduced));
            Assert.IsNull(reduced);
        }

        [Test]
        public static void BetaReduce_ArityMismatchIsRefused()
        {
            var x = Param("x");
            var lam = Lambda(Ref(x), x);                        // one parameter
            Assert.IsFalse(TirRewrite.TryBetaReduce(Invoke(lam, Int(1), Int(2)), out _));
        }

        // --- small primitives -------------------------------------------------

        [Test]
        public static void IsCheap_ClassifiesLeavesAndSeesThroughCoercions()
        {
            Assert.IsTrue(TirRewrite.IsCheap(Int(1)));
            Assert.IsTrue(TirRewrite.IsCheap(Ref(Param("x"))));
            Assert.IsTrue(TirRewrite.IsCheap(new TirCoerce(Int(1), null, null, null, null)));
            Assert.IsFalse(TirRewrite.IsCheap(Call("f")));
        }

        [Test]
        public static void CountParamUses_CountsAndFlagsUnderLambda()
        {
            var x = Param("x");
            // add(x, (unused) => x)  -> x used twice, one occurrence under a lambda
            var body = Call("add", Ref(x), Lambda(Ref(x)));
            var uses = TirRewrite.CountParamUses(body);

            Assert.AreEqual(2, uses[x].Count);
            Assert.IsTrue(uses[x].UnderLambda);
        }
    }
}
