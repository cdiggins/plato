// Checks limits on witness enumeration during law runs.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-485: every Witness_* runs under a cap. A witness slower than the cap is dropped as
/// faulted, naming the cap, and a law whose parameter had no other values is an error naming the
/// witness; the run finishes instead of waiting on the witness (one took 20 minutes in a sweep).
/// </summary>
[TestFixture]
public class WitnessCapTests
{
    // Heavy has a function-typed field, so the generator cannot build one: its laws have only the
    // witness. The witness does about 8 billion additions, far past a 1 s cap on either backend.
    private const string Source = """
        type Heavy
        {
            Size: Integer;
            Step: Function1<Integer, Integer>;
        }

        library WitnessCapFixture
        {
            Plus(i: Integer, j: Integer): Integer => i + j;
            Inner(i: Integer, j: Integer): Integer => 2000.Range.Map(k => Plus(i, j + k)).Count;
            Middle(i: Integer): Integer => 2000.Range.Map(j => Inner(i, j)).Count;
            Slow(_: Integer): Integer => 2000.Range.Map(i => Middle(i)).Count;
            Witness_Heavy(_: Heavy): Heavy => new Heavy(Slow(0), x => x + 1);
            Law_HeavyIsSized(h: Heavy): Boolean => h.Size >= 0;
            Law_Unrelated(k: Integer): Boolean => k == k;
        }
        """;

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp)]
    public void A_witness_past_its_cap_is_dropped_and_its_law_is_an_error_naming_it(LawBackendKind kind)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var options = new LawRunOptions { Filters = new[] { "WitnessCapFixture" }, WitnessBudgetSeconds = 1 };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var runner = kind == LawBackendKind.Interpreter
            ? new LawRunner(interp, options)
            : new LawRunner(LawBackends.Create(kind, interp), interp, options);

        var clock = System.Diagnostics.Stopwatch.StartNew();
        var results = runner.RunAll().ToDictionary(r => r.Law.Name, r => r);
        TestContext.Out.WriteLine(string.Join("\n", results.Values.Select(r => r.ToString())));
        TestContext.Out.WriteLine(string.Join("\n", runner.Witnesses.Select(w => $"{w.Name} {w.Elapsed.TotalSeconds:F1}s {w.Fault}")));

        Assert.That(clock.Elapsed.TotalSeconds, Is.LessThan(60), "the run should not wait on the witness");
        var witness = runner.Witnesses.Single(w => w.Name.EndsWith("Witness_Heavy"));
        Assert.That(witness.Fault, Does.Contain("witness cap"));
        Assert.That(results["Law_HeavyIsSized"].Status, Is.EqualTo(LawStatus.Error));
        Assert.That(results["Law_HeavyIsSized"].Message, Does.Contain("Witness_Heavy").And.Contain("witness cap"));
        Assert.That(results["Law_Unrelated"].Status, Is.EqualTo(LawStatus.Pass));
    }
}
