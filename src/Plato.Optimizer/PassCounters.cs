// The per-thread counters behind the temporaries the passes name, scoped to one body.
using System;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The passes number their temporaries from per-thread counters (the inliner's renames, fusion's and the loop
/// lowerer's locals). TKT-1137: they restart for every body, so editing one function cannot renumber another's.
/// <see cref="Scope"/> restarts them and restores the caller's values when disposed.
/// </summary>
public static class PassCounters
{
    public static void Reset()
    {
        TirInliner.NextRenameId = 0;
        TirFusion.NextId = 0;
        TirLoopLowerer.NextId = 0;
    }

    /// <summary>The three counters as one value.</summary>
    public readonly struct Saved
    {
        private readonly int _inliner, _fusion, _loops;
        private Saved(int inliner, int fusion, int loops) => (_inliner, _fusion, _loops) = (inliner, fusion, loops);
        public static Saved Take() => new(TirInliner.NextRenameId, TirFusion.NextId, TirLoopLowerer.NextId);
        public void Restore() => (TirInliner.NextRenameId, TirFusion.NextId, TirLoopLowerer.NextId) = (_inliner, _fusion, _loops);
    }

    /// <summary>Restarts the counters for one body; disposing the result restores the caller's values.</summary>
    public static IDisposable Scope()
    {
        var saved = Saved.Take();
        Reset();
        return new Restorer(saved);
    }

    private sealed class Restorer : IDisposable
    {
        private readonly Saved _saved;
        public Restorer(Saved saved) => _saved = saved;
        public void Dispose() => _saved.Restore();
    }
}
