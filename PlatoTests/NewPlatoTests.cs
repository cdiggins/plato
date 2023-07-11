using Parakeet.Tests;
using Parakeet;
using Parakeet.Demos;
using Parakeet.Demos.Plato;
using PlatoAst;

namespace PlatoTests;

public class NewPlatoTests
{
    [Test]
    [TestCase("C:\\Users\\cdigg\\git\\plato\\PlatoStandardLibrary\\modules.plato")]
    [TestCase("C:\\Users\\cdigg\\git\\plato\\PlatoStandardLibrary\\types.plato")]
    [TestCase("C:\\Users\\cdigg\\git\\plato\\PlatoStandardLibrary\\concepts.plato")]
    public static void TestFile(string file)
    {
        var pi = ParserInput.FromFile(file);
        var rule = PlatoGrammar.Instance.File;
        var c = new Compilation(pi, rule, pt =>
            CstNodeFactory.Create(pt), AstFromPlatoCst.Convert);
        Console.WriteLine($"Success = {c.Success}");
        Console.WriteLine($"Message = {c.Message}");
        Console.WriteLine($"Tree = {c.AstTree.ToXml()}");
    }

}