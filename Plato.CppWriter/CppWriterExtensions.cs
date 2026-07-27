using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter
{
    public static class CppWriterExtensions
    {
        public static CppWriter ToCpp(this Compiler.Compilation compilation, DirectoryPath outputFolder,
            CppDialect dialect = CppDialect.Cpp)
        {
            var writer = new CppWriter(compilation, outputFolder, dialect);
            writer.WriteAll();
            return writer;
        }

        public static CppWriter ToCuda(this Compiler.Compilation compilation, DirectoryPath outputFolder)
            => compilation.ToCpp(outputFolder, CppDialect.Cuda);
    }
}
