using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

internal static class TirLoopCallbacks
{
    public static TirLoweredLoop Freshen(TirLoweredLoop loop, Func<TypeExpression, VariableDef> fresh,
        Func<TirNode, TirNode> rewrite)
    {
        TirNode Clone(TirNode callback) => TirRewrite.StripCoerce(callback) is TirLambda
            ? rewrite(TirInlineCloner.Clone(callback, new Dictionary<ParameterDef, TirNode>(), null, fresh)) : callback;
        return new TirLoweredLoop(loop.Kind, loop.Sources, Clone(loop.Fn), loop.Seed,
            loop.IncludeFirst, loop.TempName, loop.Id, loop.Type, loop.Origin, loop.SourceFunction)
        {
            ElemType = loop.ElemType,
            IndexSource = loop.IndexSource,
            SourceStages = loop.SourceStages.Select(Clone).ToArray()
        };
    }
}
