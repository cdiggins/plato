using Ara3D.Utils;
using NUnit.Framework;

namespace Ara3D.Geometry.Navigation.Tests;

/// <summary>The v2 gate: an incremental update must be indistinguishable from a full rebuild.
/// Structural identity is checked through <see cref="NavigationJson"/>, which carries the sources,
/// every definition and reference with its spans and targets, the per-file states, the diagnostics
/// and the generation — so byte-identical JSON means byte-identical index.</summary>
[TestFixture]
public class IncrementalIndexerTests
{
    [Test]
    public void UpdateMatchesFullBuildAcrossEveryKindOfChange()
    {
        var indexer = new IncrementalIndexer();
        var texts = CorpusTexts();

        AssertMatchesFullBuild(indexer, Snapshot(texts));                       // (a) first build
        AssertMatchesFullBuild(indexer, Snapshot(texts));                       // (b) unchanged

        var edited = new Dictionary<FilePath, string>(texts);
        var victim = edited.Keys.OrderBy(SourceSnapshot.PathKey, StringComparer.Ordinal).First();
        edited[victim] = "// edited\n" + edited[victim];
        AssertMatchesFullBuild(indexer, Snapshot(edited));                      // (c) one file edited

        var added = new Dictionary<FilePath, string>(edited) { [NewFile] = NewFileText };
        AssertMatchesFullBuild(indexer, Snapshot(added));                       // (d) file added

        var removed = new Dictionary<FilePath, string>(added);
        removed.Remove(victim);
        AssertMatchesFullBuild(indexer, Snapshot(removed));                     // (e) file removed
    }

    [Test]
    public void EditingOneFileReparsesExactlyOneFile()
    {
        var indexer = new IncrementalIndexer();
        var texts = CorpusTexts();

        indexer.Update(Snapshot(texts));
        Assert.That(indexer.LastUpdate.FilesParsed, Is.EqualTo(texts.Count), "cold build parses everything");
        Assert.That(indexer.LastUpdate.FilesReused, Is.Zero);

        indexer.Update(Snapshot(texts));
        Assert.That(indexer.LastUpdate.FilesParsed, Is.Zero, "an unchanged snapshot parses nothing");
        Assert.That(indexer.LastUpdate.FilesReused, Is.EqualTo(texts.Count));

        var edited = new Dictionary<FilePath, string>(texts);
        var victim = edited.Keys.OrderBy(SourceSnapshot.PathKey, StringComparer.Ordinal).First();
        edited[victim] += "\n// touched\n";

        indexer.Update(Snapshot(edited));
        Assert.That(indexer.LastUpdate.FilesParsed, Is.EqualTo(1));
        Assert.That(indexer.LastUpdate.FilesReused, Is.EqualTo(texts.Count - 1));
    }

    /// <summary>Constraint: the cache key is (path, content hash). An AST carries the path it was
    /// parsed from in every range, so reusing it for a different file with the same text would
    /// attribute every span to the wrong file — silently, and everywhere.</summary>
    [Test]
    public void SameContentAtADifferentPathIsNotReused()
    {
        var indexer = new IncrementalIndexer();
        var first = new FilePath(Path.Combine(Corpus.RepoRoot.Value, "scratch", "cache_a.plato"));
        var second = new FilePath(Path.Combine(Corpus.RepoRoot.Value, "scratch", "cache_b.plato"));

        indexer.Update(Snapshot(new Dictionary<FilePath, string> { [first] = NewFileText }));
        Assert.That(indexer.LastUpdate.FilesParsed, Is.EqualTo(1));

        var index = indexer.Update(Snapshot(new Dictionary<FilePath, string> { [second] = NewFileText }));
        Assert.That(indexer.LastUpdate.FilesParsed, Is.EqualTo(1), "identical text at a new path must be reparsed");

        Assert.That(index.Files[0].Path, Does.EndWith("cache_b.plato"));
        Assert.That(index.Defs.Select(d => d.FileId), Is.All.EqualTo(0));
        Assert.That(index.Refs.Select(r => r.FileId), Is.All.EqualTo(0));
        Assert.That(index.Outline(second), Is.Not.Empty);
        Assert.That(index.Outline(first), Is.Empty);
    }

    [Test]
    public void BothPathsCoexistWithTheirOwnSpans()
    {
        var indexer = new IncrementalIndexer();
        var a = new FilePath(Path.Combine(Corpus.RepoRoot.Value, "scratch", "cache_a.plato"));
        var b = new FilePath(Path.Combine(Corpus.RepoRoot.Value, "scratch", "cache_b.plato"));

        var snapshot = Snapshot(new Dictionary<FilePath, string>
        {
            [a] = NewFileText,
            [b] = NewFileText.Replace("CacheProbeA", "CacheProbeB")
        });

        AssertMatchesFullBuild(indexer, snapshot);
        var index = indexer.Current;
        Assert.That(index.Search("CacheProbeA", SearchKind.Exact).Single().FileId,
            Is.EqualTo(index.Snapshot.Find(a)!.Id));
        Assert.That(index.Search("CacheProbeB", SearchKind.Exact).Single().FileId,
            Is.EqualTo(index.Snapshot.Find(b)!.Id));
    }

    [Test]
    public void RetainDropsSupersededVersions()
    {
        var indexer = new IncrementalIndexer();
        var file = new FilePath(Path.Combine(Corpus.RepoRoot.Value, "scratch", "cache_a.plato"));

        for (var i = 0; i < 5; i++)
            indexer.Update(Snapshot(new Dictionary<FilePath, string>
                { [file] = NewFileText + $"\n// version {i}\n" }));

        Assert.That(indexer.Cache.Count, Is.EqualTo(1));
    }

    private static void AssertMatchesFullBuild(IncrementalIndexer indexer, SourceSnapshot snapshot)
    {
        var expected = NavigationJson.Write(NavigationIndex.Build(snapshot));
        var actual = NavigationJson.Write(indexer.Update(snapshot));
        Assert.That(actual, Is.EqualTo(expected));
    }

    private static SourceSnapshot Snapshot(IReadOnlyDictionary<FilePath, string> texts)
        => SourceSnapshot.FromTexts(texts);

    private static Dictionary<FilePath, string> CorpusTexts()
        => Corpus.Index.Snapshot.Files.ToDictionary(f => f.Path, f => f.Text);

    /// <summary>Paths that exist only inside a snapshot — nothing in this fixture touches the disk
    /// (<see cref="SourceSnapshot.FromTexts"/> takes the text directly), so they name a folder that
    /// need not exist rather than borrowing a real library folder's name.</summary>
    private static FilePath NewFile { get; } =
        new(Path.Combine(Corpus.RepoRoot.Value, "scratch", "cache_probe.plato"));

    private const string NewFileText = """
        library CacheProbeA
        {
            Twice(x: Number): Number => x + x;
        }
        """;
}
