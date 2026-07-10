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
                {
                    // ALSO pair the solver-ZONKED signature: the TIR body's residual variables are
                    // in terminal zonked form ($G14, $Ret0), not the declared form ($T), so only
                    // the zonked pairing can bind them (declared keys stay, and win, for the
                    // reifier cross-check).
                    if (baseTir.ZonkedParameterTypes != null || baseTir.ZonkedReturnType != null)
                        sub = sub.MergedWith(TypeSubstitution.FromSignature(
                            baseTir.ZonkedParameterTypes, baseTir.ZonkedReturnType,
                            rf.ParameterTypes, rf.ReturnType));
                    tir = Specialize(baseTir, sub, stats);

                    // Residual grounding: variables the signature pairing cannot reach can still
                    // be determined by the GROUND context — the reified return type at the
                    // function's return positions (a tuple result pairs against the concrete
                    // struct's fields) and the reified Self type's own concept instances (a
                    // leftover IArrayLike<$x> pairs against Self's IArrayLike<Number>). Derive
                    // those bindings from the first specialization and re-specialize.
                    if (!tir.AllNodes.All(NodeIsGround))
                    {
                        var extra = DeriveResidualBindings(tir, rf.ReifiedType?.Self, rf.ReturnType);
                        if (extra != null && !extra.IsEmpty)
                        {
                            sub = sub.MergedWith(extra);
                            stats = new MonomorphizeStats();
                            tir = Specialize(baseTir, sub, stats);
                        }
                    }
                }

                var ground = tir != null && tir.AllNodes.All(NodeIsGround);
                results.Add(new MonomorphizedFunction(rf, sub, tir, stats, ground));
            }

            // Library functions whose FIRST parameter is already a CONCRETE type are not reified
            // (ReifiedType only stamps functions whose first-parameter interface the type
            // implements), but the writer still emits them as members of that type
            // (`Matrix(r: LookAt3D)` becomes LookAt3D.Matrix). Cover them directly: their
            // signature is its own ground signature; only Self needs binding.
            var covered = new HashSet<(FunctionDef, string)>();
            foreach (var m in results)
                if (m.Original != null && m.ConcreteType?.Name != null)
                    covered.Add((m.Original, m.ConcreteType.Name));

            foreach (var f in Compilation.FunctionDefinitions ?? Enumerable.Empty<FunctionDef>())
            {
                if (f.Body == null || f.Parameters.Count == 0)
                    continue;
                var firstType = f.Parameters[0].Type?.Def;
                if (firstType == null || !firstType.IsConcrete() || covered.Contains((f, firstType.Name)))
                    continue;
                if (!tirByOriginal.TryGetValue(f, out var baseTir))
                    continue;

                var self = firstType.ToTypeExpression();
                var sub = TypeSubstitution
                    .FromSignature(baseTir.ZonkedParameterTypes, baseTir.ZonkedReturnType,
                        f.Parameters.Select(p => p.Type).ToList(), f.ReturnType)
                    .WithSelf(self);
                var stats = new MonomorphizeStats();
                var tir = Specialize(baseTir, sub, stats);
                if (!tir.AllNodes.All(NodeIsGround))
                {
                    var extra = DeriveResidualBindings(tir, self, f.ReturnType);
                    if (extra != null && !extra.IsEmpty)
                    {
                        sub = sub.MergedWith(extra);
                        stats = new MonomorphizeStats();
                        tir = Specialize(baseTir, sub, stats);
                    }
                }
                results.Add(new MonomorphizedFunction(null, sub, tir, stats,
                    tir.AllNodes.All(NodeIsGround), f, firstType));
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
                    return new TirTypeRef(t.TypeDef, _sub.Apply(t.Type), t.Origin, t.NamespaceQualified);
                case TirLet let:
                    return new TirLet(let.Def, SpecializeNode(let.Value), _sub.Apply(let.Type), let.Origin);
                case TirDefault d:
                    return new TirDefault(_sub.Apply(d.Type), d.Origin);
                case TirName nm:
                    return new TirName(nm.Name, _sub.Apply(nm.Type), nm.Origin);
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

            // Concept re-dispatch (direct case only): a call whose callee is an abstract concept
            // declaration, now that its receiver has become concrete, re-points to that type's
            // concrete implementation — but only on an unambiguous match, so we never mis-dispatch.
            // The EmissionKind is NOT re-derived: it encodes the call-site SHAPE (member access vs
            // arg list), which is what the emitted syntax keys on; the target only changes identity.
            if (callee != null && callee.FunctionType == FunctionType.Concept && ps != null && ps.Count > 0)
            {
                var target = RedispatchResolver?.Invoke(call.Name, ps[0], ps, ret);
                if (target != null)
                {
                    callee = target;
                    _stats.Redispatched++;
                }
                else
                {
                    _stats.DeferredConcept++;
                }
            }

            return new TirCall(callee, call.EmissionKind, ps, ret, args, type, call.Origin, call.Name);
        }

        // --- residual grounding ----------------------------------------------------

        /// <summary>Bindings for residual variables in a specialized (but not fully ground) TIR,
        /// derived from ground context: return-position types pair against the reified return type
        /// (a <c>Tuple2&lt;Angle,$r&gt;</c> against concrete <c>AnglePair</c> pairs element-wise
        /// with its fields), and any leftover interface instance pairs against the reified Self
        /// type's own instance of that concept (<c>IArrayLike&lt;$x&gt;</c> against Time's
        /// <c>IArrayLike&lt;Number&gt;</c>).</summary>
        private TypeSubstitution DeriveResidualBindings(TirFunction tir, TypeExpression self, TypeExpression returnType)
        {
            var map = new Dictionary<string, TypeExpression>();

            // Return positions vs. the ground reified return type. When the reified return is an
            // interface instance the Self type implements (IInterval<Angle> on AnglePair), the
            // stamped method actually returns the CONCRETE Self type (the writer's Self-refinement
            // style), so pair against that — a tuple result then pairs against its fields.
            var groundReturn = returnType;
            if (groundReturn?.Def != null && groundReturn.Def.IsInterface()
                && self != null
                && TypeSubstitution.FindConceptInstance(self, groundReturn.Def.Name) != null)
                groundReturn = self;
            if (groundReturn != null && TypeSubstitution.IsGround(groundReturn))
            {
                void PairReturnPosition(TirNode n)
                {
                    if (n == null)
                        return;
                    if (n.Type != null)
                        ResidualPair(n.Type, groundReturn, map);
                    // A call's declared ReturnType is the PRE-coercion type — a tuple result whose
                    // value type was already coerced to the struct still carries its residual
                    // element variables there (Tuple2<Angle,$r> under an AnglePair-typed node).
                    if (n is TirCall c && c.ReturnType != null)
                        ResidualPair(c.ReturnType, groundReturn, map);
                }

                if (!(tir.Body is TirBlock))
                    PairReturnPosition(tir.Body);
                foreach (var r in tir.AllNodes.OfType<TirReturn>())
                    PairReturnPosition(r.Value);
            }

            // Leftover interface instances vs. the Self type's own concept instances.
            if (self != null)
                foreach (var t in tir.AllNodes.SelectMany(CarriedTypes))
                    if (t?.Def != null && t.Def.IsInterface() && t.TypeArgs.Count > 0
                        && !TypeSubstitution.IsGround(t))
                    {
                        var instance = TypeSubstitution.FindConceptInstance(self, t.Def.Name);
                        if (instance != null)
                            ResidualPair(t, instance, map);
                    }

            return new TypeSubstitution(map);
        }

        /// <summary>Every type a node carries (value type plus call/coercion/new signature types).</summary>
        private static IEnumerable<TypeExpression> CarriedTypes(TirNode n)
        {
            if (n.Type != null)
                yield return n.Type;
            switch (n)
            {
                case TirCall c:
                    if (c.ReturnType != null)
                        yield return c.ReturnType;
                    foreach (var t in c.ParameterTypes)
                        if (t != null)
                            yield return t;
                    break;
                case TirCoerce co:
                    if (co.FromType != null) yield return co.FromType;
                    if (co.ToType != null) yield return co.ToType;
                    break;
                case TirNew nw:
                    if (nw.NewType != null) yield return nw.NewType;
                    break;
            }
        }

        /// <summary>Pair a residual (partially abstract) type against a ground one, binding the
        /// abstract side's variables: recurse on equal heads, bind a bare variable, pair a
        /// <c>Tuple{N}</c> against a concrete N-field struct's field types, and pair an interface
        /// instance against the ground type's instance of that concept.</summary>
        private static void ResidualPair(TypeExpression residual, TypeExpression ground,
            Dictionary<string, TypeExpression> map)
        {
            if (residual?.Def == null || ground?.Def == null || !TypeSubstitution.IsGround(ground))
                return;

            if (residual.Def.Kind == TypeKind.TypeVariable)
            {
                var key = residual.Name;
                if (!map.ContainsKey(key))
                    map[key] = ground;
                return;
            }

            if (residual.Def.Name == ground.Def.Name && residual.TypeArgs.Count == ground.TypeArgs.Count)
            {
                for (var i = 0; i < residual.TypeArgs.Count; i++)
                    ResidualPair(residual.TypeArgs[i], ground.TypeArgs[i], map);
                return;
            }

            // Tuple{N} result for a concrete struct with N fields (the generated implicit
            // tuple-conversion operator): pair element-wise with the field types.
            if (residual.Def.Name.StartsWith("Tuple") && ground.Def.IsConcrete()
                && ground.Def.Fields.Count == residual.TypeArgs.Count)
            {
                for (var i = 0; i < residual.TypeArgs.Count; i++)
                    ResidualPair(residual.TypeArgs[i], ground.Def.Fields[i].Type, map);
                return;
            }

            // An interface instance for a ground type that implements it.
            if (residual.Def.IsInterface())
            {
                var instance = TypeSubstitution.FindConceptInstance(ground, residual.Def.Name);
                if (instance != null && instance.TypeArgs.Count == residual.TypeArgs.Count)
                    for (var i = 0; i < residual.TypeArgs.Count; i++)
                        ResidualPair(residual.TypeArgs[i], instance.TypeArgs[i], map);
            }
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

        /// <summary>A node is fully ground when its value type — and, for calls/coercions/new, every
        /// type it carries — is ground, and it is not an unresolved-elaboration placeholder.
        /// SYNTACTIC nodes (a null-callee call, a bare name) are exempt from the value-type
        /// requirement: their emission is determined by name + shape alone, and their type is a
        /// best-effort context annotation the compiler may genuinely not know (a handwritten
        /// intrinsic member).</summary>
        internal static bool NodeIsGround(TirNode n)
        {
            if (n is TirUnresolved)
                return false;
            if (n is TirName)
                return true;
            if (n is TirCall syntactic && syntactic.Callee == null)
                return true;
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

        // Normally derived from the oracle entry; set explicitly for the non-reified case (a
        // library function whose first parameter is already a concrete type, which the reifier
        // never stamps but the writer still emits as a member of that type).
        private readonly FunctionDef _original;
        private readonly TypeDef _concreteType;

        public FunctionDef Original => _original ?? Reified?.Original;
        public TypeDef ConcreteType => _concreteType ?? Reified?.ReifiedType?.Type;
        public bool HasBody => Tir != null;

        public MonomorphizedFunction(ReifiedFunction reified, TypeSubstitution substitution,
            TirFunction tir, MonomorphizeStats stats, bool isFullyGround,
            FunctionDef original = null, TypeDef concreteType = null)
        {
            Reified = reified;
            Substitution = substitution;
            Tir = tir;
            Stats = stats ?? new MonomorphizeStats();
            IsFullyGround = isFullyGround;
            _original = original;
            _concreteType = concreteType;
        }

        public override string ToString()
            => $"{ConcreteType?.Name}.{Original?.Name} {Substitution}"
               + (HasBody ? IsFullyGround ? " [ground]" : " [partial]" : " [no body]");
    }
}
