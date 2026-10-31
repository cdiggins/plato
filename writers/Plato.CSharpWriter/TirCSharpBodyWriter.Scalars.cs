// Body-local scalar lowering (--lower-scalars): scalar values as primitives inside a body.
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// The lowered rendering of Number, Integer and Boolean values (<see cref="CSharpWriter.LowerScalars"/>).
/// Two renderings exist for every scalar-typed node. <see cref="WritePrimitive"/> writes it as the
/// primitive the wrapper holds: an intrinsic becomes a C# operator over its operands' primitives
/// (<see cref="ScalarLowering"/>), a literal is bare, a local or loop parameter the body holds as a
/// primitive is its name, and anything else is the wrapper expression followed by <c>.Value</c>.
/// <see cref="TirCSharpBodyWriter.WriteNode"/> keeps writing the wrapper the node's Plato type names,
/// so every call argument, return, field and lambda result sees the type it saw before; where the
/// value is held or computed as a primitive it is wrapped there, once, by the struct's constructor.
/// Scalar lets, inline bindings, reduction accumulators and the scalar parameters of inlined loop
/// lambdas are declared as primitives and remembered in the three maps below, keyed as the nodes
/// that read them are keyed.
/// </summary>
public partial class TirCSharpBodyWriter
{
    private bool Lower => _tw.Writer.LowerScalars;

    // Scalar locals, loop-lambda parameters and loop temporaries this body holds as primitives,
    // each with the wrapper it stands for.
    private readonly Dictionary<VariableDef, string> _primitiveLocals = new();
    private readonly Dictionary<ParameterDef, string> _primitiveParams = new();
    private readonly Dictionary<string, string> _primitiveTemps = new();

    private static string ScalarOf(TypeExpression type) => ScalarLowering.ScalarOf(type);

    private string PrimitiveType(string scalar) => ScalarLowering.PrimitiveType(scalar, _tw.Writer.Profile);

    /// <summary>The scalar a let, binding or region result holds, from the most to the least
    /// specific type the node carries; null when it is not a lowered scalar.</summary>
    private static string ScalarOf(TirNode node, VariableDef def, TirNode value)
        => ScalarOf(node?.Type) ?? ScalarOf(value?.Type) ?? ScalarOf(def?.Type);

    /// <summary>The one wrap of a lowered value, as <c>Unsafe.BitCast&lt;primitive, Wrapper&gt;(...)</c>.
    /// The wrapper is a single-field struct over the primitive, so the reinterpretation is the value
    /// the constructor would build; the JIT expands BitCast in its importer, so the wrap costs no
    /// call whatever the enclosing method's locals count, where the constructor and the implicit
    /// operator become real calls past the cap (measured on .NET 8.0.30, TKT-1129: 20 wraps in a
    /// 700-local method, 40 calls for either of those and none for BitCast).</summary>
    private void WriteWrapped(string scalar, Action primitive)
    {
        Write($"Unsafe.BitCast<{PrimitiveType(scalar)}, {scalar}>(");
        primitive();
        Write(")");
    }

