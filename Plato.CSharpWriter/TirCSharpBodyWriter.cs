using System.Collections.Generic;
using System.Linq;
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

    // >0 while rendering a lambda body: parameter #0 is emitted by name, not `this`
    // (the reference lambda body writer runs in "static" mode, isStatic:true).
    private int _lambdaDepth;

    public TirCSharpBodyWriter(CSharpTypeWriter tw, TirFunction tir)
    {
        IndentLevel = tw.IndentLevel;
        _tw = tw;
        _tir = tir;
        _selfType = tw.SelfType;
        WriteFunctionBody();
    }

    private bool IsTypeName(string name)
        => name != null && _tw.Writer.AllTypeNames.Contains(name);

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

        // Member instance path (WriteMemberFunction always calls WriteBody(fi, isStatic:false)):
        // a single-parameter member is emitted as a property getter.
        var numParams = _tir.Original?.NumParameters ?? _tir.Parameters.Count;
        var isProp = numParams == 1;

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
                Write($"(({lit.LiteralType}){lit.Value.ToLiteralString()})");
                return;

            case TirParameter p:
            {
                var idx = p.Def?.Index ?? -1;
                Write(idx == 0 && _lambdaDepth == 0 ? "this" : p.Def?.Name ?? "?");
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
                // emits the bare name and lets C# member lookup bind it.
                Write(nm.Name);
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
                    Write(etaName);
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
                if (IsStatementNode(lamBody))
                {
                    WriteStatement(lamBody);
                }
                else
                {
                    Write(" => ");
                    WriteNode(lamBody);
                }
                _lambdaDepth--;
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

    private void WriteCall(TirCall call)
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

        var noParens = args.Count == 1
            && (call.EmissionKind == EmissionKind.Property
                || (call.EmissionKind == EmissionKind.Conversion && IsTypeName(name)));
        if (noParens)
            return;

        Write("(");
        WriteArgs(args.Skip(1));
        Write(")");
    }
}
