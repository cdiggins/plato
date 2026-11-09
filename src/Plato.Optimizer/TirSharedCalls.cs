// Evaluates a call that a statement repeats once (--share-calls, TKT-1136).
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// A <c>match</c> lowers to a chain of tag tests and field reads that all name the scrutinee's
/// <em>expression</em>: <c>row.IsDecoded() ? Decoded(row.Curve) : Skipped(row.Code, row.About)</c> with
/// <c>row</c> a call becomes five calls, and the C# compiler cannot merge calls. Plato is pure, so a call
/// that a statement evaluates twice returns the same value twice; this pass evaluates it once, in a local
/// declared immediately before the statement.
///
/// A call node is shared when, inside one statement's expression, a second node with the same callee,
/// the same emission kind and the same arguments (variables and parameters compared by identity,
/// literals by value) appears and
/// <list type="bullet">
/// <item>at least one occurrence is evaluated on every path through the expression (an operand of calls,
/// constructors and conversions, or the condition of a conditional, never a branch of one), so
/// evaluating it first adds no work and cannot raise an error the expression would not have raised;</item>
/// <item>the node is a method call (a field read, an operator, a constructor and a conversion cost nothing
/// worth a local), neither it nor anything below it has an affine builder type (<c>List</c>,
/// <c>Buffer</c>) or an array type (left to the loop passes, which already materialise repeated
/// arrays), and nothing below it is a lambda or a node the pass does not know;</item>
/// <item>the expression assigns nothing.</item>
/// </list>
/// The largest such node goes first; the rest are searched again on what is left, so a shared call whose
/// argument is also shared gets two locals in dependency order. A lambda body is a separate expression
/// and is not entered. Runs before the loop lowerer, which takes the new bindings as statements.
/// The identity of a call (the key table inside <see cref="TirFacts"/>), its size and whether an expression assigns come from
/// the body's fact table (<see cref="PassContext.Facts"/>), computed once per node (compiler-546).
/// </summary>
public static class TirSharedCalls
{
    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On("share-calls") || tir?.Body == null)
            return tir;
        var sharing = new Sharing(tir, pass.Facts, pass.On("share-calls-wide"), pass.Stats.Rewrites);
        var body = tir.Body;
        if (pass.On("share-calls-wide"))
            body = sharing.Lambdas(sharing.Statements(body));
        body = sharing.Body(body);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private sealed class Sharing
    {
        private readonly HashSet<string> _names;
        private int _next;

        private readonly TirFacts _facts;
        private readonly bool _wide;
        private readonly WriterRewriteStats _stats;
        private readonly HashSet<object> _parameters;

        public Sharing(TirFunction function, TirFacts facts, bool wide = false, WriterRewriteStats stats = null)
        {
            _facts = facts;
            _wide = wide;
            _stats = stats ?? new WriterRewriteStats();
            _parameters = function.Parameters.Cast<object>().ToHashSet(ReferenceEqualityComparer.Instance);
            _names = facts.BoundNames(function);
        }

        private VariableDef Fresh(TypeExpression type)
        {
            string name;
            do name = $"_shared{_next++}"; while (!_names.Add(name));
            return new VariableDef(null, name, type, null);
        }

        public TirNode Body(TirNode node)
        {
            if (TirRewrite.IsStatementNode(node))
                return Scope(node);
            var value = TirEvaluation.Capture(node, Value);
            return value.Statements.Count == 0 ? value.Value : new TirBlock(value.Statements
                .Append(new TirReturn(value.Value, node.Origin)).ToArray(), node.Origin);
        }

        private TirNode Scope(TirNode node)
            => node == null ? null : new TirBlock((node is TirBlock b ? b.Statements : new[] { node })
                .SelectMany(n => TirStatementRewriter.Rewrite(n, Value, Scope)).ToArray(), node.Origin);

        private sealed class Site
        {
            public int Count;
            public int Strict;
            public int Size;
            public int First;
        }

        /// <summary>The expression with every shared call replaced by a local; the locals are appended to
        /// <paramref name="before"/> in the order they must run.</summary>
        private TirNode Value(TirNode expression, List<TirNode> before)
        {
            if (expression == null || _facts.ContainsAssign(expression))
                return expression;
            var items = new List<(VariableDef Def, TirNode Value)>();
            var current = expression;
            while (true)
            {
                var sites = new Dictionary<string, Site>();
                var samples = new Dictionary<string, TirCall>();
                var order = 0;
                Collect(current, true, sites, samples, ref order);
                foreach (var item in items)
                    Collect(item.Value, true, sites, samples, ref order);
                var key = sites.Where(s => s.Value.Count >= 2 && s.Value.Strict >= 1)
                    .OrderByDescending(s => s.Value.Size).ThenBy(s => s.Value.First)
                    .Select(s => s.Key).FirstOrDefault();
                if (key == null)
                    break;
                var sample = samples[key];
                var def = Fresh(sample.Type);
                var reference = new TirVariable(def, def.Type, sample.Origin);
                TirNode Replace(TirNode root) => root == null ? null : ReplaceAll(root, key, reference);
                // The local goes before the first item that mentions the call, and the items after it
                // that mention it read the local.
                var position = items.FindIndex(i => Mentions(i.Value, key));
                items = items.Select(i => (i.Def, Replace(i.Value))).ToList();
                current = Replace(current);
                items.Insert(position < 0 ? items.Count : position, (def, sample));
            }
            foreach (var (def, value) in items)
                before.Add(new TirInlineBinding(def, value));
            return current;
        }

        private bool Mentions(TirNode root, string key)
        {
            var found = false;
            void Walk(TirNode n)
            {
                if (n == null || found || n is TirLambda)
                    return;
                if (n is TirCall c && IsCandidate(c) && Key(c) == key)
                    found = true;
                foreach (var child in n.Children)
                    Walk(child);
            }
            Walk(root);
            return found;
        }

        private TirNode ReplaceAll(TirNode root, string key, TirNode reference)
        {
            var depth = 0;
            return TirRewrite.Rewrite(root,
                n => depth == 0 && n is TirCall c && IsCandidate(c) && Key(c) == key ? reference : n,
                enter: n => { if (n is TirLambda) depth++; },
                exit: n => { if (n is TirLambda) depth--; });
        }

        /// <summary>Counts the candidate calls of an expression by key. <paramref name="strict"/> says that the
        /// node is evaluated on every path through the expression.</summary>
        private void Collect(TirNode n, bool strict, Dictionary<string, Site> sites,
            Dictionary<string, TirCall> samples, ref int order)
        {
            if (n == null || n is TirLambda)
                return;
            if (n is TirCall call && IsCandidate(call))
            {
                var key = Key(call);
                if (key != null)
                {
                    if (!sites.TryGetValue(key, out var site))
                    {
                        sites[key] = site = new Site { Size = _facts.Size(call), First = order };
                        samples[key] = call;
                    }
                    site.Count++;
                    if (strict)
                        site.Strict++;
                }
            }
            order++;
            if (n is TirConditional conditional)
            {
                Collect(conditional.Condition, strict, sites, samples, ref order);
                Collect(conditional.IfTrue, false, sites, samples, ref order);
                Collect(conditional.IfFalse, false, sites, samples, ref order);
                return;
            }
            var childrenStrict = strict && n is TirCall or TirCoerce or TirInvoke or TirNew or TirArray
                or TirConstructorCall or TirComponentAccess;
            foreach (var child in n.Children)
                Collect(child, childrenStrict, sites, samples, ref order);
        }

        /// <summary>A method call whose result is worth keeping: not an array (the loop passes own those) and
        /// of no affine type.</summary>
        private static bool IsCandidate(TirCall c)
            => c.Callee != null
               && c.EmissionKind is EmissionKind.InstanceMethod or EmissionKind.StaticMethod
               && c.Type != null && !TirRewrite.IsListType(c.Type) && !UniqueTypes.IsUniqueType(c.Type);

        /// <summary>Structural identity of a candidate, or null when something below it is not known to be pure
        /// and repeatable; memoized per node by the body's fact table. When the key is not null, the definitions
        /// it reads are exactly <c>_facts.MentionedDefs(call)</c>: a key covers every leaf of the call or is null.</summary>
        private string Key(TirCall call) => _facts.StructuralKey(call, _wide);

        // --- --share-calls-wide: across the statements of a block, and inside lambda bodies ---------

        /// <summary>The block's statements with a call that one of them evaluates on every path, and that a
        /// later statement (or a lambda or branch of its own) names again, bound once before the first.</summary>
        public TirNode Statements(TirNode body)
        {
            if (body is not TirBlock block || _facts.ContainsAssign(body) || _facts.ContainsLoop(body))
                return body;
            return Block(block, new HashSet<object>(_parameters, ReferenceEqualityComparer.Instance));
        }

        private TirNode Block(TirBlock block, HashSet<object> outer)
        {
            var available = new HashSet<object>(outer, ReferenceEqualityComparer.Instance);
            var list = block.Statements.ToList();
            for (var a = 0; a < list.Count; a++)
            {
                // The binding a round inserts is list[a]; it may itself hold a shared call.
                while (SharedBefore(list, a, available))
                {
                }
                list[a] = Nested(list[a], available);
                if (list[a] is TirLet l && l.Def != null)
                    available.Add(l.Def);
                else if (list[a] is TirInlineBinding b)
                    available.Add(b.Def);
            }
            return new TirBlock(list, block.Origin);
        }

        private TirNode Nested(TirNode statement, HashSet<object> available)
        {
            TirNode Branch(TirNode s) => s is TirBlock b ? Block(b, available) : s;
            return statement switch
            {
                TirBlock b => Block(b, available),
                TirIf i => new TirIf(i.Condition, Branch(i.IfTrue), Branch(i.IfFalse), i.Origin),
                _ => statement
            };
        }

        private static IEnumerable<TirNode> OwnExpressions(TirNode statement)
            => statement switch
            {
                TirLet l => new[] { l.Value },
                TirReturn r => new[] { r.Value },
                TirIf i => new[] { i.Condition },
                TirInlineBinding b => new[] { b.Value },
                _ => Enumerable.Empty<TirNode>()
            };

        /// <summary>Binds one call that statement <paramref name="a"/> evaluates on every path and that the
        /// statements from <paramref name="a"/> on name at least twice, in a local inserted at
        /// <paramref name="a"/>; false when there is none.</summary>
        private bool SharedBefore(List<TirNode> list, int a, HashSet<object> available)
        {
            var sites = new Dictionary<string, Site>();
            var samples = new Dictionary<string, TirCall>();
            var order = 0;
            foreach (var expression in OwnExpressions(list[a]))
                Collect(expression, true, sites, samples, ref order);
            foreach (var key in sites.Where(s => s.Value.Strict >= 1).OrderByDescending(s => s.Value.Size)
                         .ThenBy(s => s.Value.First).Select(s => s.Key).ToList())
            {
                var sample = samples[key];
                if (!_facts.MentionedDefs(sample).All(available.Contains))
                    continue;
                var total = 0;
                for (var k = a; k < list.Count; k++)
                    total += CountAll(list[k], key);
                // Repeats inside the one expression are the single-statement pass's; this binds the others.
                if (total < 2 || sites[key].Count == total)
                    continue;
                var def = Fresh(sample.Type);
                var reference = new TirVariable(def, def.Type, sample.Origin);
                for (var k = a; k < list.Count; k++)
                    list[k] = ReplaceEverywhere(list[k], key, reference);
                list.Insert(a, new TirInlineBinding(def, sample));
                Interlocked.Increment(ref _stats.SharedAcrossStatements);
                return true;
            }
            return false;
        }

        private int CountAll(TirNode root, string key)
        {
            var count = 0;
            void Walk(TirNode n)
            {
                if (n == null)
                    return;
                if (n is TirCall c && IsCandidate(c) && Key(c) == key)
                    count++;
                foreach (var child in n.Children)
                    Walk(child);
            }
            Walk(root);
            return count;
        }

        private TirNode ReplaceEverywhere(TirNode root, string key, TirNode reference)
            => TirRewrite.Rewrite(root, n => n is TirCall c && IsCandidate(c) && Key(c) == key ? reference : n);

        /// <summary>Each lambda body as its own expression: a call the body repeats is bound once inside it
        /// (an expression body becomes a block body when it needs a local).</summary>
        public TirNode Lambdas(TirNode body)
            => body == null ? null : TirRewrite.Rewrite(body, n =>
            {
                if (n is not TirLambda lambda || lambda.Body == null)
                    return n;
                var rewritten = Body(lambda.Body);
                if (ReferenceEquals(rewritten, lambda.Body))
                    return n;
                Interlocked.Add(ref _stats.SharedInLambdas, rewritten.Descendants().Count(d => d is TirInlineBinding b && b.Def.Name.StartsWith("_shared"))
                    - lambda.Body.Descendants().Count(d => d is TirInlineBinding b && b.Def.Name.StartsWith("_shared")));
                return new TirLambda(lambda.Parameters, rewritten, lambda.Type, lambda.Origin);
            });
    }
}
