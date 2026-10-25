using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Statements that execute once, in order, immediately before their value.</summary>
public sealed class TirEvaluation
{
    public IReadOnlyList<TirNode> Statements { get; }
    public TirNode Value { get; }

    public TirEvaluation(IReadOnlyList<TirNode> statements, TirNode value)
        => (Statements, Value) = (statements.ToArray(), value);

    public TirNode AppendTo(List<TirNode> statements)
    {
        statements.AddRange(Statements);
        return Value;
    }

    public static TirEvaluation Conditional(TirConditional node, TirNode condition,
        TirEvaluation yes, TirEvaluation no, Func<TypeExpression, VariableDef> fresh,
        IReadOnlyList<FunctionDef> ancestry = null)
    {
        if (yes.Statements.Count == 0 && no.Statements.Count == 0)
            return new TirEvaluation(Array.Empty<TirNode>(),
                new TirConditional(condition, yes.Value, no.Value, node.Type, node.Origin));
        var result = fresh(node.Type);
        TirNode Branch(TirEvaluation value) => new TirBlock(value.Statements
            .Append(new TirInlineExit(result, value.Value)).ToArray(), node.Origin);
        var body = new TirIf(condition, Branch(yes), Branch(no), node.Origin);
        return new TirEvaluation(new[] { new TirInlineRegion(result, body, ancestry) },
            new TirVariable(result, result.Type, node.Origin));
    }

    public static TirEvaluation Capture(TirNode node, Func<TirNode, List<TirNode>, TirNode> rewrite)
    {
        var statements = new List<TirNode>();
        var value = rewrite(node, statements);
        return new TirEvaluation(statements, value);
    }

    /// <summary>Earlier operands must finish before a later operand's prerequisites execute.</summary>
    public static IReadOnlyList<TirNode> Sequence(IReadOnlyList<TirEvaluation> operands,
        List<TirNode> statements, Func<TirNode, List<TirNode>, TirNode> bind)
    {
        if (operands.All(p => p.Statements.Count == 0))
            return operands.Select(p => p.Value).ToArray();
        var values = new TirNode[operands.Count];
        for (var i = 0; i < operands.Count; i++)
        {
            statements.AddRange(operands[i].Statements);
            values[i] = bind(operands[i].Value, statements);
        }
        return values;
    }
}
