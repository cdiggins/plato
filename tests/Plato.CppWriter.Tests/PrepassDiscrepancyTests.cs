// TKT-1209: a body whose pipeline fails in the parallel prepass but succeeds on the sequential rerun fails the
// generation loudly instead of being emitted from the rerun, which would hide a pass that depends on its thread.
using System.Collections.Concurrent;
using NUnit.Framework;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.CppWriter;
using Ara3D.Geometry.Optimizer;
using Ara3D.Utils;

namespace PlatoCppWriterTests;

[TestFixture]
public class PrepassDiscrepancyTests
{
    private sealed class ProbeException : Exception
    {
        public ProbeException(string name) : base($"probe refused {name}") { }
    }

    /// <summary>Throws the first time it sees each body, and passes the body through after that: the shape of a pass
    /// whose answer depends on the thread it runs on.</summary>
    private sealed class FailsOncePerBody : ITirPass
    {
        private readonly ConcurrentDictionary<object, bool> _seen = new();
        public string Name => "probe-fails-once";
        public TirFunction Apply(TirFunction tir, PassContext ctx)
            => _seen.TryAdd((object)tir.Original ?? tir, true) ? throw new ProbeException(tir.Original?.Name ?? "?") : tir;
    }

    /// <summary>Throws every time for one body: a deterministic failure, which is not a discrepancy.</summary>
    private sealed class AlwaysFailsOn : ITirPass
    {
        private readonly string _name;
        public AlwaysFailsOn(string name) => _name = name;
        public string Name => "probe-always-fails";
        public TirFunction Apply(TirFunction tir, PassContext ctx)
            => tir.Original?.Name == _name ? throw new ProbeException(_name) : tir;
    }

    private static CppWriter CpuWriter(ITirPass probe)
    {
        var folder = PlatoSource.FindRepoFolder("stdlib/foundation");
        var compilation = new Compilation(Ara3D.Logging.Logger.Null, Directory.GetFiles(folder, "*.plato").Select(PlatoSource.ParseFile).ToList());
        Assert.That(compilation.CompletedCompilation, Is.True);
        var output = new DirectoryPath(Path.Combine(Path.GetTempPath(), "plato-prepass-discrepancy"));
        var writer = new CppWriter(compilation, output, CppDialect.Cpu) { InlineCalls = true, CpuOptions = new CppCpuOptions(true, true, false, false, Array.Empty<string>()) };
        writer.CpuPipeline.ProbePass = probe;
        return writer;
    }

    [Test]
    public void A_body_that_fails_in_the_prepass_and_succeeds_on_rerun_fails_the_generation()
    {
        var writer = CpuWriter(new FailsOncePerBody());
        var e = Assert.Throws<InvalidOperationException>(() => writer.WriteAll())!;
        Assert.That(e.Message, Does.Contain("parallel prepass").And.Contain("sequential rerun").And.Contain("TKT-1209"));
        Assert.That(e.InnerException, Is.TypeOf<ProbeException>(), "the prepass's own exception travels with the report");
        Assert.That(e.Message, Does.Contain(e.InnerException!.Message));
    }

    [Test]
    public void A_body_that_fails_both_times_is_not_a_discrepancy()
    {
        var writer = CpuWriter(new AlwaysFailsOn("Add"));
        try { writer.WriteAll(); }
        catch (Exception e) { Assert.That(e, Is.TypeOf<ProbeException>(), "the rerun's own failure surfaces as it did before TKT-1209"); }
    }
}
