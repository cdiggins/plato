// Checks assignment sites resolve to the correct local declaration across scopes.
using Ara3D.Geometry.Navigation;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

[TestFixture]
public class AssignmentReferenceTests
{
    [Test]
    public void AssignmentsInNestedLoopsKeepTheirDeclarationAndRespectShadowing()
    {
        const string source = """
            primitive Number { }
            primitive Boolean { }
            library Locals
            {
                Walk(start: Number, repeat: Boolean): Number
                {
                    var x = start;
                    while (repeat)
                    {
                        x = start;
                        while (repeat)
                        {
                            x = start;
                        }
                        var x = start;
                        x = start;
                    }
                    x = start;
                    return x;
                }
            }
            """;
        var path = new FilePath("assignments.plato");
        var index = NavigationIndex.Build(SourceSnapshot.FromTexts(new Dictionary<FilePath, string>
        {
            [path] = source
        }));
        Assert.That(index.Diagnostics, Is.Empty);
        var declarations = index.Defs.Where(d => d.Kind == DefKind.Variable && d.Name == "x")
            .OrderBy(d => d.NameSpan.Begin).ToList();
        Assert.That(declarations, Has.Count.EqualTo(2));

        var assignments = System.Text.RegularExpressions.Regex.Matches(source, @"(?m)^\s*(x) = start;")
            .Select(m => m.Groups[1].Index).ToList();
        Assert.That(assignments, Has.Count.EqualTo(4));
        var expected = new[] { declarations[0].Id, declarations[0].Id, declarations[1].Id, declarations[0].Id };
        for (var i = 0; i < assignments.Count; i++)
        {
            var hit = index.FindAt(path, assignments[i]);
            Assert.That(hit?.Ref, Is.Not.Null, $"assignment at offset {assignments[i]}");
            Assert.That(index.GetDefinitions(hit).Select(d => d.Id), Is.EqualTo(new[] { expected[i] }));
            Assert.That(hit!.Ref!.Span.Length, Is.EqualTo(1), "the reference covers the identifier alone");
            Assert.That(index.FindReferences(expected[i]).Select(r => r.Span.Begin), Does.Contain(assignments[i]));
        }
    }
}
