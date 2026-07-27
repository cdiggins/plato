using System.Security.Cryptography;
using System.Text;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation;

/// <summary>One file of a <see cref="SourceSnapshot"/>: its path, its full text, and a content
/// hash. <see cref="Id"/> is the file's index in the snapshot and is the only file identifier
/// carried by index records.</summary>
public sealed class SourceFile
{
    public int Id { get; }
    public FilePath Path { get; }
    public string Text { get; }
    public string ContentHash { get; }

    public SourceFile(int id, FilePath path, string text)
    {
        Id = id;
        Path = path;
        Text = text ?? throw new ArgumentNullException(nameof(text));
        ContentHash = Hash(text);
    }

    public static string Hash(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}

/// <summary>An immutable, filesystem-decoupled set of Plato source files. Every index is built
/// from one of these and from nothing else — no file is read inside the build pipeline. That is
/// what lets unsaved editor buffers (<see cref="FromTexts"/>) and a future incremental indexer
/// reuse the same path as a folder scan.</summary>
public sealed class SourceSnapshot
{
    public IReadOnlyList<SourceFile> Files { get; }

    /// <summary>Hash over the ordered (path, content-hash) pairs. Two snapshots with the same
    /// generation produce byte-identical indexes.</summary>
    public string Generation { get; }

    private readonly Dictionary<string, SourceFile> _byPath;

    public SourceSnapshot(IReadOnlyList<SourceFile> files)
    {
        Files = files ?? throw new ArgumentNullException(nameof(files));
        _byPath = files.ToDictionary(f => Key(f.Path), f => f);
        Generation = ComputeGeneration(files);
    }

    public SourceFile? Find(FilePath path)
        => _byPath.TryGetValue(Key(path), out var f) ? f : null;

    public static SourceSnapshot FromDirectories(IReadOnlyList<DirectoryPath> roots)
        => FromFiles(roots.SelectMany(r => r.GetFiles("*.plato", recurse: true)).ToList());

    public static SourceSnapshot FromDirectory(DirectoryPath root)
        => FromDirectories(new[] { root });

    public static SourceSnapshot FromFiles(IReadOnlyList<FilePath> paths)
        => FromTexts(Ordered(paths).ToDictionary(p => p, p => File.ReadAllText(p)));

    public static SourceSnapshot FromTexts(IReadOnlyDictionary<FilePath, string> texts)
        => new(Ordered(texts.Keys.ToList())
            .Select((p, i) => new SourceFile(i, p, texts[p]))
            .ToList());

    /// <summary>Deterministic file order: the snapshot's order is the binder's declaration order,
    /// so it must not depend on how the filesystem enumerated the roots.</summary>
    private static IEnumerable<FilePath> Ordered(IReadOnlyList<FilePath> paths)
        => paths.GroupBy(Key, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => g.First());

    private static string Key(FilePath path)
        => path.ToString()!.Replace('\\', '/').ToLowerInvariant();

    private static string ComputeGeneration(IReadOnlyList<SourceFile> files)
    {
        var sb = new StringBuilder();
        foreach (var f in files)
            sb.Append(Key(f.Path)).Append('\0').Append(f.ContentHash).Append('\n');
        return SourceFile.Hash(sb.ToString());
    }
}
