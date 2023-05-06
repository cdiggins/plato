using System.Linq;
using Parakeet;

namespace PlatoAst
{
    public class AstXmlBuilder : CodeBuilder<AstXmlBuilder>
    {
        public string Encode(string s)
        {
            return s
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("&", "&amp;");
        }

        public AstXmlBuilder Write(AstNode node)
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
}