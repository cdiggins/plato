using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="Number"/> wrapper: construction, the float bridge, and the operators.
/// Everything here is exact — a wrapper that forwards to float may not round or re-associate.
/// </summary>
[TestFixture]
public static class NumberTests
{
    [TestCase(0f)]
    [TestCase(1f)]
    [TestCase(-1.5f)]
    [TestCase(3.4028235e38f)]
    [TestCase(float.Epsilon)]
    public static void ConstructionKeepsTheExactBits(float f)
    {
        Assert.That(new Number(f).Value, Is.EqualTo(f));
        Assert.That(Number.FromSystem(f).Value, Is.EqualTo(f));
    }

    [TestCase(0f)]
    [TestCase(-2.25f)]
    [TestCase(1e-30f)]
    public static void FloatRoundTripsThroughBothImplicitConversions(float f)
    {
        Number n = f;
        float back = n;
        Assert.That(back, Is.EqualTo(f));
    }

    [Test]
    public static void DefaultIsZero()
        => Assert.That(default(Number).Value, Is.EqualTo(0f));

    [Test]
    public static void ArithmeticOperatorsForwardToFloat()
    {
        Number a = 7.5f;
        Number b = 2f;
        Assert.Multiple(() =>
        {
            Assert.That((a + b).Value, Is.EqualTo(9.5f));
            Assert.That((a - b).Value, Is.EqualTo(5.5f));
            Assert.That((a * b).Value, Is.EqualTo(15f));
            Assert.That((a / b).Value, Is.EqualTo(3.75f));
            Assert.That((a % b).Value, Is.EqualTo(1.5f));
            Assert.That((-a).Value, Is.EqualTo(-7.5f));
        });
    }

    [Test]
    public static void RemainderKeepsTheSignOfTheDividend()
    {
        Assert.That((((Number)(-7.5f)) % (Number)2f).Value, Is.EqualTo(-1.5f));
        Assert.That((((Number)7.5f) % (Number)(-2f)).Value, Is.EqualTo(1.5f));
    }

    [Test]
    public static void ComparisonOperators()
    {
        Number a = 1f;
        Number b = 2f;
        Assert.Multiple(() =>
        {
            Assert.That((bool)(a < b));
            Assert.That((bool)(a <= b));
            Assert.That((bool)(b > a));
            Assert.That((bool)(b >= a));
            Assert.That((bool)(a <= (Number)1f));
            Assert.That((bool)(a >= (Number)1f));
            Assert.That((bool)(a > b), Is.False);
            Assert.That((bool)(b < a), Is.False);
        });
    }

    [Test]
    public static void CompareToMatchesFloatCompareTo()
    {
        Assert.That(((Number)1f).CompareTo(2f).Value, Is.EqualTo(-1));
        Assert.That(((Number)2f).CompareTo(2f).Value, Is.EqualTo(0));
        Assert.That(((Number)3f).CompareTo(2f).Value, Is.EqualTo(1));
    }

    [Test]
    public static void LimitsAndConstants()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Number.MinValue.Value, Is.EqualTo(float.MinValue));
            Assert.That(Number.MaxValue.Value, Is.EqualTo(float.MaxValue));
            Assert.That(Number.Pi.Value, Is.EqualTo(MathF.PI));
            Assert.That(Number.Epsilon.Value, Is.EqualTo(float.Epsilon));
        });
    }

    // ---------------------------------------------------------------------------------------
    // Float edge cases. These pin IEEE semantics rather than any Plato choice: the wrapper must
    // not "improve" on float.
    // ---------------------------------------------------------------------------------------

    [Test]
    public static void DivisionByZeroYieldsInfinityNotAnException()
    {
        Assert.That((((Number)1f) / (Number)0f).Value, Is.EqualTo(float.PositiveInfinity));
        Assert.That((((Number)(-1f)) / (Number)0f).Value, Is.EqualTo(float.NegativeInfinity));
        Assert.That(float.IsNaN((((Number)0f) / (Number)0f).Value));
    }

    [Test]
    public static void NaNPropagatesThroughArithmetic()
    {
        Number nan = float.NaN;
        Number one = 1f;
        Assert.Multiple(() =>
        {
            Assert.That(float.IsNaN((nan + one).Value));
            Assert.That(float.IsNaN((nan * one).Value));
            Assert.That(float.IsNaN((nan - one).Value));
            Assert.That(float.IsNaN((-nan).Value));
        });
    }

    [Test]
    public static void EveryComparisonWithNaNIsFalse()
    {
        Number nan = float.NaN;
        Number one = 1f;
        Assert.Multiple(() =>
        {
            Assert.That((bool)(nan < one), Is.False);
            Assert.That((bool)(nan > one), Is.False);
            Assert.That((bool)(nan <= one), Is.False);
            Assert.That((bool)(nan >= one), Is.False);
        });
    }

    [Test]
    public static void InfinityArithmetic()
    {
        Number inf = float.PositiveInfinity;
        Assert.Multiple(() =>
        {
            Assert.That((inf + (Number)1f).Value, Is.EqualTo(float.PositiveInfinity));
            Assert.That(float.IsNaN((inf - inf).Value));
            Assert.That((((Number)1f) / inf).Value, Is.EqualTo(0f));
        });
    }

    // ---------------------------------------------------------------------------------------
    // Equality. Number declares no Equals/GetHashCode of its own, so it inherits field-wise
    // ValueType equality — which is NOT float's == (NaN equals NaN under field comparison).
    // ---------------------------------------------------------------------------------------

    [Test]
    public static void EqualValuesHaveEqualHashCodes()
    {
        Number a = 12.5f;
        Number b = 12.5f;
        Assert.That(a.Equals(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public static void StructEqualityIsFieldWiseNotIeee()
    {
        Number nan = float.NaN;
        Number alsoNan = float.NaN;
        Assert.That(nan.Equals(alsoNan), "field-wise ValueType equality: NaN equals NaN");
        Assert.That((bool)(nan >= alsoNan), Is.False, "IEEE comparison still says otherwise");
    }
}
