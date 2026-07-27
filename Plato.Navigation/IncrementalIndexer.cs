using System.Diagnostics;
using Ara3D.Logging;

namespace Ara3D.Geometry.Navigation;

/// <summary>What one <see cref="IncrementalIndexer.Update"/> cost, and how much of it the cache
/// avoided. <see cref="FilesParsed"/> is the number of files that actually went through the parser.</summary>
public readonly record struct UpdateStats(
    int Files,
    int FilesParsed,
    int FilesReused,
    TimeSpan ParseTime,
    TimeSpan BindTime,
    TimeSpan TotalTime);

/// <summary>The v2 update path (§6): reparse changed files only, rebind everything, swap the index.
///
/// Per-file rebinding is deliberately not attempted — <c>SymbolFactory</c> binds every library and
/// type into scopes shared across all files before it resolves any body, so there is no per-file
/// bind to redo. That is affordable because bind is ~3% of a build.
///
/// <see cref="Update"/> is defined to produce exactly what <see cref="NavigationIndex.Build(SourceSnapshot)"/>
/// would produce for the same snapshot; reusing an AST is never allowed to change an answer.</summary>
public sealed class IncrementalIndexer
{
    private readonly ILogger? _logger;

    public IncrementalIndexer(ILogger? logger = null)
    {
        _logger = logger;
        Current = NavigationIndex.Build(new SourceSnapshot(Array.Empty<SourceFile>()));
    }

    public NavigationIndex Current { get; private set; }
    public ParseCache Cache { get; } = new();
    public UpdateStats LastUpdate { get; private set; }

    public NavigationIndex Update(SourceSnapshot next)
    {
        var timer = Stopwatch.StartNew();
        var (hits, misses) = (Cache.Hits, Cache.Misses);

        var bound = BoundSnapshot.Create(next, Cache, _logger);
        Cache.Retain(next);
        var index = NavigationIndex.Build(bound);
        timer.Stop();

        LastUpdate = new UpdateStats(next.Files.Count, Cache.Misses - misses, Cache.Hits - hits,
            bound.ParseTime, bound.BindTime, timer.Elapsed);
        Current = index;
        return index;
    }
}
