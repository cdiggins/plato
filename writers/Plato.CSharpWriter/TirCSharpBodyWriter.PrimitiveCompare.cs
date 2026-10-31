// --primitive-compare (TKT-1095): Number and Integer comparisons on the machine values.
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;

namespace Ara3D.Geometry.CSharpWriter;

public partial class TirCSharpBodyWriter
{
    // The comparison each wrapper method computes, written on the wrapped machine values. For
    // Number: LessThan is `a <= b && !a.Equals(b)`, false when either side is NaN and false for
    // -0 against 0, which is exactly IEEE `<`; GreaterThan(a, b) is LessThan(b, a), so `>`;
    // GreaterThanOrEquals(a, b) is LessThanOrEquals(b, a), so `>=`. Number equality is
    // double.Equals (NaN equals NaN, -0 equals 0), not `==`, so it keeps that call on the value.
    // Integer has no NaN or signed zero, so its equality is `==`.
    private static readonly Dictionary<string, string> NumberComparisons = new()
    {
        ["LessThan"] = "{0} < {1}", ["LessThanOrEquals"] = "{0} <= {1}",
        ["GreaterThan"] = "{0} > {1}", ["GreaterThanOrEquals"] = "{0} >= {1}",
        ["Equals"] = "{0}.Equals({1})", ["NotEquals"] = "!{0}.Equals({1})",
    };

    private static readonly Dictionary<string, string> IntegerComparisons = new()
    {
        ["LessThan"] = "{0} < {1}", ["LessThanOrEquals"] = "{0} <= {1}",
        ["GreaterThan"] = "{0} > {1}", ["GreaterThanOrEquals"] = "{0} >= {1}",
        ["Equals"] = "{0} == {1}", ["NotEquals"] = "{0} != {1}",
    };

    /// <summary>With --primitive-compare, a resolved two-argument comparison whose operands are
    /// both Number or both Integer (no conversion between them) is written as
    /// <c>((Boolean)(a.Value &lt; b.Value))</c> instead of <c>a.LessThan(b)</c>: one wrapper call
    /// fewer for the JIT to inline, the same result for every input.</summary>
    private bool TryWritePrimitiveCompare(TirCall call)
    {
        if (!_tw.Writer.PrimitiveCompare || call.Callee == null || call.Args.Count != 2
            || call.Callee.NumParameters != 2 || call.Type?.Name != "Boolean")
            return false;
        var type = call.Args[0]?.Type?.Name;
        var table = type switch { "Number" => NumberComparisons, "Integer" => IntegerComparisons, _ => null };
        if (table == null || !table.TryGetValue(call.Name ?? "", out var form))
            return false;
        foreach (var arg in call.Args)
            if (arg?.Type?.Name != type || TirRewrite.StripCoerce(arg)?.Type?.Name != type)
                return false;

        var parts = form.Split(new[] { "{0}", "{1}" }, System.StringSplitOptions.None);
        Write("((Boolean)(");
        Write(parts[0]);
        WriteMachineValue(call.Args[0], type);
        Write(parts[1]);
        WriteMachineValue(call.Args[1], type);
        Write(parts[2]);
        Write("))");
        return true;
    }

    // `x.Value` for an operand whose C# is the wrapper itself (a literal, a parameter); any other
    // expression is cast first, because some Integer-typed TIR emits as a C# int (a struct-surface
    // Count property, a local bound to one), which has no .Value. The cast is an identity
    // conversion for a wrapper and the runtime's implicit widening for a machine value.
    private void WriteMachineValue(TirNode operand, string type)
    {
        if (operand is TirLiteral || operand is TirParameter p && (p.Def == null || !_loopParamNames.ContainsKey(p.Def)))
        {
            WritePostfixReceiver(operand);
        }
        else
        {
            Write($"(({type})(");
            WriteNode(operand);
            Write("))");
        }
        Write(".Value");
    }
}
