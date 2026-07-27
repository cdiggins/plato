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
            if (kv.Key is not RefSymbol r || r.Def == null)
                continue;

            var targets = resolver.Targets(r.Def);
            if (targets.Count == 0)
                continue;

            refs.Add(new RefRecord(refs.Count, RefKind.Value, r.Name,
                NavigationBuilder.FileOf(bound, kv.Value, fileOfNode), Span.From(kv.Value), targets));
        }

        foreach (var tr in factory.TypeReferences)
        {
            var targets = resolver.Targets(tr.Type.Def);
            if (targets.Count == 0)
                continue;

            // The identifier, not the whole type node: "Array<Number>" would otherwise enclose
            // "Number" and every position inside it would hit two overlapping type references.
            var name = tr.Node.Name;
            refs.Add(new RefRecord(refs.Count, RefKind.Type, tr.Type.Def.Name,
                NavigationBuilder.FileOf(bound, name, fileOfNode), Span.From(name), targets));
        }

        return refs;
    }

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
