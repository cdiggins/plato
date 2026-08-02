using System.Text;
using Ara3D.Geometry.AST;

namespace Ara3D.Geometry.Navigation;

/// <summary>Rewrites that make a declaration say less without saying anything different. Text-level
/// and reversible: every edit is a span plus its replacement, so a caller can preview, filter, or
/// apply them. Runs on <see cref="ParsedFile"/> (AST for the shape, text for the slice), so like
/// <see cref="StyleChecker"/> it needs no <c>Compilation</c>.
///
///   SIM001 - a constructor call naming the type it is already known to produce. Fires ONLY in
///            RESULT position — the whole body of an expression-bodied function, or the operand of
///            a tail `return` — because that is the only place the declared return type is what the
///            expression is checked against, so `T(a, b)` and `(a, b)` denote the same value. In a
///            match arm, a conditional branch, an argument or a `var` initializer the tuple is
///            unified with its siblings instead, and a bare `TupleN` does not unify with the named
///            type they carry (plato-404). Also requires the argument count to equal the field count
///            of a concrete `T` in the corpus, so a same-named conversion overload is never mistaken
///            for the field-wise constructor.
///   SIM002 - an expression that re-spells a constant the vocabulary already names. Any occurrence
///            of a literal constructor call that is the whole body of a `Name(_: T): T` constant
///            (`Vector2D.UnitX`) becomes that constant. The constants' own bodies are excluded, so
///            the rule cannot rewrite a definition into itself.
///
/// SIM002 wins where both apply: naming the constant beats shortening the literal.</summary>
public static class Simplifier
{
    public const string RedundantConstructor = "SIM001";
    public const string NamedConstant = "SIM002";

    public static List<SimplifyEdit> Check(IReadOnlyList<ParsedFile> files)
    {
        var parsed = files.Where(f => f.Ast != null).ToList();
        var arity = FieldCounts(parsed);
        var constants = Constants(parsed, out var definitions);
        var edits = new List<SimplifyEdit>();
        foreach (var file in parsed)
            Check(edits, file, arity, constants, definitions);
        edits.Sort((a, b) => a.File == b.File ? a.Begin.CompareTo(b.Begin) : string.CompareOrdinal(a.File, b.File));
        return edits;
    }

    public static List<SimplifyEdit> Check(ParsedFile file)
        => Check(new[] { file });

    /// <summary>Applies edits back-to-front so earlier offsets stay valid. Edits whose span overlaps
    /// one already applied are dropped, and an edit whose <see cref="SimplifyEdit.Before"/> no longer
    /// matches the text is a stale offset and throws rather than corrupting the file.</summary>
    public static string Apply(string text, IReadOnlyList<SimplifyEdit> edits)
    {
        var sb = new StringBuilder(text);
        var applied = int.MaxValue;
        foreach (var e in edits.OrderByDescending(e => e.Begin))
        {
            if (e.End > applied)
                continue;
            var actual = text[e.Begin..e.End];
            if (actual != e.Before)
                throw new ArgumentException($"{e.File}:{e.Line} edit no longer matches the source: expected '{e.Before}', found '{actual}'");
            sb.Remove(e.Begin, e.End - e.Begin).Insert(e.Begin, e.After);
            applied = e.Begin;
        }
        return sb.ToString();
    }

    private static void Check(List<SimplifyEdit> edits, ParsedFile file,
        IReadOnlyDictionary<string, int> arity,
        IReadOnlyDictionary<string, string> constants,
        IReadOnlyDictionary<string, List<(int Begin, int End)>> definitions)
    {
        var text = file.File.Text;
        var path = file.File.Path.ToString()!;
        definitions.TryGetValue(SourceSnapshot.PathKey(file.File.Path), out var frozen);
        var claimed = new HashSet<(int, int)>();

        foreach (var method in Methods(file.Ast!))
        {
            foreach (var node in Descendants(method.Body))
            {
                var call = CallAt(text, node);
                if (call == null || IsFrozen(frozen, call.Value))
                    continue;
                if (!constants.TryGetValue(Normalized(call.Value.Text), out var name))
                    continue;
                if (claimed.Add((call.Value.Begin, call.Value.End)))
                    edits.Add(Edit(path, text, call.Value, NamedConstant,
                        $"'{call.Value.Text}' is what '{name}' means; use the named constant",
                        name));
            }

            var returns = method.Type?.Name.Text;
            if (returns == null || !arity.TryGetValue(returns, out var fields) || fields < 2)
                continue;

            foreach (var result in Results(method.Body))
            {
                var call = CallAt(text, result);
                if (call == null || call.Value.Name != returns || call.Value.ArgCount != fields)
                    continue;
                if (IsFrozen(frozen, call.Value) || !claimed.Add((call.Value.Begin, call.Value.End)))
                    continue;
                edits.Add(Edit(path, text, call.Value, RedundantConstructor,
                    $"'{returns}' is already the declared return type; the tuple alone says it",
                    text[call.Value.Paren..call.Value.End]));
            }
        }
    }

