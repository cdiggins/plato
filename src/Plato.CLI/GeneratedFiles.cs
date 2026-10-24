// Persists a writer's files in their emitted order.
using System.Text;
using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.Geometry.CLI;

internal static class GeneratedFiles
{
    public static void Write(IReadOnlyDictionary<string, StringBuilder> files, DirectoryPath folder, ILogger logger)
    {
        foreach (var (name, contents) in files)
        {
            var path = folder.RelativeFile(name);
            logger.Log($"Writing {name}");
            path.WriteAllText(contents.ToString());
        }
    }
}
