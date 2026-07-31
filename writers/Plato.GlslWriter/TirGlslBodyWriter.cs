using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.GlslWriter;

/// <summary>
/// Renders a GLSL function <em>body</em> from a <see cref="TirFunction"/> — the GLSL analog of
/// <c>TirRustBodyWriter</c>. Every call becomes a free-function call (GLSL has no methods);
/// operator-named calls on native scalars/vectors are inlined to native operators; anything
/// GLSL cannot express (lambdas, function values, arrays, strings) throws
/// <see cref="GlslUnsupportedException"/>, which makes the caller skip the whole function.
/// </summary>
public class TirGlslBodyWriter : CodeBuilder<TirGlslBodyWriter>
{
    private readonly GlslWriter _w;
    private readonly TirFunction _tir;

    public TirGlslBodyWriter(GlslWriter w, TirFunction tir)
    {
        IndentLevel = w.IndentLevel;
        _w = w;
        _tir = tir;
        WriteFunctionBody();
    }

    private void WriteFunctionBody()
    {
        var body = _tir.Body;
        if (IsStatementNode(body))
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

            case TirIf iff:
                Write("if (");
                WriteNode(iff.Condition);
                WriteLine(")");
                WriteStatement(Blockify(iff.IfTrue));
                if (iff.IfFalse != null)
                {
                    WriteLine("else");
                    WriteStatement(Blockify(iff.IfFalse));
                }
                return;

            case TirLoop l:
                Write("while (");
                WriteNode(l.Condition);
                WriteLine(")");
                WriteStatement(Blockify(l.Body));
                return;

            default:
                WriteNode(n);
                WriteLine(";");
                return;
        }
    }

    // GLSL if/while bodies are single statements; wrap in a block for safety.
    private static TirNode Blockify(TirNode n)
        => n is TirBlock ? n : new TirBlock(new[] { n }, n?.Origin);

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
                Write(GlslWriter.EscapeName(p.Def?.Name ?? "?"));
                return;

            case TirVariable v:
                Write(GlslWriter.EscapeName(v.Def?.Name ?? "?"));
                return;

            case TirTypeRef t:
                Write(_w.GlslTypeNameOrNull(t.Type) ?? t.TypeDef?.Name ?? "?");
                return;

            case TirLet let:
            {
                var type = _w.GlslTypeName(let.Value?.Type ?? let.Type);
                Write($"{type} {GlslWriter.EscapeName(let.Def?.Name ?? "?")} = ");
                WriteNode(let.Value);
                return;
            }

            case TirDefault d:
                WriteDefault(d);
                return;

            case TirName nm:
                Write(nm.Name);
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
            {
                // Explicit conversion needed: GLSL ES 3.0 has NO implicit int->float.
                var from = _w.GlslTypeNameOrNull(c.FromType);
                var to = _w.GlslTypeNameOrNull(c.ToType);
                if (from != null && to != null && from != to && GlslWriter.IsScalar(to))
                {
                    Write(to);
                    Write("(");
                    WriteNode(c.Inner);
                    Write(")");
                }
                else
                {
                    WriteNode(c.Inner);
                }
                return;
            }

            case TirConditional cond:
                Write("(");
                WriteNode(cond.Condition);
                Write(" ? ");
                WriteNode(cond.IfTrue);
                Write(" : ");
                WriteNode(cond.IfFalse);
                Write(")");
                return;

            case TirNew nw:
            {
                var typeName = _w.GlslTypeName(nw.NewType);
                if (GlslWriter.IsScalar(typeName))
                {
                    // "new Number(x)" on a native scalar is just the value.
                    Write("(");
                    WriteArgs(nw.Args);
                    Write(")");
                    return;
                }
                Write(typeName);
                Write("(");
                WriteArgs(nw.Args);
                Write(")");
                return;
            }

            case TirAssign asg:
                WriteNode(asg.LValue);
                Write(" = ");
                WriteNode(asg.RValue);
                return;

            case TirLambda _:
                throw new GlslUnsupportedException("lambdas / function values (GLSL has no function pointers)");

            case TirInvoke _:
                throw new GlslUnsupportedException("invoking a function value (GLSL has no function pointers)");

            case TirArray arr:
            {
                // A statically sized array literal maps to a GLSL sized-array constructor
                // (T[N](e0, e1, ...)). This is the ONLY array GLSL can build: the length is
                // fixed at compile time. Dynamically sized arrays remain unrepresentable.
                if (arr.Elements.Count == 0)
                    throw new GlslUnsupportedException("empty array literal (GLSL arrays need >= 1 element and a known size)");
                var elem = _w.GlslTypeNameOrNull(arr.Elements[0].Type)
                           ?? throw new GlslUnsupportedException("array element type not representable in GLSL");
                Write($"{elem}[{arr.Elements.Count}](");
                WriteArgs(arr.Elements);
                Write(")");
                return;
            }

            case TirUnresolved u:
                throw new GlslUnsupportedException($"unresolved call '{u.Original?.Name}'");

            default:
                throw new GlslUnsupportedException($"TIR node {n.GetType().Name}");
        }
    }

    private void WriteLiteral(TirLiteral lit)
    {
        var value = lit.Value.ToLiteralString();
        switch (lit.LiteralType)
        {
            case LiteralTypesEnum.Number:
                Write(ForceDecimal(value));
                return;
            case LiteralTypesEnum.Integer:
                Write(value);
                return;
            case LiteralTypesEnum.String:
                throw new GlslUnsupportedException("string literals (GLSL has no string type)");
            default:
                // Booleans are native literals.
                Write(value);
                return;
        }
    }

    /// <summary>GLSL ES has no implicit int-to-float: force a decimal point on float literals.</summary>
    public static string ForceDecimal(string s)
        => s.Contains(".") || s.Contains("e") || s.Contains("E") ? s : s + ".0";

    private void WriteDefault(TirDefault d)
    {
        var type = _w.GlslTypeName(d.Type);
        switch (type)
        {
            case "float": Write("0.0"); return;
            case "int": Write("0"); return;
            case "bool": Write("false"); return;
            case "vec2": Write("vec2(0.0)"); return;
            case "vec3": Write("vec3(0.0)"); return;
            case "vec4": Write("vec4(0.0)"); return;
            default:
                throw new GlslUnsupportedException($"default({type}) (GLSL structs have no default constructor)");
        }
    }

    private void WriteCall(TirCall call)
    {
        var name = call.Name;
        if (name == null)
            throw new GlslUnsupportedException("unnamed call");
        var args = call.Args;

        // Zero-argument call: constants are zero-argument functions.
        if (args.Count == 0)
        {
            Write($"{GlslWriter.EscapeName(name)}()");
            return;
        }

        var receiver = StripCoerce(args[0]);

        // Field access stays a field read; X/Y/Z/W map to swizzles on native vectors.
        if (args.Count == 1
            && (call.EmissionKind == EmissionKind.Property || call.EmissionKind == EmissionKind.Conversion)
            && _w.AllFieldNames.Contains(name))
        {
            if (receiver is TirLiteral || receiver is TirConditional || receiver is TirAssign)
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
            Write(_w.MapFieldName(GlslWriter.PlatoTypeName(args[0].Type), name));
            return;
        }

        // Operator-named calls on native scalars/vectors inline to native operators.
        if (TryWriteOperator(call))
            return;

        // A type as the receiver is a static call: Type::F(x) -> F(x).
        if (receiver is TirTypeRef ttr && ttr.NamespaceQualified && ttr.Type != null)
        {
            Write($"{GlslWriter.EscapeName(name)}(");
            WriteArgs(args.Skip(1));
            Write(")");
            return;
        }

        // A call named after a natively mapped type is a constructor/conversion:
        // Vector3(a, b, c) -> vec3(a, b, c), and the single-argument broadcast
        // Vector3(0.0) -> vec3(0.0) (GLSL fills every component). Covers Conversion,
        // Constructor and static-factory emission kinds alike.
        if (GlslWriter.NativeVectors.TryGetValue(name, out var vec))
        {
            Write($"{vec}(");
            WriteArgs(args);
            Write(")");
            return;
        }
        if (GlslWriter.NativePrimitives.TryGetValue(name, out var prim) && args.Count == 1)
        {
            Write($"{prim}(");
            WriteArgs(args);
            Write(")");
            return;
        }

        // Everything else: a free-function call.
        Write($"{GlslWriter.EscapeName(name)}(");
        WriteArgs(args);
        Write(")");
    }

    private bool TryWriteOperator(TirCall call)
    {
        var args = call.Args;
        var op = args.Count == 1 ? Operators.NameToUnaryOperator(call.Name)
            : args.Count == 2 ? Operators.NameToBinaryOperator(call.Name) : null;
        if (op == null)
            return false;

        var types = args.Select(a => _w.GlslTypeNameOrNull(a.Type)).ToList();
        if (types.Any(t => t == null))
            return false;

        var allScalarNumeric = types.All(t => t == "float" || t == "int");
        var allBool = types.All(t => t == "bool");
        var vectorish = types.All(t => GlslWriter.IsVector(t) || t == "float" || t == "int");
        var sameType = types.Distinct().Count() == 1;

        bool ok;
        switch (op)
        {
            case "+": case "-": case "*": case "/":
                ok = allScalarNumeric || vectorish;
                break;
            case "%":
                ok = allScalarNumeric;
                break;
            case "<": case ">": case "<=": case ">=":
                ok = allScalarNumeric;
                break;
            case "==": case "!=":
                // GLSL defines == / != for scalars, vectors and structs alike.
                ok = sameType;
                break;
            case "&&": case "||":
                ok = allBool;
                break;
            case "!":
                ok = allBool;
                break;
            default:
                ok = false; // bitwise ops: rarely used, keep the POC simple
                break;
        }
        if (!ok)
            return false;

        if (args.Count == 1)
        {
            Write($"({op}");
            WriteNode(args[0]);
            Write(")");
            return true;
        }

        // GLSL % only works on integers; float modulo is the mod() builtin.
        if (op == "%" && types.Any(t => t == "float"))
        {
            Write("mod(");
            WriteNode(args[0]);
            Write(", ");
            WriteNode(args[1]);
            Write(")");
            return true;
        }

        Write("(");
        WriteNode(args[0]);
        Write($" {op} ");
        WriteNode(args[1]);
        Write(")");
        return true;
    }
}
