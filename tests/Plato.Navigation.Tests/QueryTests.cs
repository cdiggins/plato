using Ara3D.Geometry.Navigation;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

[TestFixture]
public class QueryTests
{
    /// <summary>The file that declares the <c>Number</c> primitive, found through the index rather
    /// than named: the forward stdlib is being re-partitioned and renamed, so a hard-coded file
    /// name is a test that breaks for reasons that have nothing to do with navigation.</summary>
    private static FilePath Primitives
    {
        get
        {
            var number = Corpus.Index.Search("Number", SearchKind.Exact)
                .First(d => d.Kind == DefKind.Type);
            return Corpus.Index.Snapshot.Files[number.FileId].Path;
        }
    }

    [Test]
    public void ExactSearchReturnsOnlyThatName()
    {
        var results = Corpus.Index.Search("Number", SearchKind.Exact);
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(d => d.Name == "Number"), Is.True);
        Assert.That(results.Any(d => d.Kind == DefKind.Type), Is.True, "the Number type itself must be found");
    }

    [Test]
    public void PrefixSearchExcludesTheExactMatch()
    {
        var prefix = Corpus.Index.Search("Vector", SearchKind.Prefix);
        Assert.That(prefix, Is.Not.Empty);
        Assert.That(prefix.All(d => d.Name.StartsWith("Vector", StringComparison.Ordinal)), Is.True);

        var all = Corpus.Index.Search("Vector", SearchKind.All);
        Assert.That(all.Count, Is.GreaterThanOrEqualTo(prefix.Count));
    }

    [Test]
    public void SearchForAnAbsentNameIsEmpty()
        => Assert.That(Corpus.Index.Search("NoSuchSymbolAnywhere"), Is.Empty);

    [Test]
    public void OutlineIsInSourceOrderAndCoversTheFile()
    {
        var outline = Corpus.Index.Outline(Primitives);
        Assert.That(outline, Is.Not.Empty);
        Assert.That(outline.Select(d => d.NameSpan.Begin), Is.Ordered);
        Assert.That(outline.Any(d => d.Kind == DefKind.Type && d.Name == "Number"), Is.True);
    }

    [Test]
    public void OutlineOfAnUnknownFileIsEmpty()
        => Assert.That(Corpus.Index.Outline(new FilePath("nowhere.plato")), Is.Empty);

    [Test]
    public void GoToDefinitionFromATypeSiteLandsOnTheDeclaration()
    {
        var index = Corpus.Index;
        var declaration = index.Search("Number", SearchKind.Exact).First(d => d.Kind == DefKind.Type);

        var site = index.Refs.First(r => r.Kind == RefKind.Type && r.Name == "Number"
                                         && r.Targets.Contains(declaration.Id));
        var hit = index.FindAt(index.Snapshot.Files[site.FileId].Path, site.Span.Begin);

        Assert.That(index.GetDefinitions(hit).Select(d => d.Id), Does.Contain(declaration.Id));
    }

    [Test]
    public void FindReferencesOnAHighFanInTypeCoversSignaturesAndBodies()
    {
        var index = Corpus.Index;
        var number = index.Search("Number", SearchKind.Exact).First(d => d.Kind == DefKind.Type);
        var refs = index.FindReferences(number.Id);

        Assert.That(refs.Count, Is.GreaterThan(500), "Number is used throughout the stdlib");
        Assert.That(refs.Any(r => r.Kind == RefKind.Type), Is.True, "signature sites (the D5 hook)");
        Assert.That(refs.Select(r => r.FileId).Distinct().Count(), Is.GreaterThan(10));
    }

    [Test]
    public void AFunctionCallOffersEveryOverload()
    {
        var index = Corpus.Index;

        // plato-364: a sum case is reachable by its bare name, so a case that collides with a
        // function name pulls its owning TYPE into the group ('-' offers BlendMode alongside the
        // 15 real Subtract methods). Restricting to Method targets keeps this assertion about
        // what it is for — overload groups — until that lands.
        var multiTarget = index.Refs.FirstOrDefault(r =>
            r.Targets.Count > 1 && r.Targets.All(t => index.Defs[t].Kind == DefKind.Method));

        Assert.That(multiTarget, Is.Not.Null, "the corpus has overloaded functions");
        Assert.That(multiTarget!.Targets.Select(t => index.Defs[t].Name).Distinct().Count(), Is.EqualTo(1),
            "every overload in a group shares the group's name");
    }

    [Test]
    public void PositionQueriesAgreeByOffsetAndByLineColumn()
    {
        var index = Corpus.Index;
        var d = index.Outline(Primitives).First(x => x.Kind == DefKind.Type);
        var path = index.Snapshot.Files[d.FileId].Path;

        var byOffset = index.FindAt(path, d.NameSpan.Begin);
        var byLineColumn = index.FindAt(path, d.NameSpan.BeginLine, d.NameSpan.BeginColumn);

        Assert.That(byLineColumn?.Def?.Id, Is.EqualTo(byOffset?.Def?.Id));
    }

    [Test]
    public void NothingIsFoundInWhitespace()
    {
        var file = Corpus.Index.Snapshot.Files.First(f => f.Text.Contains("\n\n") || f.Text.Contains("\n\r\n"));
        var blank = file.Text.IndexOf("\n\n", StringComparison.Ordinal);
        if (blank < 0)
            blank = file.Text.IndexOf("\n\r\n", StringComparison.Ordinal) + 1;

        Assert.That(Corpus.Index.FindAt(file.Path, blank), Is.Null);
    }
}
