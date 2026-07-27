using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation;

public enum SearchKind
{
    Exact,
    Prefix,
    All
}

/// <summary>An immutable, queryable index of a <see cref="SourceSnapshot"/>. Immutability is the
/// concurrency story: MCP servers and IDE hosts hold a reference and swap it for a newer one, so
/// there is no locking, no invalidation protocol, and no torn read.</summary>
public sealed class NavigationIndex
{
    public SourceSnapshot Snapshot { get; }
    public IReadOnlyList<DefRecord> Defs { get; }
    public IReadOnlyList<RefRecord> Refs { get; }
    public IReadOnlyList<FileStatus> Files { get; }
    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    /// <summary>Hash of the snapshot this index was built from — the staleness check.</summary>
    public string Generation => Snapshot.Generation;

    private readonly DefRecord[][] _defsByFile;
    private readonly RefRecord[][] _refsByFile;
    private readonly Dictionary<string, int[]> _defsByName;
    private readonly string[] _sortedNames;
    private readonly int[][] _refsByDef;

    public NavigationIndex(SourceSnapshot snapshot, IReadOnlyList<DefRecord> defs, IReadOnlyList<RefRecord> refs,
        IReadOnlyList<FileStatus> files, IReadOnlyList<Diagnostic> diagnostics)
    {
        Snapshot = snapshot;
        Defs = defs;
        Refs = refs;
        Files = files;
        Diagnostics = diagnostics;

        var n = snapshot.Files.Count;
        _defsByFile = Bucket(n, defs, d => d.FileId, d => d.NameSpan.Begin);
        _refsByFile = Bucket(n, refs, r => r.FileId, r => r.Span.Begin);

        _defsByName = defs.GroupBy(d => d.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Select(d => d.Id).ToArray(), StringComparer.Ordinal);
        _sortedNames = _defsByName.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();

        _refsByDef = new int[defs.Count][];
        var buckets = new List<int>[defs.Count];
        foreach (var r in refs)
        foreach (var t in r.Targets)
            (buckets[t] ??= new List<int>()).Add(r.Id);
        for (var i = 0; i < defs.Count; i++)
            _refsByDef[i] = buckets[i]?.ToArray() ?? Array.Empty<int>();
    }

    public static NavigationIndex Build(SourceSnapshot snapshot, ILogger? logger = null)
        => NavigationBuilder.Build(BoundSnapshot.Create(snapshot, logger));

    public static NavigationIndex Build(BoundSnapshot bound)
        => NavigationBuilder.Build(bound);

    public SymbolHit? FindAt(FilePath file, int offset)
    {
        var f = Snapshot.Find(file);
        if (f == null)
            return null;

        var def = Smallest(_defsByFile[f.Id], offset, d => d.NameSpan);
        var reference = Smallest(_refsByFile[f.Id], offset, r => r.Span);
        return def == null && reference == null ? null : new SymbolHit(def, reference);
    }

    public SymbolHit? FindAt(FilePath file, int line, int column)
    {
        var f = Snapshot.Find(file);
        return f == null ? null : FindAt(file, OffsetOf(f.Text, line, column));
    }

    /// <summary>Every definition a hit could mean. A definition site resolves to itself; a
    /// reference resolves to all of its targets (all overloads of a function group, D4).</summary>
    public IReadOnlyList<DefRecord> GetDefinitions(SymbolHit? hit)
        => hit == null
            ? Array.Empty<DefRecord>()
            : hit.Ref != null
                ? hit.Ref.Targets.Select(t => Defs[t]).ToList()
                : hit.Def != null
                    ? new[] { hit.Def }
                    : Array.Empty<DefRecord>();

    public IReadOnlyList<RefRecord> FindReferences(int defId)
        => defId < 0 || defId >= _refsByDef.Length
            ? Array.Empty<RefRecord>()
            : _refsByDef[defId].Select(i => Refs[i]).ToList();

    public IReadOnlyList<DefRecord> Search(string name, SearchKind kind = SearchKind.All)
    {
        var ids = new List<int>();
        if (kind is SearchKind.Exact or SearchKind.All && _defsByName.TryGetValue(name, out var exact))
            ids.AddRange(exact);

        if (kind is SearchKind.Prefix or SearchKind.All)
        {
            var start = LowerBound(_sortedNames, name);
            for (var i = start; i < _sortedNames.Length && _sortedNames[i].StartsWith(name, StringComparison.Ordinal); i++)
                if (kind == SearchKind.Prefix || !string.Equals(_sortedNames[i], name, StringComparison.Ordinal))
                    ids.AddRange(_defsByName[_sortedNames[i]]);
        }

        return ids.Select(i => Defs[i]).ToList();
    }

    /// <summary>Every definition in one file, in source order. Built from the AST alone, so it
    /// still works for a file whose binding degraded or whose neighbours failed to parse.</summary>
    public IReadOnlyList<DefRecord> Outline(FilePath file)
    {
        var f = Snapshot.Find(file);
        return f == null ? Array.Empty<DefRecord>() : _defsByFile[f.Id];
    }

    public FileStatus? StatusOf(FilePath file)
    {
        var f = Snapshot.Find(file);
        return f == null ? null : Files[f.Id];
    }

    public static int OffsetOf(string text, int line, int column)
    {
        var offset = 0;
        for (var i = 0; i < line; i++)
        {
            var next = text.IndexOf('\n', offset);
            if (next < 0)
                return text.Length;
            offset = next + 1;
        }
        return Math.Min(offset + column, text.Length);
    }

    private static T[][] Bucket<T>(int fileCount, IReadOnlyList<T> items, Func<T, int> fileOf, Func<T, int> beginOf)
    {
        var lists = new List<T>[fileCount];
        foreach (var item in items)
        {
            var f = fileOf(item);
            if (f >= 0 && f < fileCount)
                (lists[f] ??= new List<T>()).Add(item);
        }

        var result = new T[fileCount][];
        for (var i = 0; i < fileCount; i++)
            result[i] = lists[i] == null ? Array.Empty<T>() : lists[i].OrderBy(beginOf).ToArray();
        return result;
    }

    /// <summary>The smallest span containing the offset. Spans nest (a type argument sits inside
    /// its type node), so "first containing" is not good enough.</summary>
    private static T? Smallest<T>(T[] sorted, int offset, Func<T, Span> spanOf) where T : class
    {
        T? best = null;
        var bestLength = int.MaxValue;
        for (var i = UpperBound(sorted, offset, spanOf) - 1; i >= 0; i--)
        {
            var span = spanOf(sorted[i]);
            if (span.Contains(offset) && span.Length < bestLength)
                (best, bestLength) = (sorted[i], span.Length);

            // Everything further back begins earlier; stop once no earlier span could still reach.
            if (span.End < offset - MaxSpanLength)
                break;
        }
        return best;
    }

    private const int MaxSpanLength = 4096;

    private static int UpperBound<T>(T[] sorted, int offset, Func<T, Span> spanOf)
    {
        int lo = 0, hi = sorted.Length;
        while (lo < hi)
        {
            var mid = (lo + hi) / 2;
            if (spanOf(sorted[mid]).Begin <= offset) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }

    private static int LowerBound(string[] sorted, string value)
    {
        int lo = 0, hi = sorted.Length;
        while (lo < hi)
        {
            var mid = (lo + hi) / 2;
            if (string.CompareOrdinal(sorted[mid], value) < 0) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }
}
