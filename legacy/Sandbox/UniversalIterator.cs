namespace Plato;

public readonly record struct UniversalIterator<TValue, TIterator>(
        TIterator Current,
        Func<TIterator, TIterator> NextFunc,
        Func<TIterator, bool> HasValueFunc,
        Func<TIterator, TValue> ValueFunc)
    : IIterator<TValue>
    where TIterator : IIterator<TValue>
{
    public bool HasValue => !HasValueFunc(Current);
    public IIterator<TValue> Next => this with { Current = NextFunc(Current) };
    public TValue Value => ValueFunc(Current);
}

public readonly record struct ConcatIterator2<TValue>(IIterator<TValue> Iter1, IIterator<TValue> Iter2)
{
    public bool HasValue => true;
    public IIterator<TValue> Next => Iter1.Next.HasValue ? Iter1.Next : Iter2;
    public TValue Value => Iter1.Value;
}

public static class UniversalIteratorExtensions
{
    public static IIterator<TValue> Concat<TValue>(this ISequence<TValue> self, ISequence<TValue> other)
    {
        var iter1 = self.Iterator;
        var iter2 = other.Iterator;
        if (!iter1.HasValue) return iter2;
        return new ConcatIterator<TValue>(iter1, iter2);
    }
}