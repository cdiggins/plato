using System.Diagnostics;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;

namespace Ara3D.Geometry.Navigation;

/// <summary>The result of parsing one file. A failed parse is not fatal: the file simply
/// contributes no declarations, and every other file stays fully navigable (D3).</summary>
public sealed class ParsedFile
{
    public SourceFile File { get; }
    public AstFile? Ast { get; }
    public IReadOnlyList<string> ParseErrors { get; }
    public bool Parsed => Ast != null;

    public ParsedFile(SourceFile file, AstFile? ast, IReadOnlyList<string> parseErrors)
        => (File, Ast, ParseErrors) = (file, ast, parseErrors);
}

/// <summary>Parse + bind of a whole snapshot, with every failure mode turned into data.
/// <see cref="SymbolFactory"/> already accumulates <see cref="SymbolFactory.Errors"/> and keeps
/// going, but it can still return null on an unrecognized member declaration or throw on a null
/// inherits/implements node — so the call is wrapped and a hard abort degrades the whole build to
/// parse level rather than killing it.</summary>
public sealed class BoundSnapshot
{
    public SourceSnapshot Snapshot { get; }
    public IReadOnlyList<ParsedFile> Files { get; }
    public SymbolFactory? Factory { get; }
    public string? BindAbortReason { get; }
    public TimeSpan ParseTime { get; }
    public TimeSpan BindTime { get; }

    public bool Bound => Factory != null && BindAbortReason == null;

    public IReadOnlyList<ResolutionError> ResolutionErrors
        => Factory?.Errors ?? (IReadOnlyList<ResolutionError>)Array.Empty<ResolutionError>();

    private BoundSnapshot(SourceSnapshot snapshot, IReadOnlyList<ParsedFile> files, SymbolFactory? factory,
        string? bindAbortReason, TimeSpan parseTime, TimeSpan bindTime)
    {
        Snapshot = snapshot;
        Files = files;
        Factory = factory;
        BindAbortReason = bindAbortReason;
        ParseTime = parseTime;
        BindTime = bindTime;
    }

    public static BoundSnapshot Create(SourceSnapshot snapshot, ILogger? logger = null)
    {
        var parseTimer = Stopwatch.StartNew();
        var files = snapshot.Files.Select(Parse).ToList();
        parseTimer.Stop();

        var declarations = files
            .Where(f => f.Parsed)
            .SelectMany(f => f.Ast!.Types)
            .ToList();

        var factory = new SymbolFactory(logger ?? Logger.Null);
        string? abort = null;
        var bindTimer = Stopwatch.StartNew();
        try
        {
            if (factory.CreateTypeDefs(declarations) == null)
                abort = "SymbolFactory.CreateTypeDefs returned null (unrecognized member declaration)";
        }
        catch (Exception e)
        {
            abort = $"SymbolFactory.CreateTypeDefs threw: {e.Message}";
        }
        bindTimer.Stop();

        return new BoundSnapshot(snapshot, files, factory, abort, parseTimer.Elapsed, bindTimer.Elapsed);
    }

    private static ParsedFile Parse(SourceFile file)
    {
        var parser = CommonParsers.PlatoParser(new ParserInput(file.Text, file.Path), Logger.Null);
        if (!parser.Succeeded)
            return new ParsedFile(file, null, parser.ErrorMessages.Select(e => e.ToString()!).ToList());

        return parser.Cst?.ToAst() is AstFile ast
            ? new ParsedFile(file, ast, Array.Empty<string>())
            : new ParsedFile(file, null, new[] { "Parse produced no AstFile" });
    }
}
