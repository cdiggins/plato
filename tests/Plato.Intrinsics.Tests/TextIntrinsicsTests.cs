// Checks the text intrinsics the STEP parser needs: FromCharacters, Substring, number and integer parsing.
using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The host half of <c>FromCharacters</c>, <c>Substring</c>, <c>IsNumberText</c>,
/// <c>NumberFromText</c>, <c>IsIntegerText</c> and <c>IntegerFromText</c>
/// (<c>stdlib/foundation/intrinsics.library.plato</c>). The laws in <c>stdlib/tests/text.laws.plato</c>
/// hold the shared witnesses; these cases add what depends on the profile's widths.
/// </summary>
[TestFixture]
public static class TextIntrinsicsTests
{
    private static Ara3D.Geometry.String S(string s) => new(s);

    [Test]
    public static void FromCharactersBuildsTheStringInOrder()
    {
        Character[] cs = { 'a', 'b', 'c' };
        Assert.That(cs.FromCharacters().Value, Is.EqualTo("abc"));
        Assert.That(Array.Empty<Character>().FromCharacters().Value, Is.EqualTo(""));
    }

    [TestCase("abcd", 1, 2, "bc")]
    [TestCase("abcd", -1, 3, "ab")]
    [TestCase("abcd", 3, 9, "d")]
    [TestCase("abcd", 0, 0, "")]
    [TestCase("abcd", 0, -4, "")]
    [TestCase("abcd", 4, 1, "")]
    [TestCase("abcd", 99, 1, "")]
    [TestCase("abcd", -99, 99, "")]
    [TestCase("abcd", -1, 99, "abcd")]
    [TestCase("", 0, 1, "")]
    public static void SubstringIsTheClampedRange(string s, int start, int count, string expected)
        => Assert.That(S(s).Substring(start, count).Value, Is.EqualTo(expected));

    [Test]
    public static void SubstringNeverFailsOnTheExtremes()
    {
        var text = S("abcd");
        Assert.That(text.Substring(IntegerValue.MinValue, IntegerValue.MaxValue).Value, Is.EqualTo(""));
        Assert.That(text.Substring(IntegerValue.MaxValue, IntegerValue.MaxValue).Value, Is.EqualTo(""));
        Assert.That(text.Substring(IntegerValue.MinValue, IntegerValue.MinValue).Value, Is.EqualTo(""));
        Assert.That(text.Substring(-1, IntegerValue.MaxValue).Value, Is.EqualTo("abcd"));
        Assert.That(text.Substring(2, IntegerValue.MaxValue).Value, Is.EqualTo("cd"));
    }

    [TestCase("1.5E1", 15.0)]
    [TestCase("1.", 1.0)]
    [TestCase(".5", 0.5)]
    [TestCase("1.E-06", 1e-6)]
    [TestCase("-2.5e+1", -25.0)]
    [TestCase("+7", 7.0)]
    [TestCase("0.1", 0.1)]
    [TestCase("9007199254740993", 9007199254740992.0)]
    [TestCase("9007199254740995", 9007199254740996.0)]
    [TestCase("1e-400", 0.0)]
    [TestCase("4.9e-324", double.Epsilon)]
    [TestCase("1.7976931348623157e308", double.MaxValue)]
    public static void ParsesTheAcceptedRealForms(string text, double expected)
    {
        Assert.That(S(text).IsNumberText().Value, Is.True);
        Assert.That(BitConverter.DoubleToInt64Bits(S(text).NumberFromText().Value),
            Is.EqualTo(BitConverter.DoubleToInt64Bits((NumberValue)expected)));
    }

    [Test]
    public static void NegativeZeroSurvivesParsing()
        => Assert.That(NumberValue.IsNegative(S("-0").NumberFromText().Value), Is.True);

    [TestCase("")]
    [TestCase(".")]
    [TestCase("-")]
    [TestCase("1e")]
    [TestCase("1e+")]
    [TestCase(".e1")]
    [TestCase("NaN")]
    [TestCase("Infinity")]
    [TestCase("-Infinity")]
    [TestCase("0x10")]
    [TestCase(" 1")]
    [TestCase("1 ")]
    [TestCase("1,5")]
    [TestCase("1.5.2")]
    [TestCase("--1")]
    [TestCase("1e999")]
    [TestCase("١")]
    public static void RejectsEverythingElseWithZero(string text)
    {
        Assert.That(S(text).IsNumberText().Value, Is.False);
        Assert.That(S(text).NumberFromText().Value, Is.EqualTo((NumberValue)0));
    }

