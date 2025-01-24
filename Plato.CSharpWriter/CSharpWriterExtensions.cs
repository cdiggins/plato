using System.Linq;
using System.Net;
using System.Text;
using Ara3D.Utils;
using Plato.Compiler.Symbols;
using Plato.Compiler.Types;

namespace Plato.CSharpWriter
{
    public static class CSharpWriterExtensions
    {
        public static StringBuilder OutputInterfaces(TypeExpression te, StringBuilder sb, string indent)
        {
            sb.AppendLine($"{indent}- {te}");
            foreach (var i in te.Def.Inherits)
            {
                OutputInterfaces(i, sb, indent + "  ");
            }
            return sb;
        }

        public static void OutputTypeInterface(this Compiler.Compilation compilation, StringBuilder sb)
        {
            var types = compilation
                .AllTypeAndLibraryDefinitions
                .Where(t => t != null && t.IsConcrete())
                .OrderBy(t => t.Name)
                .ToList();

            foreach (var t in types)
            {
                sb.AppendLine("");
                sb.AppendLine($"{t.Name}");
                foreach (var i in t.Implements)
                {
                    OutputInterfaces(i, sb, "  ");
                }
            }
        }

        public static SymbolWriterCSharp ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            var writer = new SymbolWriterCSharp(compilation, outputFolder);
            writer.WriteAll("float");
            //writer.WriteAll("Plato.DoublePrecision.g.cs", "double");

            // Output documentation 
            /*
            var docWriter = new DocWriter(compilation);
            var fp = outputFolder.RelativeFile("docs.html");
            fp.WriteAllText(docWriter.ToString());
            */

            //Analyze(compilation, outputFolder);

            var sb = new StringBuilder();
            OutputTypeInterface(compilation, sb);
            outputFolder.RelativeFile("interfaces.txt").WriteAllText(sb.ToString());

            return writer;
        }

        public static TotalAnalysis Analyze(this Compiler.Compilation compilation, DirectoryPath outputFolder)
        {
            return new TotalAnalysis(compilation, outputFolder);
        }
    }
}
