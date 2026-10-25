using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>An explicitly typed local preserving an inlined call's conversion boundary.</summary>
public sealed class TirInlineBinding : TirNode
{
    public VariableDef Def { get; }
    public TirNode Value { get; }

    public TirInlineBinding(VariableDef def, TirNode value)
        : base(def.Type, value.Origin)
        => (Def, Value) = (def, value);

    public override IEnumerable<TirNode> Children => new[] { Value };

    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirInlineBinding(Def, children[0]);

    public override string ToString()
        => $"{Type} {Def.Name} = {Value};";
}
