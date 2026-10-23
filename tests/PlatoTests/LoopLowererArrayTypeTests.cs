using System;
using System.IO;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// TKT-505: the loop lowerer (--loops) once accepted a receiver or result only when its type was
    /// named <c>IArray</c>, stdlib-legacy's spelling, so a program written against the current
    /// stdlib's <c>Array&lt;T&gt;</c> lowered nothing (the STEP mesher's 1,496 generated files had
    /// no <c>for</c> loop). This compiles a small program over <c>Array&lt;T&gt;</c> alone, with the
    /// flag on, and asserts that each of MapRange, Map, Reduce and Any becomes a <c>for</c> loop.
    /// </summary>
    [TestFixture]
    public static class LoopLowererArrayTypeTests
    {
        // Self-contained, in the style of ArrayReceiverLibraryEmissionTests: no System.Numerics names.
        private const string Source = @"
type Number { }
type Integer { }
type Boolean { }
type Object { }

type Function0<TR> { }
type Function1<T0, TR> { }
type Function2<T0, T1, TR> { }

type Array<T> { }

library Intrinsics
{
    Count(xs: Array<$T>): Integer;
    At(xs: Array<$T>, n: Integer): $T;
    MapRange(n: Integer, f: Function1<Integer, $T>): Array<$T>;
    Reduce(xs: Array<$T>, acc: $U, f: Function2<$U, $T, $U>): $U;
    Map(xs: Array<$T>, f: Function1<$T, $U>): Array<$U>;
    Any(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean;
    Same(a: Number, b: Number): Boolean;
}

type Pt
{
    X: Number;
    Y: Number;
}

library Kernels
{
    Xs(points: Array<Pt>): Array<Number> => points.Map(p => p.X);
    Pts(xs: Array<Number>): Array<Pt> => MapRange(xs.Count, i => new Pt(xs.At(i), xs.At(i)));
    LastX(points: Array<Pt>): Number => points.Reduce(points.At(0).X, (acc, p) => p.X);
    HasX(points: Array<Pt>, x: Number): Boolean => points.Any(p => p.X.Same(x));
}
";

        private static string EmitKernels()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-loops-array-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-loops-array") { LowerLoops = true };
                w.WriteAll();
                Assert.IsTrue(w.Files.ContainsKey("Extensions.g.cs"), "no generated Extensions.g.cs");
                var text = w.Files["Extensions.g.cs"].ToString();
                TestContext.WriteLine(text);
                return text;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        /// <summary>The emitted body of one Kernels function: from its signature line to the next
        /// method's signature or the end of the class.</summary>
        private static string BodyOf(string text, string name)
        {
            var m = Regex.Match(text, $@"\b{name}\(.*?\n(.*?)(?=\n\s*public static |\n\s*\}}\s*$)", RegexOptions.Singleline);
            Assert.IsTrue(m.Success, $"no emitted body for {name}");
            return m.Value;
        }

        [TestCase("Xs", "Map")]
        [TestCase("Pts", "MapRange")]
        [TestCase("LastX", "Reduce")]
        [TestCase("HasX", "Any")]
        public static void CombinatorOverArrayLowersToForLoop(string fn, string combinator)
        {
            var body = BodyOf(EmitKernels(), fn);
            Assert.That(body, Does.Contain("for (var _i"),
                $"{combinator} over Array<T> was not lowered to a for loop in {fn}");
            Assert.That(body, Does.Not.Contain($".{combinator}("),
                $"{fn} still calls {combinator} after lowering");
        }
    }
}
