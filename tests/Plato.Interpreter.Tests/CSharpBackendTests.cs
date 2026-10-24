// Exercises laws through generated and compiled C#.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// The compiled backend (src/Plato.Laws) reaches the same verdicts as the interpreter over the
/// same witnesses: laws on records, sums, Integers, witnesses, an interface-typed receiver (an
/// instance method on each implementing struct once compiled), a bodiless function (an
/// obligation), and a failing law whose witness is formatted the interpreter's way.
/// </summary>
[TestFixture]
public class CSharpBackendTests
{
    private const string Source = """
        type Pair
        {
            A: Number;
            B: Number;
        }

        type Shade
            = Light
            | Dark;

        interface IHasSize
        {
            Size(x: Self): Number;
        }

        type Box implements IHasSize
        {
            Width: Number;
        }

        type Disk implements IHasSize
        {
            Radius: Number;
        }

        library CSharpBackendFixture
        {
            Size(b: Box): Number => b.Width;
            Size(d: Disk): Number => d.Radius * 2.0;
            Deferred(p: Pair): Number;
            Witness_UnitPair(_: Pair): Pair => new Pair(1.0, 1.0);
            Law_PairSumIsCommutative(p: Pair, q: Pair): Boolean => (p.A + q.B).Equals(q.B + p.A);
            Law_ShadeIsLightOrDark(s: Shade): Boolean => true;
            Law_IntegerTimesOne(i: Integer): Boolean => (i * 1).Equals(i);
            Law_SizeIsNotNegativeOfItself(x: IHasSize): Boolean => x.Size.Equals(x.Size);
            Law_DeferredIsObligation(p: Pair): Boolean => p.Deferred.Equals(p.Deferred);
            Law_FirstIsNotNegative(p: Pair): Boolean => p.A >= 0.0;
            Law_UnitPairWitnessIsSeen(p: Pair): Boolean => !p.A.Equals(1.0) || !p.B.Equals(1.0) || p.A.Equals(p.B);
        }
        """;

    private static readonly Lazy<(IReadOnlyList<LawResult> Interpreter, IReadOnlyList<LawResult> CSharp)> _runs = new(() =>
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var options = new LawRunOptions { Filters = new[] { "CSharpBackendFixture" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var interpreted = new LawRunner(interp, options).RunAll();
        var compiled = new LawRunner(LawBackends.Create(LawBackendKind.CSharp, interp), interp, options).RunAll();
        return (interpreted, compiled);
    });

    private static IReadOnlyList<LawResult> Interpreted => _runs.Value.Interpreter;
    private static IReadOnlyList<LawResult> Compiled => _runs.Value.CSharp;

    private static string Verdicts(IEnumerable<LawResult> results)
        => string.Join("\n", results.OrderBy(r => r.Name, StringComparer.Ordinal).Select(r => $"{r.Name} {r.Status.ToString().ToLowerInvariant()} {r.Trials}"));

    [Test]
    public void Both_backends_reach_the_same_verdicts_over_the_same_number_of_cases()
    {
        TestContext.Out.WriteLine(Verdicts(Compiled));
        Assert.That(Compiled, Has.Count.EqualTo(7));
        Assert.That(Verdicts(Compiled), Is.EqualTo(Verdicts(Interpreted)));
    }

    [Test]
    public void Statuses_cover_pass_fail_and_obligation()
    {
        var by = Compiled.ToDictionary(r => r.Name.Substring(r.Name.IndexOf('.') + 1), r => r);
        Assert.That(by["Law_PairSumIsCommutative"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_ShadeIsLightOrDark"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_ShadeIsLightOrDark"].Exhaustive, Is.True);
        Assert.That(by["Law_IntegerTimesOne"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_SizeIsNotNegativeOfItself"].Status, Is.EqualTo(LawStatus.Pass), "an interface-typed receiver dispatches to the implementing struct's method");
        Assert.That(by["Law_DeferredIsObligation"].Status, Is.EqualTo(LawStatus.Obligation));
        Assert.That(by["Law_DeferredIsObligation"].Message, Does.Contain("Deferred"));
        Assert.That(by["Law_FirstIsNotNegative"].Status, Is.EqualTo(LawStatus.Fail));
        Assert.That(by["Law_UnitPairWitnessIsSeen"].Status, Is.EqualTo(LawStatus.Pass));
    }

    [Test]
    public void The_failing_witness_is_formatted_as_the_interpreter_formats_it()
    {
        var compiled = Compiled.Single(r => r.Name.EndsWith("Law_FirstIsNotNegative"));
        var interpreted = Interpreted.Single(r => r.Name.EndsWith("Law_FirstIsNotNegative"));
        Assert.That(compiled.FormatWitness(), Does.StartWith("p = Pair("));
        Assert.That(compiled.FormatWitness(), Is.EqualTo(interpreted.FormatWitness()));
    }

    [Test]
    public void Elapsed_time_is_recorded_and_reported_in_milliseconds()
    {
        var verdicts = LawReport.Verdicts(Compiled);
        Assert.That(verdicts.Select(v => v.ElapsedMs), Has.All.Not.Null);
        var line = LawReport.Format(verdicts).Split('\n')[0].Split('\t');
        Assert.That(line, Has.Length.EqualTo(4));
        Assert.That(long.TryParse(line[2], out _), Is.True, "the third column is the elapsed milliseconds");
    }

    [Test]
    public void Backend_names_parse()
    {
        Assert.That(LawBackends.TryParse("csharp"), Is.EqualTo(LawBackendKind.CSharp));
        Assert.That(LawBackends.TryParse("Interpreter"), Is.EqualTo(LawBackendKind.Interpreter));
        Assert.That(LawBackends.TryParse("rust"), Is.Null);
    }
}
