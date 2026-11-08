// --refine-sums: a case test that the enclosing match arm already decided is not emitted (TIR refinement step 1).
using System.Text.RegularExpressions;
using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.Optimizer;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class SumRefinementTests
{
    // Nested re-matches the subject inside each arm. (A block body matching a local bound to a case constructor does not
    // elaborate in this fixture, "no ground TIR", with or without the pass, so the let-binding rule has no emission test here.)
    private const string Source = """
        type Shape = Circle(Radius: Number) | Rect(Width: Number, Height: Number) | Dot;
        type Box { Inner: Shape; Tag: Number; }
        library RefineFixture
        {
            Twice(s: Shape): Number
                => match (s) {
                    Circle(r) => s.Measure + r;
                    Rect(w, h) => s.Measure + w;
                    Dot => s.Measure;
                };
            ViaRead(b: Box): Number
                => match (b.Inner) {
                    Circle(r) => match (b.Inner) { Circle(q) => q + r; Rect(w, h) => w; Dot => 1.0; };
                    Rect(w, h) => b.Inner.Measure + b.Tag;
                    Dot => 3.0;
                };
            Measure(s: Shape): Number
                => match (s) {
                    Circle(r) => 3.0 * r * r;
                    Rect(w, h) => w * h;
                    Dot => 0.0;
                };
            Nested(s: Shape): Number
                => match (s) {
                    Circle(r) => match (s) { Circle(q) => q + r; Rect(w, h) => w; Dot => 1.0; };
                    Rect(w, h) => match (s) { Circle(q) => q; Rect(a, b) => a + b + w; Dot => 2.0; };
                    Dot => match (s) { Circle(q) => q; Rect(a, b) => a; Dot => 3.0; };
                };
        }
        """;

    private static readonly string[] Roots = { "Measure", "Nested", "Twice", "ViaRead" };

    private static string Emit(bool refine)
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-refine-sums-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var source = Path.Combine(output, "refine.plato");
        File.WriteAllText(source, Source);
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Append(source).Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var switches = new List<string> { "bind-scrutinee" };
        if (refine)
            switches.Add(TirSumRefinement.Switch);
        var options = new CppCpuOptions(false, false, false, false, Roots) { Switches = new SwitchSet(switches.ToArray()) };
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        Assert.That(writer.Skipped.Where(l => Roots.Any(r => l.Contains(r))), Is.Empty, "every root is emitted");
        return string.Join("\n", writer.Files.Where(f => f.Key.EndsWith(".cpp") || f.Key.EndsWith(".hpp")).Select(f => f.Value.ToString()));
    }

    /// <summary>The one-line definition of <paramref name="name"/>: its signature line through its closing brace.</summary>
    private static string Body(string text, string name)
    {
        var m = Regex.Match(text, @"^[^\r\n;]*\b" + name + @"\([^\r\n;]*\)\s*\{(.*)\}[ \t]*\r?$", RegexOptions.Multiline);
        var at = text.IndexOf(name + "(", StringComparison.Ordinal);
        Assert.That(m.Success, Is.True, $"{name} emitted; first mention: {(at < 0 ? "none" : text.Substring(at, Math.Min(400, text.Length - at)))}");
        return m.Groups[1].Value;
    }

    private static int KindTests(string body) => Regex.Matches(body, @"\.Kind == ").Count;

    [Test]
    public void The_inner_match_in_each_arm_is_gone()
    {
        var plain = Emit(false);
        var refined = Emit(true);
        Assert.That(KindTests(Body(plain, "Nested")), Is.EqualTo(2 + 3 * 2), "every arm re-tests the subject without the switch");
        Assert.That(KindTests(Body(refined, "Nested")), Is.EqualTo(2), "Nested keeps only its own two tests");
        Assert.That(Body(refined, "Measure"), Is.EqualTo(Body(plain, "Measure")), "a single match is untouched");
        // Step 2: Measure is one match on its parameter, so in an arm that knows s it is expanded and collapses to that arm.
        Assert.That(Body(plain, "Twice"), Does.Contain("Measure"), "without the pass Measure stays a call");
        var twice = Body(refined, "Twice");
        Assert.That(KindTests(twice), Is.EqualTo(2));
        Assert.That(twice, Does.Not.Contain("Measure").And.Contain("3.0 *").And.Contain("Rect_Width * "), "each arm holds its own case's measure");
        // A pure read is a value: b.Inner inside the arm is the same value the arm matched on.
        Assert.That(KindTests(Body(plain, "ViaRead")), Is.EqualTo(4));
        var viaRead = Body(refined, "ViaRead");
        Assert.That(KindTests(viaRead), Is.EqualTo(2));
        Assert.That(viaRead, Does.Not.Contain("Measure").And.Contain("Rect_Height"), "the rect arm's measure is w * h in place");
        // The live arm's field reads survive: circle radius in the circle arm, width and height in the rect arm.
        var nested = Body(refined, "Nested");
        Assert.That(nested, Does.Contain("Circle_Radius").And.Contain("Rect_Width").And.Contain("Rect_Height"));
        Assert.That(nested, Does.Contain("3.0"), "the Dot arm's inner Dot value is the one kept");
        Assert.That(nested, Does.Not.Contain("1.0").And.Not.Contain("2.0"), "the dead inner Dot arms are gone");
    }
}
