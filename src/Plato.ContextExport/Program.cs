using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Ara3D.Geometry.ContextExport;

public static class Program
{
    // Plato.ContextExport <folder> [--pretty] [--diagnostics] [--diagnostics-file <path>]
    // [--compressed] [--tight-delimiters] [--no-compressed] [--no-tight-delimiters]
    // [--hierarchy]
    // Redirect stdout to a file under .temp/ at the repo root (see AGENTS.md); do not write captures here.
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is "-h" or "--help")
        {
            Console.Error.WriteLine("Usage: Plato.ContextExport <folder> [--pretty] [--diagnostics] [--diagnostics-file <path>]");
            Console.Error.WriteLine("       [--compressed] [--tight-delimiters] [--no-compressed] [--no-tight-delimiters]");
            Console.Error.WriteLine("       [--hierarchy] [--output <path>]   ASCII concept inherits forest (bound compilation)");
            return 1;
        }

        var hierarchy = args.Contains("--hierarchy");
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

        if (hierarchy)
            return ExportHierarchy(files, GetOptionValue(args, "--output"));

        return ExportFlat(files, format, diagnostics, diagnosticsFile);
    }

    static int ExportFlat(
        List<FilePath> files,
        PlatoFormatOptions format,
        bool diagnostics,
        string? diagnosticsFile)
    {
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
            lines.Add(PlatoDeclarationWriter.WriteFormatted(declaration, format));

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

    static int ExportHierarchy(List<FilePath> files, string? outputPath)
    {
        var asts = new List<AstNode>();
        foreach (var file in files.OrderBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var ast = ParseFile(file);
            if (ast == null)
                return 1;
            asts.Add(ast);
        }

        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        if (!compilation.CompletedCompilation)
        {
            Console.Error.WriteLine("Compilation did not complete; cannot build concept hierarchy.");
            foreach (var d in compilation.Diagnostics)
                Console.Error.WriteLine(d);
            return 1;
        }

        var text = ConceptHierarchy.FormatAscii(compilation);
        if (outputPath != null)
        {
            var utf8 = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            System.IO.File.WriteAllText(outputPath, text, utf8);
        }
        else
        {
            Console.Out.Write(text);
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

    static string? GetFolderArgument(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] is "--diagnostics-file" or "--output")
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
