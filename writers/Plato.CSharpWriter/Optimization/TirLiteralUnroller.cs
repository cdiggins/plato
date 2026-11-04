// Expands bounded literal arrays and ranges into straight-line typed-IR expressions.
using System;
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
///
/// The overloads that take an <see cref="UnrollPolicy"/> (--scalar-arrays, <see cref="ConstantCounts"/>)
/// widen two limits and add a Zip rule: an element that is a component read may be repeated, a
/// delegate-typed combining function of a Reduce is invoked, and the accumulator of a Reduce may
/// repeat up to a node budget. Without a policy every rule is exactly the one above.
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

    public static TirNode UnrollMapOverArray(TirCall c, UnrollPolicy policy = null)
    {
        if (c.Args.Count != 2 || !TirRewrite.IsListType(c.Type) || !SmallArray(c.Args[0], out var arr))
            return null;
        var elem = TirRewrite.ListElementType(c.Type);
        var elements = new List<TirNode>();
        foreach (var e in arr.Elements)
        {
            var applied = Apply(c.Args[1], e, elem, c.Origin, policy);
            if (applied == null)
                return null;
            elements.Add(applied);
        }
        return new TirArray(elements, c.Type, c.Origin);
    }

    /// <summary><c>[a, b].Zip([x, y], f)</c> → <c>[f(a, x), f(b, y)]</c> (two or three sources, the
    /// shortest decides the count). Only under a policy: without --scalar-arrays Zip stays a loop.</summary>
    public static TirNode UnrollZipOverArray(TirCall c, UnrollPolicy policy)
    {
        var sourceCount = c.Args.Count - 1;
        if (policy == null || sourceCount < 2 || sourceCount > 3 || !TirRewrite.IsListType(c.Type))
            return null;
        var sources = new List<TirArray>();
        for (var i = 0; i < sourceCount; i++)
            if (SmallArray(c.Args[i], out var arr))
                sources.Add(arr);
            else
                return null;
        var elem = TirRewrite.ListElementType(c.Type);
        var count = sources.Min(a => a.Elements.Count);
        var elements = new List<TirNode>();
        for (var j = 0; j < count; j++)
        {
            var applied = ApplyAll(c.Args[sourceCount], sources.Select(a => a.Elements[j]).ToList(), elem, c.Origin, policy);
            if (applied == null)
                return null;
            elements.Add(applied);
        }
        return new TirArray(elements, c.Type, c.Origin);
    }

    public static TirNode UnrollAllAnyOverArray(TirCall c, FusionContext ctx, UnrollPolicy policy = null)
    {
        if (c.Args.Count != 2 || !SmallArray(c.Args[0], out var arr))
            return null;
        var isAll = c.Name == "All";
        if (arr.Elements.Count == 0)
            return ctx.Boolean(isAll, c.Origin);
        var terms = new List<TirNode>();
        foreach (var e in arr.Elements)
        {
            var applied = Apply(c.Args[1], e, c.Type, c.Origin, policy);
            if (applied == null)
                return null;
            terms.Add(applied);
        }
        return terms.Count == 1 ? terms[0] : new TirBooleanChain(isAll ? "&&" : "||", terms, c.Type);
    }

    public static TirNode UnrollReduceOverArray(TirCall c, UnrollPolicy policy = null)
    {
        if (c.Args.Count != 3 || !SmallArray(c.Args[0], out var arr))
            return null;
        var f = TirFusion.Lambda(c.Args[2], 2);
        if (f == null)
            return policy == null ? null : FoldDelegate(c, arr);
        var acc = c.Args[1];
        foreach (var e in arr.Elements)
        {
            // Under a policy the accumulator may be read twice: its growth is bounded by the budget below.
            if (policy == null && !TirFusion.CanSubstitute(f.Body, f.Parameters[0], acc))
                return null;
            if (!CanSubstitute(policy, f.Body, f.Parameters[1], e))
                return null;
            acc = TirRewrite.Substitute(f.Body, new Dictionary<ParameterDef, TirNode>
                { [f.Parameters[0]] = acc, [f.Parameters[1]] = e });
            if (policy != null && !policy.WithinBudget(acc))
                return null;
        }
        return acc;
    }

    /// <summary>A Reduce whose combining function is a delegate-typed reference: <c>f(f(init, a), b)</c>.
    /// The accumulator is an argument of one invocation, so nothing repeats.</summary>
    private static TirNode FoldDelegate(TirCall c, TirArray arr)
    {
        var g = TirRewrite.StripCoerce(c.Args[2]);
        if (DeclaredFunctionName(g) != "Function2")
            return null;
        var acc = c.Args[1];
        foreach (var e in arr.Elements)
            acc = new TirInvoke(g, new[] { acc, e }, c.Type, c.Origin);
        return acc;
    }

    /// <summary>f applied to one argument: the lambda body with the argument substituted, or an
    /// invocation of a delegate-typed reference. Null when neither applies or the substitution
    /// would repeat a non-leaf argument.</summary>
    private static TirNode Apply(TirNode f, TirNode arg, TypeExpression resultType, Symbol origin, UnrollPolicy policy = null)
        => ApplyAll(f, new[] { arg }, resultType, origin, policy);

    /// <summary>f applied to its arguments (one per parameter): the lambda body with each
    /// substituted, or an invocation of a delegate-typed reference.</summary>
    private static TirNode ApplyAll(TirNode f, IReadOnlyList<TirNode> args, TypeExpression resultType, Symbol origin, UnrollPolicy policy)
    {
        var lam = TirFusion.Lambda(f, args.Count);
        if (lam != null)
        {
            var map = new Dictionary<ParameterDef, TirNode>();
            for (var i = 0; i < args.Count; i++)
            {
                if (!CanSubstitute(policy, lam.Body, lam.Parameters[i], args[i]))
                    return null;
                map[lam.Parameters[i]] = args[i];
            }
            return TirRewrite.Substitute(lam.Body, map);
        }
        var g = TirRewrite.StripCoerce(f);
        return DeclaredFunctionName(g) == $"Function{args.Count}" ? new TirInvoke(g, args, resultType, origin) : null;
    }

    private static string DeclaredFunctionName(TirNode g)
        => (g as TirParameter)?.Def?.Type?.Name ?? (g as TirVariable)?.Def?.Type?.Name;

    private static bool CanSubstitute(UnrollPolicy policy, TirNode body, ParameterDef p, TirNode arg)
        => policy != null && policy.Repeatable(arg) || TirFusion.CanSubstitute(body, p, arg);

    private static bool SmallArray(TirNode n, out TirArray arr)
    {
        arr = TirRewrite.StripCoerce(n) as TirArray;
        return arr != null && arr.Elements.Count <= TirFusion.UnrollLimit;
    }
}

/// <summary>What the policy-taking unroll rules allow beyond <see cref="TirFusion.CanSubstitute"/>
/// (--scalar-arrays): <see cref="Repeatable"/> names the argument nodes whose repetition costs nothing
/// (a component read of a leaf), and <see cref="AccumulatorBudget"/> bounds the node count of a Reduce
/// accumulator that a combining function reads twice (a Min or Max step), so a fold over an array of
/// at most <see cref="TirFusion.UnrollLimit"/> elements grows by a bounded amount and never exponentially.</summary>
public sealed record UnrollPolicy(Func<TirNode, bool> Repeatable, int AccumulatorBudget)
{
    public bool WithinBudget(TirNode accumulator) => accumulator.Descendants().Count() + 1 <= AccumulatorBudget;
}
