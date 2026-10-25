using Ara3D.Geometry.Navigation;
using Ara3D.Logging;
using Ara3D.Parakeet;
using Ara3D.Parsing;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

[TestFixture]
public sealed class MissingSemicolonLocationTests
{
    [TestCase(" }", 3)]
    [TestCase("\n}", 4)]
    public void MissingSemicolonNamesTheStatementOrGiveUpLine(string closing, int line)
    {
        var text = "library Located\n{\n    Law_True(): Boolean => true" + closing;
        var file = new SourceFile(0, new FilePath("missing-semicolon.plato"), text);
        var parsed = BoundSnapshot.ParseFile(file);
        var errors = WarmProgram.ParseErrors(new[] { parsed });
        Assert.That(parsed.Parsed, Is.False);
        Assert.That(errors.Any(e => e.Contains($"Parse error at line {line}, column ")), Is.True);
        Assert.That(errors.All(e => e.Contains("missing-semicolon.plato")), Is.True);
    }

    [Test]
    public void RestoringTheSemicolonLeavesNoParseErrors()
    {
        var file = new SourceFile(0, new FilePath("valid.plato"),
            "library Located\n{\n    Law_True(): Boolean => true;\n}");
        var parsed = BoundSnapshot.ParseFile(file);
        Assert.That(parsed.Parsed, Is.True);
        Assert.That(WarmProgram.ParseErrors(new[] { parsed }), Is.Empty);
    }

}
