using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirInliner
{
    /// <summary>Experimental direct-call inlining at whole initializer and return positions.</summary>
    public static TirFunction InlineStatements(TirFunction tir, ITirInlineHost host, string owner,
        bool eliminateLambdas = false, bool simplify = false, CSharpFunctionInfo functionInfo = null)
    {
        if (tir?.Body == null)
            return tir;
        var expansion = new StatementExpansion(tir, host, owner, eliminateLambdas, simplify);
        var body = tir.Body;
        var initialSize = body.Descendants().Count();
        var initialCalls = host.InlineReport?.StatementCallsInlined ?? 0;
        var initialLambdas = host.InlineReport?.KnownLambdasInlined ?? 0;
        var budget = host is CSharpWriter w ? System.Math.Min(w.StatementNodeBudget, w.RemainingStatementGrowth) : 4096;
        for (var pass = 0; pass < (simplify ? 8 : 1); pass++)
        {
            var previous = body.ToString();
            body = expansion.Finish(expansion.Rewrite(body));
            if (simplify && host is CSharpWriter writer)
            {
                var current = new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                    tir.ZonkedParameterTypes, tir.ZonkedReturnType);
                current = TirFusion.Rewrite(current, writer);
                current = TirComponentUnroller.UnrollFunction(current, functionInfo, writer);
                body = current.Body;
            }
            var growth = System.Math.Max(0, body.Descendants().Count() - initialSize);
            if (growth > budget)
            {
                if (host.InlineReport != null)
                {
                    host.InlineReport.StatementCallsInlined = initialCalls;
                    host.InlineReport.KnownLambdasInlined = initialLambdas;
                }
                host.InlineReport?.Refuse(tir.Original?.Name, "statement expansion", InlineRefusal.ExpansionBudget);
                return tir;
            }
            expansion.SetRemainingGrowth(budget - growth);
            if (body.ToString() == previous)
                break;
        }
        if (host is CSharpWriter module)
            module.RemainingStatementGrowth -= System.Math.Max(0, body.Descendants().Count() - initialSize);
        return ReferenceEquals(body, tir.Body) ? tir : new TirFunction(tir.Original, tir.Parameters,
            tir.ReturnType, body, tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private sealed partial class StatementExpansion
    {
        private const int MaxExpandedNodes = 4096;
        private const int MaxDepth = 16;
        private readonly TirFunction _caller;
        private readonly ITirInlineHost _host;
        private readonly string _owner;
        private readonly HashSet<string> _names;
        private readonly HashSet<FunctionDef> _ancestry = new();
        private int _remaining = MaxExpandedNodes;
        private int _workRemaining = 100000;
        private int _next;

        public StatementExpansion(TirFunction caller, ITirInlineHost host, string owner, bool eliminateLambdas, bool simplify)
        {
            (_caller, _host, _owner) = (caller, host, owner);
            _names = CollectBoundNames(caller, caller.Body);
            _ancestry.Add(caller.Original);
            _eliminateLambdas = eliminateLambdas;
            _simplify = simplify;
            RecordAssignments(caller.Body);
            if (host is CSharpWriter writer)
                _remaining = System.Math.Min(writer.StatementNodeBudget, writer.RemainingStatementGrowth);
        }

        public void SetRemainingGrowth(int value) => _remaining = value;

        private VariableDef Fresh(TypeExpression type)
        {
            string name;
            do name = $"_inline{_next++}"; while (!_names.Add(name));
            return new VariableDef(null, name, type, null);
        }

        public TirNode Rewrite(TirNode node)
        {
            if (node is TirBlock block)
                return RemoveUnusedCallables(new TirBlock(block.Statements.SelectMany(Statement).ToList(), block.Origin));
            var statements = new List<TirNode>();
            var value = Expand(node, statements);
            return statements.Count == 0 ? value
                : RemoveUnusedCallables(new TirBlock(statements.Append(new TirReturn(value, node.Origin)).ToList(), node.Origin));
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
                TirInlineRegion r => RewriteRegion(r),
                TirAssign a when a.LValue is TirVariable => new TirAssign(a.LValue, Expand(a.RValue, prefix), a.Type, a.Origin),
                TirBlock b => Rewrite(b),
                TirIf i => new TirIf(Expand(i.Condition, prefix), RewriteScope(i.IfTrue), RewriteScope(i.IfFalse), i.Origin),
                TirLoop l => new TirLoop(l.Condition, RewriteScope(l.Body), l.Origin),
                _ => node
            };
            if (result is TirLet let)
                Remember(let.Def, let.Value);
            if (result is TirInlineBinding binding)
                Remember(binding.Def, binding.Value);
            return prefix.Append(result);
        }

        private TirNode RewriteScope(TirNode node)
            => node == null ? null : new TirBlock(Statement(node).ToList(), node.Origin);

        private TirNode Bind(TirNode value, TypeExpression type, List<TirNode> prefix)
        {
            var def = Fresh(type);
            prefix.Add(new TirInlineBinding(def, value));
            Remember(def, value);
            return new TirVariable(def, type, value.Origin);
        }

        private TirNode Expand(TirNode node, List<TirNode> prefix)
        {
            if (_workRemaining-- <= 0)
                return node;
            node = Simplify(node);
            if (_eliminateLambdas && node is TirInvoke invoke)
                return ExpandInvoke(invoke, prefix);
            if (node is TirConditional conditional && Ground(node.Type))
                return ExpandConditional(conditional, prefix);
            if (node is TirCoerce coerce)
                return coerce.WithChildren(new[] { Expand(coerce.Inner, prefix) });
            if (node is TirCall nested)
                node = Simplify(ExposeCallback((TirCall)ExpandArguments(nested, prefix)));
            if (node is not TirCall call)
                return node;
            TirNode Refuse(string reason)
            {
                _host.InlineReport?.RecordSite(_owner, _caller.Original, call, reason);
                return call;
            }
            if (call.Callee?.Body == null || call.Args.Count == 0)
                return Refuse("no body or nullary call");
            var receiver = TrustedTypeName(_caller, call.Args[0], _host, _owner);
            var callee = _host.TryGetGroundTirByTypeName(call.Callee, receiver);
            if (callee?.Body == null)
                return Refuse("no ground body");
            var block = callee.Body as TirBlock;
            if (block == null && _simplify)
                block = new TirBlock(new[] { new TirReturn(callee.Body, callee.Body.Origin) }, callee.Body.Origin);
            if (block == null)
                return Refuse("expression policy disabled");
            if (_ancestry.Contains(callee.Original))
                return Refuse("recursive ancestry");
            if (_ancestry.Count >= MaxDepth)
                return Refuse("depth limit");
            if (!Eligible(call, callee, block))
                return Refuse("signature or body not proven");
            var size = block.Descendants().Count() + call.Args.Count * 2 + 2;
            if (size > _remaining)
                return Refuse("growth limit");
            _remaining -= size;
            var parameters = new Dictionary<ParameterDef, TirNode>();
            for (var i = 0; i < call.Args.Count; i++)
                parameters[callee.Parameters[i]] = Bind(call.Args[i], ParameterType(callee, i), prefix);
            _ancestry.Add(callee.Original);
            var locals = block.Descendants().OfType<TirLet>().ToDictionary(l => l.Def, l => Fresh(l.Value.Type));
            var resultDef = Fresh(callee.ZonkedReturnType ?? callee.ReturnType);
            var cloned = CloneBody(block, parameters, locals, resultDef);
            RecordAssignments(cloned);
            prefix.Add(new TirInlineRegion(resultDef, Rewrite(cloned), _ancestry.ToArray()));
            if (_host.InlineReport != null) _host.InlineReport.StatementCallsInlined++;
            _host.InlineReport?.RecordSite(_owner, _caller.Original, call, "statement region expanded");
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
            if (_simplify && BooleanValue(condition) is { } selected)
                return Expand(selected ? node.IfTrue : node.IfFalse, prefix);
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
            prefix.Add(new TirInlineRegion(result, new TirBlock(new[] { branch }, node.Origin), _ancestry.ToArray()));
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
            return body.Descendants().All(n => n is not (TirName or TirUnresolved or TirTypeRef)
                && (n is not TirLambda lam || _eliminateLambdas && !IsEtaShaped(lam))
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
