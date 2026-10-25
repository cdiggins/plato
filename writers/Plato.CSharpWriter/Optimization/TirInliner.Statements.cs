using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirInliner
{
    /// <summary>Experimental direct-call inlining at whole initializer and return positions.</summary>
    public static TirFunction InlineStatements(TirFunction tir, ITirInlineHost host, string owner)
    {
        if (tir?.Body == null)
            return tir;
        var expansion = new StatementExpansion(tir, host, owner);
        var body = expansion.Rewrite(tir.Body);
        return ReferenceEquals(body, tir.Body) ? tir : new TirFunction(tir.Original, tir.Parameters,
            tir.ReturnType, body, tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private sealed class StatementExpansion
    {
        private const int MaxExpandedNodes = 4096;
        private const int MaxDepth = 16;
        private readonly TirFunction _caller;
        private readonly ITirInlineHost _host;
        private readonly string _owner;
        private readonly HashSet<string> _names;
        private readonly HashSet<FunctionDef> _ancestry = new();
        private int _remaining = MaxExpandedNodes;
        private int _next;

        public StatementExpansion(TirFunction caller, ITirInlineHost host, string owner)
        {
            (_caller, _host, _owner) = (caller, host, owner);
            _names = CollectBoundNames(caller, caller.Body);
            _ancestry.Add(caller.Original);
        }

        private VariableDef Fresh(TypeExpression type)
        {
            string name;
            do name = $"_inline{_next++}"; while (!_names.Add(name));
            return new VariableDef(null, name, type, null);
        }

        public TirNode Rewrite(TirNode node)
        {
            if (node is TirBlock block)
                return new TirBlock(block.Statements.SelectMany(Statement).ToList(), block.Origin);
            var statements = new List<TirNode>();
            var value = Expand(node, statements);
            return statements.Count == 0 ? node
                : new TirBlock(statements.Append(new TirReturn(value, node.Origin)).ToList(), node.Origin);
        }

        private IEnumerable<TirNode> Statement(TirNode node)
        {
            var prefix = new List<TirNode>();
            var result = node switch
            {
                TirReturn r => new TirReturn(Expand(r.Value, prefix), r.Origin),
                TirLet l => new TirLet(l.Def, Expand(l.Value, prefix), l.Type, l.Origin),
                TirInlineBinding b => new TirInlineBinding(b.Def, Expand(b.Value, prefix)),
                TirInlineExit e => new TirInlineExit(e.Result, Expand(e.Value, prefix)),
                TirAssign a when a.LValue is TirVariable => new TirAssign(a.LValue, Expand(a.RValue, prefix), a.Type, a.Origin),
                TirBlock b => Rewrite(b),
                TirIf i => new TirIf(Expand(i.Condition, prefix), RewriteScope(i.IfTrue), RewriteScope(i.IfFalse), i.Origin),
                TirLoop l => new TirLoop(l.Condition, RewriteScope(l.Body), l.Origin),
                _ => node
            };
            return prefix.Append(result);
        }

        private TirNode RewriteScope(TirNode node)
            => node == null ? null : new TirBlock(Statement(node).ToList(), node.Origin);

        private TirNode Bind(TirNode value, TypeExpression type, List<TirNode> prefix)
        {
            var def = Fresh(type);
            prefix.Add(new TirInlineBinding(def, value));
            return new TirVariable(def, type, value.Origin);
        }

        private TirNode Expand(TirNode node, List<TirNode> prefix)
        {
            if (node is TirConditional conditional && Ground(node.Type))
                return ExpandConditional(conditional, prefix);
            if (node is TirCoerce coerce)
                return coerce.WithChildren(new[] { Expand(coerce.Inner, prefix) });
            if (node is TirCall nested)
                node = ExpandArguments(nested, prefix);
            if (node is not TirCall call || call.Callee?.Body == null || call.Args.Count == 0)
                return node;
            var receiver = TrustedTypeName(_caller, call.Args[0], _host, _owner);
            var callee = _host.TryGetGroundTirByTypeName(call.Callee, receiver);
            if (callee?.Body is not TirBlock block || _ancestry.Contains(callee.Original)
                || _ancestry.Count >= MaxDepth || !Eligible(call, callee, block))
                return node;
            var size = block.Descendants().Count() + call.Args.Count * 2 + 2;
            if (size > _remaining)
                return node;
            _remaining -= size;
            _ancestry.Add(callee.Original);
            var parameters = new Dictionary<ParameterDef, TirNode>();
            for (var i = 0; i < call.Args.Count; i++)
                parameters[callee.Parameters[i]] = Bind(call.Args[i], ParameterType(callee, i), prefix);
            var locals = block.Descendants().OfType<TirLet>().ToDictionary(l => l.Def, l => Fresh(l.Value.Type));
            var resultDef = Fresh(callee.ZonkedReturnType ?? callee.ReturnType);
            TirNode Clone(TirNode n) => TirRewrite.Rewrite(n, x => x switch
            {
                TirParameter p when parameters.TryGetValue(p.Def, out var value) => value,
                TirVariable v when locals.TryGetValue(v.Def, out var def) => new TirVariable(def, v.Type, v.Origin),
                TirLet l => new TirInlineBinding(locals[l.Def], l.Value),
                TirReturn r => new TirInlineExit(resultDef, r.Value),
                _ => x
            });
            prefix.Add(new TirInlineRegion(resultDef, Rewrite(Clone(block))));
            _ancestry.Remove(callee.Original);
            return new TirVariable(resultDef, resultDef.Type, call.Origin);
        }

        private TirNode ExpandArguments(TirCall call, List<TirNode> prefix)
        {
            if (call.Args.Any(a => !Ground(a.Type) || a is TirTypeRef))
                return call;
            var parts = call.Args.Select(a =>
            {
                var before = new List<TirNode>();
                return (Before: before, Value: Expand(a, before));
            }).ToList();
            if (parts.All(p => p.Before.Count == 0))
                return call.WithChildren(parts.Select(p => p.Value).ToList());
            var values = new List<TirNode>();
            foreach (var part in parts)
            {
                prefix.AddRange(part.Before);
                values.Add(Bind(part.Value, part.Value.Type, prefix));
            }
            return call.WithChildren(values);
        }

        private TirNode ExpandConditional(TirConditional node, List<TirNode> prefix)
        {
            var condition = Expand(node.Condition, prefix);
            var yes = new List<TirNode>();
            var no = new List<TirNode>();
            var ifTrue = Expand(node.IfTrue, yes);
            var ifFalse = Expand(node.IfFalse, no);
            if (yes.Count == 0 && no.Count == 0)
                return new TirConditional(condition, ifTrue, ifFalse, node.Type, node.Origin);
            var result = Fresh(node.Type);
            yes.Add(new TirInlineExit(result, ifTrue));
            no.Add(new TirInlineExit(result, ifFalse));
            var branch = new TirIf(condition, new TirBlock(yes, node.Origin), new TirBlock(no, node.Origin), node.Origin);
            prefix.Add(new TirInlineRegion(result, new TirBlock(new[] { branch }, node.Origin)));
            return new TirVariable(result, node.Type, node.Origin);
        }

        private bool Eligible(TirCall call, TirFunction callee, TirBlock body)
        {
            if (body.Statements.Count == 0 || body.Statements.Last() is not TirReturn
                || callee.Parameters.Count != call.Args.Count
                || !Ground(callee.ZonkedReturnType ?? callee.ReturnType)
                || !Equals(call.Type, callee.ZonkedReturnType ?? callee.ReturnType))
                return false;
            for (var i = 0; i < call.Args.Count; i++)
                if (!Ground(ParameterType(callee, i))
                    || !Equals(call.Args[i].Type, ParameterType(callee, i))
                    || call.Args[i] is TirTypeRef)
                    return false;
            return body.Descendants().All(n => n is not (TirName or TirUnresolved or TirLambda or TirTypeRef)
                && (n is not TirAssign a || a.LValue is TirVariable)
                && (n is not TirLet l || Ground(l.Value.Type))
                && (n is not TirCall c || c.Callee != null && IsCallableOnReceiver(_host, c))
                && (n.Type == null || Ground(n.Type)));
        }

        private static bool Ground(TypeExpression type)
            => type != null && !type.IsTypeVariable && !type.IsTypeParameter && type.Name != "Self"
                && type.TypeArgs.All(Ground);

        private static TypeExpression ParameterType(TirFunction function, int index)
            => function.ZonkedParameterTypes?[index] ?? function.Parameters[index].Type;
    }
}
