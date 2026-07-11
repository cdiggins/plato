using Ara3D.Utils;

namespace Ara3D.Geometry.RustWriter
{
    public static class RustWriterExtensions
    {
        public static RustWriter ToRust(this Compiler.Compilation compilation, DirectoryPath outputFolder, bool useTir = true)
        {
            var writer = new RustWriter(compilation, outputFolder) { UseTir = useTir };
            writer.WriteAll();
            return writer;
        }
    }
}
