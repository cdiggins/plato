// Refines a sum-typed parameter or local to the cases it can still be inside each match arm, and folds the case tests that
// knowledge decides (--refine-sums).
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The first refinement on TIR (docs/plans/tir-partial-evaluation-2026-11-07.md in plato-brep, step 1). A Plato
/// <c>match</c> lowers to a chain of case predicates, <c>IsA(s) ? x : IsB(s) ? y : z</c>, and the arm bodies see
/// <c>s</c> with the same type as anywhere else, so a second match on <c>s</c> inside an arm, which inlining produces
/// whenever an evaluator calls another evaluator on its own subject, tests the tag again. This pass walks each body with
/// an environment from a parameter or local to the set of cases it can still be: a case test narrows its subject to that
/// case in the true arm and removes it in the false arm (through <c>&amp;&amp;</c> and <c>||</c>, which are conditionals
/// with a Boolean literal arm), a local bound to a case constructor or to a refined name takes that set, and a case test
/// on a name whose set decides it becomes a Boolean literal, after which the conditional or <c>if</c> on it keeps the one
/// live arm. The knowledge is semantic only in a pure language: a name that is assigned anywhere in the body (the loop
/// lowerer's locals) is never refined, and a lambda body sees its captures' sets because they are the same values.
/// </summary>
/// <remarks>
/// What it does not do yet: refine a projection such as <c>g.Surfaces[s.Value]</c> (a value number for pure reads, step 2
/// of the report), carry a set across a call (polyvariant clones, step 3), or touch any type other than a sum. A sum with
/// more than 64 cases is left alone. Results are unchanged by construction: a test is folded only when every path to it
/// has established the answer, and nothing else is rewritten.
/// </remarks>
public static class TirSumRefinement
{
    public const string Switch = "refine-sums";

    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On(Switch) || tir?.Body == null)
            return tir;
        var walker = new Walker(tir.Body, pass.Compilation);
        var body = walker.Walk(tir.Body, ImmutableDictionary<object, ulong>.Empty);
        Interlocked.Add(ref pass.Stats.Rewrites.SumTestsDecided, walker.Decided);
        Interlocked.Add(ref pass.Stats.Rewrites.SumArmsFolded, walker.Folded);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body, tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    /// <summary>A case test the elaborator made: a null-callee property call named after one case of its one argument's sum type,
    /// the argument a parameter or local (through coercions). The caller's writers read the same shape (TKT-1204).</summary>
    public static bool TryReadCaseTest(TirNode node, out object subject, out TypeDef sum, out SumCaseDef tested)
    {
        subject = null;
        sum = null;
        tested = null;
        if (node is not TirCall { Callee: null, EmissionKind: EmissionKind.Property } call || call.Args.Count != 1 || call.Name == null)
            return false;
        var receiver = TirRewrite.StripCoerce(call.Args[0]);
        subject = receiver is TirParameter p ? p.Def : receiver is TirVariable v ? (object)v.Def : null;
        sum = receiver?.Type?.Def;
        if (subject == null || sum == null || !sum.IsSum || sum.Cases.Count > 64)
            return false;
        tested = sum.Cases.FirstOrDefault(c => c.PredicateName == call.Name);
        return tested != null;
    }

    private sealed class Walker
    {
        private readonly HashSet<object> _assigned = new();
        private readonly TypeExpression _boolType;
        public int Decided;
        public int Folded;

        public Walker(TirNode body, Compiler.Compilation compilation)
        {
            foreach (var n in body.Descendants())
                if (n is TirAssign a)
                {
                    var target = TirRewrite.StripCoerce(a.LValue);
                    if (target is TirVariable v) _assigned.Add(v.Def);
                    else if (target is TirParameter p) _assigned.Add(p.Def);
                }
            _boolType = compilation?.GetTypeDef("Boolean")?.ToTypeExpression();
        }

        private static ulong All(TypeDef sum) => sum.Cases.Count == 64 ? ulong.MaxValue : (1UL << sum.Cases.Count) - 1;
        private static ulong Bit(SumCaseDef c) => 1UL << c.Tag;

        private ulong SetOf(ImmutableDictionary<object, ulong> env, object subject, TypeDef sum)
            => env.TryGetValue(subject, out var s) ? s : All(sum);

        /// <summary>The refinements that hold when <paramref name="cond"/> is true (<paramref name="whenTrue"/>) or false.</summary>
        private ImmutableDictionary<object, ulong> Facts(TirNode cond, ImmutableDictionary<object, ulong> env, bool whenTrue)
        {
            if (TryReadCaseTest(cond, out var subject, out var sum, out var tested) && !_assigned.Contains(subject))
            {
                var current = SetOf(env, subject, sum);
                return env.SetItem(subject, whenTrue ? current & Bit(tested) : current & ~Bit(tested));
            }
            if (cond is TirConditional c)
            {
                var op = TirShortCircuit.TryMatch(c, out var left, out var right);
                if (op == "&&" && whenTrue)
                    return Facts(right, Facts(left, env, true), true);
                if (op == "||" && !whenTrue)
                    return Facts(right, Facts(left, env, false), false);
            }
            return env;
        }

        private TirNode Decide(TirNode cond, ImmutableDictionary<object, ulong> env)
        {
            if (!TryReadCaseTest(cond, out var subject, out var sum, out var tested) || _assigned.Contains(subject))
                return cond;
            var current = SetOf(env, subject, sum);
            var bit = Bit(tested);
            bool value;
            if ((current & bit) == 0) value = false;
            else if (current == bit) value = true;
            else return cond;
            var type = cond.Type ?? _boolType;
            if (type == null)
                return cond;
            Decided++;
            return new TirLiteral(value, LiteralTypesEnum.Boolean, type, cond.Origin);
        }

        private static bool IsBoolean(TirNode n, out bool value)
        {
            value = false;
            if (n is TirLiteral { LiteralType: LiteralTypesEnum.Boolean, Value: bool b })
            {
                value = b;
                return true;
            }
            return false;
        }

        public TirNode Walk(TirNode node, ImmutableDictionary<object, ulong> env)
        {
            switch (node)
            {
                case null:
                    return null;
                case TirConditional c:
                {
                    var cond = Decide(Walk(c.Condition, env), env);
                    if (IsBoolean(cond, out var known))
                    {
                        Folded++;
                        return Walk(known ? c.IfTrue : c.IfFalse, env);
                    }
                    var t = Walk(c.IfTrue, Facts(cond, env, true));
                    var f = Walk(c.IfFalse, Facts(cond, env, false));
                    return ReferenceEquals(cond, c.Condition) && ReferenceEquals(t, c.IfTrue) && ReferenceEquals(f, c.IfFalse)
                        ? c : new TirConditional(cond, t, f, c.Type, c.Origin);
                }
                case TirIf s:
                {
                    var cond = Decide(Walk(s.Condition, env), env);
                    if (IsBoolean(cond, out var known))
                    {
                        Folded++;
                        var kept = known ? s.IfTrue : s.IfFalse;
                        return kept == null ? new TirBlock(new List<TirNode>(), s.Origin) : Walk(kept, env);
                    }
                    var t = Walk(s.IfTrue, Facts(cond, env, true));
                    var f = Walk(s.IfFalse, Facts(cond, env, false));
                    return ReferenceEquals(cond, s.Condition) && ReferenceEquals(t, s.IfTrue) && ReferenceEquals(f, s.IfFalse)
                        ? s : new TirIf(cond, t, f, s.Origin);
                }
                case TirBlock b:
                {
                    List<TirNode> rebuilt = null;
                    var scope = env;
                    for (var i = 0; i < b.Statements.Count; i++)
                    {
                        var st = b.Statements[i];
                        var walked = Walk(st, scope);
                        if (walked is TirLet let && !_assigned.Contains(let.Def))
                            scope = Bind(scope, let.Def, let.Value, let.Type);
                        if (!ReferenceEquals(walked, st))
                        {
                            rebuilt ??= b.Statements.ToList();
                            rebuilt[i] = walked;
                        }
                    }
                    return rebuilt == null ? b : new TirBlock(rebuilt, b.Origin);
                }
                default:
                    return RewriteChildren(node, env);
            }
        }

        /// <summary>The environment after <c>var def = value</c>: a case constructor fixes the case, a refined name passes its set on.</summary>
        private ImmutableDictionary<object, ulong> Bind(ImmutableDictionary<object, ulong> env, object def, TirNode value, TypeExpression type)
        {
            var sum = type?.Def ?? value?.Type?.Def;
            if (sum == null || !sum.IsSum || sum.Cases.Count > 64)
                return env;
            var v = TirRewrite.StripCoerce(value);
            if (v is TirCall { Callee: null, EmissionKind: EmissionKind.StaticMethod } make && make.Name != null)
            {
                var made = sum.Cases.FirstOrDefault(c => c.Name == make.Name);
                if (made != null)
                    return env.SetItem(def, Bit(made));
            }
            var source = v is TirParameter p ? p.Def : v is TirVariable vv ? (object)vv.Def : null;
            if (source != null && env.TryGetValue(source, out var set))
                return env.SetItem(def, set);
            return env;
        }

        private TirNode RewriteChildren(TirNode node, ImmutableDictionary<object, ulong> env)
        {
            var children = node.Children.ToArray();
            TirNode[] replacements = null;
            for (var i = 0; i < children.Length; i++)
            {
                var child = Walk(children[i], env);
                if (ReferenceEquals(child, children[i]))
                    continue;
                replacements ??= (TirNode[])children.Clone();
                replacements[i] = child;
            }
            return replacements == null ? node : node.WithChildren(replacements);
        }
    }
}
