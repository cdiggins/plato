using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Parakeet;
using Ara3D.Parsing;

namespace PlatoCppWriterTests;

/// <summary>Builds a real <see cref="Compilation"/> from .plato source on disk.</summary>
public static class PlatoSource
{
    /// <summary>Walk up from the test output directory to find a folder in the repo.</summary>
    public static string FindRepoFolder(string relativePath)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (Directory.Exists(candidate))
                return candidate;
        }
        throw new DirectoryNotFoundException($"Could not locate '{relativePath}' above {AppContext.BaseDirectory}");
    }

    public static AstNode ParseFile(string path)
    {
        var text = File.ReadAllText(path);
        var parser = CommonParsers.PlatoParser(new ParserInput(text, path), Ara3D.Logging.Logger.Null);
        if (!parser.Succeeded)
            throw new Exception($"Parse failed for {path}: {string.Join("; ", parser.ErrorMessages)}");
        return parser.Cst!.ToAst();
    }

    public static Compilation CompileFolder(string folder)
    {
        var asts = Directory.GetFiles(folder, "*.plato").Select(ParseFile).ToList();
        if (asts.Count == 0)
            throw new FileNotFoundException($"No .plato files in {folder}");
        return new Compilation(Ara3D.Logging.Logger.Null, asts);
    }

    /// <summary>The small hand-curated demo library (also the GLSL writer's reference input).</summary>
    public static Compilation CompileDemoLibrary()
        => CompileFolder(FindRepoFolder("demos/plato-src"));

    /// <summary>The production standard library: the stress case.</summary>
    public static Compilation CompileStandardLibrary()
        => CompileFolder(FindRepoFolder("plato-src"));
}
