// Builds source programs and locates fixtures for interpreter tests.
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Parakeet;
using Ara3D.Parsing;

namespace Plato.Interpreter.Tests;

/// <summary>Builds compilations for the interpreter tests: the forward stdlib's tiers, the law
/// packet, and the fixtures beside this project, parsed the way tests/PlatoTests does.</summary>
public static class TestSupport
{
    public static readonly string[] StdlibTiers = { "foundation", "geometry", "graphics" };

    /// <summary>The repository root: the nearest ancestor holding <c>stdlib/foundation</c> of the
    /// test output folder, or, for a build into a private <c>--artifacts-path</c> outside the
    /// checkout, of this source file.</summary>
    public static string RepoRoot
        => RootAbove(AppContext.BaseDirectory) ?? RootAbove(Path.GetDirectoryName(SourceFile()))
           ?? throw new DirectoryNotFoundException("no stdlib/foundation above " + AppContext.BaseDirectory);

    private static string RootAbove(string start)
    {
        for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "stdlib", "foundation")))
                return dir.FullName;
        return null;
    }

    private static string SourceFile([System.Runtime.CompilerServices.CallerFilePath] string path = "") => path;

    public static string StdlibFolder(string tier)
        => Path.Combine(RepoRoot, "stdlib", tier);

    public static string FixturePath(string name)
        => Path.Combine(AppContext.BaseDirectory, "fixtures", name);

    public static AstNode ParseFile(string path)
    {
        var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Ara3D.Logging.Logger.Null);
        if (!parser.Succeeded)
            throw new Exception($"Parse failed for {path}: {string.Join("; ", parser.ErrorMessages)}");
        return parser.Cst!.ToAst();
    }

    /// <summary>Every top-level *.plato of the folders, compiled as one program.</summary>
    public static Compilation Compile(IEnumerable<string> folders, params string[] extraFiles)
    {
        var paths = folders.SelectMany(f => Directory.GetFiles(f, "*.plato", SearchOption.TopDirectoryOnly))
            .Concat(extraFiles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, paths.Select(ParseFile).ToList());
        if (!compilation.CompletedCompilation)
            throw new Exception("compilation did not complete: "
                                + string.Join("; ", compilation.InternalErrors.Select(e => e.ToString())
                                    .Concat(compilation.SymbolFactory?.Errors.Select(e => e.Message) ?? Array.Empty<string>())));
        return compilation;
    }

    private static readonly Lazy<Compilation> _foundationWithUnitFixture = new(() =>
        Compile(new[] { StdlibFolder("foundation") }, FixturePath("unit.plato")));

    /// <summary>stdlib/foundation plus fixtures/unit.plato, compiled once per test run.</summary>
    public static Compilation FoundationWithUnitFixture => _foundationWithUnitFixture.Value;

    private static readonly Lazy<Compilation> _fullStdlib = new(() =>
        Compile(StdlibTiers.Select(StdlibFolder).Append(StdlibFolder("tests"))));

    /// <summary>The three shipping tiers and the law packet stdlib/tests, compiled once.</summary>
    public static Compilation FullStdlib => _fullStdlib.Value;

    public static Ara3D.Geometry.Interpreter.Interpreter UnitInterpreter(NumberProfile profile = null)
        => new(FoundationWithUnitFixture, profile ?? NumberProfile.Cpu64);
}
