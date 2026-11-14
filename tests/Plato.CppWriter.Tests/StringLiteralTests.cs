using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.CppWriter.Policies;
using NUnit.Framework;

namespace PlatoCppWriterTests;

/// <summary>String constants: the literal policies (TKT-1159 static literals, TKT-1223 the pool) and the String view they build.</summary>
[TestFixture]
public class StringLiteralTests
{
    [Test]
    public void The_default_policy_still_builds_with_make_string_n()
        => Assert.That(CppPolicies.For(CppCpuOptions.Default).Strings, Is.TypeOf<MakeStringLiterals>());

    [Test]
    public void The_static_policy_spells_each_constant_as_a_UTF16_literal_with_its_unit_count_and_the_empty_one_as_String()
    {
        var strings = CppPolicies.For(CppCpuOptions.Default with { StaticStringLiterals = true }).Strings;
        Assert.That(strings, Is.TypeOf<StaticStringLiterals>());
        Assert.That(strings.Literal("", 0), Is.EqualTo("String()"));
        Assert.That(strings.Literal("a\"b", 3), Is.EqualTo("PLATO_STR(u\"a\\\"b\", 3)"));
        // The length handed in is UTF-8 bytes; the macro takes UTF-16 code units, and the literal holds them.
        Assert.That(strings.Literal("caf\u00e9", 5), Is.EqualTo("PLATO_STR(u\"caf\\x00E9\", 4)"));
        Assert.That(strings.Literal("\U0001f600", 4), Is.EqualTo("PLATO_STR(u\"\\xD83D\\xDE00\", 2)"));
        Assert.That(strings.Literal("a\0b", 3), Is.EqualTo("PLATO_STR(u\"a\\x0000\" u\"b\", 3)"));
        // A lone surrogate is the character make_string_n would build from its UTF-8 spelling: U+FFFD.
        Assert.That(strings.Literal("\ud83d", 3), Is.EqualTo("PLATO_STR(u\"\\xFFFD\", 1)"));
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

    /// <summary>StringLiteralCheck.cpp: String is a trivially copyable view; PLATO_STR, a pool entry and a literal survive an
    /// arena reset and are shared by threads; the arena forms allocate. With and without the arena fast path, which
    /// leaves a record of Strings unconstructed.</summary>
    [TestCase("")]
    [TestCase("/DPLATO_ARENA_FAST=1")]
    public void String_views_and_constants_hold_across_arena_resets_and_threads(string define)
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required for the native check");
        var tests = PlatoSource.FindRepoFolder("tests/Plato.CppWriter.Tests");
        var prelude = PlatoSource.FindRepoFolder("writers/Plato.CppWriter/prelude");
        var output = Path.Combine(Path.GetTempPath(), $"plato-string-lit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /W4 /WX /utf-8 /DPLATO_STRING_PROBE {define} /I\"{prelude}\" \"{Path.Combine(tests, "StringLiteralCheck.cpp")}\" /Fe:lit.exe\"";
        var compile = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(compile.ExitCode, Is.Zero, compile.Output);
        var run = Toolchain.Run(Path.Combine(output, "lit.exe"), "", output);
        Assert.That(run.ExitCode, Is.Zero, run.Output);
    }
}
