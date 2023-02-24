using PlatoParser;

namespace PlatoTests
{
    public class ParseTree
    {
        public ParseNode Node { get; }
        public IReadOnlyList<ParseTree> Children { get; }
        public ParseTree(ParseNode node, IEnumerable<ParseTree> children)
            => (Node, Children) = (node, children.ToList());         
    }
}