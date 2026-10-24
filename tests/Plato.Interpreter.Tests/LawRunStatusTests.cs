using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;

namespace Plato.Interpreter.Tests;

[TestFixture]
public class LawRunStatusTests
{
    [TestCase(LawStatus.Pass, false)]
    [TestCase(LawStatus.Fail, true)]
    [TestCase(LawStatus.Error, true)]
    [TestCase(LawStatus.Vacuous, true)]
    [TestCase(LawStatus.Differs, true)]
    [TestCase(LawStatus.Obligation, false)]
    [TestCase(LawStatus.Skipped, false)]
    public void Verdict_failure_does_not_require_a_backend_mismatch(LawStatus status, bool fails)
        => Assert.That(LawRunStatus.HasFailures(new[] { new LawResult { Status = status } }), Is.EqualTo(fails));

    [Test]
    public void A_budget_error_fails_even_when_engines_agree()
        => Assert.That(LawRunStatus.HasFailures(new[]
        {
            new LawResult { Status = LawStatus.Error, Message = "law exceeded its time budget" }
        }), Is.True);

    [Test]
    public void A_dropped_witness_mismatch_fails_even_without_a_failing_law()
        => Assert.That(LawRunStatus.HasFailures(new[] { new LawResult { Status = LawStatus.Pass } }, 1), Is.True);

    [Test]
    public void An_empty_run_without_mismatches_keeps_its_existing_success_status()
        => Assert.That(LawRunStatus.HasFailures(Array.Empty<LawResult>()), Is.False);
}
