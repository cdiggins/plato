namespace PlatoLib
{
    /*
     * Experiments with LINQ. 
      
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public sealed class UniqueAttribute : Attribute
        {
            // See the attribute guidelines at 
            //  http://go.microsoft.com/fwlink/?LinkId=85236
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public sealed class CloneAsNeededAttribute : Attribute
        {
            // See the attribute guidelines at 
            //  http://go.microsoft.com/fwlink/?LinkId=85236
        }

        public interface IEnumerable<T>
        {
            TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, T, int, TAcc> f);
        }

        public interface IArray<T> : IEnumerable<T>
        {
            T this[int index] { get; }
            int Count { get; }
        }

        [Unique]
        public class ArrayBuilder<T> : ICloneable<ArrayBuilder<T>>
        {
            private T[] _array;

            private ArrayBuilder(T[] array)
                => _array = array;

            public ArrayBuilder(int count)
                => _array = new T[count];

            public IArray<T> ToIArray()
            {
                var r = _array.ToPlato();
                _array = null;
                return r;
            }

            [CloneAsNeeded]
            public ArrayBuilder<T> Set(int n, T element)
            {
                _array[n] = element;
                return this;
            }

            public ArrayBuilder<T> Clone()
                => new ArrayBuilder<T>((T[]) _array.Clone());
        }

        [Unique]
        public class ListBuilder<T> : ICloneable<ListBuilder<T>>
        {
            private List<T> _list;

            private ListBuilder(List<T> list)
                => _list = list;

            public ListBuilder(int count = 0)
                => _list = count > 0 ? new List<T> {Capacity = count} : new List<T>();

            public IArray<T> ToIArray()
            {
                var r = _list.ToPlato();
                _list = null;
                return r;
            }

            [CloneAsNeeded]
            public ListBuilder<T> Add(T element)
            {
                Add(element);
                return this;
            }

            public ListBuilder<T> Clone()
                => new ListBuilder<T>(_list.ToList());
        }

        public interface ICloneable<T>
        {
            T Clone();
        }

        public interface ISet<T> : IEnumerable<T>
        {
            bool Contains(T x);
        }

        public sealed class Array<T> : IArray<T>
        {
            public T this[int index]
                => Func(index);

            public int Count { get; }
            public readonly Func<int, T> Func;

            public Array(int count, Func<int, T> f)
                => (Count, Func) = (count, f);

            public TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, T, int, TAcc> f)
            {
                for (var i = 0; i < Count; ++i)
                    init = f(init, Func(i), i);
                return init;
            }
        }

        public sealed class SelectEnumerator<TSource, TResult> : IEnumerable<TResult>
        {
            public readonly IEnumerable<TSource> Source;
            public readonly Func<TSource, int, TResult> Func;

            public SelectEnumerator(IEnumerable<TSource> source, Func<TSource, int, TResult> f)
                => (Source, Func) = (source, f);

            public TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, TResult, int, TAcc> f)
                => Source.Aggregate(init, (acc, x, index) => f(acc, Func(x, index), index));
        }

        public sealed class WhereEnumerator<T> : IEnumerable<T>
        {
            public readonly IEnumerable<T> Source;
            public readonly Func<T, int, bool> Func;

            public WhereEnumerator(IEnumerable<T> source, Func<T, int, bool> f)
                => (Source, Func) = (source, f);

            public TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, T, int, TAcc> f)
                => Source.Aggregate(init, (acc, x, index) => Func(x, index) ? f(acc, x, index) : acc);
        }

        public sealed class TakeEnumerator<T> : IEnumerable<T>
        {
            public readonly IEnumerable<T> Source;
            public readonly Func<T, int, bool> Func;

            public TakeEnumerator(IEnumerable<T> source, Func<T, int, bool> f)
                => (Source, Func) = (source, f);

            public TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, T, int, TAcc> f)
                => Source.Aggregate((init, true),
                    (acc, x, index) =>
                        acc.Item2 && Func(x, index)
                            ? (f(acc.Item1, x, index), true)
                            : (acc.Item1, false)).Item1;
        }

        public sealed class SkipEnumerator<T> : IEnumerable<T>
        {
            public readonly IEnumerable<T> Source;
            public readonly Func<T, int, bool> Func;

            public SkipEnumerator(IEnumerable<T> source, Func<T, int, bool> f)
                => (Source, Func) = (source, f);

            public TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, T, int, TAcc> f)
                => Source.Aggregate((init, true),
                    (acc, x, index) =>
                        acc.Item2 && Func(x, index)
                            ? (acc.Item1, true)
                            : (f(acc.Item1, x, index), false)).Item1;
        }

        public sealed class ConcatEnumerator<T> : IEnumerable<T>
        {
            public readonly IEnumerable<T> Source1;
            public readonly IEnumerable<T> Source2;

            public ConcatEnumerator(IEnumerable<T> source1, IEnumerable<T> source2)
                => (Source1, Source2) = (source1, source2);

            public TAcc Aggregate<TAcc>(TAcc init, Func<TAcc, T, int, TAcc> f)
                => Source2.Aggregate(Source1.Aggregate(init, f), f);
        }

        /// <summary>
        /// Emulates the functionality of the Linq Enumerable. Works as a drop-in replacement.
        /// TODO:
        /// * some of the exception throwing is not the same.
        /// * the aggregate behavior is bad in some cases, not escaping
        /// * it would be nice to have a single better implementation of aggregate.
        /// * selectmany / join / lookup / dictionary / hashset needs to be implemented
        /// * I moved all of the min/max/average/sum out, these can be created via a source generator.
        /// </summary>
        public static class Enumerable
        {
            public static IArray<T> ToPlato<T>(this IReadOnlyList<T> self)
                => new Array<T>(self.Count, i => self[i]);

            public static IArray<T> ToIArray<T>(this IEnumerable<T> self)
                => self.ToArray().ToPlato();

            public static IArray<T> Select<T>(this int count, Func<int, T> func)
                => new Array<T>(count, func);

            public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,
                Func<TSource, bool> predicate) => source.Where((x, index) => predicate(x));

            public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,
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
                for (var i=0; i < source.Count; ++i)
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
     
     */
    /*
    public static class Sandbox
    {
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
    */
}
