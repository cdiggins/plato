// Expands one call for the loop-invariant hoister under the inliner's under-lambda rules.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

public static partial class TirInliner
{
    /// <summary>Every name bound in <paramref name="tir"/>, for <see cref="TryInlineForHoisting"/>.</summary>
    internal static HashSet<string> BoundNamesForHoisting(TirFunction tir)
        => CollectBoundNames(tir, tir.Body);

    /// <summary>One call site in a lowered loop's callback expanded as the inliner would expand a
    /// call under a lambda (lambda-free callee, cheap arguments, expression body within budget),
    /// independent of --inline. Used by <see cref="TirLoopInvariantHoister"/>, which keeps the
    /// expansion only when it exposes an invariant expression.</summary>
    internal static bool TryInlineForHoisting(TirCall call, TirFunction callerTir, ITirInlineHost host,
        string ownerTypeName, HashSet<string> callerNames, out TirNode inlined)
    {
        inlined = null;
        if (call.Args.Count == 0)
            return false;
        // The hoister's entry carries no table, so the callee's parameter uses are read from a table made here:
        // one walk of the callee body per call, what the counter it replaces cost (compiler-546).
        return TryInlineCall(call, callerTir, host, ownerTypeName, insideLambda: true, callerNames,
            new HashSet<(object Callee, string ReceiverType)>(), new List<(object, string)>(), forHoisting: true,
            new TirFacts(OptimizerNodeModel.Instance), out inlined);
    }
}
