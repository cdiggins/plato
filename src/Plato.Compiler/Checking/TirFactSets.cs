// The immutable per-node collections behind TirFacts: a set that reuses a covering child's set, and a use map (compiler-546).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>An immutable set composed bottom-up from a node's own members and its children's sets. When one
    /// child's set already holds the rest (the common case: a single non-empty child, or a child that holds everything
    /// its siblings do) that set is reused and nothing is allocated, so the sets of a body take memory near linear in
    /// its size. Members compare under the comparer given at construction.</summary>
    internal sealed class TirFactSet<T> : IReadOnlyCollection<T> where T : class
    {
        public static readonly TirFactSet<T> Empty = new(null, null, null);

        private readonly IEqualityComparer<T> _comparer;
        private readonly T _single;
        private readonly HashSet<T> _many;

        private TirFactSet(IEqualityComparer<T> comparer, T single, HashSet<T> many)
            => (_comparer, _single, _many) = (comparer, single, many);

        public int Count => _many?.Count ?? (_single == null ? 0 : 1);

        public bool Contains(T item)
            => _many != null ? _many.Contains(item) : _single != null && _comparer.Equals(_single, item);

        /// <summary>The set of <paramref name="own"/> (when not null) and every member of <paramref name="children"/>.</summary>
        public static TirFactSet<T> Of(IEqualityComparer<T> comparer, T own, IEnumerable<TirFactSet<T>> children)
            => Of(comparer, own == null ? Enumerable.Empty<T>() : Enumerable.Repeat(own, 1), children);

        /// <summary>The set of the non-null members of <paramref name="own"/> and every member of <paramref name="children"/>.</summary>
        public static TirFactSet<T> Of(IEqualityComparer<T> comparer, IEnumerable<T> own, IEnumerable<TirFactSet<T>> children)
        {
            var owns = own.Where(o => o != null).ToList();
            var parts = children.Where(c => c.Count > 0).Distinct().ToList();
            if (parts.Count == 0)
                return owns.Count switch
                {
                    0 => Empty,
                    1 => new TirFactSet<T>(comparer, owns[0], null),
                    _ => new TirFactSet<T>(comparer, null, new HashSet<T>(owns, comparer))
                };
            var largest = parts.MaxBy(p => p.Count);
            if (owns.All(largest.Contains) && parts.All(p => ReferenceEquals(p, largest) || p.IsSubsetOf(largest)))
                return largest;
            var set = new HashSet<T>(owns, comparer);
            foreach (var part in parts)
                set.UnionWith(part);
            return new TirFactSet<T>(comparer, null, set);
        }

        /// <summary>Whether every member is in <paramref name="other"/>; walks this set, so call it on the smaller.</summary>
        private bool IsSubsetOf(TirFactSet<T> other) => this.All(other.Contains);

        public IEnumerator<T> GetEnumerator()
            => _many != null ? _many.GetEnumerator()
                : _single != null ? Enumerable.Repeat(_single, 1).GetEnumerator()
                : Enumerable.Empty<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>How a subtree uses (or assigns) one definition: the number of occurrences, counting a shared node once
    /// per path, and whether any occurrence sits under a <see cref="TirLambda"/> or under a loop within the subtree.
    /// The default is no use at all.</summary>
    public readonly record struct TirUse(int Count, bool UnderLambda, bool UnderLoop)
    {
        /// <summary>One occurrence, under nothing.</summary>
        public static readonly TirUse One = new(1, false, false);

        /// <summary>The uses of two disjoint subtrees together.</summary>
        public TirUse Plus(TirUse other)
            => new(Count + other.Count, UnderLambda || other.UnderLambda, UnderLoop || other.UnderLoop);
    }

    /// <summary>An immutable map from a definition key to its <see cref="TirUse"/>, composed bottom-up: a node's map is
    /// its own occurrence (if any) plus its children's maps added up, and a child's map is reused unchanged when the
    /// node adds nothing. Keys are compared by identity, except strings (the names of lowered-loop temporaries), which
    /// compare by ordinal value.</summary>
    internal sealed class TirUseMap : IReadOnlyCollection<KeyValuePair<object, TirUse>>
    {
        public static readonly TirUseMap Empty = new(null, default, null);
        public static readonly IEqualityComparer<object> KeyComparer = new UseKeyComparer();

        private readonly object _key;
        private readonly TirUse _use;
        private readonly Dictionary<object, TirUse> _many;

        private TirUseMap(object key, TirUse use, Dictionary<object, TirUse> many) => (_key, _use, _many) = (key, use, many);

        public int Count => _many?.Count ?? (_key == null ? 0 : 1);

        /// <summary>The use of <paramref name="key"/>, or the default when the subtree never mentions it.</summary>
        public TirUse Get(object key)
            => _many != null ? _many.TryGetValue(key, out var use) ? use : default
                : _key != null && KeyComparer.Equals(_key, key) ? _use : default;

        /// <summary>The keys with at least one occurrence.</summary>
        public IReadOnlyCollection<object> Keys
            => _many != null ? _many.Keys : _key != null ? new[] { _key } : Array.Empty<object>();

        /// <summary>The map of one occurrence of <paramref name="own"/> (when not null) plus every child map. A child
        /// met twice is added twice: a shared node counts once per path, as a walk of the tree would count it.</summary>
        public static TirUseMap Of(object own, IEnumerable<TirUseMap> children)
        {
            var parts = children.Where(c => c.Count > 0).ToList();
            if (parts.Count == 0)
                return own == null ? Empty : new TirUseMap(own, TirUse.One, null);
            if (own == null && parts.Count == 1)
                return parts[0];
            var sum = new Dictionary<object, TirUse>(KeyComparer);
            if (own != null)
                sum[own] = TirUse.One;
            foreach (var part in parts)
                foreach (var (key, use) in part)
                    sum[key] = sum.TryGetValue(key, out var sofar) ? sofar.Plus(use) : use;
            return new TirUseMap(null, default, sum);
        }

        /// <summary>This map with every use marked as under a lambda; the same instance when all already are.</summary>
        public TirUseMap UnderLambda()
            => Marked(u => u.UnderLambda, u => u with { UnderLambda = true });

        /// <summary>This map with every use marked as under a loop; the same instance when all already are.</summary>
        public TirUseMap UnderLoop()
            => Marked(u => u.UnderLoop, u => u with { UnderLoop = true });

        private TirUseMap Marked(Func<TirUse, bool> already, Func<TirUse, TirUse> mark)
        {
            if (this.All(kv => already(kv.Value)))
                return this;
            return _many == null
                ? new TirUseMap(_key, mark(_use), null)
                : new TirUseMap(null, default, _many.ToDictionary(kv => kv.Key, kv => mark(kv.Value), KeyComparer));
        }

        public IEnumerator<KeyValuePair<object, TirUse>> GetEnumerator()
            => _many != null ? _many.GetEnumerator()
                : _key != null ? Enumerable.Repeat(new KeyValuePair<object, TirUse>(_key, _use), 1).GetEnumerator()
                : Enumerable.Empty<KeyValuePair<object, TirUse>>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>Identity for definitions, ordinal value for the strings that name temporaries.</summary>
        private sealed class UseKeyComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
                => x is string sx ? y is string sy && string.Equals(sx, sy, StringComparison.Ordinal) : ReferenceEquals(x, y);

            public int GetHashCode(object o)
                => o is string s ? StringComparer.Ordinal.GetHashCode(s) : RuntimeHelpers.GetHashCode(o);
        }
    }
}
