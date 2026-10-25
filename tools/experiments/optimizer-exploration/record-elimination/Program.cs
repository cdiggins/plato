using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RecordElimination;

// Handwritten emitted-shape experiment, not a Plato compiler pass or actual runtime wrappers.
public readonly struct Number(double value)
{
    public readonly double Value = value;
}

public readonly struct Pair(Number first, Number second)
{
    public readonly Number First = first;
    public readonly Number Second = second;
}

public static class Program
{
    public static readonly List<string> Trace = new();
    public static double Sink;

    public static Number Read(string name, bool fault)
    {
        Trace.Add(name);
        if (fault) throw new InvalidOperationException(name);
        return new Number(name.Length);
    }

    public static Number Convert(Number value, bool fault)
    {
        Trace.Add("convert");
        if (fault) throw new InvalidOperationException("convert");
        return value;
    }

    public static Number Original(bool firstFault, bool secondFault, bool conversionFault)
        => new Pair(Convert(Read("first", firstFault), conversionFault), Read("second", secondFault)).First;

    public static Number Scalarized(bool firstFault, bool secondFault, bool conversionFault)
    {
        var first = Convert(Read("first", firstFault), conversionFault);
        var second = Read("second", secondFault);
        return first;
    }

    public static string Observe(Func<Number> action)
    {
        Trace.Clear();
        try { return $"value:{action().Value}:{string.Join(',', Trace)}"; }
        catch (InvalidOperationException e) { return $"fault:{e.Message}:{string.Join(',', Trace)}"; }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static double Baseline(int count)
    {
        var sum = 0.0;
        for (var i = 0; i < count; i++)
            sum += new Pair(new Number(i + 0.25), new Number(i * 3.5)).First.Value;
        return sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static double Candidate(int count)
    {
        var sum = 0.0;
        for (var i = 0; i < count; i++)
        {
            var first = new Number(i + 0.25);
            var second = new Number(i * 3.5);
            sum += first.Value;
        }
        return sum;
    }

    public static (double Milliseconds, long Bytes) Measure(Func<int, double> kernel)
    {
        var bytes = GC.GetAllocatedBytesForCurrentThread();
        var start = Stopwatch.GetTimestamp();
        Sink = kernel(1_000_000);
        return (Stopwatch.GetElapsedTime(start).TotalMilliseconds, GC.GetAllocatedBytesForCurrentThread() - bytes);
    }

    public static void Main()
    {
        for (var mask = 0; mask < 8; mask++)
        {
            var a = (mask & 1) != 0;
            var b = (mask & 2) != 0;
            var c = (mask & 4) != 0;
            var before = Observe(() => Original(a, b, c));
            var after = Observe(() => Scalarized(a, b, c));
            if (before != after) throw new Exception($"Mismatch: {before} / {after}");
        }
        var unsafeResult = Observe(() => Read("first", false));
        var eagerResult = Observe(() => Original(false, true, false));
        if (unsafeResult == eagerResult) throw new Exception("Counterexample missing");
        Console.WriteLine($"Correctness: 8 eager fault/conversion cases pass; naive projection counterexample {eagerResult}");
        Console.WriteLine($"Runtime: {Environment.Version}; OS: {Environment.OSVersion}; tiering={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation")}");
        Func<int, double> baseline = Baseline;
        Func<int, double> candidate = Candidate;
        for (var i = 0; i < 20; i++) { Sink = baseline(100_000); Sink = candidate(100_000); }
        if (baseline(1_000_000) != candidate(1_000_000)) throw new Exception("Result mismatch");
        for (var i = 0; i < 12; i++)
        {
            var first = Measure(i % 2 == 0 ? baseline : candidate);
            var second = Measure(i % 2 == 0 ? candidate : baseline);
            var a = i % 2 == 0 ? first : second;
            var b = i % 2 == 0 ? second : first;
            Console.WriteLine($"sample={i} baseline_ms={a.Milliseconds:F4} candidate_ms={b.Milliseconds:F4} baseline_bytes={a.Bytes} candidate_bytes={b.Bytes}");
        }
    }
}
