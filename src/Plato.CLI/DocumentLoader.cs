// Parses many compiler input files at once, with the results and the log lines in file order.
using Ara3D.Geometry.Compiler.Utilities;
using Ara3D.Logging;
using Ara3D.Parakeet.Grammars;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    public static class DocumentLoader
    {
        /// <summary>Equal to <c>files.Select(f => new Document(f, logger)).ToList()</c>: the documents
        /// come back in file order and each document's log lines reach <paramref name="logger"/>
        /// together, in file order. Files are parsed on several threads.</summary>
        public static List<Document> ParseAll(IReadOnlyList<FilePath> files, ILogger logger)
        {
            if (files.Count <= 1)
                return files.Select(f => new Document(f, logger)).ToList();

            // The grammar creates and caches a rule on first access in a shared Dictionary, which is not
            // safe to fill from several threads. Create every rule now, before going parallel.
            _ = PlatoGrammar.Instance.GetRules().ToList();

            var quiet = ReferenceEquals(logger, Logger.Null);
            var parsed = OrderedParallel.Map(files, f =>
            {
                var buffer = quiet ? null : new BufferedLogger();
                return (Document: new Document(f, buffer ?? logger), Log: buffer);
            });
            foreach (var p in parsed)
                p.Log?.ReplayTo(logger);
            return parsed.Select(p => p.Document).ToList();
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
