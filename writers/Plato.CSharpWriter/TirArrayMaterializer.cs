using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Pass 3 of the C# optimizer (--optimize-arrays). Consumes a TIR body; produces the same body with
/// the NAME of two-argument <c>Map</c>/<c>MapRange</c> calls in a materialization position changed to
/// <c>MapEager</c>/<c>MapRangeEager</c>. Nothing else changes. Order and flags of all passes:
/// docs/csharp-optimizer-pipeline.md (written once, in CSharpWriter.RunOptimizerPasses).
///
/// Positions: a direct argument of a constructor (<see cref="TirNew"/>) or of a <c>TupleN</c> call
/// (a tuple building a struct), looking through <see cref="TirCoerce"/>; the value of a local binding
/// (<see cref="TirLet"/>) referenced more than once or under a lambda or loop.
///
/// STALE AS OF 2026-10-23 (TKT-508): this pass was written (2026-07-10) when <c>Map</c>/<c>MapRange</c>
/// returned lazy views that re-ran the function on every read. Since TKT-350 <c>MapRange</c> is eager
/// (ArrayIntrinsics.MapRange) and <c>Map</c> is a Plato body over it, so the rename no longer changes
/// behaviour. <c>MapEager</c>/<c>MapRangeEager</c> exist only under src/Plato.Intrinsics/bonepile, which
/// the compiled runtime does not include; <see cref="TirLoopLowerer"/> accepts the names and lowers them
/// to loops, so the pass is consistent only together with --loops. TIR path only.
/// </summary>
public static class TirArrayMaterializer
{
    /// <summary>The function with materialization-position Map/MapRange calls made eager, or the
    /// function unchanged (also when the flag is off).</summary>
    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.OptimizeArrays || tir?.Body == null)
            return tir;
        var body = RewriteBody(tir.Body);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static TirNode RewriteBody(TirNode body)
    {
        // Multi-consumed locals: definition -> eligible, computed over the whole body up front.
        var refCounts = new Dictionary<VariableDef, int>();
        var refsUnderLambdaOrLoop = new HashSet<VariableDef>();
        CountRefs(body, false, refCounts, refsUnderLambdaOrLoop);

        var changed = false;

        TirNode Rw(TirNode n, bool materialize)
        {
            switch (n)
            {
                case null:
                    return null;

                case TirCall c:
                {
                    var args = c.Args.Select(a => Rw(a, IsMaterializingCallee(c))).ToList();
                    if (materialize && TryEagerName(c, out var eager))
                    {
                        changed = true;
                        return new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                            args, c.Type, c.Origin, eager);
                    }
                    return new TirCall(c.Callee, c.EmissionKind, c.ParameterTypes, c.ReturnType,
                        args, c.Type, c.Origin, c.Name);
                }

                case TirNew nw:
                    return new TirNew(nw.NewType, nw.Args.Select(a => Rw(a, true)).ToList(), nw.Type, nw.Origin);

                case TirCoerce co:
                    // A coercion is transparent for position: the coerced value lands wherever
                    // the coercion does (tuple->struct wraps constructor arguments this way).
                    return new TirCoerce(Rw(co.Inner, materialize), co.FromType, co.ToType, co.ConversionFn, co.Origin);

                case TirLet let:
                {
                    var eligible = let.Def != null
                        && (refCounts.TryGetValue(let.Def, out var cnt) && cnt > 1
                            || refsUnderLambdaOrLoop.Contains(let.Def));
                    return new TirLet(let.Def, Rw(let.Value, eligible), let.Type, let.Origin);
                }

                case TirInvoke inv:
                    return new TirInvoke(Rw(inv.Target, false), inv.Args.Select(a => Rw(a, false)).ToList(), inv.Type, inv.Origin);
                case TirConditional cond:
                    return new TirConditional(Rw(cond.Condition, false), Rw(cond.IfTrue, materialize),
                        Rw(cond.IfFalse, materialize), cond.Type, cond.Origin);
                case TirArray arr:
                    return new TirArray(arr.Elements.Select(a => Rw(a, false)).ToList(), arr.Type, arr.Origin);
                case TirAssign asg:
                    return new TirAssign(Rw(asg.LValue, false), Rw(asg.RValue, false), asg.Type, asg.Origin);
                case TirLambda lam:
                    return new TirLambda(lam.Parameters, Rw(lam.Body, false), lam.Type, lam.Origin);
                case TirBlock b:
                    return new TirBlock(b.Statements.Select(s => Rw(s, false)).ToList(), b.Origin);
                case TirReturn r:
                    return new TirReturn(Rw(r.Value, false), r.Origin);
                case TirIf iff:
                    return new TirIf(Rw(iff.Condition, false), Rw(iff.IfTrue, false), Rw(iff.IfFalse, false), iff.Origin);
                case TirLoop l:
                    return new TirLoop(Rw(l.Condition, false), Rw(l.Body, false), l.Origin);
                default:
                    return n; // leaves
            }
        }

        var result = Rw(body, false);
        return changed ? result : body;
    }

    /// <summary>Arguments of a TupleN call are materialization positions (tuples build struct
    /// values in this library); other callees' arguments are not.</summary>
    private static bool IsMaterializingCallee(TirCall c)
        => c.Name != null && c.Name.StartsWith("Tuple");

    private static bool TryEagerName(TirCall c, out string eager)
    {
        eager = null;
        if (c.Args.Count != 2)
            return false;
        if (c.Name == "Map")
            eager = "MapEager";
        else if (c.Name == "MapRange")
            eager = "MapRangeEager";
        return eager != null;
    }

    private static void CountRefs(TirNode n, bool underLambdaOrLoop,
        Dictionary<VariableDef, int> counts, HashSet<VariableDef> underSet)
    {
        if (n == null)
            return;
        if (n is TirVariable v && v.Def != null)
        {
            counts[v.Def] = counts.TryGetValue(v.Def, out var c) ? c + 1 : 1;
            if (underLambdaOrLoop)
                underSet.Add(v.Def);
        }
        var inner = underLambdaOrLoop || n is TirLambda || n is TirLoop;
        foreach (var child in n.Children)
            CountRefs(child, inner, counts, underSet);
    }
}
