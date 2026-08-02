using Ara3D.Geometry.Navigation;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Utils;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

/// <summary>Each STY rule proven on a minimal in-memory file: one firing case, one clean case
/// sitting just inside the limit or inside a comment/string, so the boundary is pinned.</summary>
[TestFixture]
public static class StyleCheckerTests
{
    private static List<LintFinding> Check(string text, string name = "sample.types.plato")
        => StyleChecker.Check(BoundSnapshot.ParseFile(new SourceFile(0, new FilePath(name), text)));

    private static IEnumerable<string> Codes(string text, string name = "sample.types.plato")
        => Check(text, name).Select(f => f.Code);

    [Test]
    public static void NewIsFlaggedInCodeOnly()
    {
        Assert.That(Codes("library Foo {\n    Make(x: Number): Number => x.New;\n}", "foo.library.plato"),
            Has.Member("STY001"));
        Assert.That(Codes("// New things arrive; \"New\" in strings too.\nlibrary Foo { }", "foo.library.plato"),
            Has.No.Member("STY001"));
        Assert.That(Codes("library Foo {\n    NewtonRaphson(x: Number): Number => x;\n}", "foo.library.plato"),
            Has.No.Member("STY001"), "prefix of a longer identifier must not fire");
    }

    [Test]
    public static void ImplicitIsFlaggedInCodeOnly()
    {
        Assert.That(Codes("type Foo {\n    implicit: Number;\n}"), Has.Member("STY003"));
        Assert.That(Codes("// implicit surfaces are discussed in prose all the time\ntype Foo { A: Number; }"),
            Has.No.Member("STY003"));
    }

    [Test]
    public static void FieldCountCapsAtTen()
    {
        static string TypeWithFields(int n)
            => "type Wide {\n" + string.Join("\n", Enumerable.Range(1, n).Select(i => $"    F{i}: Number;")) + "\n}";

        Assert.That(Codes(TypeWithFields(10)), Has.No.Member("STY002"));
        Assert.That(Codes(TypeWithFields(11)), Has.Member("STY002"));
    }

    [Test]
    public static void OneKindPerFile()
    {
        Assert.That(Codes("interface IFoo\n{\n    Size(x: Self): Number;\n}\ntype Foo { A: Number; }"),
            Has.Member("STY005"), "mixed kinds");
        Assert.That(Codes("type Foo { A: Number; }", "foo.library.plato"),
            Has.Member("STY005"), "suffix disagrees with contents");
        Assert.That(Codes("type Foo { A: Number; }", "foo.types.plato"), Has.No.Member("STY005"));
        Assert.That(Codes("type Foo { A: Number; }", "foo.plato"),
            Has.No.Member("STY005"), "no suffix, homogeneous - nothing to contradict");
    }

    /// <summary>Not a gate: prints the current stdlib's style-finding profile so a threshold change
    /// can be judged against reality before it ships.</summary>
    [Test, Explicit("Survey of the live stdlib, run by name when calibrating rules.")]
    public static void SurveyForwardStdLib()
    {
        var stdlib = Corpus.RepoRoot.RelativeFolder("stdlib");
        if (!stdlib.Exists())
            Assert.Ignore("No stdlib folder above the test binary.");

        var cache = new ParseCache();
        var parsed = CheckSupport.Parse(SourceSnapshot.FromDirectory(stdlib), cache);
        var findings = StyleChecker.Check(parsed);

        TestContext.WriteLine($"files: {parsed.Count}, findings: {findings.Count}");
        foreach (var g in findings.GroupBy(f => f.Code).OrderBy(g => g.Key))
            TestContext.WriteLine($"  {g.Key}: {g.Count()}");
        foreach (var f in findings.Take(60))
            TestContext.WriteLine($"  {f}");
    }
}
