namespace Plato;

/// <summary>
/// This is a specialized category of iterator, those that return instances of themselves
/// from Next. Any iterator that implements this interface can be optimized. 
/// </summary>
public interface IIterator<T, TSelf> : IIterator<T>
    where TSelf : IIterator<T>
{
    new TSelf Next { get; }
}

public readonly record struct Sequence<TValue>(IIterator<TValue> Iterator) 
    : ISequence<TValue>;

public readonly record struct Sequence<TValue, TIterator>(TIterator Iterator)
    : ISequence<TValue>
    where TIterator : IIterator<TValue, TIterator>
{
    IIterator<TValue> ISequence<TValue>.Iterator => Iterator;
}

public record WhereIterator<T> : IIterator<T>
{
    public WhereIterator(IIterator<T> source, Func<T, bool> predicate)
    {
        Source = source;
        Predicate = predicate;
        while (Source.HasValue && !predicate(Source.Value))
            Source = Source.Next;
    }

    public IIterator<T> Source { get; }
    public Func<T, bool> Predicate { get; }
    public IIterator<T> Next => new WhereIterator<T>(Source.Next, Predicate);
    public T Value => Source.Value;
    public bool HasValue => Source.HasValue;
}

public readonly record struct WhereIterator<T, TIterator> 
    : IIterator<T, WhereIterator<T, TIterator>>
    where TIterator : IIterator<T, TIterator>
{
    public WhereIterator(TIterator source, Func<T, bool> predicate)
    {
        Predicate = predicate;
        Source = source;
        while (Source.HasValue && !predicate(Source.Value))
            Source = Source.Next;
    }

    public TIterator Source {get; }
    public Func<T, bool> Predicate { get; }
    public WhereIterator<T, TIterator> Next => new(Source.Next, Predicate);
    public bool HasValue => Source.HasValue;
    public T Value => Source.Value;
    IIterator<T> IIterator<T>.Next => Next;
}


public readonly record struct WhereIndexIterator<T>
    : IIterator<T>
{
    public WhereIndexIterator(IIterator<T> source, Func<T, int, bool> predicate, int index = 0)
    {
        while (source.HasValue && !predicate(source.Value, index))
        {
            source = source.Next;
            index++;
        }
        Predicate = predicate;
        Source = source;
        Index = index;
    }

    public int Index { get; }
    public IIterator<T> Source { get; }
    public Func<T, int, bool> Predicate { get; }
    public IIterator<T> Next => new WhereIndexIterator<T>(Source.Next, Predicate, Index);
    public bool HasValue => Source.HasValue;
    public T Value => Source.Value;
}

public readonly record struct SelectIterator<T, U>(IIterator<T> Source, Func<T, U> Map)
    : IIterator<U>
{
    public IIterator<U> Next => this with { Source = Source.Next };
    public bool HasValue => Source.HasValue;
    public U Value => Map(Source.Value);
}

public readonly record struct SelectIndexIterator<T, U>(IIterator<T> Source, Func<T, int, U> Map, int Index = 0)
    : IIterator<U>
{
    public IIterator<U> Next => new SelectIndexIterator<T, U>(Source, Map, Index + 1);
    public bool HasValue => Source.HasValue;
    public U Value => Map(Source.Value, Index);
}

public readonly record struct EmptySequence<T> : IIterator<T>, ISet<T>, IArray<T>
{
    public static EmptySequence<T> Instance = new();
    public IIterator<T> Next => Instance;
    public bool HasValue => false;
    public IIterator<T> Iterator => this;
    public int Count => 0;
    public T Value => throw new IndexOutOfRangeException();
    public T this[int n] => Value;
    public bool Contains(T item) => false;
}

public readonly record struct SingleSequence<T>(T Value) : IIterator<T>, ISet<T>, IArray<T>
{
    public IIterator<T> Next => EmptySequence<T>.Instance;
    public bool HasValue => true;
    public IIterator<T> Iterator => this;
    public int Count => 1;
    public T this[int n] => Value;
    public bool Contains(T item) => item.Equals(Value);
}

