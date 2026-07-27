using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.AST;
using Ara3D.Geometry.Compiler.Analysis;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Utils;

namespace Ara3D.Geometry.CppWriter;

/// <summary>
/// Renders a C++ / CUDA function <em>body</em> from a <see cref="TirFunction"/>. Every call
/// becomes a free-function call (there are no methods in the output); operator-named calls on
/// native scalars and vectors are inlined to native operators; anything the target cannot
/// express (lambdas, function values, dynamic arrays, strings) throws
/// <see cref="CppUnsupportedException"/>, which makes the caller skip the whole function.
///
/// The text produced here is dialect independent — see <see cref="CppPrelude"/>.
/// </summary>
public class TirCppBodyWriter : CodeBuilder<TirCppBodyWriter>
{
    private readonly CppWriter _w;
    private readonly TirFunction _tir;

    /// <summary>
    /// The Plato-level free functions this body calls, with their argument types. The writer
    /// uses them to drop any function whose callees were themselves skipped (which would not
    /// compile). Calls onto the preamble — operators, plato:: helpers, struct construction —
    /// are not recorded: they always exist.
    /// </summary>
    public List<CppWriter.CallSite> Callees { get; } = new List<CppWriter.CallSite>();

    private void RecordCall(string name, IEnumerable<TirNode> args)
        => Callees.Add(new CppWriter.CallSite
        {
            Name = name,
            Types = args.Select(a => _w.CppTypeNameOrNull(a.Type)).ToList(),
        });

    // The return type as it appears in the emitted signature: the body has to produce exactly
    // this type, so returns of a different type need an explicit conversion.
    private readonly string _retCpp;
    private readonly string _retPlato;

