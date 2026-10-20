using System.Runtime.CompilerServices;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;
using Ara3D.Geometry.Laws;
using Ara3D.Geometry.Navigation;
using NUnit.Framework;

namespace Plato.Navigation.Tests;

/// <summary>WarmLawRunner: one kept program per folder set (TKT-344), and a file that does not
/// parse reported as a parse error, never as an empty run (TKT-337). Each program is
/// stdlib/foundation plus a temporary folder holding one law file.</summary>
[TestFixture]
public class WarmLawRunnerTests
{
    private const string LawsA = """
        library WarmFixtureA
        {
            Law_AHolds(b: Boolean): Boolean => true;
        }
        """;

    private const string LawsB = """
        library WarmFixtureB
        {
            Law_BHolds(b: Boolean): Boolean => true;
        }
        """;

    private readonly List<string> _folders = new();

    [TearDown]
    public void RemoveFolders()
    {
        foreach (var f in _folders)
            Directory.Delete(f, recursive: true);
        _folders.Clear();
    }

    /// <summary>stdlib/foundation, found from this source file so the tests also run from a
    /// build under --artifacts-path outside the repository.</summary>
    private static string Foundation([CallerFilePath] string source = "")
    {
        for (var dir = new DirectoryInfo(Path.GetDirectoryName(source)!); dir != null; dir = dir.Parent)
            if (Directory.Exists(Path.Combine(dir.FullName, "stdlib", "foundation")))
                return Path.Combine(dir.FullName, "stdlib", "foundation");
        throw new DirectoryNotFoundException("no stdlib/foundation above " + source);
    }

    /// <summary>A new folder holding one file, laws.plato, with this text.</summary>
    private string Folder(string text)
    {
        var folder = Path.Combine(Path.GetTempPath(), $"warm-laws-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "laws.plato"), text);
        _folders.Add(folder);
        return folder;
    }

    private static LawRunRequest Request(string folder, NumberProfile? profile = null, LawBackendKind backend = LawBackendKind.Interpreter)
        => new() { Folders = new[] { Foundation(), folder }, Profile = profile ?? NumberProfile.Cpu64, Backend = backend };

    private static void AssertRan(LawRunOutcome outcome, string law)
    {
        Assert.That(outcome.Errors, Is.Empty);
        Assert.That(outcome.Results.Select(r => r.Name), Has.Member(law));
        Assert.That(outcome.Results.Single(r => r.Name == law).Status, Is.EqualTo(LawStatus.Pass));
    }

    [Test]
    public void A_file_that_does_not_parse_is_reported_with_its_file_and_no_law_runs()
    {
        var folder = Folder(LawsA.Replace("=> true;", "=> true"));
        var outcome = new WarmLawRunner().Run(Request(folder));

        Assert.That(outcome.Compiled, Is.False);
        Assert.That(outcome.Results, Is.Empty);
        Assert.That(outcome.Errors, Is.Not.Empty);
        Assert.That(outcome.Errors, Has.All.StartsWith("Parsing failed: "));
        Assert.That(outcome.Errors, Has.All.Contains("laws.plato"));
        TestContext.Out.WriteLine(string.Join("\n", outcome.Errors));
    }

    [Test]
    public void A_fixed_parse_error_compiles_on_the_next_run()
    {
        var folder = Folder(LawsA.Replace("=> true;", "=> true"));
        var runner = new WarmLawRunner();
        Assert.That(runner.Run(Request(folder)).Compiled, Is.False);

        File.WriteAllText(Path.Combine(folder, "laws.plato"), LawsA);
        AssertRan(runner.Run(Request(folder)), "WarmFixtureA.Law_AHolds");
    }

    [Test]
    public void Two_folder_sets_alternating_stay_warm()
    {
        var a = Folder(LawsA);
        var b = Folder(LawsB);
        var runner = new WarmLawRunner();

        var firstA = runner.Run(Request(a));
        var firstB = runner.Run(Request(b));
        var againA = runner.Run(Request(a));
        var againB = runner.Run(Request(b));

        Assert.That(new[] { firstA.Recompiled, firstB.Recompiled, againA.Recompiled, againB.Recompiled },
            Is.EqualTo(new[] { true, true, false, false }));
        AssertRan(againA, "WarmFixtureA.Law_AHolds");
        AssertRan(againB, "WarmFixtureB.Law_BHolds");
        Assert.That(againB.ProgramsKept, Is.EqualTo(2));
        Assert.That(firstB.FilesParsed, Is.EqualTo(1), "the foundation files are parsed once for both programs");
    }

    [Test]
    public void The_least_recently_used_program_is_dropped_beyond_the_bound()
    {
        var a = Folder(LawsA);
        var b = Folder(LawsB);
        var runner = new WarmLawRunner(maxPrograms: 1);

        runner.Run(Request(a));
        runner.Run(Request(b));
        var againA = runner.Run(Request(a));

        Assert.That(againA.Recompiled, Is.True);
        Assert.That(againA.ProgramsKept, Is.EqualTo(1));
    }

    [Test]
    public void Profiles_share_one_compilation()
    {
        var a = Folder(LawsA);
        var runner = new WarmLawRunner();

        runner.Run(Request(a, NumberProfile.Cpu64));
        var gpu = runner.Run(Request(a, NumberProfile.Gpu32));

        Assert.That(gpu.Recompiled, Is.False);
        AssertRan(gpu, "WarmFixtureA.Law_AHolds");
        Assert.That(gpu.ProgramsKept, Is.EqualTo(1));
    }

    [Test]
    public void An_edit_recompiles_only_its_own_program()
    {
        var a = Folder(LawsA);
        var b = Folder(LawsB);
        var runner = new WarmLawRunner();
        runner.Run(Request(a));
        runner.Run(Request(b));

        File.WriteAllText(Path.Combine(a, "laws.plato"), LawsA.Replace("=> true;", "=> b || true;"));

        Assert.That(runner.Run(Request(b)).Recompiled, Is.False);
        var editedA = runner.Run(Request(a));
        Assert.That(editedA.Recompiled, Is.True);
        Assert.That(editedA.FilesParsed, Is.EqualTo(1));
        AssertRan(editedA, "WarmFixtureA.Law_AHolds");
    }

    [Test]
    public void The_csharp_backend_compiles_once_per_source_generation()
    {
        var a = Folder(LawsA);
        var runner = new WarmLawRunner();

        var first = runner.Run(Request(a, backend: LawBackendKind.CSharp));
        var again = runner.Run(Request(a, backend: LawBackendKind.CSharp));
        File.WriteAllText(Path.Combine(a, "laws.plato"), LawsA.Replace("=> true;", "=> b || true;"));
        var edited = runner.Run(Request(a, backend: LawBackendKind.CSharp));

        AssertRan(first, "WarmFixtureA.Law_AHolds");
        Assert.That(first.Backend, Is.EqualTo(LawBackends.CSharpName));
        Assert.That(first.BackendReused, Is.False);
        Assert.That(first.CSharpCompileTime, Is.GreaterThan(TimeSpan.Zero));
        Assert.That(again.BackendReused, Is.True);
        Assert.That(again.CSharpCompileTime, Is.EqualTo(TimeSpan.Zero));
        AssertRan(again, "WarmFixtureA.Law_AHolds");
        Assert.That(edited.Recompiled, Is.True);
        Assert.That(edited.BackendReused, Is.False);
        AssertRan(edited, "WarmFixtureA.Law_AHolds");
    }
}
