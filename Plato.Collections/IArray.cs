
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plato
{
    [DebuggerTypeProxy(typeof(DebuggerArray<>))]
    public readonly struct SelectArray<T, U>
        : IArray<U>
    {
        public SelectArray(IArray<T> source, Func<T, U> map) => (Source, Map) = (source, map);
        public IArray<T> Source { get; }
        public Func<T, U> Map { get; }
        public U this[int input] => Map(Source[input]);
        public int Count => Source.Count;
        public IIterator<U> Iterator => new ArrayIterator<U>(this);
    }

    [DebuggerTypeProxy(typeof(DebuggerArray<>))]
    public readonly struct SelectIndexArray<T, U>
        : IArray<U>
    {
        public SelectIndexArray(IArray<T> source, Func<T, int, U> map)
            => (Source, Map) = (source, map);
        public IArray<T> Source { get; }
        public Func<T, int, U> Map { get; }
        public U this[int input] => Map(Source[input], input);
        public int Count => Source.Count;
        public IIterator<U> Iterator => new ArrayIterator<U>(this);
    }

    [DebuggerTypeProxy(typeof(DebuggerArray<>))]
    public readonly struct FunctionalArray<T>
        : IArray<T>
    {
        public FunctionalArray(int count, Func<int, T> map)
            => (Count, Map) = (count, map);
        public int Count { get; }
        public Func<int, T> Map {get;}
        public IIterator<T> Iterator => new ArrayIterator<T>(this);
        public T this[int n] => Map(n);
    }

    [DebuggerTypeProxy(typeof(DebuggerArray<>))]
    public readonly struct ReverseArray<T>
        : IArray<T>
    {
        public ReverseArray(IArray<T> source)
            => Source = source;
        public IArray<T> Source { get; }
        public T this[int input] => Source[Count - input];
        public int Count => Source.Count;
        public IIterator<T> Iterator => new ArrayIterator<T>(this);
    }

    [DebuggerTypeProxy(typeof(DebuggerArray<>))]
    public readonly struct ConcatArray<T>
        : IArray<T>
    {
        public ConcatArray(IArray<T> source1, IArray<T> source2)
            => (Source1, Source2) = (source1, source2);
        public IArray<T> Source1 { get; }
        public IArray<T> Source2 { get; }
        public T this[int input] => input < Source1.Count ? Source1[input] : Source2[input - Source1.Count];
        public int Count => Source1.Count + Source2.Count;
        public IIterator<T> Iterator => new ArrayIterator<T>(this);
    }

    [DebuggerTypeProxy(typeof(DebuggerArray<>))]
    public readonly struct ArrayAdapter<T>
        : IArray<T>
    {
        public ArrayAdapter(IReadOnlyList<T> source)
            => Source = source;
        public IReadOnlyList<T> Source { get; }
        public T this[int input] => Source[input];
        public int Count => Source.Count;
        public IIterator<T> Iterator => new ArrayIterator<T>(this);
    }

}
