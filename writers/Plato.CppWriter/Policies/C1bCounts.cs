// What the lane C1-b emission decisions changed or refused in one generation (TKT-1183).
using System;
using System.Threading;

namespace Ara3D.Geometry.CppWriter.Policies
{
    /// <summary>
    /// Process-wide counters of the presize and cache-constants decisions. They hold no state that reaches the output.
    /// With the environment variable <c>PLATO_C1B_STATS</c> set, one line is written to stderr when the process exits.
    /// </summary>
    public static class C1bCounts
    {
        public static int EmptyLists, PresizedLists;
        public static int ConstantBodies, ConstantSites, RefusedArenaType;

        static C1bCounts()
        {
            if (Environment.GetEnvironmentVariable("PLATO_C1B_STATS") != null)
                AppDomain.CurrentDomain.ProcessExit += (_, _) => Console.Error.WriteLine(Summary());
        }

        public static string Summary()
            => $"C1b: lists presized {PresizedLists}, lists unsized {EmptyLists}; constants cached: bodies {ConstantBodies}, sites {ConstantSites}; "
               + $"refused for an arena-held result type: {RefusedArenaType}";

        public static void Bump(ref int counter) => Interlocked.Increment(ref counter);
    }
}
