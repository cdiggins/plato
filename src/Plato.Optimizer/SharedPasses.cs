// The backend-neutral optimizer passes, by name: what a backend's pass list may name without supplying the pass.
using System.Collections.Generic;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The passes every backend can run. Each gates itself on its switches (named beside it), so naming a pass in a
/// backend's list costs nothing while its switch is off. Porting a pass to a backend is adding its name to that
/// backend's list and the backend's target to the switch's row in the CLI table (and an emitter for any node the
/// pass introduces that the backend cannot print yet).
/// </summary>
public static class SharedPasses
{
    /// <summary>β-reduction of small callees (the inline host's <c>InlineCalls</c>, or scalar-only bodies).</summary>
    public const string Inline = "inline";
    /// <summary>--scalar-arrays: the constant Count of a fixed-arity struct folded and its component arrays unrolled.</summary>
    public const string ConstantCounts = "constant-counts";
    /// <summary>--fuse: combinator chains fused, literal ranges unrolled.</summary>
    public const string Fuse = "fuse";
    /// <summary>--fuse-consumers (with --loops): independent reductions of one array in one loop.</summary>
    public const string FuseConsumers = "fuse-consumers";
    /// <summary>--optimize-arrays: multi-consumed Map results materialised.</summary>
    public const string MaterializeArrays = "materialize-arrays";
    /// <summary>--sink-bindings: a local only one arm reads moves into that arm (after inlining).</summary>
    public const string SinkBindings = "sink-bindings";
    /// <summary>--share-calls (--share-calls-wide): a call a statement repeats is computed once.</summary>
    public const string ShareCalls = "share-calls";
    /// <summary>--loops (--lower-more, --closure-free): combinator call sites lowered to loops.</summary>
    public const string LowerLoops = "lower-loops";
    /// <summary>--presize: counted List fills get their capacity.</summary>
    public const string Presize = "presize";
    /// <summary>--in-place: last-use array updates written in place.</summary>
    public const string InPlace = "in-place";
    /// <summary>--hoist-invariants (with --loops): loop-invariant callback work moved out of loops.</summary>
    public const string HoistInvariants = "hoist-invariants";

    public static IReadOnlyList<ITirPass> All { get; } = new ITirPass[]
    {
        new TirPass(Inline, "inline (layer 7)", (tir, ctx) => TirInliner.Inline(tir, ctx.InlineHost, ctx.OwnerTypeName, out _)),
        new TirPass(ConstantCounts, "scalar-arrays / constant counts and component arrays", Ara3D.Geometry.Optimizer.ConstantCounts.Rewrite),
        new TirPass(Fuse, "fuse / combinator fusion (layer 7b)", TirFusion.Rewrite),
        new TirPass(FuseConsumers, "fuse-consumers / independent reductions", TirMultiConsumerFusion.Rewrite),
        new TirPass(MaterializeArrays, "optimize-arrays / materialize (layer 9)", TirArrayMaterializer.Rewrite),
        new TirPass(SinkBindings, "sink-bindings / a local only one arm reads (TKT-1142)", TirSinkBindingsPass.Rewrite),
        new TirPass(ShareCalls, "share-calls / a call a statement repeats (TKT-1136)", TirSharedCalls.Rewrite),
        new TirPass(LowerLoops, "loops / lower (layer 10)", TirLoopLowerer.Rewrite),
        new TirPass(Presize, "presize / counted List fills", TirPresize.Rewrite),
        new TirPass(InPlace, "in-place / last-use array updates (TKT-1132)", TirInPlaceUpdates.Rewrite),
        new TirPass(HoistInvariants, "hoist-invariants / loop-invariant callback work", TirLoopInvariantHoister.Rewrite),
    };
}
