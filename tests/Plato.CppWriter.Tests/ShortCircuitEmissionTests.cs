// Checks that the C++ writer emits Boolean && and || so the right operand is evaluated only when the left does not decide.
using System.Text.RegularExpressions;
using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

/// <summary>
/// TKT-1141: the elaborator turns Boolean <c>a &amp;&amp; b</c> into <c>a ? b : false</c> and <c>a || b</c> into
/// <c>a ? true : b</c> (docs/SEMANTICS.md section 6). C++'s <c>?:</c> evaluates one arm, so the Cpu dialect needs no
/// operator of its own; the guarded <c>At</c> must stay inside the arm, whether the loop lowerer is on or off.
/// </summary>
[TestFixture]
public class ShortCircuitEmissionTests
{
    private const string Source = """
        library ScCpp
        {
            AndAt(xs: Array<Integer>): Boolean => xs.Count > 3 && xs[3] == 0;
            OrAt(xs: Array<Integer>): Boolean => xs.Count <= 3 || xs[3] == 0;
            AndLoop(xs: Array<Integer>): Boolean => xs.Count > 3 && MapRange(4, i => xs[i]).Filter(x => x != 0).Count == 0;
        }
        """;

    private static string Emit(bool loops)
    {
        var path = Path.Combine(Path.GetTempPath(), $"sc-cpp-{Guid.NewGuid():N}.plato");
        File.WriteAllText(path, Source);
        try
        {
            var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
            var asts = Directory.GetFiles(folder, "*.plato").Append(path).Select(PlatoSource.ParseFile).ToList();
            var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
            Assert.That(compilation.CompletedCompilation, "the fixture did not compile");
            var options = new CppCpuOptions(loops, loops, false, false, new[] { "AndAt", "OrAt", "AndLoop" });
            var writer = compilation.ToCpp(new DirectoryPath(Path.Combine(Path.GetTempPath(), "plato-cpp-sc")), CppDialect.Cpu, true, options);
            return string.Join("\n", writer.Files.Values.Select(v => v.ToString()));
        }
        finally { File.Delete(path); }
    }

    private static readonly Lazy<string> Plain = new(() => Emit(false));
    private static readonly Lazy<string> Lowered = new(() => Emit(true));

    private static string Definition(string text, string name)
    {
        var m = Regex.Match(text, $@"^bool {name}\([^;\n]*\)\s*\{{[^\n]*\n(?:[^\w\n][^\n]*\n|\n)*", RegexOptions.Multiline);
        Assert.That(m.Success, $"{name} was not emitted");
        return m.Value;
    }

    [TestCase(false)]
    [TestCase(true)]
    public void And_is_a_conditional_with_the_constant_false(bool loops)
    {
        var body = Definition(loops ? Lowered.Value : Plain.Value, "AndAt");
        Assert.That(body, Does.Match(@"\? .* : false\)"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Or_is_a_conditional_with_the_constant_true(bool loops)
        => Assert.That(Definition(loops ? Lowered.Value : Plain.Value, "OrAt"), Does.Match(@"\? true : "));

    [Test]
    public void A_loop_in_the_right_operand_stays_in_the_arm()
    {
        var body = Definition(Lowered.Value, "AndLoop");
        var guard = body.IndexOf("Count", StringComparison.Ordinal);
        var loop = Regex.Match(body, @"\bfor\b|MapRange|\bwhile\b");
        Assert.That(guard, Is.GreaterThanOrEqualTo(0), body);
        if (loop.Success)
            Assert.That(loop.Index, Is.GreaterThan(guard), "the loop comes before the guard:\n" + body);
    }
}