    /// <summary>Writes a scalar-typed node as its primitive value.</summary>
    private void WritePrimitive(TirNode n)
    {
        switch (n)
        {
            case TirLiteral lit when ScalarLowering.IsLoweredScalarName(lit.LiteralType.ToString()):
            {
                // A negative literal binds looser than a member access, so it keeps its parentheses.
                // Under a 64-bit Integer profile an integer literal is spelled as a long, so a local
                // declared from it has the Integer's own primitive type.
                var text = _tw.Writer.LiteralString(lit.Value);
                if (lit.LiteralType == Ara3D.Geometry.AST.LiteralTypesEnum.Integer && PrimitiveType("Integer") == "long" && char.IsDigit(text[text.Length - 1]))
                    text += "L";
                Write(text.StartsWith("-") ? $"({text})" : text);
                return;
            }
            case TirParameter p when p.Def != null && _primitiveParams.ContainsKey(p.Def):
                Write(_loopParamNames.TryGetValue(p.Def, out var loopName) ? loopName : CSharpTypeWriter.Identifier(p.Def.Name));
                return;
            case TirVariable v when v.Def != null && _primitiveLocals.ContainsKey(v.Def):
                Write(LocalName(v.Def));
                return;
            case TirTempRef tr when _primitiveTemps.ContainsKey(tr.Name):
                Write(tr.Name);
                return;
            case TirCall call when LoweredOperation(call) is { } op:
                WriteOperation(op, call.Args);
                return;
            case TirCall { Name: "Count", Args.Count: 1 } count when TirRewrite.IsListType(TirRewrite.StripCoerce(count.Args[0])?.Type):
                // The runtime list's Count is already the int the Integer wrapper would hold.
                WritePostfixReceiver(count.Args[0]);
                Write(".Count");
                return;
            case TirCall external when !BindsToGeneratedCode(external) && ScalarOf(external.Type) is { } externalScalar:
                // A call on a list, an interface or a type variable may bind outside the generated
                // code (Ara3D.Collections' IsEmpty on a list is a C# bool): the explicit conversion
                // is the identity on a primitive and the wrapper's own operator otherwise.
                Write($"(({PrimitiveType(externalScalar)})(");
                WriteNode(external);
                Write("))");
                return;
            case TirCoerce c when ScalarOf(c.ToType) is { } to:
            {
                var from = ScalarOf(c.FromType);
                if (from == to)
                {
                    WritePrimitive(c.Inner);
                    return;
                }
                if (from != null)
                {
                    // The one scalar widening, Integer to Number: the runtime's implicit conversion
                    // is the same cast of the value.
                    Write($"(({PrimitiveType(to)})");
                    WritePrimitive(c.Inner);
                    Write(")");
                    return;
                }
                // A conversion from a non-scalar (a typed index to Integer): the wrapper's own
                // conversion, then its value.
                Write($"(({to})");
                WriteNode(c.Inner);
                Write(").Value");
                return;
            }
            case TirConditional cond:
                Write("(");
                WriteCondition(cond.Condition);
                Write(" ? ");
                WritePrimitive(cond.IfTrue);
                Write(" : ");
                WritePrimitive(cond.IfFalse);
                Write(")");
                return;
            case TirBooleanChain bc:
                Write("(");
                for (var i = 0; i < bc.Terms.Count; i++)
                {
                    if (i > 0)
                        Write($" {bc.Op} ");
                    WritePrimitive(bc.Terms[i]);
                }
                Write(")");
                return;
            case TirDefault d when ScalarOf(d.Type) is { } scalar:
                Write($"default({PrimitiveType(scalar)})");
                return;
            case TirAssign { LValue: TirVariable lv } asg when lv.Def != null && _primitiveLocals.ContainsKey(lv.Def):
                Write($"({LocalName(lv.Def)} = ");
                WritePrimitive(asg.RValue);
                Write(")");
                return;
            case TirComponentAccess ca when ca.CastTo != null && CSharpWriter.ScalarPrimitives.ContainsValue(ca.CastTo):
                // The component unroller already cast this component to the primitive.
                WriteNode(ca);
                return;
        }
        // Any other scalar-typed node renders as a wrapper-valued C# expression: read its field.
        var parens = NeedsReceiverParentheses(n);
        if (parens) Write("(");
        WriteNode(n);
        if (parens) Write(")");
        Write(".Value");
    }

    /// <summary>The intrinsic a call lowers to, or null: the table's row, unless the call is an
    /// Integer operation over constants alone, which C# would fold at compile time and reject
    /// where the runtime wraps or throws (CS0220 on an overflow, CS0020 on a division by zero;
    /// the capacity laws probe exactly those). Such a call keeps the wrapper spelling.</summary>
    private ScalarLowering.Op? LoweredOperation(TirCall call)
    {
        var op = ScalarLowering.Lookup(call, out var scalar);
        return op != null && scalar == "Integer" && IsConstant(call) ? null : op;
    }

    /// <summary>Whether a scalar node is a C# constant expression once lowered: literals under
    /// lowered operations, conversions and conditionals alone.</summary>
    private bool IsConstant(TirNode n)
        => n switch
        {
            TirLiteral => true,
            TirCoerce c => IsConstant(c.Inner),
            TirConditional cond => IsConstant(cond.Condition) && IsConstant(cond.IfTrue) && IsConstant(cond.IfFalse),
            TirCall call => ScalarLowering.Lookup(call, out _) != null && call.Args.All(IsConstant),
            _ => false,
        };

