using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// TKT-506: with <c>--fuse</c>, a MapRange, Range, All, Any or Reduce whose count is an integer
    /// literal of at most 8 is unrolled to straight-line code, inside lambdas too, where the loop
    /// lowerer does not go. <c>3.Range.Any(j => p(j))</c> becomes <c>p(0) || p(1) || p(2)</c>; a
    /// count of 9 stays a loop.
    /// </summary>
    [TestFixture]
    public static class LiteralUnrollingTests
    {
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
    All(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean;
    Any(xs: Array<$T>, f: Function1<$T, Boolean>): Boolean;
    Range(self: Integer): Array<Integer>;
    Take(xs: Array<$T>, n: Integer): Array<$T>;
    Add(a: Integer, b: Integer): Integer;
    Less(a: Integer, b: Integer): Boolean;
}

library Kernels
{
    AnyOfThree(k: Integer): Boolean => 3.Range.Any(j => j.Less(k));
    AllOfTwo(k: Integer): Boolean => 2.Range.All(j => j.Less(k));
    RangeLit(k: Integer): Array<Integer> => 4.Range;
    MapRangeLit(x: Integer): Array<Integer> => 3.MapRange(i => i.Add(x));
    MapOverLit(x: Integer): Array<Integer> => 3.Range.Map(i => i.Add(x));
    ReduceLit(x: Integer): Integer => 3.Range.Reduce(x, (acc, i) => acc.Add(i));
    TakeNone(xs: Array<Integer>): Array<Integer> => xs.Take(0);
    NineStaysALoop(k: Integer): Boolean => 9.Range.Any(j => j.Less(k));
    InLambda(xs: Array<Integer>): Array<Boolean> => xs.Map(x => 3.Range.Any(j => j.Less(x)));
}
";

        private static string Emit()
        {
            var dir = Path.Combine(Path.GetTempPath(), "plato-unroll-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "corpus.plato"), Source);
                var w = new CSharpWriter(CheckerTestSupport.CompileFolder(dir), "unused-unroll")
                    { LowerLoops = true, FuseCombinators = true };
                w.WriteAll();
                var text = string.Join("\n", w.Files.Values.Select(f => f.ToString()));
                TestContext.WriteLine(text);
                return text;
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { /* best effort */ }
            }
        }

        private static int Count(string body, string pattern) => Regex.Matches(body, pattern).Count;

        [Test]
        public static void AnyOverThreeIsThreeTermsJoinedByOr()
        {
            var body = CombinatorFusionTests.BodyOf(Emit(), "AnyOfThree");
            Assert.That(Count(body, @"\|\|"), Is.EqualTo(2), body);
            Assert.That(Count(body, @"\.Less\("), Is.EqualTo(3), body);
            Assert.That(body, Does.Not.Contain("for (").And.Not.Contain("MakeArray").And.Not.Contain(".Any("), body);
        }

        [Test]
        public static void AllOverTwoIsTwoTermsJoinedByAnd()
        {
            var body = CombinatorFusionTests.BodyOf(Emit(), "AllOfTwo");
            Assert.That(Count(body, @"&&"), Is.EqualTo(1), body);
            Assert.That(body, Does.Not.Contain("for (").And.Not.Contain(".All("), body);
        }

        [TestCase("RangeLit", 4)]
        [TestCase("MapRangeLit", 3)]
        [TestCase("MapOverLit", 3)]
        [TestCase("TakeNone", 0)]
        public static void LiteralCountProducerIsAnArrayLiteral(string fn, int elements)
        {
            var body = CombinatorFusionTests.BodyOf(Emit(), fn);
            Assert.That(body, Does.Contain("Intrinsics.MakeArray<Integer>("), body);
            Assert.That(Count(body, @"\(\(Integer\)\d\)"), Is.EqualTo(elements == 0 ? 0 : elements), body);
            Assert.That(body, Does.Not.Contain("for (").And.Not.Contain(".Range").And.Not.Contain(".MapRange(").And.Not.Contain(".Take("), body);
        }

        [Test]
        public static void ReduceOverThreeIsNestedApplications()
        {
            var body = CombinatorFusionTests.BodyOf(Emit(), "ReduceLit");
            Assert.That(Count(body, @"\.Add\("), Is.EqualTo(3), body);
            Assert.That(body, Does.Not.Contain("for (").And.Not.Contain(".Reduce("), body);
        }

        [Test]
        public static void CountAboveTheLimitStaysALoop()
        {
            var body = CombinatorFusionTests.BodyOf(Emit(), "NineStaysALoop");
            Assert.That(Count(body, @"for \(var _i"), Is.EqualTo(1), body);
            Assert.That(body, Does.Not.Contain("||"), body);
        }

        [Test]
        public static void UnrollingReachesInsideLambdas()
        {
            var body = CombinatorFusionTests.BodyOf(Emit(), "InLambda");
            Assert.That(Count(body, @"\|\|"), Is.EqualTo(2), body);
            Assert.That(body, Does.Not.Contain(".Any(").And.Not.Contain(".Range"), body);
        }
    }
}
