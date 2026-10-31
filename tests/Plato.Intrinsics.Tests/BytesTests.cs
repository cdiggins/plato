// Checks the Bytes view in the handwritten runtime (TKT-1143).
using System.Text;
using Ara3D.Geometry;
using NUnit.Framework;
using String = Ara3D.Geometry.String;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="Bytes"/> view: a byte array with a start and a count. The Plato laws
/// (<c>stdlib/tests/bytes.laws.plato</c>) check every operation against String on both engines;
/// these tests pin what a law cannot say: that a slice shares the array, that the host wraps without
/// copying, that a view never reads outside its range, and the spelling of the hash.
/// </summary>
[TestFixture]
public static class BytesTests
{
    private static Bytes B(string s) => Bytes.Wrap(Encoding.Latin1.GetBytes(s));

    [Test]
    public static void ToTextIsLatin1ForEveryByteAndOnlyTheViewedRange()
    {
        var all = new byte[256];
        for (var i = 0; i < 256; i++)
            all[i] = (byte)i;
        var text = Bytes.Wrap(all).ToText().Value;
        Assert.That(text.Length, Is.EqualTo(256));
        for (var i = 0; i < 256; i++)
            Assert.That((int)text[i], Is.EqualTo(i));
        Assert.That(Encoding.Latin1.GetBytes(text), Is.EqualTo(all));
        Assert.That(B("zzfoobarzz").SubBytes(2, 6).ToText().Value, Is.EqualTo("foobar"));
        Assert.That(default(Bytes).ToText().Value, Is.EqualTo(""));
    }

    [Test]
    public static void WrapDoesNotCopyAndSliceSharesTheArray()
    {
        var array = Encoding.ASCII.GetBytes("xx1.5E1yy");
        var all = Bytes.Wrap(array);
        var slice = all.SubBytes(2, 5);
        Assert.That(slice.Count.Value, Is.EqualTo(5));
        array[2] = (byte)'9';   // the host changes the array: both views see it
        Assert.That(all.At(2).Value, Is.EqualTo('9'));
        Assert.That(slice.At(0).Value, Is.EqualTo('9'));
        Assert.That(slice.SubBytes(1, 2).AsSpan().ToArray(), Is.EqualTo(new[] { (byte)'.', (byte)'5' }));
    }

    [Test]
    public static void TheDefaultAndANullArrayAreTheEmptyView()
    {
        Assert.That(default(Bytes).Count.Value, Is.EqualTo(0));
        Assert.That(Bytes.Wrap(null).Count.Value, Is.EqualTo(0));
        Assert.That(default(Bytes).SubBytes(0, 5).Count.Value, Is.EqualTo(0));
        Assert.That(default(Bytes).SequenceEquals(B("")), Is.True);
        Assert.That(default(Bytes).Hash().Value, Is.EqualTo(unchecked((IntegerValue)2166136261u)));
    }

    [Test]
    public static void AtFaultsOutsideTheViewEvenInsideTheArray()
    {
        var view = B("abcd").SubBytes(1, 2);
        Assert.That(view.At(1).Value, Is.EqualTo('c'));
        Assert.Throws<IndexOutOfRangeException>(() => view.At(2));    // the array has a 'd' there
        Assert.Throws<IndexOutOfRangeException>(() => view.At(-1));   // and an 'a' there
        Assert.Throws<IndexOutOfRangeException>(() => default(Bytes).At(0));
    }

    [Test]
    public static void WrapWithARangeChecksTheRange()
    {
        var array = new byte[4];
        Assert.That(Bytes.Wrap(array, 1, 3).Count.Value, Is.EqualTo(3));
        Assert.That(Bytes.Wrap(array, 4, 0).Count.Value, Is.EqualTo(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Bytes.Wrap(array, 2, 3));
        Assert.Throws<ArgumentOutOfRangeException>(() => Bytes.Wrap(array, -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Bytes.Wrap(array, 0, -1));
    }

    [TestCase("", 2166136261u)]
    [TestCase("a", 0xE40C292Cu)]
    [TestCase("foo", 0xA9F37ED7u)]
    [TestCase("foobar", 0xBF9CF968u)]
    public static void HashIsFnv1a32(string text, uint expected)
        => Assert.That(B(text).Hash().Value, Is.EqualTo(unchecked((IntegerValue)expected)));

    [Test]
    public static void HashIgnoresTheArrayAndTheStart()
        => Assert.That(B("zzfoobarzz").SubBytes(2, 6).Hash().Value, Is.EqualTo(B("foobar").Hash().Value));

    [Test]
    public static void OrderIsOrdinalOnUnsignedBytes()
    {
        Assert.That(Bytes.Wrap(new byte[] { 255 }) > Bytes.Wrap(new byte[] { 1 }), Is.True);
        Assert.That(B("B") < B("a"), Is.True);
        Assert.That(B("ab") < B("abc"), Is.True);
        Assert.That(B("abc") <= B("abc") && B("abc") >= B("abc") && !(B("abc") < B("abc")), Is.True);
        Assert.That(B("") < B("a"), Is.True);
    }

    [TestCase("1.", 1.0)]
    [TestCase("-0.5E-3", -0.0005)]
    [TestCase(".5", 0.5)]
    [TestCase("+2", 2.0)]
    [TestCase("1.E-06", 1e-6)]
    [TestCase("1e-400", 0.0)]
    [TestCase("9007199254740993", 9007199254740992.0)]
    public static void NumberFromBytesReadsTheP21Forms(string text, double expected)
        => Assert.That((double)B(text).NumberFromBytes().Value, Is.EqualTo(expected));

    [TestCase("")]
    [TestCase(".")]
    [TestCase("1e")]
    [TestCase("e5")]
    [TestCase("NaN")]
    [TestCase("Infinity")]
    [TestCase("0x10")]
    [TestCase(" 1")]
    [TestCase("1 ")]
    [TestCase("1,5")]
    [TestCase("--1")]
    [TestCase("1.5.2")]
    [TestCase("1e999")]
    public static void NumberFromBytesIsNaNWhenTheFormIsWrong(string text)
        => Assert.That(NumberValue.IsNaN(B(text).NumberFromBytes().Value), Is.True);

    [Test]
    public static void NumberFromBytesKeepsNegativeZeroAndReadsOnlyItsRange()
    {
        Assert.That(NumberValue.IsNegative(B("-0").NumberFromBytes().Value), Is.True);
        Assert.That(NumberValue.IsNegative(B("0").NumberFromBytes().Value), Is.False);
        Assert.That((double)B("xx1.5E1yy").SubBytes(2, 5).NumberFromBytes().Value, Is.EqualTo(15.0));
        Assert.That(NumberValue.IsNaN(B("xx1.5E1yy").SubBytes(2, 6).NumberFromBytes().Value), Is.True);
    }

    [Test]
    public static void NumberFromBytesAgreesWithNumberFromTextOnTheSamePrecision()
    {
        foreach (var text in new[] { "0.1", "123456.789", "2.5e-7", "-3.25", "1e22", "4.9e-324", "1.7976931348623157e308" })
            Assert.That(B(text).NumberFromBytes().Value, Is.EqualTo(((String)text).NumberFromText().Value), text);
    }

    [Test]
    public static void FromIntegersKeepsTheLowEightBits()
    {
        var made = new Integer[] { 97, 256 + 98, -1 }.FromIntegers();
        Assert.That(made.AsSpan().ToArray(), Is.EqualTo(new byte[] { 97, 98, 255 }));
        Assert.That(Array.Empty<Integer>().FromIntegers().Count.Value, Is.EqualTo(0));
    }
}
