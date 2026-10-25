using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OutputSizing;

public static class Program
{
    private static long _sink;

    public static bool TrySize(int count, int width, out int size)
    {
        var product = (long)count * width;
        size = product is >= 0 and <= int.MaxValue ? (int)product : 0;
        return count >= 0 && width >= 0 && product <= Array.MaxLength;
    }

    // The compiler must prove width from an exact-typed literal, never a sampled callback.
    public static IReadOnlyList<T> Exact<T>(IReadOnlyList<int> source, int width, Func<int, IReadOnlyList<T>> callback)
    {
        if (!TrySize(source.Count, width, out var size))
            return Baseline.FlatMap(source, callback);
        var result = new T[size];
        var offset = 0;
        for (var i = 0; i < source.Count; i++)
        {
            var part = callback(source[i]);
            for (var j = 0; j < part.Count; j++)
                result[offset++] = part[j];
        }
        return result;
    }

    private static void Require(bool value, string name)
    {
        if (!value) throw new Exception(name);
    }

    private static (string Trace, string Fault) Observe(bool exact, int width, int failAt)
    {
        var trace = new List<string>();
        IReadOnlyList<int> Callback(int x)
        {
            trace.Add($"call:{x}");
            if (x == failAt) throw new InvalidOperationException($"fault:{x}");
            return Enumerable.Range(0, width).Select(j => { trace.Add($"value:{x}:{j}"); return x + j; }).ToArray();
        }
        var fault = "none";
        try
        {
            var input = new[] { 2, 1, 2 };
            var result = exact ? Exact(input, width, Callback) : Baseline.FlatMap(input, Callback);
        }
        catch (InvalidOperationException e) { fault = e.Message; }
        return (string.Join(",", trace), fault);
    }

    private static void Tests()
    {
        foreach (var count in new[] { 0, 1, 7, 101 })
        foreach (var width in new[] { 0, 1, 2, 5 })
        {
            var input = Enumerable.Range(0, count).Select(x => x % 3).ToArray();
            IReadOnlyList<long> Callback(int x) => Enumerable.Range(0, width).Select(j => (long)x - j).ToArray();
            Require(Baseline.FlatMap(input, Callback).SequenceEqual(Exact(input, width, Callback)), "values/order/duplicates");
            foreach (var failAt in new[] { -1, 1, 2 })
                Require(Observe(false, width, failAt) == Observe(true, width, failAt), "callback order and first fault");
        }
        Require(!TrySize(int.MaxValue, 2, out _), "multiplication overflow declines");
        Require(!TrySize(Array.MaxLength, 2, out _), "oversize declines");
        Require(TrySize(int.MaxValue, 0, out var empty) && empty == 0, "zero width arithmetic");
        var zeroTrace = Observe(true, 0, 1);
        Require(zeroTrace.Fault == "fault:1" && zeroTrace.Trace == "call:2,call:1", "zero width must execute callbacks");
        var calls = 0;
        IReadOnlyList<int> Fault(int x) { calls++; throw new InvalidOperationException("callback wins"); }
        var sawCallbackFault = false;
        try { Exact(new HugeSource(), 2, Fault); }
        catch (InvalidOperationException e)
        {
            sawCallbackFault = true;
            Require(e.Message == "callback wins" && calls == 1, "overflow fallback preserves fault");
        }
        Require(sawCallbackFault, "overflow fallback must run the faulting callback");
        // Deliberately false width demonstrates why arbitrary callbacks must be refused.
        var wrong = Exact(new[] { 1 }, 2, x => (IReadOnlyList<int>)new[] { x });
        Require(wrong.Count == 2 && wrong[1] == 0, "false proof produces wrong result");
        foreach (var exact in new[] { false, true })
        {
            var trace = new List<int>();
            int Element(int n) { trace.Add(n); return n == 2 ? throw new ArithmeticException() : n; }
            IReadOnlyList<int> Literal(int x) => new[] { Element(1), Element(2), Element(3) };
            try
            {
                var result = exact ? Exact(new[] { 1, 2 }, 3, Literal) : Baseline.FlatMap(new[] { 1, 2 }, Literal);
                throw new Exception("missing element fault");
            }
            catch (ArithmeticException) { Require(trace.SequenceEqual(new[] { 1, 2 }), "eager literal fault order"); }
        }
        Console.WriteLine("PASS equality, ordered callback/element trace, first faults, zero width, overflow fallback, false-width counterexample");
    }

    private static (double Milliseconds, long Bytes) Measure(Func<IReadOnlyList<long>> action, int repetitions)
    {
        var bytes = GC.GetAllocatedBytesForCurrentThread();
        var start = Stopwatch.GetTimestamp();
        for (var i = 0; i < repetitions; i++)
        {
            var result = action();
            _sink += result.Count + (result.Count > 0 ? result[result.Count - 1] : 0);
        }
        return (Stopwatch.GetElapsedTime(start).TotalMilliseconds / repetitions,
            (GC.GetAllocatedBytesForCurrentThread() - bytes) / repetitions);
    }

    public static void Main()
    {
        Tests();
        Console.WriteLine($"Runtime {RuntimeInformation.FrameworkDescription}; {RuntimeInformation.OSArchitecture}; CPU count {Environment.ProcessorCount}; tiered={Environment.GetEnvironmentVariable("DOTNET_TieredCompilation")}");
        foreach (var count in new[] { 16, 4096, 65536 })
        {
            var input = Enumerable.Range(0, count).ToArray();
            IReadOnlyList<long> Callback(int x) => new long[] { x, x + 1, x + 2 };
            Func<IReadOnlyList<long>> baseline = () => Baseline.FlatMap(input, Callback);
            Func<IReadOnlyList<long>> exact = () => Exact(input, 3, Callback);
            for (var i = 0; i < 150; i++) { baseline(); exact(); }
            var repetitions = Math.Max(5, 262144 / count);
            for (var round = 0; round < 5; round++)
            {
                var a = round % 2 == 0 ? Measure(baseline, repetitions) : Measure(exact, repetitions);
                var b = round % 2 == 0 ? Measure(exact, repetitions) : Measure(baseline, repetitions);
                var left = round % 2 == 0 ? a : b;
                var right = round % 2 == 0 ? b : a;
                Console.WriteLine($"n={count} round={round} baseline_ms={left.Milliseconds:F6} exact_ms={right.Milliseconds:F6} baseline_bytes={left.Bytes} exact_bytes={right.Bytes}");
            }
        }
        Console.WriteLine($"Consumed checksum {_sink}");
    }

    private sealed class HugeSource : IReadOnlyList<int>
    {
        public int Count => int.MaxValue;
        public int this[int index] => index;
        public IEnumerator<int> GetEnumerator() => throw new NotSupportedException();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
