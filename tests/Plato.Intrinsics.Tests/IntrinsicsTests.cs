using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// <see cref="Intrinsics"/>: array and grid literals, the hash-combining family the generated
/// GetHashCode bodies call, and the tuple constructors.
/// </summary>
[TestFixture]
public static class IntrinsicsTests
{
    [Test]
    public static void MakeArrayReturnsTheInterfaceNotTheArray()
    {
        var xs = Intrinsics.MakeArray(1, 2, 3);
        Assert.That(xs.Count, Is.EqualTo(3));
        Assert.That(xs, Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public static void MakeArrayOfNothingIsEmpty()
        => Assert.That(Intrinsics.MakeArray<int>().Count, Is.EqualTo(0));

    // MakeArray2D left the runtime 2026-08-01: Array2D is an ordinary Plato type now and its
    // constructor lives in the generated library, outside this suite's subject.

    [Test]
    public static void CombineHashCodesOfNothingIsTheSeed()
        => Assert.That(Intrinsics.CombineHashCodes().Value, Is.EqualTo(17));

    [Test]
    public static void CombineHashCodesIsAFunctionOfItsArguments()
    {
        var oneTwo = Intrinsics.CombineHashCodes(1, 2).Value;
        var againOneTwo = Intrinsics.CombineHashCodes(1, 2).Value;
        var twoOne = Intrinsics.CombineHashCodes(2, 1).Value;
        Assert.Multiple(() =>
        {
            Assert.That(oneTwo, Is.EqualTo(againOneTwo));
            Assert.That(oneTwo, Is.Not.EqualTo(twoOne), "order matters");
        });
    }

    [Test]
    public static void EveryArityUpToEightIsAvailable()
    {
        // Exercised rather than compared: HashCode.Combine is per-process randomized, so the
        // only stable claim is that each overload binds and runs.
        Assert.DoesNotThrow(() =>
        {
            _ = Intrinsics.CombineHashCodes(1);
            _ = Intrinsics.CombineHashCodes(1, 2);
            _ = Intrinsics.CombineHashCodes(1, 2, 3);
            _ = Intrinsics.CombineHashCodes(1, 2, 3, 4);
            _ = Intrinsics.CombineHashCodes(1, 2, 3, 4, 5);
            _ = Intrinsics.CombineHashCodes(1, 2, 3, 4, 5, 6);
            _ = Intrinsics.CombineHashCodes(1, 2, 3, 4, 5, 6, 7);
            _ = Intrinsics.CombineHashCodes(1, 2, 3, 4, 5, 6, 7, 8);
        });
    }

    [Test]
    public static void TheParamsOverloadHandlesTheWideStructsThatHashCodeCombineCannot()
    {
        var nine = new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var first = Intrinsics.CombineHashCodes(nine).Value;
        var second = Intrinsics.CombineHashCodes(nine).Value;
        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public static void TupleConstructors()
    {
        Assert.Multiple(() =>
        {
            Assert.That(1.Tuple2("a"), Is.EqualTo((1, "a")));
            Assert.That(1.Tuple3("a", 2.5), Is.EqualTo((1, "a", 2.5)));
            Assert.That(1.Tuple4("a", 2.5, true), Is.EqualTo((1, "a", 2.5, true)));
        });
    }
}
