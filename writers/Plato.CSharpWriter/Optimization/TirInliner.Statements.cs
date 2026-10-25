using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirInliner
{
    /// <summary>Experimental call expansion with ordered evaluation and lexical result regions.</summary>
    public static TirFunction InlineStatements(TirFunction tir, ITirInlineHost host, string owner,
        bool eliminateLambdas = false, bool simplify = false, CSharpFunctionInfo functionInfo = null)
        => tir?.Body == null ? tir : new StatementPipeline(tir, host, owner,
            eliminateLambdas, simplify, functionInfo).Run();

    private sealed partial class StatementExpansion
    {
        private readonly TirFunction _caller;
        private readonly ITirInlineHost _host;
        private readonly string _owner;
        private readonly HashSet<string> _names;
        private readonly HashSet<FunctionDef> _ancestry = new();
        private readonly InlineExpansionBudget _budget;
        public int Version { get; private set; }
        private int _next;

        public StatementExpansion(TirFunction caller, ITirInlineHost host, string owner, bool eliminateLambdas, bool simplify, InlineExpansionBudget budget)
        {
            _budget = budget;
            (_caller, _host, _owner) = (caller, host, owner);
            _names = CollectBoundNames(caller, caller.Body);
            _ancestry.Add(caller.Original);
            _eliminateLambdas = eliminateLambdas;
            _simplify = simplify;
            RecordAssignments(caller.Body);
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
                return RemoveUnusedCallables(new TirBlock(block.Statements.SelectMany(Statement).ToList(), block.Origin));
            var statements = new List<TirNode>();
            var value = Expand(node, statements);
            return statements.Count == 0 ? value
                : RemoveUnusedCallables(new TirBlock(statements.Append(new TirReturn(value, node.Origin)).ToList(), node.Origin));
        }

        private IEnumerable<TirNode> Statement(TirNode node)
        {
            var statements = TirStatementRewriter.Rewrite(node, Expand, RewriteScope, RewriteRegion);
            var result = statements.Last();
            if (result is TirLet let)
                Remember(let.Def, let.Value);
            if (result is TirInlineBinding binding)
                Remember(binding.Def, binding.Value);
            return statements;
        }

        private TirNode RewriteScope(TirNode node)
            => node == null ? null : node is TirBlock ? Rewrite(node)
                : new TirBlock(Statement(node).ToList(), node.Origin);

        private TirNode Bind(TirNode value, TypeExpression type, List<TirNode> prefix)
        {
            var def = Fresh(type);
            prefix.Add(new TirInlineBinding(def, value));
            Remember(def, value);
            return new TirVariable(def, type, value.Origin);
        }

        private TirNode Expand(TirNode node, List<TirNode> prefix)
        {
            if (!_budget.Visit())
                return node;
            node = Simplify(node);
            if (node is TirInvoke invoke)
            {
                invoke = (TirInvoke)ExpandOperands(invoke, prefix);
                return _eliminateLambdas ? ExpandInvoke(invoke, prefix) : invoke;
            }
            if (node is TirNew or TirArray or TirConstructorCall or TirComponentAccess)
                return ExpandOperands(node, prefix);
            if (node is TirConditional conditional && Ground(node.Type))
                return ExpandConditional(conditional, prefix);
            if (node is TirCoerce coerce)
                return coerce.WithChildren(new[] { Expand(coerce.Inner, prefix) });
            if (node is TirCall nested)
                node = Simplify(ExposeCallback((TirCall)ExpandOperands(nested, prefix)));
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
            if (_ancestry.Count >= InlineExpansionBudget.MaxDepth)
                return Refuse("depth limit");
            if (!Eligible(call, callee, block))
                return Refuse("signature or body not proven");
            var size = block.Descendants().Count() + call.Args.Count * 2 + 2;
            if (!_budget.TrySpend(size))
                return Refuse("growth limit");
            var parameters = new Dictionary<ParameterDef, TirNode>();
            for (var i = 0; i < call.Args.Count; i++)
                parameters[callee.Parameters[i]] = Bind(call.Args[i], ParameterType(callee, i), prefix);
            _ancestry.Add(callee.Original);
            var resultDef = Fresh(callee.ZonkedReturnType ?? callee.ReturnType);
            var cloned = TirInlineCloner.Clone(block, parameters, resultDef, Fresh);
            RecordAssignments(cloned);
            prefix.Add(new TirInlineRegion(resultDef, Rewrite(cloned), _ancestry.ToArray()));
            if (_host.InlineReport != null) _host.InlineReport.StatementCallsInlined++;
            _host.InlineReport?.RecordSite(_owner, _caller.Original, call, "statement region expanded");
            _ancestry.Remove(callee.Original);
            Version++;
            return new TirVariable(resultDef, resultDef.Type, call.Origin);
        }

        private TirNode ExpandOperands(TirNode node, List<TirNode> prefix)
        {
            var children = node.Children.ToArray();
            if (children.Any(a => !Ground(a.Type) || a is TirTypeRef))
                return node;
            var parts = children.Select(a => TirEvaluation.Capture(a, Expand)).ToArray();
            var values = TirEvaluation.Sequence(parts, prefix, (value, before) => Bind(value, value.Type, before));
            return node.WithChildren(values);
        }

        private TirNode ExpandConditional(TirConditional node, List<TirNode> prefix)
        {
            var condition = Expand(node.Condition, prefix);
            if (_simplify && BooleanValue(condition) is { } selected)
                return Expand(selected ? node.IfTrue : node.IfFalse, prefix);
            return TirEvaluation.Conditional(node, condition,
                TirEvaluation.Capture(node.IfTrue, Expand), TirEvaluation.Capture(node.IfFalse, Expand),
                Fresh, _ancestry.ToArray()).AppendTo(prefix);
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
                && (n is not TirLambda lam || !IsEtaShaped(lam))
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
