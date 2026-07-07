using Ara3D.Utils;

namespace Ara3D.Geometry.RustWriter
{
    public static class RustWriterExtensions
    {
        public static RustWriter ToRust(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new RustWriter(compilation, outputFolder);
            writer.WriteAll();
            return writer;
        }
    }
}
