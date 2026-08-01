using NUnit.Framework;
using Boolean = Ara3D.Geometry.Boolean;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="Boolean"/> wrapper. The connectives are the whole intrinsic contract; the
/// ordering operators exist because <c>Orderable</c> is discharged from them, and they encode
/// false &lt; true.
/// </summary>
[TestFixture]
public static class BooleanTests
{
    private static Boolean B(bool b)
        => b;

    [TestCase(true)]
    [TestCase(false)]
    public static void ConversionsRoundTrip(bool b)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Boolean(b).Value, Is.EqualTo(b));
            Assert.That(Boolean.FromSystem(b).Value, Is.EqualTo(b));
            Assert.That(B(b).ToSystem(), Is.EqualTo(b));
            Assert.That((bool)B(b), Is.EqualTo(b));
        });
    }

    [Test]
    public static void DefaultIsFalse()
        => Assert.That(default(Boolean).Value, Is.False);

    [TestCase(false, false, false, false, false)]
    [TestCase(false, true, false, true, true)]
    [TestCase(true, false, false, true, true)]
    [TestCase(true, true, true, true, false)]
    public static void TheThreeConnectives(bool a, bool b, bool and, bool or, bool xor)
    {
        Assert.Multiple(() =>
        {
            Assert.That((B(a) & B(b)).Value, Is.EqualTo(and));
            Assert.That((B(a) | B(b)).Value, Is.EqualTo(or));
            Assert.That((B(a) ^ B(b)).Value, Is.EqualTo(xor));
        });
    }

    [Test]
    public static void NotIsAnInvolution()
    {
        Assert.That((!B(true)).Value, Is.False);
        Assert.That((!B(false)).Value, Is.True);
        Assert.That((!!B(true)).Value, Is.True);
    }

    [Test]
    public static void DeMorgansLawsHold()
    {
        foreach (var a in new[] { false, true })
        foreach (var b in new[] { false, true })
        {
            Assert.That((!(B(a) & B(b))).Value, Is.EqualTo(((!B(a)) | !B(b)).Value));
            Assert.That((!(B(a) | B(b))).Value, Is.EqualTo(((!B(a)) & !B(b)).Value));
        }
    }

    [Test]
    public static void TrueAndFalseOperatorsMakeItUsableAsACondition()
    {
        var taken = false;
        if (B(true))
            taken = true;
        Assert.That(taken);

        // The short-circuit operators route through operator true/false plus operator & / |.
        Assert.That((B(true) && B(true)).Value);
        Assert.That((B(false) || B(true)).Value);
    }

    [TestCase(false, false, true, true)]
    [TestCase(false, true, true, false)]
    [TestCase(true, false, false, true)]
    [TestCase(true, true, true, true)]
    public static void OrderingPutsFalseBeforeTrue(bool a, bool b, bool le, bool ge)
    {
        Assert.That(B(a) <= B(b), Is.EqualTo(le));
        Assert.That(B(a) >= B(b), Is.EqualTo(ge));
    }

    [Test]
    public static void StrictOrderingIsTheNonReflexivePart()
    {
        Assert.Multiple(() =>
        {
            Assert.That(B(false) < B(true));
            Assert.That(B(true) > B(false));
            Assert.That(B(false) < B(false), Is.False);
            Assert.That(B(true) > B(true), Is.False);
            Assert.That(B(true) < B(false), Is.False);
        });
    }

    [Test]
    public static void CompareToMatchesBoolCompareTo()
    {
        Assert.That(B(false).CompareTo(true), Is.EqualTo(-1));
        Assert.That(B(true).CompareTo(true), Is.EqualTo(0));
        Assert.That(B(true).CompareTo(false), Is.EqualTo(1));
    }

    [Test]
    public static void EqualValuesHaveEqualHashCodes()
    {
        var a = B(true);
        var b = B(true);
        Assert.That(a.Equals(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        Assert.That(a.Equals(B(false)), Is.False);
    }
}
