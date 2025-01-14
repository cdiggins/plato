using System.Net;
using Ara3D.Utils;

namespace Plato.CSharpWriter
{
    public static class CSharpWriterExtensions
    {
        public static SymbolWriterCSharp ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new SymbolWriterCSharp(compilation, outputFolder);
#if CHANGE_PRECISION
            writer.WriteAll("Plato.SinglePrecision.g.cs", "float");
#endif
            writer.WriteAll("Plato.DoublePrecision.g.cs", "double");

            var docWriter = new DocWriter(compilation);
            var fp = outputFolder.RelativeFile("docs.html");
            fp.WriteAllText(docWriter.ToString());

            Analyze(compilation, outputFolder);

            return writer;
        }

        public static TotalAnalysis Analyze(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            return new TotalAnalysis(compilation, outputFolder);
        }
    }
}
