using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler;
using Ara3D.Logging;

namespace Ara3D.Geometry.Navigation;

/// <summary>The seam between the navigation index and the full compiler front-end: parse a
/// snapshot through the shared <see cref="ParseCache"/>, then hand the cached ASTs to
/// <see cref="Compilation"/>. <c>Compilation</c> never reads disk — it consumes ASTs — so on a
/// warm cache the marginal cost of a check is resolve + reified types + analyses, not parsing.
///
/// Safe to run against the same cached ASTs the index binds: the binder and the compilation both
/// write only to their own symbols, never to AST nodes (see the Plato.Navigation README, v2), and
/// <see cref="IncrementalIndexer"/> already rebinds the same cached ASTs on every update.</summary>
public static class CheckSupport
{
    public static IReadOnlyList<ParsedFile> Parse(SourceSnapshot snapshot, ParseCache cache)
        => snapshot.Files.Select(cache.Parse).ToList();

    public static Compilation Compile(IEnumerable<ParsedFile> files, ILogger? logger = null)
        => new(logger ?? Ara3D.Logging.Logger.Null, files.Where(f => f.Parsed).Select(f => (AstNode)f.Ast!));

    public static Compilation Compile(SourceSnapshot snapshot, ParseCache cache, ILogger? logger = null)
        => Compile(Parse(snapshot, cache), logger);
}
