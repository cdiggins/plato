using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Fast in-proc TIR-shape assertions for the inliner (M0 of the optimizer completion plan). The
    /// stdlib is compiled once (cached by <see cref="CheckerTestSupport"/>); a writer configured for
    /// the V2 recipe fetches the ground input TIR of flagship functions and runs
    /// <see cref="TirInliner.Inline"/> in-process — seconds, versus the ~60s regen+build+test
    /// conformance cycle which stays the milestone-end gate. Pins that the delegate indirection in
    /// the Transform family actually collapses.
    /// </summary>
    [TestFixture]
    public static class InlinerTests
    {
        // Reusable matchers over the Typed IR.

        /// <summary>Count of descendant nodes (inclusive) of a given kind.</summary>
        public static int CountNodes<T>(TirNode n) where T : TirNode
            => n == null ? 0 : n.Descendants().Count(x => x is T);

        private static TirNode Strip(TirNode n) => TirRewrite.StripCoerce(n);

        /// <summary>No residual delegate application: every <see cref="TirInvoke"/> whose target is a
        /// parameter or a lambda literal should have inlined/β-reduced away.</summary>
        public static void AssertNoDelegateInvoke(TirNode n)
        {
            var offenders = (n?.Descendants() ?? Enumerable.Empty<TirNode>())
                .OfType<TirInvoke>()
                .Where(inv => Strip(inv.Target) is TirParameter || Strip(inv.Target) is TirLambda)
                .ToList();
            Assert.IsEmpty(offenders, "residual delegate TirInvoke(s) after inlining");
        }

        /// <summary>Fully collapsed: no delegate invoke and no residual combinator call
        /// (Map/Reduce/Zip/...) left in the tree.</summary>
        public static void AssertCollapsed(TirNode n)
        {
            AssertNoDelegateInvoke(n);
            var combinators = new HashSet<string> { "Map", "MapEager", "Reduce", "Zip", "Zip2", "Zip3", "MapIdx" };
            var residual = (n?.Descendants() ?? Enumerable.Empty<TirNode>())
                .OfType<TirCall>()
                .Where(c => c.Name != null && combinators.Contains(c.Name))
                .ToList();
            Assert.IsEmpty(residual, "residual combinator call(s) after inlining");
        }

        private static CSharpWriter V2Writer()
        {
            var w = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-inliner")
            {
                ExtensionStyle = true, ScalarErase = true, Optimize = true, OptimizeArrays = true,
                InlineCalls = true, NoProperties = true, LowerLoops = true,
            };
            // Builds the extension plans + moved-member tables the inliner's emittability checks read.
            w.WriteAll("float");
            return w;
        }

        private static TirFunction Inline(CSharpWriter w, string type, string fn)
        {
            var input = w.TestGetGroundTir(type, fn);
            Assert.NotNull(input, $"no ground TIR for {type}.{fn}");
            return TirInliner.Inline(input, w, type, out _);
        }

        [Test]
        public static void PointTransformFullyBetaReduces()
        {
            // Point3D.Transform: Deform(self, p => ...) collapses to a bare
            // Transform(Vector3(self), Matrix(t)) — no delegate, no combinator.
            var w = V2Writer();
            var t = Inline(w, "Point3D", "Transform");
            AssertCollapsed(t.Body);
        }

        [Test]
        public static void BoundsTransformDropsDelegate()
        {
            // Bounds3D.Transform: the Deform delegate is inlined; the Map survives (lowered to a
            // loop only at L10, not run here) but no delegate Invoke may remain.
            var w = V2Writer();
            var t = Inline(w, "Bounds3D", "Transform");
            AssertNoDelegateInvoke(t.Body);
        }

        [Test]
        public static void TriangleTransformRewritesToConstructor()
        {
            // M1 + cheap-projection relaxation: Triangle3D.Transform's tuple body (f(A), f(B),
            // f(C)) collapses to new Triangle3D(...). The lambda reads p.X/p.Y/p.Z, so β-reduction
            // binds p := self.A used 3x; because self.A is a pure FIELD projection over a cheap
            // receiver, IsCheapProjection permits the duplication (3 field loads, no call/alloc),
            // the IIFE β-reduces away, and the ctor rewrite fires — no delegate, no tuple, no
            // Deform.
            var w = V2Writer();
            var t = Inline(w, "Triangle3D", "Transform");
            AssertNoDelegateInvoke(t.Body);
            Assert.Greater(CountNodes<TirConstructorCall>(t.Body), 0,
                "expected a `new Triangle3D(...)` constructor rewrite");
            var tup = (t.Body?.Descendants() ?? Enumerable.Empty<TirNode>())
                .OfType<TirCall>().Count(c => c.Name != null && c.Name.StartsWith("Tuple"));
            Assert.AreEqual(0, tup, "no residual Tuple_N call should remain");
        }

        [Test]
        public static void MeshTransformRewritesToConstructor()
        {
            // M1: TriangleMesh3D.Transform's tuple-returning Deform body -- (points.Map(f),
            // FaceIndices(self)) -- collapses to new TriangleMesh3D(...): a TirConstructorCall, not
            // a bare tuple, with no delegate invoke and no residual Deform. Here the delegate f is a
            // Map combinator argument (β-reduces cleanly, no per-element duplication).
            var w = V2Writer();
            var t = Inline(w, "TriangleMesh3D", "Transform");
            AssertNoDelegateInvoke(t.Body);
            Assert.Greater(CountNodes<TirConstructorCall>(t.Body), 0,
                "expected a `new TriangleMesh3D(...)` constructor rewrite");
            var tuples = (t.Body?.Descendants() ?? Enumerable.Empty<TirNode>())
                .OfType<TirCall>().Count(c => c.Name != null && c.Name.StartsWith("Tuple"));
            Assert.AreEqual(0, tuples, "no residual Tuple_N call should remain");
            var deform = (t.Body?.Descendants() ?? Enumerable.Empty<TirNode>())
                .OfType<TirCall>().Count(c => c.Name == "Deform");
            Assert.AreEqual(0, deform, "the Deform call should be inlined away");
        }
    }
}
