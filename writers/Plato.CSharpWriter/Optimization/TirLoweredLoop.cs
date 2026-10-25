// Replaces supported array combinators with loop markers and temporary references.
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>Emission-only marker (--loops): a recognized array-combinator call site lowered to a
/// for-loop STATEMENT that fills <see cref="TempName"/>; the original call position holds a
/// <see cref="TirTempRef"/> to it. Printed by TirCSharpBodyWriter.WriteLoweredLoop.</summary>
public class TirLoweredLoop : TirNode
{
    public string Kind { get; }                    // Map, MapIdx, MapRange, Zip2, Zip3, Reduce, All, Any, FlatMap, Reverse, WithNext, MapPairs, MapTriplets, MapQuartets
    public IReadOnlyList<TirNode> Sources { get; } // list expressions (the count expression for MapRange)
    public TirNode Fn { get; }                     // lambda literal or delegate-typed parameter/variable (null for Reverse)
    public TirNode Seed { get; }                   // Reduce only
    public TirNode IncludeFirst { get; }           // WithNext only
    // Fused producer chain (TKT-506): the stages (arity-1 lambdas or delegates) applied in order
    // to the base element to get the element the loop body sees, each bound to a local inside the
    // loop; no intermediate array exists. With IndexSource, Sources[0] is a COUNT and the base
    // element is the index itself (the receiver was a MapRange, the form TirFusion leaves for
    // n.Range and xs.Take(k)); otherwise it is Sources[0][i] (the receiver was a Map chain).
    public IReadOnlyList<TirNode> SourceStages { get; set; } = System.Array.Empty<TirNode>();
    public bool IndexSource { get; set; }
    public string TempName { get; }
    public int Id { get; }
    public FunctionDef SourceFunction { get; }
    // The RESULT element type, taken from the producing function's own type (the call's zonked
    // return type can be looser than the produced elements).
    public TypeExpression ElemType { get; set; }

    public TirLoweredLoop(string kind, IReadOnlyList<TirNode> sources, TirNode fn, TirNode seed,
        TirNode includeFirst, string tempName, int id, TypeExpression type, Symbol origin, FunctionDef sourceFunction = null)
        : base(type, origin)
    {
        Kind = kind;
        Sources = sources ?? new List<TirNode>();
        Fn = fn;
        Seed = seed;
        IncludeFirst = includeFirst;
        TempName = tempName;
        Id = id;
        SourceFunction = sourceFunction;
    }

    public override IEnumerable<TirNode> Children
        => Sources.Concat(new[] { Fn, Seed, IncludeFirst }.Where(n => n != null)).Concat(SourceStages);

    public override TirNode WithChildren(IReadOnlyList<TirNode> children)
    {
        var i = Sources.Count;
        TirNode Next(TirNode original) => original == null ? null : children[i++];
        var rebuilt = new TirLoweredLoop(Kind, children.Take(Sources.Count).ToList(),
            Next(Fn), Next(Seed), Next(IncludeFirst), TempName, Id, Type, Origin, SourceFunction)
            { ElemType = ElemType, IndexSource = IndexSource };
        rebuilt.SourceStages = children.Skip(i).ToList();
        return rebuilt;
    }

    public override string ToString() => $"loop:{Kind} -> {TempName}";
}

