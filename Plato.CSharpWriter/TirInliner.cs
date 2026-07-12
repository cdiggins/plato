using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Source-level function inlining (--inline; roadmap P3.2 "beta reduction"). A resolved
/// <see cref="TirCall"/> whose callee has a fully-ground TIR body for the receiver's concrete
/// type is replaced by that body with parameters substituted by the argument nodes. Purity makes
/// the substitution always SOUND; the rules below keep it from duplicating work or changing name
/// binding:
///
///   * expression bodies only (no statement trees / lets / loops);
///   * the callee body must be self-contained under a foreign enclosing type: no bare-name
///     lookups (<see cref="TirName"/>, eta-forwarding lambdas), no unresolved nodes; `Self` type
///     references are rebound to the callee's concrete type;
///   * a parameter referenced more than once only accepts a CHEAP argument (parameter /
///     variable / literal / type ref); a parameter referenced AT MOST once (and not under a
///     lambda) accepts any non-lambda argument — no expression is ever evaluated more times
///     than the call would have (a use inside a conditional branch may evaluate it FEWER
///     times, which for the pure language only changes exception timing);
///   * lambda literals never substitute (they would lose their delegate target-typing);
///   * call sites under a lambda inline only lambda-free callee bodies with all-cheap
///     arguments (a substituted lambda or compound capture would trip the per-lambda
///     capture hoist);
///   * nullary calls (Constants.X) inline their static TIR bodies under the same rules;
///   * a size budget per callee body and iteration to a fixpoint under a pass cap (inlining
///     exposes further calls — MULTI-LEVEL; the cap bounds growth and terminates recursion).
///
/// Runs BEFORE the component unroller and array materializer, so MapComponents/Map call sites
/// hidden inside small library functions become visible to those transforms — the point of the
/// exercise: after inlining, functional map/fold plumbing is exposed where loop-level rewrites
/// can see it.
/// </summary>
public static class TirInliner
{
    private const int MaxBodyNodes = 120;
    private const int MaxPasses = 8;

    // Deterministic fresh-name counter for collision renames; reset per generation
    // (CSharpWriter.WriteAll) so repeated runs produce identical output.
    public static int NextRenameId;

