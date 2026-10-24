// Checks fusion of array combinators into equivalent generated loops.
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// TKT-506: with <c>--fuse --loops</c>, a chain of array combinators over <c>Array&lt;T&gt;</c>
    /// compiles to ONE <c>for</c> loop with no intermediate array: <c>n.Range</c> feeds its
    /// consumer as a loop index, <c>Map</c> after <c>Map</c> or <c>Take</c> is one loop, and a
    /// <c>Map</c> before <c>Reduce</c> folds in one pass. Each case compiles a small program and
    /// counts the loops and the array allocations in the emitted C#.
    /// </summary>
    [TestFixture]
    public static class CombinatorFusionTests
    {
        // Self-contained, in the style of LoopLowererArrayTypeTests. Range, Take, All, Any and
        // FlatMap are declared bodiless: fusion and lowering match them by name and type.
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
    FlatMap(xs: Array<$T>, f: Function1<$T, Array<$U>>): Array<$U>;
    All(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean;
    Any(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean;
    Range(self: Integer): Array<Integer>;
    Take(xs: Array<$T>, n: Integer): Array<$T>;
    Add(a: Integer, b: Integer): Integer;
    Less(a: Integer, b: Integer): Boolean;
}

library Kernels
{
    RangeMap(n: Integer): Array<Integer> => n.Range.Map(i => i.Add(1));
    RangeReduce(n: Integer): Integer => n.Range.Reduce(0, (acc, i) => acc.Add(i));
    RangeAll(n: Integer, k: Integer): Boolean => n.Range.All(i => i.Less(k));
    RangeAny(n: Integer, k: Integer): Boolean => n.Range.Any(i => i.Less(k));
    RangeFlatMap(n: Integer, ys: Array<Integer>): Array<Integer> => n.Range.FlatMap(i => ys);
    MapMap(xs: Array<Integer>): Array<Integer> => xs.Map(x => x.Add(1)).Map(y => y.Add(2));
    MapTwice(xs: Array<Integer>): Array<Integer> => xs.Map(x => x.Add(1)).Map(y => y.Add(y));
    MapReduce(xs: Array<Integer>): Integer => xs.Map(x => x.Add(1)).Reduce(0, (acc, y) => acc.Add(y));
    TakeMap(xs: Array<Integer>, k: Integer): Array<Integer> => xs.Take(k).Map(x => x.Add(1));
}
";

        private static string EmitKernels()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-fusion-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-fusion")
                    { LowerLoops = true, FuseCombinators = true };
                w.WriteAll();
                // Kernels with an Integer receiver are emitted beside Integer, not in Extensions.
                var text = string.Join("\n", w.Files.Values.Select(f => f.ToString()));
                TestContext.WriteLine(text);
                return text;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        /// <summary>The emitted body of one Kernels function: its signature line and every line
        /// after it up to the next method's signature.</summary>
        public static string BodyOf(string text, string name)
        {
            var m = Regex.Match(text, $@"\b{name}\([^\n]*(\n(?!\s*(\[MethodImpl\(AggressiveInlining\)\] )?public )[^\n]*)*");
            Assert.IsTrue(m.Success, $"no emitted body for {name}");
            return m.Value;
        }

        private static int Loops(string body) => Regex.Matches(body, @"for \(var _i").Count;
        private static int Arrays(string body) => Regex.Matches(body, @"new \w+\[").Count;

        /// <summary>A Range consumer walks the index: one loop, no index array, no call left.</summary>
        [TestCase("RangeMap", 1, 1)]
        [TestCase("RangeReduce", 1, 0)]
        [TestCase("RangeAll", 1, 0)]
        [TestCase("RangeAny", 1, 0)]
        [TestCase("RangeFlatMap", 1, 0)]
        public static void RangeChainIsOneLoopWithNoIndexArray(string fn, int loops, int arrays)
        {
            var body = BodyOf(EmitKernels(), fn);
            Assert.That(Loops(body), Is.EqualTo(loops), "for loops in " + body);
            Assert.That(Arrays(body), Is.EqualTo(arrays), "array allocations in " + body);
            Assert.That(body, Does.Not.Contain(".Range("), "Range still called in " + body);
            Assert.That(body, Does.Not.Contain(".MapRange("), "MapRange still called in " + body);
        }

        /// <summary>Map after Map, Map before Reduce, Take before Map: one loop, at most the result array.</summary>
        [TestCase("MapMap", 1, 1)]
        [TestCase("MapTwice", 1, 1)]   // y used twice: fused by the lowerer's per-stage local, not by substitution
        [TestCase("MapReduce", 1, 0)]
        [TestCase("TakeMap", 1, 1)]
        public static void MapChainIsOneLoopWithNoIntermediateArray(string fn, int loops, int arrays)
        {
            var body = BodyOf(EmitKernels(), fn);
            Assert.That(Loops(body), Is.EqualTo(loops), "for loops in " + body);
            Assert.That(Arrays(body), Is.EqualTo(arrays), "array allocations in " + body);
            Assert.That(body, Does.Not.Contain(".Map("), "Map still called in " + body);
            Assert.That(body, Does.Not.Contain(".Take("), "Take still called in " + body);
        }

        /// <summary>Without the flag the chains are what TKT-505 left: the lowerer alone keeps the
        /// index array of n.Range (a call) before the consumer's loop.</summary>
        [Test]
        public static void WithoutFuseTheRangeCallRemains()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-fusion-off-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-fusion-off") { LowerLoops = true };
                w.WriteAll();
                var body = BodyOf(string.Join("\n", w.Files.Values.Select(f => f.ToString())), "RangeReduce");
                Assert.That(body, Does.Contain("Range("));
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }
    }
}
