using Ara3D.Utils;

namespace Plato.CSharpWriter
{
    public static class CSharpWriterExtensions
    {
        public static SymbolWriterCSharp ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new SymbolWriterCSharp(compilation, outputFolder);
            writer.WriteAll("Ara3D.SinglePrecision.cs","float");
            writer.WriteAll("Ara3D.DoublePrecision.cs", "double");
            //writer.WriteAnalyses();
            return writer;
        }
    }
}