    public TirCppBodyWriter(CppWriter w, TirFunction tir, string retCpp = null, string retPlato = null)
    {
        IndentLevel = w.IndentLevel;
        _w = w;
        _tir = tir;
        _retCpp = retCpp;
        _retPlato = retPlato;
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
            WriteReturnValue(body);
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
                WriteReturnValue(r.Value);
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

    // if/while bodies are single statements; wrap in a block for safety.
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

    /// <summary>
    /// Writes <paramref name="inner"/> as a value of <paramref name="toType"/>. C++ has no
    /// implicit user-defined conversions, so a change of type — whether the checker marked it
    /// with a TirCoerce or it only shows up as a return value whose type differs from the
    /// declared one — becomes an explicit cast (scalars) or a call to the library's own
    /// conversion function (Point3D -> Vector3D(p)). If that function was not emitted, the
    /// enclosing function is pruned rather than miscompiled.
    /// </summary>
    private void WriteConverted(TirNode inner, TypeExpression toType)
        => WriteConvertedTo(inner, _w.CppTypeNameOrNull(toType), CppWriter.PlatoTypeName(toType));

    private void WriteConvertedTo(TirNode inner, string to, string toPlato)
    {
        var from = _w.CppTypeNameOrNull(inner?.Type);
        if (inner == null || from == null || to == null || from == to)
        {
            WriteNode(inner);
            return;
        }
        if (CppWriter.IsScalar(to))
        {
            Write(to);
            Write("(");
            WriteNode(inner);
            Write(")");
            return;
        }
        RecordCall(toPlato, new[] { inner });
        Write($"{_w.FunctionName(toPlato)}(");
        WriteNode(inner);
        Write(")");
    }

    /// <summary>Writes a returned value, converted to the signature's return type.</summary>
    private void WriteReturnValue(TirNode value)
    {
        if (_retCpp == null)
            WriteConverted(value, _tir.ZonkedReturnType);
        else
            WriteConvertedTo(value, _retCpp, _retPlato);
    }

    /// <summary>Writes a constructor call for a type: braced init for structs, make_floatN for vectors.</summary>
    private void WriteConstruction(string cppType, IReadOnlyList<TirNode> args)
    {
        if (CppWriter.IsVector(cppType))
        {
            // A one-argument vector construction is the broadcast form (every component).
            var arity = cppType == "float2" ? 2 : cppType == "float3" ? 3 : 4;
            Write($"make_{cppType}(");
            if (args.Count == 1)
            {
                for (var i = 0; i < arity; i++)
                {
                    if (i > 0) Write(", ");
                    WriteNode(args[0]);
                }
            }
            else
            {
                WriteArgs(args);
            }
            Write(")");
            return;
        }

        if (CppWriter.IsScalar(cppType))
        {
            // "new Number(x)" on a native scalar is a cast.
            Write($"{cppType}(");
            WriteArgs(args);
            Write(")");
            return;
        }

        Write($"{cppType}{{ ");
        WriteArgs(args);
        Write(" }");
    }

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
                Write(CppWriter.EscapeName(p.Def?.Name ?? "?"));
                return;

            case TirVariable v:
                Write(CppWriter.EscapeName(v.Def?.Name ?? "?"));
                return;

            case TirTypeRef t:
                Write(_w.CppTypeNameOrNull(t.Type) ?? t.TypeDef?.Name ?? "?");
                return;

            case TirLet let:
            {
                var type = _w.CppTypeName(let.Value?.Type ?? let.Type);
                Write($"{type} {CppWriter.EscapeName(let.Def?.Name ?? "?")} = ");
                WriteNode(let.Value);
                return;
            }

            case TirDefault d:
                WriteDefault(d);
                return;

            case TirName nm:
                // Constants emit as zero-argument functions, so a bare reference must call one.
                // Any other bare name would emit as an undeclared identifier.
                if (!_w.ConstantNames.Contains(nm.Name))
                    throw new CppUnsupportedException($"bare name '{nm.Name}' (not an emitted constant)");
                Callees.Add(new CppWriter.CallSite { Name = nm.Name, Types = new List<string>() });
                Write($"{_w.FunctionName(nm.Name)}()");
                return;

            case TirCall call:
                WriteCall(call);
                return;

            case TirCoerce c:
                WriteConverted(c.Inner, c.ToType);
                return;

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
                WriteConstruction(_w.CppTypeName(nw.NewType), nw.Args);
                return;

            case TirAssign asg:
                WriteNode(asg.LValue);
                Write(" = ");
                WriteNode(asg.RValue);
                return;

            case TirLambda _:
                throw new CppUnsupportedException("lambdas / function values (POC: not lowered)");

            case TirInvoke _:
                throw new CppUnsupportedException("invoking a function value (POC: not lowered)");

            case TirArray _:
                throw new CppUnsupportedException("array literals (POC: no array value type)");

            case TirUnresolved u:
                throw new CppUnsupportedException($"unresolved call '{u.Original?.Name}'");

            default:
                throw new CppUnsupportedException($"TIR node {n.GetType().Name}");
        }
    }

    private void WriteLiteral(TirLiteral lit)
    {
        var value = lit.Value.ToLiteralString();
        switch (lit.LiteralType)
        {
            case LiteralTypesEnum.Number:
                Write(FloatLiteral(value));
                return;
            case LiteralTypesEnum.Integer:
                Write(value);
                return;
            case LiteralTypesEnum.String:
                throw new CppUnsupportedException("string literals (POC: no string type)");
            default:
                // Booleans are native literals.
                Write(value);
                return;
        }
    }

    /// <summary>
    /// Numbers are float32 in both dialects: the 'f' suffix keeps the literal single
    /// precision instead of forcing a double round trip (and silences MSVC C4305).
    /// </summary>
    public static string FloatLiteral(string s)
    {
        if (s.EndsWith("f") || s.EndsWith("F"))
            return s;
        if (!s.Contains(".") && !s.Contains("e") && !s.Contains("E"))
            s += ".0";
        return s + "f";
    }

    private void WriteDefault(TirDefault d)
    {
        var type = _w.CppTypeName(d.Type);
        switch (type)
        {
            case "float": Write("0.0f"); return;
            case "int": Write("0"); return;
            case "bool": Write("false"); return;
            case "float2": Write("make_float2(0.0f, 0.0f)"); return;
            case "float3": Write("make_float3(0.0f, 0.0f, 0.0f)"); return;
            case "float4": Write("make_float4(0.0f, 0.0f, 0.0f, 0.0f)"); return;
            default:
                // Aggregates value-initialize: every field is zeroed.
                Write($"{type}{{}}");
                return;
        }
    }

    private void WriteCall(TirCall call)
    {
        var name = call.Name;
        if (name == null)
            throw new CppUnsupportedException("unnamed call");
        var args = call.Args;

        // Zero-argument call: constants are zero-argument functions.
        if (args.Count == 0)
        {
            RecordCall(name, args);
            Write($"{_w.FunctionName(name)}()");
            return;
        }

        var receiver = StripCoerce(args[0]);

        // Field access stays a field read; X/Y/Z/W map to .x/.y/.z/.w on native vectors.
        if (args.Count == 1
            && (call.EmissionKind == EmissionKind.Property || call.EmissionKind == EmissionKind.Conversion)
            && _w.IsFieldOf(CppWriter.PlatoTypeName(args[0].Type), name))
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
            Write(_w.MapFieldName(CppWriter.PlatoTypeName(args[0].Type), name));
            return;
        }

        // Operator-named calls on native scalars/vectors inline to native operators.
        if (TryWriteOperator(call))
            return;

        // A type as the receiver is a static call: Type::F(x) -> F(x).
        if (receiver is TirTypeRef ttr && ttr.NamespaceQualified && ttr.Type != null)
        {
            RecordCall(name, args.Skip(1));
            Write($"{_w.FunctionName(name)}(");
            WriteArgs(args.Skip(1));
            Write(")");
            return;
        }

        // A call named after a type is a construction only when its arguments are components
        // (scalars). A type-named call taking some OTHER type is a conversion function —
        // Vector3D(Point3D), Matrix4x4(Transform3D) — and stays a call.
        var allArgsScalar = args.All(a => CppWriter.IsScalar(_w.CppTypeNameOrNull(a.Type) ?? ""));
        if (CppWriter.NativeVectors.TryGetValue(name, out var vec) && allArgsScalar)
        {
            WriteConstruction(vec, args);
            return;
        }
        if (CppWriter.NativePrimitives.TryGetValue(name, out var prim) && args.Count == 1 && allArgsScalar)
        {
            WriteConstruction(prim, args);
            return;
        }
        if (_w.StructNames.Contains(name) && call.EmissionKind == EmissionKind.Constructor)
        {
            WriteConstruction(name, args);
            return;
        }

        // Everything else: a free-function call.
        RecordCall(name, args);
        Write($"{_w.FunctionName(name)}(");
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

        var types = args.Select(a => _w.CppTypeNameOrNull(a.Type)).ToList();
        if (types.Any(t => t == null))
            return false;

        var allScalarNumeric = types.All(t => t == "float" || t == "int");
        var allBool = types.All(t => t == "bool");
        // The preamble defines vector/vector and vector/scalar arithmetic for float2/3/4.
        var vectorish = types.Any(CppWriter.IsVector)
                        && types.All(t => CppWriter.IsVector(t) || t == "float")
                        && types.Where(CppWriter.IsVector).Distinct().Count() == 1;
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
                // Defined for scalars, for the vector types and for every emitted struct.
                ok = sameType;
                break;
            case "&&": case "||": case "!":
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

        // C++ % is integer only; float modulo is fmodf.
        if (op == "%" && types.Any(t => t == "float"))
        {
            Write("fmodf(");
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
