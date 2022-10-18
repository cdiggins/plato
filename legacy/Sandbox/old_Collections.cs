using System;

namespace Plato
{
    /// <summary>
    /// Values of a unique type can't be shared. 
    /// There can only ever be one reference to it. 
    /// This is enforced by the compiler. 
    /// </summary>
    public class UniqueType : Attribute
    { }

    public interface IEnumerator<T>
    {
        IEnumerator<T> Next();
        T Value { get; }
        bool HasValue { get; }
    }

    public interface IEnumerable<T>
    {
        IEnumerator<T> GetEnumerator();
    }

    public interface ICountable
    {
        long Count { get; }
    }

    public interface IMapping<TSrc, TDest>
    {
        TDest Apply(TSrc src);
    }

    public interface IBidirectionalMapping<TSrc, TDest> : IMapping<TSrc, TDest>
    {
        IBidirectionalMapping<TDest, TSrc> Invert();
    }

    [UniqueType]
    public interface IInputStream<T>
    {
        (IInputStream<T>, T) Read();
        bool HasValue { get; }
    }

    [UniqueType]
    public interface IOutputStream<T>
    {
        IOutputStream<T> Write(T x);
        bool Close();
    }

    [UniqueType]
    public interface ICancelToken
    {
        bool IsCanceled { get; }
    }

    [UniqueType]
    public interface ICancelable
    {
        bool IsCanceled { get; }
        void Cancel();
        ICancelToken CancelToken();
    }

    public interface IProgress
    {
        float Progress { get; }
    }

    [UniqueType]
    public interface IInputOuputStream<T> : IInputStream<T>, IOutputStream<T>
    { }

    public interface ISet<T> : IMapping<T, bool>
    {
        bool Contains(T value);
    }

    public interface IOrderedSet<T> : ISet<T>, IArray<T>
    {
    }

    public interface IArray<T> : ICountable, IMapping<int, T>
    {
        T this[long n] { get; }
    }

    public interface IList<T>
    {
        bool IsEmpty { get; }
        T Head { get; }
        IList<T> Tail { get; }
    }

    [UniqueType]
    public interface IArrayBuilder<T> : IArray<T>
    {
        IArrayBuilder<T> SetCount(long n);
        IArrayBuilder<T> SetValue(long n, T x);
    }

    [UniqueType]
    public interface IListBuilder<T> : IList<T>
    {
        IListBuilder<T> Add(T x);
    }

    [UniqueType]
    public interface IOrderedSetBuilder<T>
    {
        IOrderedSetBuilder<T> Add(T x);
        IOrderedSet<T> ToOrderedSet();
    }

    [UniqueType]
    public interface ISetBuilder<T> 
    {
        ISetBuilder<T> Add(T x);
        ISet<T> ToSet();
    }

    // TODO: consider builders that permit removing items, and builder that permit inspection during building

    public interface ILookup<TKey, TValue> : IArray<(TKey, TValue)>, IMapping<TKey, TValue>
    {
        TValue this[TKey key] { get; }
        bool HasKey(TKey key);
    }

    // Stack, Queue, Deques can all be trivially adapted as Streams

    public interface IStack<T> : ICountable
    {
        IStack<T> Push(T x);
        IStack<T> Pop();
        T Peek();
    }

    public interface IQueue<T> : ICountable
    {
        IQueue<T> Enqueue(T x);
        IQueue<T> Dequeue();
        T Peek();
    }

    public interface IDequeue<T> : ICountable
    {
        IDequeue<T> PushFront(T x);
        IDequeue<T> PushBack(T x);
        (IDequeue<T>, T) PopFront();
        (IDequeue<T>, T) PopBack();
        T PeekFront();
        T PeekBack();
    }

    public interface IPrioritySet<T> : ISet<T>, IMapping<T, int>
    {
        int GetPriority(T x);
        T Max();
        T Min();
    }

    public interface IOptional<T>
    {
        bool HasValue { get; }
        T Value { get; }
    }

    public interface IPriorityQueue<T> : ICountable
    {
        IPriorityQueue<T> Enqueue(T x, long priority);
        (IPriorityQueue<T>, T) DequeueMax();
        (IPriorityQueue<T>, T) DequeueMin();
        T PeekMax();
        T PeekMin();
    }