    public static TirFunction Inline(TirFunction tir, CSharpWriter writer, string ownerTypeName, out int inlinedCalls)
    {
        inlinedCalls = 0;
        if (!writer.InlineCalls || tir?.Body == null)
            return tir;

        var body = tir.Body;
        for (var pass = 0; pass < MaxPasses; pass++)
        {
            // Names bound anywhere in the CALLER: an inlined lambda parameter that shadows one
            // of these is a CS0136 at emission, so such parameters are alpha-renamed on inline.
            var callerNames = CollectBoundNames(tir, body);

            var count = new int[1];
            var lambdaDepth = 0;
            var etaDepth = 0;
            // Call sites INSIDE lambdas inline restrictively (lambda-free callee, cheap args):
            // an inlined body that itself contains a lambda capturing substituted expressions
            // trips the per-lambda capture hoist into declaring locals out of scope. Eta-shaped
            // lambdas are OPAQUE: rewriting their bodies would destroy the exact forwarding
            // shape the emitter recovers as a bare member name.
            body = TirRewrite.Rewrite(body, n =>
                etaDepth == 0 && n is TirCall call
                && TryInlineCall(call, tir, writer, ownerTypeName, lambdaDepth > 0, callerNames, out var inlined)
                    ? Bump(count, inlined)
                    : n,
                enter: n => { if (n is TirLambda l) { lambdaDepth++; if (IsEtaShaped(l)) etaDepth++; } },
                exit: n => { if (n is TirLambda l) { lambdaDepth--; if (IsEtaShaped(l)) etaDepth--; } });

            if (count[0] == 0)
                break;
            inlinedCalls += count[0];
        }

        if (writer.InlineReport != null)
            writer.InlineReport.CallsInlined += inlinedCalls;

        return inlinedCalls == 0
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static TirNode Bump(int[] count, TirNode n)
    {
        count[0]++;
        return n;
    }

    private static bool TryInlineCall(TirCall call, TirFunction callerTir, CSharpWriter writer,
        string ownerTypeName, bool insideLambda, HashSet<string> callerNames, out TirNode inlined)
    {
        inlined = null;

        var report = writer.InlineReport;
        var callerName = callerTir?.Original?.Name;
        var calleeName = call.Callee?.Name;
        bool Refuse(InlineRefusal r) { report?.Refuse(callerName, calleeName, r); return false; }

        // A resolved member call with an explicit receiver; operators/conversions/properties are
        // all fine (they are ordinary functions in Plato), but the callee must be known.
        if (call.Callee?.Body == null)
            return Refuse(InlineRefusal.NoCalleeBody);

        // Nullary call (emitted as Constants.X): inline the static TIR body under the same
        // self-containment rules — there are no parameters to substitute.
        if (call.Args.Count == 0)
        {
            if (TryInlineNullary(call, writer, insideLambda, out inlined))
                return true;
            return Refuse(InlineRefusal.NullaryRefused);
        }

        // The callee's body, specialized for the receiver's solved concrete type. The receiver
        // type comes from TRUSTED sources (the caller's zonked signature for parameters, the
        // literal type for literals) before the node's own type: loosely-solved node types
        // (Self-unifies-anything) can name a different specialization than the emitted surface.
        var receiverTypeName = TrustedTypeName(callerTir, call.Args[0], writer, ownerTypeName);
        var calleeTir = writer.TryGetGroundTirByTypeName(call.Callee, receiverTypeName);
        var calleeBody = calleeTir?.Body;
        if (calleeBody == null)
            return Refuse(InlineRefusal.NoGroundTir);

        // Expression bodies only, within budget, and self-contained under a foreign type.
        if (TirRewrite.IsStatementNode(calleeBody))
            return Refuse(InlineRefusal.StatementBody);

        // The body's value type must BE the declared return type: a mismatch means the function
        // relies on a return-position implicit conversion (Number -> Angle in `Turns`,
        // tuple -> struct), which evaporates when the body moves to the call site — EXCEPT a
        // tuple-returning body whose declared return type is a struct with a matching-arity field
        // constructor, which is rewritten to an explicit `new T(elem...)` (TirConstructorCall).
        // That rewrite is POSITION-INDEPENDENT and ERASURE-INDEPENDENT: `new Triangle3D(a,b,c)` is
        // valid in any expression slot under any scalar setting, so it replaces the old
        // tail-position + wrapper-tuple tail-lift (M1). The arity gate excludes shapes like
        // Translation3D (1-arg Vector3 ctor vs a 3-element tuple), which stay uninlined.
        var bodyTypeName = calleeBody.Type?.Name;
        var returnTypeName = (calleeTir.ZonkedReturnType ?? calleeTir.ReturnType)?.Name;
        var tupleCtorRewrite = bodyTypeName != null && bodyTypeName.StartsWith("Tuple")
            && returnTypeName != null && TupleCtorArityMatches(writer, calleeBody, returnTypeName);
        if (bodyTypeName == null || returnTypeName == null
            || (bodyTypeName != returnTypeName && !tupleCtorRewrite))
            return Refuse(InlineRefusal.ReturnTypeMismatch);
        if (!IsSelfContained(writer, calleeBody, tupleCtorRewrite, out var calleeHasLambda))
            return Refuse(InlineRefusal.NotSelfContained);

        // Under a lambda at the CALL SITE: only lambda-free callee bodies with all-cheap
        // arguments (see Inline).
        // Under a lambda, args must be cheap — the cost-based cheap-projection relaxation (the one
        // recipe is --no-properties since C4, so this is unconditional).
        bool CheapArg(TirNode a) => IsCheapProjection(writer, a);
        if (insideLambda && (calleeHasLambda || !call.Args.All(CheapArg)))
            return Refuse(InlineRefusal.InsideLambda);

        // Argument discipline: a CHEAP leaf (receiver/parameter/variable/literal/type ref)
        // substitutes anywhere; a compound argument substitutes only when its parameter is
        // used at most once, outside any lambda in the callee body (single evaluation, no
        // capture). Lambda literals never substitute (delegate target-typing).
        var paramDefs = calleeTir.Parameters;
        if (paramDefs == null || paramDefs.Count != call.Args.Count)
            return Refuse(InlineRefusal.ArityMismatch);
        Dictionary<ParameterDef, (int Count, bool UnderLambda)> uses = null;
        for (var i = 0; i < call.Args.Count; i++)
        {
            var arg = call.Args[i];

            // Substitution-boundary type check: a KNOWN caller-side argument type must equal
            // the callee's resolved parameter type (implicit conversions the elaborator saw are
            // already TirCoerce-wrapped and carry the target type; a silent mismatch means the
            // solver unified loosely and the fetched specialization is not the emitted one).
            var paramTypeName = ResolvedParamTypeName(writer, calleeTir, paramDefs, i, receiverTypeName);
            var argTypeName = TrustedTypeName(callerTir, arg, writer, ownerTypeName);
            if (paramTypeName != null && argTypeName != null && paramTypeName != argTypeName)
                return Refuse(InlineRefusal.ArgTypeMismatch);

            if (TirRewrite.IsCheap(arg))
                continue;
            uses = uses ?? TirRewrite.CountParamUses(calleeBody);
            uses.TryGetValue(paramDefs[i], out var u);

            if (TirRewrite.StripCoerce(arg) is TirLambda lamArg)
            {
                // Delegate-parameter inlining (substituting a lambda into a Function-typed
                // parameter, then β-reducing) relies on the uniform method-form surface
                // (--no-properties, the one recipe since C4).
                // A lambda substitutes into a delegate-typed parameter only when EVERY use is a
                // CONSUMING position: the target of an application (which then β-reduces away — no
                // residual lambda) or a direct call argument (target-typed by the callee's Func
                // parameter). Never under a nested lambda (the per-lambda capture hoist), and the
                // total substituted lambda-body size is bounded to cap code growth on multi-use
                // parameters (the f(A)/f(B)/f(C) tuple bodies apply f several times).
                if (u.UnderLambda || !AllUsesConsuming(calleeBody, paramDefs[i]))
                    return Refuse(InlineRefusal.LambdaArgRefused);
                // A delegate application whose RESULT is navigated as a member-call receiver
                // (f(x).Foo()) loses the delegate-return coercion when β-reduced: the lambda body
                // type (e.g. Vector3) can differ from the Func's declared return (Point3D), and the
                // receiver position does not re-apply the conversion, so .Foo() would dispatch on
                // the wrong type. Refuse — the result stays uninlined (Ray3D.Direction re-normalize
                // is the case). Converting positions (tuple/ctor args) are fine and stay inlinable.
                if (InvokeResultNavigated(calleeBody, paramDefs[i]))
                    return Refuse(InlineRefusal.LambdaArgRefused);
                if (u.Count * lamArg.Descendants().Count() > MaxBodyNodes)
                    return Refuse(InlineRefusal.LambdaArgRefused);
                continue;
            }

            // A compound (non-lambda) argument substitutes only when its parameter is used at
            // most once, outside any lambda (single evaluation, no capture) — EXCEPT a cheap
            // projection (field/component read, scalar arithmetic), which may be duplicated across
            // multiple uses because re-running it costs only a few machine ops. The under-lambda
            // refusal stands regardless: substituting into a captured position trips the emitter's
            // per-lambda capture hoist (an emission constraint, not an evaluation-count one).
            if ((u.Count > 1 && !IsCheapProjection(writer, arg)) || u.UnderLambda)
                return Refuse(InlineRefusal.MultiUseCompound);
        }

        // Substitute parameters by arguments, and rebind `Self` to the receiver's concrete type
        // (the body is leaving its home struct, where `Self` renders as the enclosing type).
        // Lambda parameters that would shadow a caller-bound name (CS0136 at emission) are
        // alpha-renamed to a fresh clone of their ParameterDef.
        var selfDef = writer.Compilation.GetTypeDef(receiverTypeName);
        var byDef = new Dictionary<ParameterDef, TirNode>();
        for (var i = 0; i < paramDefs.Count; i++)
        {
            var arg = call.Args[i];
            // Tag the substituted argument with the callee's zonked SCALAR parameter type via a
            // transparent TirCoerce: the callee body's origins point at generic parameters, so
            // the emitter's scalar analysis cannot see that this position is (say) Number — the
            // tag carries it (TirCSharpBodyWriter.AuthoritativePrim reads it back).
            var resolvedName = ResolvedParamTypeName(writer, calleeTir, paramDefs, i, receiverTypeName);
            if (resolvedName != null && CSharpWriter.ScalarPrimitives.ContainsKey(resolvedName)
                && !(arg is TirCoerce))
            {
                var scalarDef = writer.Compilation.GetTypeDef(resolvedName);
                if (scalarDef != null)
                    arg = new TirCoerce(arg, arg.Type, scalarDef.ToTypeExpression(), null, arg.Origin);
            }
            byDef[paramDefs[i]] = arg;
        }

        inlined = TirRewrite.Rewrite(calleeBody, n =>
        {
            // Post-order: a substituted delegate parameter has already turned `f(x)` into
            // `(λ….e)(x)`; β-reduce it away so no lambda is left in applied position. The
            // lambda body is the caller's own (no callee params / Self inside), so the reduced
            // node needs no further callee-side rewriting.
            if (n is TirInvoke invk
                && TirRewrite.TryBetaReduce(invk, out var reduced, x => IsCheapProjection(writer, x)))
            {
                if (report != null) report.BetaReductions++;
                return reduced;
            }
            if (n is TirParameter p && p.Def != null && byDef.TryGetValue(p.Def, out var arg))
                return arg;
            if (n is TirTypeRef t && t.TypeDef?.Name == "Self" && selfDef != null)
                return new TirTypeRef(selfDef, t.Type, t.Origin);
            return n;
        });

        // Tuple-returning body: replace the ROOT Tuple_N(...) with an explicit constructor call for
        // the struct return type, so the tuple->struct conversion no longer depends on position or
        // wrapper types (M1). Substitution preserves the root node kind, so the root is still the
        // tuple call here.
        if (tupleCtorRewrite && inlined is TirCall rootTuple
            && rootTuple.Name != null && rootTuple.Name.StartsWith("Tuple"))
        {
            inlined = new TirConstructorCall(returnTypeName, rootTuple.Args);
            if (report != null) report.TupleConstructorRewrites++;
        }

        // Alpha-rename any lambda parameter in the FINAL inlined tree that collides with a
        // caller-bound name (CS0136 once the body lands in the caller's scope). Computed over the
        // rewritten result — not just the callee body — so it also covers a substituted
        // caller-lambda argument and any lambda a β-reduction left exposed.
        inlined = AlphaRenameCollidingLambdas(inlined, callerNames);

        // Refuse if a substituted lambda argument was left in APPLIED position without β-reducing
        // (an immediately-invoked lambda). That happens when the argument to the application is not
        // cheap and the parameter is used more than once (e.g. a struct-conversion body expanded
        // the parameter), so reducing would re-evaluate it. The emitter does not render such an
        // IIFE, and duplicating would break the evaluation-count discipline — so leave the call
        // uninlined rather than emit invalid C#. (The immediate-apply tuple bodies — Triangle/Quad/
        // Line/Ray — stay here; the mesh family, whose delegate is a Map combinator argument,
        // β-reduces cleanly and collapses.)
        if (HasResidualLambdaInvoke(inlined))
            return Refuse(InlineRefusal.LambdaArgRefused);
        return true;
    }

    // Cost threshold below which duplicating an expression during substitution is considered
    // acceptable (a handful of field loads / scalar ops). A sentinel for "do not duplicate".
    private const int MaxCheapCost = 4;
    private const int ExpensiveCost = 1000;

    // Intrinsic operators cheap enough to re-run: scalar/vector arithmetic, comparison and logic
    // map to a few machine ops with no allocation or hidden call. Everything else with no visible,
    // trivially-cheap body (Magnitude's sqrt, Normalize, trig, ...) is assumed expensive.
    private static readonly HashSet<string> CheapOperators = new HashSet<string>
    {
        "Add", "Subtract", "Multiply", "Divide", "Modulo", "Negate",
        "GreaterThan", "LessThan", "GreaterThanOrEquals", "LessThanOrEquals",
        "Equals", "NotEquals", "And", "Or", "Not", "Min", "Max",
    };

    /// <summary>Whether re-running <paramref name="n"/> is cheap enough to DUPLICATE it during
    /// β-reduction / substitution (its <see cref="ProjectionCost"/> is under
    /// <see cref="MaxCheapCost"/>). Replaces the earlier field-name-only rule with a bounded cost
    /// estimate — see <see cref="ProjectionCost"/>.</summary>
    private static bool IsCheapProjection(CSharpWriter writer, TirNode n)
        => ProjectionCost(writer, n) < MaxCheapCost;

    /// <summary>A rough evaluation-cost estimate over the TIR, for the duplicate-or-not decision.
    /// Cheap: leaves (0), a <see cref="TirComponentAccess"/> or a stored-FIELD read (the receiver's
    /// cost + 1), and a whitelisted scalar/vector OPERATOR over cheap operands (operands + 1).
    /// Expensive (<see cref="ExpensiveCost"/>, short-circuiting): allocations
    /// (<see cref="TirNew"/>/<see cref="TirArray"/>/<see cref="TirConstructorCall"/>), control flow
    /// (<see cref="TirConditional"/>/loops/<see cref="TirInvoke"/>/<see cref="TirLambda"/>), and any
    /// member call that is neither a field read nor a cheap operator — a computed property or a
    /// handwritten intrinsic whose body we cannot see is conservatively assumed to hide real
    /// work.</summary>
    private static int ProjectionCost(CSharpWriter writer, TirNode n)
    {
        n = TirRewrite.StripCoerce(n);
        if (n == null)
            return 0;
        if (n is TirParameter || n is TirVariable || n is TirLiteral || n is TirTypeRef || n is TirDefault)
            return 0;
        if (n is TirComponentAccess ca)
            return ProjectionCost(writer, ca.Receiver);
        if (n is TirCall c && c.Name != null && c.Args.Count >= 1)
        {
            var recv = TirRewrite.StripCoerce(c.Args[0]);
            var typeName = recv?.Type?.Name;
            var typeDef = typeName == null ? null : writer.Compilation.GetTypeDef(typeName);
            // A pure stored-field read.
            if (typeDef != null && typeDef.Fields.Any(f => f.Name == c.Name))
                return Cap(ProjectionCost(writer, recv) + 1);
            // A whitelisted operator over cheap operands.
            if (CheapOperators.Contains(c.Name))
            {
                var sum = 1;
                foreach (var a in c.Args)
                {
                    sum += ProjectionCost(writer, a);
                    if (sum >= ExpensiveCost) return ExpensiveCost;
                }
                return Cap(sum);
            }
        }
        // Allocations, control flow, computed members, opaque intrinsics: not cheap to duplicate.
        return ExpensiveCost;
    }

    private static int Cap(int c) => c >= ExpensiveCost ? ExpensiveCost : c;

    /// <summary>Whether any <see cref="TirInvoke"/> in the tree still applies a lambda literal
    /// directly (β-reduction did not fire) — an IIFE the emitter cannot render.</summary>
    private static bool HasResidualLambdaInvoke(TirNode node)
        => node != null && node.Descendants().OfType<TirInvoke>()
            .Any(inv => TirRewrite.StripCoerce(inv.Target) is TirLambda);

    /// <summary>Rename every lambda parameter in <paramref name="node"/> whose name collides with a
    /// caller-bound name to a fresh clone (<c>{name}_{N}</c>), updating both the lambda's parameter
    /// list and the parameter references in its body.</summary>
    private static TirNode AlphaRenameCollidingLambdas(TirNode node, HashSet<string> callerNames)
    {
        var renames = BuildLambdaParamRenames(node, callerNames);
        if (renames == null)
            return node;
        return TirRewrite.Rewrite(node, n =>
        {
            if (n is TirParameter p && p.Def != null && renames.TryGetValue(p.Def, out var renamed))
                return new TirParameter(renamed, p.Type, p.Origin);
            if (n is TirLambda lam && lam.Parameters.Any(d => renames.ContainsKey(d)))
                return new TirLambda(
                    lam.Parameters.Select(d => renames.TryGetValue(d, out var r) ? r : d).ToList(),
                    lam.Body, lam.Type, lam.Origin);
            return n;
        });
    }

    /// <summary>Whether every reference to <paramref name="pd"/> in <paramref name="body"/> sits in
    /// a position that CONSUMES it as a function value without leaving a bare lambda in a
    /// non-target-typed slot: the target of a <see cref="TirInvoke"/> (β-reduced away on
    /// substitution) or a direct argument of a <see cref="TirCall"/> (target-typed by the callee's
    /// <c>Function{N}</c> parameter). Any other occurrence (a tuple element, a <c>new</c> field, a
    /// returned/aliased value) would lose the delegate's target-typing once a lambda is substituted,
    /// so the substitution is refused.</summary>
    private static bool AllUsesConsuming(TirNode body, ParameterDef pd)
    {
        var total = 0;
        var consuming = 0;
        bool IsRef(TirNode n) => TirRewrite.StripCoerce(n) is TirParameter p && p.Def == pd;
        void Walk(TirNode n)
        {
            if (n == null)
                return;
            if (n is TirParameter tp && tp.Def == pd)
                total++;
            if (n is TirInvoke inv && IsRef(inv.Target))
                consuming++;
            if (n is TirCall c)
                foreach (var a in c.Args)
                    if (IsRef(a))
                        consuming++;
            foreach (var ch in n.Children)
                Walk(ch);
        }
        Walk(body);
        return total > 0 && total == consuming;
    }

    /// <summary>Whether any member call in <paramref name="body"/> uses an APPLICATION of the
    /// delegate parameter <paramref name="pd"/> as its receiver (arg 0), i.e. <c>f(x).Foo(...)</c>.
    /// Such a navigated result loses the delegate-return coercion under β-reduction (the lambda
    /// body type may differ from the Func's declared return, and the receiver slot does not convert),
    /// so the substitution is unsound to emit.</summary>
    private static bool InvokeResultNavigated(TirNode body, ParameterDef pd)
    {
        bool IsInvokeOfPd(TirNode n)
            => TirRewrite.StripCoerce(n) is TirInvoke inv
               && TirRewrite.StripCoerce(inv.Target) is TirParameter p && p.Def == pd;
        return body.Descendants().OfType<TirCall>()
            .Any(c => c.Args.Count >= 1 && IsInvokeOfPd(c.Args[0]));
    }

    /// <summary>Every name bound in the caller: function parameters, lambda parameters and let
    /// locals anywhere in the body. An inlined lambda parameter shadowing any of these is a
    /// compile error (CS0136) once the body lands in the caller's scope.</summary>
    private static HashSet<string> CollectBoundNames(TirFunction tir, TirNode body)
    {
        var names = new HashSet<string>();
        if (tir.Parameters != null)
            foreach (var p in tir.Parameters)
                if (p?.Name != null)
                    names.Add(p.Name);
        void Walk(TirNode n)
        {
            if (n == null)
                return;
            if (n is TirLambda lam)
                foreach (var p in lam.Parameters)
                    if (p?.Name != null)
                        names.Add(p.Name);
            if (n is TirLet let && let.Def?.Name != null)
                names.Add(let.Def.Name);
            foreach (var c in n.Children)
                Walk(c);
        }
        Walk(body);
        return names;
    }

    /// <summary>Fresh ParameterDef clones for every lambda parameter of a callee body whose name
    /// collides with a caller-bound name (`{name}_{N}`, deterministic counter).</summary>
    private static Dictionary<ParameterDef, ParameterDef> BuildLambdaParamRenames(
        TirNode calleeBody, HashSet<string> callerNames)
    {
        Dictionary<ParameterDef, ParameterDef> renames = null;
        foreach (var n in calleeBody.Descendants())
            if (n is TirLambda lam)
                foreach (var p in lam.Parameters)
                    if (p?.Name != null && callerNames.Contains(p.Name))
                    {
                        renames = renames ?? new Dictionary<ParameterDef, ParameterDef>();
                        if (!renames.ContainsKey(p))
                            renames[p] = new ParameterDef(p.Scope, $"{p.Name}_{NextRenameId++}", p.Type, p.Index);
                    }
        return renames;
    }

    /// <summary>Budget + self-containment scan of a callee body: no bare-name lookups
    /// (<see cref="TirName"/>, eta-forwarding lambdas), no unresolved/let/assign nodes, no tuple
    /// literals (their target-typed conversion belongs to the position they sit in), and no
    /// SYNTACTIC calls (null callee: only trustworthy in the body's HOME emission context — e.g.
    /// the library Dot's body for Vector3 references a Sum overload that does not exist there;
    /// the handwritten intrinsic Dot is what actually ships for that type).</summary>
    private static bool IsSelfContained(CSharpWriter writer, TirNode calleeBody, bool allowRootTuple, out bool hasLambda)
    {
        hasLambda = false;
        var nodes = calleeBody.Descendants().ToList();
        if (nodes.Count > MaxBodyNodes)
            return false;
        foreach (var n in nodes)
        {
            if (n is TirName || n is TirUnresolved || n is TirLet || n is TirAssign)
                return false;
            // Every member call inside the body must be EMITTABLE on its receiver's type: the
            // monomorphizer holds ground specializations of raw generic bodies whose calls do
            // not exist in the emitted surface (e.g. Between@Point2D references
            // GreaterThanOrEquals(Point2D) — the SHIPPED Point2D.Between is the component
            // fanout, a different instance). Emission is name + shape, so an inlined
            // non-existent member is a compile error in the caller's file.
            if (n is TirCall mc && !IsCallableOnReceiver(writer, mc))
                return false;
            if (n is TirLambda lam)
            {
                if (IsEtaShaped(lam))
                    return false;
                // The emitter builds a per-lambda scalar-erase analysis from the lambda's ORIGIN
                // symbol; a generic origin (type-variable first parameter) only renders safely in
                // its home emission context.
                if (lam.Origin is Lambda l && l.Function.NumParameters > 0
                    && l.Function.Parameters[0].Type.IsTypeVariable)
                    return false;
                hasLambda = true;
            }
            // A syntactic tuple's target-typed conversion belongs to the position it sits in, so a
            // nested tuple is refused — but the ROOT tuple of a tail-position body converts at the
            // caller's return slot (see the tuple tail-lift in TryInlineCall), so it is permitted.
            if (n is TirCall tc)
            {
                var isRootTuple = allowRootTuple && ReferenceEquals(n, calleeBody)
                    && tc.Name != null && tc.Name.StartsWith("Tuple");
                if (!isRootTuple && (tc.Callee == null || (tc.Name != null && tc.Name.StartsWith("Tuple"))))
                    return false;
            }
        }
        return true;
    }

    // Scaffolding members every generated concrete type has, which the extension plans do not
    // enumerate (they are skipped from the candidate sets).
    private static readonly HashSet<string> ScaffoldingNames = new HashSet<string>
    {
        "Create", "CreateFromComponents", "CreateFromComponent", "Default",
        "Components", "NumComponents", "Count", "At",
        "Equals", "NotEquals", "ToString", "GetHashCode",
        "FieldNames", "FieldValues", "TypeName", "Deconstruct",
        "Range", "MakeArray2D", "MapRange",
    };

    /// <summary>Whether a member call renders to something that exists on its receiver's type:
    /// the plan's member-name universe, scaffolding, With* wither functions, or an unknown
    /// receiver type (interfaces, IReadOnlyList — trusted, they carry their own members).</summary>
    private static bool IsCallableOnReceiver(CSharpWriter writer, TirCall c)
    {
        if (c.Callee == null || c.Args.Count == 0 || c.Name == null
            || c.Name.StartsWith("Tuple") || c.Name == "At")
            return true;
        var t = TirRewrite.StripCoerce(c.Args[0])?.Type?.Name;
        var plan = t == null ? null : writer.GetExtensionPlanByTypeName(t);
        if (plan == null)
            return true;
        return plan.InstanceNames.Contains(c.Name) || plan.StaticNames.Contains(c.Name)
            || ScaffoldingNames.Contains(c.Name) || c.Name.StartsWith("With");
    }

    /// <summary>The most trustworthy Plato type name for a node: an explicit coercion's target,
    /// the caller's zonked signature type for the caller's own parameters, the literal type for
    /// literals, then the node's zonked type (which loose solving can render misleading).
    ///
    /// The receiver parameter (index 0) is special-cased: a method emitted as a member of the
    /// concrete <paramref name="ownerTypeName"/> binds its receiver to that concrete type even when
    /// the signature declares an INTERFACE (<c>Transform(self: IDeformable3D)</c> emitted on
    /// Bounds3D has <c>self : Bounds3D</c>). Monomorphization specializes only <c>Self</c>, so the
    /// zonked receiver type can still be the interface; the owner type is the emitted truth and is
    /// what the concrete callee (<c>Deform@Bounds3D</c>) is keyed under.</summary>
    private static string TrustedTypeName(TirFunction callerTir, TirNode n, CSharpWriter writer, string ownerTypeName)
    {
        if (n is TirCoerce co && co.ToType?.Name != null)
            return co.ToType.Name;
        n = TirRewrite.StripCoerce(n);
        if (n is TirParameter p && p.Def != null && callerTir?.Parameters != null)
            for (var i = 0; i < callerTir.Parameters.Count; i++)
                if (ReferenceEquals(callerTir.Parameters[i], p.Def))
                {
                    var zonked = callerTir.ZonkedParameterTypes != null && i < callerTir.ZonkedParameterTypes.Count
                        ? callerTir.ZonkedParameterTypes[i]?.Name
                        : null;
                    // The receiver (param 0) needs a CONCRETE, emittable type to key the ground
                    // callee. When the caller's zonked signature kept an INTERFACE — Transform is
                    // written `Transform(self: IDeformable3D): Self` and monomorphized only in its
                    // Self return, so `self` stays IDeformable3D in the signature though the emitted
                    // member binds it to the concrete owner — the node's OWN solved type is the
                    // monomorphized truth (self.Type = Bounds3D). Prefer, in order: a concrete
                    // zonked signature type, the node's concrete solved type, then the owner type.
                    // A genuine type variable stays unresolved (none is concrete) and falls through
                    // to the zonked name unchanged, so no specialization is mis-selected elsewhere.
                    if (i == 0)
                    {
                        if (zonked != null && IsConcreteName(writer, zonked))
                            return zonked;
                        var nodeType = n.Type?.Name;
                        if (nodeType != null && IsConcreteName(writer, nodeType))
                            return nodeType;
                        if (ownerTypeName != null && IsConcreteName(writer, ownerTypeName))
                            return ownerTypeName;
                    }
                    return zonked ?? p.Def.Type?.Name ?? n.Type?.Name;
                }
        if (n is TirLiteral lit)
            return lit.LiteralType.ToString();
        return n?.Type?.Name;
    }

    private static string ZonkedParamTypeName(TirFunction calleeTir, int i)
        => calleeTir.ZonkedParameterTypes != null && i < calleeTir.ZonkedParameterTypes.Count
            ? calleeTir.ZonkedParameterTypes[i]?.Name
            : null;

    /// <summary>The callee's parameter type at the fetched specialization, from the most to the
    /// least specific source: zonked TIR type, declared type when concrete, and (for the
    /// receiver) the type name the specialization was looked up under.</summary>
    private static string ResolvedParamTypeName(CSharpWriter writer, TirFunction calleeTir,
        IReadOnlyList<ParameterDef> paramDefs, int i, string receiverTypeName)
    {
        var zonked = ZonkedParamTypeName(calleeTir, i);
        if (zonked != null && IsConcreteName(writer, zonked))
            return zonked;
        var declared = paramDefs[i]?.Type?.Name;
        if (declared != null && IsConcreteName(writer, declared))
            return declared;
        return i == 0 ? receiverTypeName : null;
    }

    private static bool IsConcreteName(CSharpWriter writer, string name)
        => CSharpWriter.ScalarPrimitives.ContainsKey(name) || writer.GetExtensionPlanByTypeName(name) != null;

    /// <summary>Whether the callee body's ROOT is a tuple literal whose arity equals the field
    /// count of the struct <paramref name="returnTypeName"/> — i.e. the struct has a
    /// field-wise constructor accepting the tuple elements in order, so the tuple can be rewritten
    /// to <c>new T(elem...)</c> (M1). A concrete, generated struct only; excludes scalar
    /// primitives and any type whose field arity differs from the tuple's.</summary>
    private static bool TupleCtorArityMatches(CSharpWriter writer, TirNode calleeBody, string returnTypeName)
    {
        if (!(calleeBody is TirCall tuple) || tuple.Name == null || !tuple.Name.StartsWith("Tuple"))
            return false;
        if (returnTypeName.StartsWith("Tuple"))
            return false; // a genuinely tuple-typed return, not a struct with a field ctor
        var typeDef = writer.Compilation.GetTypeDef(returnTypeName);
        return typeDef != null && !typeDef.IsInterface()
            && typeDef.Fields.Count == tuple.Args.Count;
    }

    /// <summary>A nullary call (emitted as <c>Constants.X</c>): inline the static TIR body when
    /// it is a self-contained ground expression within budget whose value type matches the
    /// declared return type. No parameters, no Self to rebind.</summary>
    private static bool TryInlineNullary(TirCall call, CSharpWriter writer, bool insideLambda, out TirNode inlined)
    {
        inlined = null;
        var calleeTir = writer.TryGetStaticTir(call.Callee);
        var body = calleeTir?.Body;
        if (body == null || (calleeTir.Parameters?.Count ?? 0) != 0)
            return false;
        if (TirRewrite.IsStatementNode(body))
            return false;
        var bodyTypeName = body.Type?.Name;
        var returnTypeName = (calleeTir.ZonkedReturnType ?? calleeTir.ReturnType)?.Name;
        if (bodyTypeName == null || returnTypeName == null || bodyTypeName != returnTypeName)
            return false;
        if (!IsSelfContained(writer, body, false, out var hasLambda) || (insideLambda && hasLambda))
            return false;
        inlined = body;
        return true;
    }


    // Same shape TirCSharpBodyWriter recovers as a bare member-group name (Normalizer R3).
    private static bool IsEtaShaped(TirLambda lam)
        => lam?.Parameters != null && lam.Parameters.Count > 0
           && lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta"))
           && lam.Body is TirCall c && c.Args.Count == lam.Parameters.Count;
}
