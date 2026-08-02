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
///   SIM003 - a concrete function whose body an implemented interface already derives. When a
///            library declares `F(self: I, …)` and `F(self: C, …)` with the same result type, the
///            same remaining parameters and the same body modulo parameter names, and `C` implements
///            `I`, the concrete copy adds nothing: deleting it leaves every call resolving to the
///            derived body. Applied by deleting the declaration together with its own comment block.
///   SIM004 - a family of identical bodies whose receivers share an interface. REPORT ONLY, never
///            applied: collapsing such a family usually needs a NEW interface to name what the
///            receivers have in common, which is a vocabulary decision, and a body written against
///            an interface-typed parameter re-resolves its inner calls against that interface
///            (compiler-408), so some families cannot be collapsed at all.
///
/// SIM002 wins where both apply: naming the constant beats shortening the literal. A declaration
/// SIM003 removes is claimed whole, so no other rule proposes an edit inside it.</summary>
public static class Simplifier
{
    public const string RedundantConstructor = "SIM001";
    public const string NamedConstant = "SIM002";
    public const string DerivedBody = "SIM003";
    public const string SharedBody = "SIM004";

    /// <summary>How many spellings of one body make a family worth a SIM004 report. Two identical
    /// one-liners are a coincidence; the families found by hand were seven, seven and twelve.</summary>
    public const int FamilySize = 3;

    public static List<SimplifyEdit> Check(IReadOnlyList<ParsedFile> files)
    {
        var parsed = files.Where(f => f.Ast != null).ToList();
        var vocabulary = Vocabulary.Read(parsed);
        var edits = new List<SimplifyEdit>();
        foreach (var file in parsed)
            Check(edits, file, vocabulary);
        Families(edits, vocabulary);
        edits.Sort((a, b) => a.File == b.File ? a.Begin.CompareTo(b.Begin) : string.CompareOrdinal(a.File, b.File));
        return edits;
    }

    public static List<SimplifyEdit> Check(ParsedFile file)
        => Check(new[] { file });

