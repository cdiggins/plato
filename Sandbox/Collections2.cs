using System;

namespace PlatoCollectionsTest;

public interface ISequence2<TValue>
{
    public object? GetIterator();
    public ISequence2<TValue> Where(Func<TValue, bool> predicate);
    public ISequence2<TValue> Skip(Func<TValue, bool> predicate);
    public ISequence2<TValue> Skip(int n);
    public ISequence2<TValue> TakeWhile(Func<TValue, bool> predicate);
    public ISequence2<TValue2> Select<TValue2>(Func<TValue, TValue2> transform);
    public ISequence2<TValue> Where(Func<TValue, int, bool> predicate);
    public ISequence2<TValue> Until(Func<TValue, int, bool> predicate);
    public ISequence2<TValue2> Select<TValue2>(Func<TValue, int, TValue2> transform);
    public ISequence2<TValue> Concat(ISequence2<TValue> other);
    public ISequence2<TValue3> Zip<TValue2, TValue3>(ISequence2<TValue2> other, Func<TValue, TValue2, TValue3> func);
    public TAccumulator Aggregate<TAccumulator>(TAccumulator accumulator, Func<TAccumulator, TValue, TAccumulator> reducer);
}

public record Sequence2<TIterator, TValue>(
        Func<TIterator> First,
        Func<TIterator, bool> Predicate,
        Func<TIterator, bool> Terminator,
        Func<TIterator, TValue> Transformer,
        Func<TIterator, TIterator> Advancer
    )
    : ISequence2<TValue>
{
    public object? GetIterator()
        => First();

    public TIterator Advance(TIterator iter, int n)
    {
        for (var i = 0; i < n; ++i)
            iter = Advancer(iter);
        return iter;
    }

    public TIterator Advance(TIterator iter, Func<TIterator, bool> func)
    {
        while (!Terminator(iter) && func(iter))
            iter = Advancer(iter);
        return iter;
    }

    public TIterator Advance(TIterator iter, Func<TValue, bool> func)
    {
        while (!Terminator(iter) && func(Transformer(iter)))
            iter = Advancer(iter);
        return iter;
    }

    public Sequence2<TIterator, TValue> Where(Func<TValue, bool> predicate)
        => new(
            First,
            iter => Predicate(iter) && predicate(Transformer(iter)),
            Terminator,
            Transformer,
            Advancer);

    public Sequence2<TIterator, TValue> Skip(Func<TValue, bool> predicate)
        => new(
            () => Advance(First(), predicate),
            iter => Predicate(iter) && predicate(Transformer(iter)),
            Terminator,
            Transformer,
            Advancer);

    public Sequence2<TIterator, TValue> Skip(int n)
        => new(
            () => Advance(First(), n),
            Predicate,
            Terminator,
            Transformer,
            Advancer);

    public Sequence2<TIterator, TValue> TakeWhile(Func<TValue, bool> predicate)
        => new(
            First,
            Predicate,
            iter => Terminator(iter) && predicate(Transformer(iter)),
            Transformer,
            Advancer);

    public Sequence2<TIterator, TValue2> Select<TValue2>(Func<TValue, TValue2> transform)
        => new(
            First,
            Predicate,
            Terminator,
            iter => transform(Transformer(iter)),
            Advancer);

    public Sequence2<(TIterator, int), TValue> Where(Func<TValue, int, bool> predicate)
        => new(
            () => (First(), 0),
            pair => Predicate(pair.Item1) && predicate(Transformer(pair.Item1), pair.Item2),
            pair => Terminator(pair.Item1),
            pair => Transformer(pair.Item1),
            pair => (Advancer(pair.Item1), pair.Item2 + 1));

    public Sequence2<(TIterator, int), TValue> Until(Func<TValue, int, bool> predicate)
        => new(
            () => (First(), 0),
            pair => Predicate(pair.Item1),
            pair => Terminator(pair.Item1) && predicate(Transformer(pair.Item1), pair.Item2),
            pair => Transformer(pair.Item1),
            pair => (Advancer(pair.Item1), pair.Item2 + 1));

    public Sequence2<(TIterator, int), TValue2> Select<TValue2>(Func<TValue, int, TValue2> transform)
        => new(
            () => (First(), 0),
            pair => Predicate(pair.Item1),
            pair => Terminator(pair.Item1),
            pair => transform(Transformer(pair.Item1), pair.Item2),
            pair => (Advancer(pair.Item1), pair.Item2 + 1));

    private static (T1, T2, bool) Helper<T1, T2>(T1 a, T2 b, Func<T1, bool> f)
        => (a, b, !f(a));

    public Sequence2<(TIterator, TIterator2, bool FirstOrSecond), TValue> Concat<TIterator2>(
        Sequence2<TIterator2, TValue> other)
        => new(
            () => 
                (First(), other.First(), !Terminator(First())),
            triple => 
                triple.FirstOrSecond
                    ? Predicate(triple.Item1)
                    : other.Predicate(triple.Item2),
            triple => 
                triple.FirstOrSecond
                    ? false
                    : other.Terminator(triple.Item2),
            triple => 
                triple.FirstOrSecond
                    ? Transformer(triple.Item1)
                    : other.Transformer(triple.Item2),
            triple =>
                triple.FirstOrSecond
                    ? Helper(Advancer(triple.Item1), triple.Item2, Terminator)
                    : (triple.Item1, other.Advancer(triple.Item2), false)
        );

    public Sequence2<(TIterator, TIterator2), TValue3> Zip<TIterator2, TValue2, TValue3>(
        Sequence2<TIterator2, TValue2> other, Func<TValue, TValue2, TValue3> func)
        => new(
            () => (First(), other.First()),
            pair => Predicate(pair.Item1) && other.Predicate(pair.Item2),
            pair => Terminator(pair.Item1) && other.Terminator(pair.Item2),
            pair => func(Transformer(pair.Item1), other.Transformer(pair.Item2)),
            pair => (Advancer(pair.Item1), other.Advancer(pair.Item2)));

    public TAccumulator Aggregate<TAccumulator>(
        TAccumulator accumulator,
        Func<TAccumulator, TValue, TAccumulator> reducer)
    {
        var iterator = First();
        while (!Terminator(iterator))
        {
            if (Predicate(iterator))
                accumulator = reducer(accumulator, Transformer(iterator));
            iterator = Advancer(iterator);
        }

        return accumulator;
    }

    ISequence2<TValue> ISequence2<TValue>.Where(Func<TValue, bool> predicate) => Where(predicate);
    ISequence2<TValue> ISequence2<TValue>.Skip(Func<TValue, bool> predicate) => Skip(predicate);
    ISequence2<TValue> ISequence2<TValue>.Skip(int n) => Skip(n);
    ISequence2<TValue> ISequence2<TValue>.TakeWhile(Func<TValue, bool> predicate) => TakeWhile(predicate);
    ISequence2<TValue2> ISequence2<TValue>.Select<TValue2>(Func<TValue, TValue2> transform) => Select(transform);
    ISequence2<TValue> ISequence2<TValue>.Where(Func<TValue, int, bool> predicate) => Where(predicate);
    ISequence2<TValue> ISequence2<TValue>.Until(Func<TValue, int, bool> predicate) => Until(predicate);
    ISequence2<TValue2> ISequence2<TValue>.Select<TValue2>(Func<TValue, int, TValue2> transform) => Select(transform);

    // TODO: these two functions are going to be tricky to implement. 

    ISequence2<TValue> ISequence2<TValue>.Concat(ISequence2<TValue> other)
        => Concat((dynamic)other);

    ISequence2<TValue3> ISequence2<TValue>.Zip<TValue2, TValue3>(ISequence2<TValue2> other,
        Func<TValue, TValue2, TValue3> func)
        => Zip((dynamic)other, func);
}

public static class Sequence2
{
    public static Sequence2<int, int> Range(int from, int to)
        => new(() => from, _ => true, i => i >= to, i => i, i => i + 1);

    public static Sequence2<int, int> Range(int to)
        => new(() => 0, _ => true, i => i >= to, i => i, i => i + 1);

    public static int CountElements<T>(this ISequence2<T> self)
        => self.Aggregate(0, (a, _) => a + 1);

    public static int CountElements<T>(this ISequence2<T> self, Func<T, bool> predicate)
        => self.Aggregate(0, (a, b) => predicate(b) ? a + 1 : a);
}
