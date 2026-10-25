using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirLoopLowerer
{
    private static TirFunction RewriteScoped(TirFunction function)
    {
        var nodes = function.AllNodes.ToList();
        var assigned = nodes.OfType<TirAssign>().Select(a => a.LValue)
            .OfType<TirVariable>().Select(v => v.Def).ToHashSet();
        var names = nodes.Select(n => n switch
        {
            TirVariable v => v.Def?.Name, TirParameter p => p.Def?.Name,
            TirLet l => l.Def?.Name, TirInlineBinding b => b.Def.Name,
            TirInlineRegion r => r.Result.Name, TirTempRef t => t.Name, _ => null
        }).Where(n => n != null);
        var reservedIds = names.SelectMany(n => Regex.Matches(n, @"\d+").Cast<Match>())
            .Select(m => int.TryParse(m.Value, out var value) ? value : -1).ToHashSet();

        bool SafeArgument(TirNode n) => n is TirParameter or TirVariable or TirLiteral
            || n is TirLambda l && !l.Body.Descendants().Any(x => x is TirLambda
                || x is TirVariable v && assigned.Contains(v.Def));

        TirNode Value(TirNode value, List<TirNode> prefix)
        {
            // Direct intrinsic calls only: never run the general hoister inside a newly admitted scope.
            if (value is not TirCall call || call.Callee?.Body != null
                || call.Callee?.OwnerType?.Name != "Intrinsics" || !call.Args.All(SafeArgument))
                return value;
            while (reservedIds.Contains(NextId)) NextId++;
            return TryLower(call, prefix, true) ?? value;
        }

        TirNode Scope(TirNode node)
        {
            if (node == null) return null;
            if (node is TirBlock b)
                return new TirBlock(b.Statements.SelectMany(Statement).ToList(), b.Origin);
            return new TirBlock(Statement(node).ToList(), node.Origin);
        }

        IEnumerable<TirNode> Statement(TirNode node)
        {
            var prefix = new List<TirNode>();
            var result = node switch
            {
                TirBlock b => Scope(b),
                TirInlineRegion r => new TirInlineRegion(r.Result, Scope(r.Body), r.Ancestry),
                TirIf i => new TirIf(i.Condition, Scope(i.IfTrue), Scope(i.IfFalse), i.Origin),
                TirLoop l => new TirLoop(l.Condition, Scope(l.Body), l.Origin),
                TirReturn r => new TirReturn(Value(r.Value, prefix), r.Origin),
                TirInlineExit e => new TirInlineExit(e.Result, Value(e.Value, prefix)),
                TirLet l => new TirLet(l.Def, Value(l.Value, prefix), l.Type, l.Origin),
                TirInlineBinding b => new TirInlineBinding(b.Def, Value(b.Value, prefix)),
                _ => node
            };
            return prefix.Append(result);
        }

        var body = function.Body is TirBlock ? Scope(function.Body)
            : Scope(new TirReturn(function.Body, function.Body.Origin));
        return new TirFunction(function.Original, function.Parameters, function.ReturnType, body,
            function.ZonkedParameterTypes, function.ZonkedReturnType);
    }
}
