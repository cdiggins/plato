using System.Linq;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// The TirComponentUnroller generic-body fan-out (--optimize): the monomorphized ArrayLibrary
    /// bodies over fixed-arity IArrayLike types — whose HOF argument is the body's own delegate
    /// parameter — unroll to direct field expressions instead of falling through to the loop
    /// lowerer: constructor consumers become `new T(f.Invoke(a.A, b.A), ...)`, boolean consumers
    /// become &amp;&amp; / || chains, and Reverse reverses the field order. Guards: sources that
    /// are not parameter reads still loop-lower, and everything stays put with --optimize off.
    /// Semantics are covered by the conformance suite; this pins the emitted shapes.
    /// </summary>
    [TestFixture]
    public static class ComponentUnrollerTests
    {
        private static CSharpWriter _optimized;
        private static CSharpWriter Optimized => _optimized ?? (_optimized = Build(optimize: true));

        private static CSharpWriter Build(bool optimize)
        {
            var w = new CSharpWriter(CheckerTestSupport.CompileStdLib(), "unused-component-unroller")
            {
                ExtensionStyle = true,
                ScalarErase = true,
                NoProperties = true,
                Optimize = optimize,
                OptimizeArrays = true,
                InlineCalls = true,
                LowerLoops = true,
            };
            w.WriteAll("float");
            return w;
        }

        private static string ExtractLine(CSharpWriter w, string file, string marker)
        {
            Assert.IsTrue(w.Files.ContainsKey(file), $"no generated file '{file}'");
            var lines = w.Files[file].ToString().Replace("\r\n", "\n").Split('\n');
            var matches = lines.Where(l => l.Contains(marker)).ToList();
            Assert.AreEqual(1, matches.Count, $"marker '{marker}' matched {matches.Count} line(s) in {file} (must be unique)");
            return matches[0].Trim();
        }

        [TestCase("Integer2 ZipComponents(this Integer2 a, Integer2 b, System.Func<int, int, int> f)",
            "new Ara3D.Geometry.Integer2(f.Invoke(a.A, b.A), f.Invoke(a.B, b.B))")]
        [TestCase("Integer2 ZipComponents(this Integer2 a, Integer2 b, Integer2 c, System.Func<int, int, int, int> f)",
            "new Ara3D.Geometry.Integer2(f.Invoke(a.A, b.A, c.A), f.Invoke(a.B, b.B, c.B))")]
        [TestCase("Integer2 MapComponents(this Integer2 x, System.Func<int, int> f)",
            "new Ara3D.Geometry.Integer2(f.Invoke(x.A), f.Invoke(x.B))")]
        [TestCase("bool AllZipComponents(this Integer2 a, Integer2 b, System.Func<int, int, bool> f)",
            "f.Invoke(a.A, b.A) && f.Invoke(a.B, b.B)")]
        [TestCase("bool AnyZipComponents(this Integer2 a, Integer2 b, System.Func<int, int, bool> f)",
            "f.Invoke(a.A, b.A) || f.Invoke(a.B, b.B)")]
        [TestCase("bool AllComponents(this Integer2 x, System.Func<int, bool> predicate)",
            "predicate.Invoke(x.A) && predicate.Invoke(x.B)")]
        [TestCase("bool AnyComponent(this Integer2 x, System.Func<int, bool> predicate)",
            "predicate.Invoke(x.A) || predicate.Invoke(x.B)")]
        [TestCase("Integer2 Reverse(this Integer2 xs)",
            "new Ara3D.Geometry.Integer2(xs.B, xs.A)")]
        public static void FixedArityBodiesUnrollToFieldExpressions(string marker, string expectedBody)
        {
            var line = ExtractLine(Optimized, "ArrayLibrary.g.cs", marker);
            StringAssert.Contains(expectedBody, line);
            StringAssert.Contains("=>", line); // a one-line expression body, not a lowered loop
        }

        [Test]
        public static void ThreeComponentBodiesFanOutAllFields()
        {
            var line = ExtractLine(Optimized, "ArrayLibrary.g.cs",
                "Integer3 ZipComponents(this Integer3 a, Integer3 b, System.Func<int, int, int> f)");
            StringAssert.Contains("new Ara3D.Geometry.Integer3(f.Invoke(a.A, b.A), f.Invoke(a.B, b.B), f.Invoke(a.C, b.C))", line);
        }

        [Test]
        public static void GenuinelyDynamicArraysStillLoopLower()
        {
            // A combinator over a genuinely DYNAMIC array (a mesh's point/face lists — not a
            // fixed-size component vector, a fixed array literal, or a cheap projection) must still
            // loop-lower; the fan-out / cheap-projection relaxations must not over-fire on it.
            var text = Optimized.Files["Meshes.g.cs"].ToString();
            StringAssert.Contains("for (var _i", text);
        }

        [Test]
        public static void OptimizeOffLeavesBodiesToTheLoopLowerer()
        {
            var off = Build(optimize: false);
            var text = off.Files["ArrayLibrary.g.cs"].ToString();
            StringAssert.Contains("Integer2 ZipComponents(this Integer2 a, Integer2 b, System.Func<int, int, int> f)", text);
            StringAssert.Contains("Integer2.CreateFromComponents(", text);
            StringAssert.DoesNotContain("new Ara3D.Geometry.Integer2(f.Invoke", text);
        }
    }
}
