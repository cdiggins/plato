using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

public static partial class TirInliner
{
    private sealed partial class StatementExpansion
    {
        private readonly bool _eliminateLambdas;
        private readonly Dictionary<VariableDef, TirLambda> _known = new();
        private readonly HashSet<object> _assigned = new();
        private readonly HashSet<TirLambda> _activeLambdas = new();
        private readonly Dictionary<TirLambda, HashSet<FunctionDef>> _lambdaAncestry = new();
        private readonly Dictionary<VariableDef, InlineRefusal> _callableRefusals = new();

        private TirCall ExposeCallback(TirCall call)
        {
            if (call.Callee?.Body != null || call.Callee?.OwnerType?.Name != "Intrinsics")
                return call;
            var index = call.Name switch { "MapRange" or "FlatMap" => 1, "Reduce" => 2, _ => -1 };
            if (index < 0 || call.Args.Count <= index || Known(call.Args[index]) is not { } lambda
                || !Equals(call.Args[index].Type, lambda.Type))
                return call;
            var args = call.Args.ToArray();
            args[index] = lambda;
            return (TirCall)call.WithChildren(args);
        }

        public TirNode Finish(TirNode body)
        {
            if (!_eliminateLambdas && !_simplify)
                return body;
            body = TirRewrite.Rewrite(body, n => _eliminateLambdas && n is TirLambda l && !IsEtaShaped(l)
                ? new TirLambda(l.Parameters, Rewrite(l.Body), l.Type, l.Origin) : n);
            for (var pass = 0; pass < MaxDepth; pass++)
            {
                var before = body.Descendants().Count();
                body = TirRewrite.Rewrite(body, n => n is TirBlock b ? RemoveUnusedCallables(b) : n);
                if (body.Descendants().Count() == before)
                    break;
            }
            return body;
        }

        private void RecordAssignments(TirNode node)
        {
            foreach (var assignment in node.Descendants().OfType<TirAssign>())
                if (assignment.LValue is TirVariable v) _assigned.Add(v.Def);
                else if (assignment.LValue is TirParameter p) _assigned.Add(p.Def);
        }

        private TirLambda Known(TirNode value)
        {
            if (!_eliminateLambdas)
                return null;
            if (value is TirCoerce coerce && coerce.ConversionFn == null
                && Equals(coerce.FromType, coerce.ToType))
                return Known(coerce.Inner);
            if (value is TirVariable variable && !_assigned.Contains(variable.Def))
                return _known.TryGetValue(variable.Def, out var known) ? known : null;
            if (value is not TirLambda lambda || IsEtaShaped(lambda))
                return null;
            return lambda.Body.Descendants().Any(n => n is TirVariable v && _assigned.Contains(v.Def)
                || n is TirParameter p && _assigned.Contains(p.Def)) ? null : lambda;
        }

        private void Remember(VariableDef def, TirNode value)
        {
            RememberValue(def, value);
            if (!_assigned.Contains(def) && Known(value) is { } lambda)
            {
                _known[def] = lambda;
                if (!_lambdaAncestry.ContainsKey(lambda))
                    _lambdaAncestry[lambda] = new HashSet<FunctionDef>(_ancestry);
            }
            else if (value is TirLambda)
                _callableRefusals[def] = InlineRefusal.MutableCapture;
            else if (value is TirVariable v && _callableRefusals.TryGetValue(v.Def, out var reason))
                _callableRefusals[def] = reason;
        }

        private TirNode RemoveUnusedCallables(TirBlock block)
        {
            if (!_eliminateLambdas && !_simplify)
                return block;
            var used = block.Descendants().OfType<TirVariable>().Select(v => v.Def).ToHashSet();
            return new TirBlock(block.Statements.Where(n => n switch
            {
                TirInlineBinding b => used.Contains(b.Def) || Known(b.Value) == null && !Discardable(b.Def, b.Value),
                TirLet l => used.Contains(l.Def) || Known(l.Value) == null && !Discardable(l.Def, l.Value),
                _ => true
            }).ToList(), block.Origin);
        }

