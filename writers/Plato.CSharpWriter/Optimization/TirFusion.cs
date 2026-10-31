// Combines compatible array-combinator chains before loop emission.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Combinator fusion (--fuse, TKT-506): algebraic rewrites of array-combinator chains on
/// <c>Array&lt;T&gt;</c> so that one loop does the work of two or three, and literal-count
/// ranges become straight-line code (<see cref="TirLiteralUnroller"/>). Runs after the inliner
/// and before the loop lowerer, which turns the fused call into one <c>for</c> loop.
///
/// Every rewrite replaces an expression by an equal expression of the pure language, so it is
/// sound anywhere, inside lambdas and conditionals included (nothing is hoisted, nothing is
/// evaluated more or fewer times). The rules, each a named method below:
///
///   * <see cref="RangeToMapRange"/>:   <c>n.Range</c>            → <c>n.MapRange(i => i)</c>
///   * <see cref="TakeToMapRange"/>:    <c>xs.Take(k)</c>         → <c>k.MapRange(i => xs[i])</c>  (xs a parameter or variable)
///                                      <c>n.MapRange(g).Take(k)</c> → <c>k.MapRange(g)</c>
///   * <see cref="ComposeMaps"/>:       <c>xs.Map(g).Map(f)</c>   → <c>xs.Map(x => f(g(x)))</c>
///                                      <c>n.MapRange(g).Map(f)</c> → <c>n.MapRange(i => f(g(i)))</c>
///   * <see cref="FuseMapIntoConsumer"/>: <c>xs.Map(g).Reduce(s, f)</c> → <c>xs.Reduce(s, (acc, x) => f(acc, g(x)))</c>,
///                                      and the same for All, Any and FlatMap over a Map.
///
/// A composition substitutes the inner lambda's body for the outer lambda's parameter, so it is
/// only made when that parameter is used once (not under a nested lambda) or the body is a leaf
/// (<see cref="TirRewrite.IsCheap"/>); otherwise the chain is left for the lowerer, which
/// iterates a <c>MapRange</c> or <c>Map</c> source inside the consumer's loop with a local per
/// stage (no intermediate array either). A consumer over <c>MapRange</c> (<c>n.Range.Reduce</c>)
/// is therefore not rewritten here: the lowerer walks the index with no array.
///
/// <c>Take(xs, k)</c> is defined for k at most xs.Count; the fused form reads the same elements
/// inside that domain and differs only outside it (the original threw).
/// </summary>
public static class TirFusion
{
    /// <summary>Counter for synthesized parameter names (<c>_r{N}</c>, <c>{p}_f{N}</c>); reset
    /// per generation by CSharpWriter.WriteAll, per thread like the lowerer's.</summary>
    [System.ThreadStatic] public static int NextId;

