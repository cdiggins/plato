using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>Verdict files and the baseline check: one sorted line per law, a round trip through
/// the text, overloads told apart, and which changes count as a regression.</summary>
[TestFixture]
public class LawRunnerReportTests
{
    private const string Source = """
        library LawReportFixture
        {
            Law_Holds(b: Boolean): Boolean => true;
            Law_FailsOnFalse(b: Boolean): Boolean => b;
            Law_Overloaded(b: Boolean): Boolean => true;
            Law_Overloaded(b: Boolean, c: Boolean): Boolean => b || c;
        }
        """;

    private static readonly Lazy<IReadOnlyList<LawVerdict>> _verdicts = new(() =>
    {
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(LawRunnerSelectionTests.CompileSource(Source), NumberProfile.Cpu64);
        return LawReport.Verdicts(new LawRunner(interp, new LawRunOptions { Filters = new[] { "LawReportFixture" } }).RunAll());
    });

    private static IReadOnlyList<LawVerdict> Verdicts => _verdicts.Value;

    private static LawVerdict V(string name, string verdict) => new(name, verdict, "");

    [Test]
    public void One_line_per_law_sorted_by_name()
        => Assert.That(Verdicts.Select(v => $"{v.Name} {v.Verdict}"), Is.EqualTo(new[]
        {
            "LawReportFixture.Law_FailsOnFalse fail",
            "LawReportFixture.Law_Holds pass",
            "LawReportFixture.Law_Overloaded(Boolean) pass",
            "LawReportFixture.Law_Overloaded(Boolean, Boolean) fail",
        }));

    [Test]
    public void A_failing_law_names_its_first_failing_witness()
    {
        Assert.That(Verdicts.Single(v => v.Name.EndsWith("Law_FailsOnFalse")).Detail, Is.EqualTo("b = false"));
        Assert.That(Verdicts.Single(v => v.Name.EndsWith("Law_Holds")).Detail, Is.Empty);
    }

    [Test]
    public void Format_and_parse_round_trip()
    {
        var text = LawReport.Format(Verdicts);
        Assert.That(text.Split('\n', StringSplitOptions.RemoveEmptyEntries), Has.Length.EqualTo(Verdicts.Count));
        Assert.That(text, Does.Contain("LawReportFixture.Law_FailsOnFalse\tfail\tb = false\n"));
        Assert.That(LawReport.Parse(text.Replace("\n", "\r\n") + "# comment\n\n"), Is.EqualTo(Verdicts));
    }

    [Test]
    public void Detail_is_kept_on_one_line()
    {
        var v = new LawVerdict("A.Law_X", "error", "line one\tline two\r\nline three");
        Assert.That(LawReport.Parse(LawReport.Format(new[] { v }))[0].Detail, Does.Not.Contain("\n"));
    }

    [Test]
    public void Same_verdicts_have_no_changes()
        => Assert.That(LawReport.Compare(Verdicts, Verdicts, wholeRun: true), Is.Empty);

    [Test]
    public void Pass_to_fail_is_a_regression_and_fail_to_pass_is_not()
    {
        var changes = LawReport.Compare(
            new[] { V("A.Law_1", "pass"), V("A.Law_2", "fail"), V("A.Law_3", "pass") },
            new[] { V("A.Law_1", "fail"), V("A.Law_2", "pass"), V("A.Law_3", "pass") }, wholeRun: true);
        Assert.That(changes.Select(c => (c.Name, c.Regression)), Is.EqualTo(new[] { ("A.Law_1", true), ("A.Law_2", false) }));
        Assert.That(changes[0].ToString(), Does.StartWith("regressed").And.EndWith("pass -> fail"));
    }

    [Test]
    public void Pass_to_error_skipped_or_obligation_is_a_regression()
        => Assert.That(LawReport.Compare(
                new[] { V("A.Law_1", "pass"), V("A.Law_2", "pass"), V("A.Law_3", "pass") },
                new[] { V("A.Law_1", "error"), V("A.Law_2", "skipped"), V("A.Law_3", "obligation") }, wholeRun: true)
            .All(c => c.Regression), Is.True);

    [Test]
    public void A_new_law_is_listed_and_not_a_regression()
    {
        var changes = LawReport.Compare(new LawVerdict[0], new[] { V("A.Law_New", "fail") }, wholeRun: true);
        Assert.That(changes.Single().ToString(), Does.StartWith("new"));
        Assert.That(changes.Single().Regression, Is.False);
    }

    [Test]
    public void A_missing_law_counts_only_in_a_whole_run()
    {
        var baseline = new[] { V("A.Law_Gone", "pass"), V("A.Law_Kept", "pass") };
        var current = new[] { V("A.Law_Kept", "pass") };
        Assert.That(LawReport.Compare(baseline, current, wholeRun: false), Is.Empty);
        var whole = LawReport.Compare(baseline, current, wholeRun: true).Single();
        Assert.That(whole.ToString(), Does.StartWith("removed"));
        Assert.That(whole.Regression, Is.True);
    }
}
