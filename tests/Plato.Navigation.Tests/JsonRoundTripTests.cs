using Ara3D.Geometry.Navigation;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

[TestFixture]
public class JsonRoundTripTests
{
    private static NavigationIndex Reloaded => _reloaded.Value;

    private static readonly Lazy<NavigationIndex> _reloaded =
        new(() => NavigationJson.Read(NavigationJson.Write(Corpus.Index)));

    [Test]
    public void TablesSurviveTheRoundTrip()
    {
        var a = Corpus.Index;
        var b = Reloaded;

        Assert.That(b.Generation, Is.EqualTo(a.Generation));
        Assert.That(b.Defs.Count, Is.EqualTo(a.Defs.Count));
        Assert.That(b.Refs.Count, Is.EqualTo(a.Refs.Count));
        Assert.That(b.Files.Count, Is.EqualTo(a.Files.Count));
        Assert.That(b.Diagnostics.Count, Is.EqualTo(a.Diagnostics.Count));

        for (var i = 0; i < a.Defs.Count; i++)
        {
            Assert.That(b.Defs[i].Kind, Is.EqualTo(a.Defs[i].Kind));
            Assert.That(b.Defs[i].Name, Is.EqualTo(a.Defs[i].Name));
            Assert.That(b.Defs[i].NameSpan, Is.EqualTo(a.Defs[i].NameSpan));
            Assert.That(b.Defs[i].DeclSpan, Is.EqualTo(a.Defs[i].DeclSpan));
            Assert.That(b.Defs[i].Owner, Is.EqualTo(a.Defs[i].Owner));
        }

        for (var i = 0; i < a.Refs.Count; i++)
        {
            Assert.That(b.Refs[i].Kind, Is.EqualTo(a.Refs[i].Kind));
            Assert.That(b.Refs[i].Name, Is.EqualTo(a.Refs[i].Name));
            Assert.That(b.Refs[i].Span, Is.EqualTo(a.Refs[i].Span));
            Assert.That(b.Refs[i].Targets, Is.EqualTo(a.Refs[i].Targets));
        }
    }

    [Test]
    public void QueriesAgreeAcrossTheRoundTrip()
    {
        var a = Corpus.Index;
        var b = Reloaded;

        foreach (var d in a.Defs.Where(d => d.NameSpan.HasValue))
        {
            var path = a.Snapshot.Files[d.FileId].Path;
            var hitA = a.FindAt(path, d.NameSpan.Begin);
            var hitB = b.FindAt(path, d.NameSpan.Begin);
            Assert.That(hitB?.Def?.Id, Is.EqualTo(hitA?.Def?.Id));
            Assert.That(b.FindReferences(d.Id).Count, Is.EqualTo(a.FindReferences(d.Id).Count));
        }

        foreach (var name in new[] { "Number", "Array", "Vector", "Point" })
            Assert.That(b.Search(name).Select(d => d.Id), Is.EqualTo(a.Search(name).Select(d => d.Id)));
    }

    [Test]
    public void ExportIsStable()
        => Assert.That(NavigationJson.Write(Reloaded), Is.EqualTo(NavigationJson.Write(Corpus.Index)));
}
