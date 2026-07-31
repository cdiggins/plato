using System.Text.Json;
using System.Text.Json.Serialization;
using Ara3D.Utils;

namespace Ara3D.Geometry.Navigation;

/// <summary>JSON persistence (D2 — no database at this scale). The export is self-contained: it
/// carries the source text as well as the tables, so a reloaded index answers every query the
/// original does, including the ones that need to map a line/column onto an offset. That matters
/// most for the first consumer (an MCP tool pack), which can then serve snippets without touching
/// the filesystem.</summary>
public static class NavigationJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Write(NavigationIndex index)
        => JsonSerializer.Serialize(new IndexDto
        {
            Generation = index.Generation,
            Sources = index.Snapshot.Files.Select(f => new SourceDto
            {
                Path = f.Path.ToString()!,
                Text = f.Text
            }).ToList(),
            Defs = index.Defs.ToList(),
            Refs = index.Refs.ToList(),
            Files = index.Files.ToList(),
            Diagnostics = index.Diagnostics.ToList()
        }, Options);

    public static NavigationIndex Read(string json)
    {
        var dto = JsonSerializer.Deserialize<IndexDto>(json, Options)
                  ?? throw new FormatException("Not a navigation index");

        var snapshot = new SourceSnapshot(dto.Sources
            .Select((s, i) => new SourceFile(i, new FilePath(s.Path), s.Text))
            .ToList());

        if (snapshot.Generation != dto.Generation)
            throw new InvalidDataException("Navigation index generation does not match its own sources");

        return new NavigationIndex(snapshot, dto.Defs, dto.Refs, dto.Files, dto.Diagnostics);
    }

    private sealed class IndexDto
    {
        public string Generation { get; set; } = "";
        public List<SourceDto> Sources { get; set; } = new();
        public List<DefRecord> Defs { get; set; } = new();
        public List<RefRecord> Refs { get; set; } = new();
        public List<FileStatus> Files { get; set; } = new();
        public List<Diagnostic> Diagnostics { get; set; } = new();
    }

    private sealed class SourceDto
    {
        public string Path { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
