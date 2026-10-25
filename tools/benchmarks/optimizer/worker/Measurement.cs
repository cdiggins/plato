using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace Plato.OptimizerBenchmark;

public static class Measurement
{
    public static object Run(PreparedFixture fixture, string workload, int warmup, double targetMs, int maxIterations,
        double warmupMs = 200)
    {
        if (warmup < 0 || !double.IsFinite(targetMs) || targetMs <= 0 || maxIterations < 1
            || !double.IsFinite(warmupMs) || warmupMs < 0)
            throw new ArgumentOutOfRangeException(nameof(targetMs), "Warmup must be nonnegative and measurement limits positive");
        var inputs = Operations.BorrowedInputs(fixture, workload);
        string InputHash() => PortableJson.Hash(PortableJson.Bytes(inputs));
        var inputHash = InputHash();
        var operation = Operations.Bind(fixture, workload);
        var reference = operation();
        var referenceBytes = PortableJson.Bytes(reference);
        var checks = ResultChecks.Of(fixture, reference);
        if (InputHash() != inputHash)
            throw new InvalidOperationException("Benchmark operation mutated borrowed inputs during correctness preflight");
        var warmupStart = Stopwatch.GetTimestamp();
        var warmupIterations = 0;
        while (warmupIterations < warmup || (warmupIterations < maxIterations
            && Stopwatch.GetElapsedTime(warmupStart).TotalMilliseconds < warmupMs))
        {
            GC.KeepAlive(operation());
            warmupIterations++;
        }
        var warmupElapsedMs = Stopwatch.GetElapsedTime(warmupStart).TotalMilliseconds;
        var iterations = 1;
        while (true)
        {
            var sample = Batch(operation, iterations);
            var elapsedMs = sample.Ticks * 1000.0 / Stopwatch.Frequency;
            if (elapsedMs >= Math.Min(targetMs, 10) || iterations == maxIterations)
            {
                var estimate = Math.Ceiling(iterations * targetMs / Math.Max(elapsedMs, 0.000001));
                iterations = (int)Math.Clamp(estimate, 1, maxIterations);
                break;
            }
            iterations = (int)Math.Min((long)iterations * 2, maxIterations);
        }
        if (InputHash() != inputHash)
            throw new InvalidOperationException("Benchmark operation mutated borrowed inputs during warmup or calibration");
        var measured = Batch(operation, iterations);
        var bytes = PortableJson.Bytes(measured.Output);
        foreach (var (key, valid) in ResultChecks.Of(fixture, measured.Output))
            checks[key] = checks.GetValueOrDefault(key, true) && valid;
        checks["repeat_digest_matches"] = referenceBytes.AsSpan().SequenceEqual(bytes);
        checks["input_unchanged"] = InputHash() == inputHash;
        return new
        {
            schema = 1, status = "ok", case_id = fixture.Id, workload,
            elapsed_ns = measured.Ticks * (1_000_000_000.0 / Stopwatch.Frequency), iterations,
            allocated_bytes = measured.AllocatedBytes, digest = PortableJson.Hash(bytes), checks,
            diagnostics = ResultChecks.Diagnostics(fixture, measured.Output),
            measurement = "sequential_operations_monotonic_clock", stopwatch_frequency = Stopwatch.Frequency,
            warmup_iterations = warmupIterations, warmup_elapsed_ms = warmupElapsedMs,
            runtime = new { framework = RuntimeInformation.FrameworkDescription, version = Environment.Version.ToString(),
                server_gc = GCSettings.IsServerGC, gc_latency = GCSettings.LatencyMode.ToString(),
                processor_count = Environment.ProcessorCount, process_architecture = RuntimeInformation.ProcessArchitecture.ToString() },
        };
    }

    static BatchResult Batch(Func<object> operation, int iterations)
    {
        object output = null!;
        var allocated = GC.GetTotalAllocatedBytes(true);
        var start = Stopwatch.GetTimestamp();
        for (var i = 0; i < iterations; i++)
            output = operation();
        var ticks = Stopwatch.GetTimestamp() - start;
        var bytes = GC.GetTotalAllocatedBytes(true) - allocated;
        GC.KeepAlive(output);
        return new(output, ticks, bytes);
    }

    sealed record BatchResult(object Output, long Ticks, long AllocatedBytes);
}
