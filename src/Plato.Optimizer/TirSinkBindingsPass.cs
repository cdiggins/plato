// Runs the shared sinking rewrite on a function after inlining (--sink-bindings, TKT-1142).
using System.Threading;
using Ara3D.Geometry.Compiler.Checking;
namespace Ara3D.Geometry.Optimizer;

/// <summary>The sites the C# writer's own rewrites changed (the elaboration counts are in
/// <see cref="ElaborationStats"/>).</summary>
public sealed class WriterRewriteStats
{
    public int SunkAfterInlining;
    public int SharedAcrossStatements;
    public int SharedInLambdas;
}

/// <summary>
/// The elaborator sinks the locals of each function as it builds the TIR (<see cref="TirSinkBindings"/>);
/// inlining then brings the locals of callees into the caller, and this pass sinks those too. It runs
/// before the shared-calls pass so that a call a sunk local holds is still one call.
/// </summary>
public static class TirSinkBindingsPass
{
    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On("sink-bindings") || tir?.Body == null)
            return tir;
        var body = TirSinkBindings.Rewrite(tir.Body, out var sunk);
        Interlocked.Add(ref pass.Stats.Rewrites.SunkAfterInlining, sunk);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }
}
