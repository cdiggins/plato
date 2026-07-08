using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Logging;
using Ara3D.Utils;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Geometry.TypeScriptWriter;
using Ara3D.Geometry.RustWriter;
using Logger = Ara3D.Logging.Logger;

namespace Ara3D.Geometry.CLI
{
    public static class Program
    {
        // Usage: Plato.CLI [inputFolder] [outputFolder] [--typescript|--rust] [--csharp-style=default|extensions] [--optimize] [--scalar=wrapper|float]
        //        Plato.CLI lint <inputFolder> [--strict]
        // With no arguments, the folders come from Config and C# is generated (original behavior).
        // In lint mode the sources are compiled (parse + resolve, no output) and warnings are
        // reported as "file(line): LINT###: message". Exit code is 0 unless --strict is passed,
        // in which case any finding produces exit code 1 (compilation failures always exit 1).
        public static int Main(string[] args)
        {
            var logger = Logger.Console;

            if (args.Length > 0 && args[0] == "lint")
                return Lint(args.Skip(1).ToArray(), logger);

            var typeScript = args.Contains("--typescript");
            var rust = args.Contains("--rust");

            // Component-op unrolling optimization (roadmap P3.1). C# backend only; works with
            // both --csharp-style values. Off by default (byte-identical output without it).
            var optimize = args.Contains("--optimize");

            // C# writer style (roadmap P2.2). "default" = original writer (byte-identical output);
            // "extensions" = C# 14 extension-block output (requires LangVersion 14 to compile).
            var csharpStyle = args.Where(a => a.StartsWith("--csharp-style="))
                .Select(a => a.Substring("--csharp-style=".Length)).LastOrDefault() ?? "default";
            if (csharpStyle != "default" && csharpStyle != "extensions")
            {
                Console.Error.WriteLine($"Unknown --csharp-style value '{csharpStyle}' (expected 'default' or 'extensions')");
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
            if (scalar == "float" && csharpStyle != "extensions")
            {
                Console.Error.WriteLine("--scalar=float requires --csharp-style=extensions (--csharp-style=default with erased scalars is not supported)");
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
                logger.Log("Parsing failed for one of the input files, halting");
                return 0; // preserved original behavior: generation mode always exits 0
            }
            logger.Log("Parsing succeeded for all files");

            logger.Log("Compiling");
            var trees = docs.Select(e => e.Ast);
            var compilation = new Compilation(logger, trees);
            if (!compilation.CompletedCompilation)
            {
                logger.Log("Compilation was not completed");
                return 0; // preserved original behavior: generation mode always exits 0
            }

            if (typeScript)
            {
                logger.Log("Writing TypeScript Files");
                var output = compilation.ToTypeScript(outputFolder);
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
                var output = compilation.ToRust(outputFolder);
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
                var output = compilation.ToCSharp(outputFolder, csharpStyle == "extensions", optimize, scalar == "float");
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

            return strict && linter.Findings.Count > 0 ? 1 : 0;
        }
    }
}
