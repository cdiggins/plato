// Refines a sum-typed value to the cases it can still be inside each match arm, folds the case tests that knowledge
// decides, and expands a callee that is one match on a parameter when the argument's cases are known (--refine-sums).
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The first refinement on TIR (docs/plans/tir-partial-evaluation-2026-11-07.md in plato-brep, steps 1 and 2). A Plato
/// <c>match</c> lowers to a chain of case predicates, <c>IsA(s) ? x : IsB(s) ? y : z</c>, and the arm bodies see
/// <c>s</c> with the same type as anywhere else, so a second match on <c>s</c> inside an arm tests the tag again. This
/// pass walks each body with an environment from a <em>value</em> to the set of cases it can still be. A value is a
/// parameter or local, or a pure expression over them (a call, a field read, a literal), identified by its shape: Plato
/// is pure and the names are never reassigned, so two occurrences of <c>g.SurfaceAt(s)</c> are the same value. A case
/// test narrows its subject in the true arm and removes the case in the false arm (through <c>&amp;&amp;</c> and
/// <c>||</c>, which are conditionals with a Boolean literal arm); a local bound to a case constructor or to a refined
/// value takes its set; a case test on a value whose set decides it becomes a Boolean literal, after which the
/// conditional or <c>if</c> on it keeps the one live arm.
/// <para>
/// Knowledge crosses one call boundary by expansion: a call whose callee's body is a match on a parameter, where the
/// argument for that parameter is a value with a narrowed set, is expanded through the inliner with a larger body limit
/// (<see cref="TirInliner.RefinementBodyNodes"/>), and the match it brings in collapses to the live arm. That is how
/// <c>FirstJet</c>'s plane arm comes to hold the plane jet instead of a call to a ten-way dispatch. Depth and total
/// growth are bounded (<see cref="MaxInlineDepth"/>, <see cref="InlineNodeBudget"/>); calls under a lambda are not
/// expanded (the inliner's own under-lambda rule).
/// </para>
/// </summary>
/// <remarks>
/// A name assigned anywhere in the body (the loop lowerer's locals) is never part of a value. A sum with more than 64
/// cases is left alone. Results are unchanged by construction: a test is folded only when every path to it has
/// established the answer, expansion is the inliner's own substitution, and nothing else is rewritten.
/// </remarks>
public static class TirSumRefinement
{
    public const string Switch = "refine-sums";
    public const int MaxInlineDepth = 6;
    public const int InlineNodeBudget = 4096;

    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On(Switch) || tir?.Body == null)
            return tir;
        var walker = new Walker(tir, pass);
        var body = walker.Walk(tir.Body, ImmutableDictionary<string, ulong>.Empty);
        Interlocked.Add(ref pass.Stats.Rewrites.SumTestsDecided, walker.Decided);
        Interlocked.Add(ref pass.Stats.Rewrites.SumArmsFolded, walker.Folded);
        Interlocked.Add(ref pass.Stats.Rewrites.SumMatchesInlined, walker.Inlined);
        return ReferenceEquals(body, tir.Body)
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body, tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    /// <summary>A case test the elaborator made: a null-callee property call named after one case of its one argument's sum
    /// type (the writers read the same shape, TKT-1204). <paramref name="receiver"/> is the subject through coercions.</summary>
    public static bool TryReadCaseTest(TirNode node, out TirNode receiver, out TypeDef sum, out SumCaseDef tested)
    {
        receiver = null;
        sum = null;
        tested = null;
        if (node is not TirCall { Callee: null, EmissionKind: EmissionKind.Property } call || call.Args.Count != 1 || call.Name == null)
            return false;
        receiver = TirRewrite.StripCoerce(call.Args[0]);
        sum = receiver?.Type?.Def;
        if (sum == null || !sum.IsSum || sum.Cases.Count > 64)
            return false;
        tested = sum.Cases.FirstOrDefault(c => c.PredicateName == call.Name);
        return tested != null;
    }

    private sealed class Walker
    {
        private readonly TirFunction _tir;
        private readonly ITirInlineHost _host;
        private readonly string _owner;
        private readonly HashSet<object> _assigned = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);
        private readonly TypeExpression _boolType;
        private HashSet<string> _callerNames;
        private int _lambdaDepth;
        private int _inlineDepth;
        private int _inlinedNodes;
        public int Decided;
        public int Folded;
        public int Inlined;

        public Walker(TirFunction tir, PassContext pass)
        {
            _tir = tir;
            _host = pass.InlineHost;
            _owner = pass.OwnerTypeName;
            foreach (var n in tir.Body.Descendants())
                if (n is TirAssign a)
                {
                    var target = TirRewrite.StripCoerce(a.LValue);
                    if (target is TirVariable v && v.Def != null) _assigned.Add(v.Def);
                    else if (target is TirParameter p && p.Def != null) _assigned.Add(p.Def);
                }
            _boolType = pass.Compilation?.GetTypeDef("Boolean")?.ToTypeExpression();
        }

        private static ulong All(TypeDef sum) => sum.Cases.Count == 64 ? ulong.MaxValue : (1UL << sum.Cases.Count) - 1;
        private static ulong Bit(SumCaseDef c) => 1UL << c.Tag;

        private int Id(object o)
        {
            if (!_ids.TryGetValue(o, out var id))
                _ids[o] = id = _ids.Count + 1;
            return id;
        }

        /// <summary>The identity of a pure value: a parameter or local never assigned, a literal, a type reference, or a
        /// call, constructor or component read over such values; null for anything else (a lambda, a conditional, an
        /// assigned name, an array literal).</summary>
        private string KeyOf(TirNode n)
        {
            n = TirRewrite.StripCoerce(n);
            switch (n)
            {
                case TirParameter p when p.Def != null && !_assigned.Contains(p.Def):
                    return "p" + Id(p.Def);
                case TirVariable v when v.Def != null && !_assigned.Contains(v.Def):
                    return "v" + Id(v.Def);
                case TirLiteral l:
                    return "l:" + l.LiteralType + ":" + l.Value;
                case TirTypeRef t:
                    return "t:" + t.TypeDef?.Name;
                case TirCall c when c.Callee != null || c.Name != null:
                    return Compound("c" + (c.Callee != null ? Id(c.Callee).ToString() : "~" + c.Name) + ":" + c.EmissionKind, c.Args);
                case TirNew nw:
                    return Compound("n" + nw.NewType, nw.Args);
                case TirConstructorCall cc:
                    return Compound("k" + cc.TypeName, cc.Args);
                case TirComponentAccess ca:
                    var inner = KeyOf(ca.Receiver);
                    return inner == null ? null : "a" + ca.FieldName + "(" + inner + ")";
                default:
                    return null;
            }
        }

        private string Compound(string head, IReadOnlyList<TirNode> args)
        {
            var parts = new string[args.Count];
            for (var i = 0; i < args.Count; i++)
            {
                parts[i] = KeyOf(args[i]);
                if (parts[i] == null)
                    return null;
            }
            return head + "(" + string.Join(",", parts) + ")";
        }

        private bool TryReadKeyedTest(TirNode cond, out string subject, out TypeDef sum, out SumCaseDef tested)
        {
            subject = null;
            if (!TryReadCaseTest(cond, out var receiver, out sum, out tested))
                return false;
            subject = KeyOf(receiver);
            return subject != null;
        }

        private static ulong SetOf(ImmutableDictionary<string, ulong> env, string subject, TypeDef sum)
            => env.TryGetValue(subject, out var s) ? s : All(sum);

        /// <summary>The refinements that hold when <paramref name="cond"/> is true (<paramref name="whenTrue"/>) or false.</summary>
        private ImmutableDictionary<string, ulong> Facts(TirNode cond, ImmutableDictionary<string, ulong> env, bool whenTrue)
        {
            if (TryReadKeyedTest(cond, out var subject, out var sum, out var tested))
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

        private TirNode Decide(TirNode cond, ImmutableDictionary<string, ulong> env)
        {
            if (!TryReadKeyedTest(cond, out var subject, out var sum, out var tested))
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

        public TirNode Walk(TirNode node, ImmutableDictionary<string, ulong> env)
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
                        if (walked is TirLet let && let.Def != null && !_assigned.Contains(let.Def))
                            scope = Bind(scope, "v" + Id(let.Def), let.Value, let.Type);
                        if (!ReferenceEquals(walked, st))
                        {
                            rebuilt ??= b.Statements.ToList();
                            rebuilt[i] = walked;
                        }
                    }
                    return rebuilt == null ? b : new TirBlock(rebuilt, b.Origin);
                }
                case TirLambda:
                {
                    _lambdaDepth++;
                    var r = RewriteChildren(node, env);
                    _lambdaDepth--;
                    return r;
                }
                case TirCall:
                {
                    var rebuilt = RewriteChildren(node, env);
                    return rebuilt is TirCall call ? TryExpand(call, env) ?? rebuilt : rebuilt;
                }
                default:
                    return RewriteChildren(node, env);
            }
        }

        /// <summary>The environment after <c>var def = value</c>: a case constructor fixes the case, a refined value passes its set on.</summary>
        private ImmutableDictionary<string, ulong> Bind(ImmutableDictionary<string, ulong> env, string key, TirNode value, TypeExpression type)
        {
            var sum = type?.Def ?? value?.Type?.Def;
            if (sum == null || !sum.IsSum || sum.Cases.Count > 64)
                return env;
            var v = TirRewrite.StripCoerce(value);
            if (v is TirCall { Callee: null, EmissionKind: EmissionKind.StaticMethod } make && make.Name != null)
            {
                var made = sum.Cases.FirstOrDefault(c => c.Name == make.Name);
                if (made != null)
                    return env.SetItem(key, Bit(made));
            }
            var source = KeyOf(v);
            if (source != null && env.TryGetValue(source, out var set))
                return env.SetItem(key, set);
            return env;
        }

        /// <summary>The call expanded and walked when its callee is a match on a parameter whose argument's cases are narrowed
        /// here; null otherwise.</summary>
        private TirNode TryExpand(TirCall call, ImmutableDictionary<string, ulong> env)
        {
            if (_host == null || _lambdaDepth > 0 || _inlineDepth >= MaxInlineDepth || call.Callee == null || call.Args.Count == 0)
                return null;
            var calleeTir = TirInliner.GroundCallee(call, _tir, _host, _owner);
            if (calleeTir?.Body is not TirConditional top || calleeTir.Parameters == null || calleeTir.Parameters.Count != call.Args.Count)
                return null;
            if (!TryReadCaseTest(top.Condition, out var receiver, out var sum, out _) || receiver is not TirParameter p || p.Def == null)
                return null;
            var index = -1;
            for (var i = 0; i < calleeTir.Parameters.Count; i++)
                if (ReferenceEquals(calleeTir.Parameters[i], p.Def))
                    index = i;
            if (index < 0)
                return null;
            var key = KeyOf(call.Args[index]);
            if (key == null || !env.TryGetValue(key, out var set) || set == All(sum))
                return null;
            _callerNames ??= TirInliner.CollectBoundNames(_tir, _tir.Body);
            if (!TirInliner.TryInlineForRefinement(call, _tir, _host, _owner, insideLambda: false, _callerNames, out var inlined))
                return null;
            var size = inlined.Descendants().Count();
            if (_inlinedNodes + size > InlineNodeBudget)
                return null;
            _inlinedNodes += size;
            Inlined++;
            foreach (var name in TirInliner.CollectBoundNames(_tir, inlined))
                _callerNames.Add(name);
            _inlineDepth++;
            var walked = Walk(inlined, env);
            _inlineDepth--;
            return walked;
        }

        private TirNode RewriteChildren(TirNode node, ImmutableDictionary<string, ulong> env)
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
