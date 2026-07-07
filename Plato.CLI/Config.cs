using Ara3D.Utils;

namespace Ara3D.Geometry.CLI
{
    public class Config
    {
        public static readonly DirectoryPath ThisFolder = PathUtil.GetCallerSourceFolder();
        public static readonly FilePath ConfigFile = ThisFolder.RelativeFile("config.json");
        public static string InputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "Plato.Geometry", "plato-src");
        public static string OutputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "Plato.Geometry", "Plato.Core");
    }
}