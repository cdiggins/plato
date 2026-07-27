using Ara3D.Geometry.AST;
using NUnit.Framework;

namespace Ara3D.Geometry.Navigation.Tests;

/// <summary>The "proven across the board" gate: every identifier the parser produced for the whole
/// gated corpus must be accounted for by the index — as a definition site, as a reference site, or
/// as a name the binder itself reported it could not resolve. Nothing may be silently missing.</summary>
[TestFixture]
public class IdentifierSweepTests
{
    [Test]
    public void EveryIdentifierIsClassified()
    {
        var index = Corpus.Index;
        var defBegins = Lookup(index.Defs.Select(d => (d.FileId, d.NameSpan.Begin)));
        var refBegins = Lookup(index.Refs.Select(r => (r.FileId, r.Span.Begin)));
        var errorBegins = Lookup(index.Diagnostics
            .Where(d => d.Kind == DiagnosticKind.Resolution && d.Span.HasValue)
            .Select(d => (d.FileId, d.Span.Begin)));

        var total = 0;
        var unexplained = new List<string>();

        foreach (var file in Corpus.Bound.Files.Where(f => f.Parsed))
        foreach (var id in Identifiers(file.Ast!))
        {
            var span = Span.From(id);
            if (!span.HasValue || id.Text == "var")
                continue;

            total++;
            // A type-variable reference starts one character earlier than the parser's identifier
            // range, because the index pulls the "$" sigil into the span.
            var key = (file.File.Id, span.Begin);
            var sigil = (file.File.Id, span.Begin - 1);
            if (defBegins.Contains(key) || refBegins.Contains(key) || errorBegins.Contains(key)
                || refBegins.Contains(sigil))
                continue;

            unexplained.Add($"{file.File.Path}:{span.BeginLine + 1}:{span.BeginColumn + 1} '{id.Text}'");
        }

        TestContext.WriteLine($"identifiers swept: {total}");
        Assert.That(unexplained, Is.Empty,
            $"{unexplained.Count} of {total} identifiers unexplained; first 25:\n" +
            string.Join("\n", unexplained.Take(25)));
        Assert.That(total, Is.GreaterThan(5000), "the sweep must actually cover the corpus");
    }

    [Test]
    public void EveryDefAndRefSpanMatchesItsName()
    {
        var mismatches = new List<string>();

        foreach (var d in Corpus.Index.Defs.Where(d => d.NameSpan.HasValue))
            Check(d.FileId, d.NameSpan, d.Name, $"def #{d.Id}", mismatches);

        // Operator references span the operator token, not a name — see ReferenceExtractor.Classify.
        foreach (var r in Corpus.Index.Refs.Where(r => r.Span.HasValue && r.Kind != RefKind.Operator))
            Check(r.FileId, r.Span, r.Name, $"ref #{r.Id}", mismatches);

        Assert.That(mismatches, Is.Empty, string.Join("\n", mismatches.Take(25)));
    }

    private static void Check(int fileId, Span span, string name, string what, List<string> mismatches)
    {
        if (fileId < 0)
            return;
        var text = Corpus.TextOf(fileId);
        if (span.End > text.Length)
        {
            mismatches.Add($"{what}: span past end of file");
            return;
        }
        var actual = text.Substring(span.Begin, span.Length).Trim();
        if (actual != name)
            mismatches.Add($"{what}: span reads '{actual}' but record says '{name}'");
    }

    private static HashSet<(int, int)> Lookup(IEnumerable<(int, int)> items)
        => new(items);

    /// <summary>Every identifier leaf reachable from a file's AST. A few nodes hold an identifier
    /// that is not in their Children (a var definition's name, an assignment's target), so those
    /// are visited explicitly.</summary>
    private static IEnumerable<AstIdentifier> Identifiers(AstNode root)
    {
        var stack = new Stack<AstNode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node is AstIdentifier id)
                yield return id;

            switch (node)
            {
                case AstVarDef v:
                    yield return v.Name;
                    break;
                case AstAssign a:
                    yield return a.Var;
                    break;
            }

            foreach (var child in node.Children)
                if (child != null)
                    stack.Push(child);
        }
    }
}
