// Carries the storage plan from the resolver to the body writer around a function body.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Emission-only root marker (--owned-arrays): the body the resolver covered and its
/// <see cref="StoragePlan"/>. The body writer unwraps it before printing; no pass sees it.</summary>
public sealed class TirStorageScope : TirNode
{
    public TirNode Body { get; }
    public StoragePlan Plan { get; }

    public TirStorageScope(TirNode body, StoragePlan plan) : base(body?.Type, body?.Origin)
        => (Body, Plan) = (body, plan);

    public override IEnumerable<TirNode> Children => new[] { Body };

    public override TirNode WithChildren(IReadOnlyList<TirNode> children) => new TirStorageScope(children[0], Plan);

    public override string ToString() => $"scope({Plan}) {Body}";
}