    private static bool IsFrozen(IReadOnlyList<(int Begin, int End)>? frozen, Call call)
    {
        if (frozen == null)
            return false;
        for (var i = 0; i < frozen.Count; i++)
            if (call.Begin >= frozen[i].Begin && call.End <= frozen[i].End)
                return true;
        return false;
    }

    private static SimplifyEdit Edit(string path, string text, Call call, string code, string message, string after)
        => new(path, LineOf(text, call.Begin), ColumnOf(text, call.Begin), code, message,
            call.Begin, call.End, call.Text, after);

    /// <summary>The expressions that ARE a function's result, and nothing else: the whole body of an
    /// expression-bodied function and the operand of a tail `return`. Only there is the expression
    /// checked against the declared return type, which is what makes dropping a constructor name
    /// safe (plato-404); a match arm or a conditional branch is unified with its siblings, so a bare
    /// tuple there stops unifying with the named type the other branches carry.</summary>
    private static IEnumerable<AstNode> Results(AstNode? node)
    {
        switch (node)
        {
            case null:
                yield break;
            case AstParenthesized p:
                foreach (var n in Results(p.Inner)) yield return n;
                break;
            case AstBlock b when b.Statements.Count > 0:
                foreach (var n in Results(b.Statements[^1])) yield return n;
                break;
            case AstReturn r:
                foreach (var n in Results(r.Value)) yield return n;
                break;
            case AstBlock:
            case AstMatch:
            case AstConditional:
                yield break;
            default:
                yield return node;
                break;
        }
    }

    private static IEnumerable<AstNode> Descendants(AstNode? node)
    {
        if (node == null)
            yield break;
        yield return node;
        foreach (var child in node.Children)
        foreach (var d in Descendants(child))
            yield return d;
    }

    private static IEnumerable<AstMethodDeclaration> Methods(AstFile ast)
        => ast.Types.SelectMany(t => t.Members.OfType<AstMethodDeclaration>());

    private static Dictionary<string, int> FieldCounts(IReadOnlyList<ParsedFile> files)
    {
        var counts = new Dictionary<string, int>();
        foreach (var decl in files.SelectMany(f => f.Ast!.Types).Where(t => t.Kind == TypeKind.ConcreteType))
            counts[decl.Name.Text] = decl.Members.OfType<AstFieldDeclaration>().Count();
        return counts;
    }

    /// <summary>The vocabulary's structured constants: `Name(_: T): T =&gt; T(literal, literal…)`.
    /// Keyed by the whitespace-free body so any spelling of the same call matches. Bodies that are
    /// bare scalars are deliberately excluded — rewriting every `2000.0` into a named ratio would be
    /// noise, not simplification. A body two constants share is ambiguous and is dropped.</summary>
    private static Dictionary<string, string> Constants(IReadOnlyList<ParsedFile> files,
        out Dictionary<string, List<(int Begin, int End)>> definitions)
    {
        var found = new Dictionary<string, string>();
        var ambiguous = new HashSet<string>();
        definitions = new Dictionary<string, List<(int, int)>>();

        foreach (var file in files)
        foreach (var decl in file.Ast!.Types.Where(t => t.Kind == TypeKind.Library))
        foreach (var method in decl.Members.OfType<AstMethodDeclaration>())
        {
            var type = method.Type?.Name.Text;
            if (type == null || method.Parameters.Count != 1)
                continue;
            var p = method.Parameters[0];
            if (p.Name.Text != "_" || p.Type?.Name.Text != type)
                continue;

            var call = CallAt(file.File.Text, method.Body);
            if (call == null || call.Value.Name != type || call.Value.ArgCount < 2 || !call.Value.LiteralArgs)
                continue;

            var key = Normalized(call.Value.Text);
            var value = $"{type}.{method.Name.Text}";
            if (found.TryGetValue(key, out var prior) && prior != value)
                ambiguous.Add(key);
            found[key] = value;

            var path = SourceSnapshot.PathKey(file.File.Path);
            if (!definitions.TryGetValue(path, out var spans))
                definitions[path] = spans = new List<(int, int)>();
            spans.Add((call.Value.Begin, call.Value.End));
        }

        foreach (var key in ambiguous)
            found.Remove(key);
        return found;
    }

