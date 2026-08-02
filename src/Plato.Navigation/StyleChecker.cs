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
///   STY005 (Error)   - more than one declaration kind in a file, or a kind that contradicts the
///                      file's `.interfaces.` / `.types.` / `.library.` suffix. One kind per file
///                      is the re-partition invariant that makes a file's name tell its contents.
///
/// Deliberately absent: caps on doc-comment length and on declarations per file. Both were
/// authoring preferences a checker cannot judge — length is not the same as verbosity, and a file
/// is too long when it has stopped being one subject, which is a reading, not a count. MaxFields
/// stays because it is not a preference: the stdlib declares Tuple2..Tuple10, and SymbolFactory
/// silently skips the tuple constructor for a wider type.
/// </summary>
public static class StyleChecker
{
    public const int MaxFields = 10;

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

        if (file.Ast != null)
        {
            CheckFieldCounts(findings, path, file.Ast);
            CheckOneKindPerFile(findings, path, file.Ast);
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

    /// <summary>The re-partition's three file kinds. Primitives ride with types; anything the
    /// grammar may grow later maps to null and is exempt rather than misjudged.</summary>
    private static string? FileKindOf(AstTypeDeclaration decl)
        => decl.Kind switch
        {
            TypeKind.Interface => "interfaces",
            TypeKind.Library => "library",
            TypeKind.ConcreteType or TypeKind.Primitive => "types",
            _ => null
        };

    private static string? SuffixKindOf(string path)
    {
        var name = path.Replace('\\', '/');
        name = name[(name.LastIndexOf('/') + 1)..].ToLowerInvariant();
        if (name.EndsWith(".concepts.plato")) return "interfaces";
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