    /// <summary>Whether a call's C# binds to code this writer emits or to the wrapper structs, so
    /// its scalar result is the wrapper: a nullary constant, a static on a type, or a receiver of
    /// a concrete generated or scalar type (Self standing for the enclosing type). A receiver that
    /// is a list, an interface or a type variable may bind to the runtime or to Ara3D.Collections.</summary>
    private bool BindsToGeneratedCode(TirCall call)
    {
        if (call.Args.Count == 0)
            return true;
        var receiver = TirRewrite.StripCoerce(call.Args[0]);
        if (receiver is TirTypeRef)
            return true;
        var name = receiver?.Type?.Def?.Name ?? receiver?.Type?.Name;
        if (name == "Self")
            name = _tw.TypeDef?.Name;
        return name != null && _tw.Writer.IsConcreteTypeName(name);
    }

    /// <summary>Writes a lowered Integer primitive where C# wants an <c>int</c> (an indexer, an
    /// array length): bare under a 32-bit Integer, else the checked narrowing the wrapper's own
    /// conversion to int performs.</summary>
    private void WriteAsInt(Action primitive)
    {
        var narrow = PrimitiveType("Integer") != "int";
        if (narrow) Write("checked((int)(");
        primitive();
        if (narrow) Write("))");
    }

    /// <summary>Whether a scalar let lowers to a primitive local: a constant value stays a wrapper
    /// local, since an unread primitive constant is CS0219 and a constant folds either way.</summary>
    private bool LowersLet(TirNode value) => !IsConstant(value);

    /// <summary>Writes an intrinsic's C# spelling with its operands as primitives, streaming the
    /// format's literal text and each <c>{i}</c> operand in order.</summary>
    private void WriteOperation(ScalarLowering.Op op, IReadOnlyList<TirNode> args)
    {
        var format = ScalarLowering.Format(op, _tw.Writer.Profile);
        var start = 0;
        while (true)
        {
            var open = format.IndexOf('{', start);
            if (open < 0)
            {
                Write(format.Substring(start));
                return;
            }
            Write(format.Substring(start, open - start));
            var close = format.IndexOf('}', open);
            var index = int.Parse(format.Substring(open + 1, close - open - 1));
            WritePrimitive(args[index]);
            start = close + 1;
        }
    }

    /// <summary>Writes a Boolean-typed node where C# wants a condition: its primitive when
    /// lowering, else the wrapper (whose <c>operator true</c> C# applies), with the parentheses a
    /// conditional or assignment needs there (plato-454).</summary>
    private void WriteCondition(TirNode condition)
    {
        if (Lower)
        {
            WritePrimitive(condition);
            return;
        }
        var node = TirRewrite.StripCoerce(condition);
        var wrap = node is TirConditional || node is TirAssign;
        if (wrap) Write("(");
        WriteNode(condition);
        if (wrap) Write(")");
    }

    /// <summary>Writes a Boolean-typed node as a C# <c>bool</c> in parentheses: the primitive when
    /// lowering, else the explicit conversion the loop and guard emitters have always written.</summary>
    private void WriteBoolTest(Action writeWrapper, Action writePrimitive)
    {
        if (Lower)
        {
            Write("(");
            writePrimitive();
            Write(")");
            return;
        }
        Write("(bool)(");
        writeWrapper();
        Write(")");
    }

    private void WriteBoolTest(TirNode n) => WriteBoolTest(() => WriteNode(n), () => WritePrimitive(n));

    /// <summary>Declares a scalar let as a primitive local (<c>var x = ...</c>, no terminator).</summary>
    private void WritePrimitiveLet(VariableDef def, string scalar, TirNode value)
    {
        Write($"var {LocalName(def)} = ");
        WritePrimitive(value);
        _primitiveLocals[def] = scalar;
    }

