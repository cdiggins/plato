using Ara3D.Geometry.AST;
using Ara3D.Geometry.ContextExport;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;

namespace Plato.ContextExport.Tests;

[TestFixture]
public class ContextExportTests
{
    [Test]
    public void FiltersLibrariesFromOutput()
    {
        var output = ExportTestSupport.ExportFolder(ExportTestSupport.InputFolder(), PlatoFormatOptions.CompactDefault);
        Assert.That(output, Does.Contain("concept Comparable"));
        Assert.That(output, Does.Contain("type Point2D"));
        Assert.That(output, Does.Not.Contain("library MathLib"));
        Assert.That(output, Does.Not.Contain("Add(x:Integer"));
    }

    [Test]
    public void CompactDefault_IsOneLinePerDeclaration_WithTightDelimiters()
    {
        var output = ExportTestSupport.ExportFolder(ExportTestSupport.InputFolder(), PlatoFormatOptions.CompactDefault);
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.That(lines.Length, Is.EqualTo(3));
        Assert.That(output, Does.Not.Contain(": "));
        Assert.That(output, Does.Not.Contain(", "));
        Assert.That(output, Does.Not.Contain("{ "));
        Assert.That(output, Does.Contain("concept Comparable{Compare(x:Self,y:Self):Integer;}"));
    }

    [Test]
    public void PrettyMode_PreservesReadableSpacing()
    {
        var output = ExportTestSupport.ExportFolder(
            ExportTestSupport.InputFolder(),
            new PlatoFormatOptions(Pretty: true, Compressed: false, TightDelimiters: false));
        Assert.That(output, Does.Contain("concept Comparable"));
        Assert.That(output, Does.Contain("Compare(x: Self, y: Self): Integer;"));
        Assert.That(output, Does.Not.Contain("conceptComparable"));
    }

    [Test]
    public void DiagnosticsCountsMatchSampleFixture()
    {
        var folder = ExportTestSupport.InputFolder();
        var files = new DirectoryPath(folder).GetFiles("*.plato").ToList();
        var declarations = files
            .SelectMany(f => ExportTestSupport.ParseFile(f).Types)
            .Where(d => d.Kind is TypeKind.ConcreteType or TypeKind.Interface)
            .ToList();

        var output = ExportTestSupport.ExportFolder(folder, PlatoFormatOptions.CompactDefault);
        var stats = ExportDiagnostics.FromOutput(
            output,
            files.Count,
            declarations.Count(d => d.Kind == TypeKind.ConcreteType),
            declarations.Count(d => d.Kind == TypeKind.Interface));

        Assert.That(stats.Files, Is.EqualTo(1));
        Assert.That(stats.TypeCount, Is.EqualTo(1));
        Assert.That(stats.InterfaceCount, Is.EqualTo(2));
        Assert.That(stats.CharacterCount, Is.EqualTo(output.Length));
        Assert.That(stats.EstimatedTokens, Is.EqualTo((int)Math.Ceiling(output.Length / 4.0)));
    }

    [Test]
    public void CompactOutputRoundTripsThroughParser()
    {
        var output = ExportTestSupport.ExportFolder(ExportTestSupport.InputFolder(), PlatoFormatOptions.CompactDefault);
        var parser = CommonParsers.PlatoParser(new ParserInput(output, "roundtrip.plato"));
        Assert.That(parser.Succeeded, Is.True, string.Join(Environment.NewLine, parser.ErrorMessages));
        var ast = (AstFile)parser.Cst!.ToAst();
        Assert.That(ast.Types.Count(d => d.Kind == TypeKind.ConcreteType), Is.EqualTo(1));
        Assert.That(ast.Types.Count(d => d.Kind == TypeKind.Interface), Is.EqualTo(2));
    }
}
