// States how many elements an array-producing node or a loop count expression yields, when the body says (TKT-1152 C5).
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The answer of <see cref="CountBounds"/>: a <see cref="Constant"/> count, or a guarded
/// <see cref="Expression"/> that computes the count and can be tested against a capacity at run
/// time, or neither (unknown). A constant is the count the node yields; a count of zero or less
/// yields an empty array and the consumer clamps.
/// </summary>
public sealed record CountBound(int? Constant, TirNode Expression)
{
    public static readonly CountBound Unknown = new(null, null);
    public bool IsConstant => Constant != null;
    public bool IsGuarded => Constant == null && Expression != null;
    public bool IsUnknown => Constant == null && Expression == null;
}

/// <summary>
/// A pure analysis that answers how large an array is, so that a later pass can decide whether it
/// fits a fixed-capacity slot (constant: decided now; guarded: tested at run time with a heap
/// fallback; unknown: heap).
///
/// A count is a constant when it is an integer literal, the length of a <see cref="TirArray"/>
/// literal, or <c>Add</c>, <c>Subtract</c> or <c>Multiply</c> of constants. Literal arithmetic is
/// checked in 32 bits: an intermediate or final value outside the <c>int</c> range makes the
/// count unknown, because the generated code would wrap and the folded value would be wrong.
///
/// A count is guarded when it is built only from total integer operations (<c>Add</c>,
/// <c>Subtract</c>, <c>Multiply</c>, and <c>Count</c> of a non-builder; no division, so that
/// evaluating the guard cannot throw) over literals, parameters and variables that nothing assigns
/// within <c>scope</c>. This is <c>TirPresize</c>'s stability rule (its <c>IsStable</c> is private
/// and tied to its loop shapes, so the rule is restated here over a scope). A guarded expression
/// may wrap on overflow, but the loop it guards computes the same wrapped count, so guard and loop
/// agree. Anything else (a call with effects, an unknown function, a builder's count) is unknown.
/// </summary>
public static class CountBounds
{
    /// <summary>The element count of an array-valued node: a <see cref="TirArray"/> literal, or a
    /// lowered loop that produces an array. Other nodes are unknown. <paramref name="scope"/> is the
    /// region in which a guarded expression must stay unchanged (null: nothing is assigned).</summary>
    public static CountBound OfArray(TirNode array, TirNode scope = null)
        => array switch
        {
            TirArray literal => new CountBound(literal.Elements.Count, null),
            TirLoweredLoop loop => OfLoop(loop, scope),
            _ => CountBound.Unknown,
        };

    /// <summary>The element count a lowered loop produces. <c>MapRange</c> and a loop with an
    /// <c>IndexSource</c> take it from <c>Sources[0]</c>, a count; <c>Map</c>, <c>MapIdx</c>,
    /// <c>Reverse</c> and <c>Sort</c> yield as many elements as their source array; the rest
    /// (filters, flat maps, reductions, pairwise loops) are unknown.</summary>
    public static CountBound OfLoop(TirLoweredLoop loop, TirNode scope = null)
    {
        if (loop == null || loop.Sources.Count == 0)
            return CountBound.Unknown;
        if (loop.Kind == "MapRange" || (loop.IndexSource && loop.Kind is "Map" or "MapIdx"))
            return OfCount(loop.Sources[0], scope);
        if (loop.Kind is "Map" or "MapIdx" or "Reverse" or "Sort" or "SortIdx")
            return OfSourceArray(loop.Sources[0], scope);
        return CountBound.Unknown;
    }

    /// <summary>The value of an integer count expression: constant, guarded, or unknown.</summary>
    public static CountBound OfCount(TirNode count, TirNode scope = null)
    {
        if (count == null)
            return CountBound.Unknown;
        var constant = Fold(count);
        if (constant != null)
            return new CountBound(constant, null);
        // Only literals and arithmetic that did not fold overflowed: guarding it would hide that.
        if (!count.Descendants().Append(count).Any(n => n is TirVariable or TirParameter))
            return CountBound.Unknown;
        return IsTotal(count, Assigned(scope)) ? new CountBound(null, count) : CountBound.Unknown;
    }

    private static CountBound OfSourceArray(TirNode source, TirNode scope)
    {
        if (source is TirArray or TirLoweredLoop)
            return OfArray(source, scope);
        // A variable or parameter array: its length is the guarded Count of it.
        if (source is TirVariable or TirParameter && IsTotal(source, Assigned(scope), array: true)
            && !IsBuilder(source.Type))
            return new CountBound(null, CountOf(source));
        return CountBound.Unknown;
    }

    private static int? Fold(TirNode node)
    {
        var value = FoldLong(node);
        return value == null ? null : (int)value.Value;
    }

    private static long? FoldLong(TirNode node)
    {
        switch (node)
        {
            case TirLiteral { Value: long l }:
                return InRange(l);
            case TirLiteral { Value: int i }:
                return i;
            case TirCall { Name: "Count", Args.Count: 1 } count when count.Args[0] is TirArray array:
                return array.Elements.Count;
            case TirCall { Name: "Add" or "Subtract" or "Multiply", Args.Count: 2 } c when c.Type?.Name == "Integer":
                var left = FoldLong(c.Args[0]);
                var right = FoldLong(c.Args[1]);
                if (left == null || right == null)
                    return null;
                try
                {
                    return InRange(c.Name switch
                    {
                        "Add" => checked(left.Value + right.Value),
                        "Subtract" => checked(left.Value - right.Value),
                        _ => checked(left.Value * right.Value),
                    });
                }
                catch (OverflowException)
                {
                    return null;
                }
            default:
                return null;
        }
    }

    private static long? InRange(long value) => value is >= int.MinValue and <= int.MaxValue ? value : null;

    private static HashSet<object> Assigned(TirNode scope)
        => scope == null
            ? new HashSet<object>()
            : scope.Descendants().Append(scope).OfType<TirAssign>()
                .Select(a => a.LValue is TirVariable v ? (object)v.Def : a.LValue is TirParameter p ? p.Def : null)
                .Where(d => d != null).ToHashSet();

    private static bool IsTotal(TirNode node, HashSet<object> assigned, bool array = false)
        => node switch
        {
            TirLiteral { Value: long or int } => !array,
            TirVariable v => !assigned.Contains(v.Def) && (array || v.Type?.Name == "Integer"),
            TirParameter p => !assigned.Contains(p.Def) && (array || p.Type?.Name == "Integer"),
            TirCall { Name: "Add" or "Subtract" or "Multiply", Args.Count: 2 } c
                => !array && c.Type?.Name == "Integer" && c.Args.All(a => IsTotal(a, assigned)),
            TirCall { Name: "Count", Args.Count: 1 } c
                => !array && c.Type?.Name == "Integer" && !IsBuilder(c.Args[0].Type)
                    && IsTotal(c.Args[0], assigned, array: true),
            _ => false,
        };

    private static bool IsBuilder(TypeExpression type) => type?.Name is "List" or "Buffer";

    private static TirNode CountOf(TirNode array)
        => new TirCall(null, EmissionKind.InstanceMethod, new List<TypeExpression> { array.Type }, null,
            new[] { array }, null, array.Origin, "Count");
}
