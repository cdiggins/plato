namespace Plato;

public partial record EmptySet<T>
    : ISet<T>
{ 
    public static EmptySet<T> Instance { get; } = new EmptySet<T>();
}

public partial record Set<T>(Func<T, bool> Predicate)
    : ISet<T>
{
    public bool Contains(T item) => Predicate(item);
}

public partial interface ISet<T>
{
    ISet<T> Union(ISet<T> other)
        => new Set<T>(x => Contains(x) || other.Contains(x));

    ISet<T> Intersection(ISet<T> other)
        => new Set<T>(x => Contains(x) && other.Contains(x));

    ISet<T> Difference(ISet<T> other)
        => new Set<T>(x => Contains(x) && !other.Contains(x));

    ISet<T> Complement()
        => new Set<T>(x => !Contains(x));

    ISet<T> SymmetricDifference(ISet<T> other)
        => new Set<T>(x => Contains(x) ^ other.Contains(x));
}

