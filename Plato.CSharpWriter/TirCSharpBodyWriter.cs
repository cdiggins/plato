using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The SOLE C# function-body writer (the legacy symbol-graph <c>CSharpFunctionBodyWriter</c> was
/// retired at consolidation-plan C4). Renders a C# function <em>body</em> from a monomorphized
/// <see cref="TirFunction"/> (the Typed IR produced by <see cref="Elaborator"/> +
/// <see cref="Monomorphizer"/>, then the writer-side optimizer passes): it maps the recorded
/// <see cref="EmissionKind"/> of every <see cref="TirCall"/> directly to syntax and renders every
/// conversion from an explicit <see cref="TirCoerce"/> node — no emit-time semantic re-derivation.
///
/// Under scalar erasure (<c>--scalar</c>) the body arrives already LOWERED by
/// <see cref="TirScalarLowerer"/> (wrapper types substituted to primitives, disambiguating casts
/// inserted as <see cref="TirCoerce"/> nodes); this writer is then purely type-directed — it prints
/// the coercions and makes no float-land decisions (the old emit-time scalar-analysis path was
/// deleted at S3). In non-scalar (default) mode <c>_lowered</c> is false and literals/coercions
/// render in their wrapper form.
///
/// Type/self/namespace rendering is delegated to the same <see cref="CSharpTypeWriter"/> that emits
/// signatures; the property-getter wrapper / <c>=&gt;</c> / <c>;</c> / trailing-newline framing is
/// reproduced here so the whole member renders in one place.
/// </summary>
public class TirCSharpBodyWriter : CodeBuilder<TirCSharpBodyWriter>
{
    private readonly CSharpTypeWriter _tw;
    private readonly TirFunction _tir;
    private readonly string _selfType;
    private readonly CSharpFunctionInfo _fi;

    // Static mode (constants in Constants.g.cs, the IArray library functions in Extensions.g.cs):
    // parameter #0 is emitted by name instead of `this`, and the property-getter framing applies
    // to ZERO-parameter functions (a member needs its receiver parameter, a static does not).
    private readonly bool _isStatic;

    // >0 while rendering a lambda body: parameter #0 is emitted by name, not `this`
    // (the reference lambda body writer runs in "static" mode, isStatic:true).
    private int _lambdaDepth;

    // Scalar erasure (--scalar): true when the body is a LOWERED TirFunction (its wrapper types are
    // already substituted to the erased primitives and every scalar cast is an explicit TirCoerce
    // node the TirScalarLowerer inserted). The printer is then type-directed: it renders those
    // coercions as casts and makes NO float-land decisions of its own. Threaded from
    // RunOptimizerPasses (whether the lowering pass actually RAN), so it can never disagree with
    // what the pass did. False in non-scalar (default) mode, where the reference-writer rendering
    // (wrapper literal casts, implicit conversions) applies.
    private readonly bool _lowered;

    public TirCSharpBodyWriter(CSharpTypeWriter tw, TirFunction tir, bool isStatic = false,
        CSharpFunctionInfo fi = null, bool lowered = false)
    {
        IndentLevel = tw.IndentLevel;
        _tw = tw;
        _tir = tir;
        _selfType = tw.SelfType;
        _isStatic = isStatic;
        _fi = fi;
        _lowered = lowered;
        // Under scalar erasure every emitted body must be lowered (the type-directed printer has no
        // legacy fallback — the emit-time scalar analysis was deleted at S3, and the shipping
        // recipes lower every member AND static body). A non-lowered scalar body would render
        // mis-typed, so fail loudly. (A future stdlib addition that cannot lower shows up here.)
        if (tw.Writer.ScalarErase && !_lowered && tir?.Body != null)
            throw new System.InvalidOperationException(
                $"Scalar-erased body '{tw.TypeDef?.Name}.{fi?.Name}' was not scalar-lowered "
                + "(TirScalarLowerer.IsGroundBody rejected it); the legacy emit-time scalar-analysis path was removed at S3.");
        WriteFunctionBody();
    }

    private bool IsTypeName(string name)
        => name != null && _tw.Writer.AllTypeNames.Contains(name);

    // Extension style, MOVED bodies only (ExtensionReceiverName != null): a bare name that bound
    // implicitly inside the partial struct must be re-qualified inside the static library class —
    // receiver parameter for instance members (plus "()" for moved no-arg methods), the
    // namespace-qualified type name for statics, the namespace for bare type names. Mirrors the
    // FunctionGroupRefSymbol extension branch of the legacy writer exactly. In default mode (and
    // for struct-kept extension-style bodies) the context is null and the name is written bare.
    // MethodsOnly: whether a bare STATIC name of the current receiver type is a generated
    // static METHOD (needs "()"); handwritten statics keep member-access syntax.
    private bool IsGeneratedStaticMethodName(string name)
        => _tw.Writer.NoProperties && _tw.TypeDef != null
           && _tw.Writer.GetExtensionPlanByTypeName(_tw.TypeDef.Name) is ExtensionStylePlan plan
           && plan.GeneratedNoArgStaticNames.Contains(name);