public readonly record struct RepeatedSequence<T>(T Value, int Count) : IIterator<T>, ISet<T>, IArray<T>
{
    public IIterator<T> Next => Count > 0 ? new RepeatedSequence<T>(Value, Count - 1) : this;
    public bool HasValue => Count > 0;
    public IIterator<T> Iterator => this;
    public T this[int n] => Value;
    public bool Contains(T item) => item.Equals(Value);
}

public readonly record struct RangeSequence(int From, int Count) : IRange, IIterator<int>
{
    public int this[int input] => From + input;
    public IIterator<int> Iterator => this;
    public bool HasValue => Count > 0;
    public int Value => From;
    public int To => From + Count;
    public IIterator<int> Next => new RangeSequence(From + 1, Count - 1);
    public bool Contains(int item) => item >= From && item <= Count + From;
    public IComparer<int> Ordering => Orderings.IntegerOrder;
    public int FindKey(int n) => n < From || n >= Count ? -1 : n - From;
}

public readonly record struct TakeIterator<T>(IIterator<T> Source, Func<T, int, bool> Predicate, int Index = 0)
    : IIterator<T>
{
    public IIterator<T> Next => new TakeIterator<T>(Source.Next, Predicate, Index + 1);
    public bool HasValue => Source.HasValue && Predicate(Source.Value, Index);
    public T Value => Source.Value;
}

public readonly record struct ConcatIterator<T>(IIterator<T> Source1, IIterator<T> Source2)
    : IIterator<T>
{
    public IIterator<T> Next => Source1.Next.HasValue ? Source1.Next : Source2;
    public bool HasValue => Source1.HasValue;
    public T Value => Source1.Value;
}

public readonly record struct SelectManyIterator<T, U>
    : IIterator<U>
{
    public SelectManyIterator(IIterator<T> source, Func<T, ISequence<U>> selector)
        : this(source, selector, new EmptySequence<U>())
    { }

    public SelectManyIterator(IIterator<T> source, Func<T, ISequence<U>> selector, IIterator<U> current)
    {
        while (!current.HasValue && source.HasValue)
        {
            current = selector(source.Value).Iterator;
        }
        Selector = selector;
        Source = source;
        Current = current;
    }
    public IIterator<T> Source { get; }
    public IIterator<U> Current { get; }
    public Func<T, ISequence<U>> Selector { get; }
    public IIterator<U> Next => new SelectManyIterator<T, U>(Source, Selector, Current.Next);
    public U Value => Current.Value;
    public bool HasValue => Current.HasValue;
    public IIterator<U> Iterator => this;
}

public readonly record struct ArrayIterator<T>(IArray<T> Source, int Index = 0)
    : IArray<T>, IIterator<T>
{
    public IIterator<T> Next => this with { Index = Index + 1 };
    public bool HasValue => Index < Source.Count;
    public T Value => Source[Index];
    public int Count => Source.Count - Index;
    public T this[int index] => Source[index + Index];
    public IIterator<T> Iterator => this;
}

public readonly record struct IteratorWithIndex<T>(
    T Value,
    int Index,
    Func<T, int, bool> HasValueFunc,
    Func<T, int, (T, int)> NextFunc) : IIterator<T>
{
    public IIterator<T> Next
    {
        get
        {
            if (Value == null) return EmptySequence<T>.Instance;
            var (nextValue, nextIndex) = NextFunc(Value, Index);
            return this with { Value = nextValue, Index = nextIndex };
        }
    }

    public bool HasValue => Value != null && HasValueFunc(Value, Index);
    public IIterator<T> Iterator => this;
}

public readonly record struct ChunkIterator<T>(IIterator<T> Value, int Size)
    : IIterator<IIterator<T>>
{
    public IIterator<IIterator<T>> Next => this with { Value = Value.Skip(Size) };
    public bool HasValue => Value.HasValue;
    public IIterator<IIterator<T>> Iterator => this;
}