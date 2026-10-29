// Checks the C# that --lean-inlining (TKT-1091) emits against the default.
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// TKT-1091: by default every generated member carries <c>[MethodImpl(AggressiveInlining)]</c>;
/// under <see cref="CSharpWriter.LeanInlining"/> none does, and nothing else changes. The
/// handwritten intrinsics are copied, not written, so they keep their own attributes.
/// </summary>
[TestFixture]
public static class LeanInliningTests
{
    private const string Attribute = "[MethodImpl(AggressiveInlining)]";

    private static readonly Lazy<Ara3D.Geometry.Compiler.Compilation> Compilation = new(() =>
    {
        var foundation = Path.Combine(CheckerTestSupport.FindForwardStdLib(), "foundation");
        return new Ara3D.Geometry.Compiler.Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Select(CheckerTestSupport.ParseFile).ToList());
    });

    private static readonly Dictionary<bool, IReadOnlyDictionary<string, string>> Outputs = new();

    private static IReadOnlyDictionary<string, string> Emit(bool lean)
    {
        if (Outputs.TryGetValue(lean, out var files))
            return files;
        var writer = new CSharpWriter(Compilation.Value, "unused-lean-inlining")
            { ExtensionStyle = true, LowerLoops = true, FuseCombinators = true, LeanInlining = lean };
        writer.WriteAll();
        files = writer.Files.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        Outputs[lean] = files;
        return files;
    }

    private static int Count(string text, string word)
        => Regex.Matches(text, Regex.Escape(word)).Count;

    private static string Normalized(string text)
        => Regex.Replace(text.Replace(Attribute, ""), "[ \t]+", " ");

    [Test]
    public static void DefaultMarksGeneratedMembers()
    {
        var files = Emit(false);
        Assert.That(Count(files["_Number.g.cs"], Attribute), Is.GreaterThan(0));
        Assert.That(Count(files["Extensions.g.cs"], Attribute), Is.GreaterThan(0));
        Assert.That(files["Extensions.g.cs"], Does.Contain(Attribute + " public static "));
    }

    [Test]
    public static void LeanMarksNoGeneratedMember()
    {
        var marked = Emit(true).Where(kv => kv.Value.Contains("AggressiveInlining")).Select(kv => kv.Key).ToList();
        Assert.That(marked, Is.Empty);
    }

    [Test]
    public static void LeanDiffersOnlyByTheAttribute()
    {
        var off = Emit(false);
        var on = Emit(true);
        Assert.That(on.Keys.OrderBy(k => k, StringComparer.Ordinal), Is.EqualTo(off.Keys.OrderBy(k => k, StringComparer.Ordinal)));
        foreach (var name in off.Keys)
            Assert.That(Normalized(on[name]), Is.EqualTo(Normalized(off[name])), name);
    }

    [Test]
    public static void OptionRecordCarriesTheSwitch()
    {
        Assert.That(new LoopOptimizationOptions().LeanInlining, Is.False);
        Assert.That(new LoopOptimizationOptions(LeanInlining: true).LeanInlining, Is.True);
    }
}
