using System.Text;
using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Optimizer;
using Ara3D.Utils;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>Golden snapshots for lane C1-a (TKT-1182): the shared passes the C++ pipeline gained, one set per switch and one
/// for all of them, over <c>C1aFixture.plato</c>. <c>shipping-c1a-base</c> is the same program with none of the five on, so
/// the diff of a set against it is the pass's effect. A mismatch writes the actual text to Golden/_actual/&lt;set&gt;/;
/// PLATO_GOLDEN_UPDATE=1 rewrites the snapshots.</summary>
[TestFixture]
public class C1aGoldenTests
{
    private static readonly string[] Roots = { "ShapeArea", "Same", "AreaAt", "Pick", "Twice", "Wide", "Scaled" };

    private static CppCpuOptions Shipping(params string[] switches)
        => new(true, true, true, true, Roots) { Switches = new SwitchSet(switches) };

    private static readonly string[] SliceLibraries =
    {
        "primitives", "intrinsics", "core", "numbers", "collections", "text", "functional-procedural", "algebra",
        "constants", "angles", "bytes", "sorting",
    };

    public static IEnumerable<TestCaseData> OptionSets()
    {
        yield return new TestCaseData("shipping-c1a-base", Shipping()).SetName("C1aGolden_base");
        yield return new TestCaseData("shipping-share-calls-wide", Shipping("share-calls", "share-calls-wide") with { ShareCalls = true }).SetName("C1aGolden_share_calls_wide");
        yield return new TestCaseData("shipping-sink-bindings", Shipping("sink-bindings")).SetName("C1aGolden_sink_bindings");
        yield return new TestCaseData("shipping-bind-scrutinee", Shipping("bind-scrutinee")).SetName("C1aGolden_bind_scrutinee");
        yield return new TestCaseData("shipping-c1a-bundle",
            Shipping("share-calls", "share-calls-wide", "sink-bindings", "bind-scrutinee") with { ShareCalls = true }).SetName("C1aGolden_bundle");
    }

    private static string SourceFolder([System.Runtime.CompilerServices.CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;

    private static bool IsInSlice(string path)
    {
        var name = Path.GetFileName(path);
        return name == "primitives.plato" || name.EndsWith(".types.plato") || name.EndsWith(".concepts.plato")
            || SliceLibraries.Any(l => name == l + ".library.plato");
    }

    // Each case elaborates its own copy: bind-scrutinee and sink-bindings change the TIR source, which a shared
    // Compilation would cache.
    private static Compilation Compile()
    {
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var fixture = Path.Combine(SourceFolder(), "C1aFixture.plato");
        var asts = Directory.GetFiles(folder, "*.plato").Where(IsInSlice).Append(fixture).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        return compilation;
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n");

    [TestCaseSource(nameof(OptionSets))]
    public void Emission_matches_snapshot(string set, CppCpuOptions options)
    {
        var scratch = Path.Combine(Path.GetTempPath(), $"plato-golden-c1a-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scratch);
        var writer = Compile().ToCpp(new DirectoryPath(scratch), CppDialect.Cpu, true, options);
        var actual = writer.Files.ToDictionary(f => f.Key, f => Normalize(f.Value.ToString()));
        var golden = Path.Combine(SourceFolder(), "Golden", set);
        if (Environment.GetEnvironmentVariable("PLATO_GOLDEN_UPDATE") == "1")
        {
            if (Directory.Exists(golden)) Directory.Delete(golden, true);
            Directory.CreateDirectory(golden);
            foreach (var (name, text) in actual)
                File.WriteAllText(Path.Combine(golden, name), text, new UTF8Encoding(false));
            return;
        }
        var expected = Directory.Exists(golden)
            ? Directory.GetFiles(golden).ToDictionary(Path.GetFileName, f => Normalize(File.ReadAllText(f)))
            : new Dictionary<string?, string>();
        var differing = new List<string>();
        foreach (var name in actual.Keys.Union(expected.Keys!).OrderBy(n => n, StringComparer.Ordinal))
        {
            var has = expected.TryGetValue(name, out var old);
            if (!actual.TryGetValue(name!, out var now)) differing.Add($"{name} (missing from output)");
            else if (!has) differing.Add($"{name} (new file)");
            else if (old != now) differing.Add(name!);
        }
        if (differing.Count == 0) return;
        var actualFolder = Path.Combine(SourceFolder(), "Golden", "_actual", set);
        if (Directory.Exists(actualFolder)) Directory.Delete(actualFolder, true);
        Directory.CreateDirectory(actualFolder);
        foreach (var (name, text) in actual)
            File.WriteAllText(Path.Combine(actualFolder, name), text, new UTF8Encoding(false));
        Assert.Fail($"Emission for '{set}' differs from {golden} in {differing.Count} file(s): {string.Join(", ", differing)}. Actual output is in {actualFolder}; set PLATO_GOLDEN_UPDATE=1 to accept.");
    }
}
