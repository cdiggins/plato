// Checks the --switch-match switch (TKT-1148): a match on a sum with three or more tag tests is a
// C# switch expression on Kind. Off, the output is the tag-test ternary chain it always was.
using System.IO;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;
using static PlatoTests.GeneratedValueReflection;
using PlatoCompilation = Ara3D.Geometry.Compiler.Compilation;

namespace PlatoTests
{
    [TestFixture]
    public static class SwitchMatchTests
    {
        private static CSharpWriter Emit(string file, bool switchMatch)
        {
            var ast = CheckerTestSupport.ParseFile(Path.Combine(CheckerTestSupport.FindFolder("plato-test-sum"), file));
            var comp = new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast });
            var w = new CSharpWriter(comp, "unused-switch-match") { ExtensionStyle = true, SwitchMatch = switchMatch };
            w.WriteAll();
            return w;
        }

        [Test]
        public static void Off_KeepsTheTernaryChain()
        {
            var lib = Emit("pathsegment.plato", false).Files["PathSegments.g.cs"].ToString();
            Assert.That(lib, Does.Not.Contain("switch"));
            Assert.That(lib, Does.Contain("seg.IsMove() ? seg.Move_EndPoint"));
        }

        [Test]
        public static void Off_IsIdenticalToTheDefault()
        {
            var plain = Emit("pathsegment.plato", false);
            var ast = CheckerTestSupport.ParseFile(Path.Combine(CheckerTestSupport.FindFolder("plato-test-sum"), "pathsegment.plato"));
            var untouched = new CSharpWriter(new PlatoCompilation(Ara3D.Logging.Logger.Null, new[] { ast }), "unused-switch-match")
                { ExtensionStyle = true };
            untouched.WriteAll();
            Assert.That(plain.Files.Keys, Is.EquivalentTo(untouched.Files.Keys));
            foreach (var (name, text) in plain.Files)
                Assert.That(text.ToString(), Is.EqualTo(untouched.Files[name].ToString()), name);
        }

        [Test]
        public static void On_WritesASwitchOnTheTagWithTheSubjectOnce()
        {
            var lib = Emit("pathsegment.plato", true).Files["PathSegments.g.cs"].ToString();
            Assert.That(lib, Does.Contain("seg.Kind switch {"));
            Assert.That(lib, Does.Contain("PathSegment2D.Kind_Move => seg.Move_EndPoint"));
            Assert.That(lib, Does.Contain("_ => start"));
            Assert.That(lib, Does.Not.Contain("seg.IsMove()"));
        }

        [Test]
        public static void On_CompilesAndComputesTheSameValues()
        {
            var results = new List<object[]>();
            foreach (var on in new[] { false, true })
            {
                var w = Emit("pathsegment.plato", on);
                var asm = CSharpTestCompilation.CompileAndLoad(SumTypeCodegenTests.Prelude,
                    w.Files["_PathSegment2D.g.cs"].ToString(), w.Files["PathSegments.g.cs"].ToString());
                var seg = asm.GetType("Ara3D.Geometry.PathSegment2D", true);
                var pt = asm.GetType("Ara3D.Geometry.Point2D", true);
                var lib = asm.GetType("Ara3D.Geometry.PathSegments", true);
                object Point(float x, float y) => Activator.CreateInstance(pt, x, y);
                var segments = new[]
                {
                    seg.GetMethod("Move").Invoke(null, new[] { Point(1, 2) }),
                    seg.GetMethod("Line").Invoke(null, new[] { Point(3, 4) }),
                    seg.GetMethod("Quadratic").Invoke(null, new[] { Point(7, 8), Point(5, 6) }),
                    seg.GetMethod("Cubic").Invoke(null, new[] { Point(0, 0), Point(0, 0), Point(11, 12) }),
                    seg.GetMethod("Close").Invoke(null, null),
                };
                var endPoint = lib.GetMethod("EndPoint");
                var count = lib.GetMethod("ControlPointCount");
                var row = new List<object>();
                foreach (var s in segments)
                {
                    var p = endPoint.Invoke(null, new[] { s, Point(9, 9) });
                    row.Add((pt.GetField("X").GetValue(p), pt.GetField("Y").GetValue(p), Unwrap(count.Invoke(null, new[] { s }))));
                }
                results.Add(row.ToArray());
            }
            Assert.That(results[1], Is.EqualTo(results[0]));
            Assert.That(results[1], Has.Length.EqualTo(5));
        }
    }
}