    private void WriteBareName(string name)
    {
        if (_tw.ExtensionReceiverName == null)
        {
            Write(name);
            // MethodsOnly: a bare generated static of the enclosing type is a method.
            if (IsGeneratedStaticMethodName(name))
                Write("()");
            return;
        }
        if (_tw.ExtensionInstanceNames.Contains(name))
        {
            Write($"{_tw.ExtensionReceiverName}.{name}");
            // Moved no-arg members are classic extension methods, not properties. Under
            // --no-properties every no-arg member whose name is not pinned to struct-surface
            // property syntax is a method (this also covers the scalar types' members, whose
            // erased receiver is a primitive on which they are all extension methods).
            if (_tw.Writer.MovedNoArgNames.Contains(name)
                || (_tw.Writer.NoProperties && !_tw.Writer.StructSurfacePropertyNames.Contains(name)))
                Write("()");
            return;
        }
        if (_tw.ExtensionStaticNames.Contains(name))
        {
            Write($"{_tw.ExtensionStaticQualifier}.{name}");
            if (IsGeneratedStaticMethodName(name))
                Write("()");
            return;
        }
        // Type references: namespace-qualified because members of the enclosing static library
        // class can shadow type names.
        if (IsTypeName(name))
        {
            Write($"{_tw.Writer.Namespace}.{name}");
            return;
        }
        // Unknown to the compiler: a handwritten member of the receiver type (Number.Zero).
        // Under scalar erasure such statics are wrapper-typed; normalize to the primitive.
        if (_tw.ExtensionReceiverIsScalar
            && CSharpWriter.ScalarPrimitives.TryGetValue(_tw.TypeDef?.Name ?? "", out var sqPrim))
        {
            Write($"(({sqPrim}){_tw.ExtensionStaticQualifier}.{name})");
            return;
        }
        Write($"{_tw.ExtensionStaticQualifier}.{name}");
    }

    // Recognizes the normalizer's eta-expansion of a bare member-group reference in value
    // position: `M` -> `(_eta{id}_0, ..., _eta{id}_N) => M(_eta{id}_0, ..., _eta{id}_N)`
    // (Normalizer.EtaExpand, R3). The shape is a lambda whose parameters were all synthesized by
    // that pass (their names carry the `_eta` prefix it hard-codes) and whose body is a single
    // resolved call forwarding those parameters, in order, as the call's arguments (arity >= 1).
    // The reference writer consumes the ORIGINAL (un-normalized) graph, so it emits the bare member
    // name `M`; we recover it here. The `_eta`-name guard keeps a USER-authored forwarding lambda
    // (`(x) => x.M()`) — which the reference writer keeps as a lambda — from being collapsed.
    private static bool TryGetEtaForwardedName(TirLambda lam, out string name)
    {
        name = null;
        if (lam?.Parameters == null || lam.Parameters.Count == 0)
            return false;
        if (!lam.Parameters.All(p => p.Name != null && p.Name.StartsWith("_eta")))
            return false;
        if (!(lam.Body is TirCall call) || call.Name == null)
            return false;
        if (call.Args.Count != lam.Parameters.Count)
            return false;
        for (var i = 0; i < call.Args.Count; i++)
            if (!(call.Args[i] is TirParameter p) || !ReferenceEquals(p.Def, lam.Parameters[i]))
                return false;
        name = call.Name;
        return true;
    }

    // --- top level: mirror CSharpFunctionBodyWriter's default-mode framing exactly ----------

    private void WriteFunctionBody()
    {
        if (_tir?.Body == null)
        {
            WriteLine(" => throw new NotImplementedException();");
            return;
        }

        // A receiver-only member (1 parameter) or a nullary static is emitted as a property
        // getter — the same rule the reference writer applies per isStatic.
        var numParams = _tir.Original?.NumParameters ?? _tir.Parameters.Count;
        var isProp = _isStatic ? numParams == 0 : numParams == 1;
        // MethodsOnly (--methods): the signature declared a METHOD unless the name is pinned
        // to property syntax — frame the body to match.
        if (_tw.Writer.NoProperties && _fi != null && _fi.EmitAsMethod)
            isProp = false;

        // The reference writer hoists lambda-captured references into `var _var{N} = x;` blocks
        // before emitting (RewriteLambdasCapturingVars); mirror it, sharing the same counter.
        var body = TirLambdaCaptureRewriter.Rewrite(_tir.Body);
        var isBlock = body is TirBlock;

        if (isProp)
            Write($" {{ {CSharpTypeWriter.Annotation} get ");

        if (!isBlock)
        {
            Write(" => ");
            WriteNode(body);
            Write(";");
        }
        else
        {
            WriteStatement(body);
        }

        if (isProp)
            Write(" } ");

        WriteLine();
    }

