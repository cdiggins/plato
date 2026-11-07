// Facts about a TIR subtree, memoized by node identity for one body, so passes query instead of recounting (compiler-546).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The facts about a subtree that follow from the subtree alone: its size, whether it holds a node of some kind,
    /// which binding definitions it reads, and the structural key of a call. A <see cref="TirNode"/> is immutable, so
    /// such a fact is intrinsic to the node object: it is computed once per node, bottom-up from the children's facts,
    /// and can never go stale, because a rewrite builds new nodes and never changes old ones. A shared subtree (one
    /// node reached by several parents, as inlining and the shared-calls pass produce) is therefore sized once where a
    /// <see cref="TirNode.Descendants"/> walk visits it once per path.
    /// <para>
    /// Contract: one instance per body, on one thread. The optimizer carries one in its per-body pass context and
    /// the elaborator makes one per function; a table is never shared between bodies that run at once, and never
    /// needs to be, since a fact is a function of the node. Nodes of a body's earlier trees stay in the table
    /// (bounded by the body's size times the number of passes), which is harmless. A fact that depends on where a node
    /// stands (its statement index, its enclosing lambda) is not intrinsic and does not belong here.
    /// </para>
    /// </summary>
    public sealed class TirFacts
    {
        private readonly Dictionary<TirNode, int> _size = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<Type, Dictionary<TirNode, bool>> _contains = new();
        private readonly Dictionary<TirNode, DefSet> _defs = new(ReferenceEqualityComparer.Instance);
        private TirStructuralKey _key;
        private TirStructuralKey _wideKey;

        /// <summary>The number of (fact, node) computations so far: a node is computed at most once per fact, so over
        /// a shared-subtree DAG this is linear in the distinct nodes, not in the paths. For tests.</summary>
        public int NodesVisited { get; private set; }

        /// <summary>The number of nodes in the subtree of <paramref name="n"/>, itself included, counting a shared node
        /// once per path (what <c>n.Descendants().Count()</c> returns); 0 for null.</summary>
        public int Size(TirNode n)
        {
            if (n == null)
                return 0;
            if (_size.TryGetValue(n, out var size))
                return size;
            NodesVisited++;
            size = 1;
            foreach (var child in n.Children)
                size += Size(child);
            return _size[n] = size;
        }

        /// <summary>Whether the subtree of <paramref name="n"/>, itself included, holds a node of type
        /// <typeparamref name="T"/>; false for null.</summary>
        public bool ContainsKind<T>(TirNode n) where T : TirNode
        {
            if (n == null)
                return false;
            if (!_contains.TryGetValue(typeof(T), out var memo))
                _contains[typeof(T)] = memo = new Dictionary<TirNode, bool>(ReferenceEqualityComparer.Instance);
            if (memo.TryGetValue(n, out var found))
                return found;
            NodesVisited++;
            found = n is T || n.Children.Any(ContainsKind<T>);
            return memo[n] = found;
        }

        /// <summary>Whether the subtree assigns (<see cref="TirAssign"/>).</summary>
        public bool ContainsAssign(TirNode n) => ContainsKind<TirAssign>(n);

        /// <summary>Whether the subtree loops (<see cref="TirLoop"/>).</summary>
        public bool ContainsLoop(TirNode n) => ContainsKind<TirLoop>(n);

        /// <summary>The binding definitions the subtree of <paramref name="n"/> reads: the <see cref="VariableDef"/> of
        /// every <see cref="TirVariable"/> and the <see cref="ParameterDef"/> of every <see cref="TirParameter"/>,
        /// compared by identity. Lambda bodies are included (a read under a lambda is a read). Empty for null.</summary>
        public IReadOnlySet<object> MentionedDefs(TirNode n) => Defs(n);

        /// <summary>Whether the subtree of <paramref name="n"/> reads <paramref name="def"/> (see <see cref="MentionedDefs"/>).</summary>
        public bool Mentions(TirNode n, object def) => def != null && Defs(n).Contains(def);

        private DefSet Defs(TirNode n)
        {
            if (n == null)
                return DefSet.Empty;
            if (_defs.TryGetValue(n, out var defs))
                return defs;
            NodesVisited++;
            defs = n switch
            {
                TirVariable v when v.Def != null => DefSet.Of(v.Def),
                TirParameter p when p.Def != null => DefSet.Of(p.Def),
                _ => DefSet.Empty
            };
            foreach (var child in n.Children)
                defs = defs.Union(Defs(child));
            return _defs[n] = defs;
        }

        /// <summary>The structural identity of a call for the shared-calls pass (<see cref="TirStructuralKey"/>), or
        /// null when something below it is not known to be pure and repeatable. <paramref name="wide"/> admits the
        /// factory calls that <c>--share-calls-wide</c> shares. Two keys compare only within one table: the
        /// definitions and callees they name are numbered in the order this table first met them.</summary>
        public string StructuralKey(TirCall call, bool wide = false)
        {
            if (call == null)
                return null;
            NodesVisited++;
            return wide
                ? (_wideKey ??= new TirStructuralKey(true)).Key(call)
                : (_key ??= new TirStructuralKey(false)).Key(call);
        }

        /// <summary>An immutable set of definitions compared by identity, built from a child's set without walking it: an
        /// empty set and a single-member set allocate nothing when united with a set that already holds them.</summary>
        private sealed class DefSet : IReadOnlySet<object>
        {
            public static readonly DefSet Empty = new(null, null);

            private readonly object _single;
            private readonly HashSet<object> _many;

            private DefSet(object single, HashSet<object> many) => (_single, _many) = (single, many);

            public static DefSet Of(object def) => new(def, null);

            public int Count => _many?.Count ?? (_single == null ? 0 : 1);

            public bool Contains(object item)
                => _many != null ? _many.Contains(item) : _single != null && ReferenceEquals(_single, item);

            public DefSet Union(DefSet other)
            {
                if (other.Count == 0 || ReferenceEquals(this, other))
                    return this;
                if (Count == 0)
                    return other;
                if (other.Count <= Count && other.IsSubsetOf(this))
                    return this;
                if (Count <= other.Count && IsSubsetOf(other))
                    return other;
                var set = new HashSet<object>(this, ReferenceEqualityComparer.Instance);
                set.UnionWith(other);
                return new DefSet(null, set);
            }

            public IEnumerator<object> GetEnumerator()
                => _many != null ? _many.GetEnumerator()
                    : _single != null ? Enumerable.Repeat(_single, 1).GetEnumerator()
                    : Enumerable.Empty<object>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool IsSubsetOf(IEnumerable<object> other)
            {
                if (_many != null)
                    return _many.IsSubsetOf(other);
                return _single == null || other.Any(o => ReferenceEquals(o, _single));
            }

            public bool IsSupersetOf(IEnumerable<object> other) => other.All(Contains);
            public bool IsProperSubsetOf(IEnumerable<object> other) => IsSubsetOf(other) && Materialize(other).Count > Count;
            public bool IsProperSupersetOf(IEnumerable<object> other) => IsSupersetOf(other) && Materialize(other).Count < Count;
            public bool Overlaps(IEnumerable<object> other) => other.Any(Contains);
            public bool SetEquals(IEnumerable<object> other) => IsSupersetOf(other) && Materialize(other).Count == Count;

            private static HashSet<object> Materialize(IEnumerable<object> other)
                => other as HashSet<object> ?? new HashSet<object>(other, ReferenceEqualityComparer.Instance);
        }
    }

    /// <summary>
    /// The structural key of the shared-calls pass: two calls with the same callee, emission kind and type, and
    /// arguments that are the same definitions (by identity), the same literals (by value), the same type
    /// references and the same conversions of such, get the same string. A key is null when the call or something
    /// below it is not known to be pure and repeatable: an affine (unique) type anywhere, a lambda, an unresolved
    /// call, or any node the writer does not list. Keys are memoized per node and composed from the children's keys,
    /// so a key costs its children after the first visit. Definitions and callees are numbered in the order they are
    /// first met, so keys compare only within one instance; the numbering follows the pre-order of the first request,
    /// which is what the pass's one-pass writer did.
    /// </summary>
    public sealed class TirStructuralKey
    {
        private readonly bool _wide;
        private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<TirNode, string> _keys = new(ReferenceEqualityComparer.Instance);

        /// <param name="wide">Admits a sum case written as a factory (an unresolved static call by name), as
        /// <c>--share-calls-wide</c> does.</param>
        public TirStructuralKey(bool wide) => _wide = wide;

        /// <summary>The key of <paramref name="n"/>, or null (see the class summary).</summary>
        public string Key(TirNode n)
        {
            if (n == null)
                return null;
            if (_keys.TryGetValue(n, out var key))
                return key;
            return _keys[n] = Write(n);
        }

        private int Id(object o)
        {
            if (!_ids.TryGetValue(o, out var id))
                _ids[o] = id = _ids.Count;
            return id;
        }

        private static bool IsUnique(TypeExpression t)
            => t != null && ((t.Def?.IsUnique ?? false) || t.TypeArgs.Any(IsUnique));

        private string Write(TirNode n)
        {
            switch (n)
            {
                case TirParameter p when p.Def != null && !IsUnique(p.Type):
                    return "P" + Id(p.Def);
                case TirVariable v when v.Def != null && !IsUnique(v.Type):
                    return "V" + Id(v.Def);
                case TirLiteral l:
                    return $"L{l.LiteralType}:{l.Type}:{l.Value}";
                case TirTypeRef t:
                    return $"T{t.TypeDef?.Name}{t.NamespaceQualified}";
                case TirDefault d:
                    return $"D{d.Type}";
                case TirCoerce c:
                {
                    var inner = Key(c.Inner);
                    return inner == null ? null : $"K{c.ToType}({inner})";
                }
                case TirCall c when c.Callee != null && !IsUnique(c.Type) && !c.Args.Any(a => IsUnique(a?.Type)):
                    return Args(c.Args, $"C{Id(c.Callee)}:{c.EmissionKind}:{c.Type}(");
                case TirCall c when c.Callee == null && c.EmissionKind == EmissionKind.Property
                                    && c.Args.Count == 1 && !IsUnique(c.Type):
                {
                    // A syntactic field projection (a match binder or a case test) of a shared value.
                    var inner = c.Args[0] != null ? Key(c.Args[0]) : null;
                    return inner == null ? null : $"F{c.Name}:{c.Type}({inner})";
                }
                case TirCall c when _wide && c.Callee == null && c.EmissionKind == EmissionKind.StaticMethod
                                    && c.Name != null && !IsUnique(c.Type) && !c.Args.Any(a => a == null || IsUnique(a.Type)):
                    // A sum case written as a factory (Kind.EdgeLoop): a pure value, compared by name and payload.
                    return Args(c.Args, $"S{c.Name}:{c.Type}(");
                default:
                    return null;
            }
        }

        /// <summary>The head followed by each argument's key and a comma, then the closing parenthesis; null as soon
        /// as an argument has no key, so the arguments after it are not numbered (as the one-pass writer left them).</summary>
        private string Args(IReadOnlyList<TirNode> args, string head)
        {
            var sb = new StringBuilder(head);
            foreach (var a in args)
            {
                var key = Key(a);
                if (key == null)
                    return null;
                sb.Append(key).Append(',');
            }
            return sb.Append(')').ToString();
        }
    }
}
