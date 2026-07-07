using Ara3D.Utils;

namespace PlatoWinFormsEditor
{
    public class Config
    {
        public static readonly DirectoryPath ThisFolder = PathUtil.GetCallerSourceFolder();
        public static readonly FilePath ConfigFile = ThisFolder.RelativeFile("config.json");
        public static string InputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "..", "plato-src");
        public static string OutputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "..", "src", "Plato.Generated");
    }
}
