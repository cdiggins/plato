using System.Runtime.CompilerServices;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>Which laws a run selects: comma lists, list files, class names, and filters that
/// select nothing. The laws are written to a temporary file and compiled with stdlib/foundation.</summary>
[TestFixture]
public class LawRunnerSelectionTests
{
    public const string Source = """
        library LawFixtureShapes
        {
            Law_AlwaysHolds(b: Boolean): Boolean => true;
            Law_HoldsWhenTrue(b: Boolean): Boolean => b;
            Law_AlwaysHoldsToo(b: Boolean): Boolean => true;
        }

        library LawFixtureColors
        {
            Law_ColorHolds(b: Boolean): Boolean => true;
        }
        """;

    private static readonly Lazy<Compilation> _compilation = new(() => CompileSource(Source));

    /// <summary>stdlib/foundation plus one source text, compiled as one program.</summary>
    public static Compilation CompileSource(string source)
    {
        var path = Path.Combine(Path.GetTempPath(), $"law-fixture-{Guid.NewGuid():N}.plato");
        File.WriteAllText(path, source);
        try { return TestSupport.Compile(new[] { FoundationFolder() }, path); }
        finally { File.Delete(path); }
    }

    /// <summary>stdlib/foundation found from this source file, not from the test output folder,
    /// so the tests also run from a build under --artifacts-path outside the repository.</summary>
    private static string FoundationFolder([CallerFilePath] string source = "")
    {
        for (var dir = new DirectoryInfo(Path.GetDirectoryName(source)!); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "stdlib", "foundation")))
                return Path.Combine(dir.FullName, "stdlib", "foundation");
        throw new DirectoryNotFoundException("no stdlib/foundation above " + source);
    }

    private static LawRunner Runner(IReadOnlyList<string> filters)
        => new(new Ara3D.Geometry.Interpreter.Interpreter(_compilation.Value, NumberProfile.Cpu64), new LawRunOptions { Filters = filters });

    private static string[] Selected(IReadOnlyList<string> filters)
        => Runner(filters).Laws.Select(LawSelection.FullName).Where(n => n.StartsWith("LawFixture")).OrderBy(n => n).ToArray();

    [Test]
    public void No_filter_selects_every_law()
        => Assert.That(Selected(null), Has.Length.EqualTo(4));

    [Test]
    public void Empty_filter_list_selects_none()
        => Assert.That(Runner(Array.Empty<string>()).Laws, Is.Empty);

    [Test]
    public void Comma_list_selects_the_union()
        => Assert.That(Selected(LawSelection.SplitList(" Law_HoldsWhenTrue , law_colorholds,,")),
            Is.EqualTo(new[] { "LawFixtureColors.Law_ColorHolds", "LawFixtureShapes.Law_HoldsWhenTrue" }));

    [Test]
    public void Class_name_selects_its_laws()
        => Assert.That(Selected(new[] { "LawFixtureColors" }), Is.EqualTo(new[] { "LawFixtureColors.Law_ColorHolds" }));

    [Test]
    public void Qualified_name_selects_one_law()
        => Assert.That(Selected(new[] { "LawFixtureShapes.Law_AlwaysHoldsToo" }), Is.EqualTo(new[] { "LawFixtureShapes.Law_AlwaysHoldsToo" }));

    [Test]
    public void A_name_selects_every_law_containing_it()
        => Assert.That(Selected(new[] { "Law_AlwaysHolds" }),
            Is.EqualTo(new[] { "LawFixtureShapes.Law_AlwaysHolds", "LawFixtureShapes.Law_AlwaysHoldsToo" }));

    [Test]
    public void List_file_skips_blank_lines_and_comments()
        => Assert.That(LawSelection.ParseListFile("# laws for step B\r\nLaw_ColorHolds\r\n\r\n  LawFixtureShapes.Law_HoldsWhenTrue  \n#Law_AlwaysHolds\n"),
            Is.EqualTo(new[] { "Law_ColorHolds", "LawFixtureShapes.Law_HoldsWhenTrue" }));

    [Test]
    public void No_list_and_no_file_means_no_filter()
        => Assert.That(LawSelection.FromArguments(Array.Empty<string>(), Array.Empty<string>()), Is.Null);

    [Test]
    public void Lists_and_files_combine()
    {
        var file = Path.GetTempFileName();
        try
        {
            File.WriteAllText(file, "# one per line\nLawFixtureColors\n");
            Assert.That(LawSelection.FromArguments(new[] { "Law_A,Law_B", "Law_C" }, new[] { file }),
                Is.EqualTo(new[] { "Law_A", "Law_B", "Law_C", "LawFixtureColors" }));
            File.WriteAllText(file, "\n# nothing\n");
            Assert.That(LawSelection.FromArguments(Array.Empty<string>(), new[] { file }), Is.Empty);
        }
        finally { File.Delete(file); }
    }

    [Test]
    public void A_missing_list_file_is_an_error()
        => Assert.Throws<FileNotFoundException>(() => LawSelection.FromArguments(Array.Empty<string>(), new[] { "no-such-law-list.txt" }));

    [Test]
    public void Unmatched_filters_are_named()
        => Assert.That(Runner(new[] { "Law_ColorHolds", "Law_Misspelt" }).UnmatchedFilters, Is.EqualTo(new[] { "Law_Misspelt" }));
}
