using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Freshens lexical identities and redirects only the inlined function's returns.</summary>
public static class TirInlineCloner
{
    public static TirNode RedirectReturns(TirNode body, VariableDef result)
    {
        var depth = 0;
        return TirRewrite.Rewrite(body, n => n is TirReturn r && depth == 0
            ? new TirInlineExit(result, r.Value) : n,
            n => { if (n is TirLambda) depth++; }, n => { if (n is TirLambda) depth--; });
    }

    public static TirNode Clone(TirNode body, IReadOnlyDictionary<ParameterDef, TirNode> arguments,
        VariableDef result, Func<TypeExpression, VariableDef> fresh)
    {
        var nodes = body.Descendants().ToArray();
        var locals = new Dictionary<VariableDef, VariableDef>();
        TypeExpression ValueType(TirNode value) => value switch
        {
            TirVariable v when locals.TryGetValue(v.Def, out var local) => local.Type,
            TirParameter p when arguments.TryGetValue(p.Def, out var argument) => argument.Type,
            _ => value.Type
        };
        foreach (var node in nodes)
        {
            var binding = node switch
            {
                TirLet l => (Def: l.Def, Type: ValueType(l.Value)),
                TirInlineBinding b => (Def: b.Def, Type: b.Def.Type),
                TirInlineRegion r => (Def: r.Result, Type: r.Result.Type),
                _ => (Def: (VariableDef)null, Type: (TypeExpression)null)
            };
            if (binding.Def != null && !locals.ContainsKey(binding.Def))
                locals.Add(binding.Def, fresh(binding.Type));
        }
        var parameters = nodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Distinct()
            .ToDictionary(p => p, p => new ParameterDef(p.Scope, fresh(p.Type).Name, p.Type, p.Index));
        var lambdaDepth = 0;
        return TirRewrite.Rewrite(body, n => n switch
        {
            TirParameter p when arguments.TryGetValue(p.Def, out var value) => value,
            TirParameter p when parameters.TryGetValue(p.Def, out var renamed) => new TirParameter(renamed, p.Type, p.Origin),
            TirVariable v when locals.TryGetValue(v.Def, out var local) => new TirVariable(local, local.Type ?? v.Type, v.Origin),
            TirLet l => new TirLet(locals[l.Def], l.Value, l.Type, l.Origin),
            TirInlineBinding b => new TirInlineBinding(locals[b.Def], b.Value),
            TirInlineRegion r => new TirInlineRegion(locals[r.Result], r.Body, r.Ancestry),
            TirInlineExit e => new TirInlineExit(locals[e.Result], e.Value),
            TirLambda l => new TirLambda(l.Parameters.Select(p => parameters[p]).ToArray(), l.Body, l.Type, l.Origin),
            TirReturn r when lambdaDepth == 0 => new TirInlineExit(result, r.Value),
            _ => n
        }, n => { if (n is TirLambda) lambdaDepth++; }, n => { if (n is TirLambda) lambdaDepth--; });
    }
}

