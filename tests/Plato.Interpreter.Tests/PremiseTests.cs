// Checks how law premises select valid generated cases.
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

/// <summary>
/// TKT-437: a law's <c>Premise_&lt;law&gt;</c> companion is evaluated on every case. A guarded law
/// whose premise held on no case is reported vacuous, a failing verdict; one whose premise held on
/// some cases passes and says on how many. Both backends agree.
/// </summary>
[TestFixture]
public class PremiseTests
{
    private const string Source = """
        library PremiseFixture
        {
            // Integers range over -1 to 12 when exhausted: no case exceeds 100.
            Premise_NeverTested(k: Integer): Boolean => k > 100;
            Law_NeverTested(k: Integer): Boolean => k > 100 ? k > 200 : true;

            Premise_SometimesTested(k: Integer): Boolean => k > 5;
            Law_SometimesTested(k: Integer): Boolean => k > 5 ? k > 4 : true;

            // A premise with other parameters is not this law's companion.
            Premise_NoCompanion(b: Boolean): Boolean => false;
            Law_NoCompanion(k: Integer): Boolean => true;

            Premise_FailsFirst(k: Integer): Boolean => false;
            Law_FailsFirst(k: Integer): Boolean => k < 3;
        }
        """;

    private static Dictionary<string, LawResult> Run(LawBackendKind kind)
    {
        var compilation = LawRunnerSelectionTests.CompileSource(Source);
        var options = new LawRunOptions { Filters = new[] { "PremiseFixture" } };
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(compilation, NumberProfile.Cpu64);
        var runner = kind == LawBackendKind.Interpreter
            ? new LawRunner(interp, options)
            : new LawRunner(LawBackends.Create(kind, interp), interp, options);
        return runner.RunAll().ToDictionary(r => r.Law.Name, r => r);
    }

    [TestCase(LawBackendKind.Interpreter)]
    [TestCase(LawBackendKind.CSharp, Category = "Slow")]
    public void A_premise_that_never_holds_makes_the_law_vacuous(LawBackendKind kind)
    {
        var by = Run(kind);
        TestContext.Out.WriteLine(string.Join("\n", by.Values.Select(r => r.ToString())));

        Assert.That(by["Law_NeverTested"].Status, Is.EqualTo(LawStatus.Vacuous));
        Assert.That(by["Law_NeverTested"].PremiseHeld, Is.EqualTo(0));
        Assert.That(by["Law_NeverTested"].ToString(), Does.StartWith("vacuous"));

        Assert.That(by["Law_SometimesTested"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_SometimesTested"].PremiseHeld, Is.EqualTo(7), "k = 6 to 12");
        Assert.That(by["Law_SometimesTested"].ToString(), Does.Contain("premise held on 7"));

        Assert.That(by["Law_NoCompanion"].Status, Is.EqualTo(LawStatus.Pass));
        Assert.That(by["Law_NoCompanion"].PremiseHeld, Is.Null);

        Assert.That(by["Law_FailsFirst"].Status, Is.EqualTo(LawStatus.Fail), "a failing case is a failure whatever the premise");
    }

    [Test]
    public void A_vacuous_law_is_written_as_vacuous_in_a_verdict_file()
    {
        var verdicts = LawReport.Verdicts(Run(LawBackendKind.Interpreter).Values);
        var line = verdicts.Single(v => v.Name.EndsWith("Law_NeverTested"));
        Assert.That(line.Verdict, Is.EqualTo("vacuous"));
        Assert.That(line.Detail, Does.Contain("Premise_NeverTested held on none of 14"));
    }
}