    // --- statements ----------------------------------------------------------


    private void WriteStatement(TirNode n)
    {
        switch (n)
        {
            case TirBlock b:
                WriteStartBlock();
                foreach (var s in b.Statements)
                {
                    if (s == null) continue;
                    if (TirRewrite.IsStatementNode(s))
                    {
                        WriteStatement(s);
                    }
                    else
                    {
                        WriteNode(s);
                        WriteLine(";");
                    }
                }
                WriteEndBlock();
                return;

            case TirReturn r:
                Write("return ");
                WriteNode(r.Value);
                WriteLine(";");
                return;

            case TirLet let:
                Write("var ");
                Write(let.Def?.Name ?? "?");
                Write(" = ");
                WriteNode(let.Value);
                WriteLine(";");
                return;

            case TirIf iff:
                Write("if (");
                WriteNode(iff.Condition);
                WriteLine(")");
                WriteStatement(iff.IfTrue);
                if (iff.IfFalse != null)
                {
                    WriteLine("else");
                    WriteStatement(iff.IfFalse);
                }
                return;

            case TirLoop l:
                Write("while (");
                WriteNode(l.Condition);
                WriteLine(")");
                WriteStartBlock();
                WriteStatement(l.Body);
                WriteEndBlock();
                WriteLine();
                return;

            case TirLoweredLoop ll:
                WriteLoweredLoop(ll);
                return;

            default:
                // an expression used in statement position
                WriteNode(n);
                return;
        }
    }

    // --- expressions ---------------------------------------------------------

    private void WriteArgs(IEnumerable<TirNode> args)
    {
        var first = true;
        foreach (var a in args)
        {
            if (!first)
                Write(", ");
            first = false;
            WriteNode(a);
        }
    }

