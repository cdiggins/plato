using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// EXPERIMENTAL — OFF THE DEFAULT CODE-GENERATION PATH.
///
/// Renders a C# function <em>body</em> from a fully-ground <see cref="TirFunction"/> (the Typed IR
/// produced by <see cref="Elaborator"/> + <see cref="Monomorphizer"/>), targeting the <b>default</b>
/// C# style that <c>regen-plato.ps1</c> compares against. It is the retarget probe for the
/// Elaborate → Monomorphize → Emit phase: instead of re-deriving semantics at emit time the way
/// <see cref="CSharpFunctionBodyWriter"/> does (<c>HasArgList</c>, property-vs-method guessing,
/// call-site <c>()</c> injection, scalar re-inference), it maps the recorded
/// <see cref="EmissionKind"/> of every <see cref="TirCall"/> directly to syntax and renders every
/// implicit conversion from the explicit <see cref="TirCoerce"/> node.
///
/// NOTHING in the production pipeline constructs this type — <see cref="CSharpWriter"/> /
/// <see cref="CSharpConcreteTypeWriter"/> never reference it. It exists only so the differential
/// harness (PlatoTests/EmitDifferentialTests) can measure, byte-for-byte, how close a TIR-based
/// body writer gets to the current writer over the fully-ground subset. So off-flag output stays
/// byte-identical.
///
/// To keep the experiment honest, type/self/namespace rendering is delegated to the SAME
/// <see cref="CSharpTypeWriter"/> the reference uses, and the property-getter wrapper / <c>=&gt;</c> /
/// <c>;</c> / trailing-newline framing is reproduced from
/// <see cref="CSharpFunctionBodyWriter"/> verbatim — so every measured difference is attributable to
/// how the <em>expression tree</em> is emitted (symbol-graph heuristics vs. TIR), which is the
/// research question.
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

    // Scalar erasure (--scalar=float) only; null otherwise. The "float-land" decision procedures,
    // consulted over the ORIGIN symbols of TIR nodes (same inputs as the legacy writer, so the
    // same conservative answers), while the syntax renders from the TIR. Swapped per lambda body
    // (each lambda gets its own parameter-primitive context, like the legacy per-lambda writers).
    private ScalarEraseAnalysis _scalar;

    // Mission 2: when true (--scalar erasure), the body is a LOWERED TirFunction — its types are
    // already the erased primitives and every scalar cast is an explicit TirCoerce node inserted by
    // TirScalarLowerer. The printer is then type-directed: it renders coercions as casts and makes
    // NO float-land decisions, so _scalar is never built. (The legacy ScalarEraseAnalysis path and
    // its ~16 decision sites are retired by S3 once this is proven.)
    private readonly bool _lowered;

    // Scalar erasure only: the primitive the parameters of a lambda ARGUMENT erase to, when the
    // enclosing call site could determine it (element-wise HOFs). Mirrors the legacy
    // PendingLambdaParamPrim channel.
    private string _pendingLambdaParamPrim;

    public TirCSharpBodyWriter(CSharpTypeWriter tw, TirFunction tir, bool isStatic = false,
        CSharpFunctionInfo fi = null, string lambdaParamPrim = null)
    {
        IndentLevel = tw.IndentLevel;
        _tw = tw;
        _tir = tir;
        _selfType = tw.SelfType;
        _isStatic = isStatic;
        _fi = fi;
        _lowered = tw.Writer.ScalarErase && tw.Writer.UseScalarLowering;
        // Legacy float-land path (every scalar recipe until UseScalarLowering is the default).
        if (tw.Writer.ScalarErase && !_lowered && fi != null)
            _scalar = new ScalarEraseAnalysis(tw, fi.Function.Implementation, fi.ParameterTypes, lambdaParamPrim);
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
            // Moved no-arg members are classic extension methods, not properties. Under scalar
            // erasure the receiver of a scalar type's moved member is a primitive, so ALL its
            // no-arg members are extension methods too. MethodsOnly: every no-arg member whose
            // name is not pinned to property syntax is a method.
            if (_tw.Writer.MovedNoArgNames.Contains(name)
                || (_tw.ExtensionReceiverIsScalar && _tw.Writer.ScalarMemberNames.Contains(name))
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
                // Scalar erasure: literals lose their wrapper casts and become native C# literals.
                if (_lowered || _scalar != null)
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
                // Scalar erasure: scalar-typed parameter references normalize to the primitive.
                var prim = _scalar?.ScalarPrimitiveOfParam(p.Def);
                if (prim != null)
                {
                    Write(isThis ? $"(({prim})this)" : $"(({prim}){p.Def?.Name ?? "?"})");
                    return;
                }
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
                    // A cast to a scalar PRIMITIVE (float) disambiguates an erased overload; a cast
                    // to a scalar WRAPPER (Number) restores wrapper-ness at a broadcast/intrinsic
                    // boundary (float -> Number -> Vector2, which C# will not chain implicitly).
                    // Both render as explicit casts; any other coercion is a C# implicit conversion.
                    var to = c.ToType?.Name;
                    if (to != null && (CSharpWriter.ScalarPrimitives.ContainsValue(to) || CSharpWriter.ScalarPrimitives.ContainsKey(to))
                        && !IsSameScalarCast(c.Inner, to))
                    {
                        Write($"(({to})");
                        WriteNode(c.Inner);
                        Write(")");
                    }
                    else
                    {
                        WriteNode(c.Inner);
                    }
                    return;
                }
                // A TirCoerce is ALWAYS a solver-inserted IMPLICIT-widening conversion (the
                // Elaborator only wraps ArgMatchKind.Conversion arguments): Integer->Number,
                // Self->interface, Number->Vector3 broadcast, etc. The reference writer never
                // renders these — it writes the inner value and lets C#'s implicit conversion
                // operators do the widening at the call boundary. So we suppress the conversion
                // and emit the inner node only. Genuine SOURCE-level conversions the programmer
                // wrote (`Vector3(0.0)`, `x.Number`) arrive as a TirCall with
                // EmissionKind.Conversion and are rendered by WriteCall — those are kept as-is.
                // MethodsOnly: a scalar -> concrete-type BROADCAST must go through the WRAPPER
                // (the broadcast implicit operators are deliberately wrapper-sourced), and the
                // erased scalar expression no longer converts on its own.
                if (_tw.Writer.NoProperties && _scalar != null && c.ToType?.Name != null
                    && !CSharpWriter.ScalarPrimitives.ContainsKey(c.ToType.Name)
                    && _tw.Writer.GetExtensionPlanByTypeName(c.ToType.Name) != null)
                {
                    var innerPrim = AuthoritativePrim(c.Inner, null);
                    if (innerPrim != null)
                    {
                        Write($"(({ScalarEraseAnalysis.WrapperOfPrim(innerPrim)})(");
                        WriteNode(c.Inner);
                        Write("))");
                        return;
                    }
                }
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
                // Scalar erasure: "new Number(x)" would erase to the invalid "new float(x)";
                // a scalar constructor call is just a cast of its single argument. Lowered: the
                // constructed type IS the primitive already; legacy: the wrapper name still maps.
                if (_lowered && nw.Args.Count == 1 && nw.NewType?.Name != null
                    && CSharpWriter.ScalarPrimitives.ContainsValue(nw.NewType.Name))
                {
                    Write($"(({nw.NewType.Name})");
                    WriteNode(nw.Args[0]);
                    Write(")");
                    return;
                }
                if (_scalar != null && nw.Args.Count == 1
                    && CSharpWriter.ScalarPrimitives.TryGetValue(nw.NewType?.Name ?? "", out var snPrim))
                {
                    Write($"(({snPrim})");
                    WriteNode(nw.Args[0]);
                    Write(")");
                    return;
                }
                Write($"new {_tw.ToCSharpType(nw.NewType)}(");
                WriteArgs(nw.Args);
                Write(")");
                return;

            case TirArray arr:
                // Approximation: the reference threads the expected IArray element type to pick a
                // MakeArray<T> overload; the TIR does not carry the usage type here.
                Write("Intrinsics.MakeArray(");
                WriteArgs(arr.Elements);
                Write(")");
                return;

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
                // Scalar erasure: each lambda body gets its own analysis context, seeded with
                // the parameter primitive the enclosing call site determined (the legacy writer
                // constructs a fresh body writer per lambda with lambdaParamPrim).
                var savedScalar = _scalar;
                var savedPending = _pendingLambdaParamPrim;
                // Defensive: a generic lambda origin (type-variable first parameter) cannot build
                // a FunctionInstance; keep the enclosing analysis (the inliner refuses to move
                // such lambdas out of their home context, so this only guards future callers).
                if (_tw.Writer.ScalarErase && lam.Origin is Lambda lamSym
                    && !(lamSym.Function.NumParameters > 0 && lamSym.Function.Parameters[0].Type.IsTypeVariable))
                {
                    var lfi = _tw.ToFunctionInfo(lamSym.Function, null, FunctionInstanceKind.Lambda);
                    _scalar = new ScalarEraseAnalysis(_tw, lfi.Function.Implementation, lfi.ParameterTypes, _pendingLambdaParamPrim);
                    _pendingLambdaParamPrim = null;
                }
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
                _scalar = savedScalar;
                _pendingLambdaParamPrim = savedPending;
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
    // TIR unroller (--optimize) — their origin still shows the pre-unroll lambda parameter, so
    // the marker carries the primitive itself (mirrors the legacy ScalarComponentPrim channel).
    private string NodeScalarPrim(TirNode n)
        => TirRewrite.StripCoerce(n) is TirComponentAccess ca ? ca.ScalarComponentPrim : null;

    // Scalar erasure + --inline only: the erased primitive of a LITERAL node. Inlining puts
    // literals in receiver/argument positions whose enclosing call's origin symbols still point
    // at the callee's source (a `Pi` group reference), where the origin analysis answers null;
    // the literal itself is authoritative. Deliberately NOT generalized to arbitrary node types:
    // zonked node types can be looser than the emitted surface (Self-unifies-anything), so only
    // literals are trusted. Gated on InlineCalls so non-inline output stays byte-identical.
    private string AuthoritativePrim(TirNode n, string originPrim)
    {
        var marker = NodeScalarPrim(n);
        if (marker != null)
            return marker;
        if (!_tw.Writer.InlineCalls)
            return originPrim;
        // The inliner tags substituted arguments with the callee's zonked scalar parameter type
        // via a transparent TirCoerce (see TirInliner) — walking outside-in, a scalar coercion
        // target is authoritative, and a KNOWN CONCRETE non-scalar target (a solver broadcast
        // like Number -> Vector3) is authoritatively NOT scalar; only unknown targets
        // (interfaces, type variables) are skipped.
        for (var m = n; m is TirCoerce c; m = c.Inner)
        {
            if (c.ToType?.Name == null)
                continue;
            if (CSharpWriter.ScalarPrimitives.TryGetValue(c.ToType.Name, out var coPrim))
                return coPrim;
            if (_tw.Writer.GetExtensionPlanByTypeName(c.ToType.Name) != null)
                return null;
        }
        var stripped = TirRewrite.StripCoerce(n);
        if (stripped is TirLiteral lit)
            switch (lit.LiteralType)
            {
                case Ara3D.Geometry.AST.LiteralTypesEnum.Number: return "float";
                case Ara3D.Geometry.AST.LiteralTypesEnum.Integer: return "int";
                case Ara3D.Geometry.AST.LiteralTypesEnum.Boolean: return "bool";
                case Ara3D.Geometry.AST.LiteralTypesEnum.String: return "string";
            }
        if (originPrim != null)
            return originPrim;
        // Substituted nodes: the enclosing call's origin points at the callee's (generic)
        // source, but the node itself kept ITS home origin — analyze that instead.
        return _scalar != null && stripped?.Origin is Expression oe ? _scalar.ScalarPrimOf(oe) : null;
    }

    // Lowered rendering: an inner node that already renders as a cast to the same scalar primitive
    // makes an enclosing cast to it redundant (collapses `((float)((float)x))` -> `((float)x)`).
    private static bool IsSameScalarCast(TirNode inner, string prim)
        => inner is TirCoerce c && c.ToType?.Name == prim;

    private void WriteCall(TirCall call)
    {
        // Scalar erasure: wrap calls whose function GROUP determinately returns a scalar wrapper
        // in a cast to the primitive ("float-land"). The group lives on the origin FunctionCall;
        // a bare group reference elaborated to a zero-arg TirCall has a FunctionGroupRefSymbol
        // origin and is NOT wrapped — exactly the legacy writer's two paths.
        var scalarPrim = _scalar != null && call.Origin is FunctionCall originCall
            ? ScalarEraseAnalysis.ScalarReturnPrimitive(originCall.Function)
            : null;
        if (scalarPrim != null)
            Write($"(({scalarPrim})");
        WriteCallCore(call);
        if (scalarPrim != null)
            Write(")");
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

        var originFc = call.Origin as FunctionCall;

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
            // Scalar erasure: every member of the five scalar types is an extension METHOD on
            // the primitive, so a no-arg access on a provably scalar-valued receiver needs "()".
            if (_scalar != null && _tw.Writer.ScalarMemberNames.Contains(name)
                && (AuthoritativePrim(args[0], null) != null
                    || (originFc != null && originFc.Args.Count >= 1
                        && originFc.Args[0] is Expression origRecv && _scalar.IsScalarValued(origRecv))))
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

        // Scalar erasure: when a lambda argument's parameters receive the ELEMENTS of the
        // receiver, tell the lambda writer the primitive they erase to (legacy channel).
        var savedPrim = _pendingLambdaParamPrim;
        if (_scalar != null && originFc != null && originFc.Args.Skip(1).Any(a => a is Lambda))
            _pendingLambdaParamPrim = ScalarEraseAnalysis.ElementWiseHofNames.Contains(name)
                && originFc.Args[0] is Expression hofRecv
                    ? _scalar.ElementPrimOf(hofRecv)
                    : null;

        // Scalar erasure: pin provably scalar arguments to the declared parameter primitives of
        // a uniquely matching all-scalar overload (exact match), or restore wrapper-ness of
        // scalar arguments at non-scalar member call sites — the legacy writer's two cast rules.
        IReadOnlyList<string> argPrims = null;
        string receiverPrim = null;
        if (_scalar != null && _tw.Writer.InlineCalls)
        {
            // Inlined bodies: the enclosing origin may not be a group call (operator origins),
            // but the receiver node itself may still be provably scalar — without this, a
            // known-scalar argument at an unknown receiver takes the wrapper-restoring cast
            // and turns an exact float binding into an ambiguous one.
            var recvOrigin = originFc != null && originFc.Args.Count >= 1 ? originFc.Args[0] as Expression : null;
            receiverPrim = AuthoritativePrim(args[0], recvOrigin != null ? _scalar.ScalarPrimOf(recvOrigin) : null);
        }
        if (_scalar != null && originFc != null && originFc.Function is FunctionGroupRefSymbol fgrc
            && originFc.Args.Count >= 1 && originFc.Args[0] is Expression recvExpr)
        {
            receiverPrim = receiverPrim ?? AuthoritativePrim(args[0], _scalar.ScalarPrimOf(recvExpr));
            if (receiverPrim != null)
            {
                var overloads = _scalar.MatchingScalarOverloads(fgrc.Name, originFc.Args.Count, receiverPrim);
                if (overloads != null && overloads.Count >= 1)
                {
                    var first = overloads[0].Params;
                    var agree = true;
                    foreach (var o in overloads)
                        for (var i = 1; i < o.Params.Count && agree; ++i)
                            agree = o.Params[i] == first[i];
                    if (agree)
                        argPrims = first;
                }
            }
        }

        Write("(");
        var argIndex = 1;
        foreach (var a in args.Skip(1))
        {
            if (argIndex > 1)
                Write(", ");
            var symArg = _scalar != null && originFc != null && argIndex < originFc.Args.Count
                ? originFc.Args[argIndex] as Expression
                : null;
            var argPrim = _scalar != null
                ? AuthoritativePrim(a, symArg != null ? _scalar.ScalarPrimOf(symArg) : null)
                : null;
            if (argPrims != null && argPrim != null
                && CSharpWriter.ScalarPrimitives.TryGetValue(argPrims[argIndex], out var castPrim))
            {
                WriteScalarCastArg(castPrim, a);
            }
            else if (argPrim != null && receiverPrim == null)
            {
                WriteScalarCastArg(RestoreCastType(call, argIndex, argPrim), a);
            }
            else
            {
                WriteCallArg(a);
            }
            argIndex++;
        }
        Write(")");
        _pendingLambdaParamPrim = savedPrim;
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
                var savedScalar = _scalar;
                var savedPending = _pendingLambdaParamPrim;
                if (_tw.Writer.ScalarErase && lam.Origin is Lambda lamSym
                    && !(lamSym.Function.NumParameters > 0 && lamSym.Function.Parameters[0].Type.IsTypeVariable))
                {
                    var lfi = _tw.ToFunctionInfo(lamSym.Function, null, FunctionInstanceKind.Lambda);
                    _scalar = new ScalarEraseAnalysis(_tw, lfi.Function.Implementation, lfi.ParameterTypes, _pendingLambdaParamPrim);
                    _pendingLambdaParamPrim = null;
                }
                _lambdaDepth++;
                WriteNode(lam.Body);
                _lambdaDepth--;
                _scalar = savedScalar;
                _pendingLambdaParamPrim = savedPending;
            }
            else
            {
                WriteNode(fn);
                Write("(");
                Write(string.Join(", ", elems));
                Write(")");
            }
        }
        // The element prim the lambda's parameters erase to, when the receiver's origin can
        // determine it (same channel the call-site emission uses for element-wise HOFs).
        if (lam != null && _scalar != null && ll.Origin is FunctionCall originFc
            && originFc.Args.Count >= 1 && originFc.Args[0] is Expression recvExpr)
            _pendingLambdaParamPrim = _scalar.ElementPrimOf(recvExpr);

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
        _pendingLambdaParamPrim = null;
    }

    /// <summary>The cast that pins a scalar argument's type at a call site whose receiver the
    /// writer cannot prove scalar. Classically the WRAPPER (kept members take wrapper types, and
    /// broadcasts like Number -> Vector3 are wrapper-sourced). Under --methods the kept members
    /// take erased primitives, so consult the receiver type's overload surface: a same-name
    /// overload with a scalar parameter at this position wants the primitive (exact binding);
    /// otherwise the wrapper (broadcast).</summary>
    private string RestoreCastType(TirCall call, int argIndex, string argPrim)
    {
        if (!_tw.Writer.NoProperties)
            return ScalarEraseAnalysis.WrapperOfPrim(argPrim);
        // The resolved callee's declared parameter type is the strongest signal: a scalar
        // parameter erased to the primitive (exact binding), anything else wants the wrapper.
        var declared = call.ParameterTypes != null && argIndex < call.ParameterTypes.Count
            ? call.ParameterTypes[argIndex]?.Name
            : null;
        declared = declared
            ?? (call.Callee?.Parameters != null && argIndex < call.Callee.Parameters.Count
                ? call.Callee.Parameters[argIndex]?.Type?.Name
                : null);
        if (declared != null && CSharpWriter.ScalarPrimitives.ContainsKey(declared))
            return argPrim;
        // Receiver-surface fallback: a same-name overload with a scalar parameter here means
        // the primitive binds exactly.
        var recvNode = TirRewrite.StripCoerce(call.Args[0]);
        var recvType = recvNode is TirTypeRef tr ? tr.TypeDef?.Name : recvNode?.Type?.Name;
        var plan = declared == null ? _tw.Writer.GetExtensionPlanByTypeName(recvType) : null;
        if (plan != null)
            foreach (var f in plan.CandidateFunctions)
            {
                if (f.Name != call.Name || f.ParameterTypes.Count != call.Args.Count)
                    continue;
                var pt = argIndex < f.ParameterTypes.Count ? f.ParameterTypes[argIndex]?.Name : null;
                if (pt != null && CSharpWriter.ScalarPrimitives.ContainsKey(pt))
                    return argPrim;
            }
        return ScalarEraseAnalysis.WrapperOfPrim(argPrim);
    }

    /// <summary>Writes an argument that overload resolution wants pinned to a specific type
    /// <paramref name="castType"/>. When <paramref name="castType"/> is a scalar primitive AND the
    /// argument already renders as a whole cast to that SAME primitive, the outer cast is a no-op
    /// (float->float) and is dropped — collapsing the redundant `((float)(((float)x)))` double-cast
    /// to `((float)x)`. A wrapper cast (e.g. Number, a genuine float->Number conversion) is always
    /// kept.</summary>
    private void WriteScalarCastArg(string castType, TirNode a)
    {
        var rendered = Render(() => WriteNode(a));
        if (CSharpWriter.ScalarPrimitives.ContainsValue(castType) && IsWholeScalarCast(rendered, castType))
        {
            Write(rendered);
            return;
        }
        Write($"(({castType})(");
        Write(rendered);
        Write("))");
    }

    /// <summary>Renders <paramref name="write"/> into a scratch buffer and returns the text, leaving
    /// the main buffer untouched. Used to inspect how an inline expression will render before
    /// deciding whether to wrap it (the argument runs exactly once, so no side effect is doubled).</summary>
    private string Render(System.Action write)
    {
        var saved = sb;
        sb = new System.Text.StringBuilder();
        write();
        var s = sb.ToString();
        sb = saved;
        return s;
    }

    /// <summary>Whether <paramref name="s"/> is exactly one cast to <paramref name="prim"/> around a
    /// whole inner expression — `((prim)…)` whose outermost paren closes only at the final char (so
    /// `((float)x)` qualifies but `((float)x).Foo()` does not).</summary>
    private static bool IsWholeScalarCast(string s, string prim)
    {
        var head = "((" + prim + ")";
        if (s.Length <= head.Length || !s.StartsWith(head) || s[s.Length - 1] != ')')
            return false;
        var depth = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == '(') depth++;
            else if (s[i] == ')' && --depth == 0) return i == s.Length - 1;
        }
        return false;
    }

    /// <summary>Writes a call ARGUMENT. Scalar erasure only: a reference to a function-typed
    /// parameter or variable is eta-expanded into a lambda (delegate types are invariant; a
    /// lambda is target-typed and bridges the wrapper/primitive delegate types both ways).</summary>
    private void WriteCallArg(TirNode a)
    {
        if (_scalar != null)
        {
            var s = a is TirCoerce c ? c.Inner : a;
            var def = s is TirParameter tp ? (DefSymbol)tp.Def : s is TirVariable tv ? tv.Def : null;
            var typeName = (s as TirParameter)?.Def?.Type?.Name ?? (s as TirVariable)?.Def?.Type?.Name;
            if (def != null && typeName != null && typeName.StartsWith("Function")
                && int.TryParse(typeName.Substring("Function".Length), out var arity))
            {
                var ps = string.Join(", ", Enumerable.Range(0, arity).Select(i => $"_e{i}"));
                Write($"({ps}) => {def.Name}({ps})");
                return;
            }
        }
        WriteNode(a);
    }
}
