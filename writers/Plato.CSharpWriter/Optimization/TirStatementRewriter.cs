using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Places value prerequisites in their original statement's control-flow scope.</summary>
public static class TirStatementRewriter
{
    public static IReadOnlyList<TirNode> Rewrite(TirNode node,
        Func<TirNode, List<TirNode>, TirNode> value, Func<TirNode, TirNode> scope,
        Func<TirInlineRegion, TirNode> region = null)
    {
        var before = new List<TirNode>();
        var statement = node switch
        {
            TirReturn r => new TirReturn(value(r.Value, before), r.Origin),
            TirLet l => new TirLet(l.Def, value(l.Value, before), l.Type, l.Origin),
            TirInlineBinding b => new TirInlineBinding(b.Def, value(b.Value, before)),
            TirInlineExit e => new TirInlineExit(e.Result, value(e.Value, before)),
            TirInlineRegion r => region?.Invoke(r) ?? new TirInlineRegion(r.Result, scope(r.Body), r.Ancestry),
            TirAssign a when a.LValue is TirVariable => new TirAssign(a.LValue, value(a.RValue, before), a.Type, a.Origin),
            TirBlock b => scope(b),
            TirIf i => new TirIf(value(i.Condition, before), scope(i.IfTrue), scope(i.IfFalse), i.Origin),
            TirLoop l => new TirLoop(l.Condition, scope(l.Body), l.Origin),
            _ => node
        };
        return before.Append(statement).ToArray();
    }
}
