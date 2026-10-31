// Evaluates a call that a statement repeats once (--share-calls, TKT-1136).
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.CSharpWriter;

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
/// </summary>
public static class TirSharedCalls
{
    public static TirFunction Rewrite(TirFunction tir, CSharpWriter writer)
    {
        if (!writer.ShareCalls || tir?.Body == null)
            return tir;
        var body = new Sharing(tir).Body(tir.Body);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private sealed class Sharing
    {
        private readonly HashSet<string> _names;
        private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);
        private int _next;

        public Sharing(TirFunction function)
        {
            _names = function.AllNodes.Select(n => n switch
            {
                TirVariable v => v.Def?.Name, TirParameter p => p.Def?.Name,
                TirLet l => l.Def?.Name, TirInlineBinding b => b.Def.Name,
                TirInlineRegion r => r.Result.Name, TirTempRef t => t.Name, _ => null
            }).Concat(function.Parameters.Select(p => p.Name))
                .Concat(function.AllNodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name))
                .Where(n => n != null).ToHashSet();
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
            if (expression == null || expression.Descendants().Any(n => n is TirAssign))
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
                        sites[key] = site = new Site { Size = call.Descendants().Count(), First = order };
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
               && c.Type != null && !TirRewrite.IsListType(c.Type) && !IsUnique(c.Type);

        private static bool IsUnique(TypeExpression t)
            => t != null && ((t.Def?.IsUnique ?? false) || t.TypeArgs.Any(IsUnique));

        /// <summary>Structural identity of a candidate, or null when something below it is not known to be pure
        /// and repeatable.</summary>
        private string Key(TirCall call)
        {
            var sb = new StringBuilder();
            return Write(call, sb) ? sb.ToString() : null;
        }

        private int Id(object o)
        {
            if (!_ids.TryGetValue(o, out var id))
                _ids[o] = id = _ids.Count;
            return id;
        }

        private bool Write(TirNode n, StringBuilder sb)
        {
            switch (n)
            {
                case TirParameter p when p.Def != null && !IsUnique(p.Type):
                    sb.Append("P").Append(Id(p.Def));
                    return true;
                case TirVariable v when v.Def != null && !IsUnique(v.Type):
                    sb.Append("V").Append(Id(v.Def));
                    return true;
                case TirLiteral l:
                    sb.Append("L").Append(l.LiteralType).Append(':').Append(l.Type).Append(':').Append(l.Value);
                    return true;
                case TirTypeRef t:
                    sb.Append("T").Append(t.TypeDef?.Name).Append(t.NamespaceQualified);
                    return true;
                case TirDefault d:
                    sb.Append("D").Append(d.Type);
                    return true;
                case TirCoerce c:
                    sb.Append("K").Append(c.ToType).Append('(');
                    var ok = Write(c.Inner, sb);
                    sb.Append(')');
                    return ok;
                case TirCall c when c.Callee != null && !IsUnique(c.Type) && !c.Args.Any(a => IsUnique(a?.Type)):
                    sb.Append("C").Append(Id(c.Callee)).Append(':').Append(c.EmissionKind).Append(':').Append(c.Type).Append('(');
                    foreach (var a in c.Args)
                    {
                        if (a == null || !Write(a, sb))
                            return false;
                        sb.Append(',');
                    }
                    sb.Append(')');
                    return true;
                case TirCall c when c.Callee == null && c.EmissionKind == EmissionKind.Property
                                    && c.Args.Count == 1 && !IsUnique(c.Type):
                    // A syntactic field projection (a match binder or a case test) of a shared value.
                    sb.Append("F").Append(c.Name).Append(':').Append(c.Type).Append('(');
                    var inner = c.Args[0] != null && Write(c.Args[0], sb);
                    sb.Append(')');
                    return inner;
                default:
                    return false;
            }
        }
    }
}
