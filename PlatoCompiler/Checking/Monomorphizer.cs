using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The monomorphize sub-pass: specializes each generic/abstract <see cref="TirFunction"/> per
    /// concrete instantiation, the way Plato's compilation model already stamps generic/concept
    /// functions into concrete per-type implementations. It is driven off, and cross-checked
    /// against, the existing reification machinery (<see cref="Types.ReifiedFunction"/> /
    /// <see cref="Types.ReifiedType"/> / <see cref="Compilation.ReifiedFunctions"/>) — that set is
    /// the correctness oracle for the count, shape, and ground signature of every instantiation.
    ///
    /// It is TOTAL — <see cref="Specialize"/> never throws — and runs in shadow mode: the
    /// monomorphized TIR feeds no writer, so off-flag output stays byte-identical.
    /// </summary>
    public class Monomorphizer
    {
        public Compilation Compilation { get; }

        /// <summary>Resolves a concept-method call, specialized for a concrete receiver, to that
        /// receiver's concrete implementation (or null to leave the call as an abstract dispatch).
        /// Signature: (name, receiverType, groundParameterTypes, groundReturnType) → implementation.
        /// Overridable so re-dispatch can be unit-tested without a full <see cref="Compilation"/>.</summary>
        public Func<string, TypeExpression, IReadOnlyList<TypeExpression>, TypeExpression, FunctionDef>
            RedispatchResolver { get; set; }

        // Per-call walk state (the pass is single-threaded; reset at the top of Specialize).
        private TypeSubstitution _sub = TypeSubstitution.Empty;
        private MonomorphizeStats _stats = new MonomorphizeStats();

        public Monomorphizer(Compilation compilation)
        {
            Compilation = compilation;
            RedispatchResolver = DefaultRedispatch;
        }

        // --- driver --------------------------------------------------------------

        /// <summary>
        /// Monomorphize the whole compilation: for each <see cref="Types.ReifiedFunction"/> derive the
        /// grounding substitution (<see cref="TypeSubstitution.FromSignature"/>) and, when the source
        /// function has a body, specialize its elaborated TIR. Produces exactly one
        /// <see cref="MonomorphizedFunction"/> per reified function, so the set tracks
        /// <see cref="Compilation.ReifiedFunctions"/> 1:1.
        /// </summary>
        public IReadOnlyList<MonomorphizedFunction> MonomorphizeAll()
        {
            // Elaborate every checked (bodied) function once, keyed by its original definition.
            var tirByOriginal = new Dictionary<FunctionDef, TirFunction>();
            var checker = new TypeChecker(Compilation);
            var elaborator = new Elaborator(Compilation);
            foreach (var result in checker.CheckAll())
                if (result?.Function != null)
                    tirByOriginal[result.Function] = elaborator.Elaborate(result);

            var results = new List<MonomorphizedFunction>();
            foreach (var rf in Compilation.ReifiedFunctions)
            {
                // Derive the grounding substitution from the reified signature, then bind Self to the
                // type being reified onto — library-over-interface bodies reference Self even though it
                // is absent from their signature (the reifier's own gap; Self always denotes that type).
                var sub = TypeSubstitution
                    .FromSignature(rf.Original, rf.ParameterTypes, rf.ReturnType)
                    .WithSelf(rf.ReifiedType?.Self);

                TirFunction tir = null;
                var stats = new MonomorphizeStats();
                if (rf.Original?.Body != null && tirByOriginal.TryGetValue(rf.Original, out var baseTir))
                    tir = Specialize(baseTir, sub, stats);

                var ground = tir != null && tir.AllNodes.All(NodeIsGround);
                results.Add(new MonomorphizedFunction(rf, sub, tir, stats, ground));
            }
            return results;
        }

        // --- specializer ---------------------------------------------------------

        /// <summary>Specialize a TIR function under a substitution: every node's type, every call's
        /// instantiated signature, every coercion/new target is grounded where the substitution
        /// covers it. Total — never throws. Concept-method calls whose receiver became concrete are
        /// re-dispatched to the concrete implementation on an unambiguous match.</summary>
        public TirFunction Specialize(TirFunction f, TypeSubstitution sub, MonomorphizeStats stats = null)
        {
            if (f == null) return null;
            _sub = sub ?? TypeSubstitution.Empty;
            _stats = stats ?? new MonomorphizeStats();
            var body = SpecializeNode(f.Body);
            // The parameter *symbols* keep their identity (references resolve by identity); the ground
            // parameter types live on the body's TirParameter nodes and on Reified.ParameterTypes.
            return new TirFunction(f.Original, f.Parameters, _sub.Apply(f.ReturnType), body);
        }

        private IReadOnlyList<TirNode> SpecializeNodes(IReadOnlyList<TirNode> ns)
            => ns?.Select(SpecializeNode).ToList();

        private TirNode SpecializeNode(TirNode n)
        {
            switch (n)
            {
                case null:
                    return null;
                case TirLiteral lit:
                    return new TirLiteral(lit.Value, lit.LiteralType, _sub.Apply(lit.Type), lit.Origin);
                case TirParameter p:
                    return new TirParameter(p.Def, _sub.Apply(p.Type), p.Origin);
                case TirVariable v:
                    return new TirVariable(v.Def, _sub.Apply(v.Type), v.Origin);
                case TirTypeRef t:
                    return new TirTypeRef(t.TypeDef, _sub.Apply(t.Type), t.Origin);
                case TirDefault d:
                    return new TirDefault(_sub.Apply(d.Type), d.Origin);
                case TirCoerce c:
                    return new TirCoerce(SpecializeNode(c.Inner), _sub.Apply(c.FromType), _sub.Apply(c.ToType),
                        c.ConversionFn, c.Origin);
                case TirCall call:
                    return SpecializeCall(call);
                case TirInvoke inv:
                    return new TirInvoke(SpecializeNode(inv.Target), SpecializeNodes(inv.Args),
                        _sub.Apply(inv.Type), inv.Origin);
                case TirConditional cond:
                    return new TirConditional(SpecializeNode(cond.Condition), SpecializeNode(cond.IfTrue),
                        SpecializeNode(cond.IfFalse), _sub.Apply(cond.Type), cond.Origin);
                case TirNew nw:
                    return new TirNew(_sub.Apply(nw.NewType), SpecializeNodes(nw.Args), _sub.Apply(nw.Type), nw.Origin);
                case TirArray arr:
                    return new TirArray(SpecializeNodes(arr.Elements), _sub.Apply(arr.Type), arr.Origin);
                case TirAssign asg:
                    return new TirAssign(SpecializeNode(asg.LValue), SpecializeNode(asg.RValue),
                        _sub.Apply(asg.Type), asg.Origin);
                case TirLambda lam:
                    return new TirLambda(lam.Parameters, SpecializeNode(lam.Body), _sub.Apply(lam.Type), lam.Origin);
                case TirBlock b:
                    return new TirBlock(SpecializeNodes(b.Statements), b.Origin);
                case TirReturn r:
                    return new TirReturn(SpecializeNode(r.Value), r.Origin);
                case TirIf iff:
                    return new TirIf(SpecializeNode(iff.Condition), SpecializeNode(iff.IfTrue),
                        SpecializeNode(iff.IfFalse), iff.Origin);
                case TirLoop l:
                    return new TirLoop(SpecializeNode(l.Condition), SpecializeNode(l.Body), l.Origin);
                case TirUnresolved u:
                    return new TirUnresolved(u.Original, u.Reason, SpecializeNodes(u.ChildNodes));
                default:
                    return n; // unknown node kind — leave untouched, stay total
            }
        }

        private TirNode SpecializeCall(TirCall call)
        {
            var ps = _sub.ApplyAll(call.ParameterTypes);
            var ret = _sub.Apply(call.ReturnType);
            var type = _sub.Apply(call.Type);
            var args = SpecializeNodes(call.Args);

            var callee = call.Callee;
            var kind = call.EmissionKind;

            // Concept re-dispatch (direct case only): a call whose callee is an abstract concept
            // declaration, now that its receiver has become concrete, re-points to that type's
            // concrete implementation — but only on an unambiguous match, so we never mis-dispatch.
            if (callee != null && callee.FunctionType == FunctionType.Concept && ps != null && ps.Count > 0)
            {
                var target = RedispatchResolver?.Invoke(call.Name, ps[0], ps, ret);
                if (target != null)
                {
                    callee = target;
                    kind = EmissionKindOf(target);
                    _stats.Redispatched++;
                }
                else
                {
                    _stats.DeferredConcept++;
                }
            }

            return new TirCall(callee, kind, ps, ret, args, type, call.Origin);
        }

        // --- concept re-dispatch resolution --------------------------------------

        /// <summary>The default re-dispatch: find, among the receiver type's reified functions, the
        /// unique non-concept implementation whose name and ground parameter types match the call.
        /// Returns null (leave the call as an abstract dispatch) on 0 or &gt;1 matches. Concrete
        /// receiver only — never dispatches through an interface or unbound type.</summary>
        private FunctionDef DefaultRedispatch(string name, TypeExpression receiver,
            IReadOnlyList<TypeExpression> groundParams, TypeExpression groundReturn)
        {
            if (Compilation?.ReifiedFunctionsByName == null || receiver?.Def == null)
                return null;
            if (!receiver.Def.IsConcrete() || !TypeSubstitution.IsGround(receiver))
                return null;
            if (!Compilation.ReifiedFunctionsByName.TryGetValue(name, out var candidates))
                return null;

            var matches = candidates.Where(rf =>
                rf.Original != null
                && rf.Original.FunctionType != FunctionType.Concept // an implementation, not the declaration
                && rf.ReifiedType?.Type != null
                && rf.ReifiedType.Type.Name == receiver.Def.Name    // owned by the receiver's reified type
                && SignaturesMatch(rf.ParameterTypes, groundParams)).ToList();

            // Disambiguate a residual tie by return type if that leaves a unique winner.
            if (matches.Count > 1 && groundReturn != null)
            {
                var byRet = matches
                    .Where(rf => rf.ReturnType != null && rf.ReturnType.ToString() == groundReturn.ToString())
                    .ToList();
                if (byRet.Count == 1)
                    matches = byRet;
            }

            return matches.Count == 1 ? matches[0].Original : null;
        }

        private static bool SignaturesMatch(IReadOnlyList<TypeExpression> a, IReadOnlyList<TypeExpression> b)
        {
            if (a == null || b == null || a.Count != b.Count)
                return false;
            for (var i = 0; i < a.Count; i++)
                if (!TypeSubstitution.IsGround(b[i]) || a[i]?.ToString() != b[i]?.ToString())
                    return false;
            return true;
        }

        // --- helpers -------------------------------------------------------------

        /// <summary>Re-derive an <see cref="EmissionKind"/> for a re-dispatched callee from its
        /// <see cref="FunctionType"/> and name. Shape-free (no call-site HasArgList / type-name
        /// nuance the <see cref="Elaborator"/> uses); a full re-derivation is deferred to emit.</summary>
        private static EmissionKind EmissionKindOf(FunctionDef f)
        {
            switch (f.FunctionType)
            {
                case FunctionType.Constructor: return EmissionKind.Constructor;
                case FunctionType.Cast: return EmissionKind.Conversion;
                case FunctionType.Field: return EmissionKind.Property;
                case FunctionType.Intrinsic: return EmissionKind.Intrinsic;
            }
            if (IsOperatorName(f.Name))
                return EmissionKind.Operator;
            return f.NumParameters == 0 ? EmissionKind.StaticMethod : EmissionKind.InstanceMethod;
        }

        private static bool IsOperatorName(string name)
            => name != null
               && (Operators.NamesToBinaryOperators.ContainsKey(name)
                   || Operators.NamesToUnaryOperators.ContainsKey(name));

        /// <summary>A node is fully ground when its value type — and, for calls/coercions/new, every
        /// type it carries — is ground, and it is not an unresolved-elaboration placeholder.</summary>
        internal static bool NodeIsGround(TirNode n)
        {
            if (n is TirUnresolved)
                return false;
            if (!TypeSubstitution.IsGround(n.Type))
                return false;
            switch (n)
            {
                case TirCall c:
                    if (!TypeSubstitution.IsGround(c.ReturnType))
                        return false;
                    if (c.ParameterTypes.Any(t => !TypeSubstitution.IsGround(t)))
                        return false;
                    break;
                case TirCoerce co:
                    if (!TypeSubstitution.IsGround(co.FromType) || !TypeSubstitution.IsGround(co.ToType))
                        return false;
                    break;
                case TirNew nw:
                    if (!TypeSubstitution.IsGround(nw.NewType))
                        return false;
                    break;
            }
            return true;
        }
    }

    /// <summary>Per-function monomorphization counters (concept re-dispatch coverage).</summary>
    public class MonomorphizeStats
    {
        /// <summary>Concept-method calls re-pointed to a concrete implementation.</summary>
        public int Redispatched;
        /// <summary>Concept-method calls left as abstract dispatch (no unambiguous concrete match).</summary>
        public int DeferredConcept;
    }

    /// <summary>
    /// One monomorphized instantiation: a <see cref="Types.ReifiedFunction"/> (the oracle entry), the
    /// grounding <see cref="TypeSubstitution"/> derived from it, and — when the source function has a
    /// body — its specialized TIR. The ground signature is available via
    /// <see cref="Types.ReifiedFunction.ParameterTypes"/> / <see cref="Types.ReifiedFunction.ReturnType"/>.
    /// </summary>
    public class MonomorphizedFunction
    {
        public ReifiedFunction Reified { get; }
        public TypeSubstitution Substitution { get; }
        public TirFunction Tir { get; }
        public MonomorphizeStats Stats { get; }
        public bool IsFullyGround { get; }

        public FunctionDef Original => Reified?.Original;
        public TypeDef ConcreteType => Reified?.ReifiedType?.Type;
        public bool HasBody => Tir != null;

        public MonomorphizedFunction(ReifiedFunction reified, TypeSubstitution substitution,
            TirFunction tir, MonomorphizeStats stats, bool isFullyGround)
        {
            Reified = reified;
            Substitution = substitution;
            Tir = tir;
            Stats = stats ?? new MonomorphizeStats();
            IsFullyGround = isFullyGround;
        }

        public override string ToString()
            => $"{ConcreteType?.Name}.{Original?.Name} {Substitution}"
               + (HasBody ? IsFullyGround ? " [ground]" : " [partial]" : " [no body]");
    }
}
