// This file is required to make the input compile correctly
using System;
using System.Runtime.CompilerServices;

namespace Plato
{
    // The following are prerequisites built into Plato

    public class VectorAttribute : Attribute { }
    public class ValueAttribute : Attribute { }
    public class MeasureAttribute : Attribute { }
    public class NumberAttribute : Attribute { }
    public class IntervalAttribute : Attribute { }

    public static partial class Intrinsics
    {
        public static Angle Acos(this double x) => Math.Acos(x);
        public static Angle Asin(this double x) => Math.Asin(x);
        public static Angle Atan(this double x) => Math.Atan(x);

        public static double Cos(this Angle x) => Math.Cos(x);
        public static double Cosh(this Angle x) => Math.Cosh(x);
        public static double Sin(this Angle x) => Math.Sin(x);
        public static double Sinh(this Angle x) => Math.Sinh(x);

        public static double Tan(this Angle x) => Math.Tan(x);
        public static double Tanh(this Angle x) => Math.Tanh(x);

        public static double Abs(this double x) => Math.Abs(x);
        public static double Exp(this double x) => Math.Exp(x);
        public static double Log(this double x) => Math.Log(x);
        public static double Log10(this double x) => Math.Log10(x);
        public static double Sqrt(this double x) => Math.Sqrt(x);
    }

    /*
        public static float Abs(this float x) => (float)Math.Abs(x);
        public static float Acos(this float x) => (float)Math.Acos(x);
        public static float Asin(this float x) => (float)Math.Asin(x);
        public static float Atan(this float x) => (float)Math.Atan(x);
        public static float Cos(this float x) => (float)Math.Cos(x);
        public static float Cosh(this float x) => (float)Math.Cosh(x);
        public static float Exp(this float x) => (float)Math.Exp(x);
        public static float Log(this float x) => (float)Math.Log(x);
        public static float Log10(this float x) => (float)Math.Log10(x);
        public static float Sin(this float x) => (float)Math.Sin(x);
        public static float Sinh(this float x) => (float)Math.Sinh(x);
        public static float Sqrt(this float x) => (float)Math.Sqrt(x);
        public static float Tan(this float x) => (float)Math.Tan(x);
        public static float Tanh(this float x) => (float)Math.Tanh(x);

        public static int Sign(this float x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        public static float Magnitude(this float x) => x;
        public static float MagnitudeSquared(this float x) => x * x;
        public static float Inverse(this float x) => (float)1 / x;
        public static float Truncate(this float x) => (float)Math.Truncate(x);
        public static float Ceiling(this float x) => (float)Math.Ceiling(x);
        public static float Floor(this float x) => (float)Math.Floor(x);
        public static float Round(this float x) => (float)System.Math.Round(x);
        public static float ToRadians(this float x) => (float)(x * Constants.DegreesToRadians);
        public static float ToDegrees(this float x) => (float)(x * Constants.RadiansToDegrees);
        public static float Distance(this float v1, float v2) => (v1 - v2).Abs();
        public static bool IsInfinity(this float v) => float.IsInfinity(v);
        public static bool IsNaN(this float v) => float.IsNaN(v);
        public static bool AlmostEquals(this float v1, float v2, float tolerance = Constants.Tolerance) => (v2 - v1).AlmostZero(tolerance);
        public static bool AlmostZero(this float v, float tolerance = Constants.Tolerance) => v.Abs() < tolerance;
        public static float Smoothstep(this float v) => v * v * (3 - 2 * v);
        public static int Sign(this double x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        public static double Magnitude(this double x) => x;
        public static double MagnitudeSquared(this double x) => x * x;
        public static double Inverse(this double x) => (double)1 / x;
        public static double Truncate(this double x) => (double)Math.Truncate(x);
        public static double Ceiling(this double x) => (double)Math.Ceiling(x);
        public static double Floor(this double x) => (double)Math.Floor(x);
        public static double Round(this double x) => (double)System.Math.Round(x);
        public static double ToRadians(this double x) => (double)(x * Constants.DegreesToRadians);
        public static double ToDegrees(this double x) => (double)(x * Constants.RadiansToDegrees);
        public static double Distance(this double v1, double v2) => (v1 - v2).Abs();
        public static bool IsInfinity(this double v) => double.IsInfinity(v);
        public static bool IsNaN(this double v) => double.IsNaN(v);
        public static bool AlmostEquals(this double v1, double v2, float tolerance = Constants.Tolerance) => (v2 - v1).AlmostZero(tolerance);
        public static bool AlmostZero(this double v, float tolerance = Constants.Tolerance) => v.Abs() < tolerance;
        public static double Smoothstep(this double v) => v * v * (3 - 2 * v);

        public static int Add(this int v1, int v2) => v1 + v2;
        public static int Subtract(this int v1, int v2) => v1 - v2;
        public static int Multiply(this int v1, int v2) => v1 * v2;
        public static int Divide(this int v1, int v2) => v1 / v2;
        public static int Negate(this int v) => -v;
        public static long Add(this long v1, long v2) => v1 + v2;
        public static long Subtract(this long v1, long v2) => v1 - v2;
        public static long Multiply(this long v1, long v2) => v1 * v2;
        public static long Divide(this long v1, long v2) => v1 / v2;
        public static long Negate(this long v) => -v;
        public static float Add(this float v1, float v2) => v1 + v2;
        public static float Subtract(this float v1, float v2) => v1 - v2;
        public static float Multiply(this float v1, float v2) => v1 * v2;
        public static float Divide(this float v1, float v2) => v1 / v2;
        public static float Negate(this float v) => -v;
        public static double Add(this double v1, double v2) => v1 + v2;
        public static double Subtract(this double v1, double v2) => v1 - v2;
        public static double Multiply(this double v1, double v2) => v1 * v2;
        public static double Divide(this double v1, double v2) => v1 / v2;
        public static double Negate(this double v) => -v;
    }
    */
}