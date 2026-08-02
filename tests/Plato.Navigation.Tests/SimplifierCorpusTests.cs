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

        var edits = Simplifier.Check(CheckSupport.Parse(snapshot, new ParseCache()));
        Assert.That(edits, Is.Not.Empty, "the forward stdlib is expected to hold simplifiable declarations");

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
}
