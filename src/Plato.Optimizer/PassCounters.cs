// The per-thread counters behind the temporaries the passes name, scoped to one body.
using System;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The passes and the body writers number their temporaries from per-thread counters: the inliner's renames
/// (<c>{p}_{N}</c>), fusion's (<c>_f</c>/<c>_r</c>) and the loop lowerer's locals, and the capture rewriter's
/// <c>_var{N}</c> (<see cref="SymbolRewriter.NextId"/>). TKT-1137: they restart for every body, so editing one
/// function cannot renumber another's. TKT-1209: every counter lives here, and each is <c>[ThreadStatic]</c>, so the
/// Cpu prepass's worker threads number their bodies independently and a name is a function of the body alone; a
/// counter shared across threads, Interlocked or not, would make the names depend on scheduling.
/// <see cref="Scope"/> restarts them and restores the caller's values when disposed.
/// </summary>
public static class PassCounters
{
    public static void Reset()
    {
        SymbolRewriter.NextId = 0;
        TirInliner.NextRenameId = 0;
        TirFusion.NextId = 0;
        TirLoopLowerer.NextId = 0;
    }

    /// <summary>The four counters as one value.</summary>
    public readonly struct Saved : IEquatable<Saved>
    {
        private readonly int _symbol, _inliner, _fusion, _loops;
        private Saved(int symbol, int inliner, int fusion, int loops) => (_symbol, _inliner, _fusion, _loops) = (symbol, inliner, fusion, loops);
        public static Saved Take() => new(SymbolRewriter.NextId, TirInliner.NextRenameId, TirFusion.NextId, TirLoopLowerer.NextId);
        public void Restore() => (SymbolRewriter.NextId, TirInliner.NextRenameId, TirFusion.NextId, TirLoopLowerer.NextId) = (_symbol, _inliner, _fusion, _loops);
        public bool Equals(Saved o) => (_symbol, _inliner, _fusion, _loops) == (o._symbol, o._inliner, o._fusion, o._loops);
        public override bool Equals(object o) => o is Saved s && Equals(s);
        public override int GetHashCode() => (_symbol, _inliner, _fusion, _loops).GetHashCode();
        public override string ToString() => $"symbol={_symbol} inliner={_inliner} fusion={_fusion} loops={_loops}";
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
