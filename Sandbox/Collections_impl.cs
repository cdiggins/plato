namespace Plato;

public record Generator<T>(
    T? Current, 
    Func<T, bool> HasValueFunc, 
    Func<T, T> NextFunc) : IGenerator<T>
{
    public IGenerator<T>? Next => Current == null ? null : this with { Current = NextFunc(Current) };
    public bool HasValue => Current != null && HasValueFunc(Current);
}

public record GeneratorWithIndex<T>(
    T? Current,
    int Index,
    Func<T, int, bool> HasValueFunc,
    Func<T, int, (T, int)> NextFunc) : IGenerator<T>
{
    public IGenerator<T>? Next
    {
        get
        {
            if (Current == null) return null;
            var (nextValue, nextIndex) = NextFunc(Current, Index);
            return this with { Current = nextValue, Index = nextIndex };
        }
    }

    public bool HasValue 
        => Current != null && HasValueFunc(Current, Index);
}

public static class Collections
{

    public Generator<(IGenerator<T>, int)> ToGeneratorWithIndex<T>(this IGenerator<T> generator, int index = 0)
        => new(
            (generator, index),
            tuple => tuple.Item1.HasValue,
            input => input.Next);


    public record Array<T>(int Count, Func<int, T> Func) : IArray<T>
    {
        public T this[int index] => Func(index);
        public ArrayGenerator<T> Generator => new(this);
        IGenerator<T> ISequence<T>.Generator => Generator;
    }

    public static IArray<T> Select<T>(this int count, Func<int, T> func) => new Array<T>(count, func);

    public static IGenerator<T>? SkipUntil<T>(this IGenerator<T>? self, Func<T, bool> func)
    {
        while (self != null && !func(self.Current))
            self = self.Next;
        return self;
    }

    public static IGenerator<T>? SkipWhile<T>(this IGenerator<T>? self, Func<T, bool> func)
    {
        while (self != null && func(self.Current))
            self = self.Next;
        return self;
    }

    public static WhereGenerator<TSource> Where<TSource>(this IGenerator<TSource> source,
        Func<TSource, bool> predicate) => new(source, predicate);

    public static ISequence<TSource> Where<TSource>(this ISequence<TSource> source,
        Func<TSource, int, bool> predicate) => new WhereEnumerator<TSource>(source, predicate);

