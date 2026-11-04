using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.CSharpWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class SharedCallsParityTests
{
    private Compilation _compilation = null!;

    [OneTimeSetUp]
    public void CompileFixture()
    {
        var foundation = PlatoSource.FindRepoFolder("stdlib/foundation");
        var fixtures = PlatoSource.FindRepoFolder("tests/PlatoTests/Optimization/SharedCallsFixtures");
        // Use a statement body so earlier inlining leaves calls for the sharing pass to inspect.
        var source = Path.Combine(Path.GetTempPath(), $"plato-shared-probe-{Guid.NewGuid():N}.plato");
        File.WriteAllText(source, File.ReadAllText(Path.Combine(fixtures, "shared-calls.plato"))
            .Replace("Cost(n: Integer): Integer => n * n + 1;",
                "Cost(n: Integer): Integer { var r = 1; var i = 0; while (i < n * n) { r = r + 1; i = i + 1; } return r; }"));
        _compilation = new Compilation(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(foundation, "*.plato").Append(source)
                .Select(PlatoSource.ParseFile).ToList());
        Assert.That(_compilation.CompletedCompilation, Is.True);
    }

    [TestCase("Twice", "Cost", 2, 1)]
    [TestCase("OnlyInBranches", "Cost", 2, 2)]
    [TestCase("Different", "Cost", 2, 2)]
    public void Both_targets_share_or_refuse_the_same_calls(string name, string callee, int before, int after)
    {
        var definition = _compilation.LibraryDefinitionsByName["SharedProbe"].Functions.Single(f => f.Name == name);
        foreach (var enabled in new[] { false, true })
        {
            var managed = new CSharpWriter(_compilation, "unused")
                { InlineCalls = true, LowerLoops = true, FuseCombinators = true, ShareCalls = enabled };
            var source = managed.TryGetStaticTir(definition);
            var native = new CppWriter(_compilation, new DirectoryPath("unused"), CppDialect.Cpu)
                { CpuOptions = new CppCpuOptions(true, true, false, false, Array.Empty<string>()) { ShareCalls = enabled } };
            int Calls(TirFunction tir) => tir.Body.Descendants().OfType<TirCall>().Count(c => c.Name == callee);
            var expected = enabled ? after : before;
            Assert.That(Calls(managed.RunOptimizerPasses(source, null)), Is.EqualTo(expected), "managed");
            Assert.That(Calls(CppTirPipeline.Run(source, native, null)), Is.EqualTo(expected), "native");
        }
    }

    [Test]
    public void Native_shared_call_laws_execute()
    {
        var output = Path.Combine(Path.GetTempPath(), $"plato-shared-calls-{Guid.NewGuid():N}");
        Directory.CreateDirectory(output);
        var laws = _compilation.LibraryDefinitionsByName["SharedProbe"].Functions
            .Where(f => f.Name.StartsWith("Law_")).Select(f => f.Name).ToArray();
        Assert.That(laws, Is.Not.Empty);
        var options = new CppCpuOptions(true, true, false, false, laws) { ShareCalls = true };
        var writer = _compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        foreach (var file in writer.Files) File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        var includes = writer.Files.Keys.Where(f => f.StartsWith("plato_functions_") && f.EndsWith(".cpp"))
            .Select(f => $"#include \"{f}\"");
        var checks = laws.Select(name => $"if (!plato::{name}()) return 1;");
        File.WriteAllText(Path.Combine(output, "main.cpp"), string.Join("\n", includes)
            + "\nint main() {\n" + string.Join("\n", checks) + "\nreturn 0;\n}");
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null) Assert.Ignore("MSVC is required for the native execution gate");
        var command = $"\"\"{vcvars}\" >nul && cl /nologo /std:c++17 /EHsc main.cpp /Fe:laws.exe\"";
        var compile = Toolchain.Run("cmd.exe", "/c " + command, output, nativeEnvironment: true);
        Assert.That(compile.ExitCode, Is.Zero, compile.Output);
        var run = Toolchain.Run(Path.Combine(output, "laws.exe"), "", output);
        Assert.That(run.ExitCode, Is.Zero, run.Output);
    }
}
