using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatoLib
{
    public interface IEnumerator<out T>
    {
        bool MoveNext();
        void Reset();
        T Current { get; }
    }

    public interface IEnumerable<T>
    {
        IEnumerator<T> GetEnumerator();
    }

    public interface IArray<T> : IEnumerable<T>
    {
        T this[int index] { get; }
        int Count { get; }
    }

    public sealed class ArrayEnumerator<T> : IArray<T>, IEnumerator<T>
    {
        public T this[int index]
            => Func(index);

        public int Count { get; }
        public readonly Func<int, T> Func;
        private int _index; 

        public ArrayEnumerator(int count, Func<int, T> f)
            => (Count, Func, _index) = (count, f, -1);

        public bool MoveNext()
            => ++_index < Count;

        public T Current
            => this[_index];

        public void Reset()
            => _index = -1;

        public IEnumerator<T> GetEnumerator()
            => this;
    }

    public sealed class ArrayAdapter<T> : IArray<T>, IEnumerator<T>
    {
        public T this[int index]
            => _list[index];

        public int Count => _list.Count;
        private readonly IReadOnlyList<T> _list;
        
        private int _index;

        public ArrayAdapter(IReadOnlyList<T> list)
            => (_list, _index) = (list, -1);

        public bool MoveNext()
            => ++_index < Count;

        public T Current
            => this[_index];

        public void Reset()
            => _index = -1;

        public IEnumerator<T> GetEnumerator()
            => this;
    }

    public sealed class SelectEnumerator<TSource, TResult> : IEnumerable<TResult>, IEnumerator<TResult>
    {
        public readonly IEnumerator<TSource> Source;
        public readonly Func<TSource, TResult> Func;

        public SelectEnumerator(IEnumerator<TSource> source, Func<TSource, TResult> f)
            => (Source, Func) = (source, f);

        public IEnumerator<TResult> GetEnumerator()
            => this;

        public TResult Current 
            => Func(Source.Current);

        public bool MoveNext()
            => Source.MoveNext();

        public void Reset()
            => Source.Reset();
    }

    public sealed class SelectEnumeratorIndexed<TSource, TResult> : IEnumerable<TResult>, IEnumerator<TResult>
    {
        public readonly IEnumerator<TSource> Source;
        public readonly Func<TSource, int, TResult> Func;
        public int _index;

        public SelectEnumeratorIndexed(IEnumerator<TSource> source, Func<TSource, int, TResult> f)
            => (Source, Func, _index) = (source, f, -1);

        public IEnumerator<TResult> GetEnumerator()
            => this;

        public TResult Current
            => Func(Source.Current, _index);

        public bool MoveNext()
        {
            _index++;
            return Source.MoveNext();
        }

        public void Reset()
        {
            _index = -1;
            Source.Reset();
        }
    }

    public sealed class WhereEnumerator<T> : IEnumerable<T>, IEnumerator<T>
    {
        public readonly IEnumerator<T> Source;
        public readonly Func<T, bool> Func;

        public WhereEnumerator(IEnumerator<T> source, Func<T, bool> f)
            => (Source, Func) = (source, f);

        public IEnumerator<T> GetEnumerator()
            => this;

        public T Current
            => Source.Current;

        public bool MoveNext()
        {
            while (Source.MoveNext())
            {
                if (Func(Current))
                    return true;
            }

            return false;
        }

        public void Reset()
            => Source.Reset();
    }

    public sealed class WhereEnumeratorIndexed<T> : IEnumerable<T>, IEnumerator<T>
    {
        public readonly IEnumerator<T> Source;
        public readonly Func<T, int, bool> Func;
        private int _index;

        public WhereEnumeratorIndexed(IEnumerator<T> source, Func<T, int, bool> f)
            => (Source, Func, _index) = (source, f, -1);

        public IEnumerator<T> GetEnumerator()
            => this;

        public T Current
            => Source.Current;

        public bool MoveNext()
        {
            _index++;
            while (Source.MoveNext())
            {
                if (Func(Current, _index))
                    return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
            Source.Reset();
        }
    }

    // I want all of these to work on arrays (IReadOnlyList), and I want them to work on IArray as well (and System IEnumerable)
    // And I want this library to be .NET Standard 2.0.  
    public static class Enumerable
    {
        public static IArray<T> ToPlato<T>(this IReadOnlyList<T> self)
            => new ArrayAdapter<T>(self);

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,
            Func<TSource, bool> predicate) => new WhereEnumerator<TSource>(source.GetEnumerator(), predicate);

        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,
            Func<TSource, int, bool> predicate) => new WhereEnumeratorIndexed<TSource>(source.GetEnumerator(), predicate);

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector) => new SelectEnumerator<TSource, TResult>(source.GetEnumerator(), selector);

        public static IArray<TResult> Select<TSource, TResult>(this IArray<TSource> source,
            Func<TSource, TResult> selector) => new ArrayEnumerator<TResult>(source.Count, i => selector(source[i]));

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector) => throw new NotImplementedException();

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) => throw new NotImplementedException();

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) => throw new NotImplementedException();

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) => throw new NotImplementedException();

        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count) => throw new NotImplementedException();

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => throw new NotImplementedException();

        public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count) => throw new NotImplementedException();

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate) => throw new NotImplementedException();

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector) => throw new NotImplementedException();

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();
        
        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector) => throw new NotImplementedException();

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) => throw new NotImplementedException();

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => throw new NotImplementedException();

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector) => throw new NotImplementedException();

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector) => throw new NotImplementedException();

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector) => throw new NotImplementedException();

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

        public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
            => source.ToList().ToArray();

        public static TSource[] ToArray<TSource>(this IArray<TSource> source)
        {
            var r = new TSource[source.Count];
            for (var i=0; i < source.Count; ++i)
                r[i] = source[i];
            return r;
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            var r = new List<TSource>();
            foreach (var x in source)
                r.Add(x);
            return r;
        }

        public static List<TSource> ToList<TSource>(this IArray<TSource> source)
        {
            var r = new List<TSource> { Capacity = source.Count };
            for (var i = 0; i < source.Count; ++i)
                r.Add(source[i]);
            return r;
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => throw new NotImplementedException();

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) => throw new NotImplementedException();

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) => throw new NotImplementedException();

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) => throw new NotImplementedException();

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(this IEnumerable<TSource> source, TSource defaultValue) => throw new NotImplementedException();

        //public static IEnumerable<TResult> OfType<TResult>(this IEnumerable source) => throw new NotImplementedException();

        //public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source) => throw new NotImplementedException();

        public static TSource First<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static TSource Last<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static TSource Single<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) => throw new NotImplementedException();

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) => throw new NotImplementedException();

        public static IEnumerable<int> Range(int start, int count) => throw new NotImplementedException();

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) => throw new NotImplementedException();

        public static IEnumerable<TResult> Empty<TResult>() => throw new NotImplementedException();

        public static bool Any<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            var r = 0;
            var e = source.GetEnumerator(); 
            while (e.MoveNext())
                r++;
            return r;
        }

        public static int Count<TSource>(this IArray<TSource> source)
            => source.Count;

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
            => source.Where(predicate).Count();

        public static long LongCount<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) => throw new NotImplementedException();

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) => throw new NotImplementedException();

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

        public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
            => source.Aggregate(default, func);

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source, TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func)
        {
            foreach (var x in source)
                seed = func(seed, x);
            return seed;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(this IArray<TSource> source, TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> func)
        {
            for (var i = 0; i < source.Count; ++i)
                seed = func(seed, source[i]);
            return seed;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector) => throw new NotImplementedException();

        public static int Sum(this IEnumerable<int> source) => throw new NotImplementedException();

        public static int? Sum(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static long Sum(this IEnumerable<long> source) => throw new NotImplementedException();

        public static long? Sum(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static float Sum(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Sum(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static double Sum(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Sum(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static decimal Sum(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Sum(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static int Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static int? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static long Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static long? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();

        public static int Min(this IEnumerable<int> source) => throw new NotImplementedException();

        public static int? Min(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static long Min(this IEnumerable<long> source) => throw new NotImplementedException();

        public static long? Min(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static float Min(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Min(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static double Min(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Min(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static decimal Min(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Min(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static TSource Min<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static int Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static int? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static long Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static long? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Min<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();

        public static TResult Min<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => throw new NotImplementedException();

        public static int Max(this IEnumerable<int> source) => throw new NotImplementedException();

        public static int? Max(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static long Max(this IEnumerable<long> source) => throw new NotImplementedException();

        public static long? Max(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static double Max(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Max(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static float Max(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Max(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static decimal Max(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Max(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static TSource Max<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static int? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static long Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static long? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Max<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();

        public static TResult Max<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector) => throw new NotImplementedException();

        public static double Average(this IEnumerable<int> source) => throw new NotImplementedException();

        public static double? Average(this IEnumerable<int?> source) => throw new NotImplementedException();

        public static double Average(this IEnumerable<long> source) => throw new NotImplementedException();

        public static double? Average(this IEnumerable<long?> source) => throw new NotImplementedException();

        public static float Average(this IEnumerable<float> source) => throw new NotImplementedException();

        public static float? Average(this IEnumerable<float?> source) => throw new NotImplementedException();

        public static double Average(this IEnumerable<double> source) => throw new NotImplementedException();

        public static double? Average(this IEnumerable<double?> source) => throw new NotImplementedException();

        public static decimal Average(this IEnumerable<decimal> source) => throw new NotImplementedException();

        public static decimal? Average(this IEnumerable<decimal?> source) => throw new NotImplementedException();

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector) => throw new NotImplementedException();

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector) => throw new NotImplementedException();

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector) => throw new NotImplementedException();

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector) => throw new NotImplementedException();

        public static float Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector) => throw new NotImplementedException();

        public static float? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector) => throw new NotImplementedException();

        public static double Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector) => throw new NotImplementedException();

        public static double? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector) => throw new NotImplementedException();

        public static decimal Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector) => throw new NotImplementedException();

        public static decimal? Average<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector) => throw new NotImplementedException();
    }
}
