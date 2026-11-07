using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.CppWriter.Policies;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>TKT-1159 idea B3: static string literals, selected by CppCpuOptions.StaticStringLiterals.</summary>
[TestFixture]
public class StringLiteralTests
{
    [Test]
    public void The_default_policy_still_builds_with_make_string_n()
        => Assert.That(CppPolicies.For(CppCpuOptions.Default).Strings, Is.TypeOf<MakeStringLiterals>());

    [Test]
    public void The_static_policy_spells_each_constant_once_per_site_and_the_empty_one_as_String()
    {
        var strings = CppPolicies.For(CppCpuOptions.Default with { StaticStringLiterals = true }).Strings;
        Assert.That(strings, Is.TypeOf<StaticStringLiterals>());
        Assert.That(strings.Literal("", 0), Is.EqualTo("String()"));
        Assert.That(strings.Literal("a\"b", 3), Is.EqualTo("PLATO_STR(\"a\\\"b\", 3)"));
    }

    [Test]
    public void The_pool_policy_numbers_distinct_texts_once_and_wins_over_the_static_policy()
    {
        var strings = CppPolicies.For(CppCpuOptions.Default with { StaticStringLiterals = true, StringPool = true }).Strings;
        Assert.That(strings, Is.TypeOf<PooledStringLiterals>());
        Assert.That(strings.Literal("", 0), Is.EqualTo("String()"));
        Assert.That(strings.Literal("abc", 3), Is.EqualTo("PLATO_SP(0)"));
        Assert.That(strings.Literal("xyz", 3), Is.EqualTo("PLATO_SP(1)"));
        Assert.That(strings.Literal("abc", 3), Is.EqualTo("PLATO_SP(0)"));
        Assert.That(((PooledStringLiterals)strings).Texts, Is.EqualTo(new[] { "abc", "xyz" }));
    }

    [Test]
    public void The_u16_escape_splits_a_literal_where_a_hex_digit_follows_an_escape()
    {
        Assert.That(CppWriter.EscapeCppU16String("a\"b\\c?"), Is.EqualTo("a\\\"b\\\\c\\?"));
        Assert.That(CppWriter.EscapeCppU16String("caf\u00e9"), Is.EqualTo("caf\\x00E9"));
        Assert.That(CppWriter.EscapeCppU16String("\u00e9a"), Is.EqualTo("\\x00E9\" u\"a"));
        Assert.That(CppWriter.EscapeCppU16String("\u00e9-"), Is.EqualTo("\\x00E9-"));
        Assert.That(CppWriter.EscapeCppU16String("\uD83D\uDE00\n"), Is.EqualTo("\\xD83D\\xDE00\\n"));
        Assert.That(CppWriter.EscapeCppU16String("a\0b"), Is.EqualTo("a\\x0000\" u\"b"));
    }

    [Test]
    public void The_static_literal_macro_compiles_and_holds_on_threads()
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required for the native check");
        var tests = PlatoSource.FindRepoFolder("tests/Plato.CppWriter.Tests");
        var prelude = PlatoSource.FindRepoFolder("writers/Plato.CppWriter/prelude");
        var output = Path.Combine(Path.GetTempPath(), $"plato-string-lit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /W4 /WX /utf-8 /DPLATO_STRING_PROBE /I\"{prelude}\" \"{Path.Combine(tests, "StringLiteralCheck.cpp")}\" /Fe:lit.exe\"";
        var compile = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(compile.ExitCode, Is.Zero, compile.Output);
        var run = Toolchain.Run(Path.Combine(output, "lit.exe"), "", output);
        Assert.That(run.ExitCode, Is.Zero, run.Output);
    }
}
