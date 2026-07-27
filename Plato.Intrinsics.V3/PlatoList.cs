using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The handwritten implementation of the affine builder type <c>unique type List&lt;T&gt;</c>
    /// (plato-src/unique.plato, roadmap Phase 6). A growable, runtime-checked builder in the
    /// spirit of <c>ImmutableArray&lt;T&gt;.Builder</c>: mutators return the builder (call sites
    /// use the reassignment idiom <c>xs = xs.Add(p)</c> or chaining), and <c>Freeze()</c> hands
    /// the backing array to an <see cref="IReadOnlyList{T}"/> WITHOUT copying, invalidating the
    /// builder. Any use after <c>Freeze()</c> throws <see cref="InvalidOperationException"/>.
    ///
    /// The C# name is <c>PlatoList</c> (the compiler maps the Plato name <c>List</c> to it)
    /// because this namespace also hosts handwritten geometry code for which a type literally
    /// named <c>List&lt;T&gt;</c> would shadow <c>System.Collections.Generic.List&lt;T&gt;</c>.
    ///
    /// Effect classification (the compiler's intrinsic table, PlatoCompiler UniqueTypes):
    /// observe = Count, At; mutate = Add, AddRange, Set; consume = Freeze.
    /// </summary>
    public sealed class PlatoList<T>
    {
        private T[] _items;
        private int _count;
        private bool _frozen;

        public PlatoList()
            => _items = Array.Empty<T>();

        [MethodImpl(AggressiveInlining)]
        private void CheckNotFrozen()
        {
            if (_frozen)
                throw new InvalidOperationException(
                    "This List builder has been frozen (consumed by Freeze); it may no longer be used.");
        }

        /// <summary>Observe: the number of elements added so far.</summary>
        public Integer Count
        {
            [MethodImpl(AggressiveInlining)]
            get { CheckNotFrozen(); return _count; }
        }

        /// <summary>Observe: the element at the given index.</summary>
        [MethodImpl(AggressiveInlining)]
        public T At(Integer n)
        {
            CheckNotFrozen();
            int index = n;
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            return _items[index];
        }

        public T this[Integer n]
        {
            [MethodImpl(AggressiveInlining)]
            get => At(n);
        }

        /// <summary>Mutate: appends an element and returns the builder (rebind or chain).</summary>
        public PlatoList<T> Add(T x)
        {
            CheckNotFrozen();
            if (_count == _items.Length)
                Array.Resize(ref _items, _items.Length == 0 ? 4 : _items.Length * 2);
            _items[_count++] = x;
            return this;
        }

        /// <summary>Mutate: appends all elements of an array and returns the builder.</summary>
        public PlatoList<T> AddRange(IReadOnlyList<T> xs)
        {
            CheckNotFrozen();
            var n = xs.Count;
            if (_count + n > _items.Length)
                Array.Resize(ref _items, Math.Max(_count + n, _items.Length == 0 ? 4 : _items.Length * 2));
            for (var i = 0; i < n; i++)
                _items[_count + i] = xs[i];
            _count += n;
            return this;
        }

        /// <summary>Mutate: overwrites the element at the given index and returns the builder.</summary>
        public PlatoList<T> Set(Integer i, T value)
        {
            CheckNotFrozen();
            int index = i;
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            _items[index] = value;
            return this;
        }

        /// <summary>
        /// Consume: hands the backing array to an immutable <see cref="IReadOnlyList{T}"/>
        /// WITHOUT copying (the wrapper carries the logical count) and invalidates the builder
        /// (MoveToImmutable semantics). Every later use of the builder throws.
        /// Plato call sites always write it with parentheses: <c>xs.Freeze()</c>.
        /// </summary>
        public IReadOnlyList<T> Freeze()
        {
            CheckNotFrozen();
            _frozen = true;
            var r = new FrozenArray<T>(_items, _count);
            _items = null;
            return r;
        }
    }

    /// <summary>
    /// The immutable result of <see cref="PlatoList{T}.Freeze"/> / <see cref="PlatoBuffer{T}.Freeze"/>:
    /// a zero-copy read-only view over the builder's backing array (with its logical count).
    /// </summary>
    public sealed class FrozenArray<T> : IReadOnlyList<T>
    {
        private readonly T[] _items;

        public int Count
        {
            [MethodImpl(AggressiveInlining)]
            get;
        }

        internal FrozenArray(T[] items, int count)
        {
            _items = items;
            Count = count;
        }

        public T this[int n]
        {
            [MethodImpl(AggressiveInlining)]
            get
            {
                if (n < 0 || n >= Count)
                    throw new IndexOutOfRangeException();
                return _items[n];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return _items[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    /// <summary>
    /// Creation intrinsics for the unique builder types in generic contexts, where a Plato
    /// type variable cannot appear inside a <c>new</c> expression: <c>xs.EmptyList()</c> makes
    /// an empty builder whose element type is taken from an existing array.
    /// </summary>
    public static class PlatoListExtensions
    {
        [MethodImpl(AggressiveInlining)]
        public static PlatoList<T> EmptyList<T>(this IReadOnlyList<T> xs)
            => new PlatoList<T>();
    }
}
