using System.Diagnostics;
using System.Text.Json;
using Number = Ara3D.Geometry.Number;

namespace RecordGeometryExperiment;

internal static class Qualification
{
    internal delegate Number Coordinate(Number x, Number y, Number z);
    internal delegate Number Fault(Number x, Func<Number, Number> y, Func<Number, Number> z);
    internal sealed record Variant(Coordinate Coordinate, Fault Fault);
    private static double _sink;

    internal static void Verify(Variant off, Variant on)
    {
        foreach (var x in new[] { 0d, -0d, 1d, -1d, 1e-300, 1e300, double.NaN, double.PositiveInfinity })
            if (BitConverter.DoubleToInt64Bits((double)off.Coordinate(x, 7, -9))
                != BitConverter.DoubleToInt64Bits((double)on.Coordinate(x, 7, -9)))
                throw new Exception("Coordinate bits changed");
        foreach (var fault in new[] { 0, 1, 2 })
        {
            string Observe(Fault invoke)
            {
                var trace = new List<string>();
                Number Callback(int which, Number x)
                {
                    trace.Add(which.ToString());
                    if (fault == which) throw new InvalidOperationException("field " + which);
                    return x;
                }
                try { trace.Add("result " + (double)invoke(3, x => Callback(1, x), x => Callback(2, x))); }
                catch (InvalidOperationException error) { trace.Add(error.Message); }
                return string.Join(",", trace);
            }
            var a = Observe(off.Fault);
            var b = Observe(on.Fault);
            var expected = fault == 0 ? "1,2,result 3" : fault == 1 ? "1,field 1" : "1,2,field 2";
            if (a != b || a != expected) throw new Exception("Unused field evaluation changed: " + a + " / " + b);
        }
        Console.WriteLine("PASS special coordinate bits and exact unused-field callback traces");
    }

    internal static void Measure(Coordinate off, Coordinate on, string output, bool inline)
    {
        var rows = new List<object>();
        foreach (var count in new[] { 16, 4096, 65536 })
        {
            var xs = Enumerable.Range(0, count).Select(i => (Number)((i % 1021 - 510) * 0.003)).ToArray();
            var ys = xs.Reverse().ToArray();
            var zs = xs.Select(x => (Number)(-(double)x)).ToArray();
            var repetitions = Math.Max(1, 1048576 / count);
            double Run(Coordinate kernel)
            {
                var sum = 0d;
                for (var r = 0; r < repetitions; r++)
                    for (var i = 0; i < count; i++) sum += kernel(xs[i], ys[i], zs[i]);
                return sum;
            }
            for (var warm = 0; warm < 16; warm++) { _sink = Run(off); _sink = Run(on); }
            for (var round = 0; round < 7; round++)
                foreach (var enabled in round % 2 == 0 ? new[] { false, true } : new[] { true, false })
                {
                    var allocated = GC.GetAllocatedBytesForCurrentThread();
                    var start = Stopwatch.GetTimestamp();
                    _sink = Run(enabled ? on : off);
                    var milliseconds = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    var bytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
                    rows.Add(new { count, repetitions, round, enabled, milliseconds, bytes, checksum = _sink });
                }
        }
        File.WriteAllText(Path.Combine(output, $"timing-inline-{inline}.json"), JsonSerializer.Serialize(rows,
            new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine("Timing retained; coordinate-only microbenchmark is not end-to-end geometry throughput.");
    }
}
