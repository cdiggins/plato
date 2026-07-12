using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Emission-only marker (--loops): a recognized array-combinator call site lowered to a
/// for-loop STATEMENT that fills <see cref="TempName"/>; the original call position holds a
/// <see cref="TirTempRef"/> to it. Printed by TirCSharpBodyWriter.WriteLoweredLoop.</summary>
public class TirLoweredLoop : TirNode
{
    public string Kind { get; }                    // Map, MapIdx, MapRange, Zip2, Zip3, Reduce, All, Any, Reverse, WithNext, MapPairs, MapTriplets, MapQuartets
    public IReadOnlyList<TirNode> Sources { get; } // list expressions (the count expression for MapRange)
    public TirNode Fn { get; }                     // lambda literal or delegate-typed parameter/variable (null for Reverse)
    public TirNode Seed { get; }                   // Reduce only
    public TirNode IncludeFirst { get; }           // WithNext only
    public string TempName { get; }
    public int Id { get; }
    // The RESULT element type, taken from the producing function's own type (the call's zonked
    // return type can be looser than the produced elements).
    public TypeExpression ElemType { get; set; }

    public TirLoweredLoop(string kind, IReadOnlyList<TirNode> sources, TirNode fn, TirNode seed,
        TirNode includeFirst, string tempName, int id, TypeExpression type, Symbol origin)
        : base(type, origin)
    {
        Kind = kind;
        Sources = sources ?? new List<TirNode>();
        Fn = fn;
        Seed = seed;
        IncludeFirst = includeFirst;
        TempName = tempName;
        Id = id;
    }

    public override IEnumerable<TirNode> Children
        => Sources.Concat(new[] { Fn, Seed, IncludeFirst }.Where(n => n != null));

    public override string ToString() => $"loop:{Kind} -> {TempName}";
}

/// <summary>Emission-only marker (--loops): a reference to a lowered loop's result local.</summary>
public class TirTempRef : TirNode
{
    public string Name { get; }
    public TirTempRef(string name, TypeExpression type, Symbol origin) : base(type, origin) => Name = name;
    public override string ToString() => Name;
}

