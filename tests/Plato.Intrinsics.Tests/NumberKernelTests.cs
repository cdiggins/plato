using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="Number"/> half of the irreducible intrinsic kernel (plato-378): the members
/// that cannot be written in Plato from the others. Each one is checked against its
/// <see cref="MathF"/> reference rather than a typed-in decimal, so the test says "forwards to
/// MathF" instead of "happens to equal 0.8414710".
/// </summary>
[TestFixture]
public static class NumberKernelTests
{
    private const float Tolerance = 1e-6f;

    private static Number N(float f)
        => f;

    [TestCase(-3.5f)]
    [TestCase(-0.5f)]
    [TestCase(0f)]
    [TestCase(2.25f)]
    public static void FloorIsTheLargestIntegerNotAbove(float f)
        => Assert.That(N(f).Floor.Value, Is.EqualTo(MathF.Floor(f)));

    [TestCase(0f)]
    [TestCase(0.25f)]
    [TestCase(2f)]
    [TestCase(16f)]
    public static void SqrtExpAndNaturalLogForwardToMathF(float f)
    {
        var n = N(f);
        Assert.Multiple(() =>
        {
            Assert.That(n.Sqrt.Value, Is.EqualTo(MathF.Sqrt(f)));
            Assert.That(n.Exp.Value, Is.EqualTo(MathF.Exp(f)));
            Assert.That(n.NaturalLog.Value, Is.EqualTo(MathF.Log(f)));
        });
    }

    [Test]
    public static void SqrtAndNaturalLogOutsideTheirDomain()
    {
        Assert.That(float.IsNaN(N(-1f).Sqrt.Value));
        Assert.That(float.IsNaN(N(-1f).NaturalLog.Value));
        Assert.That(N(0f).NaturalLog.Value, Is.EqualTo(float.NegativeInfinity));
    }

    [Test]
    public static void ExpAndNaturalLogInvertEachOther()
    {
        var n = N(3.75f);
        Assert.That(n.Exp.NaturalLog.Value, Is.EqualTo(3.75f).Within(1e-5f));
    }

    [TestCase(0f)]
    [TestCase(0.7f)]
    [TestCase(-2.5f)]
    public static void SinAndCosTakeRadians(float f)
    {
        var n = N(f);
        Assert.That(n.Sin.Value, Is.EqualTo(MathF.Sin(f)).Within(Tolerance));
        Assert.That(n.Cos.Value, Is.EqualTo(MathF.Cos(f)).Within(Tolerance));
    }

    [Test]
    public static void SinAndCosAtTheQuarterTurns()
    {
        var halfPi = N(MathF.PI / 2);
        Assert.Multiple(() =>
        {
            Assert.That(halfPi.Sin.Value, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(halfPi.Cos.Value, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(N(MathF.PI).Sin.Value, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(N(MathF.PI).Cos.Value, Is.EqualTo(-1f).Within(Tolerance));
        });
    }

    [TestCase(0f)]
    [TestCase(0.7f)]
    [TestCase(-2.5f)]
    public static void ThePythagoreanIdentityHolds(float f)
    {
        var n = N(f);
        var s = n.Sin.Value;
        var c = n.Cos.Value;
        Assert.That(s * s + c * c, Is.EqualTo(1f).Within(Tolerance));
    }

    [Test]
    public static void Atan2TakesTheReceiverAsYAndCoversAllFourQuadrants()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(1f).Atan2Radians(0f).Value, Is.EqualTo(MathF.PI / 2).Within(Tolerance));
            Assert.That(N(1f).Atan2Radians(1f).Value, Is.EqualTo(MathF.PI / 4).Within(Tolerance));
            Assert.That(N(0f).Atan2Radians(-1f).Value, Is.EqualTo(MathF.PI).Within(Tolerance));
            Assert.That(N(-1f).Atan2Radians(1f).Value, Is.EqualTo(-MathF.PI / 4).Within(Tolerance));
            Assert.That(N(0f).Atan2Radians(0f).Value, Is.EqualTo(0f));
        });
    }

    [TestCase(2f, 10f, 1024f)]
    [TestCase(9f, 0.5f, 3f)]
    [TestCase(5f, 0f, 1f)]
    [TestCase(2f, -1f, 0.5f)]
    public static void PowForwardsToMathF(float b, float e, float expected)
        => Assert.That(N(b).Pow(e).Value, Is.EqualTo(expected).Within(Tolerance));

    [Test]
    public static void FusedMultiplyAddIsSelfTimesYPlusZ()
    {
        Assert.That(N(2f).FusedMultiplyAdd(3f, 4f).Value, Is.EqualTo(10f));
        Assert.That(N(-2f).FusedMultiplyAdd(3f, 4f).Value, Is.EqualTo(-2f));
    }

    [Test]
    public static void RoundUsesBankersRoundingAndTakesADigitCount()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(2.5f).Round(0).Value, Is.EqualTo(2f), "midpoint goes to even");
            Assert.That(N(3.5f).Round(0).Value, Is.EqualTo(4f), "midpoint goes to even");
            Assert.That(N(-2.5f).Round(0).Value, Is.EqualTo(-2f));
            Assert.That(N(2.4f).Round(0).Value, Is.EqualTo(2f));
            Assert.That(N(1.2345f).Round(2).Value, Is.EqualTo(1.23f).Within(Tolerance));
        });
    }

    [Test]
    public static void PredicatesAndConversions()
    {
        Assert.Multiple(() =>
        {
            Assert.That((bool)N(float.NaN).IsNaN);
            Assert.That((bool)N(1f).IsNaN, Is.False);
            Assert.That((bool)N(float.PositiveInfinity).IsInfinite);
            Assert.That((bool)N(float.NegativeInfinity).IsInfinite);
            Assert.That((bool)N(1f).IsInfinite, Is.False);
            Assert.That((bool)N(float.NaN).IsInfinite, Is.False);
        });
    }

    [Test]
    public static void ToIntegerTruncatesTowardsZero()
    {
        Assert.Multiple(() =>
        {
            Assert.That(N(2.9f).ToInteger.Value, Is.EqualTo(2));
            Assert.That(N(-2.9f).ToInteger.Value, Is.EqualTo(-2));
            Assert.That(N(0f).ToInteger.Value, Is.EqualTo(0));
        });
    }

    [Test]
    public static void HashAgreesWithTheWrappedFloat()
    {
        var hash = N(2.5f).Hash.Value;
        var again = N(2.5f).Hash.Value;
        Assert.That(hash, Is.EqualTo(2.5f.GetHashCode()));
        Assert.That(hash, Is.EqualTo(again));
    }
}
