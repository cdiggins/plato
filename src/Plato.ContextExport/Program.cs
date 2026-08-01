using Ara3D.Geometry.AST;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Ara3D.Geometry.ContextExport;

public static class Program
{
    // Plato.ContextExport <folder>... [--pretty] [--diagnostics] [--diagnostics-file <path>]
    // [--compressed] [--tight-delimiters] [--no-compressed] [--no-tight-delimiters]
    // [--arrows] [--output <path>]
    // Several folders may be given; they are exported as one list, so the caller chooses the
    // corpus (e.g. the shipping stdlib tiers, leaving `future` out) without post-processing.
    // Without --output the declarations go to stdout; redirect to a file under .temp/ at the repo
    // root (see AGENTS.md); do not write captures here.
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            Console.Error.WriteLine("Usage: Plato.ContextExport <folder>... [--pretty] [--diagnostics] [--diagnostics-file <path>]");
            Console.Error.WriteLine("       [--compressed] [--tight-delimiters] [--no-compressed] [--no-tight-delimiters]");
            Console.Error.WriteLine("       [--arrows] [--output <path>]");
            Console.Error.WriteLine("  --arrows  write `inherits`/`implements` as a single arrow");
            Console.Error.WriteLine("  --output  write declarations to a file instead of stdout");
            return 1;
        }

        var format = PlatoFormatOptions.FromArgs(args);
        var diagnostics = args.Contains("--diagnostics");
        var diagnosticsFile = GetOptionValue(args, "--diagnostics-file");

        var folderPaths = GetFolderArguments(args);
        if (folderPaths.Count == 0)
        {
            Console.Error.WriteLine("Missing input folder.");
            return 1;
        }

        var files = new List<FilePath>();
        foreach (var folderPath in folderPaths)
        {
            var folder = new DirectoryPath(folderPath);
            if (!folder.Exists())
            {
                Console.Error.WriteLine($"Folder not found: {folder}");
                return 1;
            }
            files.AddRange(folder.GetFiles("*.plato", recurse: true));
        }

        if (files.Count == 0)
        {
            Console.Error.WriteLine($"No .plato files found in {string.Join(", ", folderPaths)}");
            return 1;
        }

        return ExportFlat(files, format, diagnostics, diagnosticsFile, GetOptionValue(args, "--output"));
    }

    static int ExportFlat(
        List<FilePath> files,
        PlatoFormatOptions format,
        bool diagnostics,
        string? diagnosticsFile,
        string? outputPath)
    {
        var declarations = new List<AstTypeDeclaration>();
        foreach (var file in files.OrderBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var ast = ParseFile(file);
            if (ast == null)
                return 1;

            declarations.AddRange(ast.Types.Where(IsExportable));
        }

        // Concepts first, then types, each alphabetical: a reader can find a name without knowing
        // which file declares it, and the vocabulary of abstractions reads before its instances.
        var ordered = declarations
            .OrderBy(d => d.Kind == TypeKind.Interface ? 0 : 1)
            .ThenBy(d => d.Name.Text, StringComparer.OrdinalIgnoreCase)
            .ThenBy(d => d.Name.Text, StringComparer.Ordinal);

        var lines = new List<string>();
        foreach (var declaration in ordered)
            lines.Add(PlatoDeclarationWriter.WriteFormatted(declaration, format));

        var separator = format.Pretty ? "\n\n" : "\n";
        var output = string.Join(separator, lines);
        if (outputPath != null)
        {
            var utf8 = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            File.WriteAllText(outputPath, output, utf8);
        }
        else if (output.Length > 0)
        {
            Console.Out.Write(output);
        }

        if (diagnostics || diagnosticsFile != null)
        {
            var stats = ExportDiagnostics.FromOutput(
                output,
                files.Count,
                declarations.Count(d => d.Kind == TypeKind.ConcreteType),
                declarations.Count(d => d.Kind == TypeKind.Interface));

            var diagnosticsWriter = diagnosticsFile != null
                ? (TextWriter)new StreamWriter(diagnosticsFile)
                : Console.Error;

            try
            {
                stats.Write(diagnosticsWriter);
            }
            finally
            {
                if (diagnosticsWriter != Console.Error)
                    diagnosticsWriter.Dispose();
            }
        }

        return 0;
    }

    static bool IsExportable(AstTypeDeclaration declaration)
        => declaration.Kind is TypeKind.ConcreteType or TypeKind.Interface;

    static AstFile? ParseFile(FilePath file)
    {
        var text = File.ReadAllText(file);
        var input = new ParserInput(text, file);
        var parser = CommonParsers.PlatoParser(input, Ara3D.Logging.Logger.Null);

        if (!parser.Succeeded)
        {
            Console.Error.WriteLine($"Parse failed: {file}");
            foreach (var err in parser.ErrorMessages)
                Console.Error.WriteLine(err);
            return null;
        }

        return parser.Cst?.ToAst() as AstFile;
    }

    static List<string> GetFolderArguments(string[] args)
    {
        var folders = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] is "--diagnostics-file" or "--output")
            {
                i++;
                continue;
            }
            if (!args[i].StartsWith("--"))
                folders.Add(args[i]);
        }
        return folders;
    }

    static string? GetOptionValue(string[] args, string option)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == option)
                return args[i + 1];
        }
        return null;
    }
}
