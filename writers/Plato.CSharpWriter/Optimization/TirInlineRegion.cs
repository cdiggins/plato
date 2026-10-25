using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>A local result and exit label; returning from this region cannot exit its caller.</summary>
public sealed class TirInlineRegion : TirNode
{
    public VariableDef Result { get; }
    public TirNode Body { get; }
    public string ExitLabel => Result.Name + "_exit";

    public TirInlineRegion(VariableDef result, TirNode body)
        : base(result.Type, body.Origin)
        => (Result, Body) = (result, body);

    public override IEnumerable<TirNode> Children => new[] { Body };
    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirInlineRegion(Result, children[0]);
    public override string ToString() => $"region {Result.Name}: {Body}";
}

public sealed class TirInlineExit : TirNode
{
    public VariableDef Result { get; }
    public TirNode Value { get; }

    public TirInlineExit(VariableDef result, TirNode value)
        : base(result.Type, value.Origin)
        => (Result, Value) = (result, value);

    public override IEnumerable<TirNode> Children => new[] { Value };
    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirInlineExit(Result, children[0]);
    public override string ToString() => $"exit {Result.Name} = {Value}";
}
