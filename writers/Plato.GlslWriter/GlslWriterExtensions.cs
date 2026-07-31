using Ara3D.Utils;

namespace Ara3D.Geometry.GlslWriter
{
    public static class GlslWriterExtensions
    {
        public static GlslWriter ToGlsl(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new GlslWriter(compilation, outputFolder);
            writer.WriteAll();
            return writer;
        }
    }
}
