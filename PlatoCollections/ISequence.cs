namespace Plato;

public partial interface ISequence<T>
{
    IGenerator<IGenerator<T>> Chunk(int size)
        => Generator.Chunk(size);   

    IGenerator<T> Where(Func<T, bool> predicateFunc)
        => Generator.Where(predicateFunc);

    IGenerator<T> Where(Func<T, int, bool> predicateFunc)
        => Generator.Where(predicateFunc);

    IGenerator<U> Select<U>(Func<T, U> mapFunc)
        => Generator.Select(mapFunc);

    IGenerator<U> Select<U>(Func<T, int, U> mapFunc)
        => Generator.Select(mapFunc);

    IGenerator<T> Take(int n)
        => Generator.Take(n);

    IGenerator<T> TakeWhile(Func<T, bool> predicate)
        => Generator.TakeWhile(predicate);

    IGenerator<T> TakeWhile(Func<T, int, bool> predicate)
        => Generator.TakeWhile(predicate);

    IGenerator<T> Skip(int n)
        => Generator.Skip(n);

    IArray<T> SkipLast(int n)
        => Generator.SkipLast(n);

    IGenerator<T> SkipWhile(Func<T, bool> predicate)
        => Generator.SkipWhile(predicate);

    IGenerator<T> SkipWhile(Func<T, int, bool> predicate)
        => Generator.SkipWhile(predicate);

    bool Any()
        => Generator.Any();

    bool Any(Func<T, bool> predicate)
        => Generator.Any(predicate);

    bool All(Func<T, bool> predicate)
        => Generator.All(predicate);

    T? ValueOrDefault()
        => Generator.ValueOrDefault();  

    T First()
        => Generator.First();

    T First(Func<T, bool> predicate)
        => Generator.First(predicate);

    T? FirstOrDefault()
        => Generator.FirstOrDefault();

    T? FirstOrDefault(Func<T, bool> predicate)
        => Generator.FirstOrDefault();

    T Last()
        => Generator.Last();

    T Last(Func<T, bool> predicate)
        => Generator.Last();

    int CountElements()
        => Generator.CountElements();

    int CountElements(Func<T, bool> predicate)
        => Generator.CountElements(predicate);

    T? LastOrDefault()
        => Generator.LastOrDefault();

    T? LastOrDefault(Func<T, bool> predicate)
        => Generator.LastOrDefault(predicate);

    IArray<T> Reverse()
        => Generator.Reverse();

    IGenerator<T> Prepend(T x)
        => Generator.Prepend(x);

    IGenerator<T> Append(T x)
        => Generator.Append(x);

    IArray<T> ToArray()
        => Generator.ToArray();

    T ElementAt(int n)
        => Generator.ElementAt(n);

    TAccumulate Aggregate<TAccumulate>(TAccumulate init, Func<TAccumulate, T, TAccumulate> func)
        => Generator.Aggregate(init, func);

    T Aggregate(Func<T, T, T> func)
        => Generator.Aggregate(func);

    bool Contains(T item)
        => Generator.Contains(item);

    IGenerator<T> Concat(IGenerator<T> other)
        => Generator.Concat(other);

    IGenerator<T> Concat(ISequence<T> other)
        => Generator.Concat(other);

    IGenerator<U> SelectMany<U>(Func<T, ISequence<U>> func)
        => Generator.SelectMany(func);

    IGenerator<U> SelectMany<U>(Func<T, int, ISequence<U>> func)
        => Generator.SelectMany(func);

    IGenerator<T> Union(ISequence<T> other)
        => Generator.Union(other);

    IGenerator<T> Except(ISequence<T> other)
        => Generator.Except(other); 
}

