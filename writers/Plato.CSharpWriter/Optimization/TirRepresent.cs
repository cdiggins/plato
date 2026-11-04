// The writer-only markers of the storage layer: the one node that changes representation, and a twin call.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Emission-only marker (--owned-arrays): <see cref="Inner"/> held as <see cref="From"/> is converted
/// to <see cref="To"/>. The only node that changes an array's representation; the conversions it may carry are
/// the table of <see cref="CSharpStorageRules.MayConvert"/>, and the body writer prints each mechanically.</summary>
public sealed class TirRepresent : TirNode
{
    public TirNode Inner { get; }
    public ArrayStorage From { get; }
    public ArrayStorage To { get; }

    public TirRepresent(TirNode inner, ArrayStorage from, ArrayStorage to) : base(inner?.Type, inner?.Origin)
        => (Inner, From, To) = (inner, from, to);

    public override IEnumerable<TirNode> Children => new[] { Inner };

    public override TirNode WithChildren(IReadOnlyList<TirNode> children) => new TirRepresent(children[0], From, To);

    public override string ToString() => $"represent<{From}->{To}>({Inner})";
}

/// <summary>Emission-only marker (--span-params, later chunks): a call routed to a span twin of its callee.
/// <see cref="Borrowed"/> says which arguments the twin takes as a <c>ReadOnlySpan&lt;T&gt;</c>.
/// Defined here, with the contract, so the checker's S6 rule can name it.</summary>
public sealed class TirTwinCall : TirNode
{
    public TirCall Call { get; }
    public string TwinClass { get; }
    public string TwinName { get; }
    public IReadOnlyList<bool> Borrowed { get; }

    public TirTwinCall(TirCall call, string twinClass, string twinName, IReadOnlyList<bool> borrowed)
        : base(call?.Type, call?.Origin)
        => (Call, TwinClass, TwinName, Borrowed) = (call, twinClass, twinName, borrowed);

    public override IEnumerable<TirNode> Children => Call.Args;

    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
        => new TirTwinCall((TirCall)Call.WithChildren(children), TwinClass, TwinName, Borrowed);

    public override string ToString() => $"twin:{TwinClass}.{TwinName}({string.Join(", ", Call.Args)})";
}
