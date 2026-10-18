using System.Diagnostics;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;

using Ara3D.Parakeet;
using Ara3D.Parsing;

namespace Plato.Interpreter.Tests;

/// <summary>
/// The Plato program both arms of the differential test run: stdlib/foundation plus every
/// stdlib/tests/*.laws.plato that resolves and type-checks against foundation alone. A laws file
/// that reaches into geometry or graphics is dropped, with the reason kept in <see cref="Notes"/>.
/// Built once per process; the compilation is profile-independent.
/// </summary>
public sealed class DifferentialProgram
{
    public Compilation Compilation { get; }
    public IReadOnlyList<string> LawFiles { get; }
    public IReadOnlyList<string> Notes { get; }
    public TimeSpan Elapsed { get; }

    private DifferentialProgram(Compilation compilation, IReadOnlyList<string> lawFiles, IReadOnlyList<string> notes, TimeSpan elapsed)
        => (Compilation, LawFiles, Notes, Elapsed) = (compilation, lawFiles, notes, elapsed);

    private static readonly Lazy<DifferentialProgram> _instance = new(Build);
    public static DifferentialProgram Instance => _instance.Value;

    public static AstNode Parse(string path)
    {
        var parser = CommonParsers.PlatoParser(new ParserInput(File.ReadAllText(path), path), Ara3D.Logging.Logger.Null);
        if (!parser.Succeeded)
            throw new InvalidOperationException($"parse failed for {path}: {string.Join("; ", parser.ErrorMessages)}");
        return parser.Cst!.ToAst();
    }

    private static DifferentialProgram Build()
    {
        var sw = Stopwatch.StartNew();
        var root = TestSupport.RepoRoot;
        var foundation = Directory.GetFiles(Path.Combine(root, "stdlib", "foundation"), "*.plato", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .Select(Parse)
            .ToList();
        var kept = new List<(string File, AstNode Ast)>();
        var notes = new List<string>();
        foreach (var file in Directory.GetFiles(Path.Combine(root, "stdlib", "tests"), "*.laws.plato").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            var ast = Parse(file);
            var problem = Problem(foundation.Append(ast), ast);
            notes.Add($"{Path.GetFileName(file)}: {problem ?? "kept"}");
            if (problem == null)
                kept.Add((file, ast));
        }
        var comp = new Compilation(Ara3D.Logging.Logger.Null, foundation.Concat(kept.Select(k => k.Ast)));
        if (!comp.CompletedCompilation)
            throw new InvalidOperationException("foundation plus the kept laws files did not compile: "
                + string.Join("; ", comp.InternalErrors));
        return new DifferentialProgram(comp, kept.Select(k => k.File).ToList(), notes, sw.Elapsed);
    }

    /// <summary>Null when the laws file resolves and its own functions type-check against the
    /// trees it is compiled with; otherwise the first problem, with a count.</summary>
    private static string Problem(IEnumerable<AstNode> trees, AstNode laws)
    {
        var comp = new Compilation(Ara3D.Logging.Logger.Null, trees);
        var resolution = comp.SymbolFactory?.Errors ?? new List<Ara3D.Geometry.Compiler.Symbols.ResolutionError>();
        if (resolution.Count > 0)
            return $"{resolution.Count} symbol resolution error(s), first: {resolution[0].Message}";
        if (!comp.CompletedCompilation)
            return "compilation did not complete: " + string.Join("; ", comp.InternalErrors);
        var owners = laws.GetAllTypes().Select(t => t.Name.Text).ToHashSet(StringComparer.Ordinal);
        var checker = new TypeChecker(comp);
        var errors = comp.FunctionDefinitions
            .Where(f => f.Body != null && f.OwnerType != null && owners.Contains(f.OwnerType.Name))
            .SelectMany(f => checker.Check(f).Errors.Select(e => $"{f.Name}: {e}"))
            .ToList();
        return errors.Count > 0 ? $"{errors.Count} checker error(s), first: {errors[0]}" : null;
    }
}
