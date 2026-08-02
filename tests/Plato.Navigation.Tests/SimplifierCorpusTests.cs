using Ara3D.Geometry.AST;
using Ara3D.Geometry.Navigation;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

/// <summary>The rewrite is only worth anything if it is meaning-preserving, so the gate is the
/// forward stdlib itself: apply every edit the <see cref="Simplifier"/> proposes across the whole
/// corpus, re-parse and re-resolve, and require that nothing got worse. A rule that ever produces
/// unparseable or unresolvable source fails here rather than in someone's editor.</summary>
[TestFixture]
public static class SimplifierCorpusTests
{
    /// <summary>Collapse guard, not a file count: it replaces the old "the corpus must hold
    /// something simplifiable" assertion, which stopped being true once every edit the rules propose
    /// had been applied by hand. An empty edit set is the healthy state for a tidy library; an empty
    /// CORPUS is the failure this test has to notice. Sits far below the real corpus so ordinary
    /// growth or consolidation never trips it.</summary>
    private const int MinCorpusFiles = 100;

    private static (int ParseFailures, int ResolutionErrors) Compile(SourceSnapshot snapshot)
    {
        var parsed = CheckSupport.Parse(snapshot, new ParseCache());
        var comp = CheckSupport.Compile(parsed);
        return (parsed.Count(f => !f.Parsed), comp.SymbolFactory?.Errors.Count ?? 0);
    }

    [Test]
    public static void SimplifiedCorpusStillCompiles()
    {
        var snapshot = Corpus.Index.Snapshot;
        var before = Compile(snapshot);

        Assert.That(snapshot.Files.Count, Is.GreaterThan(MinCorpusFiles),
            "the corpus collapsed; every assertion below would pass vacuously");

        var edits = Simplifier.Check(CheckSupport.Parse(snapshot, new ParseCache()));

        var byFile = edits.GroupBy(e => e.File).ToDictionary(g => g.Key, g => (IReadOnlyList<SimplifyEdit>)g.ToList());
        var texts = snapshot.Files.ToDictionary(
            f => f.Path,
            f => byFile.TryGetValue(f.Path.ToString()!, out var es) ? Simplifier.Apply(f.Text, es) : f.Text);

        var after = Compile(SourceSnapshot.FromTexts(texts));
        Assert.Multiple(() =>
        {
            Assert.That(after.ParseFailures, Is.EqualTo(before.ParseFailures), "simplification broke parsing");
            Assert.That(after.ResolutionErrors, Is.LessThanOrEqualTo(before.ResolutionErrors), "simplification broke resolution");
        });
    }

    /// <summary>plato-404: SIM001 once stripped the type name from tuples in match arms and
    /// conditional branches, which type-check against their SIBLINGS rather than against the declared
    /// return type — six CHK101 errors in three functions. The corpus is the regression test: no
    /// SIM001 edit anywhere in it may land inside a branch. (SIM002 may and does — a named constant
    /// carries the type wherever it stands.)</summary>
    [Test]
    public static void ConstructorNamesAreOnlyDroppedInResultPosition()
    {
        var parsed = CheckSupport.Parse(Corpus.Index.Snapshot, new ParseCache());
        var branches = parsed.Where(f => f.Ast != null)
            .ToDictionary(f => f.File.Path.ToString()!, f => Branches(f.Ast!).ToList());

        var inside = Simplifier.Check(parsed)
            .Where(e => e.Code == Simplifier.RedundantConstructor)
            .Where(e => branches[e.File].Any(b => e.Begin >= b.Begin && e.End <= b.End))
            .Select(e => $"{e.File}:{e.Line} {e.Before}")
            .ToList();

        Assert.That(inside, Is.Empty, "a tuple in a branch does not unify with the named type its siblings carry");
    }

    /// <summary>Every expression whose type comes from unification with a sibling rather than from
    /// the enclosing signature: a match arm's body and both arms of a conditional.</summary>
    private static IEnumerable<Span> Branches(AstNode node)
        => node.GetAllDescendants().SelectMany(n => n switch
        {
            AstMatchArm arm => new[] { Span.From(arm.Body) },
            AstConditional c => new[] { Span.From(c.IfTrue), Span.From(c.IfFalse) },
            _ => Array.Empty<Span>()
        }).Where(s => s.HasValue);
}
