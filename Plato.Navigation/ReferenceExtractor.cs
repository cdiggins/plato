using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Navigation;

/// <summary>Reads reference occurrences out of a completed bind. Value references come from
/// <see cref="SymbolFactory.SymbolsToNodes"/> (the binder makes a fresh RefSymbol per resolution,
/// so there is one entry per occurrence); type references come from
/// <see cref="SymbolFactory.TypeReferences"/>.</summary>
internal static class ReferenceExtractor
{
    public static IReadOnlyList<RefRecord> Extract(BoundSnapshot bound, IReadOnlyList<DefRecord> defs,
        Dictionary<AstNode, int> nodeToDef, Dictionary<AstNode, int> fileOfNode)
    {
        var factory = bound.Factory;
        if (factory == null)
            return Array.Empty<RefRecord>();

        var resolver = new DefResolver(factory, nodeToDef);
        var refs = new List<RefRecord>();

        foreach (var kv in factory.SymbolsToNodes)
        {
            if (kv.Key is not RefSymbol r)
                continue;

            // A reference with no targets is kept, not dropped: "Self", "Any" and the "default"
            // keyword are real name occurrences that resolve to something with no source of its
            // own. Recording them is what lets the sweep prove nothing went missing silently.
            var targets = r.Def == null ? Array.Empty<int>() : resolver.Targets(r.Def);

            var fileId = NavigationBuilder.FileOf(bound, kv.Value, fileOfNode);
            var (kind, span) = Classify(bound, fileId, Span.From(kv.Value), r.Name);
            refs.Add(new RefRecord(refs.Count, kind, r.Name, fileId, span, targets));
        }

        foreach (var tr in factory.TypeReferences)
        {
            var targets = resolver.Targets(tr.Type.Def);

            // The identifier, not the whole type node: "Array<Number>" would otherwise enclose
            // "Number" and every position inside it would hit two overlapping type references.
            var name = tr.Node.Name;
            var fileId = NavigationBuilder.FileOf(bound, name, fileOfNode);
            refs.Add(new RefRecord(refs.Count, RefKind.Type, name.Text, fileId,
                WithSigil(bound, fileId, Span.From(name)), targets));
        }

        return refs;
    }

    /// <summary>A binary operator reaches the binder as a synthesized call whose function
    /// identifier carries the WHOLE expression's location (Ast.cs, AstBinaryOp.ToInvocation), so
    /// "a + b" arrives as a reference named "Add" spanning "+ b". The span starts exactly at the
    /// operator, so it is narrowed to the operator token and the reference is marked as such
    /// rather than pretending the source there reads "Add".</summary>
    private static (RefKind, Span) Classify(BoundSnapshot bound, int fileId, Span span, string name)
    {
        if (fileId < 0 || !span.HasValue)
            return (RefKind.Value, span);

        var text = bound.Snapshot.Files[fileId].Text;
        if (span.End > text.Length || text.Substring(span.Begin, span.Length).Trim() == name)
            return (RefKind.Value, span);

        var end = span.Begin;
        while (end < text.Length && IsOperatorChar(text[end]))
            end++;

        return (RefKind.Operator, end > span.Begin
            ? span with { End = end, EndLine = span.BeginLine, EndColumn = span.BeginColumn + (end - span.Begin) }
            : span);
    }

    /// <summary>A type variable is named "$T" but the parser's identifier range starts at the "T".
    /// The sigil is pulled back into the span so clicking it navigates and the span reads as the
    /// name does.</summary>
    private static Span WithSigil(BoundSnapshot bound, int fileId, Span span)
    {
        if (fileId < 0 || !span.HasValue || span.Begin == 0)
            return span;

        var text = bound.Snapshot.Files[fileId].Text;
        return text[span.Begin - 1] == '$'
            ? span with { Begin = span.Begin - 1, BeginColumn = span.BeginColumn - 1 }
            : span;
    }

    private static bool IsOperatorChar(char c)
        => "+-*/%<>=!&|^~?".IndexOf(c) >= 0;

    /// <summary>Maps a bound definition symbol back to the AST-derived definition records. A
    /// function group maps to every overload (D4); a compiler-generated function (constructor,
    /// sum-case factory, implicit cast) has no syntax of its own and maps to its owning type.</summary>
    private sealed class DefResolver
    {
        private readonly SymbolFactory _factory;
        private readonly Dictionary<AstNode, int> _nodeToDef;
        private readonly Dictionary<FunctionDef, MemberDef> _functionToMember = new();

        public DefResolver(SymbolFactory factory, Dictionary<AstNode, int> nodeToDef)
        {
            _factory = factory;
            _nodeToDef = nodeToDef;

            foreach (var type in factory.TypeDefs)
            foreach (var member in type.Members)
                if (member.Function != null)
                    _functionToMember[member.Function] = member;
        }

        public IReadOnlyList<int> Targets(DefSymbol def)
        {
            if (def is FunctionGroupDef group)
                return group.Functions.Select(OfFunction).Where(i => i >= 0).Distinct().ToList();

            var id = OfSymbol(def);
            return id >= 0 ? new[] { id } : Array.Empty<int>();
        }

        private int OfFunction(FunctionDef f)
            => f == null ? -1
                : _functionToMember.TryGetValue(f, out var member) ? OfSymbol(member)
                : f.OwnerType != null ? OfSymbol(f.OwnerType)
                : -1;

        private int OfSymbol(Symbol symbol)
            => _factory.SymbolsToNodes.TryGetValue(symbol, out var node)
               && _nodeToDef.TryGetValue(node, out var id)
                ? id
                : -1;
    }
}
