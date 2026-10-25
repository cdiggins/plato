using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Two proven total reductions over one already materialized source.</summary>
public sealed class TirMultiReduction : TirNode
{
    public TirVariable Source { get; }
    public TirLet First { get; }
    public TirLet Second { get; }
    public int Id { get; }

    public TirMultiReduction(TirVariable source, TirLet first, TirLet second, int id)
        : base(null, first.Origin)
        => (Source, First, Second, Id) = (source, first, second, id);

    public override IEnumerable<TirNode> Children
        => new TirNode[] { Source, First, Second };

    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirMultiReduction((TirVariable)children[0], (TirLet)children[1], (TirLet)children[2], Id);

    public override string ToString()
        => $"paired-reduce({First}; {Second})";
}
