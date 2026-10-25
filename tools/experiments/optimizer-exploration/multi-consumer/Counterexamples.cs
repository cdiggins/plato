using System.Collections;
using Ara3D.Geometry;

namespace OptimizerExploration;

public static class Counterexamples
{
    public static void Run()
    {
        var separateFault = Fault(false);
        var combinedFault = Fault(true);
        if (separateFault != "first consumer at second element" || combinedFault != "second consumer at first element")
            throw new Exception("Fault-order counterexample did not reproduce.");
        Console.WriteLine($"PASS refusal evidence: separate faults '{separateFault}', combined faults '{combinedFault}'.");
        var separate = Kernels.Separate(new StatefulList());
        var combined = Kernels.Combined(new StatefulList());
        if (separate == combined)
            throw new Exception("Host list counterexample did not reproduce.");
        Console.WriteLine("PASS refusal evidence: stateful IReadOnlyList produces different values; interface type alone is insufficient.");
        Number[] values = [1e16, 1, -1e16];
        var original = (values[0] + values[1]) + values[2];
        var reassociated = (values[0] + values[2]) + values[1];
        if (original == reassociated)
            throw new Exception("Reassociation counterexample did not reproduce.");
        Console.WriteLine("PASS refusal evidence: reassociating a fold changes its value.");
    }

    public static string Fault(bool combined)
    {
        try
        {
            var xs = new[] { 0, 1 };
            if (combined)
                foreach (var x in xs)
                {
                    First(x);
                    Second(x);
                }
            else
            {
                foreach (var x in xs)
                    First(x);
                foreach (var x in xs)
                    Second(x);
            }
            return "no fault";
        }
        catch (InvalidOperationException e) { return e.Message; }
    }

    public static void First(int x)
    {
        if (x == 1) throw new InvalidOperationException("first consumer at second element");
    }

    public static void Second(int x)
    {
        if (x == 0) throw new InvalidOperationException("second consumer at first element");
    }

    public sealed class StatefulList : IReadOnlyList<Number>
    {
        private int _reads;
        public int Count => 2;
        public Number this[int index] => ++_reads;
        public IEnumerator<Number> GetEnumerator() => throw new NotSupportedException();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
