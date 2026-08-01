using Ara3D.Geometry;
using NUnit.Framework;
using String = Ara3D.Geometry.String;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="String"/> wrapper: a string field, the Countable/Indexable pair
/// (<c>Count</c> property, <c>At</c> plus indexer) and ordering.
/// </summary>
[TestFixture]
public static class StringTests
{
    private static String S(string s)
        => s;

    [TestCase("")]
    [TestCase("abc")]
    [TestCase("with spaces and ünicöde")]
    public static void ConversionsRoundTrip(string s)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new String(s).Value, Is.EqualTo(s));
            Assert.That(String.FromSystem(s).Value, Is.EqualTo(s));
            Assert.That(S(s).ToSystem(), Is.EqualTo(s));
            Assert.That((string)S(s), Is.EqualTo(s));
        });
    }

    [Test]
    public static void CountIsTheCodeUnitLength()
    {
        Assert.That(S("").Count.Value, Is.EqualTo(0));
        Assert.That(S("abc").Count.Value, Is.EqualTo(3));
    }

    [Test]
    public static void AtAndTheIndexerAgree()
    {
        var s = S("plato");
        Assert.Multiple(() =>
        {
            Assert.That(s.At(0).Value, Is.EqualTo('p'));
            Assert.That(s.At(4).Value, Is.EqualTo('o'));
            Assert.That(s[2].Value, Is.EqualTo('a'));
        });
    }

    [Test]
    public static void IndexingOutOfRangeThrows()
    {
        var s = S("ab");
        Assert.Throws<IndexOutOfRangeException>(() => _ = s.At(2));
        Assert.Throws<IndexOutOfRangeException>(() => _ = s.At(-1));
    }

    [Test]
    public static void OrderingFollowsStringCompareTo()
    {
        Assert.Multiple(() =>
        {
            Assert.That(S("apple") < S("banana"));
            Assert.That(S("banana") > S("apple"));
            Assert.That(S("apple") <= S("apple"));
            Assert.That(S("apple") >= S("apple"));
            Assert.That(S("") < S("a"));
            Assert.That(S("abc") < S("abcd"));
        });
    }

    [Test]
    public static void EqualValuesHaveEqualHashCodes()
    {
        var a = S("abc");
        var b = S(new string(new[] { 'a', 'b', 'c' }));
        Assert.That(a.Equals(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        Assert.That(a.Equals(S("abd")), Is.False);
    }

    [Test]
    public static void DefaultWrapsANullString()
    {
        // pins current behavior: String is a struct over a reference, so default(String) is a
        // null string rather than the empty one, and every observation on it throws.
        Assert.That(default(String).Value, Is.Null);
        Assert.Throws<NullReferenceException>(() => _ = default(String).Count);
    }
}