/// <summary>
/// Loop lowering (--loops): rewrites recognized array-combinator call sites — the lazy
/// Ara3D.Collections adapters Map/MapEager/MapRange(Eager)/Zip/Reduce/All/Any/Reverse/WithNext/
/// MapPairs/MapTriplets/MapQuartets on ONE-dimensional list receivers — into for-loop statements
/// that fill a materialized array (or fold/short-circuit for Reduce/All/Any). Identical value
/// semantics for the pure language; the results are eager. Rules that keep it sound:
///
///   * only call sites OUTSIDE lambdas and conditionals are lowered (hoisting an expression out
///     of a lambda or a conditional branch would change how often it evaluates);
///   * the function argument must be a NON-eta lambda literal of the right arity (its body is
///     emitted inline in the loop with the lambda parameters as loop locals — legal wherever the
///     lambda itself was legal) or a delegate-typed parameter/variable reference (invoked);
///   * the receiver and result must be one-dimensional (IArray) with a renderable element type
///     (2D/3D maps carry their dimensions through and are left to the eager intrinsics);
///   * statement bodies lower per top-level statement; if/while statements are left intact.
///
/// Runs LAST (after the inliner, unroller and materializer), so it also lowers the MapEager
/// sites the materializer produced. TIR-path only.
/// </summary>
public static class TirLoopLowerer
{
    // Deterministic temp counter; reset per generation (CSharpWriter.WriteAll).
    public static int NextId;

    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.LowerLoops || tir?.Body == null)
            return tir;
        var body = tir.Body;
        TirNode newBody;
        if (body is TirBlock block)
        {
            var stmts = new List<TirNode>();
            var changed = false;
            foreach (var s in block.Statements)
            {
                var hoists = new List<TirNode>();
                var s2 = LowerStatement(s, hoists, writer);
                changed |= hoists.Count > 0 || !ReferenceEquals(s2, s);
                stmts.AddRange(hoists);
                stmts.Add(s2);
            }
            if (!changed)
                return tir;
            newBody = new TirBlock(stmts, block.Origin);
        }
        else
        {
            var hoists = new List<TirNode>();
            var e = Lower(body, hoists, writer);
            if (hoists.Count == 0)
                return tir;
            var stmts = new List<TirNode>(hoists) { new TirReturn(e, body.Origin) };
            newBody = new TirBlock(stmts, body.Origin);
        }
        return new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, newBody,
            tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static TirNode LowerStatement(TirNode s, List<TirNode> hoists, CSharpWriter writer)
    {
        switch (s)
        {
            case TirReturn r:
                return new TirReturn(Lower(r.Value, hoists, writer), r.Origin);
            case TirLet let:
                return new TirLet(let.Def, Lower(let.Value, hoists, writer), let.Type, let.Origin);
            case TirBlock or TirIf or TirLoop:
                return s; // control flow: left intact (v1)
            default:
                return Lower(s, hoists, writer);
        }
    }

    private static TirNode Lower(TirNode n, List<TirNode> hoists, CSharpWriter writer)
    {
        switch (n)
        {
            case null:
                return null;
            case TirLambda:
            case TirConditional:
                return n; // evaluation-count boundaries: not descended
            case TirCall c:
            {
                var args = c.Args.Select(a => Lower(a, hoists, writer)).ToList();
                var call = new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                    args, c.Type, c.Origin, c.Name);
                return TryLower(call, hoists) ?? call;
            }
            case TirCoerce co:
                return new TirCoerce(Lower(co.Inner, hoists, writer), co.FromType, co.ToType, co.ConversionFn, co.Origin);
            case TirInvoke inv:
                return new TirInvoke(Lower(inv.Target, hoists, writer),
                    inv.Args.Select(a => Lower(a, hoists, writer)).ToList(), inv.Type, inv.Origin);
            case TirNew nw:
                return new TirNew(nw.NewType, nw.Args.Select(a => Lower(a, hoists, writer)).ToList(), nw.Type, nw.Origin);
            case TirArray arr:
                return new TirArray(arr.Elements.Select(a => Lower(a, hoists, writer)).ToList(), arr.Type, arr.Origin);
            case TirConstructorCall cc:
                return new TirConstructorCall(cc.TypeName, cc.Args.Select(a => Lower(a, hoists, writer)).ToList());
            case TirBooleanChain bc:
                return new TirBooleanChain(bc.Op, bc.Terms.Select(t => Lower(t, hoists, writer)).ToList());
            case TirComponentAccess ca:
                return new TirComponentAccess(Lower(ca.Receiver, hoists, writer), ca.FieldName, ca.CastTo, ca.ScalarComponentPrim);
            default:
                return n; // leaves and anything else: unchanged
        }
    }

    /// <summary>The lowered marker + temp reference for a recognized combinator call, or null.</summary>
    private static TirNode TryLower(TirCall c, List<TirNode> hoists)
    {
        if (c.Name == null || c.Args.Count == 0)
            return null;

        string kind = null;
        List<TirNode> sources = null;
        TirNode fn = null, seed = null, includeFirst = null;

        bool IsListTyped(TirNode node)
            => StripCoerce(node)?.Type?.Name == "IArray";
        bool FnOk(TirNode f, int arity)
        {
            f = StripCoerce(f);
            if (f is TirLambda lam)
                return lam.Parameters.Count == arity
                       && !(lam.Parameters.Count > 0 && lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta")));
            // A delegate-typed reference is INVOKED with `arity` arguments, so its declared
            // Function{N} arity must match exactly.
            var declared = (f as TirParameter)?.Def?.Type?.Name ?? (f as TirVariable)?.Def?.Type?.Name;
            return (f is TirParameter || f is TirVariable) && declared == $"Function{arity}";
        }

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
        if (ArrayResultKinds.Contains(kind))
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
                c.Type?.Name == "IArray" && c.Type.TypeArgs.Count == 1 ? c.Type.TypeArgs[0] : null,
                kind == "Reverse" && StripCoerce(sources[0])?.Type is TypeExpression st
                    && st.Name == "IArray" && st.TypeArgs.Count == 1 ? st.TypeArgs[0] : null,
            };
            elem = candidates.FirstOrDefault(IsRenderableConcrete);
            if (elem == null)
                return null;
        }

        var id = NextId++;
        var temp = $"_lp{id}";
        hoists.Add(new TirLoweredLoop(kind, sources, fn, seed, includeFirst, temp, id, c.Type, c.Origin)
            { ElemType = elem });
        return new TirTempRef(temp, c.Type, c.Origin);
    }

    private static readonly HashSet<string> ArrayResultKinds = new HashSet<string>
    {
        "Map", "MapIdx", "MapRange", "Zip2", "Zip3", "Reverse", "WithNext",
        "MapPairs", "MapTriplets", "MapQuartets",
    };

    /// <summary>Whether a type expression names a concrete, renderable C# type — not a solver
    /// type variable (name-mangled with '$', or flagged <see cref="TypeExpression.IsTypeVariable"/>).
    /// A `new {elem}[n]` allocation needs a real type.</summary>
    private static bool IsRenderableConcrete(TypeExpression t)
    {
        var name = t?.Name;
        if (string.IsNullOrEmpty(name) || t.IsTypeVariable)
            return false;
        var c0 = name[0];
        return char.IsLetter(c0) || c0 == '_';
    }

    /// <summary>The RETURN type of the function argument: the last type argument of its
    /// Function{N} type (lambda node type or delegate parameter's declared type).</summary>
    private static TypeExpression FnReturnType(TirNode fn)
    {
        fn = StripCoerce(fn);
        var t = fn is TirLambda lam ? lam.Type
            : (fn as TirParameter)?.Def?.Type ?? (fn as TirVariable)?.Def?.Type;
        return t?.Name != null && t.Name.StartsWith("Function") && t.TypeArgs.Count > 0
            ? t.TypeArgs[t.TypeArgs.Count - 1]
            : null;
    }

    /// <summary>Whether the call produces a ONE-dimensional list with a resolvable element type
    /// (a concrete type or a type variable that renders as a C# generic parameter).</summary>
    private static bool ResultIsList(TirCall c)
        => c.Type?.Name == "IArray" && c.Type.TypeArgs.Count == 1 && c.Type.TypeArgs[0]?.Name != null;

    private static TirNode StripCoerce(TirNode n)
        => n is TirCoerce c ? StripCoerce(c.Inner) : n;
}
