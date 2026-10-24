// Checks generated law inputs against their requested Plato types.
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-446: a declared type the library gives a <c>Generate_</c> function is drawn only from its
/// generators and witnesses, never built field by field, both as a law argument and as a field of
/// another value, on every backend. Before, the runner built a tolerance from a random Number and
/// handed laws a tolerance of 0 or -0.5 that no constructor of the library makes.
/// </summary>
[TestFixture]
public class GeneratorTests
{
    private const string Source = """
        type Tolerance
        {
            Value: Number;
        }

        type Spread
        {
            Value: Number;
        }

        type Holder
        {
            Tol: Tolerance;
            Scale: Number;
        }

        type Plain
        {
            Value: Number;
        }

        type Faulty
        {
            Value: Number;
        }

        library GeneratorFixture
        {
            Generate_Tolerance(seed: Integer): Tolerance => new Tolerance(0.001 * (seed + 1).ToNumber);
            Witness_CoarseTolerance(_: Tolerance): Tolerance => new Tolerance(0.5);
            Generate_Spread(_: Spread, seed: Integer): Spread => new Spread(seed.ToNumber + 1.0);
            Generate_Faulty(seed: Integer): Faulty => new Faulty((1 / (seed - seed)).ToNumber);

            Law_TolerancePositive(t: Tolerance): Boolean => t.Value > 0.0;
            Law_FieldTolerancePositive(h: Holder): Boolean => h.Tol.Value > 0.0;
            Law_SpreadAtLeastOne(s: Spread): Boolean => s.Value >= 1.0;
            Law_PlainPositive(p: Plain): Boolean => p.Value > 0.0;
            Law_FaultyDraw(f: Faulty): Boolean => true;
        }
        """;

    private static readonly Lazy<Ara3D.Geometry.Interpreter.Interpreter> _interp = new(() =>
        new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64));

    private static TypeExpression TypeNamed(string name)
        => _interp.Value.Compilation.GetTypeDef(name).ToTypeExpression();

    private static double ValueOf(object v)
        => (double)((PlatoRecord)v).Fields[0];

    [Test]
    public void Generators_are_found_in_both_forms_and_only_for_their_type()
    {
        var gen = new ValueGenerator(_interp.Value);
        Assert.That(gen.GeneratorsOf(TypeNamed("Tolerance")).Select(f => f.Name), Is.EqualTo(new[] { "Generate_Tolerance" }));
        Assert.That(gen.GeneratorsOf(TypeNamed("Spread")).Select(f => f.Name), Is.EqualTo(new[] { "Generate_Spread" }));
        Assert.That(gen.GeneratorsOf(TypeNamed("Plain")), Is.Empty);
        Assert.That(gen.GeneratorsOf(_interp.Value.PrimitiveType("Number")), Is.Empty);
        Assert.That(gen.FiniteDomain(TypeNamed("Tolerance")), Is.Null);
    }

    [Test]
    public void A_type_with_a_generator_never_receives_a_value_built_field_by_field()
    {
        var gen = new ValueGenerator(_interp.Value);
        var rng = new Random(7);
        for (var i = 0; i < 500; i++)
        {
            Assert.That(ValueOf(gen.Generate(TypeNamed("Tolerance"), rng)), Is.GreaterThan(0.0));
            var holder = (PlatoRecord)gen.Generate(TypeNamed("Holder"), rng);
            Assert.That(ValueOf(holder.Fields[0]), Is.GreaterThan(0.0), "a field of another value comes from the generator too");
        }
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    [TestCase(LawBackendKind.Differential)]
    public void Laws_over_generated_types_see_only_valid_values(LawBackendKind kind)
    {
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64);
        var options = new LawRunOptions { Filters = new[] { "GeneratorFixture" }, Trials = 200 };
        var runner = kind == LawBackendKind.Interpreter
            ? new LawRunner(interp, options)
            : new LawRunner(LawBackends.Create(kind, interp), interp, options);
        var by = runner.RunAll().ToDictionary(r => r.Law.Name, r => r);
        TestContext.Out.WriteLine(string.Join("\n", by.Values.Select(r => r.ToString())));

        Assert.That(by["Law_TolerancePositive"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_FieldTolerancePositive"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_SpreadAtLeastOne"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_PlainPositive"].Status, Is.EqualTo(LawStatus.Fail), "a type with no generator is still built field by field");
        Assert.That(by["Law_FaultyDraw"].Status, Is.EqualTo(LawStatus.Error));
        Assert.That(by["Law_FaultyDraw"].Message, Does.Contain("generator GeneratorFixture.Generate_Faulty faulted on seed"));
    }
}
