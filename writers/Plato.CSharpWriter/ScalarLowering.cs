// The scalar intrinsics the body writer can spell as C# over the primitives (--lower-scalars).
using System.Collections.Generic;
using Ara3D.Geometry.Compiler.Checking;
using Ara3D.Geometry.Compiler.Symbols;
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.CSharpWriter;

/// <summary>
/// Body-local scalar lowering (<see cref="CSharpWriter.LowerScalars"/>): the table of host scalar
/// intrinsics, from <c>stdlib/foundation/intrinsics.library.plato</c>, that the body writer renders
/// as C# over the wrapped primitives instead of as calls on the wrapper structs. Each entry spells
/// exactly what the handwritten runtime does for the call (<c>src/Plato.Intrinsics/Number.cs</c>,
/// <c>Integer.cs</c>, <c>Boolean.cs</c>): <c>Number.Equals</c> is <c>double.Equals</c>, which
/// holds for two NaNs, where <c>Integer.Equals</c> is <c>==</c>; integer arithmetic is unchecked;
/// <c>ToInteger</c> is the truncating cast; the connectives evaluate both operands. A change to a
/// runtime operator changes its row here, and <c>ScalarLoweringTests</c> runs both spellings over
/// the awkward inputs to keep them agreeing.
///
/// Only Number, Integer and Boolean lower. Character and String stay wrappers everywhere, and the
/// intrinsics with no one-expression spelling (Hash, Round, the limits) stay calls on the wrapper:
/// the writer reads their result's <c>.Value</c> like any other wrapper-returning call.
/// </summary>
public static class ScalarLowering
{
    /// <summary>The C# spelling of one intrinsic over primitive operands: a format string whose
    /// <c>{0}</c>, <c>{1}</c>, <c>{2}</c> are the operands, already parenthesized where they are
    /// compound, and the scalar the result is (the wrapper that would hold it).</summary>
    public readonly record struct Op(string Format, string Result);

    private static readonly Dictionary<(string Scalar, string Name, int Arity), Op> Table = Build();

    private static Dictionary<(string, string, int), Op> Build()
    {
        var t = new Dictionary<(string, string, int), Op>();
        void Add(string scalar, string name, int arity, string format, string result = null)
            => t[(scalar, name, arity)] = new Op(format, result ?? scalar);

        foreach (var s in new[] { "Number", "Integer" })
        {
            Add(s, "Add", 2, "({0} + {1})");
            Add(s, "Subtract", 2, "({0} - {1})");
            Add(s, "Multiply", 2, "({0} * {1})");
            Add(s, "Divide", 2, "({0} / {1})");
            Add(s, "Modulo", 2, "({0} % {1})");
            Add(s, "Negative", 1, "(-{0})");
            Add(s, "LessThanOrEquals", 2, "({0} <= {1})", "Boolean");
        }
        // double.Equals: true for two NaNs, where == is false; the generated struct's Equals.
        Add("Number", "Equals", 2, "{0}.Equals({1})", "Boolean");
        Add("Integer", "Equals", 2, "({0} == {1})", "Boolean");

        Add("Number", "Floor", 1, "NumberMath.Floor({0})");
        Add("Number", "Sqrt", 1, "NumberMath.Sqrt({0})");
        Add("Number", "Cbrt", 1, "NumberMath.Cbrt({0})");
        Add("Number", "Exp", 1, "NumberMath.Exp({0})");
        Add("Number", "NaturalLog", 1, "NumberMath.Log({0})");
        Add("Number", "Pow", 2, "NumberMath.Pow({0}, {1})");
        Add("Number", "FusedMultiplyAdd", 3, "NumberMath.FusedMultiplyAdd({0}, {1}, {2})");
        Add("Number", "Sin", 1, "NumberMath.Sin({0})");
        Add("Number", "Cos", 1, "NumberMath.Cos({0})");
        Add("Number", "Atan2Radians", 2, "NumberMath.Atan2({0}, {1})");
        Add("Number", "IsNaN", 1, "NumberValue.IsNaN({0})", "Boolean");
        Add("Number", "IsInfinite", 1, "NumberValue.IsInfinity({0})", "Boolean");
        // The cast the runtime's ToInteger performs; the integer type is the profile's, filled in
        // by the writer (IntegerCast).
        Add("Number", "ToInteger", 1, "((" + IntegerTypePlaceholder + "){0})", "Integer");
        Add("Integer", "ToNumber", 1, "((" + CSharpWriter.NumberRuntimeType + "){0})", "Number");

        Add("Integer", "BitwiseAnd", 2, "({0} & {1})");
        Add("Integer", "BitwiseOr", 2, "({0} | {1})");
        Add("Integer", "BitwiseXor", 2, "({0} ^ {1})");
        Add("Integer", "BitwiseNot", 1, "(~{0})");
        Add("Integer", "ShiftLeft", 2, "({0} << (int){1})");
        Add("Integer", "ShiftRight", 2, "({0} >> (int){1})");

        // Non-short-circuit, as the runtime's & and |: Plato's && and || arrive as conditionals.
        Add("Boolean", "And", 2, "({0} & {1})");
        Add("Boolean", "Or", 2, "({0} | {1})");
        Add("Boolean", "Not", 1, "(!{0})");
        return t;
    }

    private const string IntegerTypePlaceholder = "@int";

    /// <summary>The three scalars that lower, by wrapper name.</summary>
    public static bool IsLoweredScalarName(string name)
        => name == "Number" || name == "Integer" || name == "Boolean";

    /// <summary>The lowered scalar a type is, by its definition's name, or null.</summary>
    public static string ScalarOf(TypeExpression type)
    {
        var name = type?.Def?.Name ?? type?.Name;
        return IsLoweredScalarName(name) ? name : null;
    }

    /// <summary>The intrinsic a call is, when every one of its instantiated parameter types is the
    /// same lowered scalar and the name and arity are in the table; null otherwise. Keyed on the
    /// instantiated parameter types rather than the callee symbol because a call that reached the
    /// body through an interface obligation (<c>IOrderable.LessThanOrEquals</c>) or through inlining
    /// still carries the ground scalar types, and no Plato library can declare a second function
    /// with an intrinsic's signature.</summary>
    public static Op? Lookup(TirCall call, out string scalar)
    {
        scalar = null;
        if (call?.Name == null || call.Args.Count == 0 || call.ParameterTypes.Count != call.Args.Count)
            return null;
        scalar = ScalarOf(call.ParameterTypes[0]);
        if (scalar == null)
            return null;
        for (var i = 1; i < call.ParameterTypes.Count; i++)
            if (ScalarOf(call.ParameterTypes[i]) != scalar)
                return null;
        return Table.TryGetValue((scalar, call.Name, call.Args.Count), out var op) ? op : null;
    }

    /// <summary>The C# primitive a lowered scalar is held in, at the writer's profile.</summary>
    public static string PrimitiveType(string scalar, NumberProfile profile)
        => scalar switch
        {
            "Number" => CSharpWriter.NumberRuntimeType,
            "Integer" => profile.IntegerBits == 32 ? "int" : "long",
            "Boolean" => "bool",
            _ => null,
        };

    /// <summary>The format of an operation with the profile's integer type filled in.</summary>
    public static string Format(Op op, NumberProfile profile)
        => op.Format.Replace(IntegerTypePlaceholder, PrimitiveType("Integer", profile));
}
