using Ara3D.Geometry.AST;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Ara3D.Geometry.ContextExport;

public static class Program
{
    // Plato.ContextExport <folder> [--pretty] [--diagnostics] [--diagnostics-file <path>]
    // [--compressed] [--tight-delimiters] [--no-compressed] [--no-tight-delimiters]
    // Redirect stdout to a file under .temp/ at the repo root (see AGENTS.md); do not write captures here.
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            Console.Error.WriteLine("Usage: Plato.ContextExport <folder> [--pretty] [--diagnostics] [--diagnostics-file <path>]");
            Console.Error.WriteLine("       [--compressed] [--tight-delimiters] [--no-compressed] [--no-tight-delimiters]");
            return 1;
        }

        var format = PlatoFormatOptions.FromArgs(args);
        var diagnostics = args.Contains("--diagnostics");
        var diagnosticsFile = GetOptionValue(args, "--diagnostics-file");

        var folderPath = GetFolderArgument(args);
        if (folderPath == null)
        {
            Console.Error.WriteLine("Missing input folder.");
            return 1;
        }

        var folder = new DirectoryPath(folderPath);
        if (!folder.Exists())
        {
            Console.Error.WriteLine($"Folder not found: {folder}");
            return 1;
        }

        var files = folder.GetFiles("*.plato", recurse: true).ToList();
        if (files.Count == 0)
        {
            Console.Error.WriteLine($"No .plato files found in {folder}");
            return 1;
        }

        var declarations = new List<AstTypeDeclaration>();
        foreach (var file in files.OrderBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var ast = ParseFile(file);
            if (ast == null)
                return 1;

            declarations.AddRange(ast.Types.Where(IsExportable));
        }

        var lines = new List<string>();
        foreach (var declaration in declarations)
        {
            var text = PlatoDeclarationWriter.WriteFormatted(declaration, format);
            lines.Add(text);
        }

        var separator = format.Pretty ? "\n\n" : "\n";
        var output = string.Join(separator, lines);
        if (output.Length > 0)
            Console.Out.Write(output);

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
        var parser = CommonParsers.PlatoParser(input, Logger.Null);

        if (!parser.Succeeded)
        {
            Console.Error.WriteLine($"Parse failed: {file}");
            foreach (var err in parser.ErrorMessages)
                Console.Error.WriteLine(err);
            return null;
        }

        return parser.Cst?.ToAst() as AstFile;
    }

    static string? GetFolderArgument(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--diagnostics-file")
            {
                i++;
                continue;
            }
            if (!args[i].StartsWith("--"))
                return args[i];
        }
        return null;
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