        private TirNode CloneBody(TirNode body, Dictionary<ParameterDef, TirNode> parameters,
            Dictionary<VariableDef, VariableDef> locals, VariableDef result)
        {
            var lambdaParameters = body.Descendants().OfType<TirLambda>()
                .SelectMany(l => l.Parameters).Distinct().ToDictionary(p => p,
                    p => new ParameterDef(p.Scope, Fresh(p.Type).Name, p.Type, p.Index));
            var depth = 0;
            return TirRewrite.Rewrite(body, n => n switch
            {
                TirParameter p when parameters.TryGetValue(p.Def, out var value) => value,
                TirParameter p when lambdaParameters.TryGetValue(p.Def, out var renamed) => new TirParameter(renamed, p.Type, p.Origin),
                TirVariable v when locals.TryGetValue(v.Def, out var def) => new TirVariable(def, def.Type ?? v.Type, v.Origin),
                TirLet l => new TirLet(locals[l.Def], l.Value, l.Type, l.Origin),
                TirLambda l => new TirLambda(l.Parameters.Select(p => lambdaParameters[p]).ToList(), l.Body, l.Type, l.Origin),
                TirReturn r when depth == 0 => new TirInlineExit(result, r.Value),
                _ => n
            }, n => { if (n is TirLambda) depth++; }, n => { if (n is TirLambda) depth--; });
        }

        private TirNode ExpandInvoke(TirInvoke invoke, List<TirNode> prefix)
        {
            TirNode Refuse(InlineRefusal reason)
            {
                _host.InlineReport?.Refuse(_caller.Original?.Name, "delegate invocation", reason);
                _host.InlineReport?.RecordSite(_owner, _caller.Original, invoke, reason.ToString());
                return invoke;
            }
            var lambda = Known(invoke.Target);
            var signature = invoke.Target.Type?.TypeArgs;
            if (lambda == null)
                return Refuse(invoke.Target is TirVariable v && _callableRefusals.TryGetValue(v.Def, out var reason)
                    ? reason : InlineRefusal.UnknownCallable);
            if (signature?.Count != invoke.Args.Count + 1
                || lambda.Parameters.Count != invoke.Args.Count || !Ground(invoke.Type)
                || !Equals(invoke.Type, signature.Last()))
                return Refuse(InlineRefusal.CallableSignatureMismatch);
            if (_activeLambdas.Contains(lambda))
                return Refuse(InlineRefusal.SelfRecursive);
            for (var i = 0; i < invoke.Args.Count; i++)
                if (!Ground(signature[i]) || !Equals(signature[i], invoke.Args[i].Type))
                    return Refuse(InlineRefusal.CallableSignatureMismatch);
            var cost = lambda.Body.Descendants().Count() + invoke.Args.Count * 2 + 2;
            if (cost > _remaining || _activeLambdas.Count >= MaxDepth)
                return Refuse(InlineRefusal.ExpansionBudget);
            var body = lambda.Body is TirBlock ? lambda.Body : new TirBlock(new[] { new TirReturn(lambda.Body, lambda.Origin) }, lambda.Origin);
            if (body.Descendants().Any(n => n is TirInlineRegion or TirInlineBinding))
                return Refuse(InlineRefusal.RewrittenCallableBody);
            _remaining -= cost;
            _activeLambdas.Add(lambda);
            var parameters = new Dictionary<ParameterDef, TirNode>();
            for (var i = 0; i < invoke.Args.Count; i++)
                parameters[lambda.Parameters[i]] = Bind(Expand(invoke.Args[i], prefix), signature[i], prefix);
            var result = Fresh(invoke.Type);
            var locals = body.Descendants().OfType<TirLet>().ToDictionary(l => l.Def, l => Fresh(l.Value.Type));
            var clone = CloneBody(body, parameters, locals, result);
            RecordAssignments(clone);
            var enclosingAncestry = _ancestry.ToArray();
            if (_lambdaAncestry.TryGetValue(lambda, out var creationAncestry))
            {
                _ancestry.Clear();
                _ancestry.UnionWith(creationAncestry);
            }
            prefix.Add(new TirInlineRegion(result, Rewrite(clone), _ancestry.ToArray()));
            _ancestry.Clear();
            _ancestry.UnionWith(enclosingAncestry);
            _activeLambdas.Remove(lambda);
            if (_host.InlineReport != null) _host.InlineReport.KnownLambdasInlined++;
            _host.InlineReport?.RecordSite(_owner, _caller.Original, invoke, "known callable expanded");
            return new TirVariable(result, result.Type, invoke.Origin);
        }
    }
}
