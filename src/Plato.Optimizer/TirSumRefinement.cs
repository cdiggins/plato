// Refines a sum-typed value to the cases it can still be inside each match arm, folds the case tests that knowledge
// decides, expands a callee that is one match on a parameter when the argument's cases are known, redirects a call to a
// clone specialized to what is known about its arguments, and splits a configured entry function per case (--refine-sums).
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

namespace Ara3D.Geometry.Optimizer;

/// <summary>
/// The refinement layer on TIR (docs/plans/tir-partial-evaluation-2026-11-07.md in plato-brep). A Plato <c>match</c>
/// lowers to a chain of case predicates, <c>IsA(s) ? x : IsB(s) ? y : z</c>, and the arm bodies see <c>s</c> with the
/// same type as anywhere else, so a second match on <c>s</c> inside an arm tests the tag again. This pass walks each
/// body with an environment from a <em>value</em> to the set of cases it can still be. A value is a parameter or local
/// never assigned, a literal, or a call, constructor or component read over such values, written as a key over the
/// function's parameters (<c>&lt;P0&gt;</c>, <c>&lt;P1&gt;</c>, ...): Plato is pure, so two occurrences of
/// <c>g.SurfaceAt(s)</c> are the same value, and a small callee's key is its body's key (so <c>SurfaceAt(g, s)</c> and
/// <c>g.Surfaces[s.Value]</c> are one value). A case test narrows its subject in the true arm and removes the case in
/// the false arm (through <c>&amp;&amp;</c> and <c>||</c>); a local bound to a case constructor or to a refined value
/// takes its set; a case test on a value whose set decides it becomes a Boolean literal, after which the conditional
/// or <c>if</c> on it keeps the one live arm.
/// <para>
/// Knowledge crosses calls three ways. <b>Expansion:</b> a call whose callee's body is a match on a parameter, where the
/// argument for that parameter has a narrowed set, is expanded through the inliner with a larger body limit
/// (<see cref="TirInliner.RefinementBodyNodes"/>) and the match it brings in collapses to the live arm. <b>Clones:</b>
/// when the backend is an <see cref="ISumRefinementHost"/>, a call whose callee (or something it calls, with the value
/// passed along) tests a value this body knows is redirected to a clone of the callee specialized to those facts; the
/// clone's body is optimized later with the facts as its <see cref="PassContext.Seed"/>, so the knowledge flows down the
/// call tree until the tests fold. <b>The split:</b> a function named in <c>--refine-split=A,B</c> is wrapped at entry in
/// a match on the sum value its callees test most, one arm per case, each arm a copy of the body walked with that case
/// known; that is the one place a test is added, once per call of the entry, and it is what seeds everything below
/// <c>MeshFace</c> or <c>DiscretizeEdgeRangedCounted</c>. Clones are bounded by <c>--refine-clones</c> (default
/// <see cref="DefaultMaxClones"/>, enforced by the host) and expansion by <see cref="MaxInlineDepth"/> and
/// <see cref="InlineNodeBudget"/>.
/// </para>
/// </summary>
/// <remarks>
/// A name assigned anywhere in the body (the loop lowerer's locals) is never part of a value. A sum with more than 64
/// cases is left alone. Results are unchanged by construction: a test is folded only when every path to it has
/// established the answer, expansion is the inliner's own substitution, a clone is the same body under more knowledge,
/// and the split's arms are exhaustive with the last case unconditional, as the elaborator lowers a match.
/// </remarks>
public static class TirSumRefinement
{
    public const string Switch = "refine-sums";
    public const string SplitSwitch = "refine-split";
    public const string ClonesSwitch = "refine-clones";
    public const int MaxInlineDepth = 6;
    public const int InlineNodeBudget = 4096;
    public const int DefaultMaxClones = 2000;
    /// <summary>A callee whose expression body has at most this many nodes is a value of its arguments: its key is its body's key.</summary>
    public const int NormalizeBodyNodes = 40;
    public const int SubjectsDepth = 12;
    public const int SubjectsPerBody = 96;

