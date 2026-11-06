// Maps a list over a few dedicated threads, with the results in input order.
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ara3D.Geometry.Compiler.Utilities
{
    public static class OrderedParallel
    {
        // The compiler recurses over syntax and TIR trees, which overflows the 1.5 MB stack of a
        // thread-pool thread on the largest files, so the work runs on threads with a large reserved stack.
        private const int StackBytes = 256 * 1024 * 1024;
        public const int DefaultMaxWorkers = 8;

        /// <summary>Equal to <c>items.Select(f).ToArray()</c> for an <paramref name="f"/> that reads only shared
        /// state. Items are handed out in order, one at a time, so a slow item does not hold up a fixed share of
        /// the list. If <paramref name="f"/> throws, the exception of the earliest failed item is rethrown.</summary>
        public static R[] Map<T, R>(IReadOnlyList<T> items, Func<T, R> f, int maxWorkers = DefaultMaxWorkers)
        {
            var results = new R[items.Count];
            if (items.Count == 0)
                return results;
            // One worker still gets its own thread: the large stack is part of the contract.
            var workers = Math.Max(1, Math.Min(Math.Min(Environment.ProcessorCount, maxWorkers), items.Count));

            var failures = new Exception[items.Count];
            var next = -1;
            void Work()
            {
                int i;
                while ((i = Interlocked.Increment(ref next)) < items.Count)
                {
                    try { results[i] = f(items[i]); }
                    catch (Exception e) { failures[i] = e; }
                }
            }

            // Every worker that started is joined even when a later Start throws (TKT-1209), so no worker outlives
            // the call or keeps writing results and failures after the caller has given up on them.
            var threads = new Thread[workers];
            var started = 0;
            try
            {
                for (var w = 0; w < workers; w++)
                {
                    (threads[w] = new Thread(Work, StackBytes)).Start();
                    started++;
                }
            }
            finally
            {
                for (var w = 0; w < started; w++)
                    threads[w].Join();
            }

            foreach (var e in failures)
                if (e != null)
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e).Throw();
            return results;
        }
    }
}
