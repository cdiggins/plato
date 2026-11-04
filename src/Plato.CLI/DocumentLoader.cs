// Parses many compiler input files at once, with the results and the log lines in file order.
using System.Diagnostics;
using Ara3D.Logging;
using Ara3D.Parakeet.Grammars;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    public static class DocumentLoader
    {
        // The recursive CST factory overflows the 1.5 MB stack of a thread-pool thread on the
        // largest files, so parsing runs on dedicated threads with a large reserved stack.
        private const int StackBytes = 256 * 1024 * 1024;
        private const int MaxWorkers = 8;

        /// <summary>Equal to <c>files.Select(f => new Document(f, logger)).ToList()</c>: the documents
        /// come back in file order and each document's log lines reach <paramref name="logger"/>
        /// together, in file order. Files are parsed on up to eight threads.</summary>
        public static List<Document> ParseAll(IReadOnlyList<FilePath> files, ILogger logger)
        {
            var docs = new Document[files.Count];
            var workers = Math.Min(Math.Min(Environment.ProcessorCount, MaxWorkers), files.Count);
            if (workers <= 1)
                return files.Select(f => new Document(f, logger)).ToList();

            // The grammar creates and caches a rule on first access in a shared Dictionary, which is not
            // safe to fill from several threads. Create every rule now, before going parallel.
            foreach (var g in new[] { PlatoGrammar.Instance })
                _ = g.GetRules().ToList();

            var quiet = ReferenceEquals(logger, Logger.Null);
            var buffers = new BufferedLogger[files.Count];
            var next = -1;
            var threads = new List<Thread>();
            for (var w = 0; w < workers; w++)
            {
                var t = new Thread(() =>
                {
                    int i;
                    while ((i = Interlocked.Increment(ref next)) < files.Count)
                    {
                        var log = quiet ? logger : (buffers[i] = new BufferedLogger());
                        docs[i] = new Document(files[i], log);
                    }
                }, StackBytes);
                t.Start();
                threads.Add(t);
            }
            foreach (var t in threads)
                t.Join();

            foreach (var b in buffers)
                b?.ReplayTo(logger);
            return docs.ToList();
        }

        private sealed class BufferedLogger : ILogger
        {
            private readonly List<(string, LogLevel)> _lines = new();
            public string Name => "Document";

            public ILogger Log(string message, LogLevel level)
            {
                _lines.Add((message, level));
                return this;
            }

            public void ReplayTo(ILogger target)
            {
                foreach (var (m, l) in _lines)
                    target.Log(m, l);
            }
        }
    }
}
