using System.Text;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>Golden snapshots of the Cpu dialect's emitted C++ (every file, prelude included) for four option sets.
/// A writer refactor that must change no output passes without a full-library generation; a switch shows its
/// effect as a diff of the Golden/ folders. A mismatch writes the actual text to Golden/_actual/&lt;set&gt;/ and fails;
/// PLATO_GOLDEN_UPDATE=1 rewrites the snapshots.</summary>
[TestFixture]
public class GoldenEmissionTests
{
    private static readonly string[] Roots =
    {
        "Area", "TotalArea", "WideSum", "HugeSum", "CopyHuge", "NameLength", "SumX", "FirstName", "Greeting",
        "Lengths", "Scaled", "SumSquares", "PairSum", "Root",
    };

    private static CppCpuOptions Shipping => new(true, true, true, true, Roots);

    public static IEnumerable<TestCaseData> OptionSets()
    {
        yield return new TestCaseData("default", new CppCpuOptions(false, false, false, false, Roots)).SetName("Golden_default");
        yield return new TestCaseData("shipping", Shipping).SetName("Golden_shipping");
        yield return new TestCaseData("shipping-share-calls", Shipping with { ShareCalls = true }).SetName("Golden_shipping_share_calls");
        yield return new TestCaseData("shipping-sum-switch", Shipping with { SwitchSumDispatch = true }).SetName("Golden_shipping_sum_switch");
        yield return new TestCaseData("shipping-compact-sums", Shipping with { CompactSums = true, SwitchSumDispatch = true }).SetName("Golden_shipping_compact_sums");
        yield return new TestCaseData("shipping-w2", Shipping with
        {
            BorrowLargeParameters = true, BorrowReads = true, BorrowNonTrivial = true, BorrowMinBytes = 32,
        }).SetName("Golden_shipping_w2");
    }

    private static string SourceFolder([System.Runtime.CompilerServices.CallerFilePath] string path = "")
        => Path.GetDirectoryName(path)!;

    // Compiling the library dominates the run, so the cases share one Compilation (emission leaves it unchanged;
    // the snapshots would differ between cases otherwise).
    private static readonly Lazy<Compilation> Shared = new(Compile);

    // The types and concepts of foundation (every library names them) plus the libraries the fixture reaches; the rest
    // of the standard library adds emitted text, not writer decisions.
    private static readonly string[] SliceLibraries =
    {
        "primitives", "intrinsics", "core", "numbers", "collections", "text", "functional-procedural", "algebra",
        "constants", "angles", "bytes", "sorting",
    };

    private static bool IsInSlice(string path)
    {
        var name = Path.GetFileName(path);
        return name == "primitives.plato" || name.EndsWith(".types.plato") || name.EndsWith(".concepts.plato")
            || SliceLibraries.Any(l => name == l + ".library.plato");
    }

    private static Compilation Compile()
    {
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var fixture = Path.Combine(SourceFolder(), "GoldenFixture.plato");
        var asts = Directory.GetFiles(folder, "*.plato").Where(IsInSlice).Append(fixture).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        return compilation;
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n");

    [TestCaseSource(nameof(OptionSets))]
    public void Emission_matches_snapshot(string set, CppCpuOptions options)
    {
        var scratch = Path.Combine(Path.GetTempPath(), $"plato-golden-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scratch);
        var writer = Shared.Value.ToCpp(new DirectoryPath(scratch), CppDialect.Cpu, true, options);
        var actual = writer.Files.ToDictionary(f => f.Key, f => Normalize(f.Value.ToString()));
        var golden = Path.Combine(SourceFolder(), "Golden", set);
        var update = Environment.GetEnvironmentVariable("PLATO_GOLDEN_UPDATE") == "1";
        if (update)
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
        Assert.Fail($"Emission for '{set}' differs from {golden} in {differing.Count} file(s): {string.Join(", ", differing)}. " +
                    $"Actual output is in {actualFolder}; set PLATO_GOLDEN_UPDATE=1 to accept.");
    }
}
