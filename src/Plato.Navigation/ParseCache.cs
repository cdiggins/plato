using Ara3D.Geometry.AST;

namespace Ara3D.Geometry.Navigation;

/// <summary>Per-file AST cache — the only optimization v2 needs (§6). Parsing is ~97% of a build
/// and is per-file and independent; binding is whole-program and is always rerun in full.
///
/// The key is (path, content hash), not the content hash alone. Every <see cref="AstNode"/> carries
/// a range whose file path comes from the <c>ParserInput</c> used at parse time, so reusing an AST
/// across two files with identical text would silently attribute every span to the wrong file.</summary>
public sealed class ParseCache
{
    private readonly Dictionary<Key, Entry> _entries = new();

    public int Hits { get; private set; }
    public int Misses { get; private set; }
    public int Count => _entries.Count;

    public ParsedFile Parse(SourceFile file)
    {
        var key = KeyOf(file);
        if (_entries.TryGetValue(key, out var entry))
        {
            Hits++;
            return new ParsedFile(file, entry.Ast, entry.ParseErrors);
        }

        Misses++;
        var parsed = BoundSnapshot.ParseFile(file);
        _entries[key] = new Entry(parsed.Ast, parsed.ParseErrors);
        return parsed;
    }

    /// <summary>Drops every entry the snapshot no longer uses. Without it an editing session that
    /// rewrites one file repeatedly keeps every intermediate version alive.</summary>
    public void Retain(SourceSnapshot snapshot)
    {
        var live = snapshot.Files.Select(KeyOf).ToHashSet();
        foreach (var key in _entries.Keys.Where(k => !live.Contains(k)).ToList())
            _entries.Remove(key);
    }

    public void ResetCounters() => (Hits, Misses) = (0, 0);

    private static Key KeyOf(SourceFile file)
        => new(SourceSnapshot.PathKey(file.Path), file.ContentHash);

    private readonly record struct Key(string Path, string ContentHash);

    private sealed record Entry(AstFile? Ast, IReadOnlyList<string> ParseErrors);
}
