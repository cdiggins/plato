namespace Plato;

/*
public static class Array
{
    public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> mapFunc)
        => self.Select(mapFunc);

    public static ISlice<T> Take<T>(this IArray<T> self, int n)
        => self.Take(n);
}
*/
public partial interface IArray<T>
{
    IArray<ISlice<T>> Chunk(int size)
        => (size / Count).Select(i => Slice(i * size, size));

    IGenerator<T> Generator 
        => new ArrayGenerator<T>(this, 0);

    IArray<U> SelectIndices<U>(Func<int, U> func)
        => Count.Select(func);

    IArray<T> Choose(IArray<int> indicies)
        => indicies.Select(ElementAt);

    IArray<U> Select<U>(Func<T, U> mapFunc)
        => new SelectArray<T, U>(this, mapFunc);

    IArray<U> Select<U>(Func<T, int, U> mapFunc)
        => new SelectIndexArray<T, U>(this, mapFunc);

    ISlice<T> Slice(int from, int count)
        => new Slice<T>(this, from, count);

    new ISlice<T> Take(int n)
        => Slice(0, n);

    int IndexOf(Func<T, bool> f)
    {
        for (var i = 0; i < Count; i++)
            if (f(this[i]))
                return i;
        return -1;
    }

    int IndexOfLast(Func<T, bool> f)
    {
        for (var i = Count; i > 0; --i)
            if (f(this[i-1]))
                return i;
        return -1;
    }

    int IndexOf(Func<T, int, bool> f)
    {
        for (var i = 0; i < Count; i++)
            if (f(this[i], i))
                return i;
        return -1;
    }

    int IndexOfLast(Func<T, int, bool> f)
    {
        for (var i = Count; i > 0; --i)
            if (f(this[i - 1], i))
                return i;
        return -1;
    }

    ISlice<T> TakeSliceWhile(Func<T, bool> predicate)
        => Take(IndexOf(predicate));

    ISlice<T> TakeSliceWhile(Func<T, int, bool> predicate)
        => Take(IndexOf(predicate));

    ISlice<T> Skip(int n)
        => Slice(n, Count - n);

    ISlice<T> SkipLast(int n)
        => Slice(0, Count - n);

    ISlice<T> SkipSliceWhile(Func<T, bool> predicate)
        => Skip(IndexOf(predicate));

    ISlice<T> SkipSliceWhile(Func<T, int, bool> predicate)
        => Skip(IndexOf(predicate));

    bool Any()
        => Count > 0;

    bool Any(Func<T, bool> predicate)
        => Generator.Any(predicate);

    bool All(Func<T, bool> predicate)
        => Generator.All(predicate);

    T? ValueOrDefault()
        => Count > 0 ? this[0] : default;

    T First()
        => this[0];

    T First(Func<T, bool> predicate)
        => this[IndexOf(predicate)];

    T? FirstOrDefault()
        => ValueOrDefault();

    T? ElementAtOrDefault(int index)
        => index >= 0 ? this[index] : default;

    T? FirstOrDefault(Func<T, bool> predicate)
        => ElementAtOrDefault(IndexOf(predicate));

    T Last()
        => this[Count - 1];

    T Last(Func<T, bool> predicate)
        => this[IndexOfLast(predicate)];

    T? LastOrDefault()
        => ElementAtOrDefault(Count - 1);

    T? LastOrDefault(Func<T, bool> predicate)
        => ElementAtOrDefault(IndexOfLast(predicate));

    int CountElements()
        => Count;

    IArray<T> Reverse()
        => new ReverseArray<T>(this);

    IArray<T> ToArray()
        => this;

    T ElementAt(int n)
        => this[n];

    bool Contains(T item)
        => IndexOf(x => x.Equals(item)) >= 0;

    IArray<T> Concat(IArray<T> other)
        => new ConcatArray<T>(this, other);
}

public partial record SelectArray<T, U>(IArray<T> Source, Func<T, U> Map)
    : IArray<U>
{
    public U this[int input] => Map(Source[input]);
    public int Count => Source.Count;
    public IGenerator<U> Generator => new ArrayGenerator<U>(this);
}

public partial record SelectIndexArray<T, U>(IArray<T> Source, Func<T, int, U> Map)
    : IArray<U>
{
    public U this[int input] => Map(Source[input], input);
    public int Count => Source.Count;
    public IGenerator<U> Generator => new ArrayGenerator<U>(this);
}

public partial record FunctionalArray<T>(int Count, Func<int, T> Map)
    : IArray<T>
{
    public IGenerator<T> Generator => new ArrayGenerator<T>(this);
    public T this[int n] => Map(n);
}

public partial record ReverseArray<T>(IArray<T> Source)
    : IArray<T>
{
    public T this[int input] => Source[Count - input];
    public int Count => Source.Count;
    public IGenerator<T> Generator => new ArrayGenerator<T>(this);
}

public partial record ConcatArray<T>(IArray<T> Source1, IArray<T> Source2)
    : IArray<T>
{
    public T this[int input] => input < Source1.Count ? Source1[input] : Source2[input - Source1.Count];
    public int Count => Source1.Count + Source2.Count;
    public IGenerator<T> Generator => new ArrayGenerator<T>(this);
}

public partial record ArrayAdapter<T>(IReadOnlyList<T> Source)
    : IArray<T>
{
    public T this[int input] => Source[input];
    public int Count => Source.Count;
    public IGenerator<T> Generator => new ArrayGenerator<T>(this);
}

// TODO: there could be a special ArrayWhere generator and probably more that I haven't thought of yet. 