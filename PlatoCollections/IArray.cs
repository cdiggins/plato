namespace Plato;

public static class ArrayExtensions
{
    public static IArray<T> Reverse<T>(this IArray<T> self)
        => self.Count.Select(i => self[self.Count - 1 - i]);

    public static IArray<ISlice<T>> Chunk<T>(this IArray<T> self, int size)
        => (size / self.Count).Select(i => self.Slice(i * size, size));

    public static IArray<U> SelectIndices<T, U>(this IArray<T> self, Func<int, U> func)
        => self.Count.Select(func);

    public static IArray<T> Choose<T>(this IArray<T> self, IArray<int> indices)
        => indices.Select(self.ElementAt);

    public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, U> mapFunc)
        => new SelectArray<T, U>(self, mapFunc);

    public static IArray<U> Select<T, U>(this IArray<T> self, Func<T, int, U> mapFunc)
        => new SelectIndexArray<T, U>(self, mapFunc);

    public static ISlice<T> Slice<T>(this IArray<T> self, int from, int count)
        => new Slice<T>(self, from, count);

    public static ISlice<T> Take<T>(this IArray<T> self, int n)
        => self.Slice(0, n);

    public static ISlice<T> TakeSliceWhile<T>(this IArray<T> self, Func<T, bool> predicate)
        => self.Take(self.IndexOf(predicate));

    public static ISlice<T> TakeSliceWhile<T>(this IArray<T> self, Func<T, int, bool> predicate)
        => self.Take(self.IndexOf(predicate));

    public static ISlice<T> Skip<T>(this IArray<T> self, int n)
        => self.Slice(n, self.Count - n);

    public static ISlice<T> SkipLast<T>(this IArray<T> self, int n)
        => self.Slice(0, self.Count - n);

    public static ISlice<T> SkipSliceWhile<T>(this IArray<T> self, Func<T, bool> predicate)
        => self.Skip(self.IndexOf(predicate));

    public static ISlice<T> SkipSliceWhile<T>(this IArray<T> self, Func<T, int, bool> predicate)
        => self.Skip(self.IndexOf(predicate));

    public static bool Any<T>(this IArray<T> self)
        => self.Count > 0;

    public static T? ValueOrDefault<T>(this IArray<T> self)
        => self.Count > 0 ? self[0] : default;

    public static T First<T>(this IArray<T> self)
        => self[0];

    public static T? FirstOrDefault<T>(this IArray<T> self)
        => self.Count > 0 ? self.First() : default;

    public static T? ElementAtOrDefault<T>(this IArray<T> self, int index)
        => index >= 0 ? self[index] : default;

    public static T? FirstOrDefault<T>(this IArray<T> self, Func<T, bool> predicate)
        => self.ElementAtOrDefault(self.IndexOf(predicate));

    public static T Last<T>(this IArray<T> self)
        => self[self.Count - 1];

    public static T Last<T>(this IArray<T> self, Func<T, bool> predicate)
        => self[self.IndexOfLast(predicate)];

    public static T? LastOrDefault<T>(this IArray<T> self)
        => self.ElementAtOrDefault(self.Count - 1);

    public static T? LastOrDefault<T>(this IArray<T> self, Func<T, bool> predicate)
        => self.ElementAtOrDefault(self.IndexOfLast(predicate));

    public static int Count<T>(this IArray<T> self)
        => self.Count;

    public static IArray<T> ToArray<T>(this IArray<T> self)
        => self;

    public static T ElementAt<T>(this IArray<T> self, int n)
        => self[n];

    public static IArray<T> Concat<T>(this IArray<T> self, IArray<T> other)
        => new ConcatArray<T>(self, other);
}

public readonly record struct SelectArray<T, U>(IArray<T> Source, Func<T, U> Map)
    : IArray<U>
{
    public U this[int input] => Map(Source[input]);
    public int Count => Source.Count;
    public IIterator<U> Iterator => new ArrayIterator<U>(this);
}

public readonly record struct SelectIndexArray<T, U>(IArray<T> Source, Func<T, int, U> Map)
    : IArray<U>
{
    public U this[int input] => Map(Source[input], input);
    public int Count => Source.Count;
    public IIterator<U> Iterator => new ArrayIterator<U>(this);
}

public readonly record struct  FunctionalArray<T>(int Count, Func<int, T> Map)
    : IArray<T>
{
    public IIterator<T> Iterator => new ArrayIterator<T>(this);
    public T this[int n] => Map(n);
}

public readonly record struct  ReverseArray<T>(IArray<T> Source)
    : IArray<T>
{
    public T this[int input] => Source[Count - input];
    public int Count => Source.Count;
    public IIterator<T> Iterator => new ArrayIterator<T>(this);
}

public readonly record struct  ConcatArray<T>(IArray<T> Source1, IArray<T> Source2)
    : IArray<T>
{
    public T this[int input] => input < Source1.Count ? Source1[input] : Source2[input - Source1.Count];
    public int Count => Source1.Count + Source2.Count;
    public IIterator<T> Iterator => new ArrayIterator<T>(this);
}

public readonly record struct  ArrayAdapter<T>(IReadOnlyList<T> Source)
    : IArray<T>
{
    public T this[int input] => Source[input];
    public int Count => Source.Count;
    public IIterator<T> Iterator => new ArrayIterator<T>(this);
}

// TODO: there could be a special ArrayWhere generator and probably more that I haven't thought of yet. 