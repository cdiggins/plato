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
        if (tw.Writer.ScalarErase && fi != null)
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
    private void WriteBareName(string name)
    {
        if (_tw.ExtensionReceiverName == null)
        {
            Write(name);
            return;
        }
        if (_tw.ExtensionInstanceNames.Contains(name))
        {
            Write($"{_tw.ExtensionReceiverName}.{name}");
            // Moved no-arg members are classic extension methods, not properties. Under scalar
            // erasure the receiver of a scalar type's moved member is a primitive, so ALL its
            // no-arg members are extension methods too.
            if (_tw.Writer.MovedNoArgNames.Contains(name)
                || (_tw.ExtensionReceiverIsScalar && _tw.Writer.ScalarMemberNames.Contains(name)))
                Write("()");
            return;
        }
        if (_tw.ExtensionStaticNames.Contains(name))
        {
            Write($"{_tw.ExtensionStaticQualifier}.{name}");
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

    private static bool IsStatementNode(TirNode n)
        => n is TirBlock || n is TirReturn || n is TirIf || n is TirLoop;

    private void WriteStatement(TirNode n)
    {
        switch (n)
        {
            case TirBlock b:
                WriteStartBlock();
                foreach (var s in b.Statements)
                {
                    if (s == null) continue;
                    if (IsStatementNode(s))
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
                if (_scalar != null)
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
                // A TirCoerce is ALWAYS a solver-inserted IMPLICIT-widening conversion (the
                // Elaborator only wraps ArgMatchKind.Conversion arguments): Integer->Number,
                // Self->interface, Number->Vector3 broadcast, etc. The reference writer never
                // renders these — it writes the inner value and lets C#'s implicit conversion
                // operators do the widening at the call boundary. So we suppress the conversion
                // and emit the inner node only. Genuine SOURCE-level conversions the programmer
                // wrote (`Vector3(0.0)`, `x.Number`) arrive as a TirCall with
                // EmissionKind.Conversion and are rendered by WriteCall — those are kept as-is.
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
                // a scalar constructor call is just a cast of its single argument.
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
                if (_tw.Writer.ScalarErase && lam.Origin is Lambda lamSym)
                {
                    var lfi = _tw.ToFunctionInfo(lamSym.Function, null, FunctionInstanceKind.Lambda);
                    _scalar = new ScalarEraseAnalysis(_tw, lfi.Function.Implementation, lfi.ParameterTypes, _pendingLambdaParamPrim);
                    _pendingLambdaParamPrim = null;
                }
                // The reference constructs a fresh body writer per lambda, whose constructor
                // re-runs the capture hoist on the lambda's own body, and writes " => " only for
                // an expression body. Mirror both (a hoisted lambda body is block-shaped).
                var lamBody = TirLambdaCaptureRewriter.Rewrite(lam.Body);
                if (IsStatementNode(lamBody))
                {
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

    private static TirNode StripCoerce(TirNode n)
        => n is TirCoerce c ? StripCoerce(c.Inner) : n;

    // Scalar erasure only: the primitive a TIR NODE is known to erase to, when the origin-symbol
    // analysis cannot know it. The one case: component-access markers substituted by the
    // TIR unroller (--optimize) — their origin still shows the pre-unroll lambda parameter, so
    // the marker carries the primitive itself (mirrors the legacy ScalarComponentPrim channel).
    private string NodeScalarPrim(TirNode n)
        => StripCoerce(n) is TirComponentAccess ca ? ca.ScalarComponentPrim : null;

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

        // Indexer: At(a, i, ...) -> a[i, ...].
        if (name == "At")
        {
            WriteNode(args[0]);
            Write("[");
            WriteArgs(args.Skip(1));
            Write("]");
            return;
        }

        // Member / method-call form: receiver first, then `.Name`, then `(rest)` unless it reads
        // as a property (no-arg member access) or a type-named conversion property.
        WriteNode(args[0]);
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
                && (NodeScalarPrim(args[0]) != null
                    || (originFc != null && originFc.Args.Count >= 1
                        && originFc.Args[0] is Expression origRecv && _scalar.IsScalarValued(origRecv))))
                Write("()");
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
        if (_scalar != null && originFc != null && originFc.Function is FunctionGroupRefSymbol fgrc
            && originFc.Args.Count >= 1 && originFc.Args[0] is Expression recvExpr)
        {
            receiverPrim = NodeScalarPrim(args[0]) ?? _scalar.ScalarPrimOf(recvExpr);
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
                ? NodeScalarPrim(a) ?? (symArg != null ? _scalar.ScalarPrimOf(symArg) : null)
                : null;
            if (argPrims != null && argPrim != null
                && CSharpWriter.ScalarPrimitives.TryGetValue(argPrims[argIndex], out var castPrim))
            {
                Write($"(({castPrim})(");
                WriteNode(a);
                Write("))");
            }
            else if (argPrim != null && receiverPrim == null)
            {
                Write($"(({ScalarEraseAnalysis.WrapperOfPrim(argPrim)})(");
                WriteNode(a);
                Write("))");
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
