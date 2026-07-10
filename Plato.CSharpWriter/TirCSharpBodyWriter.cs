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
        var isBlock = _tir.Body is TirBlock;

        if (isProp)
            Write($" {{ {CSharpTypeWriter.Annotation} get ");

        if (!isBlock)
        {
            Write(" => ");
            WriteNode(_tir.Body);
            Write(";");
        }
        else
        {
            WriteStatement(_tir.Body);
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
                Write(t.TypeDef?.Name == "Self" ? _selfType : t.TypeDef?.Name ?? "?");
                return;

            case TirDefault:
                Write("default");
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
                // The whole point of the phase: an implicit conversion is an explicit node. The
                // default writer emits a broadcast / cast as a type-named member access on the
                // inner value (Vector3(0.0) -> ((Number)0).Vector3; Number->Angle -> x.Angle).
                WriteNode(c.Inner);
                Write(".");
                Write(c.ToType?.Name ?? "?");
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
                Write("(");
                Write(string.Join(", ", lam.Parameters.Select(p => p.Name)));
                Write(") ");
                _lambdaDepth++;
                Write(" => ");
                if (lam.Body is TirBlock)
                    WriteStatement(lam.Body);
                else
                    WriteNode(lam.Body);
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
