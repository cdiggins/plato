using System.Text.Json;
using System.Text.Json.Serialization;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation.CLI;

/// <summary>Long-lived NDJSON stdio host for IDE clients. One line in, one line out; holds an
/// <see cref="IncrementalIndexer"/> so save/reload pays for changed files only.</summary>
internal static class Serve
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static int Run(IReadOnlyList<DirectoryPath> roots)
    {
        var indexer = new IncrementalIndexer();
        var texts = LoadDisk(roots);
        var index = indexer.Update(SourceSnapshot.FromTexts(texts));
        Write(new
        {
            op = "ready",
            generation = index.Generation,
            files = index.Snapshot.Files.Count,
            defs = index.Defs.Count,
            refs = index.Refs.Count,
            roots = roots.Select(r => r.FullPath).ToList()
        });

        string? line;
        while ((line = Console.In.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Always echo the request id on failure — without it the IDE client parks forever.
            int? id = null;
            try
            {
                var req = JsonSerializer.Deserialize<Request>(line, Json)
                          ?? throw new FormatException("null request");
                id = req.Id;
                Write(Handle(req, indexer, roots, texts));
            }
            catch (Exception e)
            {
                Write(new { id, ok = false, error = e.Message });
            }
        }

        return 0;
    }

    private static object Handle(Request req, IncrementalIndexer indexer,
        IReadOnlyList<DirectoryPath> roots, Dictionary<FilePath, string> texts)
    {
        var id = req.Id;
        var index = indexer.Current;

        switch (req.Op)
        {
            case "ping":
                return Ok(id, new { generation = index.Generation });

            case "status":
                return Ok(id, Status(index, indexer));

            case "reload":
            {
                RefreshFromDisk(roots, texts);
                index = indexer.Update(SourceSnapshot.FromTexts(texts));
                return Ok(id, Status(index, indexer));
            }

            case "update":
            {
                if (req.Files != null)
                    foreach (var f in req.Files)
                        if (!string.IsNullOrWhiteSpace(f.Path))
                            texts[new FilePath(f.Path)] = f.Text ?? "";

                index = indexer.Update(SourceSnapshot.FromTexts(texts));
                return Ok(id, Status(index, indexer));
            }

            case "definition":
            {
                var hit = Hit(index, req);
                if (hit == null)
                    return Ok(id, new { locations = Array.Empty<object>() });

                var locations = index.GetDefinitions(hit)
                    .Where(d => d.NameSpan.HasValue)
                    .Select(d => Location(index, d.FileId, d.NameSpan, d.Name, d.Kind.ToString(), d.Id))
                    .ToList();
                return Ok(id, new { locations });
            }

            case "references":
            {
                var hit = Hit(index, req);
                if (hit == null)
                    return Ok(id, new { locations = Array.Empty<object>() });

                var defs = index.GetDefinitions(hit);
                var seen = new HashSet<(int file, int begin)>();
                var locations = new List<Loc>();

                if (req.IncludeDeclaration != false)
                    foreach (var d in defs.Where(d => d.NameSpan.HasValue))
                        if (seen.Add((d.FileId, d.NameSpan.Begin)))
                            locations.Add(Location(index, d.FileId, d.NameSpan, d.Name, d.Kind.ToString(), d.Id));

                foreach (var d in defs)
                foreach (var r in index.FindReferences(d.Id))
                    if (seen.Add((r.FileId, r.Span.Begin)))
                        locations.Add(Location(index, r.FileId, r.Span, r.Name, r.Kind.ToString(), null));

                locations.Sort(static (a, b) =>
                {
                    var cmp = string.CompareOrdinal(a.File, b.File);
                    if (cmp != 0) return cmp;
                    cmp = a.Line.CompareTo(b.Line);
                    return cmp != 0 ? cmp : a.Character.CompareTo(b.Character);
                });
                return Ok(id, new { locations });
            }

            case "hover":
            {
                var hit = Hit(index, req);
                if (hit == null)
                    return Ok(id, new { contents = Array.Empty<object>() });

                var defs = index.GetDefinitions(hit);
                var contents = defs
                    .Select(d => HoverContent(index, d))
                    .Where(c => c != null)
                    .Take(8)
                    .ToList();

                object? range = hit.Span.HasValue
                    ? new
                    {
                        line = hit.Span.BeginLine,
                        character = hit.Span.BeginColumn,
                        endLine = hit.Span.EndLine,
                        endCharacter = hit.Span.EndColumn
                    }
                    : null;

                return Ok(id, new { contents, range, name = hit.Name });
            }

            default:
                return new { id, ok = false, error = $"Unknown op '{req.Op}'" };
        }
    }

    private static SymbolHit? Hit(NavigationIndex index, Request req)
    {
        if (string.IsNullOrWhiteSpace(req.File) || req.Line == null || req.Column == null)
            throw new ArgumentException("definition/references/hover need file, line, column (0-based)");

        var file = ResolveFile(index, req.File)
                   ?? throw new ArgumentException($"No indexed file matches '{req.File}'");
        return index.FindAt(file.Path, req.Line.Value, req.Column.Value);
    }

    private static SourceFile? ResolveFile(NavigationIndex index, string path)
    {
        foreach (var candidate in PathCandidates(path))
        {
            var direct = index.Snapshot.Find(new FilePath(candidate));
            if (direct != null)
                return direct;
        }

        var needle = path.Replace('\\', '/').TrimStart('.', '/');
        var matches = index.Snapshot.Files
            .Where(f => PathText(f.Path).EndsWith(needle, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    /// <summary>Editors send absolute paths; disk scans may store Value as relative or absolute.
    /// Try both the raw string and its FullPath so PathKey (which keys on Value) still hits.</summary>
    private static IEnumerable<string> PathCandidates(string path)
    {
        yield return path;
        string full;
        try { full = Path.GetFullPath(path); }
        catch { yield break; }
        if (!string.Equals(full, path, StringComparison.OrdinalIgnoreCase))
            yield return full;
    }

    private static string PathText(FilePath path)
    {
        var value = path.ToString()!.Replace('\\', '/');
        var full = path.FullPath.Replace('\\', '/');
        return string.IsNullOrEmpty(full) ? value : full;
    }

    private static Loc Location(NavigationIndex index, int fileId, Span span, string name, string kind, int? defId)
        => new(
            PathText(index.Snapshot.Files[fileId].Path),
            span.BeginLine, span.BeginColumn, span.EndLine, span.EndColumn,
            name, kind, defId);

    private static object? HoverContent(NavigationIndex index, DefRecord d)
    {
        var span = d.DeclSpan.HasValue ? d.DeclSpan : d.NameSpan;
        var code = span.HasValue ? DeclText(index.Snapshot.Files[d.FileId].Text, span) : null;
        if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(d.Signature))
            return null;

        return new
        {
            kind = d.Kind.ToString(),
            name = d.Name,
            signature = d.Signature,
            code,
            file = PathText(index.Snapshot.Files[d.FileId].Path),
            line = span.HasValue ? span.BeginLine : (int?)null
        };
    }

    /// <summary>Declaration text plus contiguous <c>//</c> docs immediately above it.</summary>
    private static string DeclText(string text, Span span)
    {
        var begin = span.Begin;
        var end = Math.Min(span.End, text.Length);
        if (begin < 0 || begin >= text.Length || end <= begin)
            return "";

        begin = IncludeLeadingComments(text, begin);
        var slice = text[begin..end];
        if (slice.Length > MaxHoverChars)
            slice = slice[..MaxHoverChars] + "\n// …";
        return TrimTrailingComments(slice.TrimEnd('\r', '\n'));
    }

    private const int MaxHoverChars = 2500;

    private static string TrimTrailingComments(string slice)
    {
        var lines = slice.Replace("\r\n", "\n").Split('\n');
        var end = lines.Length;
        while (end > 0)
        {
            var t = lines[end - 1].TrimStart();
            if (t.Length == 0 || t.StartsWith("//", StringComparison.Ordinal))
            {
                end--;
                continue;
            }
            break;
        }
        return end == lines.Length ? slice : string.Join("\n", lines[..end]);
    }

    private static int IncludeLeadingComments(string text, int begin)
    {
        var lineStart = begin;
        while (lineStart > 0 && text[lineStart - 1] != '\n')
            lineStart--;

        var scan = lineStart;
        while (scan > 0)
        {
            var prevEnd = scan;
            if (prevEnd > 0 && text[prevEnd - 1] == '\n') prevEnd--;
            if (prevEnd > 0 && text[prevEnd - 1] == '\r') prevEnd--;
            var prevStart = prevEnd;
            while (prevStart > 0 && text[prevStart - 1] != '\n')
                prevStart--;

            var line = text[prevStart..prevEnd].TrimEnd('\r').TrimStart();
            // Only the contiguous // block immediately above — stop at blanks/code.
            if (!line.StartsWith("//", StringComparison.Ordinal))
                break;
            scan = prevStart;
        }
        return scan;
    }

    private static object Status(NavigationIndex index, IncrementalIndexer indexer)
        => new
        {
            generation = index.Generation,
            files = index.Snapshot.Files.Count,
            defs = index.Defs.Count,
            refs = index.Refs.Count,
            diagnostics = index.Diagnostics.Count,
            lastUpdate = new
            {
                filesParsed = indexer.LastUpdate.FilesParsed,
                filesReused = indexer.LastUpdate.FilesReused,
                parseMs = indexer.LastUpdate.ParseTime.TotalMilliseconds,
                bindMs = indexer.LastUpdate.BindTime.TotalMilliseconds,
                totalMs = indexer.LastUpdate.TotalTime.TotalMilliseconds
            }
        };

    private static object Ok(int? id, object result)
        => new Dictionary<string, object?>
        {
            ["id"] = id,
            ["ok"] = true,
            ["result"] = result
        };

    private static Dictionary<FilePath, string> LoadDisk(IReadOnlyList<DirectoryPath> roots)
        => SourceSnapshot.FromDirectories(roots).Files.ToDictionary(f => f.Path, f => f.Text);

    private static void RefreshFromDisk(IReadOnlyList<DirectoryPath> roots, Dictionary<FilePath, string> texts)
    {
        texts.Clear();
        foreach (var f in SourceSnapshot.FromDirectories(roots).Files)
            texts[f.Path] = f.Text;
    }

    private static void Write(object payload)
    {
        Console.Out.WriteLine(JsonSerializer.Serialize(payload, Json));
        Console.Out.Flush();
    }

    private sealed class Request
    {
        public int? Id { get; set; }
        public string Op { get; set; } = "";
        public string? File { get; set; }
        public int? Line { get; set; }
        public int? Column { get; set; }
        public bool? IncludeDeclaration { get; set; }
        public List<FileUpdate>? Files { get; set; }
    }

    private sealed class FileUpdate
    {
        public string Path { get; set; } = "";
        public string? Text { get; set; }
    }

    private sealed record Loc(
        string File, int Line, int Character, int EndLine, int EndCharacter,
        string Name, string Kind, int? Id);
}
