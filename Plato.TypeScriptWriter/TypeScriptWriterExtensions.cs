using Ara3D.Utils;

namespace Ara3D.Geometry.TypeScriptWriter
{
    public static class TypeScriptWriterExtensions
    {
        public static TypeScriptWriter ToTypeScript(this Compiler.Compilation compilation, DirectoryPath outputFolder, bool useTir = true)
        {
            var writer = new TypeScriptWriter(compilation, outputFolder) { UseTir = useTir };
            writer.WriteAll();
            return writer;
        }
    }
}
