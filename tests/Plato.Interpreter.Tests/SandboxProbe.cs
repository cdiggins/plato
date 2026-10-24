// Provides explicitly selected probes against an external STEP sandbox.
using System.Diagnostics;
using Ara3D.Geometry.Compiler;
using Ara3D.Geometry.Compiler.Types;
using Ara3D.Geometry.Interpreter;

namespace Plato.Interpreter.Tests;

/// <summary>Probes over the STEP design folder in bim-open-toolkit (outside this repository); run
/// by hand with --filter, never by the suite.</summary>
[TestFixture, Explicit]
public class SandboxProbe
{
    private const string Sandbox = @"C:\Users\cdigg\git\bim-open-toolkit\sandbox\step-brep-mesh";
    private static readonly string[] Excluded = { "brep.types.plato", "brep.library.plato", "brep.concepts.plato" };

    private static Compilation CompileSandbox()
    {
        // The issue's command: the three stdlib tiers top-level and the sandbox recursively, the
        // three brep files excluded from both (the sandbox redefines their types).
        var files = TestSupport.StdlibTiers.Select(TestSupport.StdlibFolder)
            .SelectMany(d => Directory.GetFiles(d, "*.plato", SearchOption.TopDirectoryOnly))
            .Concat(Directory.GetFiles(Sandbox, "*.plato", SearchOption.AllDirectories))
            .Where(p => p.EndsWith(".plato") && !p.EndsWith(".spec.plato") && !Excluded.Contains(Path.GetFileName(p)))
            .ToArray();
        return TestSupport.Compile(Array.Empty<string>(), files);
    }

    private static T OnBigStack<T>(Func<T> f)
    {
        T r = default;
        Exception err = null;
        var t = new Thread(() => { try { r = f(); } catch (Exception e) { err = e; } }, 512 * 1024 * 1024);
        t.Start(); t.Join();
        if (err != null) throw err;
        return r;
    }

    [Test]
    public void HollowBox_ReferencedVertexCount()
    {
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(CompileSandbox(), NumberProfile.Cpu64);
        var brep = new TypeToken(interp.Compilation.GetTypeDef("BrepModel").ToTypeExpression());
        var r = OnBigStack(() =>
        {
            var model = interp.Call("Witness_HollowBox", brep, 5.785732621227266, 0.5);
            var mesh = interp.Call("WitnessMesh", model);
            TestContext.Out.WriteLine("positions " + interp.Call("Count", interp.Call("Positions", mesh)));
            foreach (var k in new[] { 0L, 1L })
            {
                var shell = interp.Call("RestrictedToShell", model, mesh, interp.Call("ShellIndex", k));
                TestContext.Out.WriteLine($"shell {k}: referenced {interp.Call("ReferencedVertexCount", shell)} of {interp.Call("Count", interp.Call("Positions", shell))}");
            }
            return interp.Call("ReferencedVertexCount", mesh);
        });
        TestContext.Out.WriteLine("ReferencedVertexCount = " + r);
    }

    [Test]
    public void Profile_law()
    {
        var name = System.Environment.GetEnvironmentVariable("PLATO_LAW") ?? "Law_WitnessValidatesClean";
        var interp = new Ara3D.Geometry.Interpreter.Interpreter(CompileSandbox(), NumberProfile.Cpu64);
        // PLATO_NOMEMO=1 and PLATO_TRIALS=n make the work deterministic (the memo decides by
        // measured time what to remember), so allocation and CPU compare across builds.
        if (System.Environment.GetEnvironmentVariable("PLATO_NOMEMO") == "1")
            interp.Memo = null;
        var trials = int.TryParse(System.Environment.GetEnvironmentVariable("PLATO_TRIALS"), out var t) ? t : 25;
        var runner = new LawRunner(interp, new LawRunOptions { Filters = name.Split(','), BudgetSeconds = 600, Trials = trials });
        var profile = new CallProfile();
        interp.Profiler = System.Environment.GetEnvironmentVariable("PLATO_PROFILE") == "1" ? profile : null;
        using var allocs = System.Environment.GetEnvironmentVariable("PLATO_ALLOC") == "1" ? new AllocationSampler() : null;
        var cpu0 = Process.GetCurrentProcess().TotalProcessorTime;
        var wall = Stopwatch.StartNew();
        var alloc0 = GC.GetTotalAllocatedBytes();
        var results = OnBigStack(() => runner.RunAll());
        TestContext.Out.WriteLine($"cpu {(Process.GetCurrentProcess().TotalProcessorTime - cpu0).TotalSeconds:F1}s wall {wall.Elapsed.TotalSeconds:F1}s gen0 {GC.CollectionCount(0)} gen2 {GC.CollectionCount(2)} alloc {(GC.GetTotalAllocatedBytes() - alloc0) / 1e9:F2}GB");
        foreach (var r in results)
            TestContext.Out.WriteLine($"{r}  {r.Elapsed.TotalSeconds:F1}s");
        if (allocs != null)
            foreach (var kv in allocs.ByType.OrderByDescending(kv => kv.Value).Take(25))
                TestContext.Out.WriteLine($"alloc {kv.Value / 1e9,8:F2}GB  {kv.Key}");
        foreach (var line in profile.Report(40))
            TestContext.Out.WriteLine(line);
    }
}

/// <summary>Samples allocations by type from the runtime's GCAllocationTick events (one per
/// about 100 KB allocated, carrying the type of the object that crossed the mark).</summary>
internal sealed class AllocationSampler : System.Diagnostics.Tracing.EventListener
{
    public readonly System.Collections.Concurrent.ConcurrentDictionary<string, long> ByType = new();

    protected override void OnEventSourceCreated(System.Diagnostics.Tracing.EventSource source)
    {
        if (source.Name == "Microsoft-Windows-DotNETRuntime")
            EnableEvents(source, System.Diagnostics.Tracing.EventLevel.Verbose, (System.Diagnostics.Tracing.EventKeywords)0x1);
    }

    protected override void OnEventWritten(System.Diagnostics.Tracing.EventWrittenEventArgs e)
    {
        if (e.EventName == null || !e.EventName.StartsWith("GCAllocationTick"))
            return;
        var i = e.PayloadNames.IndexOf("TypeName");
        var size = e.PayloadNames.IndexOf("AllocationAmount64");
        if (i < 0 || size < 0)
            return;
        ByType.AddOrUpdate((string)e.Payload[i], Convert.ToInt64(e.Payload[size]), (_, v) => v + Convert.ToInt64(e.Payload[size]));
    }
}
