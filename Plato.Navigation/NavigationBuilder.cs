using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Navigation;

/// <summary>Turns a <see cref="BoundSnapshot"/> into the flat tables of a
/// <see cref="NavigationIndex"/>. Definitions come from the AST, so an outline and a name search
/// survive a failed bind; references come from the binder, which is the only thing that knows what
/// a name means.</summary>
internal static class NavigationBuilder
{
    public static NavigationIndex Build(BoundSnapshot bound)
    {
        var defs = new List<DefRecord>();
        var nodeToDef = new Dictionary<AstNode, int>(ReferenceEqualityComparer.Instance);
        var fileOfNode = new Dictionary<AstNode, int>(ReferenceEqualityComparer.Instance);

        foreach (var file in bound.Files.Where(f => f.Parsed))
        foreach (var type in file.Ast!.Types)
            AddType(type, file.File.Id, defs, nodeToDef, fileOfNode);

        AddBinderDefs(bound, defs, nodeToDef, fileOfNode);

        var refs = ReferenceExtractor.Extract(bound, defs, nodeToDef, fileOfNode);

        return new NavigationIndex(bound.Snapshot, defs, refs,
            BuildStatuses(bound), BuildDiagnostics(bound, fileOfNode));
    }

    private static void AddType(AstTypeDeclaration type, int fileId, List<DefRecord> defs,
        Dictionary<AstNode, int> nodeToDef, Dictionary<AstNode, int> fileOfNode)
    {
        var id = Add(defs, nodeToDef, fileOfNode, KindOf(type.Kind), type.Name.Text, TypeSignature(type),
            fileId, type.Name, type, -1);

        foreach (var tp in type.TypeParameters)
            Add(defs, nodeToDef, fileOfNode, DefKind.TypeParameter, tp.Name.Text, null, fileId, tp.Name, tp, id);

        foreach (var member in type.Members)
        {
            if (member is AstFieldDeclaration field)
            {
                Add(defs, nodeToDef, fileOfNode, DefKind.Field, field.Name.Text, TypeName(field.Type),
                    fileId, field.Name, field, id);
            }
            else if (member is AstMethodDeclaration method)
            {
                var mid = Add(defs, nodeToDef, fileOfNode, DefKind.Method, method.Name.Text,
                    MethodSignature(method), fileId, method.Name, method, id);
                foreach (var p in method.Parameters)
                    Add(defs, nodeToDef, fileOfNode, DefKind.Parameter, p.Name.Text, TypeName(p.Type),
                        fileId, p.Name, p, mid);
            }
        }

        foreach (var sumCase in type.Cases)
        {
            var cid = Add(defs, nodeToDef, fileOfNode, DefKind.SumCase, sumCase.Name.Text, null,
                fileId, sumCase.Name, sumCase, id);
            foreach (var f in sumCase.Fields)
                Add(defs, nodeToDef, fileOfNode, DefKind.SumCaseField, f.Name.Text, TypeName(f.Type),
                    fileId, f.Name, f, cid);
        }
    }

    /// <summary>Locals and lambda parameters exist only inside function bodies, which the AST walk
    /// above deliberately does not descend into — the binder is what turns them into definitions,
    /// so they are read back from it. Ordered by symbol id to keep the table deterministic.</summary>
    private static void AddBinderDefs(BoundSnapshot bound, List<DefRecord> defs,
        Dictionary<AstNode, int> nodeToDef, Dictionary<AstNode, int> fileOfNode)
    {
        if (bound.Factory == null)
            return;

        foreach (var kv in bound.Factory.SymbolsToNodes.OrderBy(kv => kv.Key.Id))
        {
            if (nodeToDef.ContainsKey(kv.Value))
                continue;

            var (kind, nameNode) = kv.Value switch
            {
                AstVarDef v when kv.Key is VariableDef => (DefKind.Variable, (AstNode)v.Name),
                AstParameterDeclaration p when kv.Key is ParameterDef => (DefKind.Parameter, p.Name),
                _ => (default(DefKind), null)
            };
            if (nameNode == null)
                continue;

            Add(defs, nodeToDef, fileOfNode, kind, ((AstLeaf)nameNode).Text, null,
                FileOf(bound, kv.Value, fileOfNode), nameNode, kv.Value, -1);
        }
    }

    private static int Add(List<DefRecord> defs, Dictionary<AstNode, int> nodeToDef,
        Dictionary<AstNode, int> fileOfNode, DefKind kind, string name, string? signature,
        int fileId, AstNode nameNode, AstNode declNode, int owner)
    {
        var id = defs.Count;
        defs.Add(new DefRecord(id, kind, name, signature, fileId,
            Span.From(nameNode), Span.From(declNode), owner));
        nodeToDef[declNode] = id;
        fileOfNode[declNode] = fileId;
        fileOfNode[nameNode] = fileId;
        return id;
    }

    private static DefKind KindOf(TypeKind kind) => kind switch
    {
        TypeKind.Library => DefKind.Library,
        TypeKind.Interface => DefKind.Interface,
        TypeKind.Primitive => DefKind.Primitive,
        TypeKind.TypeParameter => DefKind.TypeParameter,
        _ => DefKind.Type
    };

    private static string TypeName(AstTypeNode? type)
        => type == null ? "" :
            type.TypeArguments.Count == 0
                ? type.Name.Text
                : $"{type.Name.Text}<{string.Join(", ", type.TypeArguments.Select(TypeName))}>";

    private static string TypeSignature(AstTypeDeclaration type)
        => type.TypeParameters.Count == 0
            ? type.Name.Text
            : $"{type.Name.Text}<{string.Join(", ", type.TypeParameters.Select(tp => tp.Name.Text))}>";

    private static string MethodSignature(AstMethodDeclaration method)
        => $"{method.Name.Text}({string.Join(", ", method.Parameters.Select(p => TypeName(p.Type)))}): {TypeName(method.Type)}";

    private static IReadOnlyList<FileStatus> BuildStatuses(BoundSnapshot bound)
        => bound.Files.Select(f => new FileStatus(f.File.Id, f.File.Path.ToString()!, f.File.ContentHash,
            !f.Parsed ? FileState.ParseFailed : bound.Bound ? FileState.Bound : FileState.Parsed)).ToList();

    private static IReadOnlyList<Diagnostic> BuildDiagnostics(BoundSnapshot bound, Dictionary<AstNode, int> fileOfNode)
    {
        var result = new List<Diagnostic>();

        foreach (var f in bound.Files.Where(f => !f.Parsed))
        foreach (var message in f.ParseErrors)
            result.Add(new Diagnostic(DiagnosticKind.Parse, message, f.File.Id, Span.None));

        if (bound.BindAbortReason != null)
            result.Add(new Diagnostic(DiagnosticKind.BindAbort, bound.BindAbortReason, -1, Span.None));

        foreach (var e in bound.ResolutionErrors)
            result.Add(new Diagnostic(DiagnosticKind.Resolution, e.Message,
                FileOf(bound, e.Node, fileOfNode), Span.From(e.Node)));

        return result;
    }

    /// <summary>The file a node belongs to. The parser records the file on every range, so the
    /// lookup falls back to the range's path when the node is not one we recorded ourselves.</summary>
    internal static int FileOf(BoundSnapshot bound, AstNode? node, Dictionary<AstNode, int> fileOfNode)
    {
        if (node == null)
            return -1;
        if (fileOfNode.TryGetValue(node, out var id))
            return id;

        var path = node.GetRange()?.FilePath;
        return path == null ? -1 : bound.Snapshot.Find(path.Value)?.Id ?? -1;
    }
}
