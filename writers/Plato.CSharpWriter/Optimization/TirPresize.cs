// Gives a List builder the capacity its source already states (--presize, TKT-1131).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// A <c>List</c> grows by doubling, and each doubling copies the backing array (17 percent of a
/// parse of the Plato STEP front end was <c>Array.Resize</c>). When the body says how many
/// elements the builder will receive, this pass writes <c>new List&lt;T&gt;(count)</c> in place of
/// <c>new List&lt;T&gt;()</c>, so the array is allocated once.
///
/// The one shape recognised is a counted fill: in a block,
/// <code>
///   var xs = new List&lt;T&gt;();  ...  var i = k;  ...
///   while (i &lt; bound) { ... xs = xs.Add(e); ... i = i + 1; ... }
/// </code>
/// where <c>k</c> is a non-negative integer literal, <c>xs</c> is appended to exactly once at the
/// top level of the body, <c>i</c> is assigned only by that one increment, the body has no
/// <c>return</c>, and <c>bound</c> is built from literals, variables and parameters that nothing
/// assigns from the builder's creation to the loop's end, <c>Count</c> of a non-builder, and
/// <c>Add</c>, <c>Subtract</c> and <c>Multiply</c>. The loop then appends exactly
/// <c>bound - k</c> times and the capacity is that. (Division is left out so that evaluating the
/// capacity early cannot throw.)
///
/// The capacity is a hint: the runtime ignores a negative one and the builder still grows if
/// appended to more, so a wrong guess costs memory or a copy, never a value. A list created with
/// an explicit capacity in the source is left as written. <c>Buffer</c> already takes its size at
/// construction. A <c>MapRange</c> or <c>Map</c> result is an exactly sized array in the lowered
/// loop already.
/// </summary>
public static class TirPresize
{
    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.Presize || tir?.Body == null)
            return tir;
        var body = Visit(tir.Body);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static TirNode Visit(TirNode n)
    {
        if (n == null)
            return null;
        var children = n.Children.ToArray();
        TirNode[] replacements = null;
        for (var i = 0; i < children.Length; i++)
        {
            var child = Visit(children[i]);
            if (ReferenceEquals(child, children[i]))
                continue;
            replacements ??= (TirNode[])children.Clone();
            replacements[i] = child;
        }
        var rebuilt = replacements == null ? n : n.WithChildren(replacements);
        return rebuilt is TirBlock block ? PresizeBlock(block) : rebuilt;
    }

    private static TirNode PresizeBlock(TirBlock block)
    {
        var statements = block.Statements;
        List<TirNode> changed = null;
        for (var k = 0; k < statements.Count; k++)
        {
            if (!IsEmptyList(statements[k], out var let))
                continue;
            var capacity = CapacityFor(statements, k, let.Def);
            if (capacity == null)
                continue;
            changed ??= statements.ToList();
            var created = (TirNew)let.Value;
            changed[k] = new TirLet(let.Def,
                new TirNew(created.NewType, new[] { capacity }, created.Type, created.Origin), let.Type, let.Origin);
        }
        return changed == null ? block : new TirBlock(changed, block.Origin);
    }

    private static bool IsEmptyList(TirNode statement, out TirLet let)
    {
        let = statement as TirLet;
        return let?.Value is TirNew created && created.Args.Count == 0 && created.NewType?.Name == "List";
    }

    private static TirNode CapacityFor(IReadOnlyList<TirNode> statements, int k, VariableDef list)
    {
        for (var at = k + 1; at < statements.Count; at++)
        {
            if (statements[at] is not TirLoop loop || !TryMatchCondition(loop.Condition, out var counter, out var bound))
                continue;
            if (loop.Body is not TirBlock body || !IsCountedFill(body, list, counter.Def))
                continue;
            var start = CounterStart(statements, at, counter.Def);
            if (start == null)
                return null;
            var gap = statements.Skip(k + 1).Take(at - k - 1).ToList();
            if (!IsStable(bound, gap.Append(loop).ToList(), gap))
                return null;
            return start.Value == 0
                ? bound
                : Call("Subtract", counter.Type, bound, Literal(start.Value, counter.Type, bound.Origin));
        }
        return null;
    }

    private static bool TryMatchCondition(TirNode condition, out TirVariable counter, out TirNode bound)
    {
        counter = null;
        bound = null;
        if (condition is not TirCall { Name: "LessThan", Args.Count: 2 } less)
            return false;
        counter = less.Args[0] as TirVariable;
        bound = less.Args[1];
        return counter?.Type?.Name == "Integer" && bound?.Type?.Name == "Integer";
    }

    /// <summary>The body appends to the list once at its top level, bumps the counter by one once at
    /// its top level, assigns neither anywhere else, and has no early exit.</summary>
    private static bool IsCountedFill(TirBlock body, VariableDef list, VariableDef counter)
    {
        if (body.Descendants().Any(d => d is TirReturn))
            return false;
        var assigns = body.Descendants().OfType<TirAssign>().ToList();
        return body.Statements.Count(s => IsAppend(s, list)) == 1
            && body.Statements.Count(s => IsBump(s, counter)) == 1
            && assigns.Count(a => Target(a) == list) == 1
            && assigns.Count(a => Target(a) == counter) == 1;
    }

    private static VariableDef Target(TirAssign assign) => (assign.LValue as TirVariable)?.Def;

    private static bool IsAppend(TirNode statement, VariableDef list)
        => statement is TirAssign { LValue: TirVariable target, RValue: TirCall { Name: "Add", Args.Count: 2 } add }
            && target.Def == list && add.Args[0] is TirVariable receiver && receiver.Def == list;

    private static bool IsBump(TirNode statement, VariableDef counter)
        => statement is TirAssign { LValue: TirVariable target, RValue: TirCall { Name: "Add", Args.Count: 2 } add }
            && target.Def == counter && add.Args[0] is TirVariable left && left.Def == counter
            && add.Args[1] is TirLiteral { Value: 1L };

    /// <summary>The literal the counter starts at: its let comes before the loop, and nothing assigns
    /// it between the let and the loop.</summary>
    private static long? CounterStart(IReadOnlyList<TirNode> statements, int loopAt, VariableDef counter)
    {
        for (var at = loopAt - 1; at >= 0; at--)
        {
            if (statements[at] is TirLet { Value: TirLiteral { Value: long start } } let && let.Def == counter)
            {
                var gap = statements.Skip(at + 1).Take(loopAt - at - 1);
                return start >= 0 && !gap.Any(s => s.Descendants().OfType<TirAssign>().Any(a => Target(a) == counter))
                    ? start
                    : null;
            }
        }
        return null;
    }

    /// <summary>Whether the bound can be computed where the list is created and still be the loop's
    /// bound: made only of total integer operations, and its variables are declared before the
    /// list and assigned nowhere from there to the end of the loop.</summary>
    private static bool IsStable(TirNode bound, List<TirNode> through, List<TirNode> afterList)
    {
        var assigned = through.SelectMany(s => s.Descendants()).OfType<TirAssign>()
            .Select(a => a.LValue is TirVariable v ? (object)v.Def : a.LValue is TirParameter p ? p.Def : null)
            .Where(d => d != null).ToHashSet();
        var declared = afterList.SelectMany(s => s.Descendants()).OfType<TirLet>().Select(l => l.Def).ToHashSet();
        return IsTotal(bound);

        bool IsTotal(TirNode node)
            => node switch
            {
                TirLiteral { Value: long } => true,
                TirVariable v => !assigned.Contains(v.Def) && !declared.Contains(v.Def),
                TirParameter p => !assigned.Contains(p.Def),
                TirCall { Name: "Add" or "Subtract" or "Multiply", Args.Count: 2 } c
                    => c.Type?.Name == "Integer" && c.Args.All(IsTotal),
                TirCall { Name: "Count", Args.Count: 1 } c
                    => c.Type?.Name == "Integer" && !IsBuilder(c.Args[0].Type) && IsTotal(c.Args[0]),
                _ => false,
            };
    }

    private static bool IsBuilder(TypeExpression type) => type?.Name is "List" or "Buffer";

    private static TirCall Call(string name, TypeExpression type, params TirNode[] args)
        => new TirCall(null, EmissionKind.InstanceMethod, args.Select(a => a.Type).ToList(), type, args, type,
            args[0].Origin, name);

    private static TirLiteral Literal(long value, TypeExpression type, Symbol origin)
        => new TirLiteral(value, LiteralTypesEnum.Integer, type, origin);
}