    private void WriteNode(TirNode n)
    {
        switch (n)
        {
            case null:
                return;

            case TirLiteral lit:
                // Scalar erasure (lowered): literals lose their wrapper casts and become native C#
                // literals. Non-scalar (default) mode keeps the wrapper cast.
                if (_lowered)
                {
                    var v = lit.Value.ToLiteralString();
                    Write(lit.LiteralType == Ara3D.Geometry.AST.LiteralTypesEnum.Number ? v + "f" : v);
                    return;
                }
                Write($"(({lit.LiteralType}){lit.Value.ToLiteralString()})");
                return;

            case TirParameter p:
            {
                var idx = p.Def?.Index ?? -1;
                var isThis = idx == 0 && !_isStatic && _lambdaDepth == 0;
                // Under scalar erasure the parameter's declared C# type is already the primitive, so
                // the reference is bare; any boundary cast is an explicit TirCoerce node.
                Write(isThis ? "this" : p.Def?.Name ?? "?");
                return;
            }

            case TirVariable v:
                Write(v.Def?.Name ?? "?");
                return;

            case TirTypeRef t:
                // A raw-TypeExpression reference is namespace-qualified (`Ara3D.Geometry.Number`),
                // a bound type reference is bare (`Self` -> the concrete type name) — the two
                // paths of the reference writer.
                if (t.NamespaceQualified && t.TypeDef?.Name != "Self")
                    Write($"{_tw.Writer.Namespace}.{t.TypeDef?.Name ?? "?"}");
                else
                    Write(t.TypeDef?.Name == "Self" ? _selfType : t.TypeDef?.Name ?? "?");
                return;

            case TirLet let:
                // A let in expression position (statement-position lets are handled in
                // WriteStatement); the reference writes the same `var` form.
                Write($"var {let.Def?.Name ?? "?"} = ");
                WriteNode(let.Value);
                return;

            case TirDefault:
                Write("default");
                return;

            case TirName nm:
                // A bare multi-overload group reference (`One`, `Zero`): the reference writer
                // emits the bare name and lets C# member lookup bind it (re-qualified in moved
                // extension-style bodies).
                WriteBareName(nm.Name);
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
                // Type-directed (lowered) rendering: a coercion whose target is a scalar PRIMITIVE
                // is an explicit disambiguating cast — render it. A coercion to a non-scalar type
                // (e.g. a float -> Vector3 broadcast) is left to C#'s implicit conversion, so the
                // inner value is written bare. This is the whole of the erased cast logic; the
                // TirScalarLowerer decided WHERE casts go, the printer just prints them.
                if (_lowered)
                {
                    // A same-type coercion (coerce<float->float>) whose inner is PROVABLY already
                    // that C# primitive is the spurious `((float)x)` noise the scalar lowerer emits
                    // on every erased operand — drop it. Restricted to inners whose emitted type is
                    // unambiguously the primitive (a native scalar literal, an erased param/var, or
                    // a real erased field via the component unroller); a wrapper-returning
                    // pseudo-field (`Vector2.X` → Number) is a TirCall and is NOT dropped, because
                    // its cast disambiguates a broadcast overload (Multiply(float) vs Multiply(Vector2)).
                    if (c.ToType?.Name != null && c.FromType?.Name == c.ToType.Name
                        && RendersAsPrimitive(TirRewrite.StripCoerce(c.Inner), c.ToType.Name))
                    {
                        WriteNode(c.Inner);
                        return;
                    }
                    // A cast to a scalar PRIMITIVE (float) disambiguates an erased overload; a cast
                    // to a scalar WRAPPER (Number) restores wrapper-ness at a broadcast/intrinsic
                    // boundary (float -> Number -> Vector2, which C# will not chain implicitly).
                    // Both render as explicit casts; any other coercion is a C# implicit conversion.
                    var to = c.ToType?.Name;
                    var innerPrim = c.Inner?.Type?.Def?.Name;
                    // A C# cast binds tighter than `?:` / assignment, so a low-precedence inner
                    // (a conditional) must be re-parenthesized: ((int)(cond ? a : b)), never
                    // ((int)cond ? a : b) — the latter casts the CONDITION.
                    var innerParens = c.Inner is TirConditional || c.Inner is TirAssign;
                    if (to != null && (CSharpWriter.ScalarPrimitives.ContainsValue(to) || CSharpWriter.ScalarPrimitives.ContainsKey(to))
                        && !IsSameScalarCast(c.Inner, to))
                    {
                        WriteScalarCast(to, c.Inner, innerParens);
                    }
                    else if (c.ToType?.Def?.Kind == Ara3D.Geometry.AST.TypeKind.ConcreteType
                        && innerPrim != null && CSharpWriter.ScalarPrimitives.ContainsValue(innerPrim))
                    {
                        // A BROADCAST coercion: a scalar value (float) to a CONCRETE struct target
                        // (Number -> Vector3). C# will not chain float -> Number -> Vector3, so
                        // restore the WRAPPER cast; the runtime's Number-sourced broadcast operator
                        // then applies (`((Number)(0f))` at CreateWorld's Vector3 position). Only a
                        // concrete target is pinned here — a scalar at an ungrounded INTERFACE target
                        // is routed to the legacy path by TirScalarLowerer.IsGroundBody.
                        WriteScalarCast(WrapperOfPrim(innerPrim), c.Inner, innerParens);
                    }
                    else
                    {
                        WriteNode(c.Inner);
                    }
                    return;
                }
                // Non-scalar (default) mode: a TirCoerce is a solver-inserted IMPLICIT-widening
                // conversion (the Elaborator only wraps ArgMatchKind.Conversion arguments):
                // Integer->Number, Self->interface, Number->Vector3 broadcast, etc. The reference
                // writer never renders these — it writes the inner value and lets C#'s implicit
                // conversion operators widen at the call boundary. Genuine SOURCE-level conversions
                // the programmer wrote arrive as a TirCall with EmissionKind.Conversion (WriteCall).
                WriteNode(c.Inner);
                return;

            case TirInvoke inv:
                WriteNode(inv.Target);
                Write(".Invoke(");
                WriteArgs(inv.Args);
                Write(")");
                return;

            case TirConditional cond:
                WriteNode(cond.Condition);
                Write(" ? ");
                WriteNode(cond.IfTrue);
                Write(" : ");
                WriteNode(cond.IfFalse);
                return;

            case TirNew nw:
                // Scalar erasure (lowered): "new Number(x)" would erase to the invalid "new float(x)";
                // a scalar constructor call is just a cast of its single argument (the constructed
                // type IS the primitive already after lowering).
                if (_lowered && nw.Args.Count == 1 && nw.NewType?.Name != null
                    && CSharpWriter.ScalarPrimitives.ContainsValue(nw.NewType.Name))
                {
                    Write($"(({nw.NewType.Name})");
                    WriteNode(nw.Args[0]);
                    Write(")");
                    return;
                }
                Write($"new {_tw.ToCSharpType(nw.NewType)}(");
                WriteArgs(nw.Args);
                Write(")");
                return;

            case TirArray arr:
            {
                // Pin the element type with an explicit MakeArray<T> when the array's element type
                // is a concrete renderable type. Without it, C# infers T from the arguments, which
                // loses a per-element implicit conversion (e.g. a Vector3-valued element in a
                // Point3D array — the case the fixed-array unroller exposes when it replaces the
                // loop lowerer's `new Point3D[n]`, which pinned the type). Fall back to inference
                // for a type-variable / non-renderable element.
                var elem = arr.Type != null && arr.Type.TypeArgs.Count == 1 ? arr.Type.TypeArgs[0] : null;
                var elemName = elem?.Name;
                var renderable = !string.IsNullOrEmpty(elemName) && !elem.IsTypeVariable
                    && (char.IsLetter(elemName[0]) || elemName[0] == '_');
                Write(renderable ? $"Intrinsics.MakeArray<{_tw.ToCSharpType(elem)}>(" : "Intrinsics.MakeArray(");
                WriteArgs(arr.Elements);
                Write(")");
                return;
            }

            case TirAssign asg:
                WriteNode(asg.LValue);
                Write(" = ");
                WriteNode(asg.RValue);
                return;

            case TirLambda lam:
                // The normalizer eta-expands a bare member-group reference used in value position
                // into a forwarding lambda `(_p0..._pN) => M(_p0, ..., _pN)` (Normalizer R3). The
                // reference writer never saw that rewrite — it consumes the original graph and
                // emits the bare member name `M`. Recover the bare reference so we match it.
                if (TryGetEtaForwardedName(lam, out var etaName))
                {
                    WriteBareName(etaName);
                    return;
                }
                Write("(");
                Write(string.Join(", ", lam.Parameters.Select(p => p.Name)));
                Write(") ");
                _lambdaDepth++;
                // The reference constructs a fresh body writer per lambda, whose constructor
                // re-runs the capture hoist on the lambda's own body, and writes " => " only for
                // an expression body. Mirror both (a hoisted lambda body is block-shaped).
                var lamBody = TirLambdaCaptureRewriter.Rewrite(lam.Body);
                if (TirRewrite.IsStatementNode(lamBody))
                {
                    // A C# lambda with a statement body still needs the arrow. No such lambda
                    // occurs in any non-inline output (the flag differentials pin that), but the
                    // inliner can create capture-hoisted block bodies inside lambdas.
                    if (lamBody is TirBlock)
                        Write("=> ");
                    WriteStatement(lamBody);
                }
                else
                {
                    Write(" => ");
                    WriteNode(lamBody);
                }
                _lambdaDepth--;
                return;

            // Emission-only marker nodes produced by TirComponentUnroller (--optimize, P3.1).
            case TirComponentAccess ca:
                if (ca.CastTo != null)
                {
                    Write($"(({ca.CastTo})");
                    WriteNode(ca.Receiver);
                    Write($".{ca.FieldName})");
                }
                else
                {
                    WriteNode(ca.Receiver);
                    Write($".{ca.FieldName}");
                }
                return;

            case TirConstructorCall cc:
                // In extension-style library classes type names can be shadowed by members of
                // the enclosing static class, so qualify with the namespace (legacy rule).
                Write("new ");
                Write(_tw.ExtensionReceiverName != null ? $"{_tw.Writer.Namespace}.{cc.TypeName}" : cc.TypeName);
                Write("(");
                WriteArgs(cc.Args);
                Write(")");
                return;

            case TirTempRef tr:
                Write(tr.Name);
                return;

            case TirBooleanChain bc:
                for (var i = 0; i < bc.Terms.Count; i++)
                {
                    if (i > 0)
                        Write($" {bc.Op} ");
                    WriteNode(bc.Terms[i]);
                }
                return;

            case TirUnresolved u:
                // A call the elaborator could not resolve (bare nullary group ref, local var,
                // unhandled node). Deliberately un-mimicable so it always counts as a mismatch.
                Write($"/*unresolved:{u.Original?.Name}*/");
                return;

            default:
                Write("/*?*/");
                return;
        }
    }


