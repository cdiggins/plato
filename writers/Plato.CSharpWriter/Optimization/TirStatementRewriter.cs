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
            TirLoop l => Loop(TirEvaluation.Capture(l.Condition, value), scope(l.Body)),
            TirGuardedLoop l => Loop(new TirEvaluation(l.Test.Statements.SelectMany(s =>
                Rewrite(s, value, scope, region)).ToArray(), l.Test.Value), scope(l.Body), value),
            _ => node
        };
        return before.Append(statement).ToArray();
    }

    private static TirNode Loop(TirEvaluation test, TirNode body,
        Func<TirNode, List<TirNode>, TirNode> rewrite = null)
    {
        if (rewrite != null)
        {
            var tail = TirEvaluation.Capture(test.Value, rewrite);
            test = new TirEvaluation(test.Statements.Concat(tail.Statements).ToArray(), tail.Value);
        }
        return test.Statements.Count == 0 ? new TirLoop(test.Value, body, body.Origin)
            : new TirGuardedLoop(test, body);
    }
}
