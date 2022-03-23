using NUnit.Framework;
using Plato;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace PlatoCollectionsTest;

public static class Helpers
{
    public static (T, TimeSpan) TimeIt<T>(Func<T> f)
    {
        var sw = Stopwatch.StartNew();
        var r = f();
        return (r, sw.Elapsed);
    }

    public static void ReportTiming<T>(Func<T> f, int loopIterations = 5)
    {
        var (r, t) = TimeIt(() =>
        {
            var tmp = f();
            for (var i = 0; i < loopIterations; ++i)
            {
                var tmp2 = f();
                if (!tmp2.Equals(tmp))
                    throw new Exception();
            }

            return tmp;
        });
        Console.WriteLine($"Result {r} in {t.TotalMilliseconds:0.00}");
    }

    public static string ToEllidedString<T>(this ISequence<T> seq)
        => seq.Iterator.ToEllidedString();

    public static string ToEllidedString<T>(this IIterator<T> iter)
    {
        var sb = new StringBuilder("[");
        T? prev = iter.HasValue ? iter.Value : default;
        for (var i = 0; iter.HasValue; iter = iter.Next, i++)
        {
            if (i < 5)
            {
                sb.Append(iter.Value);
                sb.Append(", ");
            }
            if (i == 5)
            {
                sb.Append("...");
            }
            prev = iter.Value;
        }
        if (prev != null)
        {
            sb.Append(prev);
        }
        sb.Append("]");
        return sb.ToString();
    }

}

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var x = 42.Range();
        var y = Enumerable.Range(0, 42);
        //Assert.True(Compare(x, y));
    }

    [Test]
    public void Test2()
    {
        var xs1 = 42.Range();
        var xs2 = xs1.Take(3);
    }

    public static Func<int, bool> Lt5 = x => x < 5;
    public static Func<int, bool> Gt5 = x => x > 5;
    public static Func<int, bool> Evens = x => x % 2 == 0;
    public static Func<int, int, int> AddInts = (a, b) => a + b;
    public static Func<int, int, int> Count = (a, b) => a + 1;
    public static Func<int, int, int> CountIf(Func<int, bool> f) => (a, b) => f(b) ? a + 1 : a;

    [Test]
    public static void BasicLoops()
    {
        Console.WriteLine(3.Range().ToEllidedString());
        Console.WriteLine(15.Range().ToEllidedString());
        Console.WriteLine(new int[] { 5, 9, 4, 8, 3, 7, 2, 6, 1, 5 }.ToArray().ToEllidedString());
    }

    [Test]
    public static void TestProfiling2()
    {
        var length = 100000;
        Helpers.ReportTiming(() =>
        {
            int r = 0;
            for (var i=0; i < 10; ++i)
                r = length.Range().Where(Lt5).Aggregate(0, AddInts);
            return r;
        });
        Console.WriteLine("Done");
    }

    [Test]
    public static void TestProfiling1()
    {
        var length = 1000000;

        Console.WriteLine("Testing Enumerable.RangeSequence");
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Where(Lt5).Aggregate(0, AddInts));
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Where(Lt5).Count());
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Count(Lt5));
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Aggregate(0, CountIf(Lt5)));
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Aggregate(0, Count));
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Count());
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Select(x => x * 2).Count());
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Take(42).Count());
        Helpers.ReportTiming(() => Enumerable.Range(0, length).Concat(Enumerable.Range(0, length)).Count());
        Console.WriteLine();

        Console.WriteLine("Testing Plato.RangeSequence");
        Helpers.ReportTiming(() => length.Range().Where(Lt5).Aggregate(0, AddInts));
        Helpers.ReportTiming(() => length.Range().Where(Lt5).Count());
        Helpers.ReportTiming(() => length.Range().Count(Lt5));
        Helpers.ReportTiming(() => length.Range().Aggregate(0, CountIf(Lt5)));
        Helpers.ReportTiming(() => length.Range().Aggregate(0, Count));
        Helpers.ReportTiming(() => length.Range().Count());
        Helpers.ReportTiming(() => length.Range().Select(x => x * 2).Count());
        Helpers.ReportTiming(() => length.Range().Take(42).Count());
        Helpers.ReportTiming(() => length.Range().Concat(length.Range()).Count());
        Console.WriteLine();

        Console.WriteLine("Handwritten");
        Helpers.ReportTiming(() => AddIntsLessThanFive(length.Range()));

        Console.WriteLine("Optimzed");
        Helpers.ReportTiming(() => AddIntsLessThanFive_AutomaticallyOptimized(length));
    }

    public static int AddIntsLessThanFive_AutomaticallyOptimized(int length)
    {
        var x1_Source_From = 0;
        var x1_Source_Count = 1000;
        while ((x1_Source_Count > 0) && !(x1_Source_From < 5))
        {
            x1_Source_From += 1;
            x1_Source_Count -= 1;
        }
        var init = 0;
        while (x1_Source_Count > 0)
        {
            init += x1_Source_From;
            x1_Source_From += 1;
            x1_Source_Count -= 1;
            while ((x1_Source_Count > 0) && !(x1_Source_From < 5))
            {
                x1_Source_From += 1;
                x1_Source_Count -= 1;
            }
        }

        return init;
    }

    public static int AddIntsLessThanFive(IRange r)
        => AddIntsLessThanFive((RangeSequence)r);

    public static int AddIntsLessThanFive(RangeSequence r)
    {
        var acc = 0;
        for (var i=r.From; i < r.To; ++i)
        {
            if (i < 5)
            {
                acc = acc + i;
            }
        }
        return acc;
    }

    /*
    public static bool Compare<T>(ISequence2<T> s, IEnumerable<T> e)
        => s.ToEnumerable().SequenceEqual(e);
    */

    // Reasonably fast, but requires allocating and filling out memory
    public static int SumTraditional(IReadOnlyList<int> self)
    {
        var r = 0;
        for (var i = 0; i < self.Count; i++)
        {
            r = r + self[i];
        }
        return r;
    }

    // No allocation necessary, works on anything that has a
    // "first" value, and a "get next" function. 
    // Ridiculously bad in worst case: O(N^2) 
    public static int SumEnumerable_VerySlow(IEnumerable<int> self)
    {
        var r = 0;
        for (var i = 0; i < self.Count(); i++)
        {
            r = r + self.ElementAt(i);
        }
        return r;
    }

    // This has okay performance and has no memory overhead 
    // Requires "Enumerator" to be mutable. 
    public static int SumEnumerable_LongForm(IEnumerable<int> self)
    {
        var i = self.GetEnumerator();
        var r = 0;
        while (i.MoveNext())
        {
            r = r + i.Current;
        }
        return r;
    }

    // This is a short form, slightly slower due to Lambda
    public static int Sum(IEnumerable<int> self)
        => self.Aggregate(0, (a, b) => a + b);

    // We are paying an abstraction penalty. 
    // Alternatives :
    // https://github.com/nessos/LinqOptimizer
    // https://github.com/jackmott/LinqFaster
    // https://github.com/antiufo/roslyn-linq-rewrite
    // https://github.com/nessos/LinqOptimizer
    // https://github.com/asc-community/HonkPerf.NET
    // https://github.com/NetFabric/NetFabric.Hyperlinq
    // https://github.com/manofstick/Cistern.ValueLinq
    // https://github.com/kevin-montrose/LinqAF
    // https://github.com/VictorNicollet/NoAlloq
    // https://github.com/reegeek/StructLinq




    [Test]
    public static void Inlining()
    {
        var length = 1000;

        // Our Two Lambdas 
        Func<int, bool> Lt5 = x => x < 5;
        Func<int, int, int> AddInts = (x, y) => x + y;

        // Original expression
        ///
        ///


        // Source Code: 
        var r = length.Range().Where(Lt5).Aggregate(0, AddInts);

        // Expansion 1 (Intermediate variable assignment)
        {
            var x0 = length.Range();
            var x1 = x0.Where(Lt5);
            var x2 = x1.Aggregate(0, AddInts);
            Assert.AreEqual(x2, r);
        }

        // Expansion 2 (Inline function)
        {
            var x0 = new RangeSequence(0, 1000);
            var x1 = x0.Iterator.Where(Lt5).ToSequence();
            var x2 = x1.Iterator.Aggregate(0, AddInts);
            Assert.AreEqual(x2, r);
        }

        // Expansion 3 (Intermediate variable assignment #2)
        {
            var x0 = new RangeSequence(0, 1000);
            var x1 = x0.Iterator;
            var x2 = x1.Where(Lt5);
            var x3 = x2.ToSequence();
            var x4 = x3.Iterator;
            var x5 = x4.Aggregate(0, AddInts);
            Assert.AreEqual(x5, r);
        }

        // Expansion 4: Inline Aggregate
        {
            var x0 = new RangeSequence(0, 1000);
            var x1 = x0.Iterator;
            var x2 = x1.Where(Lt5);
            var x3 = x2.ToSequence();
            var x4 = x3.Iterator;
            var init = 0;
            while (x4.HasValue)
            {
                init = AddInts(init, x4.Value);
                x4= x4.Next;
            }
            Assert.AreEqual(init, r);
        }

        // Expansion 5: Coalescing some null operations.(x = this), inlining more functions
        {
            var x0 = new RangeSequence(0, 1000);
            var x1 = new WhereIterator<int>(x0, Lt5); 
            var init = 0;
            while (x1.HasValue)
            {
                init = AddInts(init, x1.Value);
                x1 = new WhereIterator<int>(x1.Source.Next, Lt5);
            }
            Assert.AreEqual(init, r);
        }

        // Expansion 6: Expanding WhereIterator to local variables 
        // NOTE: this step requires casting to work properly. 
        {
            var x1_Source = new RangeSequence(0, 1000);
            var x1_Predicate = Lt5;
            while (x1_Source.HasValue && !x1_Predicate(x1_Source.Value))
            {
                x1_Source = (RangeSequence)x1_Source.Next;
            }
            var init = 0;
            while (x1_Source.HasValue)
            {
                init = AddInts(init, x1_Source.Value);
                x1_Source = (RangeSequence)x1_Source.Next;
                x1_Predicate = x1_Predicate;
                while (x1_Source.HasValue && !x1_Predicate(x1_Source.Value))
                {
                    x1_Source = (RangeSequence)x1_Source.Next;
                }
            }
            Assert.AreEqual(init, r);
        }

        // Expansion 7: Inlining more function calls.
        {
            var x1_Source = new RangeSequence(0, 1000);
            while ((x1_Source.Count > 0) && !Lt5(x1_Source.From))
            {
                x1_Source = new RangeSequence(x1_Source.From + 1, x1_Source.Count - 1); 
            }
            var init = 0;
            while ((x1_Source.Count > 0))
            {
                init = AddInts(init, x1_Source.From);
                x1_Source = new RangeSequence(x1_Source.From + 1, x1_Source.Count - 1);
                while ((x1_Source.Count > 0) && !Lt5(x1_Source.From))
                {
                    x1_Source = new RangeSequence(x1_Source.From + 1, x1_Source.Count - 1);
                }
            }
            Assert.AreEqual(init, r);
        }

        // Expansion 8: Expanding the RangeSequence to local variables 
        {
            var x1_Source_From = 0;
            var x1_Source_Count = 1000;
            while ((x1_Source_Count > 0) && !Lt5(x1_Source_From))
            {
                x1_Source_From = x1_Source_From + 1;
                x1_Source_Count = x1_Source_Count - 1;
            }
            var init = 0;
            while (x1_Source_Count > 0)
            {
                init = AddInts(init, x1_Source_From);
                x1_Source_From = x1_Source_From + 1;
                x1_Source_Count = x1_Source_Count - 1;
                while ((x1_Source_Count > 0) && !Lt5(x1_Source_From))
                {
                    x1_Source_From = x1_Source_From + 1;
                    x1_Source_Count = x1_Source_Count - 1;
                }
            }
            Assert.AreEqual(init, r);
        }

        // Expansion 9 (Lambda Expansion)
        {
            var x1_Source_From = 0;
            var x1_Source_Count = 1000;
            while ((x1_Source_Count > 0) && !(x1_Source_From < 5))
            {
                x1_Source_From = x1_Source_From + 1;
                x1_Source_Count = x1_Source_Count - 1;
            }
            var init = 0;
            while (x1_Source_Count > 0)
            {
                init = init + x1_Source_From;
                x1_Source_From = x1_Source_From + 1;
                x1_Source_Count = x1_Source_Count - 1;
                while ((x1_Source_Count > 0) && !(x1_Source_From < 5))
                {
                    x1_Source_From = x1_Source_From + 1;
                    x1_Source_Count = x1_Source_Count - 1;
                }
            }
            Assert.AreEqual(init, r);
        }

        // Expansion 10 (Assignment Simplification)
        {
            var x1_Source_From = 0;
            var x1_Source_Count = 1000;
            while ((x1_Source_Count > 0) && !(x1_Source_From < 5))
            {
                x1_Source_From += 1;
                x1_Source_Count -= 1;
            }
            var init = 0;
            while (x1_Source_Count > 0)
            {
                init += x1_Source_From;
                x1_Source_From += 1;
                x1_Source_Count -= 1;
                while ((x1_Source_Count > 0) && !(x1_Source_From < 5))
                {
                    x1_Source_From += 1;
                    x1_Source_Count -= 1;
                }
            }
            // Assert.AreEqual(init, r);
        }
    }
}



public static class Compatibility
{
    /*
    public static IEnumerable<T> ToEnumerable<T>(this ISequence2<T> self)
        => self.Iterator.ToEnumerable();

    public static IEnumerable<T> ToEnumerable<T>(this IIterator<T> self)
    {
        while (self.HasValue)
        {
            yield return self.Value;
            self = self.Next;
        }
    }
    */
}
