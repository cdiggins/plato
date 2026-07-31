using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.RustWriter;

/// <summary>
/// Renders a Rust function <em>body</em> from a <see cref="TirFunction"/> — the Rust analog of
/// <c>TirCSharpBodyWriter</c> / <c>TirTypeScriptBodyWriter</c>. Maps the recorded
/// <see cref="EmissionKind"/> of every <see cref="TirCall"/> to syntax instead of re-deriving it
/// from the symbol graph. Framing (" { expr }" bodies, ForceDecimal number literals, i64 receiver
/// suffixes, Constants::X() calls) is reproduced from <see cref="RustFunctionBodyWriter"/>
/// verbatim, asserted byte-for-byte by <c>RustEmitFlagOnTests</c>.
/// </summary>
public class TirRustBodyWriter : CodeBuilder<TirRustBodyWriter>
{
    private readonly RustTypeWriter _tw;
    private readonly TirFunction _tir;
    private readonly string _selfType;
    private readonly bool _isStatic;
    private int _lambdaDepth;

    public TirRustBodyWriter(RustTypeWriter tw, TirFunction tir, bool isStatic)
    {
        IndentLevel = tw.IndentLevel;
        _tw = tw;
        _tir = tir;
        _selfType = tw.SelfType;
        _isStatic = isStatic;
        WriteFunctionBody();
    }

    private string TypeName(TypeExpression te)
        => _tw.ToRustTypeName(TypeInstance.Create(te));

    // Recognizes the normalizer's eta-expansion of a bare member-group reference in value
    // position (see TirCSharpBodyWriter.TryGetEtaForwardedName — same shape, same guard).
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

    // --- top level: mirror RustFunctionBodyWriter's framing exactly ----------

