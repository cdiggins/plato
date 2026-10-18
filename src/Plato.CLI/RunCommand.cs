using System.Diagnostics;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    // Plato.CLI run <inputFolder>... [--laws] [--recursive] [--exclude=<file name>] [--profile=cpu64|gpu32]
    //                [--filter=<substring>] [--trials=N] [--seed=N] [--budget=<seconds per law>] [--call=<Name>]
    //
    // Compiles the input roots as one program (top-level files, or every subfolder with --recursive,
    // *.spec.plato always skipped) and runs it in this process through the interpreter: no C# is
    // emitted or built. --laws evaluates every Law_* over the Witness_* values and generated
    // values and prints one line per law: pass, fail with the first failing witness (and the law's
    // Measure_ companion when one exists), error with the fault, obligation with the bodiless
    // function reached, or skipped with the reason. --call=<Name> invokes a nullary function (or
    // a `_`-receiver one as Type.Name) and prints its value. Exit code 1 on a parse or compile
    // failure, or when any law fails or errors; skipped laws and host obligations do not fail
    // the run, they are listed.
    public static class RunCommand
    {
        public static int Run(string[] args, ILogger logger)
        {
            var recursive = args.Contains("--recursive");
            var laws = args.Contains("--laws");
            var excluded = Flags(args, "--exclude=").ToHashSet(StringComparer.OrdinalIgnoreCase);
            var profileName = Flags(args, "--profile=").LastOrDefault() ?? NumberProfile.Default.Name;
            var profile = NumberProfile.TryParse(profileName);
            if (profile == null)
            {
                Console.Error.WriteLine($"Unknown --profile '{profileName}'; one of: {NumberProfile.Names}");
                return 1;
            }
            var options = new LawRunOptions { Filter = Flags(args, "--filter=").LastOrDefault() };
            if (int.TryParse(Flags(args, "--trials=").LastOrDefault(), out var trials)) options.Trials = trials;
            if (int.TryParse(Flags(args, "--seed=").LastOrDefault(), out var seed)) options.Seed = seed;
            if (double.TryParse(Flags(args, "--budget=").LastOrDefault(), out var budget)) options.BudgetSeconds = budget;
            var calls = Flags(args, "--call=").ToList();
            var folders = args.Where(a => !a.StartsWith("--")).ToList();
            if (folders.Count == 0 || (!laws && calls.Count == 0))
            {
                Console.Error.WriteLine("Usage: Plato.CLI run <inputFolder>... (--laws | --call=<Name>) [--recursive] [--exclude=<file>] [--profile=cpu64|gpu32] [--filter=<text>] [--trials=N] [--seed=N]");
                return 1;
            }

            var timer = Stopwatch.StartNew();
            var files = new List<FilePath>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var root in folders)
            {
                if (!Directory.Exists(root))
                {
                    Console.Error.WriteLine($"Input folder not found: {root}");
                    return 1;
                }
                foreach (var f in Directory.GetFiles(root, "*.plato", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                {
                    if (f.EndsWith(".spec.plato", StringComparison.OrdinalIgnoreCase) || excluded.Contains(Path.GetFileName(f)))
                        continue;
                    if (seen.Add(Path.GetFullPath(f)))
                        files.Add(new FilePath(f));
                }
            }

            var docs = files.Select(f => (File: f, Doc: new Document(f, Ara3D.Logging.Logger.Null))).ToList();
            var failed = docs.Where(d => d.Doc.Parser == null || !d.Doc.Parser.Succeeded).ToList();
            if (failed.Count > 0)
            {
                foreach (var d in failed)
                    Console.Error.WriteLine($"Parsing failed: {d.File}");
                return 1;
            }
            var compilation = new Compilation(Ara3D.Logging.Logger.Null, docs.Select(d => d.Doc.Ast));
            if (!compilation.CompletedCompilation)
            {
                foreach (var e in compilation.InternalErrors)
                    Console.Error.WriteLine(e);
                foreach (var e in compilation.SymbolFactory?.Errors ?? new List<Ara3D.Geometry.Compiler.Symbols.ResolutionError>())
                    Console.Error.WriteLine($"RESOLVE {e.Message}");
                Console.Error.WriteLine("Compilation was not completed");
                return 1;
            }
            var compiled = timer.Elapsed;
            Console.WriteLine($"compiled {files.Count} files, {compilation.FunctionDefinitions.Count} functions, profile {profile} ({compiled.TotalSeconds:F1}s)");

            var exit = 0;
            // Deep Plato recursion needs more stack than the default thread has.
            var thread = new Thread(() =>
            {
                var interp = new Interpreter.Interpreter(compilation, profile);
                foreach (var name in calls)
                    exit |= Call(interp, name);
                if (laws)
                    exit |= Laws(interp, options, timer, compiled);
            }, 512 * 1024 * 1024);
            thread.Start();
            thread.Join();
            return exit;
        }

        private static int Call(Interpreter.Interpreter interp, string name)
        {
            try
            {
                object result;
                var dot = name.IndexOf('.');
                if (dot > 0)
                {
                    var type = interp.Compilation.GetTypeDef(name.Substring(0, dot))?.ToTypeExpression();
                    if (type == null) { Console.Error.WriteLine($"unknown type in --call={name}"); return 1; }
                    result = interp.Call(name.Substring(dot + 1), new TypeToken(type));
                }
                else
                    result = interp.Call(name);
                Console.WriteLine($"{name} = {ValueFormat.Format(result, 4000)}");
                return 0;
            }
            catch (EvaluationException e)
            {
                Console.Error.WriteLine($"{name}: {e.Describe()}");
                return 1;
            }
        }

        private static int Laws(Interpreter.Interpreter interp, LawRunOptions options, Stopwatch timer, TimeSpan compiled)
        {
            var runner = new LawRunner(interp, options);
            var counts = new Dictionary<LawStatus, int>();
            var total = 0;
            var results = new List<LawResult>();
            foreach (var law in runner.Laws)
            {
                var r = runner.Run(law);
                total++;
                counts[r.Status] = counts.GetValueOrDefault(r.Status) + 1;
                results.Add(r);
                Console.WriteLine(r);
            }
            Console.WriteLine();
            // The slowest laws, so a long sweep names what it spent its time on.
            foreach (var r in results.Where(r => r.Elapsed.TotalSeconds >= 1).OrderByDescending(r => r.Elapsed).Take(15))
                Console.WriteLine($"slow       {r.Name}  {r.Elapsed.TotalSeconds:F1}s over {r.Trials} cases");
            Console.WriteLine($"{total} laws: " + string.Join(", ", counts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Value} {kv.Key.ToString().ToLowerInvariant()}"))
                              + $"; {interp.ElaboratedCount} bodies elaborated in {interp.ElaborationTime.TotalSeconds:F1}s; {interp.ResolutionCount} run-time resolutions; laws {(timer.Elapsed - compiled).TotalSeconds:F1}s, total {timer.Elapsed.TotalSeconds:F1}s");
            return counts.GetValueOrDefault(LawStatus.Fail) + counts.GetValueOrDefault(LawStatus.Error) > 0 ? 1 : 0;
        }

        private static IEnumerable<string> Flags(string[] args, string prefix)
            => args.Where(a => a.StartsWith(prefix)).Select(a => a.Substring(prefix.Length));
    }
}
