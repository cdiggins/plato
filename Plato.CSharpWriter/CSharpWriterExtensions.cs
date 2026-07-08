using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter
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

        // extensionStyle = false: original writer, byte-identical output (production default).
        // extensionStyle = true : C# 14 extension-block output (--csharp-style=extensions, roadmap P2.2).
        // optimize = true: component-op unrolling (--optimize, roadmap P3.1; see ComponentUnroller).
        public static CSharpWriter ToCSharp(this Compiler.Compilation compilation, DirectoryPath outputFolder, bool extensionStyle = false, bool optimize = false)
        {
            var writer = new CSharpWriter(compilation, outputFolder) { ExtensionStyle = extensionStyle, Optimize = optimize };
            writer.WriteAll("float");
            //writer.WriteAll("Plato.DoublePrecision.g.cs", "double");

            // Output documentation 
            var docWriter = new DocWriter(compilation);
            var fp = outputFolder.RelativeFile("docs.html");
            fp.WriteAllText(docWriter.ToString());

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
