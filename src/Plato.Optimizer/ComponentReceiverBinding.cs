// Binds a computed ComponentArray receiver to a local so the constant-counts fold applies (--scalar-arrays, TKT-1187).
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// <see cref="ConstantCounts"/> exposes <c>ComponentArray(v)</c> as the literal <c>[v.X, v.Y, v.Z]</c> only when
/// <c>v</c> is a leaf: a computed receiver such as <c>Subtract(p, q)</c> would be evaluated once per component.
/// Most sites in the library are computed (<c>(b - a).ReduceComponents(...)</c> in BoundsSize, Centroid and the
/// closest-point distances), so this pass, run just before it, evaluates such a receiver once in a local declared
/// immediately before its statement, and the fold then reads the local's fields.
///
/// A receiver is bound when its <c>ComponentArray</c> call is a source of a consumer the fold unrolls (<c>Map</c>,
/// <c>Zip</c>, <c>Reduce</c>, <c>All</c>, <c>Any</c>) and that consumer is evaluated on every path through the
/// statement's expression (an operand of calls, constructors and conversions, or a condition, never a branch of a
/// conditional and never inside a lambda), so evaluating the receiver first adds no work and cannot raise an error
/// the statement would not have raised; an expression that assigns is left alone. A receiver that holds such a site
/// of its own gets its local first. Lambda bodies are not entered: their statements would need a block body,
/// which the fusion pass after this one does not compose.
///
/// The Cpu pass list names this pass and the C# list does not, so the shipped C# is unchanged.
/// </summary>
public static class ComponentReceiverBinding
{
    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On("scalar-arrays") || tir?.Body == null)
            return tir;
        var binder = new Binder(tir, pass.Compilation);
        var body = binder.Body(tir.Body);
        return binder.Bound == 0
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    /// <summary>The argument positions of <paramref name="c"/> that the fold reads as arrays, or null.</summary>
    private static int[] Sources(TirCall c)
        => c.Name switch
        {
            "Map" when c.Args.Count == 2 => new[] { 0 },
            "Zip" when c.Args.Count == 3 || c.Args.Count == 4 => Enumerable.Range(0, c.Args.Count - 1).ToArray(),
            "Reduce" when c.Args.Count == 3 => new[] { 0 },
            "All" or "Any" when c.Args.Count == 2 => new[] { 0 },
            _ => null,
        };

    private sealed class Binder
    {
        private readonly Ara3D.Geometry.Compiler.Compilation _compilation;
        private readonly HashSet<string> _names;
        private int _next;

        public int Bound { get; private set; }

        public Binder(TirFunction function, Ara3D.Geometry.Compiler.Compilation compilation)
        {
            _compilation = compilation;
            _names = function.AllNodes.Select(n => n switch
            {
                TirVariable v => v.Def?.Name, TirParameter p => p.Def?.Name,
                TirLet l => l.Def?.Name, TirInlineBinding b => b.Def.Name,
                TirInlineRegion r => r.Result.Name, TirTempRef t => t.Name, _ => null
            }).Concat(function.Parameters.Select(p => p.Name))
                .Concat(function.AllNodes.OfType<TirLambda>().SelectMany(l => l.Parameters).Select(p => p.Name))
                .Where(n => n != null).ToHashSet();
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

        private TirNode Value(TirNode expression, List<TirNode> before)
            => expression == null || expression.Descendants().Any(n => n is TirAssign)
                ? expression
                : Walk(expression, true, before);

        // Children first, left to right, so a receiver's own sites are bound before it.
        private TirNode Walk(TirNode n, bool strict, List<TirNode> before)
        {
            if (n == null || n is TirLambda)
                return n;
            if (n is TirConditional conditional)
            {
                var condition = Walk(conditional.Condition, strict, before);
                return ReferenceEquals(condition, conditional.Condition) ? n
                    : new TirConditional(condition, conditional.IfTrue, conditional.IfFalse, conditional.Type, conditional.Origin);
            }
            var childrenStrict = strict && n is TirCall or TirCoerce or TirInvoke or TirNew or TirArray
                or TirConstructorCall or TirComponentAccess;
            var children = n.Children.ToList();
            var changed = false;
            for (var i = 0; i < children.Count; i++)
            {
                var child = Walk(children[i], childrenStrict, before);
                changed |= !ReferenceEquals(child, children[i]);
                children[i] = child;
            }
            if (changed)
                n = n.WithChildren(children);
            if (!strict || n is not TirCall call || Sources(call) is not { } sources)
                return n;
            var args = call.Args.ToList();
            var bound = false;
            foreach (var i in sources)
            {
                if (ConstantCounts.ComputedReceiver(_compilation, args[i]) is not { } receiver)
                    continue;
                var def = Fresh(receiver.Type);
                before.Add(new TirInlineBinding(def, receiver));
                args[i] = Replace(args[i], receiver, new TirVariable(def, def.Type, receiver.Origin));
                bound = true;
                Bound++;
            }
            return bound
                ? new TirCall(call.Callee, call.EmissionKind, call.ParameterTypes, call.ReturnType, args, call.Type, call.Origin, call.Name)
                : n;
        }

        // The tree with the one node <paramref name="target"/> replaced, rebuilt along the path to it only.
        private static TirNode Replace(TirNode root, TirNode target, TirNode with)
        {
            if (ReferenceEquals(root, target))
                return with;
            if (root == null)
                return null;
            var children = root.Children.ToList();
            for (var i = 0; i < children.Count; i++)
            {
                var child = Replace(children[i], target, with);
                if (!ReferenceEquals(child, children[i]))
                {
                    children[i] = child;
                    return root.WithChildren(children);
                }
            }
            return root;
        }

        private VariableDef Fresh(TypeExpression type)
        {
            string name;
            do name = $"_cr{_next++}"; while (!_names.Add(name));
            return new VariableDef(null, name, type, null);
        }
    }
}
