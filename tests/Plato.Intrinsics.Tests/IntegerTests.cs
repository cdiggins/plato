using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="Integer"/> wrapper. Integer semantics are exact, so every assertion here is
/// exact — including the ones that pin C#'s truncating division and its two arithmetic
/// exceptions.
/// </summary>
[TestFixture]
public static class IntegerTests
{
    private static Integer I(int i)
        => i;

    [TestCase(0)]
    [TestCase(7)]
    [TestCase(-7)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public static void ConversionsRoundTrip(int i)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Integer(i).Value, Is.EqualTo(i));
            Assert.That(Integer.FromSystem(i).Value, Is.EqualTo(i));
            Assert.That(I(i).ToSystem(), Is.EqualTo(i));
            Assert.That((int)I(i), Is.EqualTo(i));
        });
    }

    [Test]
    public static void DefaultIsZero()
        => Assert.That(default(Integer).Value, Is.EqualTo(0));

    [Test]
    public static void ArithmeticOperators()
    {
        var a = I(7);
        var b = I(2);
        Assert.Multiple(() =>
        {
            Assert.That((a + b).Value, Is.EqualTo(9));
            Assert.That((a - b).Value, Is.EqualTo(5));
            Assert.That((a * b).Value, Is.EqualTo(14));
            Assert.That((a / b).Value, Is.EqualTo(3));
            Assert.That((a % b).Value, Is.EqualTo(1));
            Assert.That((-a).Value, Is.EqualTo(-7));
        });
    }

    [TestCase(7, 2, 3, 1)]
    [TestCase(-7, 2, -3, -1)]
    [TestCase(7, -2, -3, 1)]
    [TestCase(-7, -2, 3, -1)]
    public static void DivisionTruncatesTowardsZeroAndRemainderFollowsTheDividend(
        int a, int b, int quotient, int remainder)
    {
        Assert.That((I(a) / I(b)).Value, Is.EqualTo(quotient));
        Assert.That((I(a) % I(b)).Value, Is.EqualTo(remainder));
    }

    [Test]
    public static void DivisionByZeroThrows()
    {
        Assert.Throws<DivideByZeroException>(() => _ = I(1) / I(0));
        Assert.Throws<DivideByZeroException>(() => _ = I(1) % I(0));
    }

    [Test]
    public static void MinValueDividedByMinusOneOverflows()
        => Assert.Throws<OverflowException>(() => _ = Integer.MinValue / I(-1));

    [Test]
    public static void AdditionWrapsAroundUnchecked()
        => Assert.That((Integer.MaxValue + I(1)).Value, Is.EqualTo(int.MinValue));

    [Test]
    public static void ComparisonOperators()
    {
        var a = I(1);
        var b = I(2);
        Assert.Multiple(() =>
        {
            Assert.That((bool)(a < b));
            Assert.That((bool)(a <= b));
            Assert.That((bool)(b > a));
            Assert.That((bool)(b >= a));
            Assert.That((bool)(a >= I(1)));
            Assert.That((bool)(b < a), Is.False);
        });
    }

    [Test]
    public static void CompareToMatchesIntCompareTo()
    {
        Assert.That(I(1).CompareTo(2), Is.EqualTo(-1));
        Assert.That(I(2).CompareTo(2), Is.EqualTo(0));
        Assert.That(I(3).CompareTo(2), Is.EqualTo(1));
    }

    [Test]
    public static void Limits()
    {
        Assert.That(Integer.MinValue.Value, Is.EqualTo(int.MinValue));
        Assert.That(Integer.MaxValue.Value, Is.EqualTo(int.MaxValue));
    }

    // ---------------------------------------------------------------------------------------
    // The Bitwise interface: a property for the no-arg member, methods for the rest, operators
    // for the two the generated struct renders from operator overloads.
    // ---------------------------------------------------------------------------------------

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(0x0F0F0F0F)]
    public static void BitwiseNotMatchesTheOperator(int i)
    {
        Assert.That(I(i).BitwiseNot.Value, Is.EqualTo(~i));
        Assert.That((~I(i)).Value, Is.EqualTo(~i));
    }

    [Test]
    public static void BitwiseBinaryMembers()
    {
        var a = I(0b1100);
        var b = I(0b1010);
        Assert.Multiple(() =>
        {
            Assert.That((a & b).Value, Is.EqualTo(0b1000));
            Assert.That((a | b).Value, Is.EqualTo(0b1110));
            Assert.That((a ^ b).Value, Is.EqualTo(0b0110));
            Assert.That(a.BitwiseXor(b).Value, Is.EqualTo(0b0110));
        });
    }

    [Test]
    public static void ShiftsAreArithmeticNotLogical()
    {
        Assert.Multiple(() =>
        {
            Assert.That(I(1).ShiftLeft(4).Value, Is.EqualTo(16));
            Assert.That(I(16).ShiftRight(4).Value, Is.EqualTo(1));
            Assert.That(I(-16).ShiftRight(2).Value, Is.EqualTo(-4), "sign-propagating");
            Assert.That(I(-1).ShiftRight(1).Value, Is.EqualTo(-1));
        });
    }

    [Test]
    public static void ShiftCountsAreTakenModuloThirtyTwo()
        => Assert.That(I(1).ShiftLeft(32).Value, Is.EqualTo(1),
            "C# masks the shift count to its low five bits");

    // ---------------------------------------------------------------------------------------
    // Conversions to Number, and the array kernel's only constructor.
    // ---------------------------------------------------------------------------------------

    [TestCase(0)]
    [TestCase(-7)]
    [TestCase(1000000)]
    public static void ToNumberWidensExactly(int i)
    {
        Assert.That(I(i).ToNumber.Value, Is.EqualTo((float)i));
        Number implicitly = I(i);
        Assert.That(implicitly.Value, Is.EqualTo((float)i));
    }

    [Test]
    public static void MapRangeBuildsZeroToCountExclusive()
    {
        var xs = I(4).MapRange(i => i.Value * 10);
        Assert.That(xs.Count, Is.EqualTo(4));
        Assert.That(xs, Is.EqualTo(new[] { 0, 10, 20, 30 }));
    }

    [Test]
    public static void MapRangeOfZeroIsEmpty()
        => Assert.That(I(0).MapRange(i => i.Value).Count, Is.EqualTo(0));

    [Test]
    public static void HashAgreesWithTheWrappedInt()
        => Assert.That(I(42).Hash.Value, Is.EqualTo(42.GetHashCode()));

    [Test]
    public static void EqualValuesHaveEqualHashCodes()
    {
        var a = I(5);
        var b = I(5);
        Assert.That(a.Equals(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        Assert.That(a.Equals(I(6)), Is.False);
    }
}
