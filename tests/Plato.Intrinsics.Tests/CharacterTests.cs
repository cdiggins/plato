using Ara3D.Geometry;
using NUnit.Framework;

namespace Plato.IntrinsicsTests;

/// <summary>
/// The <see cref="Character"/> wrapper: a char, its code-unit bridge to <see cref="Integer"/>,
/// and ordinal ordering.
/// </summary>
[TestFixture]
public static class CharacterTests
{
    private static Character C(char c)
        => c;

    [TestCase('a')]
    [TestCase('Z')]
    [TestCase('0')]
    [TestCase('\0')]
    [TestCase('￿')]
    public static void ConversionsRoundTrip(char c)
    {
        Assert.Multiple(() =>
        {
            Assert.That(new Character(c).Value, Is.EqualTo(c));
            Assert.That(Character.FromSystem(c).Value, Is.EqualTo(c));
            Assert.That(C(c).ToSystem(), Is.EqualTo(c));
            Assert.That((char)C(c), Is.EqualTo(c));
        });
    }

    [Test]
    public static void DefaultIsTheNullCharacter()
        => Assert.That(default(Character).Value, Is.EqualTo('\0'));

    [Test]
    public static void TheIntegerBridgeIsTheCodeUnit()
    {
        Integer code = C('A');
        Assert.That(code.Value, Is.EqualTo(65));

        Character fromCode = (Integer)66;
        Assert.That(fromCode.Value, Is.EqualTo('B'));
    }

    [Test]
    public static void TheIntegerBridgeTruncatesToSixteenBits()
    {
        Character c = (Integer)(65536 + 65);
        Assert.That(c.Value, Is.EqualTo('A'), "(char) narrowing keeps the low 16 bits");
    }

    [Test]
    public static void OrderingIsOrdinal()
    {
        Assert.Multiple(() =>
        {
            Assert.That(C('a') < C('b'));
            Assert.That(C('b') > C('a'));
            Assert.That(C('a') <= C('a'));
            Assert.That(C('a') >= C('a'));
            Assert.That(C('Z') < C('a'), "uppercase sorts before lowercase in code-unit order");
            Assert.That(C('a') < C('A'), Is.False);
        });
    }

    [Test]
    public static void EqualValuesHaveEqualHashCodes()
    {
        var a = C('q');
        var b = C('q');
        Assert.That(a.Equals(b));
        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        Assert.That(a.Equals(C('r')), Is.False);
    }
}
