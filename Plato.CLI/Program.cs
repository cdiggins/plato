using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.TypeScriptWriter;
using Ara3D.Geometry.RustWriter;
using Ara3D.Geometry.GlslWriter;
using Ara3D.Geometry.CppWriter;
using Logger = Ara3D.Logging.Logger;

namespace Ara3D.Geometry.CLI
{
    public static class Program
    {
        // Usage: Plato.CLI [inputFolder] [outputFolder] [--typescript|--rust|--glsl|--cpp|--cuda] [--csharp-style=extensions] [--optimize] [--optimize-arrays] [--inline] [--scalar=wrapper|float]
        //        Plato.CLI lint <inputFolder> [--strict]
        // With no arguments, the folders come from Config and C# is generated (original behavior).
        // In lint mode the sources are compiled (parse + resolve, no output) and warnings are
        // reported as "file(line): LINT###: message". Exit code is 0 unless --strict is passed,
        // in which case an Error-severity finding produces exit code 1 (compilation failures always
        // exit 1). Warning and Info findings never gate --strict.
        public static int Main(string[] args)
        {
            var logger = Logger.Console;

            if (args.Length > 0 && args[0] == "lint")
                return Lint(args.Skip(1).ToArray(), logger);

            var typeScript = args.Contains("--typescript");
            var rust = args.Contains("--rust");
            var glsl = args.Contains("--glsl");
            var cpp = args.Contains("--cpp");
            var cuda = args.Contains("--cuda");

            // Component-op unrolling optimization (roadmap P3.1). C# backend only; works with
            // both --csharp-style values. Off by default (byte-identical output without it).
            var optimize = args.Contains("--optimize");

            // Loop-into-buffer lowering (optimizer stage 2 increment 1): Map/MapRange results in
            // materialization positions become eager arrays. C# backend, TIR path only.
            var optimizeArrays = args.Contains("--optimize-arrays");

            // Source-level function inlining (roadmap P3.2 beta reduction): resolved calls to
            // small library functions are inlined before the other optimizer passes run.
            var inline = args.Contains("--inline");

            // No properties/indexers in the generated output: concept interfaces declare
            // scalar-erased METHOD obligations, kept struct members and constants emit as
            // methods. Requires --scalar=float (and the default TIR path).
            var methods = args.Contains("--methods");

            // No properties AT ALL in the generated output (strict superset of --methods): the
            // primitive-struct no-arg members that --methods still keeps as properties also emit
            // as methods. For a runtime whose primitive structs expose them as methods
            // (Plato.Intrinsics.V2). Implies --methods.
            var noProperties = args.Contains("--no-properties");

            // Emit `_`-receiver concept members (Plato's constructor-shaped idiom for a type-level
            // operation) as C# `static abstract` interface members rather than omitting them.
            // Opt-in: stdlib-legacy's IArrayLike declares three such members, so enabling this by
            // default would move the diff-gated goldens. See plato-312.
            var staticAbstract = args.Contains("--static-abstract");

            // Array-combinator loop lowering: Map/Zip/Reduce/All/Any/Reverse/... call sites on
            // one-dimensional list receivers become for-loop statements filling materialized
            // arrays (see TirLoopLowerer).
            var loops = args.Contains("--loops");

            // --dump-tir=<dir>: write the per-phase TIR of every emitted body to <dir> (one file
            // per owner type) for optimizer-pass development. No effect on the emitted C#.
            var tirDumpDir = args.Where(a => a.StartsWith("--dump-tir="))
                .Select(a => a.Substring("--dump-tir=".Length)).LastOrDefault();

            // --inline-report: print a per-generation table of inliner refusals + success totals
            // to stderr after emission (M0 optimizer instrumentation). No effect on emitted C#.
            var inlineReport = args.Contains("--inline-report");

            // The C# emitter produces one recipe: extension-style, scalar-erased output. The legacy
            // default (wrapper-struct) style was retired with CSharpFunctionBodyWriter (consolidation
            // plan C4); --csharp-style=extensions is still accepted for script/flag parity.
            var csharpStyle = args.Where(a => a.StartsWith("--csharp-style="))
                .Select(a => a.Substring("--csharp-style=".Length)).LastOrDefault() ?? "extensions";
            if (csharpStyle != "extensions")
            {
                Console.Error.WriteLine($"Unsupported --csharp-style value '{csharpStyle}' (only 'extensions' is supported; the default style was retired in C4)");
                return 1;
            }

            // Scalar erasure (roadmap "Phase 2 revision" item 3). "wrapper" = the Number/Integer/
            // Boolean/Character/String wrapper structs (byte-identical default); "float" = erase
            // the wrappers to native primitives in all generated code. Only supported together
            // with --csharp-style=extensions: the default style keeps scalar members as
            // instance members of the wrapper partial structs, which do not exist under erasure.
            var scalar = args.Where(a => a.StartsWith("--scalar="))
                .Select(a => a.Substring("--scalar=".Length)).LastOrDefault() ?? "wrapper";
            if (scalar != "wrapper" && scalar != "float")
            {
                Console.Error.WriteLine($"Unknown --scalar value '{scalar}' (expected 'wrapper' or 'float')");
                return 1;
            }

            var folders = args.Where(a => !a.StartsWith("--")).ToList();
            var inputFolder = new DirectoryPath(folders.Count > 0 ? folders[0] : Config.InputFolder);
            var outputFolder = new DirectoryPath(folders.Count > 1 ? folders[1] : Config.OutputFolder);

            logger.Log("Opening files");
            var files = inputFolder.GetFiles("*.plato");
            var docs = files.Select(f => new Document(f, logger)).ToList();
            var parsingSuccessful = docs.All(e => e.Parser.Succeeded);
            if (!parsingSuccessful)
            {
                Console.Error.WriteLine("Parsing failed for one of the input files, halting");
                return 1;
            }
            logger.Log("Parsing succeeded for all files");

            logger.Log("Compiling");
            var trees = docs.Select(e => e.Ast);
            var compilation = new Compilation(logger, trees);
            if (!compilation.CompletedCompilation)
            {
                Console.Error.WriteLine("Compilation was not completed");
                return 1;
            }

            if (typeScript)
            {
                logger.Log("Writing TypeScript Files");
                var output = compilation.ToTypeScript(outputFolder, true);
                foreach (var kv in output.Files)
                {
                    var fp = outputFolder.RelativeFile(kv.Key);
                    logger.Log($"Writing {kv.Key}");
                    fp.WriteAllText(kv.Value.ToString());
                }
            }
            else if (glsl)
            {
                logger.Log("Writing GLSL Files");
                var output = compilation.ToGlsl(outputFolder);
                logger.Log($"GLSL functions emitted: {output.FunctionsEmitted}, skipped: {output.Skipped.Count}");
                foreach (var kv in output.Files)
                {
                    var fp = outputFolder.RelativeFile(kv.Key);
                    logger.Log($"Writing {kv.Key}");
                    fp.WriteAllText(kv.Value.ToString());
                }
            }
            else if (cpp || cuda)
            {
                var dialect = cuda ? CppDialect.Cuda : CppDialect.Cpp;
                logger.Log($"Writing {dialect.DisplayName()} Files");
                var output = compilation.ToCpp(outputFolder, dialect, inline);
                logger.Log($"{dialect.DisplayName()} functions emitted: {output.FunctionsEmitted}, skipped: {output.Skipped.Count}");
                foreach (var kv in output.Files)
                {
                    var fp = outputFolder.RelativeFile(kv.Key);
                    logger.Log($"Writing {kv.Key}");
                    fp.WriteAllText(kv.Value.ToString());
                }
            }
            else if (rust)
            {
                logger.Log("Writing Rust Files");
                var output = compilation.ToRust(outputFolder, true);
                foreach (var kv in output.Files)
                {
                    var fp = outputFolder.RelativeFile(kv.Key);
                    logger.Log($"Writing {kv.Key}");
                    fp.WriteAllText(kv.Value.ToString());
                }
            }
            else
            {
                logger.Log("Writing C# Files");
                var output = compilation.ToCSharp(outputFolder, csharpStyle == "extensions", optimize, scalar == "float", optimizeArrays, inline, methods, loops, tirDumpDir, noProperties, inlineReport, staticAbstract);
                logger.Log($"TIR bodies emitted: {output.TirBodiesEmitted}");
                if (output.DegradedBodies.Count > 0)
                {
                    logger.Log($"DEGRADED bodies (no ground TIR, emitted as throwing stubs): {output.DegradedBodies.Count}");
                    foreach (var m in output.DegradedBodies)
                        logger.Log($"  degraded: {m}");
                }
                foreach (var kv in output.Files)
                {
                    var fp = outputFolder.RelativeFile(kv.Key);
                    logger.Log($"Writing {kv.Key}");
                    fp.WriteAllText(kv.Value.ToString());
                }
            }

            logger.Log("Completed");
            return 0;
        }

