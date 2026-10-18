using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The Number profile this suite was built under (src/Plato.Intrinsics/NumberProfile.cs). Run
/// the suite twice, plain and with -p:PlatoNumberProfile=gpu32: each test states what Number
/// must do under the profile in force, so a build whose alias and symbol disagree fails here.
/// </summary>
[TestFixture]
public static class NumberProfileTests
{
    private static bool IsCpu64 => PlatoNumberProfile.Name == "cpu64";

    // 2^24 + 1 is exact in binary64 and is not representable in binary32, where the sum rounds
    // (ties to even) back to 2^24.
    [Test]
    public static void SumAboveTwoToTheTwentyFourIsExactOnlyUnderCpu64()
    {
        var sum = (Number)16777216 + (Number)1;
        Assert.That(sum.Value, Is.EqualTo(IsCpu64 ? 16777217.0 : 16777216.0));
    }

    // 0.1 + 0.2 differs from 0.3 in both formats, by different amounts; the binary64 result is
    // the one every IEEE 754 double gives.
    [Test]
    public static void TenthPlusTwoTenthsMatchesTheProfilesFormat()
    {
        var sum = (Number)(NumberValue)0.1 + (Number)(NumberValue)0.2;
        if (IsCpu64)
            Assert.That(sum.Value, Is.EqualTo(0.30000000000000004));
        else
            Assert.That(sum.Value, Is.EqualTo(0.3f));
    }

    [Test]
    public static void NameAndPrecisionAgreeWithTheMachineType()
    {
        Assert.That(PlatoNumberProfile.Name, Is.EqualTo(sizeof(NumberValue) == 8 ? "cpu64" : "gpu32"));
        Assert.That(PlatoNumberProfile.Precision, Is.EqualTo(IsCpu64 ? 53 : 24));
    }
}
