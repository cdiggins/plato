// TKT-1209: PassCounters owns every temporary counter, including SymbolRewriter.NextId, and each is per thread.
using System.Threading;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Optimizer;
using NUnit.Framework;

namespace PlatoTests.Optimization;

[TestFixture]
public static class PassCountersTests
{
    private static void Set(int symbol, int inliner, int fusion, int loops)
    {
        SymbolRewriter.NextId = symbol;
        TirInliner.NextRenameId = inliner;
        TirFusion.NextId = fusion;
        TirLoopLowerer.NextId = loops;
    }

    private static (int, int, int, int) Read()
        => (SymbolRewriter.NextId, TirInliner.NextRenameId, TirFusion.NextId, TirLoopLowerer.NextId);

    [Test]
    public static void ScopeRestartsAllFourCountersAndRestoresThem()
    {
        Set(5, 6, 7, 8);
        var before = PassCounters.Saved.Take();
        using (PassCounters.Scope())
        {
            Assert.That(Read(), Is.EqualTo((0, 0, 0, 0)), "the scope restarts every counter, the symbol rewriter's included");
            Set(1, 2, 3, 4);
        }
        Assert.That(Read(), Is.EqualTo((5, 6, 7, 8)), "disposing the scope restores the caller's values");
        Assert.That(PassCounters.Saved.Take(), Is.EqualTo(before));
        PassCounters.Reset();
        Assert.That(Read(), Is.EqualTo((0, 0, 0, 0)));
    }

    [Test]
    public static void AnotherThreadsScopeDoesNotTouchThisThreadsCounters()
    {
        Set(11, 12, 13, 14);
        (int, int, int, int) onWorker = default;
        var worker = new Thread(() =>
        {
            using (PassCounters.Scope())
            {
                Set(100, 200, 300, 400);
                onWorker = Read();
            }
        });
        worker.Start();
        worker.Join();
        Assert.That(onWorker, Is.EqualTo((100, 200, 300, 400)));
        Assert.That(Read(), Is.EqualTo((11, 12, 13, 14)), "the counters are per thread, so a worker's body numbering is its own");
        PassCounters.Reset();
    }
}
