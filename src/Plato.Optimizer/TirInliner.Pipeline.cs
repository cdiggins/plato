using System;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

public static partial class TirInliner
{
    private sealed class StatementPipeline
    {
        private readonly TirFunction _input;
        private readonly ITirInlineHost _host;
        private readonly object _info;
        private readonly bool _simplify;
        private readonly InlineExpansionBudget _budget;
        private readonly StatementExpansion _expansion;

        public StatementPipeline(TirFunction input, ITirInlineHost host, string owner,
            bool eliminateLambdas, bool simplify, object info, TirFacts facts)
        {
            (_input, _host, _info, _simplify) = (input, host, info, simplify);
            var limit = host is IStatementInlineHost w ? Math.Min(w.StatementNodeBudget, w.RemainingStatementGrowth)
                : InlineExpansionBudget.DefaultGrowth;
            _budget = new InlineExpansionBudget(limit);
            _expansion = new StatementExpansion(input, host, owner, eliminateLambdas, simplify, _budget, facts);
        }

        private TirFunction WithBody(TirNode body)
            => new(_input.Original, _input.Parameters, _input.ReturnType, body,
                _input.ZonkedParameterTypes, _input.ZonkedReturnType);

        private TirNode Expose(TirNode body)
        {
            if (!_simplify || _host is not IStatementInlineHost writer)
                return body;
            return writer.ExposeForStatementInlining(WithBody(body), _info).Body;
        }

        public TirFunction Run()
        {
            var body = _input.Body;
            var initialSize = body.Descendants().Count();
            var calls = _host.InlineReport?.StatementCallsInlined ?? 0;
            var lambdas = _host.InlineReport?.KnownLambdasInlined ?? 0;
            var growth = 0;
            for (var pass = 0; pass < (_simplify ? InlineExpansionBudget.MaxPasses : 1); pass++)
            {
                var exposed = Expose(body);
                var changed = !ReferenceEquals(exposed, body);
                var version = _expansion.Version;
                body = _expansion.Finish(_expansion.Rewrite(exposed));
                growth = Math.Max(0, body.Descendants().Count() - initialSize);
                if (!_budget.AcceptGrowth(growth))
                    return RollBack(calls, lambdas);
                if (!changed && version == _expansion.Version)
                    break;
            }
            if (_host is IStatementInlineHost writer)
                writer.RemainingStatementGrowth -= growth;
            return ReferenceEquals(body, _input.Body) ? _input : WithBody(body);
        }

        private TirFunction RollBack(int calls, int lambdas)
        {
            if (_host.InlineReport != null)
            {
                _host.InlineReport.StatementCallsInlined = calls;
                _host.InlineReport.KnownLambdasInlined = lambdas;
            }
            _host.InlineReport?.Refuse(_input.Original?.Name, "statement expansion", InlineRefusal.ExpansionBudget);
            return _input;
        }
    }
}
