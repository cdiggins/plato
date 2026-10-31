// Switches of the optimizing rewrites the elaborator applies to the TIR it builds (decoder wave P2).
namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// Pure optimizations that run on the TIR of each function as it is elaborated, so that the
    /// interpreter and every writer see the result. All are off by default, and an elaboration with
    /// the defaults is the one the compiler always made.
    /// </summary>
    /// <param name="BindMatchScrutinees">A <c>match</c> on a non-trivial expression evaluates it once,
    /// into a local declared before the statement (<see cref="TirScrutineeBinding"/>, TKT-1138).</param>
    /// <param name="SinkBindings">A <c>var</c> that only one arm of a conditional uses is evaluated on
    /// that arm (<see cref="TirSinkBindings"/>, TKT-1142).</param>
    public sealed record ElaborationOptions(bool BindMatchScrutinees = false, bool SinkBindings = false)
    {
        public bool Any => BindMatchScrutinees || SinkBindings;
    }

    /// <summary>What the rewrites of one or more elaborations changed, for reports and tests.</summary>
    public sealed class ElaborationStats
    {
        public int BoundScrutinees;
        public int SunkBindings;
    }
}
