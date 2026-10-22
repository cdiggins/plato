using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-438: the differential backend runs every call of a law run in the interpreter and on the
/// compiled C#. Where the engines agree the verdicts are the ordinary ones; where they do not,
/// the law is <c>differs</c> with both answers, and a witness built differently is reported and
/// dropped. The disagreement is forced through a compiled arm that answers one function wrongly.
/// </summary>
[TestFixture]
public class DifferentialBackendTests
{
    private const string Source = """
        type Pair
        {
            A: Number;
            B: Number;
        }

        type Label
        {
            Text: String;
            Size: Number;
        }

        library DifferentialFixture
        {
            // A String field: the compiled value reads back as the same Plato string.
            Witness_Label(_: Label): Label => new Label("unit", 1.0);
            Law_LabelIsSized(l: Label): Boolean => l.Size > 0.0 || l.Size <= 0.0;
            Witness_UnitPair(_: Pair): Pair => new Pair(1.0, 2.0);
            Witness_Skewed(_: Pair): Pair => new Pair(3.0, 4.0);
            Twice(x: Number): Number => x * 2.0;
            Law_SumIsCommutative(p: Pair): Boolean => (p.A + p.B) == (p.B + p.A);
            Law_TwiceIsAddition(k: Integer): Boolean => Twice(k.ToNumber) == k.ToNumber + k.ToNumber;
            Law_FirstBelowTen(p: Pair): Boolean => p.A < 10.0;
            Law_AlwaysFalse(k: Integer): Boolean => k > 100;
        }
        """;

    /// <summary>The C# backend, except that <c>Law_TwiceIsAddition</c> answers the opposite, and
    /// <c>Witness_Skewed</c> is built with A and B exchanged. The differential backend compares
    /// the calls the runner makes (laws, witnesses, companions), not the calls inside them.</summary>
    private sealed class Lying : ILawBackend
    {
        private readonly CSharpBackend _real;
        public Lying(CSharpBackend real) => _real = real;
        public string Name => "lying";
        public bool SupportsParallel => true;
        public void SetDeadline(DateTime? deadline) { }
        public object FromInterpreterValue(object v) => _real.FromInterpreterValue(v);
        public object ToInterpreterValue(object v) => _real.ToInterpreterValue(v);
        public object Invoke(FunctionDef f, object[] args)
        {
            if (f.Name == "Law_TwiceIsAddition")
                return _real.FromInterpreterValue(!(bool)_real.ToInterpreterValue(_real.Invoke(f, args)));
            if (f.Name == "Witness_Skewed")
                return _real.FromInterpreterValue(new PlatoRecordProbe(_real).Swap(_real.Invoke(f, args)));
            return _real.Invoke(f, args);
        }
    }

    /// <summary>Exchanges A and B of a compiled Pair through its interpreter form.</summary>
    private sealed class PlatoRecordProbe
    {
        private readonly CSharpBackend _real;
        public PlatoRecordProbe(CSharpBackend real) => _real = real;
        public object Swap(object hostPair)
        {
            var r = (PlatoRecord)_real.ToInterpreterValue(hostPair);
            return new PlatoRecord(r.Type, new[] { r.Fields[1], r.Fields[0] });
        }
    }

    private static (IReadOnlyList<LawResult> Results, DifferentialBackend Backend, LawRunner Runner) Run(bool lie)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var compiled = CSharpBackend.Build(interp);
        var backend = new DifferentialBackend(new InterpreterBackend(interp), lie ? new Lying(compiled) : compiled);
        var runner = new LawRunner(backend, interp, new LawRunOptions { Filters = new[] { "DifferentialFixture" } });
        return (runner.RunAll(), backend, runner);
    }

    [Test]
    public void Engines_that_agree_give_the_ordinary_verdicts_and_no_mismatch()
    {
        var (results, backend, runner) = Run(lie: false);
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        var by = results.ToDictionary(r => r.Law.Name, r => r.Status);
        Assert.That(backend.Mismatches, Is.Empty);
        Assert.That(by["Law_SumIsCommutative"], Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_TwiceIsAddition"], Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_FirstBelowTen"], Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_AlwaysFalse"], Is.EqualTo(LawStatus.Fail), "a law false in both engines is a failure, not a mismatch");
        Assert.That(runner.Witnesses.Where(w => w.Fault != null), Is.Empty);
    }

    [Test]
    public void A_function_answered_differently_makes_its_law_differ_with_both_answers()
    {
        var (results, backend, _) = Run(lie: true);
        TestContext.Out.WriteLine(string.Join("\n", results.Select(r => r.ToString())));
        TestContext.Out.WriteLine(string.Join("\n", backend.Mismatches));
        var twice = results.Single(r => r.Law.Name == "Law_TwiceIsAddition");
        Assert.That(twice.Status, Is.EqualTo(LawStatus.Differs));
        Assert.That(twice.Message, Does.Contain("interpreter: true").And.Contain("csharp: false"));
        Assert.That(twice.Witness, Is.Not.Null, "the case is named");
        Assert.That(LawReport.Verdicts(results).Single(v => v.Name.EndsWith("Law_TwiceIsAddition")).Verdict, Is.EqualTo("differs"));
    }

    [Test]
    public void A_witness_built_differently_is_reported_and_dropped()
    {
        var (results, backend, runner) = Run(lie: true);
        Assert.That(backend.Mismatches, Has.Some.Contains("Witness_Skewed"));
        Assert.That(runner.Witnesses.Single(w => w.Name.EndsWith("Witness_Skewed")).Fault, Does.Contain("interpreter: Pair(3, 4)"));
        // The laws over Pair still run, on the witness both engines built alike.
        Assert.That(results.Single(r => r.Law.Name == "Law_SumIsCommutative").Status, Is.EqualTo(LawStatus.Pass));
    }
}
