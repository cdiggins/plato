namespace Plato;

public static class Extensions
{
    public static SingleSequence<T> Unit<T>(this T x) => new(x);

    public static EmptySequence<T> Empty<T>(this T _)
        => EmptySequence<T>.Instance;

    public static EmptySequence<T> Empty<T>()
        => EmptySequence<T>.Instance;

    public static IRange Range(this int n)
        => Range(0, n);

    public static IRange Range(this int n, int upto)
        => new RangeSequence(n, upto);

    public static RepeatedSequence<T> Repeat<T>(this T x, int value) 
        => new(x, value);

    public static ISet<T> ToSet<T>(this Func<T, bool> predicate) 
        => new Set<T>(predicate);

    public static Func<T, bool> Negate<T>(this Func<T, bool> func)
        => x => !func(x);

    public static IArray<T> ToArray<T>(this IReadOnlyList<T> xs)
        => new ArrayAdapter<T>(xs);

    public static IArray<T> ToArray<T>(this IIterator<T> iter)
    {
        var r = new List<T>();
        while (iter.HasValue)
        {
            r.Add(iter.Value);
            iter = iter.Next;
        }
        return new ArrayAdapter<T>(r);
    }

    //public static IIterator<IIterator<T>> Chunk<T>(this IIterator<T> iter, int size)
    //    => new ChunkIterator<T>(iter, size);

    // From: 

    public static Sequence<TValue> 
        ToSequence<TValue>(this IIterator<TValue> iterator)
        => new(iterator);

    public static Sequence<TValue, TIterator>
        ToSequence<TValue, TIterator>(this TIterator iterator)
        where TIterator : IIterator<TValue, TIterator>
        => new(iterator);

    // To:

    public static IIterator<T> Where<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => new WhereIterator<T>(iter, predicate);

    public static ISequence<T> Where<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.Where(predicate).ToSequence();

    /*
    public static WhereIterator<TValue, TIter>
        Where<TValue, TIter>(
            this TIter iter, 
            Func<TValue, bool> predicate
            )
        where TIter : IIterator<TValue, TIter>
        => 
            new(iter, predicate);

    public static Sequence<TValue, WhereIterator<TValue, TIter>>
        Where<TValue, TIter>(
            this ISequence<TValue, TIter> seq, 
            Func<TValue, bool> predicate)
        where TIter : IIterator<TValue, TIter>
        => 
            new(seq.Iterator.Where(predicate));
    */

    public static IIterator<T> Where<T>(this IIterator<T> iter, Func<T, int, bool> predicate)
        => new WhereIndexIterator<T>(iter, predicate, 0);

    public static ISequence<T> Where<T>(this ISequence<T> seq, Func<T, int, bool> predicate)
        => seq.Iterator.Where(predicate).ToSequence();

    public static IIterator<U> Select<T, U>(this IIterator<T> iter, Func<T, U> mapFunc)
        => new SelectIterator<T, U>(iter, mapFunc);

    public static ISequence<U> Select<T, U>(this ISequence<T> seq, Func<T, U> mapFunc)
        => seq.Iterator.Select(mapFunc).ToSequence();

    public static IIterator<U> Select<T, U>(this IIterator<T> iter, Func<T, int, U> mapFunc)
        => new SelectIndexIterator<T, U>(iter, mapFunc);

    public static ISequence<U> Select<T, U>(this ISequence<T> seq, Func<T, int, U> mapFunc)
        => seq.Iterator.Select(mapFunc).ToSequence();

    public static IIterator<T> Take<T>(this IIterator<T> iter, int n)
        => iter.TakeWhile((_, i) => i < n);

    public static ISequence<T> Take<T>(this ISequence<T> seq, int n)
        => seq.Iterator.Take(n).ToSequence();

    public static IIterator<T> TakeWhile<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.TakeWhile((x, _) => predicate(x));

    public static ISequence<T> TakeWhile<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.TakeWhile(predicate).ToSequence();

    public static IIterator<T> TakeWhile<T>(this IIterator<T> iter, Func<T, int, bool> predicate)
        => new TakeIterator<T>(iter, predicate);

    public static ISequence<T> TakeWhile<T>(this ISequence<T> seq, Func<T, int, bool> predicate)
        => seq.Iterator.TakeWhile(predicate).ToSequence();

    public static IIterator<T> Skip<T>(this IIterator<T> iter, int n)
        => iter.AdvanceWhile((_, i) => i < n);

    public static ISequence<T> Skip<T>(this ISequence<T> seq, int n)
        => seq.Iterator.Skip(n).ToSequence();

    public static IIterator<T> SkipLast<T>(this IIterator<T> iter, int n)
        => iter.Take(iter.Count() - n);

    public static ISequence<T> SkipLast<T>(this ISequence<T> seq, int n)
        => seq.Iterator.SkipLast(n).ToSequence();

