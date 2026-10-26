// Checks the profile that keeps binary64 numbers and 32-bit integers.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// <c>cpu64i32</c> is binary64 <c>Number</c> with a 32-bit <c>Integer</c>. The interpreter wraps
/// integers at that width and leaves sums past 2^24 exact, and the C# law backend defines only
/// the integer-width symbol so the runtime binds <c>double</c> and <c>int</c>.
/// </summary>
[TestFixture]
public class Cpu64I32ProfileTests
{
    [Test]
    public void ProfileBindsBinary64AndInt32()
    {
        var profile = NumberProfile.TryParse("cpu64i32");
        Assert.That(profile, Is.SameAs(NumberProfile.Cpu64I32));
        Assert.That(profile.NumberBits, Is.EqualTo(64));
        Assert.That(profile.Precision, Is.EqualTo(53));
        Assert.That(profile.IntegerBits, Is.EqualTo(32));
    }

    [Test]
    public void InterpreterKeepsBinary64AndWrapsIntegers()
    {
        var numbers = new Numbers(NumberProfile.Cpu64I32);
        Assert.That(numbers.Add(16777216, 1), Is.EqualTo(16777217d));
        Assert.That(numbers.I((long)int.MaxValue + 1), Is.EqualTo(int.MinValue));
        Assert.That(numbers.ToNumber(int.MinValue), Is.EqualTo((double)int.MinValue));
        Assert.That(numbers.IntegerMinValue, Is.EqualTo(int.MinValue));
        Assert.That(numbers.IntegerMaxValue, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void LawBackendDefinesOnlyTheIntegerWidthSymbol()
    {
        var symbols = CSharpProgram.ProfileSymbols(NumberProfile.Cpu64I32);
        Assert.That(symbols, Is.EqualTo(new[] { "PLATO_FORWARD_CONCEPTS", "PLATO_INTEGER_32" }));
        Assert.That(CSharpProgram.ProfileSymbols(NumberProfile.Cpu64), Is.EqualTo(new[] { "PLATO_FORWARD_CONCEPTS" }));
        Assert.That(CSharpProgram.ProfileSymbols(NumberProfile.Gpu32),
            Is.EqualTo(new[] { "PLATO_FORWARD_CONCEPTS", "PLATO_NUMBER_GPU32", "PLATO_INTEGER_32" }));
    }

    [Test]
    public void IntegerLiteralsNarrowWhenTheyAreNotCSharpIntLiterals()
    {
        Assert.That(CSharpWriter.IntegerLiteral(5, 32), Is.EqualTo("5"));
        Assert.That(CSharpWriter.IntegerLiteral(int.MaxValue, 32), Is.EqualTo("2147483647"));
        Assert.That(CSharpWriter.IntegerLiteral(int.MinValue, 32), Is.EqualTo("unchecked((int)(-2147483648L))"));
        Assert.That(CSharpWriter.IntegerLiteral((long)int.MaxValue + 1, 32), Is.EqualTo("unchecked((int)(2147483648L))"));
        Assert.That(CSharpWriter.IntegerLiteral(int.MinValue, 64), Is.EqualTo("-2147483648"));
    }
}
