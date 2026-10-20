using System.Diagnostics;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using Ara3D.Geometry.Navigation;
using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    // Plato.CLI run <inputFolder>... [--laws] [--recursive] [--exclude=<file name>] [--profile=cpu64|gpu32]
    //                [--backend=interpreter|csharp] [--filter=<text>[,<text>...]] [--laws-from=<file>]
    //                [--report=<file>] [--baseline=<file>] [--trials=N] [--seed=N]
    //                [--budget=<seconds per law>] [--abandon=<seconds>] [--parallel=N] [--call=<Name>]
    //
    // Compiles the input roots as one program (top-level files, or every subfolder with --recursive,
    // *.spec.plato always skipped) and runs it in this process. --backend=interpreter (the default)
    // evaluates in the interpreter, no C# emitted or built; --backend=csharp emits the program with
    // the C# writer, compiles it in process with Roslyn and calls the laws by reflection, on every
    // core at once (--parallel=N bounds it). Both see the same witnesses in the same order.
    // --laws evaluates every Law_* over the Witness_* values and generated
    // values and prints one line per law: pass, fail with the first failing witness (and the law's
    // Measure_ companion when one exists), error with the fault, obligation with the bodiless
    // function reached, or skipped with the reason. A law over --budget seconds (default 2) is an
    // error naming its elapsed time: the interpreter stops it at the budget; the C# backend lets it
    // finish, and gives up on it after --abandon seconds (default 120). --budget=0 lifts the limit. --filter keeps the laws whose full name
    // (Owner.Law_Name) contains any of its comma-separated texts, ignoring case; --laws-from adds
    // the names in a file, one per line (blank lines and # comments skipped); a name that
    // selects no law is reported on stderr. --report=<file> writes one tab-separated line per law
    // (name, verdict, elapsed milliseconds, first failing witness), sorted by name. --baseline=<file>
    // compares the verdicts of the run with such a file (in either column layout), prints every
    // law whose verdict changed, and then sets the exit code alone:
    // 1 when a law that passed no longer passes, 0 otherwise. --call=<Name> invokes a nullary function (or
    // a `_`-receiver one as Type.Name) and prints its value. Exit code 1 on a parse or compile
    // failure, or, without --baseline, when any law fails or errors; skipped laws and host obligations do not fail
    // the run, they are listed.
    public static class RunCommand
    {
        public static int Run(string[] args, ILogger logger)
        {
            var recursive = args.Contains("--recursive");
            var laws = args.Contains("--laws");
            var excluded = Flags(args, "--exclude=").ToList();
            var profileName = Flags(args, "--profile=").LastOrDefault() ?? NumberProfile.Default.Name;
            var profile = NumberProfile.TryParse(profileName);
            if (profile == null)
            {
                Console.Error.WriteLine($"Unknown --profile '{profileName}'; one of: {NumberProfile.Names}");
                return 1;
            }
            IReadOnlyList<string> filters;
            try { filters = LawSelection.FromArguments(Flags(args, "--filter="), Flags(args, "--laws-from=")); }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
            var backendName = Flags(args, "--backend=").LastOrDefault() ?? LawBackends.InterpreterName;
            var backendKind = LawBackends.TryParse(backendName);
            if (backendKind == null)
            {
                Console.Error.WriteLine($"Unknown --backend '{backendName}'; one of: {LawBackends.Names}");
                return 1;
            }
            var options = new LawRunOptions { Filters = filters };
            if (int.TryParse(Flags(args, "--trials=").LastOrDefault(), out var trials)) options.Trials = trials;
            if (int.TryParse(Flags(args, "--seed=").LastOrDefault(), out var seed)) options.Seed = seed;
            if (double.TryParse(Flags(args, "--budget=").LastOrDefault(), out var budget)) options.BudgetSeconds = budget > 0 ? budget : null;
            if (double.TryParse(Flags(args, "--abandon=").LastOrDefault(), out var abandon)) options.AbandonSeconds = abandon > 0 ? abandon : null;
            if (int.TryParse(Flags(args, "--parallel=").LastOrDefault(), out var parallel) && parallel > 0) options.MaxParallel = parallel;
            var reportPath = Flags(args, "--report=").LastOrDefault();
            var baselinePath = Flags(args, "--baseline=").LastOrDefault();
            if (baselinePath != null && !File.Exists(baselinePath))
            {
                Console.Error.WriteLine($"--baseline file not found: {baselinePath}");
                return 1;
            }
            // Read before the run, so --report may overwrite the baseline it is checked against.
            var baseline = baselinePath == null ? null : LawReport.Parse(File.ReadAllText(baselinePath));
            var calls = Flags(args, "--call=").ToList();
            var folders = args.Where(a => !a.StartsWith("--")).ToList();
            if (folders.Count == 0 || (!laws && calls.Count == 0))
            {
                Console.Error.WriteLine("Usage: Plato.CLI run <inputFolder>... (--laws | --call=<Name>) [--recursive] [--exclude=<file>] [--profile=cpu64|gpu32] [--backend=interpreter|csharp] [--filter=<text>[,<text>...]] [--laws-from=<file>] [--report=<file>] [--baseline=<file>] [--trials=N] [--seed=N] [--budget=<seconds>] [--abandon=<seconds>] [--parallel=N]");
                return 1;
            }

            var timer = Stopwatch.StartNew();
            List<FilePath> files;
            try { files = LawProgram.SourceFiles(folders, recursive, excluded).Select(f => new FilePath(f)).ToList(); }
            catch (DirectoryNotFoundException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
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
            var errors = WarmLawRunner.CompileErrors(compilation);
            if (errors.Count > 0)
            {
                foreach (var e in errors)
                    Console.Error.WriteLine(e);
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
                    exit |= Laws(interp, backendKind.Value, options, timer, compiled, reportPath, baseline);
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

        /// <summary>Runs the selected laws on the backend and prints them as they finish. Exit 1
        /// when one fails or errors, or, with a baseline, only when a law the baseline records as
        /// passing no longer passes.</summary>
        private static int Laws(Interpreter.Interpreter interp, LawBackendKind kind, LawRunOptions options, Stopwatch timer, TimeSpan compiled,
            string reportPath, IReadOnlyList<LawVerdict> baseline)
        {
            ILawBackend backend;
            try { backend = LawBackends.Create(kind, interp); }
            catch (InvalidOperationException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
            if (backend is CSharpBackend cs)
                Console.WriteLine($"emitted {cs.Program.FileCount} C# files in {cs.Program.EmitElapsed.TotalSeconds:F1}s, compiled in {cs.Program.CompileElapsed.TotalSeconds:F1}s"
                                  + (cs.Program.DegradedBodies.Count > 0 ? $", {cs.Program.DegradedBodies.Count} degraded bodies" : "")
                                  + $"; laws on up to {options.MaxParallel} threads");
            var ready = timer.Elapsed;
            var runner = new LawRunner(backend, interp, options);
            foreach (var f in runner.UnmatchedFilters)
                Console.Error.WriteLine($"filter '{f}' matches no law");
            var console = new object();
            var results = runner.RunAll(r =>
            {
                lock (console)
                    Console.WriteLine(r);
            });
            var counts = results.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            Console.WriteLine();
            // The slowest laws and witnesses, so a long sweep names what it spent its time on.
            foreach (var r in results.OrderByDescending(r => r.Elapsed).Take(15))
                Console.WriteLine($"slow       {r.Name}  {r.Elapsed.TotalSeconds:F2}s over {r.Trials} cases");
            foreach (var w in runner.Witnesses.Where(w => w.Fault != null || w.Elapsed.TotalSeconds >= 1).OrderByDescending(w => w.Elapsed).Take(15))
                Console.WriteLine($"witness    {w.Name}  {w.Elapsed.TotalSeconds:F2}s" + (w.Fault != null ? $"  {w.Fault}" : ""));
            var lawTime = TimeSpan.FromTicks(results.Sum(r => r.Elapsed.Ticks));
            var witnessTime = TimeSpan.FromTicks(runner.Witnesses.Sum(w => w.Elapsed.Ticks));
            Console.WriteLine($"{results.Count} laws: " + string.Join(", ", counts.OrderBy(kv => kv.Key).Select(kv => $"{kv.Value} {kv.Key.ToString().ToLowerInvariant()}"))
                              + $"; backend {backend.Name}"
                              + (kind == LawBackendKind.Interpreter ? $"; {interp.ElaboratedCount} bodies elaborated in {interp.ElaborationTime.TotalSeconds:F1}s; {interp.ResolutionCount} run-time resolutions" : "")
                              + $"; {runner.Witnesses.Count} witnesses {witnessTime.TotalSeconds:F1}s; laws {lawTime.TotalSeconds:F1}s summed, {(timer.Elapsed - ready).TotalSeconds:F1}s wall; total {timer.Elapsed.TotalSeconds:F1}s");

            var verdicts = LawReport.Verdicts(results);
            if (reportPath != null)
            {
                File.WriteAllText(reportPath, LawReport.Format(verdicts));
                Console.WriteLine($"report: {verdicts.Count} verdicts written to {reportPath}");
            }
            if (baseline != null)
                return Compare(baseline, verdicts, options.Filters == null);
            return counts.GetValueOrDefault(LawStatus.Fail) + counts.GetValueOrDefault(LawStatus.Error) > 0 ? 1 : 0;
        }

        private static int Compare(IReadOnlyList<LawVerdict> baseline, IReadOnlyList<LawVerdict> verdicts, bool wholeRun)
        {
            var changes = LawReport.Compare(baseline, verdicts, wholeRun);
            foreach (var c in changes)
                Console.WriteLine(c);
            var regressions = changes.Count(c => c.Regression);
            Console.WriteLine($"baseline: {changes.Count} verdicts changed, {regressions} regressed");
            return regressions > 0 ? 1 : 0;
        }

        private static IEnumerable<string> Flags(string[] args, string prefix)
            => args.Where(a => a.StartsWith(prefix)).Select(a => a.Substring(prefix.Length));
    }
}
