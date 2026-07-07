using System.Linq;
using Ara3D.Utils;

namespace Ara3D.Geometry.AST
{
    public class AstWriterXml : CodeBuilder<AstWriterXml>
    {
        public string Encode(string s)
        {
            return s
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("&", "&amp;");
        }

        public AstWriterXml Write(AstNode node)
        {
            if (node == null)
                return this;
            var type = node.GetType().Name;

            if (node is AstLeaf leaf)
                return WriteLine($"<{type}>{Encode(leaf.Text)}</{type}>");

            var r = WriteLine($"<{type}>").Indent();
            r = node.Children.Aggregate(r, (current, c) => current.Write(c));
            return r.Dedent().WriteLine($"</{type}>");
        }
    }

    public static class AstWriterExtensions
    {
        public static string ToXml(this AstNode node)
            => new AstWriterXml().Write(node).ToString();
    }
}