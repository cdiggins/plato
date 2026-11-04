using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.CppWriter;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class PipelineIsolationTests
{
    [Test]
    public async Task Concurrent_compilations_keep_their_own_optimization_options()
    {
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        Compilation Compile() => new(Ara3D.Logging.Logger.Null,
            Directory.GetFiles(folder, "*.plato").Select(PlatoSource.ParseFile).ToList());
        var first = Compile();
        var second = Compile();
        Assert.That(first.CompletedCompilation && second.CompletedCompilation, Is.True);
        var enabled = new CppCpuOptions(true, true, false, false, Array.Empty<string>());
        var disabled = CppCpuOptions.Default;
        string Generate(Compilation compilation, CppCpuOptions options)
        {
            var output = new DirectoryPath(Path.Combine(Path.GetTempPath(), "plato-pipeline-isolation"));
            var writer = compilation.ToCpp(output, CppDialect.Cpu, true, options);
            return string.Join("\n", writer.Files.OrderBy(p => p.Key)
                .Select(p => p.Key + "\n" + p.Value));
        }
        var firstExpected = Generate(first, enabled);
        var secondExpected = Generate(second, disabled);
        Assert.That(firstExpected, Is.Not.EqualTo(secondExpected), "The fixture must exercise different options");
        for (var repeat = 0; repeat < 3; ++repeat)
        {
            using var start = new Barrier(2);
            Task<string> Run(Compilation compilation, CppCpuOptions options) => Task.Run(() =>
            {
                start.SignalAndWait();
                return Generate(compilation, options);
            });
            var results = await Task.WhenAll(Run(first, enabled), Run(second, disabled));
            Assert.That(results[0], Is.EqualTo(firstExpected));
            Assert.That(results[1], Is.EqualTo(secondExpected));
        }
    }
}
