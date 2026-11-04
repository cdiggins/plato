using NUnit.Framework;

namespace PlatoCppWriterTests;

[TestFixture]
public class ArenaFastPathTests
{
    [TestCase("")]
    [TestCase("/DPLATO_ARENA_FAST=1")]
    [TestCase("/DPLATO_ARENA_TLS_PTR=1")]
    [TestCase("/DPLATO_ARENA_LAZY_PEAK=1")]
    [TestCase("/DPLATO_ARENA_INLINE=1")]
    [TestCase("/DPLATO_ARENA_UNINIT=1")]
    public void Arena_check_passes_with_each_part(string define)
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required for the native execution gate");
        var tests = PlatoSource.FindRepoFolder("tests/Plato.CppWriter.Tests");
        var prelude = PlatoSource.FindRepoFolder("writers/Plato.CppWriter/prelude");
        var output = Path.Combine(Path.GetTempPath(), $"plato-arena-fast-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /W4 /WX {define} /I\"{prelude}\" \"{Path.Combine(tests, "ArenaFastPathCheck.cpp")}\" /Fe:arena.exe\"";
        var compile = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(compile.ExitCode, Is.Zero, compile.Output);
        var run = Toolchain.Run(Path.Combine(output, "arena.exe"), "", output);
        Assert.That(run.ExitCode, Is.Zero, run.Output);
    }

    [Test]
    public void The_fast_path_is_off_by_default()
    {
        var config = File.ReadAllText(Path.Combine(PlatoSource.FindRepoFolder("writers/Plato.CppWriter/prelude"), "plato_config.hpp"));
        Assert.That(config, Does.Contain("#define PLATO_ARENA_FAST 0"));
    }
}
