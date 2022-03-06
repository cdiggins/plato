namespace Plato;

public partial record Map<T1, T2>(Func<T1, T2> Function)
    : IMap<T1, T2>
{
    public T2 this[T1 item] => Function(item);
}

public partial interface IMap<T1, T2>
{
    IMap<T1, T3> Select<T3>(Func<T2, T3> func)
        => new Map<T1, T3>(x => func(this[x]));
}
