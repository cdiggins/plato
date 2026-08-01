using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The affine builder types <see cref="PlatoList{T}"/> and <see cref="PlatoBuffer{T}"/> and the
/// zero-copy <see cref="FrozenArray{T}"/> they consume into. The interesting contract is the
/// affine one: Freeze consumes the builder, and every later use throws.
/// </summary>
[TestFixture]
public static class BuilderTests
{
    // -------------------------------------------------------------------------------------
    // PlatoList
    // -------------------------------------------------------------------------------------

    [Test]
    public static void AnEmptyListHasNoElements()
        => Assert.That(new PlatoList<int>().Count.Value, Is.EqualTo(0));

    [Test]
    public static void AddReturnsTheBuilderSoCallsChain()
    {
        var xs = new PlatoList<int>().Add(1).Add(2).Add(3);
        Assert.That(xs.Count.Value, Is.EqualTo(3));
        Assert.That(xs.At(0), Is.EqualTo(1));
        Assert.That(xs[2], Is.EqualTo(3));
    }

    [Test]
    public static void AddGrowsPastTheInitialCapacity()
    {
        var xs = new PlatoList<int>();
        for (var i = 0; i < 100; i++)
            xs = xs.Add(i);
        Assert.That(xs.Count.Value, Is.EqualTo(100));
        Assert.That(xs.At(99), Is.EqualTo(99));
    }

    [Test]
    public static void AddRangeAppendsInOrder()
    {
        var xs = new PlatoList<int>().Add(0).AddRange(new[] { 1, 2, 3 });
        Assert.That(xs.Count.Value, Is.EqualTo(4));
        Assert.That(xs.Freeze(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
    }

    [Test]
    public static void AddRangeOfNothingIsANoOp()
        => Assert.That(new PlatoList<int>().AddRange(Array.Empty<int>()).Count.Value, Is.EqualTo(0));

    [Test]
    public static void SetOverwritesInPlace()
    {
        var xs = new PlatoList<int>().Add(1).Add(2).Set(1, 99);
        Assert.That(xs.At(1), Is.EqualTo(99));
        Assert.That(xs.Count.Value, Is.EqualTo(2));
    }

    [Test]
    public static void ListIndicesAreCheckedAgainstTheLogicalCountNotTheCapacity()
    {
        var xs = new PlatoList<int>().Add(1);
        Assert.Multiple(() =>
        {
            Assert.Throws<IndexOutOfRangeException>(() => _ = xs.At(1));
            Assert.Throws<IndexOutOfRangeException>(() => _ = xs.At(-1));
            Assert.Throws<IndexOutOfRangeException>(() => xs.Set(1, 0));
        });
    }

    [Test]
    public static void FreezeConsumesTheListBuilder()
    {
        var xs = new PlatoList<int>().Add(1).Add(2);
        var frozen = xs.Freeze();
        Assert.That(frozen, Is.EqualTo(new[] { 1, 2 }));
        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = xs.Count);
            Assert.Throws<InvalidOperationException>(() => _ = xs.At(0));
            Assert.Throws<InvalidOperationException>(() => xs.Add(3));
            Assert.Throws<InvalidOperationException>(() => xs.Freeze());
        });
    }

    [Test]
    public static void FreezeReportsTheLogicalCountNotTheBackingArrayLength()
    {
        // Five adds grow the backing array to eight; the frozen view must still say five.
        var xs = new PlatoList<int>();
        for (var i = 0; i < 5; i++)
            xs = xs.Add(i);
        Assert.That(xs.Freeze().Count, Is.EqualTo(5));
    }

    [Test]
    public static void EmptyListTakesItsElementTypeFromAnExistingArray()
    {
        var seed = (IReadOnlyList<string>)new[] { "a" };
        var builder = seed.EmptyList();
        Assert.That(builder.Count.Value, Is.EqualTo(0));
        Assert.That(builder.Add("b").At(0), Is.EqualTo("b"));
    }

    // -------------------------------------------------------------------------------------
    // PlatoBuffer
    // -------------------------------------------------------------------------------------

    [Test]
    public static void ABufferHasItsSlotCountFromTheStart()
    {
        var b = new PlatoBuffer<int>(3);
        Assert.That(b.Count.Value, Is.EqualTo(3));
    }

    [Test]
    public static void UnwrittenSlotsHoldTheDefault()
    {
        var b = new PlatoBuffer<string>(2);
        Assert.That(b.At(0), Is.Null);
        Assert.That(new PlatoBuffer<int>(2).At(1), Is.EqualTo(0));
    }

    [Test]
    public static void SlotsMayBeFilledInAnyOrder()
    {
        var b = new PlatoBuffer<int>(3).Set(2, 30).Set(0, 10).Set(1, 20);
        Assert.That(b.Freeze(), Is.EqualTo(new[] { 10, 20, 30 }));
    }

    [Test]
    public static void ABufferOfZeroSlotsIsLegal()
    {
        var b = new PlatoBuffer<int>(0);
        Assert.That(b.Count.Value, Is.EqualTo(0));
        Assert.That(b.Freeze().Count, Is.EqualTo(0));
    }

    [Test]
    public static void ANegativeSlotCountIsRejectedEagerly()
        => Assert.Throws<ArgumentOutOfRangeException>(() => _ = new PlatoBuffer<int>(-1));

    [Test]
    public static void BufferIndicesAreBoundsChecked()
    {
        var b = new PlatoBuffer<int>(2);
        Assert.Throws<IndexOutOfRangeException>(() => _ = b.At(2));
        Assert.Throws<IndexOutOfRangeException>(() => b.Set(2, 0));
    }

    [Test]
    public static void FreezeConsumesTheBufferBuilder()
    {
        var b = new PlatoBuffer<int>(1).Set(0, 7);
        Assert.That(b.Freeze(), Is.EqualTo(new[] { 7 }));
        Assert.Multiple(() =>
        {
            Assert.Throws<InvalidOperationException>(() => _ = b.Count);
            Assert.Throws<InvalidOperationException>(() => _ = b.At(0));
            Assert.Throws<InvalidOperationException>(() => b.Set(0, 1));
            Assert.Throws<InvalidOperationException>(() => b.Freeze());
        });
    }

    // -------------------------------------------------------------------------------------
    // FrozenArray
    // -------------------------------------------------------------------------------------

    [Test]
    public static void TheFrozenViewIsIndexableEnumerableAndBoundsChecked()
    {
        var frozen = new PlatoList<int>().AddRange(new[] { 5, 6, 7 }).Freeze();
        Assert.Multiple(() =>
        {
            Assert.That(frozen.Count, Is.EqualTo(3));
            Assert.That(frozen[1], Is.EqualTo(6));
            Assert.That(frozen.ToArray(), Is.EqualTo(new[] { 5, 6, 7 }));
            Assert.Throws<IndexOutOfRangeException>(() => _ = frozen[3]);
            Assert.Throws<IndexOutOfRangeException>(() => _ = frozen[-1]);
        });
    }

    [Test]
    public static void TheFrozenViewIsStable()
    {
        var frozen = new PlatoList<int>().Add(1).Freeze();
        var first = frozen.ToArray();
        var second = frozen.ToArray();
        Assert.That(first, Is.EqualTo(second));
    }
}
