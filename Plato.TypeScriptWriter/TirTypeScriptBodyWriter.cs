using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.TypeScriptWriter;

/// <summary>
/// Renders a TypeScript function <em>body</em> from a <see cref="TirFunction"/> — the TS analog of
/// <c>TirCSharpBodyWriter</c>. Instead of re-deriving semantics at emit time the way
/// <see cref="TypeScriptFunctionBodyWriter"/> does (<c>HasArgList</c> + field-name lookups), it maps
/// the recorded <see cref="EmissionKind"/> of every <see cref="TirCall"/> to syntax. The framing
/// (" { return … ; }" expression bodies, native literals, always-parenthesized calls except field
/// reads) is reproduced from the legacy writer verbatim, asserted byte-for-byte by
/// <c>TypeScriptEmitFlagOnTests</c>.
/// </summary>
public class TirTypeScriptBodyWriter : CodeBuilder<TirTypeScriptBodyWriter>
{
    private readonly TypeScriptTypeWriter _tw;
    private readonly TirFunction _tir;
    private readonly string _selfType;

    // Static mode: parameter #0 is emitted by name instead of `this`.
    private readonly bool _isStatic;

    // >0 while rendering a lambda body: parameter #0 is emitted by name, not `this`.
    private int _lambdaDepth;

    public TirTypeScriptBodyWriter(TypeScriptTypeWriter tw, TirFunction tir, bool isStatic)
    {
        IndentLevel = tw.IndentLevel;
        _tw = tw;
        _tir = tir;
        _selfType = tw.SelfType;
        _isStatic = isStatic;
        WriteFunctionBody();
    }

    private string TypeName(TypeExpression te)
        => _tw.ToTypeScriptTypeName(TypeInstance.Create(te));

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

    // --- top level: mirror TypeScriptFunctionBodyWriter's framing exactly ----------

    private void WriteFunctionBody()
    {
        // Body-less functions never route here (the throw-stub line has no heuristics).
        var body = TirLambdaCaptureRewriter.Rewrite(_tir.Body);
        if (body is TirBlock)
        {
            WriteLine();
            WriteStatement(body);
        }
        else if (IsStatementNode(body))
        {
            WriteLine();
            WriteStatement(body);
        }
        else
        {
            Write(" { return ");
            WriteNode(body);
            WriteLine("; }");
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
                Write("let ");
                Write(TypeScriptTypeWriter.EscapeName(let.Def?.Name ?? "?"));
                Write(" = ");
                WriteNode(let.Value);
                WriteLine(";");
                return;

            case TirIf iff:
                Write("if (");
                WriteCondition(iff.Condition);
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
                WriteCondition(l.Condition);
                WriteLine(")");
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

    // The legacy writer wraps every condition in its own parentheses.
    private void WriteCondition(TirNode n)
    {
        Write("(");
        WriteNode(n);
        Write(")");
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
                WriteLiteral(lit);
                return;

            case TirParameter p:
            {
                var idx = p.Def?.Index ?? -1;
                Write(idx == 0 && !_isStatic && _lambdaDepth == 0
                    ? "this"
                    : TypeScriptTypeWriter.EscapeName(p.Def?.Name ?? "?"));
                return;
            }

            case TirVariable v:
                Write(TypeScriptTypeWriter.EscapeName(v.Def?.Name ?? "?"));
                return;

            case TirTypeRef t:
                // Raw TypeExpression references map through the TS type-name table
                // (native primitives, IArray renames); bound references emit the (escaped)
                // name, with Self resolved to the enclosing type — the legacy writer's two paths.
                if (t.NamespaceQualified && t.TypeDef?.Name != "Self" && t.Type != null)
                    Write(TypeName(t.Type));
                else
                    Write(t.TypeDef?.Name == "Self"
                        ? _selfType
                        : TypeScriptTypeWriter.EscapeName(t.TypeDef?.Name ?? "?"));
                return;

            case TirLet let:
                Write($"let {TypeScriptTypeWriter.EscapeName(let.Def?.Name ?? "?")} = ");
                WriteNode(let.Value);
                return;

            case TirDefault:
                Write("(undefined as any)");
                return;

            case TirName nm:
                Write(nm.Name);
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
                // Implicit solver-inserted widenings are invisible in the legacy TS output too.
                WriteNode(c.Inner);
                return;

            case TirInvoke inv:
                WriteNode(inv.Target);
                Write("(");
                WriteArgs(inv.Args);
                Write(")");
                return;

            case TirConditional cond:
                WriteCondition(cond.Condition);
                Write(" ? ");
                WriteNode(cond.IfTrue);
                Write(" : ");
                WriteNode(cond.IfFalse);
                return;

            case TirNew nw:
            {
                var typeName = TypeName(nw.NewType);
                // "new Number(x)" on a native type is just the value itself.
                if (TypeScriptWriter.NativeDefaults.ContainsKey(typeName))
                {
                    Write("(");
                    WriteArgs(nw.Args);
                    Write(")");
                    return;
                }
                Write($"new {typeName}(");
                WriteArgs(nw.Args);
                Write(")");
                return;
            }

            case TirArray arr:
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
                if (TryGetEtaForwardedName(lam, out var etaName))
                {
                    Write(etaName);
                    return;
                }
                Write("(");
                Write(string.Join(", ", lam.Parameters.Select(p => TypeScriptTypeWriter.EscapeName(p.Name))));
                Write(") => ");
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

    private void WriteLiteral(TirLiteral lit)
        // Numbers, integers, booleans, and strings are all native TS literals.
        => Write(lit.Value.ToLiteralString());

    private void WriteCall(TirCall call)
    {
        var name = call.Name;
        var args = call.Args;

        // Zero-argument call: a Constants reference (the `default` keyword arrives as TirDefault).
        if (args.Count == 0)
        {
            Write($"Constants.{name}");
            return;
        }

        // Tuple constructor: TupleN(a, b, ...) -> new TupleN(a, b, ...).
        if (name != null && name.StartsWith("Tuple"))
        {
            Write($"new {name}(");
            WriteArgs(args);
            Write(")");
            return;
        }

        // Some receivers must be parenthesized: "1.Subtract" is a syntax error, and a ternary or
        // lambda receiver would otherwise bind the member access to its last operand only.
        var receiver = StripCoerce(args[0]);
        if (receiver is TirLiteral || receiver is TirConditional || receiver is TirLambda || receiver is TirAssign)
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

        // Field access stays a property read (fields are the only properties);
        // every actual function call is parenthesized, including zero-argument ones.
        // A type-named field read classifies as a Conversion (rotation.Quaternion), so both
        // kinds count — the field-name guard is what decides, exactly as the legacy writer.
        if (args.Count == 1
            && (call.EmissionKind == EmissionKind.Property || call.EmissionKind == EmissionKind.Conversion)
            && _tw.Writer.AllFieldNames.Contains(name))
            return;

        Write("(");
        WriteArgs(args.Skip(1));
        Write(")");
    }
}
