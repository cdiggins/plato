using System;
using System.Text;
/*
public interface Array<T>
{
    Integer Count { get; }
    T At(Integer n);
}
*/
public static class Intrinsics
{
    public static Number Cos(Angle x) => Math.Cos(x.Value);
    public static Number Sin(Angle x) => Math.Sin(x.Value);
    public static Number Tan(Angle x) => Math.Tan(x.Value);

    public static Number Ln(Number x) => Math.Log(x.Value);
    public static Number Exp(Number x) => Math.Exp(x.Value);

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

    public static Array<Integer> Range(Integer x) => new RangeStruct(x);

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
  
    public readonly struct PrimitiveArray<T> : Array<T>
    {
        private readonly T[] _data;
        public Integer Count => _data.Length;
        public T At(Integer n) => _data[n];
        public PrimitiveArray(T[] data) => _data = data;
        public static Array<T> Default = new PrimitiveArray<T>(Array.Empty<T>());
    }

    public readonly struct MappedArray<T0, T1> : Array<T1>
    {
        public Func<T0, T1> MapFunc { get; }
        public Array<T0> Original { get; }
        public Integer Count => Original.Count;
        public T1 At(Integer n) => MapFunc(Original.At(n));

        public MappedArray(Array<T0> input, Func<T0, T1> f)
        {
            Original = input;
            MapFunc = f;
        }
    }

    public readonly struct RangeStruct : Array<Integer>
    {
        public Integer Count { get; }
        public Integer At(Integer n) => n;
        public RangeStruct(Integer n) => Count = n;
    }

    public static Array<T> MakeArray<T>(params T[] args)
        => new PrimitiveArray<T>(args);

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

    public static Array<T1> Map<T0, T1>(this Array<T0> self, Func<T0, T1> f)
        => new MappedArray<T0, T1>(self, f);

    public static TAcc Reduce<T, TAcc>(this Array<T> self, TAcc init, Func<TAcc, T, TAcc> f)
    {
        for (var i = 0; i < self.Count; ++i)
            init = f(init, self.At(i));
        return init;
    }
}