    /// <summary>Applies edits back-to-front so earlier offsets stay valid. Report-only findings
    /// (<see cref="SimplifyEdit.Applicable"/> false) are skipped, edits whose span overlaps one
    /// already applied are dropped, and an edit whose <see cref="SimplifyEdit.Before"/> no longer
    /// matches the text is a stale offset and throws rather than corrupting the file.</summary>
    public static string Apply(string text, IReadOnlyList<SimplifyEdit> edits)
    {
        var sb = new StringBuilder(text);
        var applied = int.MaxValue;
        foreach (var e in edits.Where(e => e.Applicable).OrderByDescending(e => e.Begin))
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

    private static void Check(List<SimplifyEdit> edits, ParsedFile file, Vocabulary vocabulary)
    {
        var text = file.File.Text;
        var path = file.File.Path.ToString()!;
        vocabulary.Definitions.TryGetValue(SourceSnapshot.PathKey(file.File.Path), out var frozen);
        var removed = Derived(edits, file, vocabulary, path, text);
        var claimed = new HashSet<(int, int)>();

        foreach (var method in Methods(file.Ast!))
        {
            foreach (var node in Descendants(method.Body))
            {
                var call = CallAt(text, node);
                if (call == null || Within(frozen, call.Value) || Within(removed, call.Value))
                    continue;
                if (!vocabulary.Constants.TryGetValue(Normalized(call.Value.Text), out var name))
                    continue;
                if (claimed.Add((call.Value.Begin, call.Value.End)))
                    edits.Add(Edit(path, text, call.Value, NamedConstant,
                        $"'{call.Value.Text}' is what '{name}' means; use the named constant",
                        name));
            }

            var returns = method.Type?.Name.Text;
            if (returns == null || !vocabulary.Arity.TryGetValue(returns, out var fields) || fields < 2)
                continue;

            foreach (var result in Results(method.Body))
            {
                var call = CallAt(text, result);
                if (call == null || call.Value.Name != returns || call.Value.ArgCount != fields)
                    continue;
                if (Within(frozen, call.Value) || Within(removed, call.Value)
                    || !claimed.Add((call.Value.Begin, call.Value.End)))
                    continue;
                edits.Add(Edit(path, text, call.Value, RedundantConstructor,
                    $"'{returns}' is already the declared return type; the tuple alone says it",
                    text[call.Value.Paren..call.Value.End]));
            }
        }
    }

    /// <summary>SIM003 over one file, returning the spans it deletes so the other rules leave the
    /// doomed text alone (an edit inside a deleted span would otherwise win the overlap race in
    /// <see cref="Apply"/> and keep the declaration alive).</summary>
    private static List<(int Begin, int End)> Derived(List<SimplifyEdit> edits, ParsedFile file,
        Vocabulary vocabulary, string path, string text)
    {
        var removed = new List<(int, int)>();
        foreach (var fn in vocabulary.Functions.Where(f => ReferenceEquals(f.File, file)))
        {
            var covering = Covering(vocabulary, fn);
            if (covering == null)
                continue;
            var span = Removal(text, fn.Declaration);
            if (span == null)
                continue;
            var at = Span.From(fn.Declaration.Name);
            removed.Add(span.Value);
            edits.Add(new SimplifyEdit(path, LineOf(text, at.Begin), ColumnOf(text, at.Begin),
                DerivedBody,
                $"'{fn.Name}({covering.Receiver})' already derives this body, and '{fn.Receiver}' implements '{covering.Receiver}'; the concrete copy says nothing more",
                span.Value.Begin, span.Value.End, text[span.Value.Begin..span.Value.End], ""));
        }
        return removed;
    }

    /// <summary>The interface-receiver function that already answers <paramref name="fn"/> for its
    /// concrete receiver, or null. The result type must match exactly; a remaining parameter matches
    /// either by name or when the interface spells it as one the concrete parameter implements,
    /// which is what covers `LessThanOrEquals(a: IIndex, b: IIndex)`.</summary>
    private static Fn? Covering(Vocabulary vocabulary, Fn fn)
    {
        if (!vocabulary.Arity.ContainsKey(fn.Receiver))
            return null;
        var closure = vocabulary.Closure(fn.Receiver);
        if (closure.Count == 0)
            return null;
        return vocabulary.Functions.FirstOrDefault(g => !ReferenceEquals(g, fn)
            && g.Name == fn.Name
            && g.Returns == fn.Returns
            && g.Body == fn.Body
            && closure.Contains(g.Receiver)
            && Substitutable(vocabulary, fn.Rest, g.Rest));
    }

    private static bool Substitutable(Vocabulary vocabulary, IReadOnlyList<string> concrete, IReadOnlyList<string> general)
        => concrete.Count == general.Count
           && concrete.Zip(general).All(p => p.First == p.Second || vocabulary.Closure(p.First).Contains(p.Second));

    /// <summary>SIM004: one report per family of identical bodies over three or more concrete
    /// receivers that share an interface. Members SIM003 already removes are left out, so a family
    /// the vocabulary has already named is not reported twice.</summary>
    private static void Families(List<SimplifyEdit> edits, Vocabulary vocabulary)
    {
        foreach (var group in vocabulary.Functions
                     .Where(f => vocabulary.Arity.ContainsKey(f.Receiver) && Covering(vocabulary, f) == null)
                     .GroupBy(f => (f.Name, f.Returns, f.Body, Others: string.Join(",", f.Rest))))
        {
            var members = group.OrderBy(f => f.File.File.Id).ThenBy(f => Span.From(f.Declaration).Begin).ToList();
            if (members.Count < FamilySize)
                continue;

            var shared = members.Select(m => (IEnumerable<string>)vocabulary.Closure(m.Receiver))
                .Aggregate((a, b) => a.Intersect(b))
                .OrderBy(s => s, StringComparer.Ordinal).ToList();
            if (shared.Count == 0)
                continue;

            var lead = members[0];
            var text = lead.File.File.Text;
            var at = Span.From(lead.Declaration.Name);
            var name = text[at.Begin..at.End];
            edits.Add(new SimplifyEdit(lead.File.File.Path.ToString()!,
                LineOf(text, at.Begin), ColumnOf(text, at.Begin), SharedBody,
                $"'{lead.Name}' repeats this body for {members.Count} types ({string.Join(", ", members.Select(m => m.Receiver))}); all of them implement {string.Join(", ", shared)} — one derived body there would replace the family",
                at.Begin, at.End, name, name, Applicable: false));
        }
    }

    /// <summary>The text a SIM003 deletion removes: the declaration, its trailing newline, one
    /// following blank line, and the contiguous `//` comment lines directly above it (a `//==`
    /// section banner introduces the section, not the declaration, and stays).</summary>
    private static (int Begin, int End)? Removal(string text, AstMethodDeclaration declaration)
    {
        var span = Span.From(declaration);
        if (Tail(text, declaration) is not { } tail)
            return null;

        var end = LineEnd(text, tail);
        var blank = LineEnd(text, end);
        if (text[end..blank].Trim().Length == 0 && blank > end)
            end = blank;

        var begin = span.Begin;
        while (begin < text.Length && char.IsWhiteSpace(text[begin]))
            begin++;
        begin = LineStart(text, begin);
        while (begin > 0)
        {
            var previous = LineStart(text, begin - 2);
            var line = text[previous..begin].Trim();
            if (!line.StartsWith("//", StringComparison.Ordinal) || line.StartsWith("//=", StringComparison.Ordinal))
                break;
            begin = previous;
        }
        return begin < end ? (begin, end) : null;
    }

    /// <summary>The end of a declaration's OWN text. A declaration node's range runs on to the start
    /// of the next declaration, so it carries the blank lines and the next declaration's comment
    /// block with it; this walks that trivia back off and stops at the last character of code.</summary>
    private static int? Tail(string text, AstMethodDeclaration declaration)
    {
        var span = Span.From(declaration);
        if (!span.HasValue || span.End > text.Length || span.Length <= 0)
            return null;

        var end = span.End;
        while (end > span.Begin)
        {
            while (end > span.Begin && char.IsWhiteSpace(text[end - 1]))
                end--;
            var start = LineStart(text, end - 1);
            if (start < span.Begin || !text[start..end].TrimStart().StartsWith("//", StringComparison.Ordinal))
                break;
            end = start;
        }
        return end > span.Begin ? end : null;
    }

    /// <summary>The body's own text: everything after the `=&gt;` of an expression body, or the block
    /// from its opening brace. Read from the source rather than from the body node, whose range for a
    /// binary expression starts at the OPERATOR (`a.Value &lt;= b.Value` ranges from the `&lt;=`),
    /// which would make two bodies with different left operands compare equal.</summary>
    private static string? BodySlice(string text, AstMethodDeclaration declaration)
    {
        var span = Span.From(declaration);
        if (Tail(text, declaration) is not { } end)
            return null;

        for (var i = span.Begin; i < end; i++)
        {
            if (text[i] == '{')
                return text[i..end];
            if (text[i] == '=' && i + 1 < end && text[i + 1] == '>')
                return text[(i + 2)..end].Trim().TrimEnd(';');
        }
        return null;
    }

    private static bool Within(IReadOnlyList<(int Begin, int End)>? spans, Call call)
    {
        if (spans == null)
            return false;
        for (var i = 0; i < spans.Count; i++)
            if (call.Begin >= spans[i].Begin && call.End <= spans[i].End)
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

    private static int LineStart(string text, int offset)
        => offset <= 0 ? 0 : text.LastIndexOf('\n', Math.Min(offset, text.Length - 1)) + 1;

    private static int LineEnd(string text, int offset)
    {
        var i = text.IndexOf('\n', Math.Min(offset, Math.Max(0, text.Length - 1)));
        return i < 0 ? text.Length : i + 1;
    }

    private readonly record struct Call(int Begin, int End, int Paren, string Name, string Text, int ArgCount, bool LiteralArgs);

    /// <summary>One library function reduced to what the duplicate-body rules compare: its name, the
    /// receiver's declared type, the remaining parameter types, the result type, and the body with
    /// every parameter renamed to its position, so two spellings of one computation are one string.</summary>
    private sealed record Fn(ParsedFile File, AstMethodDeclaration Declaration, string Name,
        string Receiver, IReadOnlyList<string> Rest, string Returns, string Body);

    /// <summary>The corpus-wide facts no single file can know: the field count of every concrete
    /// type, the interface closure of every declared type, the vocabulary's named constants (with
    /// the spans of their own definitions, which must never be rewritten into themselves), and every
    /// library function in comparable form.</summary>
    private sealed class Vocabulary
    {
        public IReadOnlyDictionary<string, int> Arity { get; private init; } = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, string> Constants { get; private init; } = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, List<(int Begin, int End)>> Definitions { get; private init; }
            = new Dictionary<string, List<(int, int)>>();
        public IReadOnlyList<Fn> Functions { get; private init; } = Array.Empty<Fn>();

        private IReadOnlyDictionary<string, IReadOnlyList<string>> Interfaces { get; init; }
            = new Dictionary<string, IReadOnlyList<string>>();

        /// <summary>Every interface a type implements, directly or through another interface.</summary>
        public IReadOnlyList<string> Closure(string type)
            => Interfaces.TryGetValue(type, out var c) ? c : Array.Empty<string>();

        public static Vocabulary Read(IReadOnlyList<ParsedFile> files)
        {
            var types = files.SelectMany(f => f.Ast!.Types).ToList();
            var constants = ReadConstants(files, out var definitions);
            return new Vocabulary
            {
                Arity = types.Where(t => t.Kind == TypeKind.ConcreteType)
                    .GroupBy(t => t.Name.Text)
                    .ToDictionary(g => g.Key, g => g.First().Members.OfType<AstFieldDeclaration>().Count()),
                Interfaces = Closures(types),
                Constants = constants,
                Definitions = definitions,
                Functions = files.SelectMany(ReadFunctions).ToList()
            };
        }

        private static IReadOnlyDictionary<string, IReadOnlyList<string>> Closures(IReadOnlyList<AstTypeDeclaration> types)
        {
            var interfaces = types.Where(t => t.Kind == TypeKind.Interface).Select(t => t.Name.Text).ToHashSet();
            var supers = new Dictionary<string, List<string>>();
            foreach (var t in types)
                supers[t.Name.Text] = t.Inherits.Concat(t.Implements).Select(n => n.Name.Text).ToList();

            var closures = new Dictionary<string, IReadOnlyList<string>>();
            foreach (var (name, direct) in supers)
            {
                var seen = new HashSet<string>();
                var pending = new Stack<string>(direct);
                while (pending.Count > 0)
                {
                    var s = pending.Pop();
                    if (!seen.Add(s) || !supers.TryGetValue(s, out var next))
                        continue;
                    foreach (var t in next)
                        pending.Push(t);
                }
                seen.RemoveWhere(s => !interfaces.Contains(s));
                closures[name] = seen.OrderBy(s => s, StringComparer.Ordinal).ToList();
            }
            return closures;
        }

        private static IEnumerable<Fn> ReadFunctions(ParsedFile file)
            => file.Ast!.Types.Where(t => t.Kind == TypeKind.Library)
                .SelectMany(t => t.Members.OfType<AstMethodDeclaration>())
                .Where(d => d.Type?.Name.Text != null && d.Parameters.Count > 0
                            && d.Parameters.All(p => p.Type?.Name.Text != null))
                .Select(d => (Declaration: d, Body: BodyKey(file.File.Text, d)))
                .Where(x => x.Body != null)
                .Select(x => new Fn(file, x.Declaration, x.Declaration.Name.Text,
                    x.Declaration.Parameters[0].Type!.Name.Text,
                    x.Declaration.Parameters.Skip(1).Select(p => p.Type!.Name.Text).ToList(),
                    x.Declaration.Type!.Name.Text, x.Body!));

        /// <summary>The body with every parameter replaced by its position and every run of
        /// whitespace collapsed to one space. An identifier directly after a `.` is a member name,
        /// never a parameter, so `self.Value` survives a parameter called Value.</summary>
        private static string? BodyKey(string text, AstMethodDeclaration declaration)
        {
            if (BodySlice(text, declaration) is not { Length: > 0 } slice)
                return null;

            var positions = new Dictionary<string, int>();
            for (var p = 0; p < declaration.Parameters.Count; p++)
                positions[declaration.Parameters[p].Name.Text] = p;

            var sb = new StringBuilder();
            var space = false;
            var i = 0;
            while (i < slice.Length)
            {
                if (char.IsWhiteSpace(slice[i]))
                {
                    space = true;
                    i++;
                    continue;
                }
                if (space && sb.Length > 0)
                    sb.Append(' ');
                space = false;
                if (!char.IsLetter(slice[i]) && slice[i] != '_')
                {
                    sb.Append(slice[i++]);
                    continue;
                }
                var j = i;
                while (j < slice.Length && (char.IsLetterOrDigit(slice[j]) || slice[j] == '_')) j++;
                var word = slice[i..j];
                var member = sb.Length > 0 && sb[^1] == '.';
                sb.Append(!member && positions.TryGetValue(word, out var k) ? $"${k}" : word);
                i = j;
            }
            return sb.ToString();
        }

        /// <summary>The vocabulary's structured constants: `Name(_: T): T =&gt; T(literal, literal…)`.
        /// Keyed by the whitespace-free body so any spelling of the same call matches. Bodies that are
        /// bare scalars are deliberately excluded — rewriting every `2000.0` into a named ratio would be
        /// noise, not simplification. A body two constants share is ambiguous and is dropped.</summary>
        private static Dictionary<string, string> ReadConstants(IReadOnlyList<ParsedFile> files,
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
    }
}

/// <summary>One simplification: the exact source span to replace and what to replace it with.
/// <see cref="Line"/> and <see cref="Column"/> are 1-based, matching the check findings.
/// <see cref="Applicable"/> is false for a report-only finding, whose <see cref="Before"/> and
/// <see cref="After"/> are the same text — it names a duplication a human must resolve, and
/// <see cref="Simplifier.Apply"/> never touches it.</summary>
public sealed record SimplifyEdit(
    string File, int Line, int Column, string Code, string Message,
    int Begin, int End, string Before, string After, bool Applicable = true);
