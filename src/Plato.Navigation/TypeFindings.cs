using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Navigation;

/// <summary>One type-check error, located in source. Line is 1-based; 0 when no source node is known.</summary>
public sealed record TypeFinding(string File, int Line, string Function, string? Owner, string Code, string Message)
{
    public override string ToString()
        => $"{File}:{Line}: {Code} in {(Owner == null ? "" : Owner + ".")}{Function}: {Message}";
}

/// <summary>The type checker's errors over a whole compilation, each located at the symbol the
/// checker blamed when it mapped one to source, otherwise at the function being checked (the
/// Linter's fallback).</summary>
public static class TypeFindings
{
    /// <summary>The errors of every bodied function, or only of those whose declaration lies in a
    /// file <paramref name="inFile"/> accepts (by the file path the parser recorded).</summary>
    public static IReadOnlyList<TypeFinding> Check(Compilation compilation, Func<string, bool>? inFile = null)
    {
        var checker = new TypeChecker(compilation);
        return compilation.FunctionDefinitions
            .Where(f => f.Body != null && (inFile == null || inFile(FileOf(compilation, f))))
            .Select(checker.Check)
            .Where(r => !r.Succeeded)
            .SelectMany(r => r.Errors.Select(d => Locate(compilation, r, d)))
            .ToList();
    }

    /// <summary>The file a function is declared in, as the parser recorded it; empty when unknown.</summary>
    public static string FileOf(Compilation compilation, FunctionDef f)
        => NodeOf(compilation, f)?.GetRange()?.FilePath.ToString() ?? "";

    private static TypeFinding Locate(Compilation compilation, TypeCheckResult r, CheckDiagnostic d)
    {
        var node = (d.Origin != null ? compilation.GetAstNode(d.Origin) : null) ?? NodeOf(compilation, r.Function);
        var range = node?.GetRange();
        return new TypeFinding(
            range?.FilePath.ToString() ?? "",
            range != null ? range.BeginLineIndex + 1 : 0,
            r.Function?.Name ?? "?",
            r.Function?.OwnerType?.Name,
            d.Code,
            d.Message);
    }

    private static AstNode? NodeOf(Compilation compilation, FunctionDef? fd)
    {
        if (fd == null)
            return null;
        var direct = compilation.GetAstNode(fd);
        if (direct != null)
            return direct;
        var method = fd.OwnerType?.Methods.FirstOrDefault(m => m.Function == fd);
        return method != null ? compilation.GetAstNode(method) : null;
    }
}
