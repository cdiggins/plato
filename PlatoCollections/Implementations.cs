using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Plato;

namespace Plato
{
    /// <summary>
    /// This is a specialized category of iterator, those that return instances of themselves
    /// from Next. Any iterator that implements this interface can be optimized. 
    /// </summary>
    public interface IIterator<T, TSelf> : IIterator<T>
        where TSelf : IIterator<T>
    {
        new TSelf Next { get; }
    }

    public readonly struct Sequence<TValue>
        : ISequence<TValue>
    {
        public IIterator<TValue> Iterator { get; }
        public Sequence(IIterator<TValue> iterator) => Iterator = iterator;
    }

    public readonly struct Sequence<TValue, TIterator>
        : ISequence<TValue>
        where TIterator : IIterator<TValue, TIterator>
    {
        public Sequence(TIterator iterator) => Iterator = iterator;
        public TIterator Iterator { get; }
        IIterator<TValue> ISequence<TValue>.Iterator => Iterator;
    }


    public readonly struct WhereIterator<T> : IIterator<T>
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

    public readonly struct WhereIterator<T, TIterator>
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

        public TIterator Source { get; }
        public Func<T, bool> Predicate { get; }
        public WhereIterator<T, TIterator> Next => new WhereIterator<T, TIterator>(Source.Next, Predicate);
        public bool HasValue => Source.HasValue;
        public T Value => Source.Value;
        IIterator<T> IIterator<T>.Next => Next;
    }

    public readonly struct WhereIndexIterator<T>
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

    public readonly struct SelectIterator<T, U>
        : IIterator<U>
    {
        public SelectIterator(IIterator<T> source, Func<T, U> map)
            => (Source, Map) = (source, map);

        public IIterator<T> Source { get; }
        public Func<T, U> Map { get; }
        public IIterator<U> Next => new SelectIterator<T, U>(Source.Next, Map);
        public bool HasValue => Source.HasValue;
        public U Value => Map(Source.Value);
    }

    public readonly struct SelectIndexIterator<T, U>
        : IIterator<U>
    {
        public SelectIndexIterator(IIterator<T> source, Func<T, int, U> map, int index = 0)
            => (Source, Map, Index) = (source, map, index);

        public IIterator<T> Source { get; }
        public Func<T, int, U> Map { get; }
        public int Index { get; }
        public IIterator<U> Next => new SelectIndexIterator<T, U>(Source, Map, Index + 1);
        public bool HasValue => Source.HasValue;
        public U Value => Map(Source.Value, Index);
    }

    [DebuggerDisplay("[]")]
    public readonly struct EmptySequence<T> : IIterator<T>, ISet<T>, IArray<T>
    {
        public static EmptySequence<T> Instance = new EmptySequence<T>();
        public IIterator<T> Next => Instance;
        public bool HasValue => false;
        public IIterator<T> Iterator => this;
        public int Count => 0;
        public T Value => throw new IndexOutOfRangeException();
        public T this[int n] => Value;
        public bool Contains(T item) => false;
    }

    [DebuggerDisplay("[{Value}]")]
    public readonly struct SingleSequence<T> : IIterator<T>, ISet<T>, IArray<T>
    {
        public SingleSequence(T value) => Value = value;
        public T Value { get; }
        public IIterator<T> Next => EmptySequence<T>.Instance;
        public bool HasValue => true;
        public IIterator<T> Iterator => this;
        public int Count => 1;
        public T this[int n] => Value;
        public bool Contains(T item) => item?.Equals(Value) ?? false;
    }

    [DebuggerDisplay("[{Value} * {Count}]")]
    public readonly struct RepeatedSequence<T> : IIterator<T>, ISet<T>, IArray<T>
    {
        public RepeatedSequence(T value, int count) => (Value, Count) = (value, count);
        public T Value { get; }
        public int Count { get; }
        public IIterator<T> Next => Count > 0 ? new RepeatedSequence<T>(Value, Count - 1) : this;
        public bool HasValue => Count > 0;
        public IIterator<T> Iterator => this;
        public T this[int n] => Value;
        public bool Contains(T item) => item?.Equals(Value) ?? false;
    }

    [DebuggerDisplay("[{From} .. {To})")]
    public readonly struct RangeSequence : IRange, IIterator<int>
    {
        public RangeSequence(int from, int count) => (From, Count) = (from, count);
        public int From { get; }
        public int Count { get; }
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

    public readonly struct TakeIterator<T>
        : IIterator<T>
    {
        public TakeIterator(IIterator<T> source, Func<T, int, bool> predicate, int index = 0) =>
            (Source, Predicate, Index) = (source, predicate, index);

        public IIterator<T> Source { get; }
        public Func<T, int, bool> Predicate { get; }
        public int Index { get; }
        public IIterator<T> Next => new TakeIterator<T>(Source.Next, Predicate, Index + 1);
        public bool HasValue => Source.HasValue && Predicate(Source.Value, Index);
        public T Value => Source.Value;
    }

    public readonly struct ConcatIterator<T>
        : IIterator<T>
    {
        public ConcatIterator(IIterator<T> source1, IIterator<T> source2) => (Source1, Source2) = (source1, source2);
        public IIterator<T> Source1 { get; }
        public IIterator<T> Source2 { get; }
        public IIterator<T> Next => Source1.Next.HasValue ? Source1.Next : Source2;
        public bool HasValue => Source1.HasValue;
        public T Value => Source1.Value;
    }

    public readonly struct SelectManyIterator<T, U>
        : IIterator<U>
    {
        public SelectManyIterator(IIterator<T> source, Func<T, ISequence<U>> selector)
            : this(source, selector, new EmptySequence<U>())
        {
        }

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

    public readonly struct ArrayIterator<T>
        : IArray<T>, IIterator<T>
    {
        public ArrayIterator(IArray<T> source, int index = 0)
            => (Source, Index) = (source, index);

        public IArray<T> Source { get; }
        public int Index { get; }
        public IIterator<T> Next => new ArrayIterator<T>(Source, Index + 1);
        public bool HasValue => Index < Source.Count;
        public T Value => Source[Index];
        public int Count => Source.Count - Index;
        public T this[int index] => Source[index + Index];
        public IIterator<T> Iterator => this;
    }

    public readonly struct Set<T>
        : ISet<T>
    {
        public Set(Func<T, bool> predicate) => Predicate = predicate;
        public Func<T, bool> Predicate { get; }
        public bool Contains(T item) => Predicate(item);
    }

    public readonly struct Slice<T> : ISlice<T>
    {
        public Slice(IArray<T> array, int index, int count)
            => (Array, Index, Count) = (array, index, count);
        public IArray<T> Array { get; }
        public int Index { get; }
        public int Count { get; }
        public T this[int input] => Array[input + Index];
        public IIterator<T> Iterator => new ArrayIterator<T>(this);
        T IMap<int, T>.this[int index] => Array[Index + index];
    }

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
        public Func<int, T> Map { get; }
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

    public readonly struct EmptyMap<T1, T2> : IMap<T1, T2>
    {
        public T2 this[T1 item] => throw new IndexOutOfRangeException();

        public static EmptyMap<T1, T2> Instance = new EmptyMap<T1, T2>();
    }

    public readonly struct Map<T1, T2>
        : IMap<T1, T2>
    {
        public Map(Func<T1, T2> func) => Function = func;
        private Func<T1, T2> Function { get; }
        public T2 this[T1 item] => Function(item);
    }
    public readonly struct Comparer<T> : IComparer<T>
    {
        public Comparer(Func<T, T, int> func) => Func = func;
        public Func<T, T, int> Func { get; }
        public int Compare(T x, T y)
            => x == null || y == null ? 0 : Func(x, y);
    }

    public static class Orderings
    {
        public static IComparer<int> IntegerOrder { get; }
            = new Comparer<int>((a, b) => b - a);
    }

    public class NoOrder<T> : IComparer<T>
    {
        public int Compare(T x, T y) => 0;
        public static NoOrder<T> Instance { get; } = new NoOrder<T>();
    }

}

/*
public readonly struct IteratorWithIndex<T> : IIterator<T>
{
    public IteratorWithIndex(T value, int index, )
    public T Value { get;}
    public int Index { get;}
    public Func<T, int, bool> HasValueFunc { get;}
    public Func<T, int, (T, int)> NextFunc { get; }

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
*/
