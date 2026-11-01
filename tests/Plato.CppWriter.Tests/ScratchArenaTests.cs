using NUnit.Framework;

namespace PlatoCppWriterTests;

[TestFixture]
public class ScratchArenaTests
{
    [TestCase("check.cpp")]
    [TestCase("scratch-lifetime.cpp")]
    public void Cpu_runtime_preserves_values_and_releases_scratch(string source)
    {
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required for the native execution gate");
        var folder = PlatoSource.FindRepoFolder("writers/Plato.CppWriter/cpu-prelude-check");
        var output = Path.Combine(Path.GetTempPath(), $"plato-arena-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /O2 /EHsc /W4 /WX /I\"{folder}\" \"{Path.Combine(folder, source)}\" /Fe:arena.exe\"";
        var compile = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(compile.ExitCode, Is.Zero, compile.Output);
        var exe = Path.Combine(output, "arena.exe");
        var run = Toolchain.Run(exe, "", output);
        Assert.That(run.ExitCode, Is.Zero, run.Output);
        if (source == "scratch-lifetime.cpp")
            foreach (var invalid in new[] { "overflow", "expired-mark" })
                Assert.That(Toolchain.Run(exe, invalid, output).ExitCode, Is.Not.Zero, invalid);
    }
}
