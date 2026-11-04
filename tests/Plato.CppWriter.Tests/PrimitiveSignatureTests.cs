using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class PrimitiveSignatureTests
{
    [Test]
    public void Integer_identity_is_provided_by_the_cpu_runtime()
    {
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var asts = Directory.GetFiles(folder, "*.plato").Select(PlatoSource.ParseFile).ToList();
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, asts);
        Assert.That(compilation.CompletedCompilation, Is.True);
        var output = Path.Combine(Path.GetTempPath(), $"plato-cpu-primitives-{Guid.NewGuid():N}");
        var options = new CppCpuOptions(false, false, false, false, new[] { "ToInteger" });
        var writer = compilation.ToCpp(new DirectoryPath(output), CppDialect.Cpu, true, options);
        var functions = string.Join("\n", writer.Files.Where(f => f.Key.StartsWith("plato_functions"))
            .Select(f => f.Value.ToString()));
        Assert.That(functions, Does.Not.Match(@"int32_t ToInteger\(int32_t "));
        Assert.That(writer.Files["plato_cpu.hpp"].ToString(), Does.Contain("int32_t ToInteger(int32_t"));
        var vcvars = Toolchain.FindVcVars();
        if (vcvars == null)
            Assert.Ignore("MSVC is required for the native compilation gate");
        Directory.CreateDirectory(output);
        foreach (var file in writer.Files)
            File.WriteAllText(Path.Combine(output, file.Key), file.Value.ToString());
        foreach (var file in Directory.GetFiles(output, "*.cpp"))
        {
            var (exit, log) = Toolchain.CompileWithMsvc(vcvars!, output, file);
            Assert.That(exit, Is.Zero, log);
        }
    }
}
