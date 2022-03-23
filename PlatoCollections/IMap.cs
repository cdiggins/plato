﻿namespace Plato;

public readonly record struct EmptyMap<T1, T2> : IMap<T1, T2>
{
    public T2 this[T1 item] => throw new IndexOutOfRangeException();

    public static EmptyMap<T1, T2> Instance { get; } = new();
}

public readonly record struct Map<T1, T2>(Func<T1, T2> Function)
    : IMap<T1, T2>
{
    public T2 this[T1 item] => Function(item);
}

public static class MapExtensions 
{
    public static IMap<T1, T3> Select<T1, T2, T3>(this IMap<T1, T2> self, Func<T2, T3> func)
        => new Map<T1, T3>(x => func(self[x]));
}
