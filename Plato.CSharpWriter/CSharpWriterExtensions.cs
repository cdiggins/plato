using System.Net;
using Ara3D.Utils;

namespace Plato.CSharpWriter
{
    public static class CSharpWriterExtensions
    {
        public static SymbolWriterCSharp ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new SymbolWriterCSharp(compilation, outputFolder);
            writer.WriteAll("Plato.SinglePrecision.g.cs", "float");
            //writer.WriteAll("Plato.DoublePrecision.g.cs", "double");

            // Output documentation 
            /*
            var docWriter = new DocWriter(compilation);
            var fp = outputFolder.RelativeFile("docs.html");
            fp.WriteAllText(docWriter.ToString());
            */

            //Analyze(compilation, outputFolder);

            return writer;
        }

        public static TotalAnalysis Analyze(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            return new TotalAnalysis(compilation, outputFolder);
        }
    }
}
