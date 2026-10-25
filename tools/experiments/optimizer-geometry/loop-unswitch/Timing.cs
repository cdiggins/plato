using System.Diagnostics;
using System.Text.Json;

static class Timing
{
    static long sink;
    public static void Run(Qualification.Ring off, Qualification.Ring on, string output)
    {
        var rows = new List<object>();
        foreach (var n in new[] { 4096, 65536 })
        foreach (var direction in new[] { false, true })
        {
            var iterations = Math.Max(4, 262144 / n);
            for (var warm = 0; warm < 8; warm++) { Consume(off(n, direction)); Consume(on(n, direction)); }
            var baseline = new List<double>(); var split = new List<double>();
            var offBytes = new List<long>(); var onBytes = new List<long>();
            (double Nanoseconds, long Bytes) Measure(Qualification.Ring run)
            {
                var beforeBytes = GC.GetAllocatedBytesForCurrentThread();
                var start = Stopwatch.GetTimestamp();
                for (var i = 0; i < iterations; i++) Consume(run(n, direction));
                var elapsed = Stopwatch.GetTimestamp() - start;
                return (elapsed * (1e9 / Stopwatch.Frequency) / iterations,
                    (GC.GetAllocatedBytesForCurrentThread() - beforeBytes) / iterations);
            }
            for (var sample = 0; sample < 3; sample++)
            {
                var a = default((double Nanoseconds, long Bytes));
                var b = a;
                if (sample % 2 == 0) { a = Measure(off); b = Measure(on); }
                else { b = Measure(on); a = Measure(off); }
                baseline.Add(a.Nanoseconds); split.Add(b.Nanoseconds);
                offBytes.Add(a.Bytes); onBytes.Add(b.Bytes);
            }
            var baseMedian = baseline.Order().ElementAt(1);
            var splitMedian = split.Order().ElementAt(1);
            rows.Add(new { Count = n, Forward = direction, Iterations = iterations,
                BaselineNanoseconds = baseline, SplitNanoseconds = split,
                BaselineBytes = offBytes, SplitBytes = onBytes,
                BaselineMedian = baseMedian, SplitMedian = splitMedian, Ratio = splitMedian / baseMedian });
            Console.WriteLine($"TIMING count={n} forward={direction} ratio={splitMedian/baseMedian:F4} bytes={offBytes[0]}/{onBytes[0]}");
        }
        File.WriteAllText(Path.Combine(output, "timings.json"), JsonSerializer.Serialize(new
        {
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            TieredCompilation = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation"),
            Sink = sink, Rows = rows
        }, new JsonSerializerOptions { WriteIndented = true }));
    }
    static void Consume(IReadOnlyList<Ara3D.Geometry.Integer> xs)
        => sink = unchecked(sink + xs.Count + (xs.Count == 0 ? 0 : (long)xs[xs.Count - 1]));
}
