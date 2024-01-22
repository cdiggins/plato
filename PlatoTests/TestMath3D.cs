using Ara3D.Utils;
using Ara3D.Parsing.Tests;
using Ara3D.Parsing;
using Ara3D.Parsing.Grammars;

namespace PlatoTests
{
    public class TestMath3D
    {
        public static CSharpGrammar CSharpGrammar = new CSharpGrammar();

        [Test]
        public static void ParseFuncs()
        {
            var file = PathUtil.GetCallerSourceFolder().RelativeFolder("..").RelativeFolder("PlatoStandardLibrary")
                .RelativeFile("math.funcs.plato.cs");
            var input = ParserInput.FromFile(file);
            Assert.AreEqual(1, ParserTests.ParseTest(input, CSharpGrammar.File, false));
        }
    }
}
    