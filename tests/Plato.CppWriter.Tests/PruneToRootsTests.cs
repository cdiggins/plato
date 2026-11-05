using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>--cpp-prune-to-roots: what the roots reach is written, as the whole program writes it, and nothing else.</summary>
[TestFixture]
public class PruneToRootsTests
{
    private static CppWriter Write(string[] roots, params string[] pruneRoots)
    {
        var scratch = Path.Combine(Path.GetTempPath(), $"plato-prune-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scratch);
        return GoldenEmissionTests.Shared.Value.ToCpp(new DirectoryPath(scratch), CppDialect.Cpu, true,
            new CppCpuOptions(true, true, true, true, roots) { PruneRoots = pruneRoots });
    }

    private static string Text(CppWriter writer, string prefix)
        => string.Concat(writer.Files.Where(f => f.Key.StartsWith(prefix)).OrderBy(f => f.Key, StringComparer.Ordinal).Select(f => f.Value.ToString()));

    [Test]
    public void A_root_keeps_what_it_reaches_and_drops_the_rest()
    {
        var writer = Write(new[] { "Area" });
        var functions = Text(writer, "plato_functions");
        var types = Text(writer, "plato_types");
        Assert.That(functions, Does.Contain("Area(const Shape& s)").Or.Contain("Area(Shape s)").Or.Contain("Area(ReadOnlyParameter<Shape> s)"));
        Assert.That(functions, Does.Not.Contain("Greeting("));
        Assert.That(types, Does.Contain("struct Shape\n"));
        Assert.That(types, Does.Not.Contain("struct Huge\n"));
        Assert.That(Text(writer, "plato_json"), Does.Contain("Shape& v").And.Not.Contain("Huge& v"));
    }

    [Test]
    public void An_overload_of_a_kept_name_stays_only_while_its_parameter_types_are_kept()
    {
        var functions = Text(Write(new[] { "Area" }), "plato_functions");
        Assert.That(functions, Does.Contain("Equals(Shape").Or.Contain("Equals(const Shape"));
        Assert.That(functions, Does.Not.Contain("Equals(Huge").And.Not.Contain("Equals(const Huge"));
    }

    [Test]
    public void A_host_root_may_name_a_type_or_a_function()
    {
        var writer = Write(new[] { "Area" }, "Pair", "Root");
        Assert.That(Text(writer, "plato_types"), Does.Contain("struct Pair\n"));
        Assert.That(Text(writer, "plato_functions"), Does.Contain(" Root(").And.Not.Contain("PairSum("));
    }

    [Test]
    public void Every_kept_definition_is_written_as_the_whole_program_writes_it()
    {
        var scratch = Path.Combine(Path.GetTempPath(), $"plato-prune-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scratch);
        var whole = GoldenEmissionTests.Shared.Value.ToCpp(new DirectoryPath(scratch), CppDialect.Cpu, true,
            new CppCpuOptions(true, true, true, true, new[] { "Area" }));
        var body = Text(whole, "plato_functions_");
        var pruned = Text(Write(new[] { "Area" }), "plato_functions_");
        const string Open = "namespace plato {\n\n";
        foreach (var unit in pruned.Split("} // namespace plato\n", StringSplitOptions.RemoveEmptyEntries))
            foreach (var definition in unit.Substring(unit.IndexOf(Open, StringComparison.Ordinal) + Open.Length).Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
                Assert.That(body, Does.Contain(definition));
    }

    [Test]
    public void An_unknown_root_is_reported()
    {
        var report = Write(new[] { "Area" }, "NoSuchName").UnemittedReachableFromRoots();
        Assert.That(report, Has.Some.Contains("NoSuchName"));
    }
}
