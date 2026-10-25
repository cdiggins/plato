using System.Linq;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Proves literal callback width without evaluating or sampling callbacks.</summary>
public static class TirExactOutput
{
    // The exact and fallback branches each render the callback once.
    public const int MaximumCallbackNodes = 256;

    private static bool Concrete(TypeExpression type)
        => TirLoopLowerer.IsRenderableConcrete(type) && !type.IsTypeParameter
            && !type.IsSelfType() && type.TypeArgs.All(Concrete);

    public static bool TryGetWidth(TirLoweredLoop loop, Compilation compilation, out int width)
    {
        width = 0;
        var lambda = loop.Fn as TirLambda;
        var array = lambda?.Body as TirArray;
        if (loop.Kind != "FlatMap" || loop.IndexSource || loop.SourceStages.Count != 0
            || loop.Sources.Count != 1 || loop.Seed != null || loop.IncludeFirst != null
            || !TirIntrinsicIdentity.IsIntrinsic(loop.SourceFunction, compilation, "FlatMap")
            || lambda == null || array == null
            || lambda.Parameters.Count != 1 || lambda.Type?.Name != "Function1"
            || lambda.Type.TypeArgs.Count != 2 || !Concrete(loop.ElemType)
            || array.Descendants().Take(MaximumCallbackNodes + 1).Count() > MaximumCallbackNodes
            || !TirRewrite.IsListType(array.Type) || !array.Type.Equals(loop.Type)
            || !array.Type.Equals(lambda.Type.TypeArgs[1])
            || !loop.ElemType.Equals(TirRewrite.ListElementType(array.Type))
            || array.Elements.Any(e => !loop.ElemType.Equals(e.Type)))
            return false;
        var input = TirRewrite.ListElementType(loop.Sources[0].Type);
        // The instantiated lambda signature is authoritative; ParameterDef can retain solver variables.
        if (!Concrete(input) || !input.Equals(lambda.Type.TypeArgs[0]))
            return false;
        width = array.Elements.Count;
        return true;
    }
}