    /// <summary>The lowered rendering of a node in wrapper position, when lowering changes it:
    /// a primitive-held local, parameter or temporary is wrapped; an intrinsic, a Boolean chain and
    /// a scalar-to-scalar conversion are computed as primitives and wrapped; a conditional writes
    /// its condition as a primitive; a scalar let or an assignment to a primitive local stays a
    /// primitive. False leaves the node to the ordinary rendering.</summary>
    private bool TryWriteLowered(TirNode n)
    {
        switch (n)
        {
            case TirParameter p when p.Def != null && _primitiveParams.TryGetValue(p.Def, out var ps):
                WriteWrapped(ps, () => WritePrimitive(p));
                return true;
            case TirVariable v when v.Def != null && _primitiveLocals.TryGetValue(v.Def, out var vs):
                WriteWrapped(vs, () => WritePrimitive(v));
                return true;
            case TirTempRef tr when _primitiveTemps.TryGetValue(tr.Name, out var ts):
                WriteWrapped(ts, () => WritePrimitive(tr));
                return true;
            case TirCall call when LoweredOperation(call) is { } op:
                WriteWrapped(op.Result, () => WriteOperation(op, call.Args));
                return true;
            case TirBooleanChain bc:
                WriteWrapped("Boolean", () => WritePrimitive(bc));
                return true;
            case TirCoerce c when ScalarOf(c.ToType) is { } to && ScalarOf(c.FromType) is { } from && from != to:
                WriteWrapped(to, () => WritePrimitive(c));
                return true;
            case TirConditional cond:
                WriteCondition(cond.Condition);
                Write(" ? ");
                WriteNode(cond.IfTrue);
                Write(" : ");
                WriteNode(cond.IfFalse);
                return true;
            case TirAssign { LValue: TirVariable lv } asg when lv.Def != null && _primitiveLocals.ContainsKey(lv.Def):
                Write(LocalName(lv.Def));
                Write(" = ");
                WritePrimitive(asg.RValue);
                return true;
            case TirLet let when ScalarOf(let, let.Def, let.Value) is { } ls && LowersLet(let.Value):
                WritePrimitiveLet(let.Def, ls, let.Value);
                return true;
        }
        return false;
    }

    // --- lowered loops -------------------------------------------------------------------------

    /// <summary>A value a lowered loop binds to an inlined lambda parameter, in the spelling that
    /// has the wrapper type and, when the loop knows it, the spelling that is the bare primitive
    /// (the loop index, a primitive accumulator). Null means the wrapper form is all there is, and a
    /// scalar parameter reads its <c>.Value</c>.</summary>
    private readonly record struct Operand(string Wrapped, string Primitive)
    {
        public static implicit operator Operand(string wrapped) => new(wrapped, null);
    }

    /// <summary>Binds an inlined loop-lambda parameter to the name it is declared under and, when it
    /// is a scalar held as a primitive, remembers the scalar.</summary>
    private void BindLoopParameter(ParameterDef p, string name, string primitiveScalar)
    {
        _loopParamNames[p] = name;
        if (primitiveScalar != null)
            _primitiveParams[p] = primitiveScalar;
        else
            _primitiveParams.Remove(p);
    }

    private void UnbindLoopParameter(ParameterDef p)
    {
        _loopParamNames.Remove(p);
        _primitiveParams.Remove(p);
    }

    /// <summary>The scalar the i-th parameter of a loop lambda holds when lowering, from the
    /// lambda's function type first and its parameter declaration second; null otherwise.</summary>
    private string LoweredLambdaParameterScalar(TirLambda lam, int i)
    {
        if (!Lower)
            return null;
        var fromType = lam.Type != null && lam.Type.TypeArgs.Count == lam.Parameters.Count + 1
            ? ScalarOf(lam.Type.TypeArgs[i])
            : null;
        return fromType ?? ScalarOf(lam.Parameters[i].Type);
    }

    /// <summary>Declares one inlined lambda parameter from the operand the loop binds it to: a
    /// scalar parameter as the primitive (the operand's primitive spelling, else the wrapper's
    /// <c>.Value</c>), anything else as the wrapper spelling.</summary>
    private void DeclareLoopParameter(TirLambda lam, int i, Operand operand)
    {
        var p = lam.Parameters[i];
        var scalar = LoweredLambdaParameterScalar(lam, i);
        var name = _loopParamNames[p];
        if (scalar != null)
        {
            WriteLine($"var {name} = {operand.Primitive ?? operand.Wrapped + ".Value"};");
            BindLoopParameter(p, name, scalar);
        }
        else
            WriteLine($"var {name} = {operand.Wrapped};");
    }
}
