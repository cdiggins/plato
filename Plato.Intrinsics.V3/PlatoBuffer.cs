using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The handwritten implementation of the affine builder type <c>unique type Buffer&lt;T&gt;</c>
    /// (plato-src/unique.plato, roadmap Phase 6). A fixed-size, runtime-checked builder:
    /// allocate n slots up front (<c>new Buffer&lt;T&gt;(n)</c> in Plato), fill them by index in
    /// any order (<c>Set</c> returns the builder; call sites rebind or chain), then
    /// <c>Freeze()</c> hands the backing array to an <see cref="IReadOnlyList{T}"/> WITHOUT
    /// copying and invalidates the builder. Any use after <c>Freeze()</c> throws
    /// <see cref="InvalidOperationException"/>.
    ///
    /// The C# name is <c>PlatoBuffer</c> (the compiler maps the Plato name <c>Buffer</c> to it),
    /// mirroring PlatoList and avoiding any confusion with <c>System.Buffer</c>.
    ///
    /// Effect classification (the compiler's intrinsic table, PlatoCompiler UniqueTypes):
    /// observe = Count, At; mutate = Set; consume = Freeze.
    /// </summary>
    public sealed class PlatoBuffer<T>
    {
        private T[] _items;
        private bool _frozen;

        public PlatoBuffer(Integer count)
        {
            int n = count;
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            _items = n == 0 ? Array.Empty<T>() : new T[n];
        }

        [MethodImpl(AggressiveInlining)]
        private void CheckNotFrozen()
        {
            if (_frozen)
                throw new InvalidOperationException(
                    "This Buffer builder has been frozen (consumed by Freeze); it may no longer be used.");
        }

        /// <summary>Observe: the fixed number of slots.</summary>
        public Integer Count
        {
            [MethodImpl(AggressiveInlining)]
            get { CheckNotFrozen(); return _items.Length; }
        }

        /// <summary>Observe: the element at the given index (default(T) until written).</summary>
        [MethodImpl(AggressiveInlining)]
        public T At(Integer n)
        {
            CheckNotFrozen();
            return _items[n];
        }

        public T this[Integer n]
        {
            [MethodImpl(AggressiveInlining)]
            get => At(n);
        }

        /// <summary>Mutate: writes the slot at the given index and returns the builder.</summary>
        [MethodImpl(AggressiveInlining)]
        public PlatoBuffer<T> Set(Integer i, T value)
        {
            CheckNotFrozen();
            _items[i] = value;
            return this;
        }

        /// <summary>
        /// Consume: hands the backing array to an immutable <see cref="IReadOnlyList{T}"/>
        /// WITHOUT copying and invalidates the builder. Every later use of the builder throws.
        /// Plato call sites always write it with parentheses: <c>xs.Freeze()</c>.
        /// </summary>
        public IReadOnlyList<T> Freeze()
        {
            CheckNotFrozen();
            _frozen = true;
            var r = new FrozenArray<T>(_items, _items.Length);
            _items = null;
            return r;
        }
    }
}
