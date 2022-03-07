namespace Plato;

/*
public static class Generator
{
    public static IGenerator<U> Select<T, U>(this IGenerator<T> self, Func<T, U> mapFunc)
        => self.Select(mapFunc);

    public static IGenerator<T> Take<T>(this IGenerator<T> self, int n)
        => self.Take(n);
}
*/

public partial interface IGenerator<T>
{
    IArray<T> ToArray()
    {
        var r = new List<T>();
        var iter = this;
        while (iter.HasValue)
        {
            r.Add(iter.Value);
            iter = iter.Next;
        }
        return new ArrayAdapter<T>(r);
    }

    IGenerator<IGenerator<T>> Chunk(int size)
        => new ChunkGenerator<T>(this, size);

    IGenerator<T> Where(Func<T, bool> predicateFunc)
        => new WhereGenerator<T>(this, predicateFunc);

    IGenerator<T> Where(Func<T, int, bool> predicateFunc)
        => new WhereIndexGenerator<T>(this, predicateFunc, 0);

    IGenerator<U> Select<U>(Func<T, U> mapFunc)
        => new SelectGenerator<T, U>(this, mapFunc);

    IGenerator<U> Select<U>(Func<T, int, U> mapFunc)
        => new SelectIndexGenerator<T, U>(this, mapFunc);

    new IGenerator<T> Take(int n) 
        => TakeWhile((_, i) => i < n);

    IGenerator<T> TakeWhile(Func<T, bool> predicate)
        => TakeWhile((x, _) => predicate(x));

    IGenerator<T> TakeWhile(Func<T, int, bool> predicate)
        => new TakeGenerator<T>(this, predicate);

    IGenerator<T> Skip(int n)
    {
        var r = this;
        for (var i = 0; i < n; i++)
            r = r.Next;
        return r;
    }

    IGenerator<T> SkipLast(int n)
        => Take(CountElements() - n);

    IGenerator<T> SkipWhile(Func<T, bool> predicate)
        => AdvanceWhile(g => predicate(g.Value));
    
    IGenerator<T> AdvanceWhile(Func<IGenerator<T>, bool> predicate)
    {
        var r = this;
        while (predicate(r))
            r = r.Next;
        return r;
    }

    IGenerator<T> AdvanceWhile(Func<IGenerator<T>, int, bool> predicate)
    {
        var r = this;
        var i = 0; 
        while (predicate(r, i++))
            r = r.Next; 
        return r;
    }

    IGenerator<T> SkipWhile(Func<T, int, bool> predicate)
        => AdvanceWhile((g, index) => predicate(g.Value, index));
   
    bool Any() 
        => HasValue;

    bool Any(Func<T, bool> predicate)
        => SkipWhile(predicate).HasValue;
    
    bool All(Func<T, bool> predicate) 
        => SkipWhile(predicate).HasValue;

    T? ValueOrDefault()
        => HasValue ? Value : default;

    T First()
        => Value;

    T First(Func<T, bool> predicate)
        => SkipWhile(predicate).Value;

    T? FirstOrDefault()
        => ValueOrDefault();

    T? FirstOrDefault(Func<T, bool> predicate)
        => SkipWhile(predicate).FirstOrDefault();
    
    T Last() 
        => AdvanceWhile(g => g.Next.HasValue).Value;

    T Last(Func<T, bool> predicate)
        => Where(predicate).Last();

    T? LastOrDefault()
        => AdvanceWhile(g => g.Next.HasValue).ValueOrDefault();

    T? LastOrDefault(Func<T, bool> predicate)
        => Where(predicate).LastOrDefault();

    int CountElements()
        => Aggregate(0, (i, _) => i + 1);

    int CountElements(Func<T, bool> predicate)
        => Aggregate(0, (i, x) => predicate(x) ? i + 1 : i);

    IArray<T> Reverse()
        => ToArray().Reverse();

    IGenerator<T> Prepend(T x)
        => x.Unit().Concat(this);

    IGenerator<T> Append(T x)
        => Concat(x.Unit());

    T ElementAt(int n)
        => Skip(n).First();
    
