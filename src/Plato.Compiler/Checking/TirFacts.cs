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
    /// which binding definitions it reads, which names it binds or mentions, how often and under what it uses or
    /// assigns each definition, and the structural key of a call. A <see cref="TirNode"/> is immutable, so
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
        private readonly Dictionary<TirNode, TirFactSet<object>> _defs = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<TirNode, TirFactSet<string>> _names = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<TirNode, TirUseMap> _uses = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<TirNode, TirUseMap> _assigns = new(ReferenceEqualityComparer.Instance);
        private readonly TirNodeModel _model;
        private TirStructuralKey _key;
        private TirStructuralKey _wideKey;
        private int _visited;

        /// <summary>The number of (fact, node) computations so far: a node is computed at most once per fact (a
        /// structural key counts every node it computes, inner nodes included, once per key table), so over a
        /// shared-subtree DAG this is linear in the distinct nodes, not in the paths. For tests.</summary>
        public int NodesVisited => _visited + (_key?.Computed ?? 0) + (_wideKey?.Computed ?? 0);

        /// <summary>A table that knows the checker's node kinds alone (<see cref="TirNodeModel.Default"/>).</summary>
        public TirFacts() : this(TirNodeModel.Default) { }

        /// <summary>A table whose name, use and loop facts also understand the node kinds <paramref name="model"/>
        /// describes (the optimizer's, which this project cannot name).</summary>
        public TirFacts(TirNodeModel model) => _model = model ?? TirNodeModel.Default;

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

        private TirFactSet<object> Defs(TirNode n)
        {
            if (n == null)
                return TirFactSet<object>.Empty;
            if (_defs.TryGetValue(n, out var defs))
                return defs;
            _visited++;
            var own = n switch
            {
                TirVariable v => (object)v.Def,
                TirParameter p => p.Def,
                _ => null
            };
            return _defs[n] = TirFactSet<object>.Of(ReferenceEqualityComparer.Instance, own, n.Children.Select(Defs));
        }

        /// <summary>The names the subtree of <paramref name="n"/> binds or mentions: the name of every definition a
        /// <see cref="TirVariable"/> or <see cref="TirParameter"/> reads, of every <see cref="TirLet"/>, of every
        /// lambda parameter, and of the optimizer's binders and temporaries as its <see cref="TirNodeModel"/> lists
        /// them. A pass that invents a local picks a name outside this set. The function's own parameter names are not
        /// a fact of the body; <see cref="BoundNames(TirFunction)"/> adds them. Empty for null.</summary>
        public IReadOnlyCollection<string> BoundNames(TirNode n) => Names(n);

        /// <summary>A fresh, mutable set of <see cref="BoundNames(TirNode)"/> of the body together with the function's
        /// parameter names: what a pass starts from and adds its fresh names to.</summary>
        public HashSet<string> BoundNames(TirFunction function)
        {
            var names = new HashSet<string>(Names(function?.Body), StringComparer.Ordinal);
            if (function?.Parameters != null)
                foreach (var p in function.Parameters)
                    if (p?.Name != null)
                        names.Add(p.Name);
            return names;
        }

        private TirFactSet<string> Names(TirNode n)
        {
            if (n == null)
                return TirFactSet<string>.Empty;
            if (_names.TryGetValue(n, out var names))
                return names;
            _visited++;
            return _names[n] = TirFactSet<string>.Of(StringComparer.Ordinal, _model.Names(n), n.Children.Select(Names));
        }

        /// <summary>How the subtree of <paramref name="n"/> uses <paramref name="def"/>: the number of occurrences
        /// (every <see cref="TirVariable"/> and <see cref="TirParameter"/> node whose definition it is, the target of
        /// an assignment included, and every temporary reference by name) and whether any occurrence sits under a
        /// <see cref="TirLambda"/> or under a loop that is strictly inside <paramref name="n"/>, or is
        /// <paramref name="n"/> itself: a lambda root counts its body as under the lambda. A loop is a
        /// <see cref="TirLoop"/> or whatever the model names (a guarded loop); a lowered loop is not one, since its
        /// sources run once and its repeated part is a lambda. Keys are <c>VariableDef</c>, <c>ParameterDef</c> and
        /// the temporary's name string. The default (no use) for null or an unknown key.</summary>
        public TirUse Uses(TirNode n, object def) => def == null ? default : UseMap(n).Get(def);

        /// <summary>Every definition the subtree uses with its <see cref="TirUse"/> (see <see cref="Uses(TirNode, object)"/>).</summary>
        public IReadOnlyCollection<KeyValuePair<object, TirUse>> Uses(TirNode n) => UseMap(n);

        /// <summary>The number of occurrences of <paramref name="def"/> in the subtree (see <see cref="Uses(TirNode, object)"/>).</summary>
        public int UseCount(TirNode n, object def) => Uses(n, def).Count;

        /// <summary>Whether an occurrence of <paramref name="def"/> sits under a lambda (see <see cref="Uses(TirNode, object)"/>).</summary>
        public bool UsesUnderLambda(TirNode n, object def) => Uses(n, def).UnderLambda;

        /// <summary>How the subtree of <paramref name="n"/> assigns <paramref name="def"/>: the number of
        /// <see cref="TirAssign"/> nodes whose target is the variable or parameter, and whether any sits under a lambda
        /// or a loop, with the same rules as <see cref="Uses(TirNode, object)"/>. An assignment whose target is
        /// neither is not counted. The default for null or an unknown key.</summary>
        public TirUse Assigns(TirNode n, object def) => def == null ? default : AssignMap(n).Get(def);

        /// <summary>Every definition the subtree assigns with its <see cref="TirUse"/> (see <see cref="Assigns"/>).</summary>
        public IReadOnlyCollection<KeyValuePair<object, TirUse>> Assignments(TirNode n) => AssignMap(n);

        /// <summary>The definitions (<c>VariableDef</c> or <c>ParameterDef</c>) that some <see cref="TirAssign"/> in the
        /// subtree targets. Empty for null.</summary>
        public IReadOnlyCollection<object> AssignedDefs(TirNode n) => AssignMap(n).Keys;

        private TirUseMap UseMap(TirNode n) => Counts(n, _uses, _model.UseKey);

        private TirUseMap AssignMap(TirNode n) => Counts(n, _assigns, _model.AssignedKey);

        /// <summary>The per-definition counts of <paramref name="key"/> occurrences over the subtree, marked under
        /// lambda or loop by the node that is one, memoized in <paramref name="memo"/>.</summary>
        private TirUseMap Counts(TirNode n, Dictionary<TirNode, TirUseMap> memo, Func<TirNode, object> key)
        {
            if (n == null)
                return TirUseMap.Empty;
            if (memo.TryGetValue(n, out var map))
                return map;
            _visited++;
            map = TirUseMap.Of(key(n), n.Children.Select(c => Counts(c, memo, key)));
            if (n is TirLambda)
                map = map.UnderLambda();
            if (_model.IsLoop(n))
                map = map.UnderLoop();
            return memo[n] = map;
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

    /// <summary>
    /// What <see cref="TirFacts"/> asks of a node kind. The optimizer adds node kinds (an inline binding, an inline
    /// region, a temporary reference, a guarded loop) in a project this one cannot reference, so the knowledge of which
    /// node binds a name, reads a key, or repeats its children is one overridable object: the optimizer's model answers
    /// for its own kinds and defers to the base for the checker's, and hands itself to its table. The default knows the
    /// checker's nodes alone. A model is pure and stateless, so a table's facts stay intrinsic to the node.
    /// </summary>
    public class TirNodeModel
    {
        public static readonly TirNodeModel Default = new();

        /// <summary>The names the node itself binds or mentions (its children are asked separately): the definition
        /// read by a <see cref="TirVariable"/> or <see cref="TirParameter"/>, the local of a <see cref="TirLet"/>, the
        /// parameters of a <see cref="TirLambda"/>. Nulls are dropped.</summary>
        public virtual IEnumerable<string> Names(TirNode n) => n switch
        {
            TirVariable v => One(v.Def?.Name),
            TirParameter p => One(p.Def?.Name),
            TirLet l => One(l.Def?.Name),
            TirLambda l => l.Parameters.Select(p => p?.Name),
            _ => Enumerable.Empty<string>()
        };

        /// <summary>The key the node itself is one use of (a <c>VariableDef</c>, a <c>ParameterDef</c>, or a
        /// temporary's name), or null.</summary>
        public virtual object UseKey(TirNode n) => n switch
        {
            TirVariable v => v.Def,
            TirParameter p => p.Def,
            _ => null
        };

        /// <summary>The definition the node assigns (the variable or parameter a <see cref="TirAssign"/> targets), or null.</summary>
        public virtual object AssignedKey(TirNode n) => n is TirAssign a ? Target(a.LValue) : null;

        /// <summary>Whether the node runs its children once per iteration (<see cref="TirLoop"/>: condition and body).</summary>
        public virtual bool IsLoop(TirNode n) => n is TirLoop;

        private static object Target(TirNode lvalue) => lvalue switch
        {
            TirVariable v => v.Def,
            TirParameter p => p.Def,
            _ => null
        };

        private static IEnumerable<string> One(string name) => name == null ? Enumerable.Empty<string>() : Enumerable.Repeat(name, 1);
    }
}
