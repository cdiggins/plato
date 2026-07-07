using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    public class Config
    {
        public static readonly DirectoryPath ThisFolder = PathUtil.GetCallerSourceFolder();
        public static readonly FilePath ConfigFile = ThisFolder.RelativeFile("config.json");
        // Defaults for standalone use; studio scripts pass input/output on the command line.
        public static string InputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "plato-src");
        public static string OutputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "..", "ara3d-sdk", "src", "Plato.Generated");
    }
}