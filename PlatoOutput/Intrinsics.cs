using System;
using System.Text;

public static class Intrinsics
{
    public static Number Cos(Angle x) => Math.Cos(x.Value);
    public static Number Sin(Angle x) => Math.Sin(x.Value);
    public static Number Tan(Angle x) => Math.Tan(x.Value);

    public static Angle Acos(Number x) => new Angle(Math.Acos(x));
    public static Angle Asin(Number x) => new Angle(Math.Asin(x));
    public static Angle Atan(Number x) => new Angle(Math.Atan(x));

    public static Number Pow(Number x, Number y) => Math.Pow(x, y);
    public static Number Log(Number x, Number y) => Math.Log(x, y);
    public static Number NaturalLog(Number x) => Math.Log(x);
    public static Number NaturalPower(Number x) => Math.Pow(x, Math.E);

    public static Number Add(Number x, Number y) => x.Value + y.Value;
    public static Number Subtract(Number x, Number y) => x.Value - y.Value;
    public static Number Divide(Number x, Number y) => x.Value / y.Value;
    public static Number Multiply(Number x, Number y) => x.Value * y.Value;
    public static Number Modulo(Number x, Number y) => x.Value % y.Value;
    public static Number Negative(Number x) => -x.Value;

    public static Integer Add(Integer x, Integer y) => x.Value + y.Value;
    public static Integer Subtract(Integer x, Integer y) => x.Value - y.Value;
    public static Integer Divide(Integer x, Integer y) => x.Value / y.Value;
    public static Integer Multiply(Integer x, Integer y) => x.Value * y.Value;
    public static Integer Modulo(Integer x, Integer y) => x.Value % y.Value;
    public static Integer Negative(Integer x) => -x.Value;

    public static Boolean And(Boolean x, Boolean y) => x.Value && y.Value;
    public static Boolean Or(Boolean x, Boolean y) => x.Value || y.Value;
    public static Boolean Not(Boolean x) => !x.Value;

    public static Number ToNumber(Integer x) => x.Value;

    public static string MakeString(string typeName, Array<String> fieldNames, Array<Dynamic> fieldValues)
    {
        var sb = new StringBuilder();
        sb.Append($"{{ _type=\"{typeName}\" ");
        for (var i = 0; i < fieldNames.Count; i++)
            sb.Append(", ").Append(fieldNames.At(i).Value).Append(" = ").Append(fieldValues.At(i).Value);
        sb.Append(" }");
        return sb.ToString();
    }

    public static int CombineHashCodes(params object[] objects)
    {
        if (objects.Length == 0) return 0;
        var r = objects[0].GetHashCode();
        for (var i = 1; i < objects.Length; ++i)
            r = CombineHashCodes(r, objects[i].GetHashCode());
        return r;
    }

    public static int CombineHashCodes(int h1, int h2)
    {
        unchecked
        {
            // RyuJIT optimizes this to use the ROL instruction
            // Related GitHub pull request: dotnet/coreclr#1830
            var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)rol5 + h1) ^ h2;
        }
    }
}