    private void WriteFunctionBody()
    {
        var body = TirLambdaCaptureRewriter.Rewrite(_tir.Body);
        if (IsStatementNode(body))
        {
            WriteLine();
            WriteStatement(body);
        }
        else
        {
            Write(" { ");
            WriteNode(body);
            WriteLine(" }");
        }
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
                Write("let mut ");
                Write(RustTypeWriter.EscapeName(let.Def?.Name ?? "?"));
                Write(" = ");
                WriteNode(let.Value);
                WriteLine(";");
                return;

            case TirIf iff:
                Write("if ");
                WriteNode(iff.Condition);
                WriteLine();
                WriteStatement(iff.IfTrue);
                if (iff.IfFalse != null)
                {
                    WriteLine("else");
                    WriteStatement(iff.IfFalse);
                }
                return;

            case TirLoop l:
                Write("while ");
                WriteNode(l.Condition);
                WriteLine();
                WriteStartBlock();
                WriteStatement(l.Body);
                WriteEndBlock();
                WriteLine();
                return;

            default:
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

    private static TirNode StripCoerce(TirNode n)
        => n is TirCoerce c ? StripCoerce(c.Inner) : n;

    private void WriteNode(TirNode n)
    {
        switch (n)
        {
            case null:
                return;

            case TirLiteral lit:
                WriteLiteral(lit, false);
                return;

            case TirParameter p:
            {
                var idx = p.Def?.Index ?? -1;
                Write(idx == 0 && !_isStatic && _lambdaDepth == 0
                    ? "self"
                    : RustTypeWriter.EscapeName(p.Def?.Name ?? "?"));
                return;
            }

            case TirVariable v:
                Write(RustTypeWriter.EscapeName(v.Def?.Name ?? "?"));
                return;

            case TirTypeRef t:
                if (t.NamespaceQualified && t.TypeDef?.Name != "Self" && t.Type != null)
                    Write(TypeName(t.Type));
                else
                    Write(t.TypeDef?.Name == "Self"
                        ? _selfType
                        : RustTypeWriter.EscapeName(t.TypeDef?.Name ?? "?"));
                return;

            case TirLet let:
                Write($"let mut {RustTypeWriter.EscapeName(let.Def?.Name ?? "?")} = ");
                WriteNode(let.Value);
                return;

            case TirDefault:
                Write("Default::default()");
                return;

            case TirName nm:
                Write(nm.Name);
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
                // Implicit solver-inserted widenings are invisible in the legacy Rust output too.
                WriteNode(c.Inner);
                return;

            case TirInvoke inv:
                WriteNode(inv.Target);
                Write("(");
                WriteArgs(inv.Args);
                Write(")");
                return;

            case TirConditional cond:
                Write("if ");
                WriteNode(cond.Condition);
                Write(" { ");
                WriteNode(cond.IfTrue);
                Write(" } else { ");
                WriteNode(cond.IfFalse);
                Write(" }");
                return;

            case TirNew nw:
            {
                var typeName = TypeName(nw.NewType);
                // "new Number(x)" on a native type is just the value itself.
                if (RustWriter.NativeDefaults.ContainsKey(typeName))
                {
                    Write("(");
                    WriteArgs(nw.Args);
                    Write(")");
                    return;
                }
                Write($"{typeName}::new(");
                WriteArgs(nw.Args);
                Write(")");
                return;
            }

            case TirArray arr:
                Write("Intrinsics::MakeArray(vec![");
                WriteArgs(arr.Elements);
                Write("])");
                return;

            case TirAssign asg:
                WriteNode(asg.LValue);
                Write(" = ");
                WriteNode(asg.RValue);
                return;

            case TirLambda lam:
                if (TryGetEtaForwardedName(lam, out var etaName))
                {
                    Write(etaName);
                    return;
                }
                Write("|");
                Write(string.Join(", ", lam.Parameters.Select(p => RustTypeWriter.EscapeName(p.Name))));
                Write("| ");
                _lambdaDepth++;
                var lamBody = TirLambdaCaptureRewriter.Rewrite(lam.Body);
                if (IsStatementNode(lamBody))
                    WriteStatement(lamBody);
                else
                    WriteNode(lamBody);
                _lambdaDepth--;
                return;

            case TirUnresolved u:
                Write($"/*unresolved:{u.Original?.Name}*/");
                return;

            default:
                Write("/*?*/");
                return;
        }
    }

    private void WriteLiteral(TirLiteral lit, bool asReceiver)
    {
        var value = lit.Value.ToLiteralString();
        switch (lit.LiteralType)
        {
            case LiteralTypesEnum.Number:
                Write(RustFunctionBodyWriter.ForceDecimal(value));
                return;
            case LiteralTypesEnum.Integer:
                // As a receiver, the literal needs an explicit i64 type so that
                // method resolution finds the IntegerExt trait.
                Write(asReceiver ? $"{value}i64" : value);
                return;
            case LiteralTypesEnum.String:
                Write($"{value}.to_string()");
                return;
            default:
                // Booleans are native literals.
                Write(value);
                return;
        }
    }

    private void WriteCall(TirCall call)
    {
        var name = call.Name;
        var args = call.Args;

        // Zero-argument call: constants are zero-argument functions in Rust.
        if (args.Count == 0)
        {
            Write($"Constants::{name}()");
            return;
        }

        // Tuple constructor: TupleN(a, b, ...) -> TupleN::new(a, b, ...).
        if (name != null && name.StartsWith("Tuple"))
        {
            Write($"{name}::new(");
            WriteArgs(args);
            Write(")");
            return;
        }

        var receiver = StripCoerce(args[0]);

        // A type as the receiver is a static (associated function) call: Type::F(x).
        if (receiver is TirTypeRef ttr && ttr.NamespaceQualified && ttr.Type != null)
        {
            Write(TypeName(ttr.Type));
            Write($"::{name}(");
            WriteArgs(args.Skip(1));
            Write(")");
            return;
        }

        // Some receivers must be parenthesized: "1.Subtract" would not parse, and integer
        // literals additionally need an i64 suffix so method lookup finds the IntegerExt trait.
        if (receiver is TirLiteral rl)
        {
            Write("(");
            WriteLiteral(rl, true);
            Write(")");
        }
        else if (receiver is TirConditional || receiver is TirLambda || receiver is TirAssign)
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

        // Field access stays a field read (fields are public struct fields); every actual
        // function call is parenthesized. A type-named field read classifies as a Conversion,
        // so both kinds count — the field-name guard decides, exactly as the legacy writer.
        if (args.Count == 1
            && (call.EmissionKind == EmissionKind.Property || call.EmissionKind == EmissionKind.Conversion)
            && _tw.Writer.AllFieldNames.Contains(name))
            return;

        Write("(");
        WriteArgs(args.Skip(1));
        Write(")");
    }
}
