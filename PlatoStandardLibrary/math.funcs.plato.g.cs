using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
namespace Plato {
public static class VectorOperations {
public static Double2 Normal(this Double2 v)
{
return v / v.Length();
}
public static Double3 Normal(this Double3 v)
{
return v / v.Length();
}
public static Double4 Normal(this Double4 v)
{
return v / v.Length();
}
public static Float2 Normal(this Float2 v)
{
return v / (float)v.Length();
}
public static Float3 Normal(this Float3 v)
{
return v / (float)v.Length();
}
public static Float4 Normal(this Float4 v)
{
return v / (float)v.Length();
}
public static double Length(this Line2D line)
{
return Distance(line.A, line.B);
}
public static double Distance(this Double2 a, Double2 b)
{
return (b - a).Length();
}
public static double Distance(this Double3 a, Double3 b)
{
return (b - a).Length();
}
public static double Distance(this Double4 a, Double4 b)
{
return (b - a).Length();
}
public static double Distance(this Float2 a, Float2 b)
{
return (b - a).Length();
}
public static double Distance(this Float3 a, Float3 b)
{
return (b - a).Length();
}
public static double Distance(this Float4 a, Float4 b)
{
return (b - a).Length();
}
}
public static class VectorizedOperations {
public static double SafeDivide(this double x, double y)
{
return y.AlmostZero() ? x : x / y;
}
public static Float2 SafeDivide(this Float2 x, Float2 y) => ((float)SafeDivide((double)x.X, (double)y.X), (float)SafeDivide((double)x.Y, (double)y.Y));
public static Float3 SafeDivide(this Float3 x, Float3 y) => ((float)SafeDivide((double)x.X, (double)y.X), (float)SafeDivide((double)x.Y, (double)y.Y), (float)SafeDivide((double)x.Z, (double)y.Z));
public static Float4 SafeDivide(this Float4 x, Float4 y) => ((float)SafeDivide((double)x.X, (double)y.X), (float)SafeDivide((double)x.Y, (double)y.Y), (float)SafeDivide((double)x.Z, (double)y.Z), (float)SafeDivide((double)x.W, (double)y.W));
public static Double2 SafeDivide(this Double2 x, Double2 y) => ((double)SafeDivide((double)x.X, (double)y.X), (double)SafeDivide((double)x.Y, (double)y.Y));
public static Double3 SafeDivide(this Double3 x, Double3 y) => ((double)SafeDivide((double)x.X, (double)y.X), (double)SafeDivide((double)x.Y, (double)y.Y), (double)SafeDivide((double)x.Z, (double)y.Z));
public static Double4 SafeDivide(this Double4 x, Double4 y) => ((double)SafeDivide((double)x.X, (double)y.X), (double)SafeDivide((double)x.Y, (double)y.Y), (double)SafeDivide((double)x.Z, (double)y.Z), (double)SafeDivide((double)x.W, (double)y.W));
public static Int2 SafeDivide(this Int2 x, Int2 y) => ((int)SafeDivide((double)x.A, (double)y.A), (int)SafeDivide((double)x.B, (double)y.B));
public static Int3 SafeDivide(this Int3 x, Int3 y) => ((int)SafeDivide((double)x.A, (double)y.A), (int)SafeDivide((double)x.B, (double)y.B), (int)SafeDivide((double)x.C, (double)y.C));
public static Int4 SafeDivide(this Int4 x, Int4 y) => ((int)SafeDivide((double)x.A, (double)y.A), (int)SafeDivide((double)x.B, (double)y.B), (int)SafeDivide((double)x.C, (double)y.C), (int)SafeDivide((double)x.D, (double)y.D));
public static Long2 SafeDivide(this Long2 x, Long2 y) => ((long)SafeDivide((double)x.A, (double)y.A), (long)SafeDivide((double)x.B, (double)y.B));
public static Long3 SafeDivide(this Long3 x, Long3 y) => ((long)SafeDivide((double)x.A, (double)y.A), (long)SafeDivide((double)x.B, (double)y.B), (long)SafeDivide((double)x.C, (double)y.C));
public static Long4 SafeDivide(this Long4 x, Long4 y) => ((long)SafeDivide((double)x.A, (double)y.A), (long)SafeDivide((double)x.B, (double)y.B), (long)SafeDivide((double)x.C, (double)y.C), (long)SafeDivide((double)x.D, (double)y.D));
public static Complex SafeDivide(this Complex x, Complex y) => ((double)SafeDivide((double)x.Real, (double)y.Real), (double)SafeDivide((double)x.Imaginary, (double)y.Imaginary));
public static double Half(this double x)
{
return x * 0.5;
}
public static Float2 Half(this Float2 x) => ((float)Half((double)x.X), (float)Half((double)x.Y));
public static Float3 Half(this Float3 x) => ((float)Half((double)x.X), (float)Half((double)x.Y), (float)Half((double)x.Z));
public static Float4 Half(this Float4 x) => ((float)Half((double)x.X), (float)Half((double)x.Y), (float)Half((double)x.Z), (float)Half((double)x.W));
public static Double2 Half(this Double2 x) => ((double)Half((double)x.X), (double)Half((double)x.Y));
public static Double3 Half(this Double3 x) => ((double)Half((double)x.X), (double)Half((double)x.Y), (double)Half((double)x.Z));
public static Double4 Half(this Double4 x) => ((double)Half((double)x.X), (double)Half((double)x.Y), (double)Half((double)x.Z), (double)Half((double)x.W));
public static Int2 Half(this Int2 x) => ((int)Half((double)x.A), (int)Half((double)x.B));
public static Int3 Half(this Int3 x) => ((int)Half((double)x.A), (int)Half((double)x.B), (int)Half((double)x.C));
public static Int4 Half(this Int4 x) => ((int)Half((double)x.A), (int)Half((double)x.B), (int)Half((double)x.C), (int)Half((double)x.D));
public static Long2 Half(this Long2 x) => ((long)Half((double)x.A), (long)Half((double)x.B));
public static Long3 Half(this Long3 x) => ((long)Half((double)x.A), (long)Half((double)x.B), (long)Half((double)x.C));
public static Long4 Half(this Long4 x) => ((long)Half((double)x.A), (long)Half((double)x.B), (long)Half((double)x.C), (long)Half((double)x.D));
public static Complex Half(this Complex x) => ((double)Half((double)x.Real), (double)Half((double)x.Imaginary));
public static double Quarter(this double x)
{
return x * 0.25;
}
public static Float2 Quarter(this Float2 x) => ((float)Quarter((double)x.X), (float)Quarter((double)x.Y));
public static Float3 Quarter(this Float3 x) => ((float)Quarter((double)x.X), (float)Quarter((double)x.Y), (float)Quarter((double)x.Z));
public static Float4 Quarter(this Float4 x) => ((float)Quarter((double)x.X), (float)Quarter((double)x.Y), (float)Quarter((double)x.Z), (float)Quarter((double)x.W));
public static Double2 Quarter(this Double2 x) => ((double)Quarter((double)x.X), (double)Quarter((double)x.Y));
public static Double3 Quarter(this Double3 x) => ((double)Quarter((double)x.X), (double)Quarter((double)x.Y), (double)Quarter((double)x.Z));
public static Double4 Quarter(this Double4 x) => ((double)Quarter((double)x.X), (double)Quarter((double)x.Y), (double)Quarter((double)x.Z), (double)Quarter((double)x.W));
public static Int2 Quarter(this Int2 x) => ((int)Quarter((double)x.A), (int)Quarter((double)x.B));
public static Int3 Quarter(this Int3 x) => ((int)Quarter((double)x.A), (int)Quarter((double)x.B), (int)Quarter((double)x.C));
public static Int4 Quarter(this Int4 x) => ((int)Quarter((double)x.A), (int)Quarter((double)x.B), (int)Quarter((double)x.C), (int)Quarter((double)x.D));
public static Long2 Quarter(this Long2 x) => ((long)Quarter((double)x.A), (long)Quarter((double)x.B));
public static Long3 Quarter(this Long3 x) => ((long)Quarter((double)x.A), (long)Quarter((double)x.B), (long)Quarter((double)x.C));
public static Long4 Quarter(this Long4 x) => ((long)Quarter((double)x.A), (long)Quarter((double)x.B), (long)Quarter((double)x.C), (long)Quarter((double)x.D));
public static Complex Quarter(this Complex x) => ((double)Quarter((double)x.Real), (double)Quarter((double)x.Imaginary));
public static double Twice(this double x)
{
return x * 2;
}
public static Float2 Twice(this Float2 x) => ((float)Twice((double)x.X), (float)Twice((double)x.Y));
public static Float3 Twice(this Float3 x) => ((float)Twice((double)x.X), (float)Twice((double)x.Y), (float)Twice((double)x.Z));
public static Float4 Twice(this Float4 x) => ((float)Twice((double)x.X), (float)Twice((double)x.Y), (float)Twice((double)x.Z), (float)Twice((double)x.W));
public static Double2 Twice(this Double2 x) => ((double)Twice((double)x.X), (double)Twice((double)x.Y));
public static Double3 Twice(this Double3 x) => ((double)Twice((double)x.X), (double)Twice((double)x.Y), (double)Twice((double)x.Z));
public static Double4 Twice(this Double4 x) => ((double)Twice((double)x.X), (double)Twice((double)x.Y), (double)Twice((double)x.Z), (double)Twice((double)x.W));
public static Int2 Twice(this Int2 x) => ((int)Twice((double)x.A), (int)Twice((double)x.B));
public static Int3 Twice(this Int3 x) => ((int)Twice((double)x.A), (int)Twice((double)x.B), (int)Twice((double)x.C));
public static Int4 Twice(this Int4 x) => ((int)Twice((double)x.A), (int)Twice((double)x.B), (int)Twice((double)x.C), (int)Twice((double)x.D));
public static Long2 Twice(this Long2 x) => ((long)Twice((double)x.A), (long)Twice((double)x.B));
public static Long3 Twice(this Long3 x) => ((long)Twice((double)x.A), (long)Twice((double)x.B), (long)Twice((double)x.C));
public static Long4 Twice(this Long4 x) => ((long)Twice((double)x.A), (long)Twice((double)x.B), (long)Twice((double)x.C), (long)Twice((double)x.D));
public static Complex Twice(this Complex x) => ((double)Twice((double)x.Real), (double)Twice((double)x.Imaginary));
public static double Thrice(this double x)
{
return x * 3;
}
public static Float2 Thrice(this Float2 x) => ((float)Thrice((double)x.X), (float)Thrice((double)x.Y));
public static Float3 Thrice(this Float3 x) => ((float)Thrice((double)x.X), (float)Thrice((double)x.Y), (float)Thrice((double)x.Z));
public static Float4 Thrice(this Float4 x) => ((float)Thrice((double)x.X), (float)Thrice((double)x.Y), (float)Thrice((double)x.Z), (float)Thrice((double)x.W));
public static Double2 Thrice(this Double2 x) => ((double)Thrice((double)x.X), (double)Thrice((double)x.Y));
public static Double3 Thrice(this Double3 x) => ((double)Thrice((double)x.X), (double)Thrice((double)x.Y), (double)Thrice((double)x.Z));
public static Double4 Thrice(this Double4 x) => ((double)Thrice((double)x.X), (double)Thrice((double)x.Y), (double)Thrice((double)x.Z), (double)Thrice((double)x.W));
public static Int2 Thrice(this Int2 x) => ((int)Thrice((double)x.A), (int)Thrice((double)x.B));
public static Int3 Thrice(this Int3 x) => ((int)Thrice((double)x.A), (int)Thrice((double)x.B), (int)Thrice((double)x.C));
public static Int4 Thrice(this Int4 x) => ((int)Thrice((double)x.A), (int)Thrice((double)x.B), (int)Thrice((double)x.C), (int)Thrice((double)x.D));
public static Long2 Thrice(this Long2 x) => ((long)Thrice((double)x.A), (long)Thrice((double)x.B));
public static Long3 Thrice(this Long3 x) => ((long)Thrice((double)x.A), (long)Thrice((double)x.B), (long)Thrice((double)x.C));
public static Long4 Thrice(this Long4 x) => ((long)Thrice((double)x.A), (long)Thrice((double)x.B), (long)Thrice((double)x.C), (long)Thrice((double)x.D));
public static Complex Thrice(this Complex x) => ((double)Thrice((double)x.Real), (double)Thrice((double)x.Imaginary));
public static double MinusOne(this double x)
{
return x - 1;
}
public static Float2 MinusOne(this Float2 x) => ((float)MinusOne((double)x.X), (float)MinusOne((double)x.Y));
public static Float3 MinusOne(this Float3 x) => ((float)MinusOne((double)x.X), (float)MinusOne((double)x.Y), (float)MinusOne((double)x.Z));
public static Float4 MinusOne(this Float4 x) => ((float)MinusOne((double)x.X), (float)MinusOne((double)x.Y), (float)MinusOne((double)x.Z), (float)MinusOne((double)x.W));
public static Double2 MinusOne(this Double2 x) => ((double)MinusOne((double)x.X), (double)MinusOne((double)x.Y));
public static Double3 MinusOne(this Double3 x) => ((double)MinusOne((double)x.X), (double)MinusOne((double)x.Y), (double)MinusOne((double)x.Z));
public static Double4 MinusOne(this Double4 x) => ((double)MinusOne((double)x.X), (double)MinusOne((double)x.Y), (double)MinusOne((double)x.Z), (double)MinusOne((double)x.W));
public static Int2 MinusOne(this Int2 x) => ((int)MinusOne((double)x.A), (int)MinusOne((double)x.B));
public static Int3 MinusOne(this Int3 x) => ((int)MinusOne((double)x.A), (int)MinusOne((double)x.B), (int)MinusOne((double)x.C));
public static Int4 MinusOne(this Int4 x) => ((int)MinusOne((double)x.A), (int)MinusOne((double)x.B), (int)MinusOne((double)x.C), (int)MinusOne((double)x.D));
public static Long2 MinusOne(this Long2 x) => ((long)MinusOne((double)x.A), (long)MinusOne((double)x.B));
public static Long3 MinusOne(this Long3 x) => ((long)MinusOne((double)x.A), (long)MinusOne((double)x.B), (long)MinusOne((double)x.C));
public static Long4 MinusOne(this Long4 x) => ((long)MinusOne((double)x.A), (long)MinusOne((double)x.B), (long)MinusOne((double)x.C), (long)MinusOne((double)x.D));
public static Complex MinusOne(this Complex x) => ((double)MinusOne((double)x.Real), (double)MinusOne((double)x.Imaginary));
public static double PlusOne(this double x)
{
return x + 1;
}
public static Float2 PlusOne(this Float2 x) => ((float)PlusOne((double)x.X), (float)PlusOne((double)x.Y));
public static Float3 PlusOne(this Float3 x) => ((float)PlusOne((double)x.X), (float)PlusOne((double)x.Y), (float)PlusOne((double)x.Z));
public static Float4 PlusOne(this Float4 x) => ((float)PlusOne((double)x.X), (float)PlusOne((double)x.Y), (float)PlusOne((double)x.Z), (float)PlusOne((double)x.W));
public static Double2 PlusOne(this Double2 x) => ((double)PlusOne((double)x.X), (double)PlusOne((double)x.Y));
public static Double3 PlusOne(this Double3 x) => ((double)PlusOne((double)x.X), (double)PlusOne((double)x.Y), (double)PlusOne((double)x.Z));
public static Double4 PlusOne(this Double4 x) => ((double)PlusOne((double)x.X), (double)PlusOne((double)x.Y), (double)PlusOne((double)x.Z), (double)PlusOne((double)x.W));
public static Int2 PlusOne(this Int2 x) => ((int)PlusOne((double)x.A), (int)PlusOne((double)x.B));
public static Int3 PlusOne(this Int3 x) => ((int)PlusOne((double)x.A), (int)PlusOne((double)x.B), (int)PlusOne((double)x.C));
public static Int4 PlusOne(this Int4 x) => ((int)PlusOne((double)x.A), (int)PlusOne((double)x.B), (int)PlusOne((double)x.C), (int)PlusOne((double)x.D));
public static Long2 PlusOne(this Long2 x) => ((long)PlusOne((double)x.A), (long)PlusOne((double)x.B));
public static Long3 PlusOne(this Long3 x) => ((long)PlusOne((double)x.A), (long)PlusOne((double)x.B), (long)PlusOne((double)x.C));
public static Long4 PlusOne(this Long4 x) => ((long)PlusOne((double)x.A), (long)PlusOne((double)x.B), (long)PlusOne((double)x.C), (long)PlusOne((double)x.D));
public static Complex PlusOne(this Complex x) => ((double)PlusOne((double)x.Real), (double)PlusOne((double)x.Imaginary));
public static double FromOne(this double x)
{
return 1 - x;
}
public static Float2 FromOne(this Float2 x) => ((float)FromOne((double)x.X), (float)FromOne((double)x.Y));
public static Float3 FromOne(this Float3 x) => ((float)FromOne((double)x.X), (float)FromOne((double)x.Y), (float)FromOne((double)x.Z));
public static Float4 FromOne(this Float4 x) => ((float)FromOne((double)x.X), (float)FromOne((double)x.Y), (float)FromOne((double)x.Z), (float)FromOne((double)x.W));
public static Double2 FromOne(this Double2 x) => ((double)FromOne((double)x.X), (double)FromOne((double)x.Y));
public static Double3 FromOne(this Double3 x) => ((double)FromOne((double)x.X), (double)FromOne((double)x.Y), (double)FromOne((double)x.Z));
public static Double4 FromOne(this Double4 x) => ((double)FromOne((double)x.X), (double)FromOne((double)x.Y), (double)FromOne((double)x.Z), (double)FromOne((double)x.W));
public static Int2 FromOne(this Int2 x) => ((int)FromOne((double)x.A), (int)FromOne((double)x.B));
public static Int3 FromOne(this Int3 x) => ((int)FromOne((double)x.A), (int)FromOne((double)x.B), (int)FromOne((double)x.C));
public static Int4 FromOne(this Int4 x) => ((int)FromOne((double)x.A), (int)FromOne((double)x.B), (int)FromOne((double)x.C), (int)FromOne((double)x.D));
public static Long2 FromOne(this Long2 x) => ((long)FromOne((double)x.A), (long)FromOne((double)x.B));
public static Long3 FromOne(this Long3 x) => ((long)FromOne((double)x.A), (long)FromOne((double)x.B), (long)FromOne((double)x.C));
public static Long4 FromOne(this Long4 x) => ((long)FromOne((double)x.A), (long)FromOne((double)x.B), (long)FromOne((double)x.C), (long)FromOne((double)x.D));
public static Complex FromOne(this Complex x) => ((double)FromOne((double)x.Real), (double)FromOne((double)x.Imaginary));
public static double Abs(this double x)
{
return Math.Abs(x);
}
public static Float2 Abs(this Float2 x) => ((float)Abs((double)x.X), (float)Abs((double)x.Y));
public static Float3 Abs(this Float3 x) => ((float)Abs((double)x.X), (float)Abs((double)x.Y), (float)Abs((double)x.Z));
public static Float4 Abs(this Float4 x) => ((float)Abs((double)x.X), (float)Abs((double)x.Y), (float)Abs((double)x.Z), (float)Abs((double)x.W));
public static Double2 Abs(this Double2 x) => ((double)Abs((double)x.X), (double)Abs((double)x.Y));
public static Double3 Abs(this Double3 x) => ((double)Abs((double)x.X), (double)Abs((double)x.Y), (double)Abs((double)x.Z));
public static Double4 Abs(this Double4 x) => ((double)Abs((double)x.X), (double)Abs((double)x.Y), (double)Abs((double)x.Z), (double)Abs((double)x.W));
public static Int2 Abs(this Int2 x) => ((int)Abs((double)x.A), (int)Abs((double)x.B));
public static Int3 Abs(this Int3 x) => ((int)Abs((double)x.A), (int)Abs((double)x.B), (int)Abs((double)x.C));
public static Int4 Abs(this Int4 x) => ((int)Abs((double)x.A), (int)Abs((double)x.B), (int)Abs((double)x.C), (int)Abs((double)x.D));
public static Long2 Abs(this Long2 x) => ((long)Abs((double)x.A), (long)Abs((double)x.B));
public static Long3 Abs(this Long3 x) => ((long)Abs((double)x.A), (long)Abs((double)x.B), (long)Abs((double)x.C));
public static Long4 Abs(this Long4 x) => ((long)Abs((double)x.A), (long)Abs((double)x.B), (long)Abs((double)x.C), (long)Abs((double)x.D));
public static Complex Abs(this Complex x) => ((double)Abs((double)x.Real), (double)Abs((double)x.Imaginary));
public static double Exp(this double x)
{
return Math.Exp(x);
}
public static Float2 Exp(this Float2 x) => ((float)Exp((double)x.X), (float)Exp((double)x.Y));
public static Float3 Exp(this Float3 x) => ((float)Exp((double)x.X), (float)Exp((double)x.Y), (float)Exp((double)x.Z));
public static Float4 Exp(this Float4 x) => ((float)Exp((double)x.X), (float)Exp((double)x.Y), (float)Exp((double)x.Z), (float)Exp((double)x.W));
public static Double2 Exp(this Double2 x) => ((double)Exp((double)x.X), (double)Exp((double)x.Y));
public static Double3 Exp(this Double3 x) => ((double)Exp((double)x.X), (double)Exp((double)x.Y), (double)Exp((double)x.Z));
public static Double4 Exp(this Double4 x) => ((double)Exp((double)x.X), (double)Exp((double)x.Y), (double)Exp((double)x.Z), (double)Exp((double)x.W));
public static Int2 Exp(this Int2 x) => ((int)Exp((double)x.A), (int)Exp((double)x.B));
public static Int3 Exp(this Int3 x) => ((int)Exp((double)x.A), (int)Exp((double)x.B), (int)Exp((double)x.C));
public static Int4 Exp(this Int4 x) => ((int)Exp((double)x.A), (int)Exp((double)x.B), (int)Exp((double)x.C), (int)Exp((double)x.D));
public static Long2 Exp(this Long2 x) => ((long)Exp((double)x.A), (long)Exp((double)x.B));
public static Long3 Exp(this Long3 x) => ((long)Exp((double)x.A), (long)Exp((double)x.B), (long)Exp((double)x.C));
public static Long4 Exp(this Long4 x) => ((long)Exp((double)x.A), (long)Exp((double)x.B), (long)Exp((double)x.C), (long)Exp((double)x.D));
public static Complex Exp(this Complex x) => ((double)Exp((double)x.Real), (double)Exp((double)x.Imaginary));
public static double Log(this double x)
{
return Math.Log(x);
}
public static Float2 Log(this Float2 x) => ((float)Log((double)x.X), (float)Log((double)x.Y));
public static Float3 Log(this Float3 x) => ((float)Log((double)x.X), (float)Log((double)x.Y), (float)Log((double)x.Z));
public static Float4 Log(this Float4 x) => ((float)Log((double)x.X), (float)Log((double)x.Y), (float)Log((double)x.Z), (float)Log((double)x.W));
public static Double2 Log(this Double2 x) => ((double)Log((double)x.X), (double)Log((double)x.Y));
public static Double3 Log(this Double3 x) => ((double)Log((double)x.X), (double)Log((double)x.Y), (double)Log((double)x.Z));
public static Double4 Log(this Double4 x) => ((double)Log((double)x.X), (double)Log((double)x.Y), (double)Log((double)x.Z), (double)Log((double)x.W));
public static Int2 Log(this Int2 x) => ((int)Log((double)x.A), (int)Log((double)x.B));
public static Int3 Log(this Int3 x) => ((int)Log((double)x.A), (int)Log((double)x.B), (int)Log((double)x.C));
public static Int4 Log(this Int4 x) => ((int)Log((double)x.A), (int)Log((double)x.B), (int)Log((double)x.C), (int)Log((double)x.D));
public static Long2 Log(this Long2 x) => ((long)Log((double)x.A), (long)Log((double)x.B));
public static Long3 Log(this Long3 x) => ((long)Log((double)x.A), (long)Log((double)x.B), (long)Log((double)x.C));
public static Long4 Log(this Long4 x) => ((long)Log((double)x.A), (long)Log((double)x.B), (long)Log((double)x.C), (long)Log((double)x.D));
public static Complex Log(this Complex x) => ((double)Log((double)x.Real), (double)Log((double)x.Imaginary));
public static double Log10(this double x)
{
return Math.Log10(x);
}
public static Float2 Log10(this Float2 x) => ((float)Log10((double)x.X), (float)Log10((double)x.Y));
public static Float3 Log10(this Float3 x) => ((float)Log10((double)x.X), (float)Log10((double)x.Y), (float)Log10((double)x.Z));
public static Float4 Log10(this Float4 x) => ((float)Log10((double)x.X), (float)Log10((double)x.Y), (float)Log10((double)x.Z), (float)Log10((double)x.W));
public static Double2 Log10(this Double2 x) => ((double)Log10((double)x.X), (double)Log10((double)x.Y));
public static Double3 Log10(this Double3 x) => ((double)Log10((double)x.X), (double)Log10((double)x.Y), (double)Log10((double)x.Z));
public static Double4 Log10(this Double4 x) => ((double)Log10((double)x.X), (double)Log10((double)x.Y), (double)Log10((double)x.Z), (double)Log10((double)x.W));
public static Int2 Log10(this Int2 x) => ((int)Log10((double)x.A), (int)Log10((double)x.B));
public static Int3 Log10(this Int3 x) => ((int)Log10((double)x.A), (int)Log10((double)x.B), (int)Log10((double)x.C));
public static Int4 Log10(this Int4 x) => ((int)Log10((double)x.A), (int)Log10((double)x.B), (int)Log10((double)x.C), (int)Log10((double)x.D));
public static Long2 Log10(this Long2 x) => ((long)Log10((double)x.A), (long)Log10((double)x.B));
public static Long3 Log10(this Long3 x) => ((long)Log10((double)x.A), (long)Log10((double)x.B), (long)Log10((double)x.C));
public static Long4 Log10(this Long4 x) => ((long)Log10((double)x.A), (long)Log10((double)x.B), (long)Log10((double)x.C), (long)Log10((double)x.D));
public static Complex Log10(this Complex x) => ((double)Log10((double)x.Real), (double)Log10((double)x.Imaginary));
public static double Sqrt(this double x)
{
return Math.Sqrt(x);
}
public static Float2 Sqrt(this Float2 x) => ((float)Sqrt((double)x.X), (float)Sqrt((double)x.Y));
public static Float3 Sqrt(this Float3 x) => ((float)Sqrt((double)x.X), (float)Sqrt((double)x.Y), (float)Sqrt((double)x.Z));
public static Float4 Sqrt(this Float4 x) => ((float)Sqrt((double)x.X), (float)Sqrt((double)x.Y), (float)Sqrt((double)x.Z), (float)Sqrt((double)x.W));
public static Double2 Sqrt(this Double2 x) => ((double)Sqrt((double)x.X), (double)Sqrt((double)x.Y));
public static Double3 Sqrt(this Double3 x) => ((double)Sqrt((double)x.X), (double)Sqrt((double)x.Y), (double)Sqrt((double)x.Z));
public static Double4 Sqrt(this Double4 x) => ((double)Sqrt((double)x.X), (double)Sqrt((double)x.Y), (double)Sqrt((double)x.Z), (double)Sqrt((double)x.W));
public static Int2 Sqrt(this Int2 x) => ((int)Sqrt((double)x.A), (int)Sqrt((double)x.B));
public static Int3 Sqrt(this Int3 x) => ((int)Sqrt((double)x.A), (int)Sqrt((double)x.B), (int)Sqrt((double)x.C));
public static Int4 Sqrt(this Int4 x) => ((int)Sqrt((double)x.A), (int)Sqrt((double)x.B), (int)Sqrt((double)x.C), (int)Sqrt((double)x.D));
public static Long2 Sqrt(this Long2 x) => ((long)Sqrt((double)x.A), (long)Sqrt((double)x.B));
public static Long3 Sqrt(this Long3 x) => ((long)Sqrt((double)x.A), (long)Sqrt((double)x.B), (long)Sqrt((double)x.C));
public static Long4 Sqrt(this Long4 x) => ((long)Sqrt((double)x.A), (long)Sqrt((double)x.B), (long)Sqrt((double)x.C), (long)Sqrt((double)x.D));
public static Complex Sqrt(this Complex x) => ((double)Sqrt((double)x.Real), (double)Sqrt((double)x.Imaginary));
public static double Sign(this double x)
{
return x > 0 ? 1 : x < 0 ? -1 : 0;
}
public static Float2 Sign(this Float2 x) => ((float)Sign((double)x.X), (float)Sign((double)x.Y));
public static Float3 Sign(this Float3 x) => ((float)Sign((double)x.X), (float)Sign((double)x.Y), (float)Sign((double)x.Z));
public static Float4 Sign(this Float4 x) => ((float)Sign((double)x.X), (float)Sign((double)x.Y), (float)Sign((double)x.Z), (float)Sign((double)x.W));
public static Double2 Sign(this Double2 x) => ((double)Sign((double)x.X), (double)Sign((double)x.Y));
public static Double3 Sign(this Double3 x) => ((double)Sign((double)x.X), (double)Sign((double)x.Y), (double)Sign((double)x.Z));
public static Double4 Sign(this Double4 x) => ((double)Sign((double)x.X), (double)Sign((double)x.Y), (double)Sign((double)x.Z), (double)Sign((double)x.W));
public static Int2 Sign(this Int2 x) => ((int)Sign((double)x.A), (int)Sign((double)x.B));
public static Int3 Sign(this Int3 x) => ((int)Sign((double)x.A), (int)Sign((double)x.B), (int)Sign((double)x.C));
public static Int4 Sign(this Int4 x) => ((int)Sign((double)x.A), (int)Sign((double)x.B), (int)Sign((double)x.C), (int)Sign((double)x.D));
public static Long2 Sign(this Long2 x) => ((long)Sign((double)x.A), (long)Sign((double)x.B));
public static Long3 Sign(this Long3 x) => ((long)Sign((double)x.A), (long)Sign((double)x.B), (long)Sign((double)x.C));
public static Long4 Sign(this Long4 x) => ((long)Sign((double)x.A), (long)Sign((double)x.B), (long)Sign((double)x.C), (long)Sign((double)x.D));
public static Complex Sign(this Complex x) => ((double)Sign((double)x.Real), (double)Sign((double)x.Imaginary));
public static double Inverse(this double x)
{
return 1 / x;
}
public static Float2 Inverse(this Float2 x) => ((float)Inverse((double)x.X), (float)Inverse((double)x.Y));
public static Float3 Inverse(this Float3 x) => ((float)Inverse((double)x.X), (float)Inverse((double)x.Y), (float)Inverse((double)x.Z));
public static Float4 Inverse(this Float4 x) => ((float)Inverse((double)x.X), (float)Inverse((double)x.Y), (float)Inverse((double)x.Z), (float)Inverse((double)x.W));
public static Double2 Inverse(this Double2 x) => ((double)Inverse((double)x.X), (double)Inverse((double)x.Y));
public static Double3 Inverse(this Double3 x) => ((double)Inverse((double)x.X), (double)Inverse((double)x.Y), (double)Inverse((double)x.Z));
public static Double4 Inverse(this Double4 x) => ((double)Inverse((double)x.X), (double)Inverse((double)x.Y), (double)Inverse((double)x.Z), (double)Inverse((double)x.W));
public static Int2 Inverse(this Int2 x) => ((int)Inverse((double)x.A), (int)Inverse((double)x.B));
public static Int3 Inverse(this Int3 x) => ((int)Inverse((double)x.A), (int)Inverse((double)x.B), (int)Inverse((double)x.C));
public static Int4 Inverse(this Int4 x) => ((int)Inverse((double)x.A), (int)Inverse((double)x.B), (int)Inverse((double)x.C), (int)Inverse((double)x.D));
public static Long2 Inverse(this Long2 x) => ((long)Inverse((double)x.A), (long)Inverse((double)x.B));
public static Long3 Inverse(this Long3 x) => ((long)Inverse((double)x.A), (long)Inverse((double)x.B), (long)Inverse((double)x.C));
public static Long4 Inverse(this Long4 x) => ((long)Inverse((double)x.A), (long)Inverse((double)x.B), (long)Inverse((double)x.C), (long)Inverse((double)x.D));
public static Complex Inverse(this Complex x) => ((double)Inverse((double)x.Real), (double)Inverse((double)x.Imaginary));
public static double Truncate(this double x)
{
return Math.Truncate(x);
}
public static Float2 Truncate(this Float2 x) => ((float)Truncate((double)x.X), (float)Truncate((double)x.Y));
public static Float3 Truncate(this Float3 x) => ((float)Truncate((double)x.X), (float)Truncate((double)x.Y), (float)Truncate((double)x.Z));
public static Float4 Truncate(this Float4 x) => ((float)Truncate((double)x.X), (float)Truncate((double)x.Y), (float)Truncate((double)x.Z), (float)Truncate((double)x.W));
public static Double2 Truncate(this Double2 x) => ((double)Truncate((double)x.X), (double)Truncate((double)x.Y));
public static Double3 Truncate(this Double3 x) => ((double)Truncate((double)x.X), (double)Truncate((double)x.Y), (double)Truncate((double)x.Z));
public static Double4 Truncate(this Double4 x) => ((double)Truncate((double)x.X), (double)Truncate((double)x.Y), (double)Truncate((double)x.Z), (double)Truncate((double)x.W));
public static Int2 Truncate(this Int2 x) => ((int)Truncate((double)x.A), (int)Truncate((double)x.B));
public static Int3 Truncate(this Int3 x) => ((int)Truncate((double)x.A), (int)Truncate((double)x.B), (int)Truncate((double)x.C));
public static Int4 Truncate(this Int4 x) => ((int)Truncate((double)x.A), (int)Truncate((double)x.B), (int)Truncate((double)x.C), (int)Truncate((double)x.D));
public static Long2 Truncate(this Long2 x) => ((long)Truncate((double)x.A), (long)Truncate((double)x.B));
public static Long3 Truncate(this Long3 x) => ((long)Truncate((double)x.A), (long)Truncate((double)x.B), (long)Truncate((double)x.C));
public static Long4 Truncate(this Long4 x) => ((long)Truncate((double)x.A), (long)Truncate((double)x.B), (long)Truncate((double)x.C), (long)Truncate((double)x.D));
public static Complex Truncate(this Complex x) => ((double)Truncate((double)x.Real), (double)Truncate((double)x.Imaginary));
public static double Ceiling(this double x)
{
return Math.Ceiling(x);
}
public static Float2 Ceiling(this Float2 x) => ((float)Ceiling((double)x.X), (float)Ceiling((double)x.Y));
public static Float3 Ceiling(this Float3 x) => ((float)Ceiling((double)x.X), (float)Ceiling((double)x.Y), (float)Ceiling((double)x.Z));
public static Float4 Ceiling(this Float4 x) => ((float)Ceiling((double)x.X), (float)Ceiling((double)x.Y), (float)Ceiling((double)x.Z), (float)Ceiling((double)x.W));
public static Double2 Ceiling(this Double2 x) => ((double)Ceiling((double)x.X), (double)Ceiling((double)x.Y));
public static Double3 Ceiling(this Double3 x) => ((double)Ceiling((double)x.X), (double)Ceiling((double)x.Y), (double)Ceiling((double)x.Z));
public static Double4 Ceiling(this Double4 x) => ((double)Ceiling((double)x.X), (double)Ceiling((double)x.Y), (double)Ceiling((double)x.Z), (double)Ceiling((double)x.W));
public static Int2 Ceiling(this Int2 x) => ((int)Ceiling((double)x.A), (int)Ceiling((double)x.B));
public static Int3 Ceiling(this Int3 x) => ((int)Ceiling((double)x.A), (int)Ceiling((double)x.B), (int)Ceiling((double)x.C));
public static Int4 Ceiling(this Int4 x) => ((int)Ceiling((double)x.A), (int)Ceiling((double)x.B), (int)Ceiling((double)x.C), (int)Ceiling((double)x.D));
public static Long2 Ceiling(this Long2 x) => ((long)Ceiling((double)x.A), (long)Ceiling((double)x.B));
public static Long3 Ceiling(this Long3 x) => ((long)Ceiling((double)x.A), (long)Ceiling((double)x.B), (long)Ceiling((double)x.C));
public static Long4 Ceiling(this Long4 x) => ((long)Ceiling((double)x.A), (long)Ceiling((double)x.B), (long)Ceiling((double)x.C), (long)Ceiling((double)x.D));
public static Complex Ceiling(this Complex x) => ((double)Ceiling((double)x.Real), (double)Ceiling((double)x.Imaginary));
public static double Floor(this double x)
{
return Math.Floor(x);
}
public static Float2 Floor(this Float2 x) => ((float)Floor((double)x.X), (float)Floor((double)x.Y));
public static Float3 Floor(this Float3 x) => ((float)Floor((double)x.X), (float)Floor((double)x.Y), (float)Floor((double)x.Z));
public static Float4 Floor(this Float4 x) => ((float)Floor((double)x.X), (float)Floor((double)x.Y), (float)Floor((double)x.Z), (float)Floor((double)x.W));
public static Double2 Floor(this Double2 x) => ((double)Floor((double)x.X), (double)Floor((double)x.Y));
public static Double3 Floor(this Double3 x) => ((double)Floor((double)x.X), (double)Floor((double)x.Y), (double)Floor((double)x.Z));
public static Double4 Floor(this Double4 x) => ((double)Floor((double)x.X), (double)Floor((double)x.Y), (double)Floor((double)x.Z), (double)Floor((double)x.W));
public static Int2 Floor(this Int2 x) => ((int)Floor((double)x.A), (int)Floor((double)x.B));
public static Int3 Floor(this Int3 x) => ((int)Floor((double)x.A), (int)Floor((double)x.B), (int)Floor((double)x.C));
public static Int4 Floor(this Int4 x) => ((int)Floor((double)x.A), (int)Floor((double)x.B), (int)Floor((double)x.C), (int)Floor((double)x.D));
public static Long2 Floor(this Long2 x) => ((long)Floor((double)x.A), (long)Floor((double)x.B));
public static Long3 Floor(this Long3 x) => ((long)Floor((double)x.A), (long)Floor((double)x.B), (long)Floor((double)x.C));
public static Long4 Floor(this Long4 x) => ((long)Floor((double)x.A), (long)Floor((double)x.B), (long)Floor((double)x.C), (long)Floor((double)x.D));
public static Complex Floor(this Complex x) => ((double)Floor((double)x.Real), (double)Floor((double)x.Imaginary));
public static double Round(this double x)
{
return Math.Round(x);
}
public static Float2 Round(this Float2 x) => ((float)Round((double)x.X), (float)Round((double)x.Y));
public static Float3 Round(this Float3 x) => ((float)Round((double)x.X), (float)Round((double)x.Y), (float)Round((double)x.Z));
public static Float4 Round(this Float4 x) => ((float)Round((double)x.X), (float)Round((double)x.Y), (float)Round((double)x.Z), (float)Round((double)x.W));
public static Double2 Round(this Double2 x) => ((double)Round((double)x.X), (double)Round((double)x.Y));
public static Double3 Round(this Double3 x) => ((double)Round((double)x.X), (double)Round((double)x.Y), (double)Round((double)x.Z));
public static Double4 Round(this Double4 x) => ((double)Round((double)x.X), (double)Round((double)x.Y), (double)Round((double)x.Z), (double)Round((double)x.W));
public static Int2 Round(this Int2 x) => ((int)Round((double)x.A), (int)Round((double)x.B));
public static Int3 Round(this Int3 x) => ((int)Round((double)x.A), (int)Round((double)x.B), (int)Round((double)x.C));
public static Int4 Round(this Int4 x) => ((int)Round((double)x.A), (int)Round((double)x.B), (int)Round((double)x.C), (int)Round((double)x.D));
public static Long2 Round(this Long2 x) => ((long)Round((double)x.A), (long)Round((double)x.B));
public static Long3 Round(this Long3 x) => ((long)Round((double)x.A), (long)Round((double)x.B), (long)Round((double)x.C));
public static Long4 Round(this Long4 x) => ((long)Round((double)x.A), (long)Round((double)x.B), (long)Round((double)x.C), (long)Round((double)x.D));
public static Complex Round(this Complex x) => ((double)Round((double)x.Real), (double)Round((double)x.Imaginary));
public static double Smoothstep(this double v)
{
return v.Pow2() * (3 - 2 * v);
}
public static Float2 Smoothstep(this Float2 v) => ((float)Smoothstep((double)v.X), (float)Smoothstep((double)v.Y));
public static Float3 Smoothstep(this Float3 v) => ((float)Smoothstep((double)v.X), (float)Smoothstep((double)v.Y), (float)Smoothstep((double)v.Z));
public static Float4 Smoothstep(this Float4 v) => ((float)Smoothstep((double)v.X), (float)Smoothstep((double)v.Y), (float)Smoothstep((double)v.Z), (float)Smoothstep((double)v.W));
public static Double2 Smoothstep(this Double2 v) => ((double)Smoothstep((double)v.X), (double)Smoothstep((double)v.Y));
public static Double3 Smoothstep(this Double3 v) => ((double)Smoothstep((double)v.X), (double)Smoothstep((double)v.Y), (double)Smoothstep((double)v.Z));
public static Double4 Smoothstep(this Double4 v) => ((double)Smoothstep((double)v.X), (double)Smoothstep((double)v.Y), (double)Smoothstep((double)v.Z), (double)Smoothstep((double)v.W));
public static Int2 Smoothstep(this Int2 v) => ((int)Smoothstep((double)v.A), (int)Smoothstep((double)v.B));
public static Int3 Smoothstep(this Int3 v) => ((int)Smoothstep((double)v.A), (int)Smoothstep((double)v.B), (int)Smoothstep((double)v.C));
public static Int4 Smoothstep(this Int4 v) => ((int)Smoothstep((double)v.A), (int)Smoothstep((double)v.B), (int)Smoothstep((double)v.C), (int)Smoothstep((double)v.D));
public static Long2 Smoothstep(this Long2 v) => ((long)Smoothstep((double)v.A), (long)Smoothstep((double)v.B));
public static Long3 Smoothstep(this Long3 v) => ((long)Smoothstep((double)v.A), (long)Smoothstep((double)v.B), (long)Smoothstep((double)v.C));
public static Long4 Smoothstep(this Long4 v) => ((long)Smoothstep((double)v.A), (long)Smoothstep((double)v.B), (long)Smoothstep((double)v.C), (long)Smoothstep((double)v.D));
public static Complex Smoothstep(this Complex v) => ((double)Smoothstep((double)v.Real), (double)Smoothstep((double)v.Imaginary));
public static double Lerp(this double v1, double v2, double t)
{
return v1 * (1 - t) + v2 * t;
}
public static Float2 Lerp(this Float2 v1, Float2 v2, Float2 t) => ((float)Lerp((double)v1.X, (double)v2.X, (double)t.X), (float)Lerp((double)v1.Y, (double)v2.Y, (double)t.Y));
public static Float3 Lerp(this Float3 v1, Float3 v2, Float3 t) => ((float)Lerp((double)v1.X, (double)v2.X, (double)t.X), (float)Lerp((double)v1.Y, (double)v2.Y, (double)t.Y), (float)Lerp((double)v1.Z, (double)v2.Z, (double)t.Z));
public static Float4 Lerp(this Float4 v1, Float4 v2, Float4 t) => ((float)Lerp((double)v1.X, (double)v2.X, (double)t.X), (float)Lerp((double)v1.Y, (double)v2.Y, (double)t.Y), (float)Lerp((double)v1.Z, (double)v2.Z, (double)t.Z), (float)Lerp((double)v1.W, (double)v2.W, (double)t.W));
public static Double2 Lerp(this Double2 v1, Double2 v2, Double2 t) => ((double)Lerp((double)v1.X, (double)v2.X, (double)t.X), (double)Lerp((double)v1.Y, (double)v2.Y, (double)t.Y));
public static Double3 Lerp(this Double3 v1, Double3 v2, Double3 t) => ((double)Lerp((double)v1.X, (double)v2.X, (double)t.X), (double)Lerp((double)v1.Y, (double)v2.Y, (double)t.Y), (double)Lerp((double)v1.Z, (double)v2.Z, (double)t.Z));
public static Double4 Lerp(this Double4 v1, Double4 v2, Double4 t) => ((double)Lerp((double)v1.X, (double)v2.X, (double)t.X), (double)Lerp((double)v1.Y, (double)v2.Y, (double)t.Y), (double)Lerp((double)v1.Z, (double)v2.Z, (double)t.Z), (double)Lerp((double)v1.W, (double)v2.W, (double)t.W));
public static Int2 Lerp(this Int2 v1, Int2 v2, Int2 t) => ((int)Lerp((double)v1.A, (double)v2.A, (double)t.A), (int)Lerp((double)v1.B, (double)v2.B, (double)t.B));
public static Int3 Lerp(this Int3 v1, Int3 v2, Int3 t) => ((int)Lerp((double)v1.A, (double)v2.A, (double)t.A), (int)Lerp((double)v1.B, (double)v2.B, (double)t.B), (int)Lerp((double)v1.C, (double)v2.C, (double)t.C));
public static Int4 Lerp(this Int4 v1, Int4 v2, Int4 t) => ((int)Lerp((double)v1.A, (double)v2.A, (double)t.A), (int)Lerp((double)v1.B, (double)v2.B, (double)t.B), (int)Lerp((double)v1.C, (double)v2.C, (double)t.C), (int)Lerp((double)v1.D, (double)v2.D, (double)t.D));
public static Long2 Lerp(this Long2 v1, Long2 v2, Long2 t) => ((long)Lerp((double)v1.A, (double)v2.A, (double)t.A), (long)Lerp((double)v1.B, (double)v2.B, (double)t.B));
public static Long3 Lerp(this Long3 v1, Long3 v2, Long3 t) => ((long)Lerp((double)v1.A, (double)v2.A, (double)t.A), (long)Lerp((double)v1.B, (double)v2.B, (double)t.B), (long)Lerp((double)v1.C, (double)v2.C, (double)t.C));
public static Long4 Lerp(this Long4 v1, Long4 v2, Long4 t) => ((long)Lerp((double)v1.A, (double)v2.A, (double)t.A), (long)Lerp((double)v1.B, (double)v2.B, (double)t.B), (long)Lerp((double)v1.C, (double)v2.C, (double)t.C), (long)Lerp((double)v1.D, (double)v2.D, (double)t.D));
public static Complex Lerp(this Complex v1, Complex v2, Complex t) => ((double)Lerp((double)v1.Real, (double)v2.Real, (double)t.Real), (double)Lerp((double)v1.Imaginary, (double)v2.Imaginary, (double)t.Imaginary));
public static double Mix(this double v1, double v2, double t)
{
return Lerp(v1, v2, t);
}
public static Float2 Mix(this Float2 v1, Float2 v2, Float2 t) => ((float)Mix((double)v1.X, (double)v2.X, (double)t.X), (float)Mix((double)v1.Y, (double)v2.Y, (double)t.Y));
public static Float3 Mix(this Float3 v1, Float3 v2, Float3 t) => ((float)Mix((double)v1.X, (double)v2.X, (double)t.X), (float)Mix((double)v1.Y, (double)v2.Y, (double)t.Y), (float)Mix((double)v1.Z, (double)v2.Z, (double)t.Z));
public static Float4 Mix(this Float4 v1, Float4 v2, Float4 t) => ((float)Mix((double)v1.X, (double)v2.X, (double)t.X), (float)Mix((double)v1.Y, (double)v2.Y, (double)t.Y), (float)Mix((double)v1.Z, (double)v2.Z, (double)t.Z), (float)Mix((double)v1.W, (double)v2.W, (double)t.W));
public static Double2 Mix(this Double2 v1, Double2 v2, Double2 t) => ((double)Mix((double)v1.X, (double)v2.X, (double)t.X), (double)Mix((double)v1.Y, (double)v2.Y, (double)t.Y));
public static Double3 Mix(this Double3 v1, Double3 v2, Double3 t) => ((double)Mix((double)v1.X, (double)v2.X, (double)t.X), (double)Mix((double)v1.Y, (double)v2.Y, (double)t.Y), (double)Mix((double)v1.Z, (double)v2.Z, (double)t.Z));
public static Double4 Mix(this Double4 v1, Double4 v2, Double4 t) => ((double)Mix((double)v1.X, (double)v2.X, (double)t.X), (double)Mix((double)v1.Y, (double)v2.Y, (double)t.Y), (double)Mix((double)v1.Z, (double)v2.Z, (double)t.Z), (double)Mix((double)v1.W, (double)v2.W, (double)t.W));
public static Int2 Mix(this Int2 v1, Int2 v2, Int2 t) => ((int)Mix((double)v1.A, (double)v2.A, (double)t.A), (int)Mix((double)v1.B, (double)v2.B, (double)t.B));
public static Int3 Mix(this Int3 v1, Int3 v2, Int3 t) => ((int)Mix((double)v1.A, (double)v2.A, (double)t.A), (int)Mix((double)v1.B, (double)v2.B, (double)t.B), (int)Mix((double)v1.C, (double)v2.C, (double)t.C));
public static Int4 Mix(this Int4 v1, Int4 v2, Int4 t) => ((int)Mix((double)v1.A, (double)v2.A, (double)t.A), (int)Mix((double)v1.B, (double)v2.B, (double)t.B), (int)Mix((double)v1.C, (double)v2.C, (double)t.C), (int)Mix((double)v1.D, (double)v2.D, (double)t.D));
public static Long2 Mix(this Long2 v1, Long2 v2, Long2 t) => ((long)Mix((double)v1.A, (double)v2.A, (double)t.A), (long)Mix((double)v1.B, (double)v2.B, (double)t.B));
public static Long3 Mix(this Long3 v1, Long3 v2, Long3 t) => ((long)Mix((double)v1.A, (double)v2.A, (double)t.A), (long)Mix((double)v1.B, (double)v2.B, (double)t.B), (long)Mix((double)v1.C, (double)v2.C, (double)t.C));
public static Long4 Mix(this Long4 v1, Long4 v2, Long4 t) => ((long)Mix((double)v1.A, (double)v2.A, (double)t.A), (long)Mix((double)v1.B, (double)v2.B, (double)t.B), (long)Mix((double)v1.C, (double)v2.C, (double)t.C), (long)Mix((double)v1.D, (double)v2.D, (double)t.D));
public static Complex Mix(this Complex v1, Complex v2, Complex t) => ((double)Mix((double)v1.Real, (double)v2.Real, (double)t.Real), (double)Mix((double)v1.Imaginary, (double)v2.Imaginary, (double)t.Imaginary));
public static double InverseLerp(this double v, double a, double b)
{
return (v - a) / (b - a);
}
public static Float2 InverseLerp(this Float2 v, Float2 a, Float2 b) => ((float)InverseLerp((double)v.X, (double)a.X, (double)b.X), (float)InverseLerp((double)v.Y, (double)a.Y, (double)b.Y));
public static Float3 InverseLerp(this Float3 v, Float3 a, Float3 b) => ((float)InverseLerp((double)v.X, (double)a.X, (double)b.X), (float)InverseLerp((double)v.Y, (double)a.Y, (double)b.Y), (float)InverseLerp((double)v.Z, (double)a.Z, (double)b.Z));
public static Float4 InverseLerp(this Float4 v, Float4 a, Float4 b) => ((float)InverseLerp((double)v.X, (double)a.X, (double)b.X), (float)InverseLerp((double)v.Y, (double)a.Y, (double)b.Y), (float)InverseLerp((double)v.Z, (double)a.Z, (double)b.Z), (float)InverseLerp((double)v.W, (double)a.W, (double)b.W));
public static Double2 InverseLerp(this Double2 v, Double2 a, Double2 b) => ((double)InverseLerp((double)v.X, (double)a.X, (double)b.X), (double)InverseLerp((double)v.Y, (double)a.Y, (double)b.Y));
public static Double3 InverseLerp(this Double3 v, Double3 a, Double3 b) => ((double)InverseLerp((double)v.X, (double)a.X, (double)b.X), (double)InverseLerp((double)v.Y, (double)a.Y, (double)b.Y), (double)InverseLerp((double)v.Z, (double)a.Z, (double)b.Z));
public static Double4 InverseLerp(this Double4 v, Double4 a, Double4 b) => ((double)InverseLerp((double)v.X, (double)a.X, (double)b.X), (double)InverseLerp((double)v.Y, (double)a.Y, (double)b.Y), (double)InverseLerp((double)v.Z, (double)a.Z, (double)b.Z), (double)InverseLerp((double)v.W, (double)a.W, (double)b.W));
public static Int2 InverseLerp(this Int2 v, Int2 a, Int2 b) => ((int)InverseLerp((double)v.A, (double)a.A, (double)b.A), (int)InverseLerp((double)v.B, (double)a.B, (double)b.B));
public static Int3 InverseLerp(this Int3 v, Int3 a, Int3 b) => ((int)InverseLerp((double)v.A, (double)a.A, (double)b.A), (int)InverseLerp((double)v.B, (double)a.B, (double)b.B), (int)InverseLerp((double)v.C, (double)a.C, (double)b.C));
public static Int4 InverseLerp(this Int4 v, Int4 a, Int4 b) => ((int)InverseLerp((double)v.A, (double)a.A, (double)b.A), (int)InverseLerp((double)v.B, (double)a.B, (double)b.B), (int)InverseLerp((double)v.C, (double)a.C, (double)b.C), (int)InverseLerp((double)v.D, (double)a.D, (double)b.D));
public static Long2 InverseLerp(this Long2 v, Long2 a, Long2 b) => ((long)InverseLerp((double)v.A, (double)a.A, (double)b.A), (long)InverseLerp((double)v.B, (double)a.B, (double)b.B));
public static Long3 InverseLerp(this Long3 v, Long3 a, Long3 b) => ((long)InverseLerp((double)v.A, (double)a.A, (double)b.A), (long)InverseLerp((double)v.B, (double)a.B, (double)b.B), (long)InverseLerp((double)v.C, (double)a.C, (double)b.C));
public static Long4 InverseLerp(this Long4 v, Long4 a, Long4 b) => ((long)InverseLerp((double)v.A, (double)a.A, (double)b.A), (long)InverseLerp((double)v.B, (double)a.B, (double)b.B), (long)InverseLerp((double)v.C, (double)a.C, (double)b.C), (long)InverseLerp((double)v.D, (double)a.D, (double)b.D));
public static Complex InverseLerp(this Complex v, Complex a, Complex b) => ((double)InverseLerp((double)v.Real, (double)a.Real, (double)b.Real), (double)InverseLerp((double)v.Imaginary, (double)a.Imaginary, (double)b.Imaginary));
public static double Unmix(this double v, double a, double b)
{
return InverseLerp(v, a, b);
}
public static Float2 Unmix(this Float2 v, Float2 a, Float2 b) => ((float)Unmix((double)v.X, (double)a.X, (double)b.X), (float)Unmix((double)v.Y, (double)a.Y, (double)b.Y));
public static Float3 Unmix(this Float3 v, Float3 a, Float3 b) => ((float)Unmix((double)v.X, (double)a.X, (double)b.X), (float)Unmix((double)v.Y, (double)a.Y, (double)b.Y), (float)Unmix((double)v.Z, (double)a.Z, (double)b.Z));
public static Float4 Unmix(this Float4 v, Float4 a, Float4 b) => ((float)Unmix((double)v.X, (double)a.X, (double)b.X), (float)Unmix((double)v.Y, (double)a.Y, (double)b.Y), (float)Unmix((double)v.Z, (double)a.Z, (double)b.Z), (float)Unmix((double)v.W, (double)a.W, (double)b.W));
public static Double2 Unmix(this Double2 v, Double2 a, Double2 b) => ((double)Unmix((double)v.X, (double)a.X, (double)b.X), (double)Unmix((double)v.Y, (double)a.Y, (double)b.Y));
public static Double3 Unmix(this Double3 v, Double3 a, Double3 b) => ((double)Unmix((double)v.X, (double)a.X, (double)b.X), (double)Unmix((double)v.Y, (double)a.Y, (double)b.Y), (double)Unmix((double)v.Z, (double)a.Z, (double)b.Z));
public static Double4 Unmix(this Double4 v, Double4 a, Double4 b) => ((double)Unmix((double)v.X, (double)a.X, (double)b.X), (double)Unmix((double)v.Y, (double)a.Y, (double)b.Y), (double)Unmix((double)v.Z, (double)a.Z, (double)b.Z), (double)Unmix((double)v.W, (double)a.W, (double)b.W));
public static Int2 Unmix(this Int2 v, Int2 a, Int2 b) => ((int)Unmix((double)v.A, (double)a.A, (double)b.A), (int)Unmix((double)v.B, (double)a.B, (double)b.B));
public static Int3 Unmix(this Int3 v, Int3 a, Int3 b) => ((int)Unmix((double)v.A, (double)a.A, (double)b.A), (int)Unmix((double)v.B, (double)a.B, (double)b.B), (int)Unmix((double)v.C, (double)a.C, (double)b.C));
public static Int4 Unmix(this Int4 v, Int4 a, Int4 b) => ((int)Unmix((double)v.A, (double)a.A, (double)b.A), (int)Unmix((double)v.B, (double)a.B, (double)b.B), (int)Unmix((double)v.C, (double)a.C, (double)b.C), (int)Unmix((double)v.D, (double)a.D, (double)b.D));
public static Long2 Unmix(this Long2 v, Long2 a, Long2 b) => ((long)Unmix((double)v.A, (double)a.A, (double)b.A), (long)Unmix((double)v.B, (double)a.B, (double)b.B));
public static Long3 Unmix(this Long3 v, Long3 a, Long3 b) => ((long)Unmix((double)v.A, (double)a.A, (double)b.A), (long)Unmix((double)v.B, (double)a.B, (double)b.B), (long)Unmix((double)v.C, (double)a.C, (double)b.C));
public static Long4 Unmix(this Long4 v, Long4 a, Long4 b) => ((long)Unmix((double)v.A, (double)a.A, (double)b.A), (long)Unmix((double)v.B, (double)a.B, (double)b.B), (long)Unmix((double)v.C, (double)a.C, (double)b.C), (long)Unmix((double)v.D, (double)a.D, (double)b.D));
public static Complex Unmix(this Complex v, Complex a, Complex b) => ((double)Unmix((double)v.Real, (double)a.Real, (double)b.Real), (double)Unmix((double)v.Imaginary, (double)a.Imaginary, (double)b.Imaginary));
public static double Clamp(this double v, double min, double max)
{
return Max(Min(v, max), min);
}
public static Float2 Clamp(this Float2 v, Float2 min, Float2 max) => ((float)Clamp((double)v.X, (double)min.X, (double)max.X), (float)Clamp((double)v.Y, (double)min.Y, (double)max.Y));
public static Float3 Clamp(this Float3 v, Float3 min, Float3 max) => ((float)Clamp((double)v.X, (double)min.X, (double)max.X), (float)Clamp((double)v.Y, (double)min.Y, (double)max.Y), (float)Clamp((double)v.Z, (double)min.Z, (double)max.Z));
public static Float4 Clamp(this Float4 v, Float4 min, Float4 max) => ((float)Clamp((double)v.X, (double)min.X, (double)max.X), (float)Clamp((double)v.Y, (double)min.Y, (double)max.Y), (float)Clamp((double)v.Z, (double)min.Z, (double)max.Z), (float)Clamp((double)v.W, (double)min.W, (double)max.W));
public static Double2 Clamp(this Double2 v, Double2 min, Double2 max) => ((double)Clamp((double)v.X, (double)min.X, (double)max.X), (double)Clamp((double)v.Y, (double)min.Y, (double)max.Y));
public static Double3 Clamp(this Double3 v, Double3 min, Double3 max) => ((double)Clamp((double)v.X, (double)min.X, (double)max.X), (double)Clamp((double)v.Y, (double)min.Y, (double)max.Y), (double)Clamp((double)v.Z, (double)min.Z, (double)max.Z));
public static Double4 Clamp(this Double4 v, Double4 min, Double4 max) => ((double)Clamp((double)v.X, (double)min.X, (double)max.X), (double)Clamp((double)v.Y, (double)min.Y, (double)max.Y), (double)Clamp((double)v.Z, (double)min.Z, (double)max.Z), (double)Clamp((double)v.W, (double)min.W, (double)max.W));
public static Int2 Clamp(this Int2 v, Int2 min, Int2 max) => ((int)Clamp((double)v.A, (double)min.A, (double)max.A), (int)Clamp((double)v.B, (double)min.B, (double)max.B));
public static Int3 Clamp(this Int3 v, Int3 min, Int3 max) => ((int)Clamp((double)v.A, (double)min.A, (double)max.A), (int)Clamp((double)v.B, (double)min.B, (double)max.B), (int)Clamp((double)v.C, (double)min.C, (double)max.C));
public static Int4 Clamp(this Int4 v, Int4 min, Int4 max) => ((int)Clamp((double)v.A, (double)min.A, (double)max.A), (int)Clamp((double)v.B, (double)min.B, (double)max.B), (int)Clamp((double)v.C, (double)min.C, (double)max.C), (int)Clamp((double)v.D, (double)min.D, (double)max.D));
public static Long2 Clamp(this Long2 v, Long2 min, Long2 max) => ((long)Clamp((double)v.A, (double)min.A, (double)max.A), (long)Clamp((double)v.B, (double)min.B, (double)max.B));
public static Long3 Clamp(this Long3 v, Long3 min, Long3 max) => ((long)Clamp((double)v.A, (double)min.A, (double)max.A), (long)Clamp((double)v.B, (double)min.B, (double)max.B), (long)Clamp((double)v.C, (double)min.C, (double)max.C));
public static Long4 Clamp(this Long4 v, Long4 min, Long4 max) => ((long)Clamp((double)v.A, (double)min.A, (double)max.A), (long)Clamp((double)v.B, (double)min.B, (double)max.B), (long)Clamp((double)v.C, (double)min.C, (double)max.C), (long)Clamp((double)v.D, (double)min.D, (double)max.D));
public static Complex Clamp(this Complex v, Complex min, Complex max) => ((double)Clamp((double)v.Real, (double)min.Real, (double)max.Real), (double)Clamp((double)v.Imaginary, (double)min.Imaginary, (double)max.Imaginary));
public static double ClampZeroToOne(this double v)
{
return Clamp(v, 0, 1);
}
public static Float2 ClampZeroToOne(this Float2 v) => ((float)ClampZeroToOne((double)v.X), (float)ClampZeroToOne((double)v.Y));
public static Float3 ClampZeroToOne(this Float3 v) => ((float)ClampZeroToOne((double)v.X), (float)ClampZeroToOne((double)v.Y), (float)ClampZeroToOne((double)v.Z));
public static Float4 ClampZeroToOne(this Float4 v) => ((float)ClampZeroToOne((double)v.X), (float)ClampZeroToOne((double)v.Y), (float)ClampZeroToOne((double)v.Z), (float)ClampZeroToOne((double)v.W));
public static Double2 ClampZeroToOne(this Double2 v) => ((double)ClampZeroToOne((double)v.X), (double)ClampZeroToOne((double)v.Y));
public static Double3 ClampZeroToOne(this Double3 v) => ((double)ClampZeroToOne((double)v.X), (double)ClampZeroToOne((double)v.Y), (double)ClampZeroToOne((double)v.Z));
public static Double4 ClampZeroToOne(this Double4 v) => ((double)ClampZeroToOne((double)v.X), (double)ClampZeroToOne((double)v.Y), (double)ClampZeroToOne((double)v.Z), (double)ClampZeroToOne((double)v.W));
public static Int2 ClampZeroToOne(this Int2 v) => ((int)ClampZeroToOne((double)v.A), (int)ClampZeroToOne((double)v.B));
public static Int3 ClampZeroToOne(this Int3 v) => ((int)ClampZeroToOne((double)v.A), (int)ClampZeroToOne((double)v.B), (int)ClampZeroToOne((double)v.C));
public static Int4 ClampZeroToOne(this Int4 v) => ((int)ClampZeroToOne((double)v.A), (int)ClampZeroToOne((double)v.B), (int)ClampZeroToOne((double)v.C), (int)ClampZeroToOne((double)v.D));
public static Long2 ClampZeroToOne(this Long2 v) => ((long)ClampZeroToOne((double)v.A), (long)ClampZeroToOne((double)v.B));
public static Long3 ClampZeroToOne(this Long3 v) => ((long)ClampZeroToOne((double)v.A), (long)ClampZeroToOne((double)v.B), (long)ClampZeroToOne((double)v.C));
public static Long4 ClampZeroToOne(this Long4 v) => ((long)ClampZeroToOne((double)v.A), (long)ClampZeroToOne((double)v.B), (long)ClampZeroToOne((double)v.C), (long)ClampZeroToOne((double)v.D));
public static Complex ClampZeroToOne(this Complex v) => ((double)ClampZeroToOne((double)v.Real), (double)ClampZeroToOne((double)v.Imaginary));
public static double Average(this double v1, double v2)
{
return Lerp(v1, v2, 0.5);
}
public static Float2 Average(this Float2 v1, Float2 v2) => ((float)Average((double)v1.X, (double)v2.X), (float)Average((double)v1.Y, (double)v2.Y));
public static Float3 Average(this Float3 v1, Float3 v2) => ((float)Average((double)v1.X, (double)v2.X), (float)Average((double)v1.Y, (double)v2.Y), (float)Average((double)v1.Z, (double)v2.Z));
public static Float4 Average(this Float4 v1, Float4 v2) => ((float)Average((double)v1.X, (double)v2.X), (float)Average((double)v1.Y, (double)v2.Y), (float)Average((double)v1.Z, (double)v2.Z), (float)Average((double)v1.W, (double)v2.W));
public static Double2 Average(this Double2 v1, Double2 v2) => ((double)Average((double)v1.X, (double)v2.X), (double)Average((double)v1.Y, (double)v2.Y));
public static Double3 Average(this Double3 v1, Double3 v2) => ((double)Average((double)v1.X, (double)v2.X), (double)Average((double)v1.Y, (double)v2.Y), (double)Average((double)v1.Z, (double)v2.Z));
public static Double4 Average(this Double4 v1, Double4 v2) => ((double)Average((double)v1.X, (double)v2.X), (double)Average((double)v1.Y, (double)v2.Y), (double)Average((double)v1.Z, (double)v2.Z), (double)Average((double)v1.W, (double)v2.W));
public static Int2 Average(this Int2 v1, Int2 v2) => ((int)Average((double)v1.A, (double)v2.A), (int)Average((double)v1.B, (double)v2.B));
public static Int3 Average(this Int3 v1, Int3 v2) => ((int)Average((double)v1.A, (double)v2.A), (int)Average((double)v1.B, (double)v2.B), (int)Average((double)v1.C, (double)v2.C));
public static Int4 Average(this Int4 v1, Int4 v2) => ((int)Average((double)v1.A, (double)v2.A), (int)Average((double)v1.B, (double)v2.B), (int)Average((double)v1.C, (double)v2.C), (int)Average((double)v1.D, (double)v2.D));
public static Long2 Average(this Long2 v1, Long2 v2) => ((long)Average((double)v1.A, (double)v2.A), (long)Average((double)v1.B, (double)v2.B));
public static Long3 Average(this Long3 v1, Long3 v2) => ((long)Average((double)v1.A, (double)v2.A), (long)Average((double)v1.B, (double)v2.B), (long)Average((double)v1.C, (double)v2.C));
public static Long4 Average(this Long4 v1, Long4 v2) => ((long)Average((double)v1.A, (double)v2.A), (long)Average((double)v1.B, (double)v2.B), (long)Average((double)v1.C, (double)v2.C), (long)Average((double)v1.D, (double)v2.D));
public static Complex Average(this Complex v1, Complex v2) => ((double)Average((double)v1.Real, (double)v2.Real), (double)Average((double)v1.Imaginary, (double)v2.Imaginary));
public static double Barycentric(this double v1, double v2, double v3, Double2 uv)
{
return v1 + (v2 - v1) * uv.X + (v3 - v1) * uv.Y;
}
public static Float2 Barycentric(this Float2 v1, Float2 v2, Float2 v3, Double2 uv) => ((float)Barycentric((double)v1.X, (double)v2.X, (double)v3.X, uv), (float)Barycentric((double)v1.Y, (double)v2.Y, (double)v3.Y, uv));
public static Float3 Barycentric(this Float3 v1, Float3 v2, Float3 v3, Double2 uv) => ((float)Barycentric((double)v1.X, (double)v2.X, (double)v3.X, uv), (float)Barycentric((double)v1.Y, (double)v2.Y, (double)v3.Y, uv), (float)Barycentric((double)v1.Z, (double)v2.Z, (double)v3.Z, uv));
public static Float4 Barycentric(this Float4 v1, Float4 v2, Float4 v3, Double2 uv) => ((float)Barycentric((double)v1.X, (double)v2.X, (double)v3.X, uv), (float)Barycentric((double)v1.Y, (double)v2.Y, (double)v3.Y, uv), (float)Barycentric((double)v1.Z, (double)v2.Z, (double)v3.Z, uv), (float)Barycentric((double)v1.W, (double)v2.W, (double)v3.W, uv));
public static Double2 Barycentric(this Double2 v1, Double2 v2, Double2 v3, Double2 uv) => ((double)Barycentric((double)v1.X, (double)v2.X, (double)v3.X, uv), (double)Barycentric((double)v1.Y, (double)v2.Y, (double)v3.Y, uv));
public static Double3 Barycentric(this Double3 v1, Double3 v2, Double3 v3, Double2 uv) => ((double)Barycentric((double)v1.X, (double)v2.X, (double)v3.X, uv), (double)Barycentric((double)v1.Y, (double)v2.Y, (double)v3.Y, uv), (double)Barycentric((double)v1.Z, (double)v2.Z, (double)v3.Z, uv));
public static Double4 Barycentric(this Double4 v1, Double4 v2, Double4 v3, Double2 uv) => ((double)Barycentric((double)v1.X, (double)v2.X, (double)v3.X, uv), (double)Barycentric((double)v1.Y, (double)v2.Y, (double)v3.Y, uv), (double)Barycentric((double)v1.Z, (double)v2.Z, (double)v3.Z, uv), (double)Barycentric((double)v1.W, (double)v2.W, (double)v3.W, uv));
public static Int2 Barycentric(this Int2 v1, Int2 v2, Int2 v3, Double2 uv) => ((int)Barycentric((double)v1.A, (double)v2.A, (double)v3.A, uv), (int)Barycentric((double)v1.B, (double)v2.B, (double)v3.B, uv));
public static Int3 Barycentric(this Int3 v1, Int3 v2, Int3 v3, Double2 uv) => ((int)Barycentric((double)v1.A, (double)v2.A, (double)v3.A, uv), (int)Barycentric((double)v1.B, (double)v2.B, (double)v3.B, uv), (int)Barycentric((double)v1.C, (double)v2.C, (double)v3.C, uv));
public static Int4 Barycentric(this Int4 v1, Int4 v2, Int4 v3, Double2 uv) => ((int)Barycentric((double)v1.A, (double)v2.A, (double)v3.A, uv), (int)Barycentric((double)v1.B, (double)v2.B, (double)v3.B, uv), (int)Barycentric((double)v1.C, (double)v2.C, (double)v3.C, uv), (int)Barycentric((double)v1.D, (double)v2.D, (double)v3.D, uv));
public static Long2 Barycentric(this Long2 v1, Long2 v2, Long2 v3, Double2 uv) => ((long)Barycentric((double)v1.A, (double)v2.A, (double)v3.A, uv), (long)Barycentric((double)v1.B, (double)v2.B, (double)v3.B, uv));
public static Long3 Barycentric(this Long3 v1, Long3 v2, Long3 v3, Double2 uv) => ((long)Barycentric((double)v1.A, (double)v2.A, (double)v3.A, uv), (long)Barycentric((double)v1.B, (double)v2.B, (double)v3.B, uv), (long)Barycentric((double)v1.C, (double)v2.C, (double)v3.C, uv));
public static Long4 Barycentric(this Long4 v1, Long4 v2, Long4 v3, Double2 uv) => ((long)Barycentric((double)v1.A, (double)v2.A, (double)v3.A, uv), (long)Barycentric((double)v1.B, (double)v2.B, (double)v3.B, uv), (long)Barycentric((double)v1.C, (double)v2.C, (double)v3.C, uv), (long)Barycentric((double)v1.D, (double)v2.D, (double)v3.D, uv));
public static Complex Barycentric(this Complex v1, Complex v2, Complex v3, Double2 uv) => ((double)Barycentric((double)v1.Real, (double)v2.Real, (double)v3.Real, uv), (double)Barycentric((double)v1.Imaginary, (double)v2.Imaginary, (double)v3.Imaginary, uv));
public static double Min(this double v1, double v2)
{
return Math.Min(v1, v2);
}
public static Float2 Min(this Float2 v1, Float2 v2) => ((float)Min((double)v1.X, (double)v2.X), (float)Min((double)v1.Y, (double)v2.Y));
public static Float3 Min(this Float3 v1, Float3 v2) => ((float)Min((double)v1.X, (double)v2.X), (float)Min((double)v1.Y, (double)v2.Y), (float)Min((double)v1.Z, (double)v2.Z));
public static Float4 Min(this Float4 v1, Float4 v2) => ((float)Min((double)v1.X, (double)v2.X), (float)Min((double)v1.Y, (double)v2.Y), (float)Min((double)v1.Z, (double)v2.Z), (float)Min((double)v1.W, (double)v2.W));
public static Double2 Min(this Double2 v1, Double2 v2) => ((double)Min((double)v1.X, (double)v2.X), (double)Min((double)v1.Y, (double)v2.Y));
public static Double3 Min(this Double3 v1, Double3 v2) => ((double)Min((double)v1.X, (double)v2.X), (double)Min((double)v1.Y, (double)v2.Y), (double)Min((double)v1.Z, (double)v2.Z));
public static Double4 Min(this Double4 v1, Double4 v2) => ((double)Min((double)v1.X, (double)v2.X), (double)Min((double)v1.Y, (double)v2.Y), (double)Min((double)v1.Z, (double)v2.Z), (double)Min((double)v1.W, (double)v2.W));
public static Int2 Min(this Int2 v1, Int2 v2) => ((int)Min((double)v1.A, (double)v2.A), (int)Min((double)v1.B, (double)v2.B));
public static Int3 Min(this Int3 v1, Int3 v2) => ((int)Min((double)v1.A, (double)v2.A), (int)Min((double)v1.B, (double)v2.B), (int)Min((double)v1.C, (double)v2.C));
public static Int4 Min(this Int4 v1, Int4 v2) => ((int)Min((double)v1.A, (double)v2.A), (int)Min((double)v1.B, (double)v2.B), (int)Min((double)v1.C, (double)v2.C), (int)Min((double)v1.D, (double)v2.D));
public static Long2 Min(this Long2 v1, Long2 v2) => ((long)Min((double)v1.A, (double)v2.A), (long)Min((double)v1.B, (double)v2.B));
public static Long3 Min(this Long3 v1, Long3 v2) => ((long)Min((double)v1.A, (double)v2.A), (long)Min((double)v1.B, (double)v2.B), (long)Min((double)v1.C, (double)v2.C));
public static Long4 Min(this Long4 v1, Long4 v2) => ((long)Min((double)v1.A, (double)v2.A), (long)Min((double)v1.B, (double)v2.B), (long)Min((double)v1.C, (double)v2.C), (long)Min((double)v1.D, (double)v2.D));
public static Complex Min(this Complex v1, Complex v2) => ((double)Min((double)v1.Real, (double)v2.Real), (double)Min((double)v1.Imaginary, (double)v2.Imaginary));
public static double Max(this double v1, double v2)
{
return Math.Max(v1, v2);
}
public static Float2 Max(this Float2 v1, Float2 v2) => ((float)Max((double)v1.X, (double)v2.X), (float)Max((double)v1.Y, (double)v2.Y));
public static Float3 Max(this Float3 v1, Float3 v2) => ((float)Max((double)v1.X, (double)v2.X), (float)Max((double)v1.Y, (double)v2.Y), (float)Max((double)v1.Z, (double)v2.Z));
public static Float4 Max(this Float4 v1, Float4 v2) => ((float)Max((double)v1.X, (double)v2.X), (float)Max((double)v1.Y, (double)v2.Y), (float)Max((double)v1.Z, (double)v2.Z), (float)Max((double)v1.W, (double)v2.W));
public static Double2 Max(this Double2 v1, Double2 v2) => ((double)Max((double)v1.X, (double)v2.X), (double)Max((double)v1.Y, (double)v2.Y));
public static Double3 Max(this Double3 v1, Double3 v2) => ((double)Max((double)v1.X, (double)v2.X), (double)Max((double)v1.Y, (double)v2.Y), (double)Max((double)v1.Z, (double)v2.Z));
public static Double4 Max(this Double4 v1, Double4 v2) => ((double)Max((double)v1.X, (double)v2.X), (double)Max((double)v1.Y, (double)v2.Y), (double)Max((double)v1.Z, (double)v2.Z), (double)Max((double)v1.W, (double)v2.W));
public static Int2 Max(this Int2 v1, Int2 v2) => ((int)Max((double)v1.A, (double)v2.A), (int)Max((double)v1.B, (double)v2.B));
public static Int3 Max(this Int3 v1, Int3 v2) => ((int)Max((double)v1.A, (double)v2.A), (int)Max((double)v1.B, (double)v2.B), (int)Max((double)v1.C, (double)v2.C));
public static Int4 Max(this Int4 v1, Int4 v2) => ((int)Max((double)v1.A, (double)v2.A), (int)Max((double)v1.B, (double)v2.B), (int)Max((double)v1.C, (double)v2.C), (int)Max((double)v1.D, (double)v2.D));
public static Long2 Max(this Long2 v1, Long2 v2) => ((long)Max((double)v1.A, (double)v2.A), (long)Max((double)v1.B, (double)v2.B));
public static Long3 Max(this Long3 v1, Long3 v2) => ((long)Max((double)v1.A, (double)v2.A), (long)Max((double)v1.B, (double)v2.B), (long)Max((double)v1.C, (double)v2.C));
public static Long4 Max(this Long4 v1, Long4 v2) => ((long)Max((double)v1.A, (double)v2.A), (long)Max((double)v1.B, (double)v2.B), (long)Max((double)v1.C, (double)v2.C), (long)Max((double)v1.D, (double)v2.D));
public static Complex Max(this Complex v1, Complex v2) => ((double)Max((double)v1.Real, (double)v2.Real), (double)Max((double)v1.Imaginary, (double)v2.Imaginary));
public static double ClampPositive(this double v)
{
return Math.Max(v, 0);
}
public static Float2 ClampPositive(this Float2 v) => ((float)ClampPositive((double)v.X), (float)ClampPositive((double)v.Y));
public static Float3 ClampPositive(this Float3 v) => ((float)ClampPositive((double)v.X), (float)ClampPositive((double)v.Y), (float)ClampPositive((double)v.Z));
public static Float4 ClampPositive(this Float4 v) => ((float)ClampPositive((double)v.X), (float)ClampPositive((double)v.Y), (float)ClampPositive((double)v.Z), (float)ClampPositive((double)v.W));
public static Double2 ClampPositive(this Double2 v) => ((double)ClampPositive((double)v.X), (double)ClampPositive((double)v.Y));
public static Double3 ClampPositive(this Double3 v) => ((double)ClampPositive((double)v.X), (double)ClampPositive((double)v.Y), (double)ClampPositive((double)v.Z));
public static Double4 ClampPositive(this Double4 v) => ((double)ClampPositive((double)v.X), (double)ClampPositive((double)v.Y), (double)ClampPositive((double)v.Z), (double)ClampPositive((double)v.W));
public static Int2 ClampPositive(this Int2 v) => ((int)ClampPositive((double)v.A), (int)ClampPositive((double)v.B));
public static Int3 ClampPositive(this Int3 v) => ((int)ClampPositive((double)v.A), (int)ClampPositive((double)v.B), (int)ClampPositive((double)v.C));
public static Int4 ClampPositive(this Int4 v) => ((int)ClampPositive((double)v.A), (int)ClampPositive((double)v.B), (int)ClampPositive((double)v.C), (int)ClampPositive((double)v.D));
public static Long2 ClampPositive(this Long2 v) => ((long)ClampPositive((double)v.A), (long)ClampPositive((double)v.B));
public static Long3 ClampPositive(this Long3 v) => ((long)ClampPositive((double)v.A), (long)ClampPositive((double)v.B), (long)ClampPositive((double)v.C));
public static Long4 ClampPositive(this Long4 v) => ((long)ClampPositive((double)v.A), (long)ClampPositive((double)v.B), (long)ClampPositive((double)v.C), (long)ClampPositive((double)v.D));
public static Complex ClampPositive(this Complex v) => ((double)ClampPositive((double)v.Real), (double)ClampPositive((double)v.Imaginary));
public static double ClampNegative(this double v)
{
return Math.Min(v, 0);
}
public static Float2 ClampNegative(this Float2 v) => ((float)ClampNegative((double)v.X), (float)ClampNegative((double)v.Y));
public static Float3 ClampNegative(this Float3 v) => ((float)ClampNegative((double)v.X), (float)ClampNegative((double)v.Y), (float)ClampNegative((double)v.Z));
public static Float4 ClampNegative(this Float4 v) => ((float)ClampNegative((double)v.X), (float)ClampNegative((double)v.Y), (float)ClampNegative((double)v.Z), (float)ClampNegative((double)v.W));
public static Double2 ClampNegative(this Double2 v) => ((double)ClampNegative((double)v.X), (double)ClampNegative((double)v.Y));
public static Double3 ClampNegative(this Double3 v) => ((double)ClampNegative((double)v.X), (double)ClampNegative((double)v.Y), (double)ClampNegative((double)v.Z));
public static Double4 ClampNegative(this Double4 v) => ((double)ClampNegative((double)v.X), (double)ClampNegative((double)v.Y), (double)ClampNegative((double)v.Z), (double)ClampNegative((double)v.W));
public static Int2 ClampNegative(this Int2 v) => ((int)ClampNegative((double)v.A), (int)ClampNegative((double)v.B));
public static Int3 ClampNegative(this Int3 v) => ((int)ClampNegative((double)v.A), (int)ClampNegative((double)v.B), (int)ClampNegative((double)v.C));
public static Int4 ClampNegative(this Int4 v) => ((int)ClampNegative((double)v.A), (int)ClampNegative((double)v.B), (int)ClampNegative((double)v.C), (int)ClampNegative((double)v.D));
public static Long2 ClampNegative(this Long2 v) => ((long)ClampNegative((double)v.A), (long)ClampNegative((double)v.B));
public static Long3 ClampNegative(this Long3 v) => ((long)ClampNegative((double)v.A), (long)ClampNegative((double)v.B), (long)ClampNegative((double)v.C));
public static Long4 ClampNegative(this Long4 v) => ((long)ClampNegative((double)v.A), (long)ClampNegative((double)v.B), (long)ClampNegative((double)v.C), (long)ClampNegative((double)v.D));
public static Complex ClampNegative(this Complex v) => ((double)ClampNegative((double)v.Real), (double)ClampNegative((double)v.Imaginary));
public static double Pow2(this double x)
{
return x * x;
}
public static Float2 Pow2(this Float2 x) => ((float)Pow2((double)x.X), (float)Pow2((double)x.Y));
public static Float3 Pow2(this Float3 x) => ((float)Pow2((double)x.X), (float)Pow2((double)x.Y), (float)Pow2((double)x.Z));
public static Float4 Pow2(this Float4 x) => ((float)Pow2((double)x.X), (float)Pow2((double)x.Y), (float)Pow2((double)x.Z), (float)Pow2((double)x.W));
public static Double2 Pow2(this Double2 x) => ((double)Pow2((double)x.X), (double)Pow2((double)x.Y));
public static Double3 Pow2(this Double3 x) => ((double)Pow2((double)x.X), (double)Pow2((double)x.Y), (double)Pow2((double)x.Z));
public static Double4 Pow2(this Double4 x) => ((double)Pow2((double)x.X), (double)Pow2((double)x.Y), (double)Pow2((double)x.Z), (double)Pow2((double)x.W));
public static Int2 Pow2(this Int2 x) => ((int)Pow2((double)x.A), (int)Pow2((double)x.B));
public static Int3 Pow2(this Int3 x) => ((int)Pow2((double)x.A), (int)Pow2((double)x.B), (int)Pow2((double)x.C));
public static Int4 Pow2(this Int4 x) => ((int)Pow2((double)x.A), (int)Pow2((double)x.B), (int)Pow2((double)x.C), (int)Pow2((double)x.D));
public static Long2 Pow2(this Long2 x) => ((long)Pow2((double)x.A), (long)Pow2((double)x.B));
public static Long3 Pow2(this Long3 x) => ((long)Pow2((double)x.A), (long)Pow2((double)x.B), (long)Pow2((double)x.C));
public static Long4 Pow2(this Long4 x) => ((long)Pow2((double)x.A), (long)Pow2((double)x.B), (long)Pow2((double)x.C), (long)Pow2((double)x.D));
public static Complex Pow2(this Complex x) => ((double)Pow2((double)x.Real), (double)Pow2((double)x.Imaginary));
public static double Pow3(this double x)
{
return x * x * x;
}
public static Float2 Pow3(this Float2 x) => ((float)Pow3((double)x.X), (float)Pow3((double)x.Y));
public static Float3 Pow3(this Float3 x) => ((float)Pow3((double)x.X), (float)Pow3((double)x.Y), (float)Pow3((double)x.Z));
public static Float4 Pow3(this Float4 x) => ((float)Pow3((double)x.X), (float)Pow3((double)x.Y), (float)Pow3((double)x.Z), (float)Pow3((double)x.W));
public static Double2 Pow3(this Double2 x) => ((double)Pow3((double)x.X), (double)Pow3((double)x.Y));
public static Double3 Pow3(this Double3 x) => ((double)Pow3((double)x.X), (double)Pow3((double)x.Y), (double)Pow3((double)x.Z));
public static Double4 Pow3(this Double4 x) => ((double)Pow3((double)x.X), (double)Pow3((double)x.Y), (double)Pow3((double)x.Z), (double)Pow3((double)x.W));
public static Int2 Pow3(this Int2 x) => ((int)Pow3((double)x.A), (int)Pow3((double)x.B));
public static Int3 Pow3(this Int3 x) => ((int)Pow3((double)x.A), (int)Pow3((double)x.B), (int)Pow3((double)x.C));
public static Int4 Pow3(this Int4 x) => ((int)Pow3((double)x.A), (int)Pow3((double)x.B), (int)Pow3((double)x.C), (int)Pow3((double)x.D));
public static Long2 Pow3(this Long2 x) => ((long)Pow3((double)x.A), (long)Pow3((double)x.B));
public static Long3 Pow3(this Long3 x) => ((long)Pow3((double)x.A), (long)Pow3((double)x.B), (long)Pow3((double)x.C));
public static Long4 Pow3(this Long4 x) => ((long)Pow3((double)x.A), (long)Pow3((double)x.B), (long)Pow3((double)x.C), (long)Pow3((double)x.D));
public static Complex Pow3(this Complex x) => ((double)Pow3((double)x.Real), (double)Pow3((double)x.Imaginary));
public static double Pow4(this double x)
{
return x * x * x * x;
}
public static Float2 Pow4(this Float2 x) => ((float)Pow4((double)x.X), (float)Pow4((double)x.Y));
public static Float3 Pow4(this Float3 x) => ((float)Pow4((double)x.X), (float)Pow4((double)x.Y), (float)Pow4((double)x.Z));
public static Float4 Pow4(this Float4 x) => ((float)Pow4((double)x.X), (float)Pow4((double)x.Y), (float)Pow4((double)x.Z), (float)Pow4((double)x.W));
public static Double2 Pow4(this Double2 x) => ((double)Pow4((double)x.X), (double)Pow4((double)x.Y));
public static Double3 Pow4(this Double3 x) => ((double)Pow4((double)x.X), (double)Pow4((double)x.Y), (double)Pow4((double)x.Z));
public static Double4 Pow4(this Double4 x) => ((double)Pow4((double)x.X), (double)Pow4((double)x.Y), (double)Pow4((double)x.Z), (double)Pow4((double)x.W));
public static Int2 Pow4(this Int2 x) => ((int)Pow4((double)x.A), (int)Pow4((double)x.B));
public static Int3 Pow4(this Int3 x) => ((int)Pow4((double)x.A), (int)Pow4((double)x.B), (int)Pow4((double)x.C));
public static Int4 Pow4(this Int4 x) => ((int)Pow4((double)x.A), (int)Pow4((double)x.B), (int)Pow4((double)x.C), (int)Pow4((double)x.D));
public static Long2 Pow4(this Long2 x) => ((long)Pow4((double)x.A), (long)Pow4((double)x.B));
public static Long3 Pow4(this Long3 x) => ((long)Pow4((double)x.A), (long)Pow4((double)x.B), (long)Pow4((double)x.C));
public static Long4 Pow4(this Long4 x) => ((long)Pow4((double)x.A), (long)Pow4((double)x.B), (long)Pow4((double)x.C), (long)Pow4((double)x.D));
public static Complex Pow4(this Complex x) => ((double)Pow4((double)x.Real), (double)Pow4((double)x.Imaginary));
public static double Pow5(this double x)
{
return x * x * x * x * x;
}
public static Float2 Pow5(this Float2 x) => ((float)Pow5((double)x.X), (float)Pow5((double)x.Y));
public static Float3 Pow5(this Float3 x) => ((float)Pow5((double)x.X), (float)Pow5((double)x.Y), (float)Pow5((double)x.Z));
public static Float4 Pow5(this Float4 x) => ((float)Pow5((double)x.X), (float)Pow5((double)x.Y), (float)Pow5((double)x.Z), (float)Pow5((double)x.W));
public static Double2 Pow5(this Double2 x) => ((double)Pow5((double)x.X), (double)Pow5((double)x.Y));
public static Double3 Pow5(this Double3 x) => ((double)Pow5((double)x.X), (double)Pow5((double)x.Y), (double)Pow5((double)x.Z));
public static Double4 Pow5(this Double4 x) => ((double)Pow5((double)x.X), (double)Pow5((double)x.Y), (double)Pow5((double)x.Z), (double)Pow5((double)x.W));
public static Int2 Pow5(this Int2 x) => ((int)Pow5((double)x.A), (int)Pow5((double)x.B));
public static Int3 Pow5(this Int3 x) => ((int)Pow5((double)x.A), (int)Pow5((double)x.B), (int)Pow5((double)x.C));
public static Int4 Pow5(this Int4 x) => ((int)Pow5((double)x.A), (int)Pow5((double)x.B), (int)Pow5((double)x.C), (int)Pow5((double)x.D));
public static Long2 Pow5(this Long2 x) => ((long)Pow5((double)x.A), (long)Pow5((double)x.B));
public static Long3 Pow5(this Long3 x) => ((long)Pow5((double)x.A), (long)Pow5((double)x.B), (long)Pow5((double)x.C));
public static Long4 Pow5(this Long4 x) => ((long)Pow5((double)x.A), (long)Pow5((double)x.B), (long)Pow5((double)x.C), (long)Pow5((double)x.D));
public static Complex Pow5(this Complex x) => ((double)Pow5((double)x.Real), (double)Pow5((double)x.Imaginary));
public static double Pow(this double x, double y)
{
return Math.Pow(x, y);
}
public static Float2 Pow(this Float2 x, Float2 y) => ((float)Pow((double)x.X, (double)y.X), (float)Pow((double)x.Y, (double)y.Y));
public static Float3 Pow(this Float3 x, Float3 y) => ((float)Pow((double)x.X, (double)y.X), (float)Pow((double)x.Y, (double)y.Y), (float)Pow((double)x.Z, (double)y.Z));
public static Float4 Pow(this Float4 x, Float4 y) => ((float)Pow((double)x.X, (double)y.X), (float)Pow((double)x.Y, (double)y.Y), (float)Pow((double)x.Z, (double)y.Z), (float)Pow((double)x.W, (double)y.W));
public static Double2 Pow(this Double2 x, Double2 y) => ((double)Pow((double)x.X, (double)y.X), (double)Pow((double)x.Y, (double)y.Y));
public static Double3 Pow(this Double3 x, Double3 y) => ((double)Pow((double)x.X, (double)y.X), (double)Pow((double)x.Y, (double)y.Y), (double)Pow((double)x.Z, (double)y.Z));
public static Double4 Pow(this Double4 x, Double4 y) => ((double)Pow((double)x.X, (double)y.X), (double)Pow((double)x.Y, (double)y.Y), (double)Pow((double)x.Z, (double)y.Z), (double)Pow((double)x.W, (double)y.W));
public static Int2 Pow(this Int2 x, Int2 y) => ((int)Pow((double)x.A, (double)y.A), (int)Pow((double)x.B, (double)y.B));
public static Int3 Pow(this Int3 x, Int3 y) => ((int)Pow((double)x.A, (double)y.A), (int)Pow((double)x.B, (double)y.B), (int)Pow((double)x.C, (double)y.C));
public static Int4 Pow(this Int4 x, Int4 y) => ((int)Pow((double)x.A, (double)y.A), (int)Pow((double)x.B, (double)y.B), (int)Pow((double)x.C, (double)y.C), (int)Pow((double)x.D, (double)y.D));
public static Long2 Pow(this Long2 x, Long2 y) => ((long)Pow((double)x.A, (double)y.A), (long)Pow((double)x.B, (double)y.B));
public static Long3 Pow(this Long3 x, Long3 y) => ((long)Pow((double)x.A, (double)y.A), (long)Pow((double)x.B, (double)y.B), (long)Pow((double)x.C, (double)y.C));
public static Long4 Pow(this Long4 x, Long4 y) => ((long)Pow((double)x.A, (double)y.A), (long)Pow((double)x.B, (double)y.B), (long)Pow((double)x.C, (double)y.C), (long)Pow((double)x.D, (double)y.D));
public static Complex Pow(this Complex x, Complex y) => ((double)Pow((double)x.Real, (double)y.Real), (double)Pow((double)x.Imaginary, (double)y.Imaginary));
}
public static class ConversionOperations {
public static Angle Revs(this double x)
{
return x * Constants.TwoPi;
}
public static Angle Rads(this double x)
{
return x;
}
public static Angle Degrees(this double x)
{
return (x / 360).Revs();
}
public static double Revs(this Angle x)
{
return x.Radians / Constants.TwoPi;
}
public static double Rads(this Angle x)
{
return x.Radians;
}
public static double Degrees(this Angle x)
{
return x.Revs() * 360;
}
}
public static class TrigOperations {
public static Angle Acos(this double x)
{
return Math.Acos(x);
}
public static Angle Asin(this double x)
{
return Math.Asin(x);
}
public static Angle Atan(this double x)
{
return Math.Atan(x);
}
public static double Versin(this Angle x)
{
return 1 - Cos(x);
}
public static double Vercosin(this Angle x)
{
return 1 + Cos(x);
}
public static double Coversin(this Angle x)
{
return 1 - Sin(x);
}
public static double Covercosin(this Angle x)
{
return 1 + Sin(x);
}
public static double Haversin(this Angle x)
{
return Versin(x) / 2;
}
public static double Havercosin(this Angle x)
{
return Vercosin(x) / 2;
}
public static double Hacoversin(this Angle x)
{
return Coversin(x) / 2;
}
public static double Hacovercosin(this Angle x)
{
return Coversin(x) / 2;
}
public static double Exsec(this Angle x)
{
return Sec(x) - 1;
}
public static double Excsc(this Angle x)
{
return Csc(x) - 1;
}
public static double Cos(this Angle x)
{
return Math.Cos(x);
}
public static double Cosh(this Angle x)
{
return Math.Cosh(x);
}
public static double Sin(this Angle x)
{
return Math.Sin(x);
}
public static double Sinh(this Angle x)
{
return Math.Sinh(x);
}
public static double Tan(this Angle x)
{
return Math.Tan(x);
}
public static double Tanh(this Angle x)
{
return Math.Tanh(x);
}
public static double Sec(this Angle x)
{
return 1.0 / Cos(x);
}
public static double Csc(this Angle x)
{
return 1.0 / Sin(x);
}
public static double Cot(this Angle x)
{
return 1.0 / Tan(x);
}
}
public static class ComparisonOperations {
public static bool GtZ(this double x)
{
return x > 0;
}
public static bool LtZ(this double x)
{
return x < 0;
}
public static bool GtEqZ(this double x)
{
return x >= 0;
}
public static bool LtEqZ(this double x)
{
return x <= 0;
}
public static bool Gt(this double x, double y)
{
return x > y;
}
public static bool Lt(this double x, double y)
{
return x > y;
}
public static bool GtEq(this double x, double y)
{
return x >= y;
}
public static bool LtEq(this double x, double y)
{
return x <= y;
}
public static bool IsInfinity(this double v)
{
return double.IsInfinity(v);
}
public static bool IsNaN(this double v)
{
return double.IsNaN(v);
}
public static bool AlmostZero(this double v)
{
return v.AlmostEquals(0);
}
public static bool AlmostOne(this double v)
{
return v.AlmostEquals(1);
}
public static bool AlmostZeroOrOne(this double v)
{
return v.AlmostZero() && v.AlmostOne();
}
public static bool Within(this double v, double min, double max)
{
return v >= min && v < max;
}
}
public static class IntervalOperations {
public static double Size(this Interval interval)
{
return (interval.A - interval.B).Abs();
}
public static Interval ResetLeftToZero(this Interval interval)
{
return (0, Size(interval));
}
public static Interval ResetRightToZero(this Interval interval)
{
return (-Size(interval), 0);
}
public static double HalfSize(this Interval interval)
{
return (interval.A - interval.B).Abs().Half();
}
public static Interval Invert(this Interval interval)
{
return (interval.B, interval.A);
}
public static Interval Normalize(this Interval interval)
{
return (interval.A.Min(interval.B), interval.B.Max(interval.B));
}
public static double Clamp(this double value, Interval interval)
{
return value.Clamp(interval.A, interval.B);
}
public static double Lerp(this double value, Interval interval)
{
return value.Lerp(interval.A, interval.B);
}
public static double InverseLerp(this double value, Interval interval)
{
return value.InverseLerp(interval.A, interval.B);
}
public static double Clamp(this Interval interval, double value)
{
return Clamp(value, interval);
}
public static double Lerp(this Interval interval, double value)
{
return Lerp(value, interval);
}
public static double InverseLerp(this Interval interval, double value)
{
return InverseLerp(value, interval);
}
public static double Remap(this double value, Interval input, Interval output)
{
return Lerp(InverseLerp(value, input), output);
}
public static double Center(this Interval interval)
{
return Lerp(0.5, interval);
}
public static Interval Left(this Interval interval, double amount)
{
return (interval.A, Lerp(amount, interval));
}
public static Interval Right(this Interval interval, double amount)
{
return (Lerp(amount, interval), interval.B);
}
public static ValueTuple<Interval, Interval> Split(this Interval interval, double amount)
{
return (Left(interval, amount), Right(interval, amount));
}
public static Interval Resize(this Interval interval, double amount)
{
return (Center(interval) - amount / 2, Center(interval) + amount * 2);
}
public static Interval ResizeRelative(this Interval interval, double amount)
{
return Resize(interval, Size(interval) * amount);
}
public static Interval Multiply(this Interval interval, double amount)
{
return ResizeRelative(interval, Size(interval) * amount);
}
public static Interval Divide(this Interval interval, double amount)
{
return Multiply(interval, 1.0 / amount);
}
public static Interval Offset(this Interval interval, double amount)
{
return (interval.A + amount, interval.B + amount);
}
public static Interval CenterAt(this Interval interval, double value = 0)
{
return Offset(interval, value - Center(interval));
}
}
public static class EasingOperations {
public static Func<double, double> BlendEaseFunc(this Func<double, double> easeIn, Func<double, double> easeOut)
{
return (double p) => {
    return p < 0.5 ? 0.5 * easeIn(p * 2) : 0.5 * easeOut(p * 2 - 1) + 0.5;
}
;
}
public static Func<double, double> InvertEaseFunc(this Func<double, double> easeIn)
{
return (double p) => {
    return 1 - easeIn(1 - p);
}
;
}
public static double Linear(this double p)
{
return p;
}
public static double QuadraticEaseIn(this double p)
{
return p.Pow2();
}
public static double QuadraticEaseOut(this double p)
{
return InvertEaseFunc(QuadraticEaseIn)(p);
}
public static double QuadraticEaseInOut(this double p)
{
return BlendEaseFunc(QuadraticEaseIn, QuadraticEaseOut)(p);
}
public static double CubicEaseIn(this double p)
{
return p.Pow3();
}
public static double CubicEaseOut(this double p)
{
return InvertEaseFunc(CubicEaseIn)(p);
}
public static double CubicEaseInOut(this double p)
{
return BlendEaseFunc(CubicEaseIn, CubicEaseOut)(p);
}
public static double QuarticEaseIn(this double p)
{
return p.Pow4();
}
public static double QuarticEaseOut(this double p)
{
return InvertEaseFunc(QuarticEaseIn)(p);
}
public static double QuarticEaseInOut(this double p)
{
return BlendEaseFunc(QuarticEaseIn, QuarticEaseOut)(p);
}
public static double QuinticEaseIn(this double p)
{
return p.Pow5();
}
public static double QuinticEaseOut(this double p)
{
return InvertEaseFunc(QuinticEaseIn)(p);
}
public static double QuinticEaseInOut(this double p)
{
return BlendEaseFunc(QuinticEaseIn, QuinticEaseOut)(p);
}
public static double SineEaseIn(this double p)
{
return InvertEaseFunc(SineEaseOut)(p);
}
public static double SineEaseOut(this double p)
{
return p.Quarter().Revs().Sin();
}
public static double SineEaseInOut(this double p)
{
return BlendEaseFunc(SineEaseIn, SineEaseOut)(p);
}
public static double CircularEaseIn(this double p)
{
return 1 - (1 - p.Pow2()).Sqrt();
}
public static double CircularEaseOut(this double p)
{
return InvertEaseFunc(CircularEaseIn)(p);
}
public static double CircularEaseInOut(this double p)
{
return BlendEaseFunc(CircularEaseIn, CircularEaseOut)(p);
}
public static double ExponentialEaseIn(this double p)
{
return p.AlmostZero() ? p : 2.0.Pow(10 * (p - 1));
}
public static double ExponentialEaseOut(this double p)
{
return InvertEaseFunc(ExponentialEaseIn)(p);
}
public static double ExponentialEaseInOut(this double p)
{
return BlendEaseFunc(ExponentialEaseIn, ExponentialEaseOut)(p);
}
public static double ElasticEaseIn(this double p)
{
return (13 * p.Quarter().Revs()) * 2.0.Pow(10 * (p - 1)).Rads().Sin();
}
public static double ElasticEaseOut(this double p)
{
return InvertEaseFunc(ElasticEaseIn)(p);
}
public static double ElasticEaseInOut(this double p)
{
return BlendEaseFunc(ElasticEaseIn, ElasticEaseOut)(p);
}
public static double BackEaseIn(this double p)
{
return p.Pow3() - p * p.Half().Revs().Sin();
}
public static double BackEaseOut(this double p)
{
return InvertEaseFunc(BackEaseIn)(p);
}
public static double BackEaseInOut(this double p)
{
return BlendEaseFunc(BackEaseIn, BackEaseOut)(p);
}
public static double BounceEaseIn(this double p)
{
return InvertEaseFunc(BounceEaseOut)(p);
}
public static double BounceEaseOut(this double p)
{
{
    if (p < 4 / 11.0)
    {
        return 121.0 * p.Pow2() / 16.0;
    }
    else
    {
        ;
    }
    
    if (p < 8 / 11.0)
    {
        return 363.0 / 40.0 * p.Pow2() - 99.0 / 10.0 * p + 17.0 / 5.0;
    }
    else
    {
        ;
    }
    
    if (p < 9 / 10.0)
    {
        return 4356.0 / 361.0 * p.Pow2() - 35442.0 / 1805.0 * p + 16061.0 / 1805.0;
    }
    else
    {
        ;
    }
    
    return 54.0 / 5.0 * p.Pow2() - 513.0 / 25.0 * p + 268.0 / 25.0;
}

}
public static double BounceEaseInOut(this double p)
{
return BlendEaseFunc(BounceEaseIn, BounceEaseOut)(p);
}
}
public static class ShapingOperations {
public static double ExponentialImpulse(this double x, double k)
{
return k * x * (1.0 - k * x).Exp();
}
public static double QuadraticImpulse(this double x, double k)
{
return 2.0 * k.Sqrt() * x / (1.0 + k * x * x);
}
public static double PolynomalialImpulse(this double x, double k, double n)
{
return (n / (n - 1.0)) * ((n - 1.0) * k).Pow(1.0 / n) * x / (1.0 + k * x.Pow(n));
}
public static double NormalizedPowerCurve(this double x, double a, double b)
{
return (a + b.Pow(a + b) / (a.Pow(a) * (b.Pow(b)) * UnnormalizedPowerCurve(x, a, b)));
}
public static double UnnormalizedPowerCurve(this double x, double a, double b)
{
return x.Pow(a) * (1.0 - x).Pow(b);
}
public static double Parabola(this double x, double k)
{
return 4.0 * x * (1.0 - x).Pow(k);
}
public static double Sinc(this double x, double k)
{
{
    var a = k * (x - 1.0).Half().Revs();
    return a.Sin() / a;
}

}
public static double Gain(this double x, double k)
{
{
    var a = 0.5 * (2.0 * ((x < 0.5) ? x : 1.0 - x).Pow(k));
    return (x < 0.5) ? a : 1.0 - a;
}

}
public static double ExponentialStep(this double x, double k, double n)
{
return (-k * x.Pow(n)).Exp();
}
public static double NearIdentityCubic(this double x, double threshold, double constant = 0)
{
{
    if (x > threshold)
    {
        return x;
    }
    else
    {
        ;
    }
    
    var a = 2.0 * constant - threshold;
    var b = 2.0 * threshold - 3.0 * constant;
    var t = x / threshold;
    return (a * t + b) * t * t + constant;
}

}
public static double NearIdentitySqrt(this double x, double constant = 0)
{
return (x * x + constant).Sqrt();
}
public static double NearUnityIdentity(this double x)
{
return x * x * (2.0 - x);
}
public static double IntegralSmoothstep(this double x, double t)
{
return (x > t) ? x - t / 2.0 : (x).Pow3() * (1.0 - x * 0.5 / t) / t / t;
}
}
public static class CurveOperations {
public static double CircleSigmoid(this double x, double a = 0.5)
{
return x <= a ? a - (a.Pow2() - x.Pow2()).Sqrt() : a + ((1 - a).Pow2() - (x - 1).Pow2().Pow2()).Sqrt();
}
public static double CircularSeat(this double x, double a = 0.5)
{
return x <= a ? (a.Pow2() - (x - a).Pow2()).Sqrt() : ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();
}
public static double EllipticalSeat(this double x, double a, double b)
{
return (a.AlmostZeroOrOne()) ? a : (x <= a) ? (b / a) * (a.Pow2() - (x - a).Pow2()).Sqrt() : 1 - ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - a).Pow2()).Sqrt();
}
public static double EllipticalSigmoid(this double x, double a, double b)
{
return a.AlmostZeroOrOne() ? a : (x <= a) ? b * (1 - ((a.Pow2() - x.Pow2()) / a).Sqrt()) : b + ((1 - b) / (1 - a)) * ((1 - a).Pow2() - (x - 1).Pow2()).Sqrt();
}
public static double Caternay(this double x, double a = 1.0)
{
return (x / a).Rads().Cosh();
}
public static Double2 Parabola(this double x)
{
return (x, x.Pow2());
}
public static Double2 Hyperbola(this Angle t)
{
return (t.Sec(), t.Tan());
}
public static Double2 LeminscateOfGerono(this Angle t)
{
return (t.Cos(), t.Sin() * t.Cos());
}
public static Double2 Circle(this Angle t)
{
return (t.Cos(), t.Sin());
}
public static Double2 Lissajous(this Angle t, double a, double b, double kx, double ky)
{
return (a * (kx * t).Cos(), b * (ky * t).Sin());
}
public static Double2 Hypotrochoid(this Angle t, double r, double d)
{
return ((1 - r) * t.Cos() + d * (t - 1.0).Cos(), (1 - r) * t.Sin() + d * (t - 1.0).Sin());
}
public static Double2 Epicycloid(this Angle t, double r, double k)
{
return (r * (k + 1) * t.Cos() - r * ((k + 1) * t).Cos(), r * (k + 1) * t.Sin() - r * ((k + 1) * t).Sin());
}
public static Double2 ClosedEpicycloid(this Angle t, int k)
{
return Epicycloid(t, 1.0 / k, 1);
}
public static Double2 Cardoid(this Angle t)
{
return ClosedEpicycloid(t, 1);
}
public static Double2 Nephroid(this Angle t)
{
return ClosedEpicycloid(t, 2);
}
public static Double2 Trefoiloid(this Angle t)
{
return ClosedEpicycloid(t, 3);
}
public static Double2 Quatrefoiloid(this Angle t)
{
return ClosedEpicycloid(t, 4);
}
public static Double2 Hypocycloid(this Angle t, double r, double k)
{
return (r * (k - 1) * t.Cos() + r * ((k - 1) * t).Cos(), r * (k - 1) * t.Sin() + r * ((k - 1) * t).Cos());
}
public static Double2 ClosedHypocycloid(this Angle t, int k)
{
return Hypocycloid(t, 1.0 / k, 1);
}
public static Double2 Deltoid(this Angle t)
{
return ClosedHypocycloid(t, 3);
}
public static Double2 Astroid(this Angle t)
{
return ClosedHypocycloid(t, 4);
}
public static Double2 Epitrochoid(this Angle t, double r, double d)
{
return ((1 + r) * t.Cos() - d * ((1 + r) / r * t).Cos(), (1 + r) * t.Sin() - d * ((1 + r) / r * t).Sin());
}
public static Double2 GeneralizedParametricCurve(this Angle t, double a, double b, double c, double d, double j, double k)
{
return ((a * t).Cos() - (b * t).Cos().Pow(j), (c * t).Sin() - (d * t).Sin().Pow(k));
}
public static double PolarLimacon(this Angle t, double a, double b)
{
return b + a * t.Cos();
}
public static Double2 CartesianLimacon(this Angle t, double a = 1, double b = 1)
{
return ToCartesian((PolarLimacon(t, a, b), t));
}
public static Double2 ToCartesian(this PolarCoordinate polar)
{
return Circle(polar.Angle) * polar.Radius;
}
}
public static class DistanceField2DOperations {
public static double CircleSDF(this Double2 p)
{
return CircleSDF(p, 1);
}
public static double CircleSDF(this Double2 p, double r)
{
return p.Length() - r;
}
public static Double2 XY(this Double4 d)
{
return (d.X, d.Y);
}
public static Double2 ZW(this Double4 d)
{
return (d.Z, d.W);
}
public static double RoundedBoxSDF(this Double2 p, Double2 size, Double4 r)
{
{
    var xy = p.X > 0.0 ? r.XY() : r.ZW();
    var x = p.Y > 0.0 ? xy.X : xy.Y;
    var q = p.Abs() - size + x;
    return q.X.Max(q.Y).Min(0.0) + q.Max(Double2.Zero).Length() - x;
}

}
public static double BoxSDF(this Double2 p, Double2 size)
{
{
    var d = p.Abs() - size;
    return d.ClampPositive().Length() + d.X.Max(d.Y).ClampPositive();
}

}
public static double LineSDF(this Double2 p, Line2D line)
{
{
    var pa = p - line.A;
    var ba = line.B.Value - line.A.Value;
    var h = (pa.Dot(ba) / ba.Dot(ba)).Clamp(0.0, 1.0);
    return (pa - ba * h).Length();
}

}
public static double PolygonSDF(this Double2 p, IArray<Double2> v)
{
{
    var d = (p - v[0]).Dot(p - v[0]);
    var s = 1.0;
    var i = 0;
    var j = v.Count - 1;
    while (i < v.Count)
    {
        {
            var e = v[j] - v[i];
            var w = p - v[i];
            var b = w - e * (w.Dot(e) / e.Dot(e)).ClampZeroToOne();
            d = d.Min(b.Dot(b));
            var cond1 = p.Y >= v[i].Y;
            var cond2 = p.Y < v[j].Y;
            var cond3 = e.X * w.Y > e.Y * w.X;
            if (cond1 == cond2 && cond1 == cond3)
            {
                s = -s;
            }
            else
            {
                ;
            }
            
        }
        
        j = i;
        i++;
    }
    
    return s * d.Sqrt();
}

}
public static double EquilateralTriangleSDF(this Double2 p)
{
{
    var k = 3.0.Sqrt();
    var x = p.X.Abs() - 1.0;
    var y = p.Y + 1.0 / k;
    var r = p;
    if (x + k * y > 0.0)
    {
        r = new Double2(x - k * y, -k * x - y) / 2.0;
    }
    else
    {
        ;
    }
    
    r = r.WithX(r.X - p.X.Clamp(-2.0, 0.0));
    return -r.Length() * r.Y.Sign();
}

}
public static double RoundXSDF(this Double2 p, double w, double r)
{
{
    p = p.Abs();
    return (p - (p.X + p.Y.Min(w) * 0.5)).Length() - r;
}

}
public static Func<Double2, double> RoundedSDF(this Func<Double2, double> func, double r)
{
return (Double2 p) => {
    return func(p) - r;
}
;
}
public static Func<Double2, double> AnnularSDF(this Func<Double2, double> func, double r)
{
return (Double2 p) => {
    return func(p).Abs() - r;
}
;
}
public static Func<Double2, double> RepeatSDF(this Func<Double2, double> func, Double2 period)
{
return (Double2 p) => {
    return func((p + new Double2(0.5, 0.5) * period).Modulo(period) - period * 0.5);
}
;
}
}
} // End namespace