    // Scalar erasure only: the primitive a TIR NODE is known to erase to, when the origin-symbol
    // analysis cannot know it. The one case: component-access markers substituted by the
    // Lowered rendering: an inner node that already renders as a cast to the same scalar primitive
    // makes an enclosing cast to it redundant (collapses `((float)((float)x))` -> `((float)x)`).
    private static bool IsSameScalarCast(TirNode inner, string prim)
        => inner is TirCoerce c && c.ToType?.Name == prim;

    /// <summary>Whether <paramref name="inner"/> emits as C# code whose type is unambiguously the
    /// scalar primitive <paramref name="to"/> (either the primitive name like "float" or its
    /// wrapper "Number"), so a same-type cast around it is pure redundant noise. Conservative — only
    /// cases whose EMITTED type is provably the primitive regardless of surrounding overloads:
    ///   * a native scalar literal;
    ///   * a real erased component field the unroller produced (`TirComponentAccess` with no wrapper
    ///     cast, carrying its <see cref="TirComponentAccess.ScalarComponentPrim"/>);
    ///   * an erased parameter that is NOT the `this` receiver of a scalar WRAPPER-struct instance
    ///     member — there `this` is the wrapper (`Number`), and its cast pins a broadcast overload
    ///     (`Number.Multiply(Vector2)` vs `(float)`); loop-temp lambda params (rendered under a
    ///     lambda) and ordinary erased params are fine.
    /// Excluded: pseudo-field method calls (`Vector2.X` → wrapper), `TirVariable` lets, and any
    /// other node whose emitted type is context-dependent.</summary>
    private bool RendersAsPrimitive(TirNode inner, string to)
    {
        var prim = CSharpWriter.ScalarPrimitives.TryGetValue(to, out var p) ? p : to;
        bool ErasesTo(TypeExpression t)
        {
            var name = t?.Def?.Name ?? t?.Name;
            if (name == null) return false;
            return name == prim || (CSharpWriter.ScalarPrimitives.TryGetValue(name, out var q) && q == prim);
        }
        switch (inner)
        {
            case TirLiteral:
                return true;
            case TirComponentAccess ca:
                return ca.CastTo == null && ca.ScalarComponentPrim == prim;
            case TirParameter par:
                var isThisReceiver = par.Def?.Index == 0 && !_isStatic && _lambdaDepth == 0;
                return !isThisReceiver && ErasesTo(par.Type);
            case TirCall call when call.EmissionKind == Ara3D.Geometry.Compiler.Checking.EmissionKind.Operator:
                // A scalar OPERATOR (Add/Subtract/Multiply/...) whose result erases to the primitive:
                // its emitted type is that primitive (a broadcast operator would have a vector result
                // type, so ErasesTo fails and its cast is kept).
                return ErasesTo(call.Type);
            default:
                return false;
        }
    }

