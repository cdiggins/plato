// Marks an array update the in-place pass proved unobservable, for the C# body writer.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Emission-only marker (--in-place, TKT-1132): <c>SetAt(array, index, value)</c> whose array
/// <see cref="TirInPlaceUpdates"/> proved to be unshared and not used again as the old value. The body
/// writer prints it as <c>Intrinsics.SetAtInPlace(array, index, value)</c>, which writes the element
/// into the array's own storage and returns the same array. The Plato meaning is unchanged, and the
/// interpreter still builds a new array.</summary>
public class TirInPlaceSet : TirNode
{
    public TirNode Array { get; }
    public TirNode Index { get; }
    public TirNode Value { get; }

    public TirInPlaceSet(TirNode array, TirNode index, TirNode value, TypeExpression type, Symbol origin)
        : base(type, origin) => (Array, Index, Value) = (array, index, value);

    public override IEnumerable<TirNode> Children => new[] { Array, Index, Value };

    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirInPlaceSet(children[0], children[1], children[2], Type, Origin);

    public override string ToString() => $"inplace:SetAt({Array}, {Index}, {Value})";
}
