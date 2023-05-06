using Parakeet;
using Parakeet.Demos;
using Parakeet.Tests;
using PlatoAst;
using Ptarmigan.Utils;

namespace PlatoTests
{

    public class PlatoParserTests
    {
        public static string DllPath => typeof(PlatoParserTests).Assembly.Location;
        public static string ProjectFolder => Path.Combine(Path.GetDirectoryName(DllPath), "..", "..", "..");
        public static string InputFilesFolder => Path.Combine(ProjectFolder, "input");
        public static string OutputFilesFolder => Path.Combine(ProjectFolder, "output");

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
                Console.WriteLine($"Parsing exception {pe.Message} occurred at {pe.LastValidState}");
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
            var cstXml = new CstXmlBuilder().Write(cst).ToString();

            var cstXmlFile = FileUtil.ChangeDirectoryAndExt(inputFile, OutputFilesFolder, ".cst.xml");
            File.WriteAllText(cstXmlFile, cstXml);

            var ast = new AstFromCst().ToAst(cst);
            var astXml = new AstXmlBuilder().Write(ast).ToString();

            var astXmlFile = FileUtil.ChangeDirectoryAndExt(inputFile, OutputFilesFolder, ".ast.xml");
            File.WriteAllText(astXmlFile, astXml);

            var astWriter = new AstWriter(AstWriter.Language.CSharp);
            astWriter.Write(ast);
            var astFile = FileUtil.ChangeDirectoryAndExt(inputFile, OutputFilesFolder, ".ast.cs.txt");
            File.WriteAllText(astFile, astWriter.ToString());
        }
    }
}