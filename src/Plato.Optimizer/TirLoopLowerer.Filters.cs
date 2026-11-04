// The filter idiom as a Filter loop (--filter-lowering, TKT-1186): FlatMap(x => cond ? [x] : []) in one pass.
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
namespace Ara3D.Geometry.Optimizer;

public static partial class TirLoopLowerer
{
    /// <summary>
    /// The predicate lambda when <paramref name="fn"/> is <c>x => cond ? [x] : none</c> with <c>none</c> an empty array,
    /// <c>[x].Take(0)</c> or <c>0.MapRange(..)</c> (the shapes the library's own idiom and <c>Filter</c>'s fusion take).
    /// A FlatMap of that shape keeps the elements for which <c>cond</c> holds and evaluates <c>cond</c> once per element,
    /// in order, exactly as a Filter does, but allocates a one-element array per element on the way; as a Filter loop it
    /// fills one presized buffer. Null when the lambda is any other shape.
    /// </summary>
    private static TirLambda FilterPredicate(TirNode fn, TirCall flatMap)
    {
        if (TirRewrite.StripCoerce(fn) is not TirLambda { Parameters.Count: 1 } lam
            || lam.Parameters[0].Name != null && lam.Parameters[0].Name.StartsWith("_eta")
            || TirRewrite.StripCoerce(lam.Body) is not TirConditional cond
            || !IsSingletonOf(cond.IfTrue, lam.Parameters[0]) || !IsNone(cond.IfFalse, lam.Parameters[0]))
            return null;
        // The kept elements are the source's, so the result repeats the parameter's type. The instantiated lambda type is
        // authoritative: a ParameterDef can retain solver variables (TirExactOutput does the same).
        var element = TirRewrite.ListElementType(flatMap.Type);
        if (element == null || lam.Type is not { TypeArgs.Count: 2 } function || !element.Equals(function.TypeArgs[0]))
            return null;
        var boolean = TirRewrite.StripCoerce(cond.Condition)?.Type;
        if (boolean?.Name != "Boolean")
            return null;
        var type = new TypeExpression(function.Def, function.TypeArgs[0], boolean);
        return new TirLambda(lam.Parameters, cond.Condition, type, lam.Origin);
    }

    private static bool IsParameter(TirNode n, ParameterDef p)
        => TirRewrite.StripCoerce(n) is TirParameter r && ReferenceEquals(r.Def, p);

    private static bool IsSingletonOf(TirNode n, ParameterDef p)
        => TirRewrite.StripCoerce(n) is TirArray { Elements.Count: 1 } a && IsParameter(a.Elements[0], p);

    private static bool IsZero(TirNode n)
        => TirRewrite.StripCoerce(n) is TirLiteral { LiteralType: LiteralTypesEnum.Integer } l && l.Value is long v && v == 0;

    private static bool IsNone(TirNode n, ParameterDef p)
    {
        n = TirRewrite.StripCoerce(n);
        return n is TirArray { Elements.Count: 0 }
            || n is TirCall { Name: "Take", Args.Count: 2 } take && IsSingletonOf(take.Args[0], p) && IsZero(take.Args[1])
            || n is TirCall { Name: "MapRange" or "MapRangeEager", Args.Count: 2 } range && IsZero(range.Args[0]);
    }
}
