using System.Diagnostics;
using Ara3D.Utils;
using NUnit.Framework;

namespace Ara3D.Geometry.Navigation.Tests;

/// <summary>D3: a broken file must cost only itself. An IDE indexes source mid-edit, so "one file
/// is garbage" is the normal case, not an error case.</summary>
[TestFixture]
public class DegradedModeTests
{
    private const string Good = """
        library Shapes
        {
            Area(w: Number, h: Number): Number => w * h;
        }
        """;

    private const string Broken = "library Oops { this is not plato at all ((( ";

    private const string Unresolvable = """
        library Mystery
        {
            Convert(x: NoSuchTypeAnywhere): Number => 0.0;
        }
        """;

    private static NavigationIndex IndexOf(params (string name, string text)[] files)
        => NavigationIndex.Build(SourceSnapshot.FromTexts(
            files.ToDictionary(f => new FilePath(f.name), f => f.text)));

    [Test]
    public void AParseFailureLeavesEveryOtherFileNavigable()
    {
        var index = IndexOf(("good.plato", Good), ("broken.plato", Broken));

        Assert.That(index.StatusOf(new FilePath("broken.plato"))!.State, Is.EqualTo(FileState.ParseFailed));
        Assert.That(index.Outline(new FilePath("broken.plato")), Is.Empty);
        Assert.That(index.Diagnostics.Any(d => d.Kind == DiagnosticKind.Parse), Is.True);

        var outline = index.Outline(new FilePath("good.plato"));
        Assert.That(outline.Any(d => d.Kind == DefKind.Library && d.Name == "Shapes"), Is.True);
        Assert.That(outline.Any(d => d.Kind == DefKind.Method && d.Name == "Area"), Is.True);
    }

    [Test]
    public void AnUnresolvableNameIsADiagnosticNotAFailure()
    {
        var index = IndexOf(("mystery.plato", Unresolvable));

        Assert.That(index.Diagnostics.Any(d => d.Kind == DiagnosticKind.Resolution), Is.True);
        Assert.That(index.Outline(new FilePath("mystery.plato")).Any(d => d.Name == "Convert"), Is.True);
        Assert.That(index.Diagnostics.Any(d => d.Kind == DiagnosticKind.BindAbort), Is.False);
    }

    [Test]
    public void OutlineAndSearchWorkWithoutAnySuccessfulBind()
    {
        var index = IndexOf(("broken.plato", Broken));

        Assert.That(index.Files[0].State, Is.EqualTo(FileState.ParseFailed));
        Assert.That(index.Search("Anything"), Is.Empty);
        Assert.That(index.Refs, Is.Empty);
    }

    [Test]
    public void TheGenerationStampTracksContent()
    {
        var a = SourceSnapshot.FromTexts(new Dictionary<FilePath, string> { [new FilePath("a.plato")] = Good });
        var b = SourceSnapshot.FromTexts(new Dictionary<FilePath, string> { [new FilePath("a.plato")] = Good });
        var c = SourceSnapshot.FromTexts(new Dictionary<FilePath, string> { [new FilePath("a.plato")] = Good + "\n" });

        Assert.That(b.Generation, Is.EqualTo(a.Generation));
        Assert.That(c.Generation, Is.Not.EqualTo(a.Generation));
    }

    [Test]
    public void FullCorpusBuildStaysInsideItsBudget()
    {
        var snapshot = Corpus.Index.Snapshot;   // force the shared corpus to be warm first
        var timer = Stopwatch.StartNew();
        var index = NavigationIndex.Build(snapshot);
        timer.Stop();

        TestContext.WriteLine($"warm rebuild of {snapshot.Files.Count} files: {timer.ElapsedMilliseconds} ms, " +
                              $"{index.Defs.Count} defs, {index.Refs.Count} refs");
        Assert.That(timer.ElapsedMilliseconds, Is.LessThan(2000), "rebuild-the-world is the v2 incremental strategy");
    }
}
