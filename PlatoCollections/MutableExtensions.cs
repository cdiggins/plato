using System;

namespace Plato
{
    public static class MutableExtensions
    {
        // https://en.wikipedia.org/wiki/Quicksort
        public static IMutableArray<T> QuickSort<T>(this IMutableArray<T> xs) where T : IComparable<T>
            => QuickSort(xs, 0, xs.Count - 1, (a, b) => a.CompareTo(b));

        public static IMutableArray<T> QuickSort<T>(this IMutableArray<T> xs, Func<T, T, int> compare)
            => QuickSort(xs, 0, xs.Count - 1, compare);

        public static IMutableArray<T> QuickSort<T>(this IMutableArray<T> xs, int lo, int hi, Func<T, T, int> compare)
        {
            if (lo >= hi) return xs;
            var p = Partition(xs, lo, hi, compare);
            QuickSort(xs, lo, p - 1, compare);
            QuickSort(xs, p + 1, hi, compare);
            return xs;
        }

        public static IMutableArray<T> OrderBy<T>(this IMutableArray<T> xs, Func<T, T, int> compare)
            => xs.QuickSort(compare);

        public static IMutableArray<T> OrderBy<T>(this IMutableArray<T> xs) where T: IComparable<T>
            => xs.QuickSort((a, b) => a.CompareTo(b));

        public static IMutableArray<T0> OrderBy<T0, T1>(this IMutableArray<T0> xs, Func<T0, T1> selector) where T1: IComparable<T1>
            => xs.QuickSort((a, b) => selector(a).CompareTo(selector(b)));

        public static void Swap<T>(this IMutableArray<T> xs, int a, int b)
            => (xs[a], xs[b]) = (xs[b], xs[a]);                    

        public static int Partition<T>(this IMutableArray<T> xs, int lo, int hi, Func<T, T, int> compare)
        {
            var pivot = xs[hi];
            var i = lo - 1;
            for (var j=lo; j < hi; ++j)
            {
                if (compare(xs[j], pivot) <= 0)
                {
                    i++;
                    Swap(xs, i, j);
                }
            }
            i++;
            Swap(xs, i, hi);
            return i;
        }
    }
}