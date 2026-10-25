using ConsumerGeometry;

if (args.Length != 5) throw new ArgumentException("Expected repo-root snapshot-root frozen-stdlib evidence-directory mode(census|verify|benchmark).");
var repo = Path.GetFullPath(args[0]);
var snapshot = Path.GetFullPath(args[1]);
var stdlib = Path.GetFullPath(args[2]);
var evidence = Path.GetFullPath(args[3]);
Directory.CreateDirectory(evidence);
var fixture = Path.Combine(repo, "tools/experiments/optimizer-geometry/consumer-fusion/geometry.plato");
Generation.Fingerprint(snapshot, stdlib, fixture, evidence);
Generation.VerifyLoadedAssemblies(snapshot, evidence, "before");
try
{
    if (args[4] == "census")
        Generation.Census(stdlib, evidence);
    else
    {
        if (args[4] is not ("verify" or "benchmark")) throw new ArgumentException("Unknown mode.");
        var compilation = Generation.Compilation(stdlib, fixture);
        var off = Generation.Build(compilation, evidence, false);
        var on = Generation.Build(compilation, evidence, true);
        Qualification.Verify(off, on, evidence);
        if (args[4] == "benchmark") Qualification.Benchmark(off, on, evidence);
    }
}
finally { Generation.VerifyLoadedAssemblies(snapshot, evidence, "after"); }
