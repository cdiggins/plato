// Reports sorted lint findings and applies the strict error-severity exit policy.
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Logging;

namespace Ara3D.Geometry.CLI;

internal static class LintCommand
{
    public static int Run(string[] args, ILogger logger)
    {
        var strict = args.Contains("--strict");
        var folders = args.Where(a => !a.StartsWith("--")).ToList();
        if (folders.Count < 1)
        {
            Console.Error.WriteLine("Usage: Plato.CLI lint <inputFolder>... [--strict]");
            return 1;
        }

        var (files, inputError) = SourceFiles.Discover(folders);
        if (inputError != null)
        {
            Console.Error.WriteLine(inputError);
            return 1;
        }
        var docs = files.Select(f => new Document(f, logger)).ToList();
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