    public static IIterator<T> SkipWhile<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.AdvanceWhile(x => predicate(x.Value));

    public static IIterator<T> AdvanceWhile<T>(this IIterator<T> iter, Func<IIterator<T>, bool> predicate)
    {
        while (iter.HasValue && predicate(iter))
            iter = iter.Next;
        return iter;
    }

    public static IIterator<T> AdvanceWhile<T>(this IIterator<T> iter, Func<IIterator<T>, int, bool> predicate)
    {
        var i = 0;
        while (predicate(iter, i++))
            iter = iter.Next;
        return iter;
    }

    public static IIterator<T> SkipWhile<T>(this IIterator<T> iter, Func<T, int, bool> predicate)
        => iter.AdvanceWhile((g, index) => predicate(g.Value, index));

    public static ISequence<T> SkipWhile<T>(this ISequence<T> seq, Func<T, int, bool> predicate)
        => seq.Iterator.SkipWhile(predicate).ToSequence();

    public static bool Any<T>(this IIterator<T> iter)
        => iter.HasValue;

    public static bool Any<T>(this ISequence<T> seq)
        => seq.Iterator.Any();

    public static bool Any<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.SkipWhile(predicate).Any();

    public static bool Any<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.Any(predicate);

    public static bool All<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => !iter.SkipWhile(predicate).HasValue;

    public static bool All<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.All(predicate);

    public static T? ValueOrDefault<T>(this IIterator<T> iter)
        => iter.HasValue ? iter.Value : default;

    public static T First<T>(this IIterator<T> iter)
        => iter.Value;

    public static T First<T>(this ISequence<T> seq)
        => seq.Iterator.First();

    public static T First<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.SkipWhile(predicate).First();

    public static T First<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.First(predicate);

    public static T? FirstOrDefault<T>(this IIterator<T> iter)
        => iter.ValueOrDefault();

    public static T? FirstOrDefault<T>(this ISequence<T> seq)
        => seq.Iterator.FirstOrDefault();

    public static T? FirstOrDefault<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.SkipWhile(predicate).FirstOrDefault();

    public static T? FirstOrDefault<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.FirstOrDefault(predicate);

    public static T Last<T>(this IIterator<T> iter)
        => iter.AdvanceWhile(g => g.Next.HasValue).Value;

    public static T Last<T>(this ISequence<T> seq)
        => seq.Iterator.Last();

    public static IIterator<T> AdvanceToLast<T>(this IIterator<T> iter, Func<T, bool> predicate)
    {
        var prev = iter;
        for (; iter.HasValue; prev = prev.Next)
        {
            if (predicate(iter.Value))
                prev = iter;
        }
        return prev;
    }

    public static T Last<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.AdvanceToLast(predicate).Value;

    public static T Last<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.Last(predicate);

    public static T? LastOrDefault<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.AdvanceToLast(predicate).ValueOrDefault();

    public static T? LastOrDefault<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.LastOrDefault(predicate);

    public static int Count<T>(this IIterator<T> iter)
        => iter.Aggregate(0, (i, _) => i + 1);

    public static int Count<T>(this ISequence<T> seq)
        => seq.Iterator.Count();

    public static int Count<T>(this IIterator<T> iter, Func<T, bool> predicate)
        => iter.Aggregate(0, (i, x) => predicate(x) ? i + 1 : i);

    public static int Count<T>(this ISequence<T> seq, Func<T, bool> predicate)
        => seq.Iterator.Count(predicate);

    public static IIterator<T> Prepend<T>(this IIterator<T> iter, T x)
        => x.Unit().Concat(iter);

    public static ISequence<T> Prepend<T>(this ISequence<T> seq, T x)
        => seq.Iterator.Prepend(x).ToSequence();

    public static IIterator<T> Append<T>(this IIterator<T> iter, T x)
        => iter.Concat(x.Unit());

    public static IIterator<T> Append<T>(this ISequence<T> seq, T x)
        => seq.Iterator.Append(x);

    public static T ElementAt<T>(this IIterator<T> iter, int n)
        => iter.Skip(n).First();

    public static T ElementAt<T>(this ISequence<T> seq, int n)
        => seq.Iterator.ElementAt(n);

    public static TAccumulate Aggregate<T, TAccumulate>(
        this IIterator<T> iter, 
        TAccumulate init, 
        Func<TAccumulate, T, TAccumulate> func)
    {
        while (iter.HasValue)
        {
            init = func(init, iter.Value);
            iter = iter.Next;
        }
        return init;
    }

    public static TAccumulate Aggregate<T, TAccumulate>(this ISequence<T> seq, TAccumulate init, Func<TAccumulate, T, TAccumulate> func)
        => seq.Iterator.Aggregate(init, func);

