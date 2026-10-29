// Checks the C# that --lean-inlining (TKT-1091) emits against the default.
using System.Text.RegularExpressions;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CSharpWriter;
using NUnit.Framework;

namespace PlatoTests.Optimization;

/// <summary>
/// TKT-1091: by default every generated member carries <c>[MethodImpl(AggressiveInlining)]</c>;
/// under <see cref="CSharpWriter.LeanInlining"/> only one-line forwarding members keep it, and
/// nothing else changes. The handwritten intrinsics are copied, not written, so they keep theirs.
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

    private static IEnumerable<string> MarkedLines(IReadOnlyDictionary<string, string> files)
        => files.Values.SelectMany(text => text.Split('\n')).Where(line => line.Contains(Attribute));

    [Test]
    public static void LeanKeepsTheAttributeOnlyOnForwardingMembers()
    {
        var kept = MarkedLines(Emit(true)).ToList();
        Assert.That(kept, Is.Not.Empty);
        Assert.That(kept.Count, Is.LessThan(MarkedLines(Emit(false)).Count()));
        foreach (var line in kept)
        {
            var rest = line.Substring(line.IndexOf(Attribute, StringComparison.Ordinal));
            Assert.That(rest.Contains("=>") && LeanInliningPass.IsForwarding(rest.Substring(rest.IndexOf("=>", StringComparison.Ordinal) + 2)),
                Is.True, line);
        }
        Assert.That(Emit(true)["_Number.g.cs"], Does.Contain(Attribute + " public Number Multiply(Number b) => this * b;"));
    }

    [TestCase("a.Name(b, c);", true)]
    [TestCase("x.MagnitudeSquared().LessThanOrEquals(tolerance);", true)]
    [TestCase("new Pair(a, b);", true)]
    [TestCase("this.Candelas; }", true)]
    [TestCase("PlatoJson.Deserialize<Angle>(input);", true)]
    [TestCase("this * b;", true)]
    [TestCase("this.LessThanOrEquals(b) ? this.NotEquals(b) : false;", false)]
    [TestCase("a.Add(b.Multiply(c));", false)]
    [TestCase("x.Add(1.0D);", false)]
    [TestCase("xs.Map(x => x.Add(y));", false)]
    [TestCase("a * b + c;", false)]
    public static void ForwardingShapes(string body, bool forwarding)
        => Assert.That(LeanInliningPass.IsForwarding(body), Is.EqualTo(forwarding));

    [Test]
    public static void ApplyDropsTheAttributeFromStatementAndComplexBodies()
    {
        var text = "    [MethodImpl(AggressiveInlining)] public Number F(Number b)\r\n    {\r\n"
            + "    [MethodImpl(AggressiveInlining)] public Number G(Number b) => b.Add(b.Add(b));\r\n"
            + "    [MethodImpl(AggressiveInlining)] public Number H(Number b) => b.Add(b);\r\n";
        Assert.That(LeanInliningPass.Apply(text), Is.EqualTo("    public Number F(Number b)\r\n    {\r\n"
            + "    public Number G(Number b) => b.Add(b.Add(b));\r\n"
            + "    [MethodImpl(AggressiveInlining)] public Number H(Number b) => b.Add(b);\r\n"));
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
