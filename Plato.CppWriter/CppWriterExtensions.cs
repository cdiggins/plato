using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter
{
    public static class CppWriterExtensions
    {
        public static CppWriter ToCpp(this Compiler.Compilation compilation, DirectoryPath outputFolder,
            CppDialect dialect = CppDialect.Cpp, bool inlineCalls = false)
        {
            var writer = new CppWriter(compilation, outputFolder, dialect) { InlineCalls = inlineCalls };
            writer.WriteAll();
            return writer;
        }

        public static CppWriter ToCuda(this Compiler.Compilation compilation, DirectoryPath outputFolder,
            bool inlineCalls = false)
            => compilation.ToCpp(outputFolder, CppDialect.Cuda, inlineCalls);
    }
}
