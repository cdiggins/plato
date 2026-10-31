// Binds the scrutinee of a match to one local (TKT-1138).
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
    /// must assign nothing. A match inside the arm of another match, or of a conditional, keeps its
    /// scrutinee as written. The statements considered are those of the function body reached through
    /// blocks and <c>if</c> branches: a return, a local, an <c>if</c> condition, and a body that is one
    /// expression (which becomes a block ending in a return).
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
            var before = new List<TirNode>();
            var value = Hoist(body, before);
            return before.Count == 0 ? body : new TirBlock(before.Append(new TirReturn(value, body.Origin)).ToList(), body.Origin);
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

        /// <summary>The expression with each strictly evaluated scrutinee replaced by a local; the locals
        /// are appended to <paramref name="before"/> in the order they must run.</summary>
        private TirNode Hoist(TirNode expression, List<TirNode> before, bool inside = false)
        {
            if (expression == null || expression.Descendants().Any(n => n is TirAssign))
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

        private static TirNode Replace(TirNode n, TirNode target, TirNode reference)
        {
            if (n == null)
                return null;
            if (ReferenceEquals(n, target))
                return reference;
            var children = n.Children.ToArray();
            TirNode[] replaced = null;
            for (var i = 0; i < children.Length; i++)
            {
                var child = Replace(children[i], target, reference);
                if (ReferenceEquals(child, children[i]))
                    continue;
                replaced ??= (TirNode[])children.Clone();
                replaced[i] = child;
            }
            return replaced == null ? n : n.WithChildren(replaced);
        }
    }
}
