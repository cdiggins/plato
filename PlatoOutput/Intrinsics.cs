using System;

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
}