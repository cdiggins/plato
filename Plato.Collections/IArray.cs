namespace Plato;


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