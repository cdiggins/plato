using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace RecordGeometryExperiment;

// Private direct-return experiment. It is deliberately not an expression-tree rewrite.
internal static class Projection
{
    internal static bool TryRewrite(TirFunction function, out TirFunction result)
    {
        result = function;
        if (function.Body is not TirCall getter || getter.EmissionKind != EmissionKind.Property
            || getter.Args.Count != 1 || getter.Callee?.OwnerType is not { } owner
            || owner.IsPrimitiveDeclaration || owner.IsSum || owner.TypeParameters.Count != 0
            || getter.Args[0] is not TirCall constructor || constructor.EmissionKind != EmissionKind.Constructor
            || !ReferenceEquals(constructor.Callee?.OwnerType, owner)
            || !owner.CompilerGeneratedFunctions.Any(f => ReferenceEquals(f, constructor.Callee))
            || constructor.Callee.FunctionType != FunctionType.Constructor
            || constructor.Args.Count != owner.Fields.Count
            || constructor.ParameterTypes.Count != owner.Fields.Count
            || !constructor.Type.Equals(owner.ToTypeExpression()))
            return false;
        var index = owner.Fields.FindIndex(f => ReferenceEquals(f.Function, getter.Callee));
        if (index < 0 || !getter.Type.Equals(owner.Fields[index].Type))
            return false;
        for (var i = 0; i < owner.Fields.Count; ++i)
            if (!owner.Fields[i].Type.Equals(constructor.Args[i].Type)
                || !owner.Fields[i].Type.Equals(constructor.ParameterTypes[i]))
                return false;
        var names = function.Parameters.Select(p => p.Name).ToHashSet();
        names.UnionWith(function.AllNodes.OfType<TirVariable>().Select(v => v.Def.Name));
        names.UnionWith(function.AllNodes.OfType<TirLet>().Select(v => v.Def.Name));
        names.UnionWith(function.AllNodes.OfType<TirLambda>().SelectMany(v => v.Parameters).Select(p => p.Name));
        var statements = new List<TirNode>();
        TirVariable selected = null;
        for (var i = 0; i < constructor.Args.Count; ++i)
        {
            var name = "projectionField" + i;
            while (!names.Add(name)) name += "_";
            var value = constructor.Args[i];
            var local = new VariableDef(null, name, value.Type, null);
            statements.Add(new TirLet(local, value, value.Type, value.Origin));
            if (i == index) selected = new TirVariable(local, value.Type, value.Origin);
        }
        statements.Add(new TirReturn(selected, getter.Origin));
        result = new TirFunction(function.Original, function.Parameters, function.ReturnType,
            new TirBlock(statements, function.Body.Origin), function.ZonkedParameterTypes, function.ZonkedReturnType);
        return true;
    }
}
