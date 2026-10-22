using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Ara3D.Geometry.Compiler.Symbols;
using NUnit.Framework;

namespace PlatoTests
{
    /// <summary>
    /// Symbols are equal when their Ids are, so two symbols must never share one. The warm law
    /// server compiles several programs at once, one per folder set; with a plain NextId++ two of
    /// them could hand out the same Id, and an expression whose child had its Id was taken for its
    /// own child: "Circular expression", with no location (toolkit v1-reliability, 2026-10-22).
    /// </summary>
    [TestFixture]
    public static class SymbolIdTests
    {
        [Test]
        public static void Symbols_made_on_many_threads_at_once_get_distinct_ids()
        {
            const int threads = 8;
            const int perThread = 200_000;
            var ids = new ConcurrentBag<long>();
            Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = threads }, _ =>
            {
                var mine = new long[perThread];
                for (var i = 0; i < perThread; i++)
                    mine[i] = new VariableDef(null, "x", null, null).Id;
                foreach (var id in mine)
                    ids.Add(id);
            });
            Assert.That(ids.Distinct().Count(), Is.EqualTo(threads * perThread));
        }
    }
}
