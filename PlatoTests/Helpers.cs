using PlatoParser;
using System.Diagnostics;

namespace PlatoTests
{
    public static class Helpers
    {
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

        public static ParseNode CreateParseRoot(this ParseNode node)
        {
            return new ParseNode(node.Input, "_root_", 0, node.Input.Length, node);
        }

        public static (ParseTree, ParseNode) ToParseTree(this ParseNode node)
        {
            if (node == null) return (null, null);
            var prev = node.Previous;
            var children = new List<ParseTree>();
            while (prev != null && IsAParent(node, prev))
            {
                (ParseTree child, prev) = ToParseTree(prev);
                children.Add(child);
            }
            return (new ParseTree(node, children), prev);
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

        public static void OutputState(this ParserState state)
        {
            if (state == null)
                Console.WriteLine("Failed!");
            else
            {
                Console.WriteLine($"State position {state.Position} of {state.Input.Length}");
                OutputNodes(state.Node);
            }
        }

        public static string Descriptor(this ParseNode node)
            => $"Node {node.Type}: {node.Start} to {node.End} = {node.EllidedContents}";        

        public static void OutputTree(this ParseTree tree, string indent = "+-+")
        {
            Console.WriteLine($"{indent} {tree.Node.Descriptor()}");
            foreach (var child in tree.Children)
            {
                OutputTree(child, "| " + indent);
            }
        }

        public static void OutputNodes(this ParseNode last)
        {
            var list = last.AllNodes();

            /*
            foreach (var node in list)
            {
                Console.WriteLine(node.Descriptor());
            }
            */

            if (last == null)
            {
                Console.WriteLine("No nodes found");
            }
            else
            {

                var top = last.CreateParseRoot();
                var (tree, _) = top.ToParseTree();
                OutputTree(tree);
            }
        }
    }
}