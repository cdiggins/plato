using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Compiler.Checking
{
    /// <summary>
    /// The solve pass. Consumes a <see cref="ConstraintSystem"/> and produces a substitution
    /// (a binding for each unification variable) plus located diagnostics. It is:
    ///
    ///   * total — never throws; a clash / no-match / ambiguity / recursive type is a diagnostic;
    ///   * occurs-checked — a variable is never bound to a type that contains it (no infinite types);
    ///   * deferred-commitment — an overloaded call is resolved only once its argument types are
    ///     ground; the most-specific surviving candidate is committed, 0 is a no-match error, and an
    ///     unresolved tie with differing return types is reported rather than silently picked.
    ///
    /// Argument matching is tiered, mirroring Plato's own resolution primitives
    /// (<see cref="TypeExtensions.IsImplementing"/> and <see cref="Compilation.GetRelation"/>):
    ///
    ///   exact (unify) &lt; generic (bind a type variable) &lt; concept (arg implements an interface
    ///   parameter, with the return refined to the concrete arg — "Self") &lt; conversion (an implicit
    ///   cast relation exists). Lower cost wins, so a concrete overload beats a generic one and an
    ///   exact match beats a conversion.
    /// </summary>
    public class Solver
    {
        public Compilation Compilation { get; }
        public Dictionary<string, TypeExpression> Substitution { get; } = new Dictionary<string, TypeExpression>();
        public List<CheckDiagnostic> Diagnostics { get; } = new List<CheckDiagnostic>();

        /// <summary>The candidate the solver committed for each cleanly-resolved call. This is the
        /// bridge to elaboration: it remembers *which* overload won, its instantiated signature, and
        /// how each argument matched (so a conversion becomes an explicit IR node). Populated in
        /// <see cref="CommitCandidate"/>; ambiguous / no-match calls are absent.</summary>
        public Dictionary<FunctionCall, ResolvedCall> ResolvedCalls { get; } = new Dictionary<FunctionCall, ResolvedCall>();

        private readonly TypeVarFactory _vars;
        private const int MaxDepth = 512;

        // Argument-match cost tiers (lower is more specific).
        private const int ExactCost = 0;
        private const int GenericCost = 2;
        private const int ConceptCost = 3;
        private const int ConversionCost = 5;

        public Solver(Compilation compilation, TypeVarFactory vars = null)
        {
            Compilation = compilation;
            _vars = vars ?? new TypeVarFactory();
        }

        public void Solve(ConstraintSystem system)
        {
            Diagnostics.AddRange(system.Diagnostics);

            // Phase 1: equalities are unconditional.
            foreach (var eq in system.Equalities)
                Unify(eq.A, eq.B, Substitution, eq.Origin, record: true);

            // Phase 2: overloads and return-coercions to a joint fixpoint. A call is resolved when
            // its argument types are ground (a function-shaped argument may keep holes — the chosen
            // candidate's signature determines a lambda's parameter types); resolving one may
            // ground another (its result feeds an outer call, a coercion target grounds a tuple).
            var pending = system.Overloads.ToList();
            var coercions = system.Coercions.ToList();
            bool progress = true;
            while (progress && (pending.Count > 0 || coercions.Count > 0))
            {
                progress = false;

                var still = new List<OverloadConstraint>();
                foreach (var oc in pending)
                {
                    if (ResolveOverload(oc, force: false)) progress = true;
                    else still.Add(oc);
                }
                pending = still;

                var stillCoerce = new List<CoercionConstraint>();
                foreach (var cc in coercions)
                {
                    if (ResolveCoercion(cc, force: false)) progress = true;
                    else stillCoerce.Add(cc);
                }
                coercions = stillCoerce;
            }

            // Whatever could not be grounded is resolved on a best-effort basis and reported.
            // Each forced resolution may unblock others, so re-enter the fixpoint after each one.
            while (pending.Count > 0)
            {
                ResolveOverload(pending[0], force: true);
                pending.RemoveAt(0);
                progress = true;
                while (progress && pending.Count > 0)
                {
                    progress = false;
                    var still = new List<OverloadConstraint>();
                    foreach (var oc in pending)
                    {
                        if (ResolveOverload(oc, force: false)) progress = true;
                        else still.Add(oc);
                    }
                    pending = still;
                }
            }

            foreach (var cc in coercions)
                ResolveCoercion(cc, force: true);
        }

        // --- overload resolution -------------------------------------------------

        /// <summary>An instantiated candidate: fresh generic holes, interface params replaced by
        /// fresh "concept" variables (recorded in <see cref="ConceptOf"/>), and a return type
        /// refined to share a concept variable when it names the same interface as a parameter.</summary>
        private class Candidate
        {
            public FunctionDef Function;
            public TypeExpression[] Ps;
            public TypeExpression Ret;
            public Dictionary<string, TypeExpression> ConceptOf = new Dictionary<string, TypeExpression>();
        }

        private bool ResolveOverload(OverloadConstraint oc, bool force)
        {
            var args = oc.ArgTypes.Select(a => Zonk(a, Substitution)).ToList();

            // Defer until the argument types are ground — EXCEPT function-shaped arguments
            // (Function{N}), whose holes are lambda parameter/result types that the chosen
            // candidate's signature is supposed to determine (checking-mode inference for HOFs).
            if (!force && args.Any(a => HasVar(a) && !IsFunctionShaped(a)))
                return false; // not ground yet — defer

            // Trial each candidate on a scratch substitution; keep the viable ones with their cost.
            var viable = new List<(Candidate cand, int cost)>();
            foreach (var f in oc.Candidates)
            {
                if (f.Parameters.Count != args.Count)
                    continue;
                var cand = InstantiateCandidate(f);
                var scratch = new Dictionary<string, TypeExpression>(Substitution);
                var cost = 0;
                var ok = true;
                for (var i = 0; i < args.Count; i++)
                {
                    var m = MatchArg(args[i], cand.Ps[i], cand, scratch);
                    if (!m.ok) { ok = false; break; }
                    cost += m.cost;
                }
                if (ok)
                    viable.Add((cand, cost));
            }

            if (viable.Count == 0)
            {
                Report(DiagnosticSeverity.Error, "CHK201",
                    $"No overload of '{oc.Name}' matches argument types ({string.Join(", ", args)})", oc.Origin);
                return true;
            }

            // "Most specific wins": keep only the lowest-cost candidates.
            var best = viable.Min(v => v.cost);
            var winners = viable.Where(v => v.cost == best).Select(v => v.cand).ToList();

            // Specificity tie-break (C#-style): among equal-cost winners, prefer the MOST-DERIVED
            // parameter types. A candidate whose every parameter is a subtype-or-equal of another's
            // — strictly so at some position — is more specific and eliminates the other. Resolves
            // `IArray2D`-vs-`IArray` style ties (`x.PointGrid.Map(f)`, where the 2D overload is the
            // intended, more specific one). No effect unless it narrows a genuine tie.
            if (winners.Count > 1)
            {
                var mostSpecific = winners
                    .Where(w => !winners.Any(o => !ReferenceEquals(o, w) && MoreSpecific(o, w, Substitution)))
                    .ToList();
                if (mostSpecific.Count >= 1 && mostSpecific.Count < winners.Count)
                    winners = mostSpecific;
            }

            if (winners.Count == 1)
            {
                CommitCandidate(winners[0], args, oc);
                return true;
            }

            // A tie at the best cost: well-defined iff all winners agree on the return type — OR
            // every winner's return is still an unbound type VARIABLE. The latter is the
            // concept-method case: each candidate returns its own fresh Self var (the winning trials'
            // scratch bindings were discarded, so the shared var is unbound here). These carry no
            // genuine disagreement — monomorphization grounds every one of them to the SAME concrete
            // argument type, and re-dispatch there selects the right concrete overload by type. So
            // committing the first is an identity hint, not a semantic choice (type-checker handoff:
            // "re-dispatch is only an identity refinement").
            var zonkedRets = winners
                .Select(w => Zonk(w.Ret, new Dictionary<string, TypeExpression>(Substitution)))
                .ToList();
            var rets = zonkedRets.Select(r => r?.ToString() ?? "?").Distinct().ToList();
            var allVarReturns = zonkedRets.All(r => r != null && IsVar(r));

            if (rets.Count == 1 || allVarReturns)
            {
                CommitCandidate(winners[0], args, oc);
                Report(DiagnosticSeverity.Info, "CHK202",
                    $"Call to '{oc.Name}' has {winners.Count} equally-specific overloads with a common return type", oc.Origin);
            }
            else if (args.Any(HasVar))
            {
                // A BOUNDED-POLYMORPHIC call: an argument is still an unbound (constraint-bounded)
                // type variable — e.g. `x.Max - x.Min` in `IBounds.Size`, whose receiver has the
                // interface's own parameter type (`$TValue : IVectorLike, IDifference<$TDelta>`). The
                // concrete overloads only "tie" by binding that abstract variable to each concrete
                // type, which is illegitimate: at the generic level the body genuinely cannot choose,
                // but monomorphization dispatches the call by the GROUNDED argument type, where it is
                // unambiguous. So this is not a real ambiguity — defer it (name+shape emission,
                // exactly as an unresolved generic call) rather than reporting a false CHK203. A
                // genuine ambiguity has ground arguments and so does not reach this branch.
                Report(DiagnosticSeverity.Info, "CHK204",
                    $"Bounded-polymorphic call to '{oc.Name}': {winners.Count} concrete overloads tie on an unbound type variable; deferred to monomorphization", oc.Origin);
            }
            else
            {
                Report(DiagnosticSeverity.Error, "CHK203",
                    $"Ambiguous call to '{oc.Name}': {winners.Count} equally-specific overloads with return types {string.Join(" | ", rets)}", oc.Origin);
            }
            return true;
        }

        private void CommitCandidate(Candidate cand, IReadOnlyList<TypeExpression> args, OverloadConstraint oc)
        {
            // Re-run the matches on the real substitution to bind generic/concept variables. Because
            // the base substitution and argument order are identical to the winning trial, this
            // reproduces that trial's per-argument outcomes exactly, so the recorded match kinds are
            // faithful. Capture them (and any cast function) for the elaborate pass.
            var kinds = new ArgMatchKind[args.Count];
            var conversions = new IFunction[args.Count];
            var paramTypes = new TypeExpression[args.Count];
            for (var i = 0; i < args.Count; i++)
            {
                var m = MatchArg(args[i], cand.Ps[i], cand, Substitution);
                kinds[i] = m.kind;

                // The declared parameter type: a concept parameter is recorded as its interface (the
                // fresh concept var only exists to carry the Self binding), everything else as the
                // instantiated parameter.
                var p = cand.Ps[i];
                var target = p != null && IsVar(p) && cand.ConceptOf.TryGetValue(p.Name, out var concept)
                    ? concept
                    : p;
                paramTypes[i] = Zonk(target, Substitution);

                if (m.kind == ArgMatchKind.Conversion)
                    conversions[i] = FindConversion(Zonk(args[i], Substitution), paramTypes[i]);
            }

            // Refine the element types of generic concept parameters from the concrete argument
            // (List<Number> : IArray<$T> => $T = Number). Best-effort and post-commit: it only
            // sharpens the result type and never rejects the already-chosen overload.
            for (var i = 0; i < args.Count; i++)
            {
                var p = cand.Ps[i];
                if (p != null && IsVar(p) && cand.ConceptOf.TryGetValue(p.Name, out var concept))
                {
                    var direct = DirectInstance(Zonk(args[i], Substitution), concept.Def);
                    if (direct != null)
                        BindBestEffort(concept, direct, Substitution);
                }
            }

            Unify(oc.Result, cand.Ret, Substitution, oc.Origin, record: true);

            // Record the committed decision (only for real calls; synthetic unit-test constraints
            // carry a null Call). Zonk after the result unify so the signature is as ground as
            // possible. Re-zonk the parameter types too, in case element inference sharpened them.
            if (oc.Call != null)
            {
                for (var i = 0; i < paramTypes.Length; i++)
                    paramTypes[i] = Zonk(paramTypes[i], Substitution);
                ResolvedCalls[oc.Call] = new ResolvedCall(
                    oc.Call, cand.Function, paramTypes, Zonk(cand.Ret, Substitution), kinds, conversions);
            }
        }

        /// <summary>Whether candidate <paramref name="a"/> is STRICTLY more specific than
        /// <paramref name="b"/>: every parameter of <paramref name="a"/> is a subtype-or-equal of the
        /// corresponding parameter of <paramref name="b"/>, and at least one is a strict subtype
        /// (implements/inherits it). Used to break equal-cost ties toward the most-derived overload,
        /// mirroring C#'s better-function-member rule.</summary>
        private bool MoreSpecific(Candidate a, Candidate b, Dictionary<string, TypeExpression> sub)
        {
            if (a.Ps.Length != b.Ps.Length)
                return false;
            var strict = false;
            for (var i = 0; i < a.Ps.Length; i++)
            {
                var pa = SpecificityType(a, i, sub);
                var pb = SpecificityType(b, i, sub);
                if (pa?.Def == null || pb?.Def == null)
                    return false; // not comparable — do not claim more-specific
                if (pa.Def.Name == pb.Def.Name)
                    continue;
                if (ConceptClosure.FindInstance(pa, pb.Def.Name) != null)
                    strict = true; // pa <: pb at this position
                else
                    return false;  // pa is not a subtype of pb — a is not uniformly more specific
            }
            return strict;
        }

        /// <summary>The type used to compare a candidate's parameter for specificity: a concept
        /// parameter contributes its interface (the fresh concept var carries no usable name),
        /// everything else the instantiated parameter, zonked.</summary>
        private TypeExpression SpecificityType(Candidate cand, int i, Dictionary<string, TypeExpression> sub)
        {
            var p = cand.Ps[i];
            if (p != null && IsVar(p) && cand.ConceptOf.TryGetValue(p.Name, out var concept))
                return Zonk(concept, sub);
            return Zonk(p, sub);
        }

        /// <summary>
        /// Match one argument against one (instantiated) parameter, returning success, a cost, and
        /// the match kind. Order of preference: exact unify, generic binding, concept satisfaction,
        /// implicit cast. The ok/cost values are unchanged from before the kind was added, so
        /// overload ranking is unaffected.
        /// </summary>
        private (bool ok, int cost, ArgMatchKind kind) MatchArg(TypeExpression argType, TypeExpression param, Candidate cand, Dictionary<string, TypeExpression> sub)
        {
            if (param == null || argType == null)
                return (true, ExactCost, ArgMatchKind.Exact);

            // A concept parameter: the argument must implement the interface (or convert to it).
            if (IsVar(param) && cand.ConceptOf.TryGetValue(param.Name, out var concept))
            {
                if (SatisfiesConcept(argType, Zonk(concept, sub), sub))
                {
                    // Viability + Self here; SatisfiesConcept also binds the concept's element
                    // holes from the argument's instance (IVectorLike : IArrayLike<Number> binds
                    // $T = Number). The coarser post-commit refinement in CommitCandidate remains
                    // for the paths this walk does not reach.
                    Unify(param, argType, sub, null, record: false); // Self: bind the concept var to the concrete arg
                    return (true, ConceptCost, ArgMatchKind.Concept);
                }
                if (HasConversion(argType, concept))
                    return (true, ConversionCost, ArgMatchKind.Conversion);
                return (false, 0, ArgMatchKind.Exact);
            }

            var generic = IsVar(ResolveTop(param, sub));
            if (Unify(param, argType, sub, null, record: false))
                return (true, generic ? GenericCost : ExactCost, generic ? ArgMatchKind.Generic : ArgMatchKind.Exact);

            if (HasConversion(argType, param))
                return (true, ConversionCost, ArgMatchKind.Conversion);

            return (false, 0, ArgMatchKind.Exact);
        }

        /// <summary>
        /// Solver-side concept satisfaction: does <paramref name="argType"/> implement
        /// <paramref name="concept"/>? Unlike <see cref="TypeExtensions.IsImplementing"/> (which
        /// requires equal type-argument counts up front, so a non-generic interface like
        /// <c>IVectorLike</c> can never satisfy <c>IArrayLike&lt;$T&gt;</c>), this walks the
        /// argument's transitive Implements/Inherits closure with per-level type-argument
        /// substitution, and — on a name match — unifies the found instance's arguments with the
        /// concept's (binding element holes: <c>IVectorLike : IArrayLike&lt;Number&gt;</c> binds
        /// <c>$T = Number</c>). A type variable or type parameter satisfies anything (it may yet be
        /// bound); <c>Self</c> satisfies anything (the reifier decides what Self is).
        /// </summary>
        private bool SatisfiesConcept(TypeExpression argType, TypeExpression concept, Dictionary<string, TypeExpression> sub)
        {
            if (argType?.Def == null || concept?.Def == null)
                return true;
            if (IsVar(argType) || argType.Def.Kind == TypeKind.TypeParameter
                || argType.Def.Kind == TypeKind.SelfType)
                return true;

            if (argType.Def.Name == concept.Def.Name)
                return UnifyAllOrNothing(argType, concept, sub);

            foreach (var inst in ConceptClosure.InstancesOf(argType))
                if (inst?.Def != null && inst.Def.Name == concept.Def.Name
                    && UnifyAllOrNothing(inst, concept, sub))
                    return true;

            return false;
        }

        /// <summary>Unify on a scratch copy; adopt the bindings only on full success. Returns
        /// whether unification succeeded (contrast <see cref="BindBestEffort"/>, which is void).</summary>
        private bool UnifyAllOrNothing(TypeExpression a, TypeExpression b, Dictionary<string, TypeExpression> sub)
        {
            var scratch = new Dictionary<string, TypeExpression>(sub);
            if (!Unify(a, b, scratch, null, record: false))
                return false;
            foreach (var kv in scratch)
                sub[kv.Key] = kv.Value;
            return true;
        }

        /// <summary>A top-level function type (<c>Function{N}</c>) — a lambda's shape.</summary>
        private static bool IsFunctionShaped(TypeExpression t)
            => t?.Def?.Name != null && t.Def.Name.StartsWith("Function");

        // --- return-position coercion ---------------------------------------------

        /// <summary>
        /// Resolve a return-position coercion: the body's type must unify with — or implicitly
        /// convert to — the declared return type. Accepted conversions mirror what the generated C#
        /// compiles implicitly: a cast relation (<c>Vector3</c> → <c>Point3D</c>), a same-shape
        /// tuple for a struct (<c>(a, b)</c> → <c>Line2D</c>, binding the tuple's element holes
        /// from the struct's fields), and a value for an interface it implements. When not forced,
        /// defers while either side still has holes a later resolution could fill.
        /// </summary>
        private bool ResolveCoercion(CoercionConstraint cc, bool force)
        {
            var from = Zonk(cc.From, Substitution);
            var to = Zonk(cc.To, Substitution);

            // A variable on either side: unifying is both the cheapest and the most informative
            // outcome (it grounds the body type from the declared return, or vice versa).
            if (from == null || to == null || IsVar(from) || IsVar(to))
            {
                Unify(cc.From, cc.To, Substitution, cc.Origin, record: true);
                return true;
            }

            if (!force && (HasVar(from) || HasVar(to)))
                return false; // inner holes may still be filled — defer

            if (TryCoerce(from, to, Substitution))
                return true;

            // Nothing fits: report as the unification failure it is (located).
            Unify(from, to, Substitution, cc.Origin, record: true);
            return true;
        }

        /// <summary>
        /// Whether <paramref name="from"/> coerces to <paramref name="to"/> under the same rules the
        /// generated C# compiles implicitly, binding any variables the match determines (committed to
        /// <paramref name="sub"/> only on full success):
        ///   * identity / unification;
        ///   * a declared cast relation (<c>Vector3</c> → <c>Point3D</c>);
        ///   * a value into a single-field struct wrapping a type it coerces to (<c>Vector3</c> →
        ///     <c>Translation3D { Vector: Vector3 }</c>) — the generated single-field implicit operator;
        ///   * a same-shape <c>Tuple{N}</c> into an N-field struct, each element coercing to its field
        ///     (the generated tuple-constructor implicit operator applies per-argument conversions,
        ///     so <c>(Vector3, Number)</c> → <c>AxisAngle { Axis: Vector3, Angle: Angle }</c> works via
        ///     <c>Number</c> → <c>Angle</c>);
        ///   * a value into an interface it implements.
        /// Mirrors <see cref="CSharpConcreteTypeWriter"/>'s emitted implicit operators, so a coercion
        /// accepted here is one the runtime actually performs.
        /// </summary>
        private bool TryCoerce(TypeExpression from, TypeExpression to, Dictionary<string, TypeExpression> sub, int depth = 0)
        {
            if (from == null || to == null || depth > MaxDepth)
                return true;
            if (UnifyAllOrNothing(from, to, sub))
                return true;
            if (HasConversion(from, to))
                return true;
            if (TupleConvertsToStruct(from, to, sub, depth))
                return true;
            if (ValueConvertsToSingleFieldStruct(from, to, sub, depth))
                return true;
            if (to.Def != null && to.Def.IsInterface() && SatisfiesConcept(from, to, sub))
                return true;
            return false;
        }

        /// <summary>A <c>Tuple{N}</c> converts to a concrete struct with N fields, each element
        /// COERCING (not merely unifying) to its field type — the generated tuple-constructor
        /// implicit operator applies an implicit conversion per constructor argument.</summary>
        private bool TupleConvertsToStruct(TypeExpression from, TypeExpression to, Dictionary<string, TypeExpression> sub, int depth)
        {
            if (from?.Def?.Name == null || !from.Def.Name.StartsWith("Tuple"))
                return false;
            if (to?.Def == null || !to.Def.IsConcrete() || to.Def.Fields.Count != from.TypeArgs.Count)
                return false;
            var map = BuildParamSubstitution(to);
            var scratch = new Dictionary<string, TypeExpression>(sub);
            for (var i = 0; i < from.TypeArgs.Count; i++)
            {
                var fieldType = Substitute(to.Def.Fields[i].Type, map);
                if (!TryCoerce(Zonk(from.TypeArgs[i], scratch), Zonk(fieldType, scratch), scratch, depth + 1))
                    return false;
            }
            foreach (var kv in scratch)
                sub[kv.Key] = kv.Value;
            return true;
        }

        /// <summary>A value converts to a single-field concrete struct whose (non-interface) field
        /// type it coerces to — the generated <c>implicit operator {Struct}({FieldType})</c> (emitted
        /// by <see cref="CSharpConcreteTypeWriter"/> only when the field is not an interface).
        /// <c>Matrix4x4</c> → <c>Transform3D { Matrix: Matrix4x4 }</c>.</summary>
        private bool ValueConvertsToSingleFieldStruct(TypeExpression from, TypeExpression to, Dictionary<string, TypeExpression> sub, int depth)
        {
            if (to?.Def == null || !to.Def.IsConcrete() || to.Def.Fields.Count != 1)
                return false;
            var fieldType = to.Def.Fields[0].Type;
            if (fieldType?.Def == null || fieldType.Def.IsInterface())
                return false;
            fieldType = Substitute(fieldType, BuildParamSubstitution(to));
            var scratch = new Dictionary<string, TypeExpression>(sub);
            if (!TryCoerce(Zonk(from, scratch), Zonk(fieldType, scratch), scratch, depth + 1))
                return false;
            foreach (var kv in scratch)
                sub[kv.Key] = kv.Value;
            return true;
        }

        /// <summary>
        /// The instance of <paramref name="conceptDef"/> the argument implements *directly* (as itself,
        /// or via a one-level Implements/Inherits clause), with the argument's type arguments
        /// substituted in — e.g. List&lt;Number&gt; : IArray&lt;T&gt; yields IArray&lt;Number&gt;. Returns null
        /// for purely transitive implementations, where a one-level substitution would be incomplete.
        /// </summary>
        private TypeExpression DirectInstance(TypeExpression argType, TypeDef conceptDef)
        {
            if (argType.Def.Name == conceptDef.Name)
                return argType;
            var map = BuildParamSubstitution(argType);
            foreach (var impl in argType.Def.Implements.Concat(argType.Def.Inherits))
                if (impl?.Def != null && impl.Def.Name == conceptDef.Name)
                    return Substitute(impl, map);
            return null;
        }

        /// <summary>Unify on a scratch copy and adopt the bindings only if it fully succeeds, so a
        /// partial/failed match never leaves spurious bindings or rejects the candidate.</summary>
        private void BindBestEffort(TypeExpression a, TypeExpression b, Dictionary<string, TypeExpression> sub)
        {
            var scratch = new Dictionary<string, TypeExpression>(sub);
            if (Unify(a, b, scratch, null, record: false))
                foreach (var kv in scratch)
                    sub[kv.Key] = kv.Value;
        }

        /// <summary>Map a type's declared parameters to its actual arguments (List's T -&gt; Number).</summary>
        private static Dictionary<string, TypeExpression> BuildParamSubstitution(TypeExpression t)
        {
            var map = new Dictionary<string, TypeExpression>();
            var tps = t.Def.TypeParameters;
            for (var i = 0; i < tps.Count && i < t.TypeArgs.Count; i++)
                map[tps[i].Name] = t.TypeArgs[i];
            return map;
        }

        private bool HasConversion(TypeExpression from, TypeExpression to)
            => FindConversion(from, to) != null;

        /// <summary>The cast function witnessing an implicit conversion from <paramref name="from"/>
        /// to <paramref name="to"/>, or null if none. Looks up cast relations directly rather than via
        /// Compilation.GetRelation, which throws when a source has multiple relations to the same
        /// destination — the solver must stay total. The returned function is recorded on a
        /// <see cref="ResolvedCall"/> so the elaborator's TirCoerce node names the exact conversion.</summary>
        private IFunction FindConversion(TypeExpression from, TypeExpression to)
        {
            if (Compilation?.TypeRelations == null || from?.Def == null || to?.Def == null)
                return null;
            if (!Compilation.TypeRelations.RelationLookup.TryGetValue(from.Def, out var list))
                return null;
            return list.FirstOrDefault(rel => rel.Cast != null && rel.Dest.Equals(to.Def))?.Cast;
        }

        // --- instantiation -------------------------------------------------------

        /// <summary>
        /// Give a candidate's generic holes fresh unification variables, replace interface
        /// parameters with fresh "concept" variables (so a concrete argument binds through them),
        /// and refine an interface return type to the concept variable of the parameter it names.
        /// A CONCEPT METHOD's <c>Self</c> becomes a concept variable constrained to the owning
        /// interface (with the interface's type parameters as fresh holes), so
        /// <c>Divide(self: Self, other: Number): Self</c> on <c>IScalarArithmetic</c> matches any
        /// implementing argument and returns its concrete type — the "Self" behavior.
        /// </summary>
        private Candidate InstantiateCandidate(FunctionDef f)
        {
            var map = new Dictionary<string, TypeExpression>();
            foreach (var t in f.Parameters.Select(p => p.Type).Append(f.ReturnType))
                CollectHoles(t, map);

            var cand = new Candidate { Function = f, Ps = new TypeExpression[f.Parameters.Count] };
            var conceptVarByInstance = new Dictionary<string, TypeExpression>();

            // Concept method: map "Self" (anywhere in the signature — Substitute keys by name) to a
            // fresh concept variable constrained to the owning interface's instance.
            if (f.OwnerType != null && f.OwnerType.IsInterface())
            {
                var ownerInstance = f.OwnerType.ToTypeExpression();
                CollectHoles(ownerInstance, map);
                ownerInstance = Substitute(ownerInstance, map);
                var selfVar = _vars.Fresh("C");
                cand.ConceptOf[selfVar.Name] = ownerInstance;
                conceptVarByInstance[ownerInstance.ToString()] = selfVar;
                map["Self"] = selfVar;
            }

            for (var i = 0; i < cand.Ps.Length; i++)
            {
                var pt = Substitute(f.Parameters[i].Type, map);
                if (pt?.Def != null && pt.Def.IsInterface())
                {
                    var v = _vars.Fresh("C");
                    cand.ConceptOf[v.Name] = pt;
                    conceptVarByInstance[pt.ToString()] = v;
                    cand.Ps[i] = v;
                }
                else
                {
                    cand.Ps[i] = pt;
                }
            }

            var ret = Substitute(f.ReturnType, map);
            if (ret != null && conceptVarByInstance.TryGetValue(ret.ToString(), out var rv))
                ret = rv; // Self-style refinement: return the concrete argument's type
            cand.Ret = ret;

            return cand;
        }

        private void CollectHoles(TypeExpression t, Dictionary<string, TypeExpression> map)
        {
            if (t == null) return;
            if (IsHole(t) && !map.ContainsKey(t.Name))
                map[t.Name] = _vars.Fresh("G");
            foreach (var a in t.TypeArgs)
                CollectHoles(a, map);
        }

        private static bool IsHole(TypeExpression t)
            => t.Def != null && (t.Def.Kind == TypeKind.TypeVariable || t.Def.Kind == TypeKind.TypeParameter);

        private TypeExpression Substitute(TypeExpression t, IReadOnlyDictionary<string, TypeExpression> map)
        {
            if (t == null) return null;
            if (map.TryGetValue(t.Name, out var replace))
                return replace;
            if (t.TypeArgs.Count == 0)
                return t;
            return new TypeExpression(t.Def, t.TypeArgs.Select(a => Substitute(a, map)).ToArray());
        }

        // --- unification ---------------------------------------------------------

        private static bool IsVar(TypeExpression t)
            => t.Def != null && t.Def.Kind == TypeKind.TypeVariable;

        public bool Unify(TypeExpression a, TypeExpression b, Dictionary<string, TypeExpression> sub, Symbol origin, bool record, int depth = 0)
        {
            if (a == null || b == null || depth > MaxDepth)
                return true;

            a = ResolveTop(a, sub);
            b = ResolveTop(b, sub);

            if (IsVar(a) && IsVar(b) && a.Name == b.Name)
                return true;
            if (IsVar(a))
                return Bind(a, b, sub, origin, record);
            if (IsVar(b))
                return Bind(b, a, sub, origin, record);

            // `Self` is compatible with anything: it is a placeholder the reifier replaces with
            // the concrete type a function is stamped onto, so at generic-check time both
            // `Self ~ Vector3` and `Self ~ IArrayLike<T>` are satisfiable-by-substitution. No
            // binding: Self stays Self in the solved view and monomorphization grounds it.
            if (a.Def?.Kind == TypeKind.SelfType || b.Def?.Kind == TypeKind.SelfType)
                return true;

            // Both are named (or rigid type parameters): match nominally.
            if (a.Def?.Name != b.Def?.Name)
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK101", $"Cannot unify '{a}' with '{b}'", origin);
                return false;
            }
            if (a.TypeArgs.Count != b.TypeArgs.Count)
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK102", $"Type argument count mismatch between '{a}' and '{b}'", origin);
                return false;
            }

            var ok = true;
            for (var i = 0; i < a.TypeArgs.Count; i++)
                ok &= Unify(a.TypeArgs[i], b.TypeArgs[i], sub, origin, record, depth + 1);
            return ok;
        }

        private bool Bind(TypeExpression var, TypeExpression t, Dictionary<string, TypeExpression> sub, Symbol origin, bool record)
        {
            if (IsVar(t) && t.Name == var.Name)
                return true;
            if (Occurs(var.Name, t, sub))
            {
                if (record)
                    Report(DiagnosticSeverity.Error, "CHK103", $"Recursive type: '{var.Name}' occurs in '{Zonk(t, sub)}'", origin);
                return false;
            }
            sub[var.Name] = t;
            return true;
        }

        private bool Occurs(string name, TypeExpression t, Dictionary<string, TypeExpression> sub, int depth = 0)
        {
            if (t == null || depth > MaxDepth) return false;
            t = ResolveTop(t, sub);
            if (IsVar(t))
                return t.Name == name;
            return t.TypeArgs.Any(a => Occurs(name, a, sub, depth + 1));
        }

        private static TypeExpression ResolveTop(TypeExpression t, Dictionary<string, TypeExpression> sub)
        {
            var depth = 0;
            while (t != null && IsVar(t) && sub.TryGetValue(t.Name, out var u) && depth++ < MaxDepth)
                t = u;
            return t;
        }

        /// <summary>Fully apply the substitution (deep), yielding a variable-free type where solved.</summary>
        public TypeExpression Zonk(TypeExpression t, Dictionary<string, TypeExpression> sub = null, int depth = 0)
        {
            sub ??= Substitution;
            if (t == null || depth > MaxDepth) return t;
            t = ResolveTop(t, sub);
            if (t == null || t.TypeArgs.Count == 0)
                return t;
            return new TypeExpression(t.Def, t.TypeArgs.Select(a => Zonk(a, sub, depth + 1)).ToArray());
        }

        private static bool HasVar(TypeExpression t)
            => t != null && (IsVar(t) || t.TypeArgs.Any(HasVar));

        private void Report(DiagnosticSeverity severity, string code, string message, Symbol origin)
            => Diagnostics.Add(new CheckDiagnostic(severity, code, message, origin));

        public bool Succeeded => Diagnostics.All(d => d.Severity != DiagnosticSeverity.Error);
    }
}
