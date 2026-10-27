// Replaces supported array combinators with loop markers and temporary references.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Recognizes array operations; the scope traversal places their loops in evaluation order.</summary>
public static partial class TirLoopLowerer
{
    // Deterministic temp counter; reset per generation (CSharpWriter.WriteAll).
    // Per thread: the C# writer resets it per generation so output is deterministic, and two
    // programs emitted at once (the warm law server) must not reset each other's names.
    [System.ThreadStatic] public static int NextId;

    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.LowerLoops || tir?.Body == null)
            return tir;
        return RewriteScoped(tir);
    }

    /// <summary>Whether a call is a single-source consumer whose receiver chain the lowerer walks
    /// inside its own loop (see <see cref="TirLoweredLoop.SourceStages"/>).</summary>
    private static bool IsChainConsumer(TirCall c)
        => c.Name != null && c.Args.Count >= 2 && IndexSourceKinds.Contains(c.Name == "MapEager" ? "Map" : c.Name)
           && IsListTyped(c.Args[0]) && IsProducer(c.Args[0]);

    /// <summary>Whether a node is a Map over a list or a MapRange, with an inlinable function.</summary>
    private static bool IsProducer(TirNode n)
        => TirRewrite.StripCoerce(n) is TirCall p && p.Args.Count == 2 && FnOk(p.Args[1], 1)
           && ((p.Name == "Map" && IsListTyped(p.Args[0])) || p.Name == "MapRange" || p.Name == "MapRangeEager");

    private static bool IsListTyped(TirNode node)
        => TirRewrite.IsListType(TirRewrite.StripCoerce(node)?.Type);

    /// <summary>Whether a function argument can be placed in a loop: a non-eta lambda literal of
    /// the arity, or a delegate-typed reference of that arity.</summary>
    private static bool FnOk(TirNode f, int arity)
    {
        f = TirRewrite.StripCoerce(f);
        if (f is TirLambda lam)
            return lam.Parameters.Count == arity
                   && !(lam.Parameters.Count > 0 && lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta")));
        // A delegate-typed reference is INVOKED with `arity` arguments, so its declared
        // Function{N} arity must match exactly.
        var declared = (f as TirParameter)?.Def?.Type?.Name ?? (f as TirVariable)?.Def?.Type?.Name;
        return (f is TirParameter || f is TirVariable) && declared == $"Function{arity}";
    }

    /// <summary>The lowered marker + temp reference for a recognized combinator call, or null.</summary>
    private static TirNode TryLower(TirCall c, List<TirNode> hoists)
    {
        if (c.Name == null || c.Args.Count == 0)
            return null;

        string kind = null;
        List<TirNode> sources = null;
        TirNode fn = null, seed = null, includeFirst = null;

        switch (c.Name)
        {
            case "Map" or "MapEager" when c.Args.Count == 2 && IsListTyped(c.Args[0]) && ResultIsList(c):
                if (FnOk(c.Args[1], 1)) { kind = "Map"; }
                else if (FnOk(c.Args[1], 2)) { kind = "MapIdx"; }
                else return null;
                sources = new List<TirNode> { c.Args[0] };
                fn = c.Args[1];
                break;
            case "MapRange" or "MapRangeEager" when c.Args.Count == 2 && ResultIsList(c) && FnOk(c.Args[1], 1):
                kind = "MapRange"; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1];
                break;
            case "Zip" when c.Args.Count == 3 && IsListTyped(c.Args[0]) && IsListTyped(c.Args[1]) && ResultIsList(c) && FnOk(c.Args[2], 2):
                kind = "Zip2"; sources = new List<TirNode> { c.Args[0], c.Args[1] }; fn = c.Args[2];
                break;
            case "Zip" when c.Args.Count == 4 && IsListTyped(c.Args[0]) && IsListTyped(c.Args[1]) && IsListTyped(c.Args[2]) && ResultIsList(c) && FnOk(c.Args[3], 3):
                kind = "Zip3"; sources = new List<TirNode> { c.Args[0], c.Args[1], c.Args[2] }; fn = c.Args[3];
                break;
            case "Reduce" when c.Args.Count == 3 && IsListTyped(c.Args[0]) && FnOk(c.Args[2], 2):
                kind = "Reduce"; sources = new List<TirNode> { c.Args[0] }; seed = c.Args[1]; fn = c.Args[2];
                break;
            case "All" or "Any" when c.Args.Count == 2 && IsListTyped(c.Args[0]) && FnOk(c.Args[1], 1):
                kind = c.Name; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1];
                break;
            case "FlatMap" when c.Args.Count == 2 && IsListTyped(c.Args[0]) && ResultIsList(c) && FnOk(c.Args[1], 1):
                kind = "FlatMap"; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1];
                break;
            case "Reverse" when c.Args.Count == 1 && IsListTyped(c.Args[0]) && ResultIsList(c):
                kind = "Reverse"; sources = new List<TirNode> { c.Args[0] };
                break;
            case "WithNext" when c.Args.Count == 3 && IsListTyped(c.Args[0]) && ResultIsList(c) && FnOk(c.Args[1], 2):
                kind = "WithNext"; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1]; includeFirst = c.Args[2];
                break;
            case "MapPairs" when c.Args.Count == 2 && IsListTyped(c.Args[0]) && ResultIsList(c) && FnOk(c.Args[1], 2):
                kind = "MapPairs"; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1];
                break;
            case "MapTriplets" when c.Args.Count == 2 && IsListTyped(c.Args[0]) && ResultIsList(c) && FnOk(c.Args[1], 3):
                kind = "MapTriplets"; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1];
                break;
            case "MapQuartets" when c.Args.Count == 2 && IsListTyped(c.Args[0]) && ResultIsList(c) && FnOk(c.Args[1], 4):
                kind = "MapQuartets"; sources = new List<TirNode> { c.Args[0] }; fn = c.Args[1];
                break;
            default:
                return null;
        }

        // The result element type. The producing FUNCTION's type is the most reliable source
        // (Function{N}<..., R>); the call's zonked IArray<T> argument and (for Reverse) the
        // source's element type are fallbacks. Disagreement between known sources means the
        // solver unified loosely — skip rather than allocate the wrong array type.
        TypeExpression elem = null;
        if (kind == "FlatMap")
        {
            // The function yields a list per element; the result element type is its element type.
            elem = new[] { TirRewrite.ListElementType(FnReturnType(fn)), TirRewrite.ListElementType(c.Type) }
                .FirstOrDefault(IsRenderableConcrete);
            if (elem == null)
                return null;
        }
        else if (ArrayResultKinds.Contains(kind))
        {
            // Candidate element types in order of reliability: the producing function's own
            // return type, the call's zonked IArray<T> argument, and (Reverse) the source's
            // element type. Pick the first CONCRETE, renderable one. A generic-element library
            // function (MapComponents/ZipComponents over IArrayLike<Self,T>) is not ground in
            // its element type here — its body carries a raw solver type variable ($T), which is
            // not a valid C# type. Leaving such a call unlowered is correct: it emits as the
            // normal (generic-capable) library method, and --optimize already unrolls the
            // component fanout at concrete call sites.
            var candidates = new[]
            {
                fn == null ? null : FnReturnType(fn),
                TirRewrite.ListElementType(c.Type),
                kind == "Reverse" ? TirRewrite.ListElementType(TirRewrite.StripCoerce(sources[0])?.Type) : null,
            };
            elem = candidates.FirstOrDefault(IsRenderableConcrete);
            if (elem == null)
                return null;
        }

        // A receiver that is a chain of Map calls over a list or a MapRange (TKT-506: what
        // TirFusion makes of n.Range and xs.Take(k), and the Map pairs it could not compose by
        // substitution) is walked inside this loop: each Map's function is a stage applied to the
        // element, and a MapRange base makes the index itself the element. No intermediate array.
        var stages = new List<TirNode>();
        var indexSource = false;
        if (IndexSourceKinds.Contains(kind) && sources.Count == 1)
        {
            var src = TirRewrite.StripCoerce(sources[0]);
            while (src is TirCall sc && sc.Name == "Map" && sc.Args.Count == 2 && IsListTyped(sc.Args[0]) && FnOk(sc.Args[1], 1))
            {
                stages.Insert(0, sc.Args[1]);
                src = TirRewrite.StripCoerce(sc.Args[0]);
            }
            if (TirFusion.IsMapRange(src, out var range) && FnOk(range.Args[1], 1))
            {
                stages.Insert(0, range.Args[1]);
                src = range.Args[0];
                indexSource = true;
            }
            if (stages.Count > 0)
                sources = new List<TirNode> { src };
        }

        var id = NextId++;
        var temp = $"_lp{id}";
        hoists.Add(new TirLoweredLoop(kind, sources, fn, seed, includeFirst, temp, id, c.Type, c.Origin, c.Callee)
            { ElemType = elem, SourceStages = stages, IndexSource = indexSource });
        return new TirTempRef(temp, c.Type, c.Origin);
    }

    /// <summary>The single-source kinds whose receiver may be a fused producer chain (see
    /// <see cref="TirLoweredLoop.SourceStages"/>). MapIdx needs the source element AND its index,
    /// which the chain also has, so it is included.</summary>
    private static readonly HashSet<string> IndexSourceKinds = new HashSet<string>
    {
        "Map", "MapIdx", "Reduce", "All", "Any", "FlatMap",
    };

    /// <summary>Whether a lowered kind fills an array (as against a fold, All or Any).</summary>
    public static bool ProducesArray(string kind) => ArrayResultKinds.Contains(kind);

    private static readonly HashSet<string> ArrayResultKinds = new HashSet<string>
    {
        "Map", "MapIdx", "MapRange", "Zip2", "Zip3", "Reverse", "WithNext",
        "MapPairs", "MapTriplets", "MapQuartets",
    };

    /// <summary>Whether a type expression names a concrete, renderable C# type — not a solver
    /// type variable (name-mangled with '$', or flagged <see cref="TypeExpression.IsTypeVariable"/>).
    /// A `new {elem}[n]` allocation needs a real type. The test descends into the type arguments:
    /// <c>Located&lt;$T&gt;</c> has a renderable name but is no more a C# type than <c>$T</c> is
    /// (TKT-1070).</summary>
    public static bool IsRenderableConcrete(TypeExpression t)
    {
        var name = t?.Name;
        if (string.IsNullOrEmpty(name) || t.IsTypeVariable)
            return false;
        var c0 = name[0];
        if (!char.IsLetter(c0) && c0 != '_')
            return false;
        foreach (var arg in t.TypeArgs)
            if (!IsRenderableConcrete(arg))
                return false;
        return true;
    }

    /// <summary>The RETURN type of the function argument: the last type argument of its
    /// Function{N} type (lambda node type or delegate parameter's declared type).</summary>
    private static TypeExpression FnReturnType(TirNode fn)
    {
        fn = TirRewrite.StripCoerce(fn);
        var t = fn is TirLambda lam ? lam.Type
            : (fn as TirParameter)?.Def?.Type ?? (fn as TirVariable)?.Def?.Type;
        return t?.Name != null && t.Name.StartsWith("Function") && t.TypeArgs.Count > 0
            ? t.TypeArgs[t.TypeArgs.Count - 1]
            : null;
    }

    /// <summary>Whether the call produces a ONE-dimensional list with a resolvable element type
    /// (a concrete type or a type variable that renders as a C# generic parameter).</summary>
    private static bool ResultIsList(TirCall c)
        => TirRewrite.ListElementType(c.Type)?.Name != null;

}
