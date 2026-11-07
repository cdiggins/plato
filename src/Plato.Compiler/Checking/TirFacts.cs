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
    /// Lists: a node keeps the caller's lists (<c>Args</c>, <c>Statements</c>) rather than copying them, and a memoized
    /// fact assumes no list is mutated after its node is built; a caller that edits a list in place makes the table stale.
    /// </para>
    /// <para>
    /// Retention: a table lives as long as its body's context and holds the nodes (and definition sets) of every
    /// intermediate tree of the body, so it is dropped with the body.
    /// </para>
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
        private int _visited;

        /// <summary>The number of (fact, node) computations so far: a node is computed at most once per fact (a
        /// structural key counts every node it computes, inner nodes included, once per key table), so over a
        /// shared-subtree DAG this is linear in the distinct nodes, not in the paths. For tests.</summary>
        public int NodesVisited => _visited + (_key?.Computed ?? 0) + (_wideKey?.Computed ?? 0);

        /// <summary>The number of nodes in the subtree of <paramref name="n"/>, itself included, counting a shared node
        /// once per path (what <c>n.Descendants().Count()</c> returns); 0 for null.</summary>
        public int Size(TirNode n)
        {
            if (n == null)
                return 0;
            if (_size.TryGetValue(n, out var size))
                return size;
            _visited++;
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
            _visited++;
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
        public IReadOnlyCollection<object> MentionedDefs(TirNode n) => Defs(n);

        /// <summary>Whether the subtree of <paramref name="n"/> reads <paramref name="def"/> (see <see cref="MentionedDefs"/>).</summary>
        public bool Mentions(TirNode n, object def) => def != null && Defs(n).Contains(def);

        private DefSet Defs(TirNode n)
        {
            if (n == null)
                return DefSet.Empty;
            if (_defs.TryGetValue(n, out var defs))
                return defs;
            _visited++;
            var own = n switch
            {
                TirVariable v => (object)v.Def,
                TirParameter p => p.Def,
                _ => null
            };
            return _defs[n] = DefSet.Of(own, n.Children.Select(Defs));
        }

        /// <summary>The structural identity of a call for the shared-calls pass (see <c>TirStructuralKey</c> below), or
        /// null when something below it is not known to be pure and repeatable. <paramref name="wide"/> admits the
        /// factory calls that <c>--share-calls-wide</c> shares. Two keys compare only within one table: the
        /// definitions and callees they name are numbered in the order this table first met them.</summary>
        public string StructuralKey(TirCall call, bool wide = false)
        {
            if (call == null)
                return null;
            return wide
                ? (_wideKey ??= new TirStructuralKey(true)).Key(call)
                : (_key ??= new TirStructuralKey(false)).Key(call);
        }

        /// <summary>An immutable set of definitions compared by identity. A node's set is built once from all its
        /// children's sets; when one set already holds the rest (the common case: a single non-empty child, or a child
        /// that reads everything its siblings do) that set is reused and nothing is allocated.</summary>
        private sealed class DefSet : IReadOnlyCollection<object>
        {
            public static readonly DefSet Empty = new(null, null);

            private readonly object _single;
            private readonly HashSet<object> _many;

            private DefSet(object single, HashSet<object> many) => (_single, _many) = (single, many);

            public int Count => _many?.Count ?? (_single == null ? 0 : 1);

            public bool Contains(object item)
                => _many != null ? _many.Contains(item) : _single != null && ReferenceEquals(_single, item);

            /// <summary>The set of <paramref name="own"/> (when not null) and every member of <paramref name="children"/>.</summary>
            public static DefSet Of(object own, IEnumerable<DefSet> children)
            {
                var parts = children.Where(c => c.Count > 0).Distinct().ToList();
                if (parts.Count == 0)
                    return own == null ? Empty : new DefSet(own, null);
                var largest = parts.MaxBy(p => p.Count);
                if ((own == null || largest.Contains(own)) && parts.All(p => ReferenceEquals(p, largest) || p.IsSubsetOf(largest)))
                    return largest;
                var set = new HashSet<object>(ReferenceEqualityComparer.Instance);
                if (own != null)
                    set.Add(own);
                foreach (var part in parts)
                    set.UnionWith(part);
                return new DefSet(null, set);
            }

            /// <summary>Whether every member is in <paramref name="other"/>; walks this set, so call it on the smaller.</summary>
            private bool IsSubsetOf(DefSet other) => this.All(other.Contains);

            public IEnumerator<object> GetEnumerator()
                => _many != null ? _many.GetEnumerator()
                    : _single != null ? Enumerable.Repeat(_single, 1).GetEnumerator()
                    : Enumerable.Empty<object>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
        private sealed class TirStructuralKey
        {
            private readonly bool _wide;
            private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);
            private readonly Dictionary<TirNode, string> _keys = new(ReferenceEqualityComparer.Instance);

            /// <param name="wide">Admits a sum case written as a factory (an unresolved static call by name), as
            /// <c>--share-calls-wide</c> does.</param>
            public TirStructuralKey(bool wide) => _wide = wide;

            /// <summary>The number of nodes whose key this table has computed (not answered from its memo).</summary>
            public int Computed { get; private set; }

            /// <summary>The key of <paramref name="n"/>, or null (see the class summary).</summary>
            public string Key(TirNode n)
            {
                if (n == null)
                    return null;
                if (_keys.TryGetValue(n, out var key))
                    return key;
                Computed++;
                return _keys[n] = Write(n);
            }

            private int Id(object o)
            {
                if (!_ids.TryGetValue(o, out var id))
                    _ids[o] = id = _ids.Count;
                return id;
            }

                        private string Write(TirNode n)
            {
                switch (n)
                {
                    case TirParameter p when p.Def != null && !UniqueTypes.IsUniqueType(p.Type):
                        return "P" + Id(p.Def);
                    case TirVariable v when v.Def != null && !UniqueTypes.IsUniqueType(v.Type):
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
                    case TirCall c when c.Callee != null && !UniqueTypes.IsUniqueType(c.Type) && !c.Args.Any(a => UniqueTypes.IsUniqueType(a?.Type)):
                        return Args(c.Args, $"C{Id(c.Callee)}:{c.EmissionKind}:{c.Type}(");
                    case TirCall c when c.Callee == null && c.EmissionKind == EmissionKind.Property
                                        && c.Args.Count == 1 && !UniqueTypes.IsUniqueType(c.Type):
                    {
                        // A syntactic field projection (a match binder or a case test) of a shared value.
                        var inner = c.Args[0] != null ? Key(c.Args[0]) : null;
                        return inner == null ? null : $"F{c.Name}:{c.Type}({inner})";
                    }
                    case TirCall c when _wide && c.Callee == null && c.EmissionKind == EmissionKind.StaticMethod
                                        && c.Name != null && !UniqueTypes.IsUniqueType(c.Type) && !c.Args.Any(a => a == null || UniqueTypes.IsUniqueType(a.Type)):
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
}