    /// <summary>The literal count at or below which a MapRange, Range, All or Any is unrolled.</summary>
    public const int UnrollLimit = 8;

    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.FuseCombinators || tir?.Body == null)
            return tir;
        var ctx = new FusionContext(writer);
        var body = TirRewrite.Rewrite(tir.Body, n => n is TirCall c ? FuseCall(c, ctx) : n);
        return new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
            tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    /// <summary>Apply the rules to one call (whose children are already fused) until none fires.
    /// Each rule strictly shrinks the chain, so this terminates.</summary>
    private static TirNode FuseCall(TirCall c, FusionContext ctx)
    {
        TirNode node = c;
        for (var guard = 0; guard < 8 && node is TirCall call; guard++)
        {
            var next = Step(call, ctx);
            if (next == null)
                return node;
            node = next;
        }
        return node;
    }

    private static TirNode Step(TirCall c, FusionContext ctx)
    {
        if (c.Name == null)
            return null;
        switch (c.Name)
        {
            case "Range":
                return RangeToMapRange(c, ctx);
            case "Take":
                return TakeToMapRange(c, ctx);
            case "Map":
                return ComposeMaps(c, ctx) ?? TirLiteralUnroller.UnrollMapOverArray(c);
            case "MapRange" or "MapRangeEager":
                return TirLiteralUnroller.UnrollMapRange(c, ctx);
            case "Reduce":
                return FuseMapIntoConsumer(c, ctx, paramIndex: 1) ?? TirLiteralUnroller.UnrollReduceOverArray(c);
            case "All" or "Any":
                return FuseMapIntoConsumer(c, ctx, paramIndex: 0) ?? TirLiteralUnroller.UnrollAllAnyOverArray(c, ctx);
            case "FlatMap":
                return FuseMapIntoConsumer(c, ctx, paramIndex: 0);
            case "Filter":
                return FilterToFlatMap(c, ctx);
            default:
                return null;
        }
    }

    // ---- rules -----------------------------------------------------------------------------

    /// <summary><c>xs.Filter(keep)</c> → <c>xs.FlatMap(x => keep(x) ? [x] : [x].Take(0))</c>, the body
    /// stdlib/foundation/primitives.library.plato gives Filter. Only the stdlib Primitives.Filter, and
    /// only when <c>keep</c> is a lambda or a leaf, so the rewrite calls it exactly where the body would.</summary>
    public static TirNode FilterToFlatMap(TirCall c, FusionContext ctx)
    {
        if (c.Args.Count != 2 || !ctx.IsStandard(c.Callee, "Filter") || !TirRewrite.IsListType(c.Type)
            || !(TirRewrite.IsCheap(c.Args[1]) || TirRewrite.StripCoerce(c.Args[1]) is TirLambda))
            return null;
        var take = ctx.Standard("Take");
        var element = TirRewrite.ListElementType(c.Type);
        if (take == null || element == null || ctx.BooleanType == null)
            return null;
        var x = new ParameterDef(null, $"_f{TirFusion.NextId++}", element, 0);
        TirNode Ref() => new TirParameter(x, element, c.Origin);
        TirNode keep = new TirInvoke(c.Args[1], new[] { Ref() }, ctx.BooleanType, c.Origin);
        if (TirRewrite.StripCoerce(c.Args[1]) is TirLambda && keep is TirInvoke invoke
            && TirRewrite.TryBetaReduce(invoke, out var reduced))
            keep = reduced;
        var one = new TirArray(new[] { Ref() }, c.Type, c.Origin);
        var none = new TirCall(take, EmissionKind.InstanceMethod,
            new[] { c.Type, ctx.IntegerType }, c.Type, new TirNode[] { new TirArray(new[] { Ref() }, c.Type, c.Origin), ctx.Integer(0, c.Origin) }, c.Type, c.Origin);
        var body = new TirConditional(keep, one, none, c.Type, c.Origin);
        return ctx.Call("FlatMap", new[] { c.Args[0], ctx.Lambda(new[] { x }, body, c.Type) }, c.Type, c.Origin);
    }

    /// <summary><c>n.Range</c> → <c>n.MapRange(i => i)</c>: the canonical producer every consumer
    /// and the lowerer understand.</summary>
    public static TirNode RangeToMapRange(TirCall c, FusionContext ctx)
    {
        if (c.Args.Count != 1 || !IsInteger(c.Args[0]) || !TirRewrite.IsListType(c.Type))
            return null;
        var p = ctx.FreshIndexParam();
        var identity = ctx.Lambda(new[] { p }, new TirParameter(p, p.Type, c.Origin), ctx.IntegerType);
        return ctx.MapRange(c.Args[0], identity, c.Type, c.Origin);
    }

    /// <summary><c>xs.Take(k)</c> → <c>k.MapRange(i => xs[i])</c> when xs is a leaf (a parameter or
    /// variable, so repeating it costs nothing); <c>n.MapRange(g).Take(k)</c> → <c>k.MapRange(g)</c>.</summary>
    public static TirNode TakeToMapRange(TirCall c, FusionContext ctx)
    {
        if (c.Args.Count != 2 || !IsInteger(c.Args[1]) || !TirRewrite.IsListType(c.Type))
            return null;
        var xs = TirRewrite.StripCoerce(c.Args[0]);
        if (IsMapRange(xs, out var mr))
            return ctx.MapRange(c.Args[1], mr.Args[1], c.Type, c.Origin);
        if (!TirRewrite.IsCheap(xs) || !TirRewrite.IsListType(xs.Type))
            return null;
        var p = ctx.FreshIndexParam();
        var at = ctx.At(xs, new TirParameter(p, p.Type, c.Origin), c.Origin);
        return ctx.MapRange(c.Args[1], ctx.Lambda(new[] { p }, at, at.Type), c.Type, c.Origin);
    }

    /// <summary><c>xs.Map(g).Map(f)</c> → <c>xs.Map(x => f(g(x)))</c>;
    /// <c>n.MapRange(g).Map(f)</c> → <c>n.MapRange(i => f(g(i)))</c>.</summary>
    public static TirNode ComposeMaps(TirCall c, FusionContext ctx)
    {
        if (c.Args.Count != 2 || !TirRewrite.IsListType(c.Type))
            return null;
        var inner = TirRewrite.StripCoerce(c.Args[0]);
        if (!(inner is TirCall ic) || ic.Args.Count != 2 || !(ic.Name == "Map" || ic.Name == "MapRange"))
            return null;
        var composed = Compose(c.Args[1], 0, ic.Args[1], ctx, c.Type.TypeArgs[0]);
        if (composed == null)
            return null;
        return ic.Name == "MapRange"
            ? ctx.MapRange(ic.Args[0], composed, c.Type, c.Origin)
            : ctx.Call("Map", new[] { ic.Args[0], composed }, c.Type, c.Origin);
    }

    /// <summary><c>xs.Map(g).C(..., f)</c> → <c>xs.C(..., f')</c> where f' feeds g's result to the
    /// parameter of f at <paramref name="paramIndex"/>: Reduce (the element is f's second
    /// parameter), All, Any and FlatMap (the first).</summary>
    public static TirNode FuseMapIntoConsumer(TirCall c, FusionContext ctx, int paramIndex)
    {
        if (c.Args.Count < 2)
            return null;
        var inner = TirRewrite.StripCoerce(c.Args[0]);
        if (!(inner is TirCall ic) || ic.Name != "Map" || ic.Args.Count != 2)
            return null;
        var f = c.Args[c.Args.Count - 1];
        var composed = Compose(f, paramIndex, ic.Args[1], ctx, resultType: null);
        if (composed == null)
            return null;
        var args = c.Args.ToList();
        args[0] = ic.Args[0];
        args[args.Count - 1] = composed;
        return ctx.Call(c.Name, args, c.Type, c.Origin);
    }

    /// <summary>The lambda <c>(…, x, …) => f(…, g(x), …)</c>: f's body with its parameter number
    /// <paramref name="paramIndex"/> replaced by g's body, over g's parameter. Null unless both are
    /// inlinable lambda literals (g of arity 1) and the substitution evaluates g's body once.</summary>
    private static TirLambda Compose(TirNode f, int paramIndex, TirNode g, FusionContext ctx, TypeExpression resultType)
    {
        var fLam = Lambda(f, -1);
        var gLam = Lambda(g, 1);
        if (fLam == null || gLam == null || fLam.Parameters.Count <= paramIndex)
            return null;
        var target = fLam.Parameters[paramIndex];
        if (!CanSubstitute(fLam.Body, target, gLam.Body))
            return null;

        // g's parameter is bound around f's body now; a lambda inside f's body binding the same
        // name would shadow it in the printed C# (CS0136) or capture it. Rename g's parameter.
        var gParam = gLam.Parameters[0];
        var gBody = gLam.Body;
        if (BindsName(fLam.Body, gParam.Name) || fLam.Parameters.Any(p => p != target && p.Name == gParam.Name))
        {
            var renamed = new ParameterDef(gParam.Scope, $"{gParam.Name}_f{NextId++}", gParam.Type, gParam.Index);
            gBody = TirRewrite.Substitute(gBody, new Dictionary<ParameterDef, TirNode>
                { [gParam] = new TirParameter(renamed, renamed.Type, gLam.Origin) });
            gParam = renamed;
        }
        var body = TirRewrite.Substitute(fLam.Body, new Dictionary<ParameterDef, TirNode> { [target] = gBody });
        var parameters = fLam.Parameters.Select(p => p == target ? gParam : p).ToList();
        var fType = fLam.Type;
        var result = resultType ?? (fType != null && fType.TypeArgs.Count > 0 ? fType.TypeArgs[fType.TypeArgs.Count - 1] : null);
        return ctx.Lambda(parameters, body, result);
    }

    // ---- shared predicates -------------------------------------------------------------------

    /// <summary>The function argument as an inlinable lambda literal: an expression body, not an
    /// eta-forwarding lambda, of the given arity (any arity when <paramref name="arity"/> is -1).
    /// Null otherwise. The same discipline as the lowerer's.</summary>
    public static TirLambda Lambda(TirNode f, int arity)
    {
        if (!(TirRewrite.StripCoerce(f) is TirLambda lam))
            return null;
        if (arity >= 0 && lam.Parameters.Count != arity)
            return null;
        if (lam.Body == null || TirRewrite.IsStatementNode(lam.Body))
            return null;
        if (lam.Parameters.Count > 0 && lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta")))
            return null;
        return lam;
    }

    /// <summary>Whether replacing <paramref name="p"/> in <paramref name="body"/> by
    /// <paramref name="replacement"/> evaluates it as often as the lambda would have: a leaf may
    /// repeat, anything else needs one use outside a nested lambda.</summary>
    public static bool CanSubstitute(TirNode body, ParameterDef p, TirNode replacement)
    {
        if (TirRewrite.IsCheap(replacement))
            return true;
        var uses = TirRewrite.CountParamUses(body);
        return !uses.TryGetValue(p, out var u) || (u.Count <= 1 && !u.UnderLambda);
    }

    public static bool IsMapRange(TirNode n, out TirCall call)
    {
        call = TirRewrite.StripCoerce(n) as TirCall;
        return call != null && (call.Name == "MapRange" || call.Name == "MapRangeEager") && call.Args.Count == 2;
    }

    public static bool IsInteger(TirNode n)
        => TirRewrite.StripCoerce(n)?.Type?.Name == "Integer";

    /// <summary>The value of an integer literal of at most <see cref="UnrollLimit"/>, or -1.</summary>
    public static int SmallLiteral(TirNode n)
    {
        if (!(TirRewrite.StripCoerce(n) is TirLiteral lit) || lit.LiteralType != LiteralTypesEnum.Integer)
            return -1;
        var v = lit.Value is long l ? l : lit.Value is int i ? i : -1L;
        return v >= 0 && v <= UnrollLimit ? (int)v : -1;
    }

    private static bool BindsName(TirNode body, string name)
        => name != null && body.Descendants().OfType<TirLambda>().Any(l => l.Parameters.Any(p => p.Name == name));
}