        // Plato.CLI lint <inputFolder> [--strict]
        // Compiles (parse + resolve) with no output and reports lint warnings.
        public static int Lint(string[] args, ILogger logger)
        {
            var strict = args.Contains("--strict");
            var folders = args.Where(a => !a.StartsWith("--")).ToList();
            if (folders.Count < 1)
            {
                Console.Error.WriteLine("Usage: Plato.CLI lint <inputFolder> [--strict]");
                return 1;
            }

            var inputFolder = new DirectoryPath(folders[0]);
            var files = inputFolder.GetFiles("*.plato");
            var docs = files.Select(f => new Document(f, logger)).ToList();
            if (docs.Count == 0)
            {
                Console.Error.WriteLine($"No .plato files found in {inputFolder}");
                return 1;
            }
            if (!docs.All(d => d.Parser.Succeeded))
            {
                Console.Error.WriteLine("Parsing failed for one of the input files, halting");
                return 1;
            }

            var compilation = new Compilation(logger, docs.Select(d => d.Ast));
            if (!compilation.CompletedCompilation)
            {
                Console.Error.WriteLine("Compilation was not completed; cannot lint");
                return 1;
            }

            var linter = new Linter(compilation);
            foreach (var finding in linter.SortedFindings)
                Console.WriteLine(finding);

            var byCode = linter.Findings.GroupBy(f => f.Code).OrderBy(g => g.Key);
            Console.WriteLine();
            Console.WriteLine($"{linter.Findings.Count} lint finding(s)" +
                (linter.Findings.Count > 0
                    ? ": " + string.Join(", ", byCode.Select(g => $"{g.Key} x{g.Count()}"))
                    : ""));

            var bySeverity = linter.Findings.GroupBy(f => f.Severity).OrderByDescending(g => g.Key);
            if (linter.Findings.Count > 0)
                Console.WriteLine("By severity: " +
                    string.Join(", ", bySeverity.Select(g => $"{g.Key} x{g.Count()}")));

            // Ratchet = Error + Warning only; Info describes vocabulary shape and is not burned down.
            Console.WriteLine(
                $"Ratchet: {linter.RatchetCount} (Error {linter.ErrorCount} + Warning {linter.WarningCount}); " +
                $"Info {linter.InfoCount} excluded");

            // --strict gates on Errors only: Warning/Info findings report incomplete or merely
            // shapeless declarations, which are expected on a declarations-first folder.
            return strict && linter.ErrorCount > 0 ? 1 : 0;
        }
    }
}
