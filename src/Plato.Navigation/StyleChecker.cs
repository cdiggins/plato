using System.Text.RegularExpressions;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;

namespace Ara3D.Geometry.Navigation;

/// <summary>The text-and-shape rules of the forward stdlib that no compiler pass owns — the
/// authoring conventions from the plato-293 re-partition and the C# writer's hard limits. Runs on
/// a <see cref="ParsedFile"/> (text for the token rules, AST for the shape rules), so it needs no
/// <c>Compilation</c> and is effectively free. Reuses <see cref="LintFinding"/> so downstream
/// consumers shape one finding type.
///
///   STY001 (Error)   - the identifier `New` in non-comment source. `New` is the C# writer's
///                      reserved constructor name; a Plato declaration or call named New collides
///                      with every generated struct's constructor.
///   STY002 (Error)   - a concrete type with more than 10 fields. The C# runtime's TupleN
///                      surface stops at 10; an 11-field type has no functioning generated form.
///   STY003 (Error)   - the token `implicit` in non-comment source. Implicit operators are a
///                      C#-side decision (ara3d-056: reserved for a few argument-position API
///                      edges); the vocabulary must not declare them.
///   STY004 (Warning) - a `//` comment block longer than 12 lines. Doc comments state what a
///                      declaration is and its invariants (STYLE_GUIDE.md); essays belong in
///                      CONVENTIONS.md or a linked doc. The file-header block (first block,
///                      line 1) is exempt: headers legitimately carry the file's charter.
///   STY005 (Error)   - more than one declaration kind in a file, or a kind that contradicts the
///                      file's `.concepts.` / `.types.` / `.library.` suffix. One kind per file
///                      is the re-partition invariant that makes a file's name tell its contents.
///   STY006 (Warning) - more than 12 top-level declarations in a file (the re-partition cap).
/// </summary>
public static class StyleChecker
{
    public const int MaxFields = 10;
    public const int MaxDeclarations = 12;
    public const int MaxDocCommentLines = 12;

    private static readonly Regex NewToken = new(@"\bNew\b", RegexOptions.Compiled);
    private static readonly Regex ImplicitToken = new(@"\bimplicit\b", RegexOptions.Compiled);
    private static readonly Regex StringLiteral = new("\"[^\"\n]*\"", RegexOptions.Compiled);

    public static List<LintFinding> Check(IEnumerable<ParsedFile> files)
        => files.SelectMany(Check).ToList();

    public static List<LintFinding> Check(ParsedFile file)
    {
        var findings = new List<LintFinding>();
        var path = file.File.Path.ToString()!;

        CheckTokens(findings, path, file.File.Text);
        CheckDocCommentLength(findings, path, file.File.Text);

        if (file.Ast != null)
        {
            CheckFieldCounts(findings, path, file.Ast);
            CheckOneKindPerFile(findings, path, file.Ast);
            CheckDeclarationCount(findings, path, file.Ast);
        }

        return findings;
    }