    [TestCase("-12", -12L)]
    [TestCase("+7", 7L)]
    [TestCase("007", 7L)]
    [TestCase("-0", 0L)]
    [TestCase("2147483647", int.MaxValue)]
    [TestCase("-2147483648", int.MinValue)]
    public static void ParsesIntegersThatFitEveryProfile(string text, long expected)
    {
        Assert.That(S(text).IsIntegerText().Value, Is.True);
        Assert.That(S(text).IntegerFromText().Value, Is.EqualTo((IntegerValue)expected));
    }

    [TestCase("")]
    [TestCase("+")]
    [TestCase("-")]
    [TestCase("1.0")]
    [TestCase("1e3")]
    [TestCase(" 1")]
    [TestCase("1 ")]
    [TestCase("--1")]
    [TestCase("0x10")]
    [TestCase("99999999999999999999")]
    [TestCase("-99999999999999999999")]
    [TestCase("١")]
    public static void RejectsNonIntegersAndOverflowWithZero(string text)
    {
        Assert.That(S(text).IsIntegerText().Value, Is.False);
        Assert.That(S(text).IntegerFromText().Value, Is.EqualTo((IntegerValue)0));
    }

    [TestCase(0.5, "0.5")]
    [TestCase(100.0, "100")]
    [TestCase(1e15, "1000000000000000")]
    [TestCase(1e16, "10000000000000000")]
    [TestCase(1e17, "1E+17")]
    [TestCase(123456789012345.0, "123456789012345")]
    [TestCase(1e-5, "1E-05")]
    [TestCase(0.0001, "0.0001")]
    [TestCase(-1.2345e-7, "-1.2345E-07")]
    [TestCase(double.NaN, "NaN")]
    [TestCase(double.PositiveInfinity, "Infinity")]
    [TestCase(double.NegativeInfinity, "-Infinity")]
    public static void ShortestStringLayout(double x, string expected)
    {
        if (PlatoNumberProfile.Precision != 53)
            Assert.Ignore("the layout cases are binary64 digits");
        Assert.That(((Number)(NumberValue)x).NumberToShortestString().Value, Is.EqualTo(expected));
    }

    [Test]
    public static void NegativeZeroHasTheTextMinusZero()
    {
        var negativeZero = (Number)NumberValue.NegativeZero;
        Assert.That(negativeZero.NumberToShortestString().Value, Is.EqualTo("-0"));
        Assert.That(NumberValue.IsNegative(negativeZero.NumberToShortestString().NumberFromText().Value), Is.True);
    }

    [Test]
    public static void ShortestStringRoundTripsBitForBit()
    {
        var random = new Random(12345);
        var values = new List<NumberValue> { 0.1f, 0.1f + 0.2f, 1f / 3f, NumberValue.Epsilon, NumberValue.MaxValue, NumberValue.MinValue, -1e22f, 4.35f, 1e15f, 1e-5f };
        for (var i = 0; i < 20000; i++)
        {
            var bits = new byte[8];
            random.NextBytes(bits);
            var d = BitConverter.ToDouble(bits, 0);
            if (!double.IsFinite(d))
                continue;
            values.Add((NumberValue)d);
            values.Add((NumberValue)(random.NextDouble() * 20.0 - 10.0));
        }
        foreach (var x in values.Where(v => NumberValue.IsFinite(v)))
        {
            var text = ((Number)x).NumberToShortestString();
            Assert.That(text.IsNumberText().Value, Is.True, text.Value);
            Assert.That(BitConverter.DoubleToInt64Bits(text.NumberFromText().Value), Is.EqualTo(BitConverter.DoubleToInt64Bits(x)), text.Value);
        }
    }

    [Test]
    public static void IntegerOverflowFollowsTheProfileWidth()
    {
        var max = IntegerValue.MaxValue.ToString();
        var min = IntegerValue.MinValue.ToString();
        Assert.That(S(max).IsIntegerText().Value, Is.True);
        Assert.That(S(min).IsIntegerText().Value, Is.True);
        Assert.That(S(max).IntegerFromText().Value, Is.EqualTo(IntegerValue.MaxValue));
        Assert.That(S(min).IntegerFromText().Value, Is.EqualTo(IntegerValue.MinValue));
        var pastMax = ((System.Numerics.BigInteger)IntegerValue.MaxValue + 1).ToString();
        var pastMin = ((System.Numerics.BigInteger)IntegerValue.MinValue - 1).ToString();
        Assert.That(S(pastMax).IsIntegerText().Value, Is.False);
        Assert.That(S(pastMin).IsIntegerText().Value, Is.False);
    }
}
