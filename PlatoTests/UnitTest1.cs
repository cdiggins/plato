using PlatoParser;
using PlatoTypeInference;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static PlatoAbstractSyntax.Parser;

namespace PlatoTests
{    
    public class ParseTree
    {
        public ParseNode Node { get; }
        public IReadOnlyList<ParseTree> Children { get; }
        public ParseTree(ParseNode node, IEnumerable<ParseTree> children)
            => (Node, Children) = (node, children.ToList());         
    }


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
            }*/

            var top = last.CreateParseRoot();
            var (tree, _) = top.ToParseTree();
            OutputTree(tree);
        }
    }

    public static class ParserTests
    {
        public static string TestDigits = "0123456789";
        public static string TestNumbersThenUpperCaseLetters = "0123ABC";
        public static string HelloWorld = "Hello world!";
        public static string MathEquation = "(1.23 + (4.56 / 7.9) - 0.8)";
        public static string SomeCode = "var x = 123; x += 23; f(1, a);";

        public static CommonRules Rules = new CommonRules();

        [Test]
        public static void TestExpression()
        {
            {
                var ps = MathEquation.Parse(Rules.Expression);
                ps.OutputState();
            }
        }

        [Test]
        public static void TestToken()
        {
            /*
            {
                var ps = SomeCode.Parse(Rules.Token.ZeroOrMore());
                ps.OutputState();
            }
            {
                var ps = MathEquation.Parse(Rules.Token.ZeroOrMore());
                ps.OutputState();
            }
            */
            {
                var ps = SomeCode.Parse(Rules.Element.ZeroOrMore());
                ps.OutputState();
            }
            {
                var ps = MathEquation.Parse(Rules.Element.ZeroOrMore());
                ps.OutputState();
            }
        }


        [Test]
        public static void TestParser()
        {
            {
                var ps = TestDigits.Parse(Rules.Digits);
                Assert.NotNull(ps);
                Assert.IsTrue(ps.AtEnd);
            }
            {
                var ps = TestDigits.Parse(Rules.Digits.Node("Digits"));
                Console.WriteLine(ps.Node);
            }
            {

            }
        }
    }

    public static class InferenceTests
    {
        public static TypeVar Var(string name, int id) => new (name, id);
        public static TypeList List(params BaseType[] types) => new(types);
        public static TypeConstant Const(string name) => new(name);

        [Test]
        public static void TestInference()
        {
            var t0 = Const("int");
            var t1 = Const("func");
            
            var var0 = Var("a", 0);
            var var1 = Var("b", 1);
            var var2 = Var("c", 2);
            var var3 = Var("d", 3);

            var f1 = List(t1, var0, var1);
            var f2 = List(t1, var1, var2);

            {
                var unifier = new TypeUnifier();
                var r = unifier.Unify(f1, f2);
                Console.WriteLine($"Unification of {f1} and {f2} produced {r}");
            }
        }
    }
}