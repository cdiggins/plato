// Expands bounded literal arrays and ranges into straight-line typed-IR expressions.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Literal-count unrolling (--fuse, TKT-506): a MapRange whose count is an integer literal of at
/// most <see cref="TirFusion.UnrollLimit"/> becomes an array literal of its elements, and a Map,
/// All, Any or Reduce over such an array literal becomes straight-line code:
///
///   * <see cref="UnrollMapRange"/>:       <c>3.MapRange(f)</c>  → <c>[f(0), f(1), f(2)]</c>  (so <c>3.Range</c> → <c>[0, 1, 2]</c>)
///   * <see cref="UnrollMapOverArray"/>:   <c>[a, b].Map(f)</c>  → <c>[f(a), f(b)]</c>
///   * <see cref="UnrollAllAnyOverArray"/>: <c>[a, b, c].Any(p)</c> → <c>p(a) || p(b) || p(c)</c>, All with <c>&amp;&amp;</c>
///   * <see cref="UnrollReduceOverArray"/>: <c>[a, b].Reduce(s, f)</c> → <c>f(f(s, a), b)</c>
///
/// A lambda is applied by substituting the argument for its parameter, so an argument that is
/// not a leaf is substituted only where the parameter is used once (<see cref="TirFusion.CanSubstitute"/>);
/// a literal index always is. A delegate-typed function is invoked. Like the fusion rules, these
/// are pure rewrites and apply anywhere, inside lambdas included, which is where the lowerer does
/// not go: <c>3.Range.Any(j => ...)</c> per tested triangle in the STEP mesher's refinement test
/// was the motivating site.
/// </summary>
public static class TirLiteralUnroller
{
    public static TirNode UnrollMapRange(TirCall c, FusionContext ctx)
    {
        if (c.Args.Count != 2 || !TirRewrite.IsListType(c.Type))
            return null;
        var k = TirFusion.SmallLiteral(c.Args[0]);
        if (k < 0)
            return null;
        var elem = TirRewrite.ListElementType(c.Type);
        var elements = new List<TirNode>();
        for (var j = 0; j < k; j++)
        {
            var applied = Apply(c.Args[1], ctx.Integer(j, c.Origin), elem, c.Origin);
            if (applied == null)
                return null;
            elements.Add(applied);
        }
        return new TirArray(elements, c.Type, c.Origin);
    }

    public static TirNode UnrollMapOverArray(TirCall c)
    {
        if (c.Args.Count != 2 || !TirRewrite.IsListType(c.Type) || !SmallArray(c.Args[0], out var arr))
            return null;
        var elem = TirRewrite.ListElementType(c.Type);
        var elements = new List<TirNode>();
        foreach (var e in arr.Elements)
        {
            var applied = Apply(c.Args[1], e, elem, c.Origin);
            if (applied == null)
                return null;
            elements.Add(applied);
        }
        return new TirArray(elements, c.Type, c.Origin);
    }

    public static TirNode UnrollAllAnyOverArray(TirCall c, FusionContext ctx)
    {
        if (c.Args.Count != 2 || !SmallArray(c.Args[0], out var arr))
            return null;
        var isAll = c.Name == "All";
        if (arr.Elements.Count == 0)
            return ctx.Boolean(isAll, c.Origin);
        var terms = new List<TirNode>();
        foreach (var e in arr.Elements)
        {
            var applied = Apply(c.Args[1], e, c.Type, c.Origin);
            if (applied == null)
                return null;
            terms.Add(applied);
        }
        return terms.Count == 1 ? terms[0] : new TirBooleanChain(isAll ? "&&" : "||", terms, c.Type);
    }

    public static TirNode UnrollReduceOverArray(TirCall c)
    {
        if (c.Args.Count != 3 || !SmallArray(c.Args[0], out var arr))
            return null;
        var f = TirFusion.Lambda(c.Args[2], 2);
        if (f == null)
            return null;
        var acc = c.Args[1];
        foreach (var e in arr.Elements)
        {
            if (!TirFusion.CanSubstitute(f.Body, f.Parameters[0], acc) || !TirFusion.CanSubstitute(f.Body, f.Parameters[1], e))
                return null;
            acc = TirRewrite.Substitute(f.Body, new Dictionary<ParameterDef, TirNode>
                { [f.Parameters[0]] = acc, [f.Parameters[1]] = e });
        }
        return acc;
    }

    /// <summary>f applied to one argument: the lambda body with the argument substituted, or an
    /// invocation of a delegate-typed reference. Null when neither applies or the substitution
    /// would repeat a non-leaf argument.</summary>
    private static TirNode Apply(TirNode f, TirNode arg, TypeExpression resultType, Symbol origin)
    {
        var lam = TirFusion.Lambda(f, 1);
        if (lam != null)
            return TirFusion.CanSubstitute(lam.Body, lam.Parameters[0], arg)
                ? TirRewrite.Substitute(lam.Body, new Dictionary<ParameterDef, TirNode> { [lam.Parameters[0]] = arg })
                : null;
        var g = TirRewrite.StripCoerce(f);
        var declared = (g as TirParameter)?.Def?.Type?.Name ?? (g as TirVariable)?.Def?.Type?.Name;
        return declared == "Function1" ? new TirInvoke(g, new[] { arg }, resultType, origin) : null;
    }

    private static bool SmallArray(TirNode n, out TirArray arr)
    {
        arr = TirRewrite.StripCoerce(n) as TirArray;
        return arr != null && arr.Elements.Count <= TirFusion.UnrollLimit;
    }
}
