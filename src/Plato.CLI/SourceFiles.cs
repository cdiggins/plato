// Discovers source files in root order, without recursion, deduplicating full paths.
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI;

internal static class SourceFiles
{
    public static (IReadOnlyList<FilePath> Files, string? Error) Discover(IReadOnlyList<string> roots)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<FilePath>();
        foreach (var root in roots)
        {
            if (!Directory.Exists(root))
                return (result, $"Input folder not found: {root}");
            var found = 0;
            foreach (var f in new DirectoryPath(root).GetFiles("*.plato"))
            {
                found++;
                if (seen.Add(Path.GetFullPath(f)))
                    result.Add(f);
            }
            if (found == 0)
                return (result, $"No .plato files found in {root}");
        }
        return (result, null);
    }
}
