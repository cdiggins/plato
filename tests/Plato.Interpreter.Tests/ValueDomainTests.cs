// Checks the value domains used to generate law inputs.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>A law parameter typed by a primitive declaration (Number, String, Character, an Array)
/// is drawn from generated values, never given the one-value domain of a zero-field record: before
/// 1a3298fa every such parameter received a <c>Number()</c> record and the first host intrinsic it
/// reached faulted with a cast error. Each law below sends its parameter through an intrinsic.</summary>
[TestFixture]
public class ValueDomainTests
{
    private const string Source = """
        library ValueDomainFixture
        {
            Law_NumberParameterIsANumber(x: Number): Boolean => x * 1.0 == x || x != x;
            Law_StringParameterIsAString(s: String): Boolean => s.Count >= 0;
            Law_CharacterParameterIsACharacter(c: Character): Boolean => c <= c;
            Law_ArrayParameterHoldsNumbers(xs: Array<Number>): Boolean => xs.All(x => x * 1.0 == x || x != x);
            Law_NumberBesideAnInteger(x: Number, k: Integer): Boolean => x + k.ToNumber == k.ToNumber + x || x != x;
        }
        """;

    private static readonly Lazy<Ara3D.Geometry.Interpreter.Interpreter> _interp = new(() =>
        new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64));

    [Test]
    public void Primitive_declarations_have_no_finite_domain()
    {
        var gen = new ValueGenerator(_interp.Value);
        foreach (var name in new[] { "Number", "String", "Character", "Array" })
            Assert.That(gen.FiniteDomain(_interp.Value.PrimitiveType(name)), Is.Null, name);
    }

    [Test]
    public void Laws_over_primitive_parameters_get_verdicts()
    {
        var results = new LawRunner(_interp.Value, new LawRunOptions { Filters = new[] { "ValueDomainFixture" } }).RunAll();
        Assert.That(results.Select(r => r.Name).ToList(), Has.Count.EqualTo(5));
        Assert.That(results.Where(r => r.Status != LawStatus.Pass).Select(r => r.ToString()), Is.Empty);
        Assert.That(results.All(r => r.Trials > 1), Is.True, "sampled, not one value");
    }
}