    /// <summary>STY001 + STY003: banned tokens, judged per line with string literals and `//`
    /// comments stripped (the stdlib discusses "implicit surfaces" in prose constantly; only the
    /// code stream is policed).</summary>
    private static void CheckTokens(List<LintFinding> findings, string path, string text)
    {
        var lines = text.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var code = StripCommentAndStrings(lines[i]);
            if (code.Length == 0)
                continue;

            if (NewToken.IsMatch(code))
                findings.Add(new LintFinding(path, i + 1, "STY001", LintSeverity.Error,
                    "the identifier 'New' is reserved by the C# writer (generated constructor name); rename the declaration or call"));

            if (ImplicitToken.IsMatch(code))
                findings.Add(new LintFinding(path, i + 1, "STY003", LintSeverity.Error,
                    "'implicit' is banned in the vocabulary; implicit conversions are a C#-side API-edge decision, not a stdlib one"));
        }
    }

    private static string StripCommentAndStrings(string line)
    {
        var noStrings = StringLiteral.Replace(line, "\"\"");
        var comment = noStrings.IndexOf("//", StringComparison.Ordinal);
        return comment >= 0 ? noStrings[..comment] : noStrings;
    }

    /// <summary>STY004: consecutive whole-line `//` blocks longer than the cap, excluding a block
    /// that starts on line 1 (the file header).</summary>
    private static void CheckDocCommentLength(List<LintFinding> findings, string path, string text)
    {
        var lines = text.Split('\n');
        var blockStart = -1;
        for (var i = 0; i <= lines.Length; i++)
        {
            var isComment = i < lines.Length && lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal);
            if (isComment)
            {
                if (blockStart < 0)
                    blockStart = i;
                continue;
            }
            if (blockStart >= 0)
            {
                var length = i - blockStart;
                if (length > MaxDocCommentLines && blockStart > 0)
                    findings.Add(new LintFinding(path, blockStart + 1, "STY004", LintSeverity.Warning,
                        $"doc comment block is {length} lines (cap {MaxDocCommentLines}); state what the declaration is and its invariants, and link out for the essay"));
                blockStart = -1;
            }
        }
    }

    /// <summary>STY002: the C# TupleN cap.</summary>
    private static void CheckFieldCounts(List<LintFinding> findings, string path, AstFile ast)
    {
        foreach (var decl in ast.Types)
        {
            if (decl.Kind != TypeKind.ConcreteType)
                continue;
            var fields = decl.Members.OfType<AstFieldDeclaration>().Count();
            if (fields > MaxFields)
                findings.Add(new LintFinding(path, Line(decl), "STY002", LintSeverity.Error,
                    $"type '{decl.Name.Text}' has {fields} fields; the generated TupleN surface caps at {MaxFields} — split the type"));
        }
    }

    private static void CheckOneKindPerFile(List<LintFinding> findings, string path, AstFile ast)
    {
        if (ast.Types.Count == 0)
            return;

        var kinds = ast.Types.Select(FileKindOf).Where(k => k != null).Distinct().ToList();
        if (kinds.Count > 1)
            findings.Add(new LintFinding(path, 1, "STY005", LintSeverity.Error,
                $"file mixes declaration kinds ({string.Join(" + ", kinds)}); one kind per file — move declarations to the matching file"));

        var suffix = SuffixKindOf(path);
        if (suffix != null && kinds.Count == 1 && kinds[0] != suffix)
            findings.Add(new LintFinding(path, 1, "STY005", LintSeverity.Error,
                $"file suffix says '{suffix}' but the declarations are '{kinds[0]}'; rename the file or move the declarations"));
    }

    private static void CheckDeclarationCount(List<LintFinding> findings, string path, AstFile ast)
    {
        if (ast.Types.Count > MaxDeclarations)
            findings.Add(new LintFinding(path, 1, "STY006", LintSeverity.Warning,
                $"file holds {ast.Types.Count} top-level declarations (cap {MaxDeclarations}); split it along its natural seams"));
    }

    /// <summary>The re-partition's three file kinds. Primitives ride with types; anything the
    /// grammar may grow later maps to null and is exempt rather than misjudged.</summary>
    private static string? FileKindOf(AstTypeDeclaration decl)
        => decl.Kind switch
        {
            TypeKind.Interface => "concepts",
            TypeKind.Library => "library",
            TypeKind.ConcreteType or TypeKind.Primitive => "types",
            _ => null
        };

    private static string? SuffixKindOf(string path)
    {
        var name = path.Replace('\\', '/');
        name = name[(name.LastIndexOf('/') + 1)..].ToLowerInvariant();
        if (name.EndsWith(".concepts.plato")) return "concepts";
        if (name.EndsWith(".types.plato")) return "types";
        if (name.EndsWith(".library.plato")) return "library";
        return null;
    }

    private static int Line(AstNode node)
    {
        var span = Span.From(node);
        return span.HasValue ? span.BeginLine + 1 : 0;
    }
}
