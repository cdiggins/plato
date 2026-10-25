using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Combines adjacent total Number folds without changing either fold's arithmetic order.</summary>
public static class TirMultiConsumerFusion
{
    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.FuseConsumers || !writer.LowerLoops || tir?.Body is not TirBlock block)
            return tir;
        var nodes = tir.AllNodes.ToList();
        var assigned = nodes.OfType<TirAssign>().Select(a => a.LValue).Select(n => n switch
        {
            TirVariable v => (DefSymbol)v.Def, TirParameter p => p.Def, _ => null
        }).ToHashSet();
        var names = nodes.Select(n => n switch
        {
            TirVariable v => v.Def.Name, TirParameter p => p.Def.Name,
            TirLet l => l.Def.Name, TirInlineBinding b => b.Def.Name, TirInlineRegion r => r.Result.Name, _ => null
        }).Where(n => n != null).Concat(tir.Parameters.Select(p => p.Name))
            .Concat(nodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name)).ToList();
        var statements = new List<TirNode>();
        var id = 0;
        for (var i = 0; i < block.Statements.Count; i++)
        {
            var statement = block.Statements[i];
            statements.Add(statement);
            if (i + 2 >= block.Statements.Count || statement is not TirLet source
                || assigned.Contains(source.Def) || !OwnedArray(source.Value, writer)
                || block.Statements[i + 1] is not TirLet first
                || block.Statements[i + 2] is not TirLet second
                || !Reduction(first, source.Def, tir, assigned, writer)
                || !Reduction(second, source.Def, tir, assigned, writer))
                continue;
            while (names.Any(n => n.StartsWith($"_mc{id}_"))) id++;
            var receiver = (TirVariable)((TirCall)first.Value).Args[0];
            statements.Add(new TirMultiReduction(receiver, first, second, id++));
            i += 2;
        }
        if (statements.Count == block.Statements.Count)
            return tir;
        return new TirFunction(tir.Original, tir.Parameters, tir.ReturnType,
            new TirBlock(statements, block.Origin), tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static bool TypeIs(TypeExpression type, CSharpWriter writer, string name, int arity = 0)
        => type != null && type.TypeArgs.Count == arity
           && writer.Compilation.TypeDefinitionsByName.TryGetValue(name, out var definition)
           && definition.IsPrimitiveDeclaration && ReferenceEquals(type.Def, definition);

    private static bool Number(TypeExpression type, CSharpWriter writer)
        => TypeIs(type, writer, "Number");

    private static bool Numbers(TypeExpression type, CSharpWriter writer)
        => TypeIs(type, writer, "Array", 1) && Number(type.TypeArgs[0], writer);

    private static bool Intrinsic(TirCall call, CSharpWriter writer, string name)
        => call.Name == name && TirIntrinsicIdentity.IsIntrinsic(call.Callee, writer.Compilation, name);

    private static bool OwnedArray(TirNode node, CSharpWriter writer)
        => Numbers(node.Type, writer) && (node is TirArray
           || node is TirCall call && Intrinsic(call, writer, "MapRange") && call.Args.Count == 2
           && TypeIs(call.Args[0].Type, writer, "Integer")
           && call.ParameterTypes.Count == 2 && call.ParameterTypes.SequenceEqual(call.Args.Select(a => a.Type))
           && Equals(call.ReturnType, call.Type));

    private static bool Reduction(TirLet binding, VariableDef source, TirFunction function,
        HashSet<DefSymbol> assigned, CSharpWriter writer)
        => binding.Value is TirCall call && Intrinsic(call, writer, "Reduce")
           && Number(call.Type, writer) && Equals(call.ReturnType, call.Type)
           && call.Args.Count == 3 && call.ParameterTypes.Count == 3
           && call.ParameterTypes.SequenceEqual(call.Args.Select(a => a.Type))
           && call.Args[0] is TirVariable receiver && ReferenceEquals(receiver.Def, source)
           && Numbers(receiver.Type, writer) && call.Args[1] is TirLiteral seed && Number(seed.Type, writer)
           && call.Args[2] is TirLambda lambda && lambda.Parameters.Count == 2
           && TypeIs(lambda.Type, writer, "Function2", 3)
           && System.Linq.Enumerable.All(lambda.Type.TypeArgs, t => Number(t, writer))
           && Total(lambda.Body, function.Parameters.Concat(lambda.Parameters).ToHashSet(), assigned, writer);

    private static bool Total(TirNode node, HashSet<ParameterDef> parameters,
        HashSet<DefSymbol> assigned, CSharpWriter writer)
        => Number(node.Type, writer) && (node switch
        {
            TirLiteral => true,
            TirParameter p => parameters.Contains(p.Def) && !assigned.Contains(p.Def),
            TirCall call => (Intrinsic(call, writer, "Add") || Intrinsic(call, writer, "Multiply"))
                && call.Args.Count == 2 && call.ParameterTypes.Count == 2
                && System.Linq.Enumerable.All(call.ParameterTypes, t => Number(t, writer)) && Number(call.ReturnType, writer)
                && System.Linq.Enumerable.All(call.Args, a => Total(a, parameters, assigned, writer)),
            _ => false
        });
}
