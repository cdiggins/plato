using Ara3D.Utils;

namespace PlatoWinFormsEditor
{
    public class Config
    {
        public static readonly DirectoryPath ThisFolder = PathUtil.GetCallerSourceFolder();
        public static readonly FilePath ConfigFile = ThisFolder.RelativeFile("config.json");
        public static readonly Config Current = Load(ConfigFile);
        public Config Save(FilePath fp) => fp.WriteAsJson(this);
        public static Config Load(FilePath? fp) => fp.LoadObjectAsJsonOrWriteDefault<Config>();

        public string InputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "Plato.Geometry", "plato-src");
        public string OutputFolder = SourceCodeLocation.GetFolder().RelativeFolder("..", "..", "Plato.Geometry", "Plato.Geometry.CSharpx");
    }
}