    /// <summary>A node's source text read back as `Name(arg, arg…)`, or null when it is not exactly
    /// that. Reading the text rather than trusting the node shape is what makes the rewrite safe:
    /// chained postfix expressions share one span with their inner call, and only the spelling says
    /// which is which.</summary>
    private static Call? CallAt(string text, AstNode? node)
    {
        var span = Span.From(node);
        if (!span.HasValue || span.End > text.Length || span.Length <= 0)
            return null;

        var begin = span.Begin;
        var end = span.End;
        while (begin < end && char.IsWhiteSpace(text[begin])) begin++;
        while (end > begin && char.IsWhiteSpace(text[end - 1])) end--;

        var slice = text[begin..end];
        if (slice.Contains('"') || slice.Length < 3 || slice[^1] != ')')
            return null;

        var i = 0;
        while (i < slice.Length && (char.IsLetterOrDigit(slice[i]) || slice[i] == '_')) i++;
        if (i == 0 || char.IsDigit(slice[0]))
            return null;
        var name = slice[..i];
        while (i < slice.Length && char.IsWhiteSpace(slice[i])) i++;
        if (i >= slice.Length || slice[i] != '(')
            return null;

        var args = Arguments(slice, i);
        return args == null ? null : new Call(begin, end, begin + i, name, slice, args.Value.Count, args.Value.Literal);
    }

    /// <summary>Top-level argument count of the parenthesized list starting at <paramref name="open"/>,
    /// or null when the parens do not close exactly at the end of the slice (a chained or partial
    /// expression). <c>Literal</c> is true when every argument is a numeric or boolean literal.</summary>
    private static (int Count, bool Literal)? Arguments(string slice, int open)
    {
        var depth = 0;
        var count = 0;
        var literal = true;
        var argStart = open + 1;
        for (var i = open; i < slice.Length; i++)
        {
            var c = slice[i];
            if (c is '(' or '[')
                depth++;
            else if (c is ')' or ']')
            {
                depth--;
                if (depth != 0)
                    continue;
                if (i != slice.Length - 1)
                    return null;
                var last = slice[argStart..i].Trim();
                if (last.Length > 0)
                {
                    count++;
                    literal &= IsLiteral(last);
                }
            }
            else if (c == ',' && depth == 1)
            {
                count++;
                literal &= IsLiteral(slice[argStart..i].Trim());
                argStart = i + 1;
            }
        }
        return depth == 0 ? (count, literal) : null;
    }

    private static bool IsLiteral(string arg)
        => arg is "true" or "false"
           || (arg.Length > 0 && arg.All(c => char.IsDigit(c) || c is '.' or '-' or '+' or 'e' or 'E'));

    private static string Normalized(string text)
        => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));

    private static int LineOf(string text, int offset)
        => text.Take(offset).Count(c => c == '\n') + 1;

    private static int ColumnOf(string text, int offset)
        => offset - (text.LastIndexOf('\n', Math.Max(0, offset - 1)) + 1) + 1;

    private readonly record struct Call(int Begin, int End, int Paren, string Name, string Text, int ArgCount, bool LiteralArgs);
}

/// <summary>One simplification: the exact source span to replace and what to replace it with.
/// <see cref="Line"/> and <see cref="Column"/> are 1-based, matching the check findings.</summary>
public sealed record SimplifyEdit(
    string File, int Line, int Column, string Code, string Message,
    int Begin, int End, string Before, string After);
