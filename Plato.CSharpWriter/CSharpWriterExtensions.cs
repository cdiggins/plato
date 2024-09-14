using Ara3D.Utils;

namespace Plato.CSharpWriter
{
    public static class CSharpWriterExtensions
    {
        public static SymbolWriterCSharp ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new SymbolWriterCSharp(compilation, outputFolder);
#if CHANGE_PRECISION
            writer.WriteAll("Plato.SinglePrecision.g.cs","float");
#endif
            writer.WriteAll("Plato.DoublePrecision.g.cs", "double");
            //writer.WriteAnalyses();

            var docWriter = new DocWriter(compilation);
            var fp = outputFolder.RelativeFile("docs.md");
            fp.WriteAllText(docWriter.ToString());

            return writer;
        }
    }
}
