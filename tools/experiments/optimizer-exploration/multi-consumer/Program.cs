using Ara3D.Geometry;
using OptimizerExploration;
using System.Diagnostics;
using System.Runtime.InteropServices;

Console.WriteLine($"{RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSDescription}; {RuntimeInformation.ProcessArchitecture}");
var random = new Random(506);
List<Number[]> cases = [[], [0], [-0.0], [1, -1, 1],
    [1e16, 1, -1e16], [double.PositiveInfinity, double.NegativeInfinity],
    [double.NaN, 2], [double.Epsilon, -double.Epsilon], [double.MaxValue, 1]];
for (var n = 0; n < 200; n++)
    cases.Add(Enumerable.Range(0, random.Next(0, 500)).Select(_ => (Number)(random.NextDouble() * 2 - 1)).ToArray());
foreach (var xs in cases)
{
    Equal(Kernels.Separate(xs), Kernels.Combined(xs));
    Equal(Kernels.Runtime(xs), Kernels.Combined(xs));
}
Console.WriteLine($"PASS: {cases.Count} cases agree bitwise for both accumulators (runtime/separate/combined).");
Counterexamples.Run();
foreach (var size in new[] { 16, 4096, 1_000_000 })
{
    var xs = Enumerable.Range(0, size).Select(i => (Number)((i % 127 - 63) * 0.125)).ToArray();
    var repeats = Math.Max(8, 4_000_000 / size);
    for (var i = 0; i < 100; i++)
    {
        Kernels.Separate(xs);
        Kernels.Combined(xs);
    }
    var separate = new List<double>();
    var combined = new List<double>();
    long separateBytes = 0, combinedBytes = 0;
    for (var sample = 0; sample < 7; sample++)
    {
        if (sample % 2 == 0)
        {
            Measure(Kernels.Separate, separate, ref separateBytes);
            Measure(Kernels.Combined, combined, ref combinedBytes);
        }
        else
        {
            Measure(Kernels.Combined, combined, ref combinedBytes);
            Measure(Kernels.Separate, separate, ref separateBytes);
        }
    }
    separate.Sort();
    combined.Sort();
    Console.WriteLine($"n={size}; calls/sample={repeats}; separate_ms={separate[3]:F3}; combined_ms={combined[3]:F3}; ratio={separate[3] / combined[3]:F3}; bytes/call={separateBytes / (7.0 * repeats):F1}/{combinedBytes / (7.0 * repeats):F1}");

    void Measure(Func<IReadOnlyList<Number>, Moments> fn, List<double> samples, ref long bytes)
    {
        var before = GC.GetAllocatedBytesForCurrentThread();
        var start = Stopwatch.GetTimestamp();
        var result = default(Moments);
        for (var i = 0; i < repeats; i++)
            result = fn(xs);
        var elapsed = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        bytes += GC.GetAllocatedBytesForCurrentThread() - before;
        samples.Add(elapsed);
        Equal(Kernels.Separate(xs), result);
    }
}

static void Equal(Moments a, Moments b)
{
    if (BitConverter.DoubleToInt64Bits(a.Sum.Value) != BitConverter.DoubleToInt64Bits(b.Sum.Value)
        || BitConverter.DoubleToInt64Bits(a.Squares.Value) != BitConverter.DoubleToInt64Bits(b.Squares.Value))
        throw new Exception($"Different results: {a} / {b}");
}
