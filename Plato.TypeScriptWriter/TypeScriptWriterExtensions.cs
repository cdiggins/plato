using Ara3D.Utils;

namespace Ara3D.Geometry.TypeScriptWriter
{
    public static class TypeScriptWriterExtensions
    {
        public static TypeScriptWriter ToTypeScript(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new TypeScriptWriter(compilation, outputFolder);
            writer.WriteAll();
            return writer;
        }
    }
}
