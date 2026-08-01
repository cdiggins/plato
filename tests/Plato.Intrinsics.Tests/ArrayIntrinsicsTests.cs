using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The Array half of the intrinsic kernel: <see cref="ArrayIntrinsics.MapRange{T}"/> (the only
/// constructor), <see cref="ArrayIntrinsics.Reduce{T,TAcc}"/> (the only sequential fold) and
/// <see cref="ArrayIntrinsics.FlatMap{T,TResult}"/> (the only length-varying producer).
/// </summary>
[TestFixture]
public static class ArrayIntrinsicsTests
{
    private static IReadOnlyList<int> Ints(params int[] xs)
        => xs;

    [Test]
    public static void MapRangeCoversZeroToCountExclusive()
    {
        var xs = 5.MapRange(i => i.Value * i.Value);
        Assert.That(xs.Count, Is.EqualTo(5));
        Assert.That(xs, Is.EqualTo(new[] { 0, 1, 4, 9, 16 }));
    }

    [Test]
    public static void MapRangeOfZeroIsEmpty()
        => Assert.That(0.MapRange(i => i.Value).Count, Is.EqualTo(0));

    [Test]
    public static void MapRangeIndexesAreIntegerTyped()
    {
        var xs = 3.MapRange(i => i + (Integer)1);
        Assert.That(xs.Select(i => i.Value), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public static void MapRangeIsRepeatableAndIndexable()
    {
        var xs = 4.MapRange(i => i.Value * 3);
        Assert.That(xs[2], Is.EqualTo(6));
        Assert.That(xs[2], Is.EqualTo(6), "a second read gives the same element");
        Assert.That(xs.Count, Is.EqualTo(4));
    }

    [Test]
    public static void ReduceFoldsLeftToRight()
    {
        Assert.That(Ints(1, 2, 3, 4).Reduce(0, (a, x) => a + x), Is.EqualTo(10));
        Assert.That(Ints(1, 2, 3).Reduce("", (a, x) => a + x), Is.EqualTo("123"));
    }

    [Test]
    public static void ReduceOnAnEmptyArrayIsTheSeed()
        => Assert.That(Ints().Reduce(99, (a, x) => a + x), Is.EqualTo(99));

    [Test]
    public static void ReduceCanChangeTheAccumulatorType()
        => Assert.That(Ints(1, 2, 3).Reduce(1L, (a, x) => a * x), Is.EqualTo(6L));

    [Test]
    public static void FlatMapConcatenatesInOrder()
    {
        var xs = Ints(1, 2, 3).FlatMap(x => (IReadOnlyList<int>)new[] { x, -x });
        Assert.That(xs, Is.EqualTo(new[] { 1, -1, 2, -2, 3, -3 }));
    }

    [Test]
    public static void FlatMapCanShrinkAndGrow()
    {
        var dropped = Ints(1, 2, 3).FlatMap(x => (IReadOnlyList<int>)Array.Empty<int>());
        Assert.That(dropped.Count, Is.EqualTo(0));

        var grown = Ints(2).FlatMap(x => (IReadOnlyList<int>)new[] { x, x, x });
        Assert.That(grown.Count, Is.EqualTo(3));
    }

    [Test]
    public static void TheKernelComposes()
    {
        // count -> map -> flat-map -> fold, the whole array contract in one expression.
        var total = 4.MapRange(i => i.Value)
            .FlatMap(x => (IReadOnlyList<int>)new[] { x, x })
            .Reduce(0, (a, x) => a + x);
        Assert.That(total, Is.EqualTo(12));
    }
}