    public interface ITree<T>
    {
        IOptional<ITree<T>> Left { get; }
        IOptional<ITree<T>> Right { get; }
    }

    public interface IStackArray<T> : IStack<T>, IArray<T>
    { }

    public class Set<T> : Map<T, bool>, ISet<T>
    {
        public Set(Func<T, bool> f) 
            : base(f) 
        { }
        public bool Contains(T x) => Apply(x);
        public static ISet<T> Create(Func<T, bool> f) => new Set<T>(f);
    }

    public class Map<TSrc, TDest> : IMapping<TSrc, TDest>
    {
        public Map(Func<TSrc, TDest> f) 
            => Function = f;
        public Func<TSrc, TDest> Function { get; }
        public TDest Apply(TSrc src) => Function(src);
    }

    public class Enumerator<TValue, TState> : IEnumerator<TValue>
    {
        public Enumerator(TState state, Func<TState, TValue> valueFunc, Func<TState, TState> stateFunc, Func<TState, bool> hasValueFunc)
            => (State, ValueFunc, StateFunc, HasValueFunc) = (state, valueFunc, stateFunc, hasValueFunc);
        public TState State { get; }
        public Func<TState, TValue> ValueFunc { get; }
        public Func<TState, TState> StateFunc { get; }
        public Func<TState, bool> HasValueFunc { get; }
        public TValue Value => ValueFunc(State);
        public bool HasValue => HasValueFunc(State);
        public IEnumerator<TValue> Next() => new Enumerator<TValue, TState>(StateFunc(State), ValueFunc, StateFunc, HasValueFunc);
    }

    public class Enumerable<T> : IEnumerable<T>
    {
        public Enumerable(IEnumerator<T> enumerator)
            => Enumerator = enumerator;
        public IEnumerator<T> Enumerator { get; }
        public IEnumerator<T> GetEnumerator() => Enumerator;
    }

    public static class Enumerable
    {
        public static IEnumerable<T> Create<T>(IEnumerator<T> enumerator)
            => new Enumerable<T>(enumerator);         
    }

    public class Lookup<TKey, TValue> : ILookup<TKey, TValue>
    {
        public Lookup(IArray<(TKey, TValue)> keyValuePairs, Func<TKey, int> keyToIndexFunc, Func<TKey, bool> keyPresentFunc)
            => (KeyValuePairs, KeyToIndexFunc, KeyPresentFunc) = (keyValuePairs, keyToIndexFunc, keyPresentFunc);
        public IArray<(TKey, TValue)> KeyValuePairs { get; }
        public Func<TKey, int> KeyToIndexFunc { get; }
        public Func<TKey, bool> KeyPresentFunc { get; }
        public TValue this[TKey key] => KeyValuePairs[KeyToIndexFunc(key)].Item2;
        public (TKey, TValue) this[long n] => KeyValuePairs[n];
        public long Count => KeyValuePairs.Count;
        public (TKey, TValue) Apply(int src) => this[src];
        public TValue Apply(TKey src) => this[src];
        public bool HasKey(TKey key) => KeyPresentFunc(key);
    }

    public class Stack<T> : IStack<T>
    {
        public Stack(Stack<T> prev, T top)
            => (Prev, Top, Count) = (prev, top, prev.Count + 1);
        public Stack<T> Prev { get; }
        public T Top { get; }
        public long Count { get; private set; }
        public T Peek() => Top;
        public IStack<T> Pop() => Prev;
        public IStack<T> Push(T x) => new Stack<T>(this, x);
    }

    public static class SetExtensions
    {
        public static ISet<T> Intersection<T>(this ISet<T> self, ISet<T> other)
            => Set<T>.Create(x => self.Contains(x) && other.Contains(x));

        public static ISet<T> Union<T>(this ISet<T> self, ISet<T> other)
            => Set<T>.Create(x => self.Contains(x) || other.Contains(x));

        public static ISet<T> Difference<T>(this ISet<T> self, ISet<T> other)
            => Set<T>.Create(x => self.Contains(x) ^ other.Contains(x));

        public static ISet<T> Complement<T>(this ISet<T> self)
            => Set<T>.Create(x => !self.Contains(x));       
    }
}
