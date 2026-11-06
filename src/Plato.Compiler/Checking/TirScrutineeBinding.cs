// Binds the scrutinee of a match to one local (TKT-1138, TKT-1199).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The elaborator lowers <c>match (E)</c> to a tag test and one field read per case, and each of them
    /// holds the same node E. When E is a call it would run two to five times, in the interpreter as in the
    /// compiled code. Plato is pure, so this pass evaluates E once: it declares
    /// <c>var _scrutN = E;</c> immediately before the statement whose expression holds the match and
    /// replaces every occurrence of E by the local.
    ///
    /// A scrutinee is bound only where evaluating it first adds no work: it must be evaluated on every
    /// path through the statement's expression (an operand of calls, constructors and conversions, the
    /// condition of a conditional, never a branch of one, never inside a lambda), and the expression
    /// must assign nothing. The statements considered are those of the function body reached through
    /// blocks and <c>if</c> branches: a return, a local, an <c>if</c> condition, and a body that is one
    /// expression (which becomes a block ending in a return).
    ///
    /// A match that is an arm of a conditional (<c>c ? x : match (E) { ... }</c>, the shape of an
    /// expression-bodied guard, TKT-1199) is not evaluated on every path, so its scrutinee cannot be
    /// declared before the statement. When the statement is a return, the conditional becomes an
    /// <c>if</c> whose branches return and the arm that holds the match declares the local:
    /// <c>if (c) { return x; } else { var _scrutN = E; return ...; }</c>. The tag chain of a match is
    /// itself a conditional, so a match in the arm of a match is reached the same way. A match in the
    /// arm of a conditional that is the value of a local, or of an <c>if</c> condition, keeps its
    /// scrutinee as written: no local can be declared there.
    /// </summary>
    internal sealed class TirScrutineeBinding
    {
        private readonly HashSet<TirNode> _scrutinees;
        private readonly ElaborationStats _stats;
        private int _next;

        public TirScrutineeBinding(HashSet<TirNode> scrutinees, ElaborationStats stats)
            => (_scrutinees, _stats) = (scrutinees, stats);

        /// <summary>A scrutinee that costs nothing to repeat: a variable, a parameter, a literal or a
        /// field read of one.</summary>
        public static bool IsTrivial(TirNode n)
        {
            while (n is TirCoerce c)
                n = c.Inner;
            return n is TirParameter or TirVariable or TirLiteral or TirTypeRef or TirDefault
                || n is TirCall { EmissionKind: EmissionKind.Property } call && call.Args.Count == 1 && IsTrivial(call.Args[0]);
        }

        public TirNode Body(TirNode body)
        {
            if (body == null || _scrutinees.Count == 0)
                return body;
            if (body is TirBlock or TirReturn or TirIf or TirLoop)
                return One(Statement(body));
            // A body that is one expression is the value of its return.
            var statements = Statement(new TirReturn(body, body.Origin));
            if (statements.Count == 1 && statements[0] is TirReturn r && ReferenceEquals(r.Value, body))
                return body;
            return statements.Count == 1 && statements[0] is TirBlock ? statements[0] : new TirBlock(statements.ToList(), body.Origin);
        }

        private static TirNode One(IReadOnlyList<TirNode> statements)
            => statements.Count == 1 ? statements[0] : new TirBlock(statements.ToList(), statements[0].Origin);

        private IReadOnlyList<TirNode> Statement(TirNode s)
        {
            var before = new List<TirNode>();
            switch (s)
            {
                case TirBlock b:
                    return new[] { new TirBlock(b.Statements.SelectMany(Statement).ToList(), b.Origin) };
                case TirReturn r:
                {
                    var value = Hoist(r.Value, before);
                    if (value is TirConditional c && !Assigns(value) && (WouldBind(c.IfTrue) || WouldBind(c.IfFalse)))
                        before.Add(new TirIf(c.Condition, Arm(c.IfTrue, r), Arm(c.IfFalse, r), r.Origin));
                    else
                        before.Add(ReferenceEquals(value, r.Value) ? r : new TirReturn(value, r.Origin));
                    return before;
                }
                case TirLet l:
                {
                    var value = Hoist(l.Value, before);
                    before.Add(ReferenceEquals(value, l.Value) ? l : new TirLet(l.Def, value, l.Type, l.Origin));
                    return before;
                }
                case TirIf i:
                {
                    var condition = Hoist(i.Condition, before);
                    before.Add(new TirIf(condition, Branch(i.IfTrue), Branch(i.IfFalse), i.Origin));
                    return before;
                }
                case TirLoop l:
                    return new[] { new TirLoop(l.Condition, Branch(l.Body), l.Origin) };
                default:
                    return new[] { s };
            }
        }

        private TirNode Branch(TirNode s) => s == null ? null : One(Statement(s));

        /// <summary>An arm of a returned conditional as a branch of the <c>if</c> it becomes: a return of the
        /// arm's value, with the scrutinees the arm evaluates on every path bound before it.</summary>
        private TirNode Arm(TirNode value, TirReturn of) => One(Statement(new TirReturn(value, of.Origin)));

        private static bool Assigns(TirNode e) => e.Descendants().Any(n => n is TirAssign);

        /// <summary>Whether the expression, as the value of a return, declares a local: it holds a scrutinee
        /// on every path, or it is a conditional with such an arm.</summary>
        private bool WouldBind(TirNode e)
        {
            if (e == null || Assigns(e))
                return false;
            var found = new List<TirNode>();
            Find(e, true, found);
            return found.Count > 0 || e is TirConditional c && (WouldBind(c.IfTrue) || WouldBind(c.IfFalse));
        }

        /// <summary>The expression with each strictly evaluated scrutinee replaced by a local; the locals
        /// are appended to <paramref name="before"/> in the order they must run.</summary>
        private TirNode Hoist(TirNode expression, List<TirNode> before, bool inside = false)
        {
            if (expression == null || Assigns(expression))
                return expression;
            var found = new List<TirNode>();
            Find(expression, true, found, inside);
            foreach (var scrutinee in found)
            {
                // What the scrutinee itself holds is bound first.
                var value = Hoist(scrutinee, before, inside: true);
                var def = new VariableDef(null, $"_scrut{_next++}", scrutinee.Type, null);
                before.Add(new TirLet(def, value, scrutinee.Type, scrutinee.Origin));
                var reference = new TirVariable(def, scrutinee.Type, scrutinee.Origin);
                expression = Replace(expression, scrutinee, reference);
                _stats.BoundScrutinees++;
            }
            return expression;
        }

        private void Find(TirNode n, bool strict, List<TirNode> found, bool root = false)
        {
            if (n == null || n is TirLambda)
                return;
            if (strict && !root && _scrutinees.Contains(n))
            {
                if (!found.Contains(n))
                    found.Add(n);
                return;
            }
            if (n is TirConditional c)
            {
                Find(c.Condition, strict, found);
                Find(c.IfTrue, false, found);
                Find(c.IfFalse, false, found);
                return;
            }
            foreach (var child in n.Children)
                Find(child, strict, found);
        }

        private TirNode Replace(TirNode n, TirNode target, TirNode reference)
            => Replace(n, target, reference, new Dictionary<TirNode, TirNode>(ReferenceEqualityComparer.Instance));

        // A node the elaborator shares between positions (a match's subject, a binder's field read) is
        // rebuilt once and shared again, so a scrutinee that holds the replaced one (a match in the arm
        // of a match, on a field of the outer binder) keeps one identity, and that identity joins the
        // set so a later statement still finds it.
        private TirNode Replace(TirNode n, TirNode target, TirNode reference, Dictionary<TirNode, TirNode> rebuilt)
        {
            if (n == null)
                return null;
            if (ReferenceEquals(n, target))
                return reference;
            if (rebuilt.TryGetValue(n, out var done))
                return done;
            var children = n.Children.ToArray();
            TirNode[] replaced = null;
            for (var i = 0; i < children.Length; i++)
            {
                var child = Replace(children[i], target, reference, rebuilt);
                if (ReferenceEquals(child, children[i]))
                    continue;
                replaced ??= (TirNode[])children.Clone();
                replaced[i] = child;
            }
            var result = replaced == null ? n : n.WithChildren(replaced);
            if (replaced != null && _scrutinees.Contains(n))
                _scrutinees.Add(result);
            rebuilt[n] = result;
            return result;
        }
    }
}
