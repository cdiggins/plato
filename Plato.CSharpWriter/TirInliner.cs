using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;

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

    public static TirFunction Inline(TirFunction tir, CSharpWriter writer, out int inlinedCalls)
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

            var count = 0;
            var lambdaDepth = 0;
            var etaDepth = 0;
            // Call sites INSIDE lambdas inline restrictively (lambda-free callee, cheap args):
            // an inlined body that itself contains a lambda capturing substituted expressions
            // trips the per-lambda capture hoist into declaring locals out of scope. Eta-shaped
            // lambdas are OPAQUE: rewriting their bodies would destroy the exact forwarding
            // shape the emitter recovers as a bare member name.
            body = TirRewrite.Rewrite(body, n =>
                etaDepth == 0 && n is TirCall call
                && TryInlineCall(call, tir, writer, lambdaDepth > 0, callerNames, out var inlined)
                    ? Bump(ref count, inlined)
                    : n,
                enter: n => { if (n is TirLambda l) { lambdaDepth++; if (IsEtaShaped(l)) etaDepth++; } },
                exit: n => { if (n is TirLambda l) { lambdaDepth--; if (IsEtaShaped(l)) etaDepth--; } });
            if (count == 0)
                break;
            inlinedCalls += count;
        }

        return inlinedCalls == 0
            ? tir
            : new TirFunction(tir.Original, tir.Parameters, tir.ReturnType, body,
                tir.ZonkedParameterTypes, tir.ZonkedReturnType);
    }

    private static TirNode Bump(ref int count, TirNode n)
    {
        count++;
        return n;
    }

    private static bool TryInlineCall(TirCall call, TirFunction callerTir, CSharpWriter writer,
        bool insideLambda, HashSet<string> callerNames, out TirNode inlined)
    {
        inlined = null;

        // A resolved member call with an explicit receiver; operators/conversions/properties are
        // all fine (they are ordinary functions in Plato), but the callee must be known.
        if (call.Callee?.Body == null)
            return false;

        // Nullary call (emitted as Constants.X): inline the static TIR body under the same
        // self-containment rules — there are no parameters to substitute.
        if (call.Args.Count == 0)
            return TryInlineNullary(call, writer, insideLambda, out inlined);

        // The callee's body, specialized for the receiver's solved concrete type. The receiver
        // type comes from TRUSTED sources (the caller's zonked signature for parameters, the
        // literal type for literals) before the node's own type: loosely-solved node types
        // (Self-unifies-anything) can name a different specialization than the emitted surface.
        var receiverTypeName = TrustedTypeName(callerTir, call.Args[0]);
        var calleeTir = writer.TryGetGroundTirByTypeName(call.Callee, receiverTypeName);
        var calleeBody = calleeTir?.Body;
        if (calleeBody == null)
            return false;

        // Expression bodies only, within budget, and self-contained under a foreign type.
        if (IsStatementNode(calleeBody))
            return false;

        // The body's value type must BE the declared return type: a mismatch means the function
        // relies on a return-position implicit conversion (Number -> Angle in `Turns`,
        // tuple -> struct), which evaporates when the body moves to the call site.
        var bodyTypeName = calleeBody.Type?.Name;
        var returnTypeName = (calleeTir.ZonkedReturnType ?? calleeTir.ReturnType)?.Name;
        if (bodyTypeName == null || returnTypeName == null || bodyTypeName != returnTypeName)
            return false;
        if (!IsSelfContained(writer, calleeBody, out var calleeHasLambda))
            return false;

        // Under a lambda at the CALL SITE: only lambda-free callee bodies with all-cheap
        // arguments (see Inline).
        if (insideLambda && (calleeHasLambda || !call.Args.All(TirRewrite.IsCheap)))
            return false;

        // Argument discipline: a CHEAP leaf (receiver/parameter/variable/literal/type ref)
        // substitutes anywhere; a compound argument substitutes only when its parameter is
        // used at most once, outside any lambda in the callee body (single evaluation, no
        // capture). Lambda literals never substitute (delegate target-typing).
        var paramDefs = calleeTir.Parameters;
        if (paramDefs == null || paramDefs.Count != call.Args.Count)
            return false;
        Dictionary<ParameterDef, (int Count, bool UnderLambda)> uses = null;
        for (var i = 0; i < call.Args.Count; i++)
        {
            var arg = call.Args[i];

            // Substitution-boundary type check: a KNOWN caller-side argument type must equal
            // the callee's resolved parameter type (implicit conversions the elaborator saw are
            // already TirCoerce-wrapped and carry the target type; a silent mismatch means the
            // solver unified loosely and the fetched specialization is not the emitted one).
            var paramTypeName = ResolvedParamTypeName(writer, calleeTir, paramDefs, i, receiverTypeName);
            var argTypeName = TrustedTypeName(callerTir, arg);
            if (paramTypeName != null && argTypeName != null && paramTypeName != argTypeName)
                return false;

            if (TirRewrite.IsCheap(arg))
                continue;
            if (TirRewrite.StripCoerce(arg) is TirLambda)
                return false;
            uses = uses ?? TirRewrite.CountParamUses(calleeBody);
            if (uses.TryGetValue(paramDefs[i], out var u) && (u.Count > 1 || u.UnderLambda))
                return false;
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

        var renames = calleeHasLambda ? BuildLambdaParamRenames(calleeBody, callerNames) : null;

        inlined = TirRewrite.Rewrite(calleeBody, n =>
        {
            if (n is TirParameter p && p.Def != null)
            {
                if (byDef.TryGetValue(p.Def, out var arg))
                    return arg;
                if (renames != null && renames.TryGetValue(p.Def, out var renamed))
                    return new TirParameter(renamed, p.Type, p.Origin);
            }
            if (n is TirLambda lam && renames != null
                && lam.Parameters.Any(d => renames.ContainsKey(d)))
                return new TirLambda(
                    lam.Parameters.Select(d => renames.TryGetValue(d, out var r) ? r : d).ToList(),
                    lam.Body, lam.Type, lam.Origin);
            if (n is TirTypeRef t && t.TypeDef?.Name == "Self" && selfDef != null)
                return new TirTypeRef(selfDef, t.Type, t.Origin);
            return n;
        });
        return true;
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
    private static bool IsSelfContained(CSharpWriter writer, TirNode calleeBody, out bool hasLambda)
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
            if (n is TirCall tc && (tc.Callee == null || (tc.Name != null && tc.Name.StartsWith("Tuple"))))
                return false;
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
    /// literals, then the node's zonked type (which loose solving can render misleading).</summary>
    private static string TrustedTypeName(TirFunction callerTir, TirNode n)
    {
        if (n is TirCoerce co && co.ToType?.Name != null)
            return co.ToType.Name;
        n = TirRewrite.StripCoerce(n);
        if (n is TirParameter p && p.Def != null && callerTir?.Parameters != null)
            for (var i = 0; i < callerTir.Parameters.Count; i++)
                if (ReferenceEquals(callerTir.Parameters[i], p.Def))
                    return (callerTir.ZonkedParameterTypes != null && i < callerTir.ZonkedParameterTypes.Count
                               ? callerTir.ZonkedParameterTypes[i]?.Name
                               : null)
                           ?? p.Def.Type?.Name ?? n.Type?.Name;
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
        if (IsStatementNode(body))
            return false;
        var bodyTypeName = body.Type?.Name;
        var returnTypeName = (calleeTir.ZonkedReturnType ?? calleeTir.ReturnType)?.Name;
        if (bodyTypeName == null || returnTypeName == null || bodyTypeName != returnTypeName)
            return false;
        if (!IsSelfContained(writer, body, out var hasLambda) || (insideLambda && hasLambda))
            return false;
        inlined = body;
        return true;
    }

    private static bool IsStatementNode(TirNode n)
        => n is TirBlock || n is TirReturn || n is TirIf || n is TirLoop;

    // Same shape TirCSharpBodyWriter recovers as a bare member-group name (Normalizer R3).
    private static bool IsEtaShaped(TirLambda lam)
        => lam?.Parameters != null && lam.Parameters.Count > 0
           && lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta"))
           && lam.Body is TirCall c && c.Args.Count == lam.Parameters.Count;
}
