// Evaluates a local only on the arm of a conditional that uses it (TKT-1142).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// <c>var basis = depth &gt; 1 ? F(depth - 1) : plain;</c> followed by a return chain that names
    /// <c>basis</c> in one arm only evaluates <c>basis</c> on every call. Plato is pure, so a local that
    /// no other path reads can be evaluated where it is read. This pass moves each local down to the
    /// statement that reads it, and from there into the arm that reads it:
    /// <list type="bullet">
    /// <item>into the one branch of an <c>if</c> that reads it, as the first statement of that branch;</item>
    /// <item>through the arms of a conditional expression, where a single read takes the value in its
    /// place (<c>c ? a : (d ? basis : k)</c> becomes <c>c ? a : (d ? F(depth - 1) : k)</c>);</item>
    /// <item>for a <c>return</c> of a conditional whose arm reads it more than once, by turning the
    /// return into an <c>if</c> whose arm declares the local.</item>
    /// </list>
    /// A local stays where it is when it is read on every path (in a condition, or in no conditional's arm),
    /// in more than one statement, in both arms, or under a lambda (where moving it would change how often
    /// it is evaluated); when it is a variable, a literal or a lambda, or has an affine builder type; and
    /// when its name is declared twice in the function (a move into a nested block could capture a
    /// name). A function that assigns or loops is left alone.
    ///
    /// Sinking only removes evaluation. An error or a nonterminating computation in a value that no
    /// path read stops being raised, so a program that failed in a never-read local now returns; the
    /// interpreter runs the same pass (<see cref="ElaborationOptions.SinkBindings"/>), so the engines
    /// agree.
    /// </summary>
    public static class TirSinkBindings
    {
        /// <summary>The body with its sinkable locals moved; <paramref name="sunk"/> counts the locals moved.</summary>
        public static TirNode Rewrite(TirNode body, out int sunk)
        {
            sunk = 0;
            if (body == null || body.Descendants().Any(n => n is TirAssign or TirLoop))
                return body;
            var nodes = body.Descendants().ToList();
            var duplicated = nodes.OfType<TirLet>().Select(l => l.Def?.Name)
                .Concat(nodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name))
                .Where(n => n != null).GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();
            var sinker = new Sinker(duplicated);
            var result = sinker.Statement(body);
            sunk = sinker.Sunk;
            return result;
        }

        private sealed class Sinker
        {
            private readonly HashSet<string> _duplicated;
            public int Sunk;

            public Sinker(HashSet<string> duplicated) => _duplicated = duplicated;

            public TirNode Statement(TirNode s)
            {
                switch (s)
                {
                    case TirBlock b:
                        return Block(b);
                    case TirIf i:
                        return new TirIf(i.Condition, Statement(i.IfTrue), Statement(i.IfFalse), i.Origin);
                    default:
                        return s;
                }
            }

            private TirNode Block(TirBlock block)
            {
                var list = block.Statements.ToList();
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] is not TirLet let || !Sinkable(let))
                        continue;
                    var readers = Enumerable.Range(i + 1, list.Count - i - 1).Where(k => Mentions(list[k], let.Def)).ToList();
                    if (readers.Count != 1)
                        continue;
                    var moved = Place(list[readers[0]], let);
                    if (moved == null)
                        continue;
                    list[readers[0]] = moved;
                    list.RemoveAt(i);
                    Sunk++;
                }
                for (var k = 0; k < list.Count; k++)
                    list[k] = Statement(list[k]);
                return new TirBlock(list, block.Origin);
            }

            private bool Sinkable(TirLet let)
            {
                var value = let.Value;
                if (value == null || let.Def == null || _duplicated.Contains(let.Def.Name))
                    return false;
                var plain = value;
                while (plain is TirCoerce c)
                    plain = c.Inner;
                if (plain is TirVariable or TirParameter or TirLiteral or TirTypeRef or TirDefault or TirLambda)
                    return false;
                return !IsUnique(let.Def.Type) && !IsUnique(value.Type);
            }

            private static bool IsUnique(TypeExpression t)
                => t != null && ((t.Def?.IsUnique ?? false) || t.TypeArgs.Any(IsUnique));

            /// <summary>The statement with the local moved into it, or null when it cannot go any deeper
            /// than before it.</summary>
            private TirNode Place(TirNode statement, TirLet let, bool underArm = false)
            {
                switch (statement)
                {
                    case TirReturn r:
                    {
                        var value = PlaceInExpression(r.Value, let, underArm);
                        if (value != null)
                            return new TirReturn(value, r.Origin);
                        return ReturnAsIf(r, let);
                    }
                    case TirLet l:
                    {
                        var value = PlaceInExpression(l.Value, let, false);
                        return value == null ? null : new TirLet(l.Def, value, l.Type, l.Origin);
                    }
                    case TirIf i:
                    {
                        if (Mentions(i.Condition, let.Def))
                            return null;
                        var inTrue = Mentions(i.IfTrue, let.Def);
                        var inFalse = Mentions(i.IfFalse, let.Def);
                        if (inTrue == inFalse)
                            return null;
                        return inTrue
                            ? new TirIf(i.Condition, Prepend(let, i.IfTrue), i.IfFalse, i.Origin)
                            : new TirIf(i.Condition, i.IfTrue, Prepend(let, i.IfFalse), i.Origin);
                    }
                    default:
                        return null;
                }
            }

            private static TirNode Prepend(TirLet let, TirNode branch)
                => branch is TirBlock b
                    ? new TirBlock(b.Statements.Prepend(let).ToList(), b.Origin)
                    : new TirBlock(new[] { let, branch }, branch.Origin);

            /// <summary><c>return c ? a : b</c> with the local read by one arm only, as an <c>if</c>.</summary>
            private TirNode ReturnAsIf(TirReturn r, TirLet let)
            {
                if (r.Value is not TirConditional c || Mentions(c.Condition, let.Def))
                    return null;
                var inTrue = Mentions(c.IfTrue, let.Def);
                var inFalse = Mentions(c.IfFalse, let.Def);
                if (inTrue == inFalse)
                    return null;
                TirNode Arm(TirNode value, bool reads)
                {
                    var arm = new TirReturn(value, r.Origin);
                    return reads ? Place(arm, let, true) ?? Prepend(let, arm) : arm;
                }
                return new TirIf(c.Condition, Arm(c.IfTrue, inTrue), Arm(c.IfFalse, inFalse), r.Origin);
            }

            /// <summary>The expression with the single read of the local replaced by its value, when the read
            /// is under an arm of a conditional; null otherwise.</summary>
            private static TirNode PlaceInExpression(TirNode e, TirLet let, bool underArm)
            {
                if (e == null)
                    return null;
                if (e is TirVariable v && v.Def == let.Def)
                    return underArm ? let.Value : null;
                if (e is TirLambda)
                    return null;
                if (e is TirConditional c)
                {
                    if (Mentions(c.Condition, let.Def))
                        return null;
                    var inTrue = Mentions(c.IfTrue, let.Def);
                    var inFalse = Mentions(c.IfFalse, let.Def);
                    if (inTrue == inFalse)
                        return null;
                    var arm = PlaceInExpression(inTrue ? c.IfTrue : c.IfFalse, let, true);
                    if (arm == null)
                        return null;
                    return inTrue
                        ? new TirConditional(c.Condition, arm, c.IfFalse, c.Type, c.Origin)
                        : new TirConditional(c.Condition, c.IfTrue, arm, c.Type, c.Origin);
                }
                var children = e.Children.ToArray();
                var reading = Enumerable.Range(0, children.Length).Where(k => Mentions(children[k], let.Def)).ToList();
                if (reading.Count != 1)
                    return null;
                var inner = PlaceInExpression(children[reading[0]], let, underArm);
                if (inner == null)
                    return null;
                children[reading[0]] = inner;
                var rebuilt = e.WithChildren(children);
                return Mentions(rebuilt, let.Def) ? null : rebuilt;
            }

            private static bool Mentions(TirNode root, VariableDef def)
                => root != null && root.Descendants().Any(n => n is TirVariable v && ReferenceEquals(v.Def, def));
        }
    }
}
