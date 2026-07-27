using NUnit.Framework;

namespace Ara3D.Geometry.Navigation.Tests;

/// <summary>Round-trip invariants over the whole gated corpus: positions must lead back to the
/// records they came from, in both directions.</summary>
[TestFixture]
public class InvariantTests
{
    [Test]
    public void EveryDefinitionIsFoundAtItsOwnSpan()
    {
        var index = Corpus.Index;
        var misses = new List<string>();

        foreach (var d in index.Defs.Where(d => d.NameSpan.HasValue && d.FileId >= 0))
        {
            var hit = index.FindAt(index.Snapshot.Files[d.FileId].Path, d.NameSpan.Begin);
            if (hit?.Def == null || hit.Def.NameSpan != d.NameSpan)
                misses.Add($"{d.Kind} '{d.Name}' at {d.NameSpan.BeginLine + 1}:{d.NameSpan.BeginColumn + 1} not found at its own span");
        }

        Assert.That(misses, Is.Empty, $"{misses.Count} misses; first 25:\n" + string.Join("\n", misses.Take(25)));
    }

    [Test]
    public void EveryReferenceResolvesBackToItsTargets()
    {
        var index = Corpus.Index;
        var misses = new List<string>();

        foreach (var r in index.Refs.Where(r => r.Span.HasValue && r.FileId >= 0 && r.Targets.Count > 0))
        {
            var hit = index.FindAt(index.Snapshot.Files[r.FileId].Path, r.Span.Begin);
            var found = index.GetDefinitions(hit).Select(d => d.Id).ToHashSet();
            foreach (var t in r.Targets.Where(t => !found.Contains(t)))
                misses.Add($"ref '{r.Name}' at {r.Span.BeginLine + 1}:{r.Span.BeginColumn + 1} lost target {index.Defs[t]}");
        }

        Assert.That(misses, Is.Empty, $"{misses.Count} misses; first 25:\n" + string.Join("\n", misses.Take(25)));
    }

    [Test]
    public void FindReferencesIsTheInverseOfTargets()
    {
        var index = Corpus.Index;
        var expected = new Dictionary<int, int>();
        foreach (var r in index.Refs)
        foreach (var t in r.Targets)
            expected[t] = expected.GetValueOrDefault(t) + 1;

        foreach (var (defId, count) in expected)
            Assert.That(index.FindReferences(defId).Count, Is.EqualTo(count), $"for {index.Defs[defId]}");
    }

    [Test]
    public void EveryRecordBelongsToAKnownFile()
    {
        var n = Corpus.Index.Snapshot.Files.Count;
        Assert.That(Corpus.Index.Defs.All(d => d.FileId >= 0 && d.FileId < n), Is.True);
        Assert.That(Corpus.Index.Refs.All(r => r.FileId >= 0 && r.FileId < n), Is.True);
    }

    /// <summary>References the binder could not tie to any source definition. These are the
    /// compiler's own built-ins, and the list is asserted so a regression cannot quietly add to it.</summary>
    [Test]
    public void UnresolvedReferencesAreOnlyKnownBuiltins()
    {
        var names = Corpus.Index.Refs.Where(r => r.Targets.Count == 0)
            .Select(r => r.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToList();

        TestContext.WriteLine("unresolved: " + string.Join(", ", names));

        var unexpected = names
            .Where(n => !n.StartsWith("$", StringComparison.Ordinal) && n != "Self" && n != "default")
            .ToList();
        Assert.That(unexpected, Is.Empty,
            "only type variables ($T), the Self type and the default keyword may lack a source definition");
    }
}
