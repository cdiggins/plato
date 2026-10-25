// Checks generation of values constrained by sealed constructors.
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-444: the law runner draws a sealed type (<c>sealed by F, G</c>) without a generator only
/// from the functions its declaration names, called on drawn arguments, never field by field, as a
/// law argument and as a field of another value, on every backend. The path is TKT-446's: a type
/// with a <c>Generate_</c> function still draws from it.
/// </summary>
[TestFixture]
public class SealedGeneratorTests
{
    private const string Source = """
        type Check
            sealed by CheckWithin, WorseOf
        {
            Worst: Number;
            Bound: Number;
            Depth: Number;
        }

        type Report
        {
            Main: Check;
            Scale: Number;
        }

        type Orphan
            sealed by Make
        {
            Value: Number;
        }

        type Faulty
            sealed by Explode
        {
            Value: Number;
        }

        type Generated
            sealed by Wrap
        {
            Value: Number;
        }

        library SealedFixture
        {
            CheckWithin(worst: Number, slack: Number): Check => Check(worst * worst, worst * worst + slack * slack + 1.0, 0.0);
            WorseOf(a: Check, b: Check): Check => Check(a.Worst + b.Worst, a.Bound + b.Bound, a.Depth + b.Depth + 1.0);
            Make(x: Number): Orphan;
            Explode(n: Integer): Faulty => Faulty((1 / (n - n)).ToNumber);
            Wrap(x: Number): Generated => Generated(x);
            Generate_Generated(seed: Integer): Generated => Wrap(100.0 + seed.ToNumber);

            Law_CheckHolds(c: Check): Boolean => c.Worst < c.Bound;
            Law_FieldCheckHolds(r: Report): Boolean => r.Main.Worst < r.Main.Bound;
            Law_OrphanDraw(o: Orphan): Boolean => true;
            Law_FaultyDraw(f: Faulty): Boolean => true;
            Law_GeneratedFromGenerator(g: Generated): Boolean => g.Value >= 100.0;
        }
        """;

    private static readonly Lazy<Ara3D.Geometry.Interpreter.Interpreter> _interp = new(() =>
        new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64));

    private static TypeExpression TypeNamed(string name)
        => _interp.Value.Compilation.GetTypeDef(name).ToTypeExpression();

    private static double Field(object v, int i)
        => (double)((PlatoRecord)v).Fields[i];

    [Test]
    public void A_sealed_type_is_drawn_only_from_its_named_constructors_and_composes_them()
    {
        var gen = new ValueGenerator(_interp.Value);
        Assert.That(gen.SealedConstructorsOf(TypeNamed("Check")).Select(f => f.Name), Is.EquivalentTo(new[] { "CheckWithin", "WorseOf" }));
        Assert.That(gen.SealedConstructorsOf(TypeNamed("Report")), Is.Empty);
        Assert.That(gen.FiniteDomain(TypeNamed("Check")), Is.Null);

        var rng = new Random(11);
        var depths = new List<double>();
        for (var i = 0; i < 500; i++)
        {
            var check = gen.Generate(TypeNamed("Check"), rng);
            Assert.That(Field(check, 0), Is.LessThan(Field(check, 1)), "only CheckWithin and WorseOf build a Check");
            depths.Add(Field(check, 2));
            var report = (PlatoRecord)gen.Generate(TypeNamed("Report"), rng);
            Assert.That(Field(report.Fields[0], 0), Is.LessThan(Field(report.Fields[0], 1)), "a field of another value comes from the constructors too");
        }
        Assert.That(depths, Has.Some.GreaterThan(0.0), "WorseOf composes drawn Checks");
        Assert.That(depths.Max(), Is.LessThan(Math.Pow(2, ValueGenerator.MaxDepth)), "compositions stop at the depth limit");
    }

    [Test]
    public void A_sealed_type_whose_constructors_have_no_body_cannot_be_drawn()
    {
        var gen = new ValueGenerator(_interp.Value);
        Assert.That(gen.CanGenerate(TypeNamed("Orphan"), out var reason), Is.False);
        Assert.That(reason, Is.EqualTo("Orphan is sealed by Make, and none of them has a body"));
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp, Category = "Slow")]
    [TestCase(LawBackendKind.Differential, Category = "Slow")]
    public void Laws_over_sealed_types_see_only_values_their_constructors_build(LawBackendKind kind)
    {
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64);
        var options = new LawRunOptions { Filters = new[] { "SealedFixture" }, Trials = 200 };
        var runner = kind == LawBackendKind.Interpreter
            ? new LawRunner(interp, options)
            : new LawRunner(LawBackends.Create(kind, interp), interp, options);
        var by = runner.RunAll().ToDictionary(r => r.Law.Name, r => r);
        TestContext.Out.WriteLine(string.Join("\n", by.Values.Select(r => r.ToString())));

        Assert.That(by["Law_CheckHolds"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_FieldCheckHolds"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_GeneratedFromGenerator"].Status, Is.EqualTo(LawStatus.Pass), "a generator still wins over the named constructors");
        Assert.That(by["Law_OrphanDraw"].Status, Is.Not.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_OrphanDraw"].Message, Does.Contain("Orphan is sealed by Make, and none of them has a body"));
        Assert.That(by["Law_FaultyDraw"].Status, Is.EqualTo(LawStatus.Error));
        Assert.That(by["Law_FaultyDraw"].Message, Does.Contain("sealed constructor SealedFixture.Explode faulted on drawn arguments"));
    }
}