/// <summary>The node factory the fusion rules share: the Integer and Function types of the
/// program being written, and synthesized calls (<c>MapRange</c>, <c>At</c>) that carry a name and
/// no callee, which the body writer prints by name like every intrinsic.</summary>
public sealed class FusionContext
{
    private readonly CSharpWriter _writer;
    public TypeExpression IntegerType { get; }
    public TypeExpression BooleanType { get; }

    public FusionContext(CSharpWriter writer)
    {
        _writer = writer;
        IntegerType = writer.Compilation.GetTypeDef("Integer")?.ToTypeExpression();
        BooleanType = writer.Compilation.GetTypeDef("Boolean")?.ToTypeExpression();
    }

    public ParameterDef FreshIndexParam()
        => new ParameterDef(null, $"_r{TirFusion.NextId++}", IntegerType, 0);

    /// <summary>A lambda over <paramref name="parameters"/> typed <c>Function{N}&lt;P..., R&gt;</c>
    /// (null type when the program declares no such Function type).</summary>
    public TirLambda Lambda(IReadOnlyList<ParameterDef> parameters, TirNode body, TypeExpression result)
    {
        var def = _writer.Compilation.GetTypeDef($"Function{parameters.Count}");
        var argTypes = parameters.Select(p => p.Type).Append(result).ToArray();
        var type = def != null && argTypes.All(t => t != null) ? new TypeExpression(def, argTypes) : null;
        return new TirLambda(parameters, body, type, body?.Origin);
    }

