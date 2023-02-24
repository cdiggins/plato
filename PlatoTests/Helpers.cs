using PlatoParser;
using System.Diagnostics;

namespace PlatoTests
{
    public static class Helpers
    {
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

    }
}