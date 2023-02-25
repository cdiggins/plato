using System.Collections.Generic;

namespace PlatoParser
{
    public class ParseTree
    {
        public ParseNode Node { get; }
        public string Type => Node.Type;
        public IReadOnlyList<ParseTree> Children { get; }
        public ParseTree(ParseNode node, IReadOnlyList<ParseTree> children)
            => (Node, Children) = (node, children);
    }
}