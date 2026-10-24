// Applies the selected numeric profile to interpreted arithmetic and literals.
using Ara3D.Geometry.Compiler.Types;

namespace Ara3D.Geometry.Interpreter;

/// <summary>
/// The arithmetic a <see cref="NumberProfile"/> gives Number and Integer, applied by every host
/// intrinsic and every literal. Number is always stored as a double; under gpu32 each result is
/// rounded to binary32 (a double holding a float is exact, and rounding a binary64 result of
/// +, -, *, / or sqrt to binary32 gives the correctly rounded binary32 result, so the stored
/// doubles are the floats the C# gpu32 build would hold). The transcendental functions go
/// through MathF under gpu32 and Math under cpu64, as the C# runtime's NumberMath alias does.
/// Integer is a long; under gpu32 it wraps to 32 bits after every operation, as the C# runtime's
/// int does; under cpu64 it is 64-bit storage, exact within 2^53 when converted to Number.
/// </summary>
public sealed class Numbers
{
    public NumberProfile Profile { get; }
    public bool Is32 => Profile.NumberBits == 32;
    private readonly bool _int32;

    public Numbers(NumberProfile profile)
    {
        Profile = profile;
        _int32 = profile.IntegerBits == 32;
    }

    public double N(double x) => Is32 ? (double)(float)x : x;
    public long I(long x) => _int32 ? (int)x : x;

    public double Add(double a, double b) => N(a + b);
    public double Subtract(double a, double b) => N(a - b);
    public double Multiply(double a, double b) => N(a * b);
    public double Divide(double a, double b) => Is32 ? (double)((float)a / (float)b) : a / b;
    public double Modulo(double a, double b) => Is32 ? (double)((float)a % (float)b) : a % b;
    public double Negative(double a) => -a;
    public double Floor(double a) => Is32 ? MathF.Floor((float)a) : Math.Floor(a);
    public double Round(double a, long digits) => Is32 ? MathF.Round((float)a, (int)digits) : Math.Round(a, (int)digits);
    public double Sqrt(double a) => Is32 ? MathF.Sqrt((float)a) : Math.Sqrt(a);
    public double Exp(double a) => Is32 ? MathF.Exp((float)a) : Math.Exp(a);
    public double NaturalLog(double a) => Is32 ? MathF.Log((float)a) : Math.Log(a);
    public double Pow(double a, double b) => Is32 ? MathF.Pow((float)a, (float)b) : Math.Pow(a, b);
    public double FusedMultiplyAdd(double a, double b, double c)
        => Is32 ? MathF.FusedMultiplyAdd((float)a, (float)b, (float)c) : Math.FusedMultiplyAdd(a, b, c);
    public double Sin(double a) => Is32 ? MathF.Sin((float)a) : Math.Sin(a);
    public double Cos(double a) => Is32 ? MathF.Cos((float)a) : Math.Cos(a);
    public double Atan2(double y, double x) => Is32 ? MathF.Atan2((float)y, (float)x) : Math.Atan2(y, x);
    public double MinValue => Is32 ? float.MinValue : double.MinValue;
    public double MaxValue => Is32 ? float.MaxValue : double.MaxValue;
    public double Epsilon => Is32 ? float.Epsilon : double.Epsilon;
    public double Pi => Is32 ? MathF.PI : Math.PI;

    /// <summary>The C# runtime's <c>(int)Value</c>: truncation toward zero; an out-of-range or
    /// non-finite value saturates the way .NET's conversion does.</summary>
    public long ToInteger(double a)
    {
        if (_int32)
            return double.IsNaN(a) ? int.MinValue : (int)Math.Clamp(Math.Truncate(a), int.MinValue, int.MaxValue);
        return double.IsNaN(a) ? long.MinValue : (long)Math.Clamp(Math.Truncate(a), long.MinValue, long.MaxValue);
    }

    public double ToNumber(long a) => N(a);

    /// <summary>The hash the C# wrapper reports: double.GetHashCode under cpu64, float's under gpu32.</summary>
    public long HashNumber(double a) => I(Is32 ? ((float)a).GetHashCode() : a.GetHashCode());
    public long HashInteger(long a) => I(_int32 ? ((int)a).GetHashCode() : a.GetHashCode());

    public long IntegerMinValue => _int32 ? int.MinValue : long.MinValue;
    public long IntegerMaxValue => _int32 ? int.MaxValue : long.MaxValue;
}