    public static TirFunction Rewrite(TirFunction tir, PassContext pass)
    {
        if (!pass.On(Switch) || tir?.Body == null)
            return tir;
        var walker = new Walker(tir, pass);
        var env = ImmutableDictionary<string, ulong>.Empty;
        if (pass.Seed != null)
            foreach (var fact in pass.Seed)
                env = env.SetItem(fact.Key, fact.Set);
        var body = pass.Seed == null && walker.IsSplitRoot() ? walker.Split(env) : walker.Walk(tir.Body, env);
        Interlocked.Add(ref pass.Stats.Rewrites.SumTestsDecided, walker.Decided);
        Interlocked.Add(ref pass.Stats.Rewrites.SumArmsFolded, walker.Folded);
        Interlocked.Add(ref pass.Stats.Rewrites.SumMatchesInlined, walker.Inlined);
        Interlocked.Add(ref pass.Stats.Rewrites.SumClonesRequested, walker.Cloned);
        Interlocked.Add(ref pass.Stats.Rewrites.SumSplits, walker.Splits);
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

    /// <summary>A match subject reachable from a body, as a key over that body's parameters, with a node that computes it
    /// from those parameters and the sum it belongs to.</summary>
    public sealed record Subject(string Key, TirNode Node, TypeDef Sum);

    private static readonly Regex ParamToken = new(@"<P(\d+)>", RegexOptions.Compiled);

    // Bodies whose subjects are being collected on this thread: a cycle returns nothing instead of recursing forever.
    [ThreadStatic] private static HashSet<TirFunction> _collecting;

    private sealed class Walker
    {
        private readonly TirFunction _tir;
        private readonly PassContext _ctx;
        private readonly ITirInlineHost _host;
        private readonly ISumRefinementHost _refHost;
        private readonly string _owner;
        private readonly HashSet<object> _assigned = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, string> _alias = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, TirNode> _aliasNode = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, int> _ids = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<object, int> _paramIndex = new(ReferenceEqualityComparer.Instance);
        private readonly TypeExpression _boolType;
        private readonly int _normDepth;
        private HashSet<string> _callerNames;
        private int _lambdaDepth;
        private int _inlineDepth;
        private int _inlinedNodes;
        public int Decided;
        public int Folded;
        public int Inlined;
        public int Cloned;
        public int Splits;

        public Walker(TirFunction tir, PassContext pass, int normDepth = 0)
        {
            _tir = tir;
            _ctx = pass;
            _host = pass.InlineHost;
            _refHost = pass.Backend as ISumRefinementHost;
            _owner = pass.OwnerTypeName;
            _normDepth = normDepth;
            for (var i = 0; i < tir.Parameters.Count; i++)
                if (tir.Parameters[i] != null)
                    _paramIndex[tir.Parameters[i]] = i;
            foreach (var n in tir.Body.Descendants())
                if (n is TirAssign a)
                {
                    var target = TirRewrite.StripCoerce(a.LValue);
                    if (target is TirVariable v && v.Def != null) _assigned.Add(v.Def);
                    else if (target is TirParameter p && p.Def != null) _assigned.Add(p.Def);
                }
            // A local's key is its value's key, in source order, so later reads of it name the same value.
            foreach (var n in tir.Body.Descendants())
                if (n is TirLet let && let.Def != null && !_assigned.Contains(let.Def))
                {
                    var k = KeyOf(let.Value);
                    if (k != null)
                    {
                        _alias[let.Def] = k;
                        _aliasNode[let.Def] = let.Value;
                    }
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

        private string FunctionToken(FunctionDef f)
        {
            var id = _ctx.StableId(f);
            if (id >= 0)
                return "F" + id;
            // Outside the compilation's list (a generated getter): name, owner and parameter types identify it.
            return "F?" + f.Name + "@" + f.OwnerType?.Name + "/" + string.Join(",", f.Parameters.Select(p => p.Type?.ToString()));
        }

        /// <summary>The identity of a pure value, or null for anything else (a lambda, a conditional, an assigned name, an array
        /// literal). Parameters are <c>&lt;Pi&gt;</c>, locals <c>&lt;vN&gt;</c> unless bound to a value, so a key without
        /// <c>&lt;v</c> is a value of the parameters alone.</summary>
        public string KeyOf(TirNode n)
        {
            n = TirRewrite.StripCoerce(n);
            switch (n)
            {
                case TirParameter p when p.Def != null && !_assigned.Contains(p.Def):
                    if (_paramIndex.TryGetValue(p.Def, out var i))
                        return "<P" + i + ">";
                    return _alias.TryGetValue(p.Def, out var pa) ? pa : "<v" + Id(p.Def) + ">";
                case TirVariable v when v.Def != null && !_assigned.Contains(v.Def):
                    return _alias.TryGetValue(v.Def, out var va) ? va : "<v" + Id(v.Def) + ">";
                case TirLiteral l:
                    return "<L:" + l.LiteralType + ":" + l.Value + ">";
                case TirTypeRef t:
                    return "<T:" + t.TypeDef?.Name + ">";
                case TirCall c when c.Callee != null:
                    return Compound(c, c.Args) ;
                case TirCall c when c.Name != null:
                    return CompoundOf("F~" + c.Name + ":" + c.EmissionKind, c.Args);
                case TirNew nw:
                    return CompoundOf("N" + nw.NewType, nw.Args);
                case TirConstructorCall cc:
                    return CompoundOf("K" + cc.TypeName, cc.Args);
                case TirComponentAccess ca:
                    var inner = KeyOf(ca.Receiver);
                    return inner == null ? null : "A" + ca.FieldName + "(" + inner + ")";
                default:
                    return null;
            }
        }

        private string Compound(TirCall c, IReadOnlyList<TirNode> args)
        {
            var parts = new string[args.Count];
            for (var i = 0; i < args.Count; i++)
                if ((parts[i] = KeyOf(args[i])) == null)
                    return null;
            // A small pure callee is a value of its arguments: its key is its body's key with the arguments in.
            var template = BodyTemplate(c);
            if (template != null)
                return ParamToken.Replace(template, m => parts[int.Parse(m.Groups[1].Value)]);
            return FunctionToken(c.Callee) + "(" + string.Join(",", parts) + ")";
        }

        private string CompoundOf(string head, IReadOnlyList<TirNode> args)
        {
            var parts = new string[args.Count];
            for (var i = 0; i < args.Count; i++)
                if ((parts[i] = KeyOf(args[i])) == null)
                    return null;
            return head + "(" + string.Join(",", parts) + ")";
        }

        /// <summary>The body key of a small expression callee over <c>&lt;Pi&gt;</c>, or null when the callee is not that
        /// (no ground body, a statement body, a lambda or a local in it, too large, or the parameter count disagrees).</summary>
        private string BodyTemplate(TirCall c)
        {
            if (_normDepth >= 3 || _host == null || c.Args.Count == 0)
                return null;
            var calleeTir = TirInliner.GroundCallee(c, _tir, _host, _owner);
            if (calleeTir?.Body == null || calleeTir.Parameters.Count != c.Args.Count)
                return null;
            Func<string> compute = () =>
            {
                var body = calleeTir.Body;
                if (TirRewrite.IsStatementNode(body))
                    return "";
                var count = 0;
                foreach (var n in body.Descendants())
                {
                    if (++count > NormalizeBodyNodes || n is TirLambda || n is TirLet || n is TirAssign || n is TirLoop)
                        return "";
                }
                var inner = new Walker(calleeTir, _ctx.ForBody(TirInliner.TrustedTypeName(_tir, c.Args[0], _host, _owner)), _normDepth + 1);
                var key = inner.KeyOf(body);
                return key == null || key.Contains("<v") ? "" : key;
            };
            var template = _refHost != null ? _refHost.Memo(("template", calleeTir), compute) : compute();
            return template.Length == 0 ? null : template;
        }

        /// <summary>The node with every local bound to a value replaced by that value, so it computes from the parameters alone;
        /// null when a local without a value, or a parameter of another function, remains.</summary>
        private TirNode Canonical(TirNode node)
        {
            var ok = true;
            var result = TirRewrite.Rewrite(node, n =>
            {
                switch (n)
                {
                    case TirVariable v when v.Def != null && _aliasNode.TryGetValue(v.Def, out var value):
                        return Canonical(value) ?? Fail(n);
                    case TirVariable:
                        return Fail(n);
                    case TirParameter p when p.Def != null && _paramIndex.ContainsKey(p.Def):
                        return n;
                    case TirParameter p when p.Def != null && _aliasNode.TryGetValue(p.Def, out var value):
                        return Canonical(value) ?? Fail(n);
                    case TirParameter:
                        return Fail(n);
                    default:
                        return n;
                }
            });
            return ok ? result : null;

            TirNode Fail(TirNode n)
            {
                ok = false;
                return n;
            }
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
                            scope = Bind(scope, KeyOf(new TirVariable(let.Def, let.Type, let.Origin)), let.Value, let.Type);
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
                    if (rebuilt is not TirCall call)
                        return rebuilt;
                    return TryExpand(call, env) ?? TryRedirect(call, env) ?? rebuilt;
                }
                default:
                    return RewriteChildren(node, env);
            }
        }

        /// <summary>The environment after <c>var def = value</c>: a case constructor fixes the case, a refined value passes its set on.</summary>
        private ImmutableDictionary<string, ulong> Bind(ImmutableDictionary<string, ulong> env, string key, TirNode value, TypeExpression type)
        {
            var sum = type?.Def ?? value?.Type?.Def;
            if (key == null || sum == null || !sum.IsSum || sum.Cases.Count > 64)
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

        /// <summary>The facts of <paramref name="env"/> that are about values of <paramref name="call"/>'s arguments, as facts
        /// over the callee's parameters; a fact that mentions anything else is left behind.</summary>
        private List<SumFact> FactsForCallee(TirCall call, ImmutableDictionary<string, ulong> env)
        {
            if (env.IsEmpty)
                return null;
            var argKeys = new string[call.Args.Count];
            for (var i = 0; i < argKeys.Length; i++)
                argKeys[i] = KeyOf(call.Args[i]);
            // Longer argument keys first, so an argument that contains another is replaced whole.
            var order = Enumerable.Range(0, argKeys.Length).Where(i => argKeys[i] != null).OrderByDescending(i => argKeys[i].Length).ToList();
            List<SumFact> facts = null;
            foreach (var (key, set) in env)
            {
                var k = key;
                foreach (var i in order)
                    k = k.Replace(argKeys[i], "<Q" + i + ">");
                if (k.Contains("<P") || k.Contains("<v") || !k.Contains("<Q"))
                    continue;
                (facts ??= new List<SumFact>()).Add(new SumFact(k.Replace("<Q", "<P"), set));
            }
            facts?.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return facts;
        }

        /// <summary>The call redirected to a clone of its callee when something this body knows about the arguments is a value
        /// the callee, or a function it passes the value to, tests; null otherwise.</summary>
        private TirNode TryRedirect(TirCall call, ImmutableDictionary<string, ulong> env)
        {
            if (_refHost == null || call.Callee == null || call.Args.Count == 0 || env.IsEmpty)
                return null;
            if (_tir.Original != null && ReferenceEquals(call.Callee, _tir.Original))
                return null;
            var calleeTir = TirInliner.GroundCallee(call, _tir, _host, _owner);
            if (calleeTir?.Body == null || calleeTir.Parameters.Count != call.Args.Count)
                return null;
            var facts = FactsForCallee(call, env);
            if (facts == null)
                return null;
            var receiverTypeName = TirInliner.TrustedTypeName(_tir, call.Args[0], _host, _owner);
            var subjects = Subjects(calleeTir, receiverTypeName, 0);
            var used = facts.Where(f => subjects.Any(s => s.Key == f.Key && f.Set != All(s.Sum))).ToList();
            if (used.Count == 0)
                return null;
            var name = _refHost.RequestClone(call.Callee, receiverTypeName, calleeTir, used);
            if (name == null)
                return null;
            Cloned++;
            return new TirCall(call.Callee, call.EmissionKind, call.ParameterTypes, call.ReturnType, call.Args, call.Type, call.Origin, name);
        }

        /// <summary>The match subjects reachable from <paramref name="tir"/> that are values of its parameters: its own case
        /// tests, and its callees' subjects with the arguments substituted, to <see cref="SubjectsDepth"/> calls down.</summary>
        private List<Subject> Subjects(TirFunction tir, string ownerTypeName, int depth)
        {
            if (tir?.Body == null || depth > SubjectsDepth)
                return new List<Subject>();
            Func<List<Subject>> compute = () =>
            {
                _collecting ??= new HashSet<TirFunction>();
                if (!_collecting.Add(tir))
                    return new List<Subject>();
                try
                {
                    var inner = new Walker(tir, _ctx.ForBody(ownerTypeName), _normDepth);
                    return inner.CollectSubjects(depth);
                }
                finally
                {
                    _collecting.Remove(tir);
                }
            };
            return _refHost != null ? _refHost.Memo(("subjects", tir, ownerTypeName), compute) : compute();
        }

        private List<Subject> CollectSubjects(int depth)
        {
            var found = new List<Subject>();
            var seen = new HashSet<string>();
            void Add(string key, TirNode node, TypeDef sum)
            {
                if (key == null || key.Contains("<v") || found.Count >= SubjectsPerBody)
                    return;
                found.Add(new Subject(key, node, sum));
            }
            foreach (var n in _tir.Body.Descendants())
            {
                if (TryReadCaseTest(n, out var receiver, out var sum, out _))
                {
                    var canonical = Canonical(receiver);
                    if (canonical != null)
                        Add(KeyOf(receiver), canonical, sum);
                    continue;
                }
                if (n is not TirCall call || call.Callee == null || call.Args.Count == 0 || _host == null)
                    continue;
                if (_tir.Original != null && ReferenceEquals(call.Callee, _tir.Original))
                    continue;
                var calleeTir = TirInliner.GroundCallee(call, _tir, _host, _owner);
                if (calleeTir?.Body == null || calleeTir.Parameters.Count != call.Args.Count)
                    continue;
                var argKeys = call.Args.Select(KeyOf).ToArray();
                List<Subject> below = null;
                Dictionary<ParameterDef, TirNode> map = null;
                foreach (var s in Subjects(calleeTir, TirInliner.TrustedTypeName(_tir, call.Args[0], _host, _owner), depth + 1))
                {
                    var missing = false;
                    var key = ParamToken.Replace(s.Key, m =>
                    {
                        var k = argKeys[int.Parse(m.Groups[1].Value)];
                        if (k == null) missing = true;
                        return k ?? "";
                    });
                    if (missing || key.Contains("<v") || !seen.Add(key + "|" + s.Sum.Name))
                        continue;
                    if (map == null)
                    {
                        map = new Dictionary<ParameterDef, TirNode>();
                        for (var i = 0; i < calleeTir.Parameters.Count; i++)
                            if (calleeTir.Parameters[i] != null)
                                map[calleeTir.Parameters[i]] = Canonical(call.Args[i]) ?? call.Args[i];
                    }
                    var node = Canonical(TirRewrite.Substitute(s.Node, map));
                    if (node == null)
                        continue;
                    (below ??= new List<Subject>()).Add(new Subject(key, node, s.Sum));
                }
                if (below != null)
                    foreach (var s in below)
                        Add(s.Key, s.Node, s.Sum);
            }
            return found;
        }

        /// <summary>The sum type the switch names for this root (<c>Name:Sum</c>), "" for a root without one, null when this
        /// body is not a split root.</summary>
        private string SplitSumOf()
        {
            if (_refHost == null || _tir.Original?.Name == null)
                return null;
            var roots = _ctx.Switches.Value(SplitSwitch);
            if (roots == null)
                return null;
            foreach (var entry in roots.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var colon = entry.IndexOf(':');
                var name = colon < 0 ? entry : entry.Substring(0, colon);
                if (name == _tir.Original.Name)
                    return colon < 0 ? "" : entry.Substring(colon + 1);
            }
            return null;
        }

        public bool IsSplitRoot() => SplitSumOf() != null;

        /// <summary>The body wrapped in a match on the subject its callees test most, one arm per case of the sum, each arm the
        /// body walked knowing that case; the body itself when nothing below tests a value of the parameters.</summary>
        public TirNode Split(ImmutableDictionary<string, ulong> env)
        {
            var wanted = SplitSumOf();
            var subjects = Subjects(_tir, _owner, 0);
            var best = subjects.Where(s => s.Sum.Cases.Count >= 2 && (string.IsNullOrEmpty(wanted) || s.Sum.Name == wanted))
                .GroupBy(s => s.Key + "|" + s.Sum.Name)
                .OrderByDescending(g => g.Count()).ThenByDescending(g => g.First().Sum.Cases.Count).ThenBy(g => g.Key, StringComparer.Ordinal)
                .FirstOrDefault();
            if (best == null || _boolType == null)
                return Walk(_tir.Body, env);
            var subject = best.First();
            var cases = subject.Sum.Cases.OrderBy(c => c.Tag).ToList();
            var statement = TirRewrite.IsStatementNode(_tir.Body);
            var origin = _tir.Body.Origin;
            Splits++;
            // Declaration order, the last case unconditional: what the elaborator makes of an exhaustive match.
            TirNode node = Walk(_tir.Body, env.SetItem(subject.Key, Bit(cases[cases.Count - 1])));
            for (var i = cases.Count - 2; i >= 0; i--)
            {
                var test = new TirCall(null, EmissionKind.Property, null, _boolType, new List<TirNode> { subject.Node }, _boolType, origin, cases[i].PredicateName);
                var arm = Walk(_tir.Body, env.SetItem(subject.Key, Bit(cases[i])));
                node = statement
                    ? new TirIf(test, arm, node, origin)
                    : new TirConditional(test, arm, node, _tir.Body.Type, origin);
            }
            return node;
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