    public TirCall MapRange(TirNode count, TirNode fn, TypeExpression type, Symbol origin)
        => Call("MapRange", new[] { count, fn }, type, origin);

    public TirCall At(TirNode xs, TirNode index, Symbol origin)
    {
        var elem = TirRewrite.ListElementType(xs.Type);
        return Call("At", new[] { xs, index }, elem, origin);
    }

    public TirCall Call(string name, IReadOnlyList<TirNode> args, TypeExpression type, Symbol origin)
        => new TirCall(null, EmissionKind.InstanceMethod, args.Select(a => a?.Type).ToList(), type, args, type, origin, name);

    public TirLiteral Integer(long value, Symbol origin)
        => new TirLiteral(value, LiteralTypesEnum.Integer, IntegerType, origin);

    public FunctionDef Standard(string name)
        => _writer.Compilation.LibraryDefinitionsByName.TryGetValue("Primitives", out var library)
            ? library.Functions.FirstOrDefault(f => f.Name == name && f.Body != null) : null;

    public bool IsStandard(FunctionDef function, string name)
        => function != null && ReferenceEquals(function, Standard(name));

    public TirLiteral Boolean(bool value, Symbol origin)
        => new TirLiteral(value, LiteralTypesEnum.Boolean, BooleanType, origin);
}
