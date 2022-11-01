/*
    This is the standard library for Plato collection interfaces.
    All Plato collections are immutable, side-effect free, and thread-safe.
  
    Plato is inspired heavily by LINQ. Many of the LINQ extension methods 
    are implemented for the various types, as they makes sense.     
*/

using System;
using System.Collections.Generic;

namespace Plato
{
    public readonly struct Comparer<T> : IComparer<T>
    {
        public Comparer(Func<T, T, int> func) => Func = func;
        public Func<T, T, int> Func { get; }
        public int Compare(T x, T y)
            => x == null || y == null ? 0 : Func(x, y);
    }

    public static class Orderings
    {
        public static IComparer<int> IntegerOrder { get; }
            = new Comparer<int>((a, b) => b - a);
    }

    public class NoOrder<T> : IComparer<T>
    {
        public int Compare(T x, T y) => 0;
        public static NoOrder<T> Instance { get; } = new NoOrder<T>();
    }


}