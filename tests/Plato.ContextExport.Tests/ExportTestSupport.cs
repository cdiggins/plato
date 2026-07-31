using Ara3D.Geometry.AST;
using Ara3D.Geometry.ContextExport;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Plato.ContextExport.Tests;

public static class ExportTestSupport
{
    public static string ExportFolder(string folder, PlatoFormatOptions format)
    {
        var files = new DirectoryPath(folder).GetFiles("*.plato").ToList();
        var lines = new List<string>();
        foreach (var file in files.OrderBy(f => f.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var ast = ParseFile(file);
            foreach (var declaration in ast.Types.Where(d =>
                         d.Kind is TypeKind.ConcreteType or TypeKind.Interface))
            {
                lines.Add(PlatoDeclarationWriter.WriteFormatted(declaration, format));
            }
        }

        var separator = format.Pretty ? "\n\n" : "\n";
        return string.Join(separator, lines);
    }

    public static AstFile ParseFile(FilePath file)
    {
        var text = File.ReadAllText(file);
        var parser = CommonParsers.PlatoParser(new ParserInput(text, file));
        Assert.That(parser.Succeeded, Is.True, string.Join(Environment.NewLine, parser.ErrorMessages));
        return (AstFile)parser.Cst!.ToAst();
    }

    public static string InputFolder()
        => Path.Combine(TestContext.CurrentContext.TestDirectory, "input");
}