    // Render `(({cast})inner)`, parenthesizing a low-precedence inner (a conditional would
    // otherwise bind the cast to its condition — see the TirCoerce case).
    private void WriteScalarCast(string cast, TirNode inner, bool innerParens)
    {
        Write($"(({cast})");
        if (innerParens) Write("(");
        WriteNode(inner);
        if (innerParens) Write(")");
        Write(")");
    }

    // The scalar WRAPPER for an erased primitive (float -> Number) — used to restore wrapper-ness at
    // a broadcast boundary where C# will not chain float -> Number -> {struct} implicitly.
    private static string WrapperOfPrim(string prim)
        => CSharpWriter.ScalarPrimitives.FirstOrDefault(kv => kv.Value == prim).Key;

    private void WriteCall(TirCall call)
    {
        // Under scalar lowering every scalar-returning-wrapper call site already carries an explicit
        // disambiguating TirCoerce (inserted by TirScalarLowerer), so no float-land wrapping here.
        WriteCallCore(call);
    }

    private void WriteCallCore(TirCall call)
    {
        var name = call.Name;
        var args = call.Args;

        // Zero-argument call: the reference emits a Constants.<Name> reference. (The `default`
        // keyword arrives as TirDefault, not here.)
        if (args.Count == 0)
        {
            Write($"Constants.{name}");
            // MethodsOnly: constants are static methods.
            if (_tw.Writer.NoProperties)
                Write("()");
            return;
        }

        // Tuple constructor: TupleN(a, b, ...) -> (a, b, ...).
        if (name != null && name.StartsWith("Tuple"))
        {
            Write("(");
            WriteArgs(args);
            Write(")");
            return;
        }

        // A lowered-loop result temp is a C# ARRAY: Count reads become Length.
        if (name == "Count" && args.Count == 1 && TirRewrite.StripCoerce(args[0]) is TirTempRef)
        {
            WriteNode(args[0]);
            Write(".Length");
            return;
        }

        // Indexer: At(a, i, ...) -> a[i, ...]. MethodsOnly: generated structs have no indexer,
        // so a receiver of a known generated type calls .At(...) instead (IReadOnlyList and
        // other unknown receivers keep their own indexers).
        if (name == "At")
        {
            var recvType = TirRewrite.StripCoerce(args[0])?.Type?.Name;
            if (_tw.Writer.NoProperties && recvType != null
                && _tw.Writer.GetExtensionPlanByTypeName(recvType) != null
                && !CSharpWriter.ScalarPrimitives.ContainsKey(recvType))
            {
                WriteNode(args[0]);
                Write(".At(");
                WriteArgs(args.Skip(1));
                Write(")");
                return;
            }
            WriteNode(args[0]);
            Write("[");
            WriteArgs(args.Skip(1));
            Write("]");
            return;
        }

        // Member / method-call form: receiver first, then `.Name`, then `(rest)` unless it reads
        // as a property (no-arg member access) or a type-named conversion property. A ternary
        // (or lambda/assignment) receiver must be parenthesized or the member access binds to its
        // last operand; such receivers only arise from the inliner (the flag differentials pin
        // that no non-inline output has them).
        var recvNode = TirRewrite.StripCoerce(args[0]);
        if (recvNode is TirConditional || recvNode is TirLambda || recvNode is TirAssign
            || recvNode is TirBooleanChain)
        {
            Write("(");
            WriteNode(args[0]);
            Write(")");
        }
        else
        {
            WriteNode(args[0]);
        }
        Write(".");
        Write(name);

        var noParens = args.Count == 1
            && (call.EmissionKind == EmissionKind.Property
                || (call.EmissionKind == EmissionKind.Conversion && IsTypeName(name)));
        if (noParens)
        {
            // Extension style: no-arg library functions that moved out of their structs are
            // classic extension METHODS (v.Length()), so their call sites need "()". Applies in
            // ALL extension-style bodies (kept and moved) — the moved/kept name partition is
            // global, so this is decidable by name.
            if (_tw.Writer.ExtensionStyle && _tw.Writer.MovedNoArgNames.Contains(name))
            {
                Write("()");
                return;
            }
            // MethodsOnly: every no-arg member access is a method call unless the name is
            // pinned to property/field syntax. STATIC member accesses (type-ref receiver) are
            // decided by the target type's plan: generated statics are methods, handwritten
            // statics (Number.MinValue) keep member syntax.
            if (_tw.Writer.NoProperties)
            {
                if (TirRewrite.StripCoerce(args[0]) is TirTypeRef recvT)
                {
                    var recvPlan = _tw.Writer.GetExtensionPlanByTypeName(recvT.TypeDef?.Name);
                    if (recvPlan != null && recvPlan.GeneratedNoArgStaticNames.Contains(name))
                        Write("()");
                }
                else if (!_tw.Writer.StructSurfacePropertyNames.Contains(name))
                    Write("()");
            }
            return;
        }

        // Every scalar disambiguation is an explicit TirCoerce the lowerer inserted; the printer
        // just writes each argument (TirCoerce renders as the cast in WriteNode).
        Write("(");
        var argIndex = 1;
        foreach (var a in args.Skip(1))
        {
            if (argIndex > 1)
                Write(", ");
            WriteNode(a);
            argIndex++;
        }
        Write(")");
    }

