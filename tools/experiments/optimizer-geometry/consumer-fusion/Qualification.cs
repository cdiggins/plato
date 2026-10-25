using System.Collections;
using System.Diagnostics;
using System.Text.Json;
using RuntimeNumber = Ara3D.Geometry.Number;

namespace ConsumerGeometry;

internal static class Qualification
{
    private static double _sink;
    private static long Bits(RuntimeNumber value) => BitConverter.DoubleToInt64Bits((double)value);
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    internal sealed record Cloud(RuntimeNumber[] X, RuntimeNumber[] Y, RuntimeNumber[] Z);

    internal static Cloud Create(int count, int kind = 0)
    {
        var x = new RuntimeNumber[count];
        var y = new RuntimeNumber[count];
        var z = new RuntimeNumber[count];
        for (var i = 0; i < count; i++)
        {
            x[i] = ((i * 17) % 1009 - 504) * 0.01;
            y[i] = ((i * 31) % 1013 - 506) * 0.01;
            z[i] = kind == 1 ? 0 : ((i * 47) % 1019 - 509) * 0.003;
        }
        return new Cloud(x, y, z);
    }

    internal static void Verify(Generation.Kernel off, Generation.Kernel on, string evidence)
    {
        var cases = 0;
        void Compare(Cloud cloud, double nx, double ny, double nz, double offset)
        {
            var a = off(cloud.X, cloud.Y, cloud.Z, nx, ny, nz, offset);
            var b = on(cloud.X, cloud.Y, cloud.Z, nx, ny, nz, offset);
            Require(Bits(a.First) == Bits(b.First) && Bits(a.Second) == Bits(b.Second), "Changed moment bits.");
            cases++;
        }
        foreach (var count in new[] { 0, 1, 2, 16, 257, 4096, 65536 })
        {
            Compare(Create(count), 0.6, 0, 0.8, -0.25);
            Compare(Create(count, 1), 0, 0, 1, 0);
            Compare(Create(count), 0, 0, 0, 0);
            var planar = on(Create(count, 1).X, Create(count, 1).Y, Create(count, 1).Z, 0, 0, 1, 0);
            Require((double)planar.First == 0 && (double)planar.Second == 0, "On-plane analytical check failed.");
        }
        var signed = new Cloud(new RuntimeNumber[4], new RuntimeNumber[4], new RuntimeNumber[] { 1, -1, 2, -2 });
        Compare(signed, 0, 0, 1, 0);
        var expected = on(signed.X, signed.Y, signed.Z, 0, 0, 1, 0);
        Require((double)expected.First == 0 && (double)expected.Second == 10, "Signed residual oracle failed.");
        foreach (var value in new[] { -0d, 1e-300, -1e-300, 1e200, -1e200, double.PositiveInfinity, double.NegativeInfinity, double.NaN })
            Compare(new Cloud(new RuntimeNumber[] { value, 1, value }, new RuntimeNumber[3], new RuntimeNumber[3]), 1, 0, 0, 0);
        var traceOff = Observe(off);
        var traceOn = Observe(on);
        Require(traceOff.SequenceEqual(traceOn), "Producer fault or source read order changed.");
        File.WriteAllText(Path.Combine(evidence, "correctness.json"), JsonSerializer.Serialize(new { cases, traceOff, traceOn, passed = true }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"PASS: {cases} bitwise moment pairs, analytic planar/signed cases, producer fault and read-order agreement.");
    }

    private static string[] Observe(Generation.Kernel kernel)
    {
        var trace = new List<string>();
        var x = new ObservedList("x", trace, -1);
        var y = new ObservedList("y", trace, 2);
        var z = new ObservedList("z", trace, -1);
        try { kernel(x, y, z, 0.6, 0, 0.8, -0.25); throw new Exception("Expected producer fault."); }
        catch (InvalidOperationException exception) { trace.Add(exception.GetType().Name + ":" + exception.Message); }
        return trace.ToArray();
    }

    private sealed class ObservedList(string name, List<string> trace, int fail) : IReadOnlyList<RuntimeNumber>
    {
        public int Count { get { trace.Add(name + ".Count"); return 4; } }
        public RuntimeNumber this[int index]
        {
            get
            {
                trace.Add($"{name}[{index}]");
                if (index == fail) throw new InvalidOperationException("geometry source failure");
                return index + 0.25;
            }
        }
        public IEnumerator<RuntimeNumber> GetEnumerator() => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed record Sample(double Milliseconds, long Bytes, double Checksum);
    private sealed record Measurement(int Count, int Calls, double SeparateMedianMs, double FusedMedianMs,
        double Ratio, long SeparateBytesPerCall, long FusedBytesPerCall, Sample[] Separate, Sample[] Fused);

    private static Sample Measure(Generation.Kernel kernel, Cloud cloud, int calls)
    {
        var sum = 0d;
        var timer = new Stopwatch();
        var before = GC.GetAllocatedBytesForCurrentThread();
        timer.Start();
        for (var i = 0; i < calls; i++)
        {
            var result = kernel(cloud.X, cloud.Y, cloud.Z, 0.6, 0, 0.8, -0.25);
            sum += (double)result.First + (double)result.Second;
        }
        timer.Stop();
        var bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        _sink += sum;
        return new Sample(timer.Elapsed.TotalMilliseconds, bytes, sum);
    }

    internal static void Benchmark(Generation.Kernel off, Generation.Kernel on, string evidence)
    {
        var results = new List<Measurement>();
        foreach (var count in new[] { 4096, 65536 })
        {
            var cloud = Create(count);
            var calls = Math.Clamp(262144 / count, 2, 64);
            for (var warm = 0; warm < 16; warm++)
            {
                _sink += (double)off(cloud.X, cloud.Y, cloud.Z, 0.6, 0, 0.8, -0.25).Second;
                _sink += (double)on(cloud.X, cloud.Y, cloud.Z, 0.6, 0, 0.8, -0.25).Second;
            }
            var separate = new Sample[3];
            var fused = new Sample[3];
            for (var round = 0; round < 3; round++)
            {
                if (round % 2 == 0) { separate[round] = Measure(off, cloud, calls); fused[round] = Measure(on, cloud, calls); }
                else { fused[round] = Measure(on, cloud, calls); separate[round] = Measure(off, cloud, calls); }
                Require(BitConverter.DoubleToInt64Bits(separate[round].Checksum) == BitConverter.DoubleToInt64Bits(fused[round].Checksum), "Timing result not equivalent.");
            }
            var separateMedian = separate.OrderBy(v => v.Milliseconds).ElementAt(1).Milliseconds;
            var fusedMedian = fused.OrderBy(v => v.Milliseconds).ElementAt(1).Milliseconds;
            var result = new Measurement(count, calls, separateMedian, fusedMedian, separateMedian / fusedMedian,
                separate[0].Bytes / calls, fused[0].Bytes / calls, separate, fused);
            results.Add(result);
            Console.WriteLine($"n={count}, calls={calls}: separate={separateMedian:F3}ms, fused={fusedMedian:F3}ms, ratio={result.Ratio:F3}; bytes/call {result.SeparateBytesPerCall}/{result.FusedBytesPerCall}");
        }
        File.WriteAllText(Path.Combine(evidence, "timings.json"), JsonSerializer.Serialize(new { results, sink = _sink }, new JsonSerializerOptions { WriteIndented = true }));
    }
}
