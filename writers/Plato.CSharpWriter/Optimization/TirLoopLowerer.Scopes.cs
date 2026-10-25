using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirLoopLowerer
{
    private static TirFunction RewriteScoped(TirFunction function)
    {
        var lowering = new ScopedLoopLowering(function);
        var body = lowering.Body(function.Body);
        return new TirFunction(function.Original, function.Parameters, function.ReturnType, body,
            function.ZonkedParameterTypes, function.ZonkedReturnType);
    }

    private sealed class ScopedLoopLowering
    {
        private readonly HashSet<string> _names;
        private readonly HashSet<int> _reservedIds;
        private int _next;

        public ScopedLoopLowering(TirFunction function)
        {
            _names = function.AllNodes.Select(n => n switch
            {
                TirVariable v => v.Def?.Name, TirParameter p => p.Def?.Name,
                TirLet l => l.Def?.Name, TirInlineBinding b => b.Def.Name,
                TirInlineRegion r => r.Result.Name, TirTempRef t => t.Name, _ => null
            }).Concat(function.Parameters.Select(p => p.Name))
                .Concat(function.AllNodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name))
                .Where(n => n != null).ToHashSet();
            _reservedIds = _names.SelectMany(n => Regex.Matches(n, @"\d+").Cast<Match>())
                .Select(m => int.TryParse(m.Value, out var value) ? value : -1).ToHashSet();
        }

        private VariableDef Fresh(TypeExpression type)
        {
            string name;
            do name = $"_eval{_next++}"; while (!_names.Add(name));
            return new VariableDef(null, name, type, null);
        }

        private TirNode Bind(TirNode value, List<TirNode> before)
        {
            // A literal lambda is needed by loop recognition; its body does not execute here.
            if (value is TirLambda or TirTypeRef)
                return value;
            var def = Fresh(value.Type);
            before.Add(new TirInlineBinding(def, value));
            return new TirVariable(def, def.Type, value.Origin);
        }

        public TirNode Body(TirNode node)
        {
            if (TirRewrite.IsStatementNode(node))
                return Scope(node);
            var value = TirEvaluation.Capture(node, Value);
            return value.Statements.Count == 0 ? value.Value : new TirBlock(value.Statements
                .Append(new TirReturn(value.Value, node.Origin)).ToArray(), node.Origin);
        }

        public TirNode Scope(TirNode node)
            => node == null ? null : new TirBlock((node is TirBlock b ? b.Statements : new[] { node })
                .SelectMany(n => TirStatementRewriter.Rewrite(n, Value, Scope)).ToArray(), node.Origin);

        private TirNode Value(TirNode node, List<TirNode> before)
        {
            if (node is TirConditional conditional)
                return Conditional(conditional, before);
            if (node is not (TirCall or TirCoerce or TirInvoke or TirNew or TirArray
                or TirConstructorCall or TirComponentAccess))
                return node;
            var chain = node is TirCall c && IsChainConsumer(c);
            var operands = node.Children.Select((n, i) => TirEvaluation.Capture(n,
                chain && i == 0 ? Chain : Value)).ToArray();
            var values = TirEvaluation.Sequence(operands, before, Bind);
            var rebuilt = node.WithChildren(values);
            if (rebuilt is not TirCall call)
                return rebuilt;
            var lowered = LowerCall(call, before);
            if (lowered != null || !chain)
                return lowered ?? rebuilt;
            var fallback = call.Args.Select(a => TirEvaluation.Capture(a, Value)).ToArray();
            call = (TirCall)call.WithChildren(TirEvaluation.Sequence(fallback, before, Bind));
            return LowerCall(call, before) ?? call;
        }

        private TirNode LowerCall(TirCall call, List<TirNode> before)
        {
            while (_reservedIds.Contains(NextId)) NextId++;
            var loops = new List<TirNode>();
            var value = TryLower(call, loops);
            before.AddRange(loops.Cast<TirLoweredLoop>().Select(l => TirLoopCallbacks.Freshen(l, Fresh, Callback)));
            return value;
        }

        private TirNode Callback(TirNode node)
        {
            if (node is TirCoerce coerce)
                return coerce.WithChildren(new[] { Callback(coerce.Inner) });
            if (node is not TirLambda lambda)
                return node;
            return lambda.WithChildren(new[] { Body(lambda.Body) });
        }

        private TirNode Chain(TirNode node, List<TirNode> before)
        {
            if (!IsProducer(node) || TirRewrite.StripCoerce(node) is not TirCall call)
                return Value(node, before);
            var source = call.Name == "Map" ? Chain(call.Args[0], before) : Value(call.Args[0], before);
            var rebuilt = call.WithChildren(new[] { source, call.Args[1] });
            return node is TirCoerce coerce ? coerce.WithChildren(new[] { rebuilt }) : rebuilt;
        }

        private TirNode Conditional(TirConditional node, List<TirNode> before)
        {
            var condition = Value(node.Condition, before);
            return TirEvaluation.Conditional(node, condition,
                TirEvaluation.Capture(node.IfTrue, Value), TirEvaluation.Capture(node.IfFalse, Value),
                Fresh).AppendTo(before);
        }
    }
}