    TAccumulate Aggregate<TAccumulate>(TAccumulate init, Func<TAccumulate, T, TAccumulate> func)
    {
        var tmp = this;
        while (tmp.HasValue)
        {
            init = func(init, tmp.Value);
            tmp = tmp.Next;
        }
        return init;
    }        

    bool Contains(T item)
        => Aggregate(false, (_, x) => _ || x.Equals(item));
    
    IGenerator<T> Concat(IGenerator<T> other) 
        => new ConcatGenerator<T>(this, other);

    IGenerator<T> Concat(ISequence<T> other)
        => Concat(other.Generator);

    IGenerator<U> SelectMany<U>(Func<T, ISequence<U>> func)
        => new SelectManyGenerator<T, U>(this, func);

    IGenerator<T> Union(ISequence<T> other)
        => throw new NotImplementedException();

    IGenerator<T> Except(ISequence<T> other)
        => throw new NotImplementedException();
}

public partial record WhereGenerator<T> : IGenerator<T>
{
    public WhereGenerator(IGenerator<T> source, Func<T, bool> predicate)
    {
        Source = source.SkipWhile(predicate);
        Predicate = predicate;
    }

    public IGenerator<T> Source { get; }
    public Func<T, bool> Predicate { get; }
    public IGenerator<T> Next => new WhereGenerator<T>(Source, Predicate);
    public bool HasValue => Source.HasValue;
    public T Value => Source.Value;
    public IGenerator<T> Generator => this;
}

public partial record WhereIndexGenerator<T> 
    : IGenerator<T>
{
    public WhereIndexGenerator(IGenerator<T> source, Func<T, int, bool> predicate, int index = 0)
    {
        while (!predicate(source.Value, index))
        {
            source = source.Next;
            index++;
        }
        Predicate = predicate;
        Source = source;
        Index = index;
    }

    public int Index; 
    public IGenerator<T> Source { get; }
    public Func<T, int, bool> Predicate { get; }
    public IGenerator<T> Next => new WhereIndexGenerator<T>(Source, Predicate, Index);
    public bool HasValue => Source.HasValue;
    public T Value => Source.Value;
    public IGenerator<T> Generator => this;
}

public partial record SelectGenerator<T, U>(IGenerator<T> Source, Func<T, U> Map) 
    : IGenerator<U>
{
    public IGenerator<U> Next => this with { Source = Source.Next };
    public bool HasValue => Source.HasValue;
    public U Value => Map(Source.Value);
    public IGenerator<U> Generator => this;
}

public partial record SelectIndexGenerator<T, U>(IGenerator<T> Source, Func<T, int, U> Map, int Index = 0)
    : IGenerator<U>
{
    public IGenerator<U> Next => this with { Source = Source.Next, Index = Index + 1 };
    public bool HasValue => Source.HasValue;
    public U Value => Map(Source.Value, Index);
    public IGenerator<U> Generator => this;
}

public partial record UnitGenerator<T>(T Value) 
    : IGenerator<T>, IArray<T>, ISet<T>
{
    public IGenerator<T> Next => Generator.Empty();
    public bool HasValue => true;
    public IGenerator<T> Generator => this;
    public int Count => 1;
    public T this[int n] => Value;
    public bool Contains(T item) => item.Equals(Value);
}

public partial record EmptyGenerator<T>
    : IGenerator<T>, IArray<T>, ISet<T>
{
    public IGenerator<T> Next => this;
    public bool HasValue => false;
    public IGenerator<T> Generator => this;
    public int Count => 0;
    public T Value => throw new NotSupportedException();
    public T this[int n] => throw new NotSupportedException();
    public bool Contains(T item) => false;
    public static readonly EmptyGenerator<T> Instance = new();
}

public partial record RepeatGenerator<T>(T Value, int Count)
    : IGenerator<T>, IArray<T>, ISet<T>
{
    public IGenerator<T> Next => Count > 0 ? new RepeatGenerator<T>(Value, Count - 1) : this;
    public bool HasValue => Count > 0;
    public IGenerator<T> Generator => this;
    public T this[int n] => Value;
    public bool Contains(T item) => item.Equals(Value);
}

