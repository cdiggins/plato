using System.Linq;
using Parakeet;

namespace PlatoAst
{
    public class AstXmlBuilder : CodeBuilder<AstXmlBuilder>
    {
        public AstXmlBuilder Write(AstNode node)
        {
            if (node is AstIdentifier ident)
                return Write(ident.Text);

            var type = node.GetType().Name;
            var r = WriteLine($"<{type}>").Indent();
            r = node.Children.Aggregate(r, (current, c) => current.Write(c));
            return r.Dedent().WriteLine($"</{type}>");
        }
    }
}