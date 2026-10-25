using Ara3D.Geometry.Navigation;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

[TestFixture]
public sealed class ParserErrorLocationTests
{
    [Test]
    public void FailedAlternativeRetainsItsFurthestGrammarPosition()
    {
        var rule = ((Rule)"a\n").Then("x") | ((Rule)"a").Then("z");
        Assert.That(rule.Parse("a\nq"), Is.Null);
        Assert.That(rule.FurthestFailurePosition(new ParserInput("a\nq")), Is.EqualTo(2));
    }

    [Test]
    public void LookaheadAndNestedLookaheadDoNotContributeSpeculativePositions()
    {
        var probe = ((Rule)"a\n").Then("x");
        var input = new ParserInput("a\nq");
        Assert.That(probe.NotAt().Then("z").FurthestFailurePosition(input), Is.Zero);
        Assert.That(probe.At().FurthestFailurePosition(input), Is.Zero);
        Assert.That(probe.NotAt().At().Then("z").FurthestFailurePosition(input), Is.Zero);
        Assert.That(probe.At().NotAt().Then("z").FurthestFailurePosition(input), Is.Zero);
    }

    [Test]
    public void RecoveryScanDoesNotDisplaceTheGrammarFailure()
    {
        var rule = new SequenceRule("a", new OnFail(((Rule)"q").ZeroOrMore()), "x");
        var input = new ParserInput("aqq?");
        var recovered = rule.Parse(input);
        Assert.That(recovered.Position, Is.EqualTo(3));
        Assert.That(recovered.AllErrors().Count(), Is.EqualTo(1));
        Assert.That(rule.FurthestFailurePosition(input), Is.EqualTo(1));
        var parsed = new Parser(input, rule, logger: Logger.Null);
        Assert.That(parsed.Succeeded, Is.False);
        Assert.That(parsed.ParseResult.Position, Is.EqualTo(recovered.Position));
        Assert.That(parsed.ParseResult.AllErrors().Count(), Is.EqualTo(1));
    }

    [Test]
    public void ReusedInputHasIndependentFailurePositions()
    {
        var input = new ParserInput("abc\nq");
        Assert.That(((Rule)"abc\n").Then("x").FurthestFailurePosition(input), Is.EqualTo(4));
        Assert.That(((Rule)"a").Then("z").FurthestFailurePosition(input), Is.EqualTo(1));
    }

    [Test]
    public void ConcurrentParsesOfSharedInputDoNotShareDiagnosticState()
    {
        var input = new ParserInput("abc\nq");
        var deep = ((Rule)"abc\n").Then("x");
        var early = ((Rule)"a").Then("z");
        var positions = new int[32];
        Parallel.For(0, positions.Length, i =>
            positions[i] = (i % 2 == 0 ? deep : early).FurthestFailurePosition(input));
        for (var i = 0; i < positions.Length; ++i)
            Assert.That(positions[i], Is.EqualTo(i % 2 == 0 ? 4 : 1));
    }

    private sealed class RetryThrows : Rule
    {
        private int _calls;

        protected override ParserState MatchImplementation(ParserState state)
            => ++_calls == 1 ? null! : throw new InvalidOperationException("diagnostic retry");

        protected override int GetHashCodeInternal()
            => 0;
    }

    [Test]
    public void DiagnosticRetryFailurePreservesOriginalParseFailure()
    {
        var parsed = new Parser(new ParserInput("x"), new RetryThrows(), logger: Logger.Null);
        Assert.That(parsed.Succeeded, Is.False);
        Assert.That(parsed.ParseResult, Is.Null);
        Assert.That(parsed.Exception, Is.Null);
        Assert.That(parsed.ErrorMessages, Does.Contain("Parsing failed with no info"));
        Assert.That(parsed.ErrorMessages, Does.Contain("Failure location unavailable: diagnostic retry"));
    }

    private sealed class OriginalThrows : Rule
    {
        protected override ParserState MatchImplementation(ParserState state)
            => throw new InvalidOperationException("original parse exception");

        protected override int GetHashCodeInternal()
            => 0;
    }

    [Test]
    public void OriginalParseExceptionIsPreservedWithoutDiagnosticRetry()
    {
        var parsed = new Parser(new ParserInput("x"), new OriginalThrows(), logger: Logger.Null);
        Assert.That(parsed.Succeeded, Is.False);
        Assert.That(parsed.Exception.Message, Is.EqualTo("original parse exception"));
        Assert.That(parsed.ErrorMessages, Has.Count.EqualTo(1));
    }
}
