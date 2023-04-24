using System.Diagnostics;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Tests;
using PlatoAbstractSyntax;

namespace PlatoTests
{

    public class PlatoParserTests
    {
        public static string DllPath => typeof(PlatoParserTests).Assembly.Location;
        public static string ProjectFolder => Path.Combine(Path.GetDirectoryName(DllPath), "..", "..", "..");
        public static string InputFilesFolder => Path.Combine(ProjectFolder, "input");

		public static IEnumerable<string> InputFiles => 
            Directory.GetFiles(InputFilesFolder);

        public static CSharpGrammar Grammar = new CSharpGrammar();

        [TestCaseSource(nameof(InputFiles))]
        public static void TestParser(string inputFile)
        {
            var input = ParserInput.FromFile(inputFile);

			ParserState ps = null;
            try
            {
                ps = input.Parse(Grammar.Script);
            }
            catch (ParserException pe)
            {
                Console.WriteLine($"Parsing exception {pe.Message} occured at {pe.LastValidState}");
                Assert.Fail();
            }

            if (ps == null)
            {
                Assert.Fail("No parser state created");
            }

            ParserTests.OutputParseErrors(ps);

            if (ps.LastError != null)
            {
                Assert.Fail("Accumulated parser errors");
            }

            if (!ps.AtEnd())
            {
                Assert.Fail($"PARTIAL PASSED: {ps.Position}/{ps.Input.Length}");
            }

            if (ps.Node == null)
            {
                Assert.Fail($"No parse node created");
            }

            ParserTests.OutputNodeCounts(ps.Node);

            var treeAndNode = ps.Node.ToParseTreeAndNode();
            var tree = treeAndNode.Item1;

            if (tree == null)
            {
                Assert.Fail($"No parse tree created");
            }

            Console.WriteLine($"Tree {tree}");
            Console.WriteLine($"Tree {tree.Contents}");

            var cst = Parakeet.Demos.CSharp.CstNodeFactory.Create(tree);

            // TODO: print the CST.
        }
    }
}