    public static TAccumulate Aggregate<T, TAccumulate>(this IIterator<T> iter, TAccumulate init, Func<TAccumulate, T, int, TAccumulate> func)
    {
        var i = 0;
        while (iter.HasValue)
        {
            init = func(init, iter.Value, i++);
            iter = iter.Next;
        }
        return init;
    }

    public static TAccumulate Aggregate<T, TAccumulate>(this ISequence<T> seq, TAccumulate init, Func<TAccumulate, T, int, TAccumulate> func)
        => seq.Iterator.Aggregate(init, func);

    public static bool Contains<T>(this IIterator<T> iter, T item)
        => iter.Aggregate(false, (_, x) => _ || (x?.Equals(item) ?? false));

    public static bool Contains<T>(this ISequence<T> seq, T item)
        => seq.Iterator.Contains(item);

    public static IIterator<T> Concat<T>(this IIterator<T> iter, IIterator<T> other)
        => new ConcatIterator<T>(iter, other);

    public static ISequence<T> Concat<T>(this ISequence<T> seq, ISequence<T> other)
        => seq.Iterator.Concat(other.Iterator).ToSequence();

    public static IIterator<U> SelectMany<T, U>(this IIterator<T> iter, Func<T, ISequence<U>> func)
        => new SelectManyIterator<T, U>(iter, func);

    public static ISequence<U> SelectMany<T, U>(this ISequence<T> seq, Func<T, ISequence<U>> func)
        => seq.Iterator.SelectMany(func).ToSequence();

    public static bool IsOrderedBy<T>(this IIterator<T> iter, Func<T, T, int> ordering)
        => iter.IsOrderedBy(new Comparer<T>(ordering));

    public static bool IsOrderedBy<T>(this ISequence<T> seq, Func<T, T, int> ordering)
        => seq.Iterator.IsOrderedBy(new Comparer<T>(ordering));

    public static ISortedSequence<T> OrderBy<T>(this IIterator<T> iter, IComparer<T> ordering)
        => iter.ExplicitSort(ordering);

    public static ISortedSequence<T> OrderBy<T>(this IIterator<T> iter, Func<T, T, int> ordering)
        => iter.OrderBy(new Comparer<T>(ordering));

    // TODO:
    public static ISortedSequence<T> ExplicitSort<T>(this IIterator<T> iter, IComparer<T> ordering)
        => throw new NotImplementedException();

    public static bool IsOrderedBy<T>(this ISequence<T> seq, IComparer<T> ordering)
        => seq.Iterator.IsOrderedBy(ordering);

    public static bool IsOrderedBy<T>(this IIterator<T> iter, IComparer<T> ordering)
    {
        if (!iter.HasValue) return true;
        var prev = iter.Value;
        iter = iter.Next;
        while (iter.HasValue)
        {
            var curr = iter.Value;
            if (ordering.Compare(prev, curr) > 0)
            {
                return false;
            }
            prev = curr;
            iter = iter.Next;
        }
        return true;
    }

    public static int IndexOf<T>(this IIterator<T> self, Func<T, int, bool> f)
        => self.Aggregate(-1, (acc, cur, index) => acc >= 0 ? acc : f(cur, index) ? index : -1);

    public static int IndexOf<T>(this ISequence<T> self, Func<T, int, bool> f)
        => self.Iterator.IndexOf(f);

    public static int IndexOf<T>(this IIterator<T> self, Func<T, bool> f)
        => self.Aggregate(-1, (acc, cur, index) => acc >= 0 ? acc : f(cur) ? index : -1);

    public static int IndexOf<T>(this ISequence<T> self, Func<T, bool> f)
        => self.Iterator.IndexOf(f);

    public static int IndexOfLast<T>(this IIterator<T> self, Func<T, int, bool> f)
        => self.Aggregate(-1, (acc, cur, index) => f(cur, index) ? index : acc);

    public static int IndexOfLast<T>(this ISequence<T> self, Func<T, int, bool> f)
        => self.Iterator.IndexOfLast(f);

    public static int IndexOfLast<T>(this IIterator<T> self, Func<T, bool> f)
        => self.Aggregate(-1, (acc, cur, index) => f(cur) ? index : acc);

    public static int IndexOfLast<T>(this ISequence<T> self, Func<T, bool> f)
        => self.Iterator.IndexOfLast(f);

    // TODO: 

    public static IIterator<T> Union<T>(this IIterator<T> self, IIterator<T> other)
        => throw new NotImplementedException();

    public static ISequence<T> Union<T>(this ISequence<T> self, ISequence<T> other)
        => self.Iterator.Union(other.Iterator).ToSequence();

    public static IIterator<T> Except<T>(this IIterator<T> self, IIterator<T> other)
        => throw new NotImplementedException();

    public static ISequence<T> Except<T>(this ISequence<T> self, ISequence<T> other)
        => self.Iterator.Except(other.Iterator).ToSequence();

}