    public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, TResult> selector) => source.Select((x, _) => selector(x));

    public static IArray<TResult> Select<TSource, TResult>(this IArray<TSource> source,
        Func<TSource, TResult> selector) => new Array<TResult>(source.Count, i => selector(source[i]));

    public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source,
        Func<TSource, int, TResult> selector)
        => new SelectEnumerator<TSource, TResult>(source, selector);

    public static IArray<TResult> Select<TSource, TResult>(this IArray<TSource> source,
        Func<TSource, int, TResult> selector)
        => source.Count.Select(i => selector(source[i], i));

    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector) => throw new NotImplementedException();

    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector) => throw new NotImplementedException();

    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector) => throw new NotImplementedException();

    public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        => TakeWhile(source, (_, i) => i < count);

    public static IArray<TSource> Take<TSource>(this IArray<TSource> source, int count)
        => count.Select(x => source[x]);

    public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
        => source.TakeWhile((x, _) => predicate(x));

    public static IEnumerable<TSource> TakeWhile<TSource>(this IEnumerable<TSource> source,
        Func<TSource, int, bool> predicate)
        => new TakeEnumerator<TSource>(source, predicate);

    public static IArray<TSource> Skip<TSource>(this IArray<TSource> source, int count)
        => Math.Min(source.Count - count, 0).Select(i => source[i + count]);

    public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
        => source.SkipWhile((_, index) => index < count);

    public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.SkipWhile((x, _) => predicate(x));

    public static IEnumerable<TSource> SkipWhile<TSource>(this IEnumerable<TSource> source, Func<TSource, int, bool> predicate)
        => new SkipEnumerator<TSource>(source, predicate);

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

    public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        => new ConcatEnumerator<TSource>(first, second);

    public static IArray<TResult> Zip<TFirst, TSecond, TResult>(this IArray<TFirst> first, IArray<TSecond> second,
        Func<TFirst, TSecond, TResult> resultSelector)
        => first.Count.Select(i => resultSelector(first[i], second[i]));

    public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        => throw new NotImplementedException();

    public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

    public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

    public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

    public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

    public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

    public static IEnumerable<TSource> Intersect<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

    public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

    public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

    public static IArray<TSource> Reverse<TSource>(this IArray<TSource> source)
        => source.Count.Select(i => source[source.Count - i - 1]);

    public static IArray<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
        => source.ToIArray().Reverse();

    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second) => throw new NotImplementedException();

    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) => throw new NotImplementedException();

    public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source) => throw new NotImplementedException();

    public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        => source.ToList().ToArray();

    public static TSource[] ToArray<TSource>(this IArray<TSource> source)
    {
        var r = new TSource[source.Count];
        for (var i = 0; i < source.Count; ++i)
            r[i] = source[i];
        return r;
    }

    public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate(new List<TSource>(), (list, x, _) =>
        {
            list.Add(x);
            return list;
        });

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

    public static TSource First<TSource>(this IArray<TSource> source)
        => source[0];

    // TODO: doesn't throw exception. 
    public static TSource First<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate((default(TSource), false), (acc, x) => acc.Item2 ? acc : (x, true)).Item1;

    // TODO: doesn't throw exception. 
    public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate((default(TSource), false), (acc, x) => acc.Item2 ? acc : predicate(x) ? (x, true) : (default(TSource), false)).Item1;

    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate((default(TSource), false), (acc, x) => acc.Item2 ? acc : (x, true)).Item1;

    public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate((default(TSource), false), (acc, x) => acc.Item2 ? acc : predicate(x) ? (x, true) : (default(TSource), false)).Item1;

    public static TSource Last<TSource>(this IArray<TSource> source)
        => source[source.Count - 1];

    // TODO: doesn't throw exception
    public static TSource Last<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate((default(TSource), false), (acc, x) => (x, true)).Item1;

    // TODO: doesn't throw exception
    public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate((default(TSource), false), (acc, x) => predicate(x) ? (x, true) : acc).Item1;

    public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate((default(TSource), false), (acc, x) => (x, true)).Item1;

    public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate((default(TSource), false), (acc, x) => predicate(x) ? (x, true) : acc).Item1;

    public static TSource Single<TSource>(this IArray<TSource> source)
        => source.Count != 1 ? throw new Exception("Expected only one instance") : source[0];

    private static T CheckSingleResult<T>((T value, int count) tuple, bool throwOrDefault)
        => tuple.count == 1 ? tuple.value : throwOrDefault ? throw new Exception($"Single found {tuple.count} results, not 1") : default(T);

    public static TSource Single<TSource>(this IEnumerable<TSource> source)
        => CheckSingleResult(source.Aggregate((default(TSource), 0), (acc, x, index) => (x, acc.Item2 + 1)), true);

    public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => CheckSingleResult(source.Aggregate((default(TSource), 0), (acc, x, index) => predicate(x) ? (x, acc.Item2 + 1) : acc), true);

    public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        => CheckSingleResult(source.Aggregate((default(TSource), 0), (acc, x, index) => (x, acc.Item2 + 1)), false);

    public static TSource SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => CheckSingleResult(source.Aggregate((default(TSource), 0), (acc, x, index) => predicate(x) ? (x, acc.Item2 + 1) : acc), false);

    // TODO: doesn't throw
    public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        => source.Aggregate(default(TSource), (acc, x, i) => i == index ? x : acc);

    // TODO: doesn't throw
    public static TSource ElementAt<TSource>(this IArray<TSource> source, int index)
        => source[index];

    public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        => source.Aggregate(default(TSource), (acc, x, i) => i == index ? x : acc);

    public static TSource ElementAtOrDefault<TSource>(this IArray<TSource> source, int index)
        => index >= 0 && index < source.Count ? source[index] : default;

    public static IArray<int> Range(int start, int count)
        => count.Select(i => i + start);

    public static IEnumerable<TResult> Repeat<TResult>(this TResult element, int count)
        => count.Select(_ => element);

    public static IEnumerable<TResult> Empty<TResult>()
        => default(TResult).Repeat(0);

    public static bool Any<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate(false, (_acc, _x) => true);

    public static bool Any<TSource>(this IArray<TSource> source)
        => source.Count > 0;

    public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate(false, (acc, x) => acc || predicate(x));

    public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate(true, (acc, x) => acc && predicate(x));

    public static int Count<TSource>(this IEnumerable<TSource> source)
        => source.Aggregate(0, (i, x) => i + 1);

    public static int Count<TSource>(this IArray<TSource> source)
        => source.Count;

    public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => source.Aggregate(0, (i, x) => predicate(x) ? i + 1 : i);

    public static long LongCount<TSource>(this IEnumerable<TSource> source)
        => throw new NotImplementedException();

    public static long LongCount<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        => throw new NotImplementedException();

    public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        => throw new NotImplementedException();

    public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        => throw new NotImplementedException();

    public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        => source.Aggregate(default, func);

    public static TResult Aggregate<TSource, TAccumulate, TResult>(this IEnumerable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
        => throw new NotImplementedException();

    public static TAccumulate Aggregate<TSource, TAccumulate>(this IEnumerable<TSource> source,
        TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
        => source.Aggregate(seed, (acc, x, index) => func(acc, x));
}

}