public interface IRangeGenerator
    : IArray<int>, ISet<int>
{
    int From { get; }
    int To { get; }
}

public partial record RangeGenerator(int From, int To)
    : IRangeGenerator
{
    public IGenerator<int> Next => new RangeGenerator(From + 1, To);
    public int Count => To - From;
    public bool HasValue => From < To;
    public IGenerator<int> Generator => this;
    public int this[int n] => From + n;
    public int Value => From;
    public bool Contains(int item) => item >= From && item < To;
}

public partial record TakeGenerator<T>(IGenerator<T> Source, Func<T, int, bool> Predicate, int Index = 0)
    : IGenerator<T>
{
    public IGenerator<T> Next => this with { Source = Source.Next, Index = Index + 1 };
    public bool HasValue => Source.HasValue && Predicate(Source.Value, Index);
    public T Value => Source.Value;
    public IGenerator<T> Generator => this;
}

public partial record ConcatGenerator<T>(IGenerator<T> Source1, IGenerator<T> Source2) 
    : IGenerator<T>
{
    public IGenerator<T> Next => Source1.HasValue ? Source1.Next : Source2;
    public bool HasValue => Source1.HasValue;
    public T Value => Source1.Value;
    public IGenerator<T> Generator => this;
}

public partial record SelectManyGenerator<T, U>
    : IGenerator<U>
{
    public SelectManyGenerator(IGenerator<T> source, Func<T, ISequence<U>> selector)
        : this(source, selector, new EmptyGenerator<U>())
    { }

    public SelectManyGenerator(IGenerator<T> source, Func<T, ISequence<U>> selector, IGenerator<U> current)
    {
        while (!current.HasValue && source.HasValue)
        {
            current = selector(source.Value).Generator;
        }
        Selector = selector;
        Source = source;
        Current = current;
    }
    public IGenerator<T> Source { get; }
    public IGenerator<U> Current { get; }
    public Func<T, ISequence<U>> Selector { get; }
    public IGenerator<U> Next => new SelectManyGenerator<T, U>(Source, Selector, Current.Next);
    public U Value => Current.Value;
    public bool HasValue => Current.HasValue;
    public IGenerator<U> Generator => this;
}

public partial record FunctionalGenerator<T>(
    T Value, 
    Func<T, bool> HasValueFunc, 
    Func<T, T> NextFunc) : IGenerator<T>
{
    public IGenerator<T> Next => Value == null ? new EmptyGenerator<T>() : this with { Value = NextFunc(Value) };
    public bool HasValue => Value != null && HasValueFunc(Value);
    public IGenerator<T> Generator => this;
}

public partial record ArrayGenerator<T>(IArray<T> Source, int Index = 0)
    : IArray<T>, IGenerator<T>
{
    public IGenerator<T> Next => this with { Index = Index + 1 };
    public bool HasValue => Index < Source.Count;
    public T Value => Source[Index];
    public int Count => Source.Count - Index;
    public T this[int index] => Source[index + Index];
    public IGenerator<T> Generator => this;
}

public record GeneratorWithIndex<T>(
    T Value,
    int Index,
    Func<T, int, bool> HasValueFunc,
    Func<T, int, (T, int)> NextFunc) : IGenerator<T>
{
    public IGenerator<T> Next
    {
        get
        {
            if (Value == null) return EmptyGenerator<T>.Instance;
            var (nextValue, nextIndex) = NextFunc(Value, Index);
            return this with { Value = nextValue, Index = nextIndex };
        }
    }

    public bool HasValue => Value != null && HasValueFunc(Value, Index);
    public IGenerator<T> Generator => this;
}

public record ChunkGenerator<T>(IGenerator<T> Value, int Size)
    : IGenerator<IGenerator<T>>
{
    public IGenerator<IGenerator<T>> Next => this with { Value = Value.Skip(Size) };
    public bool HasValue => Value.HasValue;
    public IGenerator<IGenerator<T>> Generator => this;
}