    // --- loop lowering emission (--loops; see TirLoopLowerer) --------------------------------

    private void WriteLoweredLoop(TirLoweredLoop ll)
    {
        var k = ll.Id;
        // Prefer the lowerer's authoritative result-element type (taken from the producing
        // function's own type); fall back to the call's zonked IArray<T> argument.
        var elem = ll.ElemType?.Name != null
            ? _tw.ToCSharpType(ll.ElemType)
            : ll.Type != null && ll.Type.TypeArgs.Count == 1 ? _tw.ToCSharpType(ll.Type.TypeArgs[0]) : null;
        var srcNames = new List<string>();
        var srcCounts = new List<string>();
        for (var s = 0; s < ll.Sources.Count; s++)
        {
            var name = ll.Sources.Count == 1 ? $"_s{k}" : $"_s{k}{(char)('a' + s)}";
            srcNames.Add(name);
            // A source that is itself a lowered-loop result is a C# ARRAY: Length, not Count.
            srcCounts.Add(TirRewrite.StripCoerce(ll.Sources[s]) is TirTempRef ? $"{name}.Length" : $"{name}.Count");
            Write($"var {name} = ");
            WriteNode(ll.Sources[s]);
            WriteLine(";");
        }
        var n = $"_n{k}";
        var i = $"_i{k}";
        var t = ll.TempName;

        void For(string count, System.Action body)
        {
            WriteLine($"for (var {i} = 0; {i} < {count}; {i}++)");
            WriteStartBlock();
            body();
            WriteEndBlock();
        }

        // The function argument: a lambda literal's body is emitted INLINE with its parameters
        // declared as loop locals; a delegate-typed reference is invoked.
        var fn = ll.Fn == null ? null : TirRewrite.StripCoerce(ll.Fn);
        var lam = fn as TirLambda;
        void DeclareParams(params string[] elems)
        {
            if (lam == null)
                return;
            for (var p = 0; p < lam.Parameters.Count && p < elems.Length; p++)
                WriteLine($"var {lam.Parameters[p].Name} = {elems[p]};");
        }
        void WriteFnValue(params string[] elems)
        {
            if (lam != null)
            {
                _lambdaDepth++;
                WriteNode(lam.Body);
                _lambdaDepth--;
            }
            else
            {
                WriteNode(fn);
                Write("(");
                Write(string.Join(", ", elems));
                Write(")");
            }
        }

        switch (ll.Kind)
        {
            case "Map" or "MapIdx":
                WriteLine($"var {n} = {srcCounts[0]};");
                WriteLine($"var {t} = new {elem}[{n}];");
                For(n, () =>
                {
                    var elems = ll.Kind == "MapIdx"
                        ? new[] { $"{srcNames[0]}[{i}]", i }
                        : new[] { $"{srcNames[0]}[{i}]" };
                    DeclareParams(elems);
                    Write($"{t}[{i}] = ");
                    WriteFnValue(elems);
                    WriteLine(";");
                });
                break;
            case "MapRange":
                WriteLine($"var {t} = new {elem}[{srcNames[0]}];");
                For($"{srcNames[0]}", () =>
                {
                    DeclareParams(i);
                    Write($"{t}[{i}] = ");
                    WriteFnValue(i);
                    WriteLine(";");
                });
                break;
            case "Zip2":
            case "Zip3":
                Write($"var {n} = System.Math.Min({srcCounts[0]}, {srcCounts[1]});");
                WriteLine(ll.Kind == "Zip3" ? $" {n} = System.Math.Min({n}, {srcCounts[2]});" : "");
                WriteLine($"var {t} = new {elem}[{n}];");
                For(n, () =>
                {
                    var elems = srcNames.Select(sn => $"{sn}[{i}]").ToArray();
                    DeclareParams(elems);
                    Write($"{t}[{i}] = ");
                    WriteFnValue(elems);
                    WriteLine(";");
                });
                break;
            case "Reduce":
                Write($"var {t} = ");
                WriteNode(ll.Seed);
                WriteLine(";");
                WriteLine($"var {n} = {srcCounts[0]};");
                For(n, () =>
                {
                    DeclareParams(t, $"{srcNames[0]}[{i}]");
                    Write($"{t} = ");
                    WriteFnValue(t, $"{srcNames[0]}[{i}]");
                    WriteLine(";");
                });
                break;
            case "All":
            case "Any":
                var isAll = ll.Kind == "All";
                WriteLine($"var {t} = {(isAll ? "true" : "false")};");
                WriteLine($"var {n} = {srcCounts[0]};");
                For(n, () =>
                {
                    DeclareParams($"{srcNames[0]}[{i}]");
                    Write(isAll ? "if (!(bool)(" : "if ((bool)(");
                    WriteFnValue($"{srcNames[0]}[{i}]");
                    WriteLine("))");
                    WriteStartBlock();
                    WriteLine($"{t} = {(isAll ? "false" : "true")};");
                    WriteLine("break;");
                    WriteEndBlock();
                });
                break;
            case "Reverse":
                WriteLine($"var {n} = {srcCounts[0]};");
                WriteLine($"var {t} = new {elem}[{n}];");
                For(n, () => WriteLine($"{t}[{i}] = {srcNames[0]}[{n} - 1 - {i}];"));
                break;
            case "WithNext":
            {
                WriteLine($"var {n} = {srcCounts[0]};");
                Write($"var _m{k} = (bool)(");
                WriteNode(ll.IncludeFirst);
                WriteLine($") ? {n} : ({n} > 1 ? {n} - 1 : 0);");
                WriteLine($"var {t} = new {elem}[_m{k}];");
                For($"_m{k}", () =>
                {
                    DeclareParams($"{srcNames[0]}[{i}]", $"{srcNames[0]}[({i} + 1) % {n}]");
                    Write($"{t}[{i}] = ");
                    WriteFnValue($"{srcNames[0]}[{i}]", $"{srcNames[0]}[({i} + 1) % {n}]");
                    WriteLine(";");
                });
                break;
            }
            case "MapPairs":
            case "MapTriplets":
            case "MapQuartets":
            {
                var stride = ll.Kind == "MapPairs" ? 2 : ll.Kind == "MapTriplets" ? 3 : 4;
                WriteLine($"var {n} = {srcCounts[0]} / {stride};");
                WriteLine($"var {t} = new {elem}[{n}];");
                For(n, () =>
                {
                    var elems = Enumerable.Range(0, stride)
                        .Select(o => o == 0 ? $"{srcNames[0]}[{i} * {stride}]" : $"{srcNames[0]}[{i} * {stride} + {o}]")
                        .ToArray();
                    DeclareParams(elems);
                    Write($"{t}[{i}] = ");
                    WriteFnValue(elems);
                    WriteLine(";");
                });
                break;
            }
            default:
                WriteLine($"/*unknown lowered loop kind: {ll.Kind}*/");
                break;
        }
    }
}
