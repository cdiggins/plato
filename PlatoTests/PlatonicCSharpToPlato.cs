using Parakeet;
using Parakeet.Demos;

namespace PlatoTests
{
    public static class PlatonicCSharpToPlato
    {
        [Test, Explicit]
        public static void CSharpTypesToPlato()
        {
            var inputFile = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\math.types.plato.cs";
            var outputFile = @"C:\Users\cdigg\git\plato\PlatoStandardLibrary\temp.plato";
            var grammar = new CSharpGrammar();
            var input = ParserInput.FromFile(inputFile);
            /*
            var c = new Compilation(input, 
                grammar.File, 
                CstNodeFactory.Create, 
                cst => new AstFromCSharpCst().ToAst(cst));
            Assert.IsTrue(c.Success);
            var output = c.AstTree.ToPlato();
            File.WriteAllText(outputFile, output);
            */
        }
    }
}
