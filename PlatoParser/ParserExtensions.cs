using System.Collections.Generic;
using System.Diagnostics;

namespace PlatoParser
{
    public static class ParserExtensions
    {
        public static ParserState Parse(this string s, Rule r, ParseCache results)
            => r.Match(new ParserState(s, 0, null), results);

        public static ParserState Parse(this string s, Rule r)
            => r.Match(new ParserState(s, 0, null), new ParseCache(s.Length));

        public static ParseNode CreateParseRoot(this ParseNode node)
            => new ParseNode(node.Input, null, 0, node.Input.Length, node);

        public static (ParseTree, ParseNode) ToParseTree(this ParseNode node)
        {
            if (node == null) return (null, null);
            var prev = node.Previous;
            var children = new List<ParseTree>();
            while (prev != null && IsAParent(node, prev))
            {
                ParseTree child;
                (child, prev) = ToParseTree(prev);
                children.Add(child);
            }
            return (new ParseTree(node, children.Reverse().ToList()), prev);
        }

        public static List<ParseNode> AllNodes(this ParseNode node)
        {
            var r = new List<ParseNode>();
            while (node != null)
            {
                r.Add(node);
                node = node.Previous;
            }
            r.Reverse();
            return r;
        }

        public static bool IsAParent(this ParseNode node, ParseNode other)
        {
            if (node == null || other == null) return false;
            if (other.Start >= node.End) return false;
            if (other.End <= node.Start) return false;

            // In this case it was a child
            if (other.Start >= node.Start)
            {
                Debug.Assert(node.End >= node.End);
                return true;
            }

            // Otherwise it was a sibling
            Debug.Assert(other.End <= node.Start);
            return false;
        }
    }
}