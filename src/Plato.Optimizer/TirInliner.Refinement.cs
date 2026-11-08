// Expands one call for the sum refinement pass: a callee that is one match on a parameter whose argument's cases are known.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.Optimizer;

public static partial class TirInliner
{
    /// <summary>The body limit for a callee the refinement pass expands: the whole ten-arm evaluator may come in, since the
    /// fold that follows keeps one arm of it.</summary>
    internal const int RefinementBodyNodes = 1200;

    /// <summary>The callee body <see cref="TirSumRefinement"/> would see if it expanded <paramref name="call"/>: the
    /// ground specialization for the receiver's trusted type, or null.</summary>
    internal static TirFunction GroundCallee(TirCall call, TirFunction callerTir, ITirInlineHost host, string ownerTypeName)
    {
        if (call?.Callee == null || call.Args.Count == 0)
            return null;
        var receiverTypeName = TrustedTypeName(callerTir, call.Args[0], host, ownerTypeName);
        return host.TryGetGroundTirByTypeName(call.Callee, receiverTypeName);
    }

    /// <summary>One call expanded under the inliner's rules except the body size, which is <see cref="RefinementBodyNodes"/>:
    /// the refinement pass asks for it only when the callee's root match is on a parameter whose argument's cases it
    /// knows, so most of the expanded body is dead on arrival.</summary>
    internal static bool TryInlineForRefinement(TirCall call, TirFunction callerTir, ITirInlineHost host,
        string ownerTypeName, bool insideLambda, HashSet<string> callerNames, out TirNode inlined)
    {
        inlined = null;
        if (call.Args.Count == 0)
            return false;
        return TryInlineCall(call, callerTir, host, ownerTypeName, insideLambda, callerNames,
            new HashSet<(object Callee, string ReceiverType)>(), new List<(object, string)>(), forHoisting: false,
            RefinementBodyNodes, allowSumSyntactic: true, out inlined);
    }
}
