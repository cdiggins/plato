using System;
using System.Runtime.CompilerServices;

namespace Plato
{
    /*
    public static partial class Intrinsics
    {
        public static T Zero<T>() => throw new NotImplementedException();
        public static T One<T>() => throw new NotImplementedException();
        public static T NaN<T>() => throw new NotImplementedException();
        public static T MinValue<T>() => throw new NotImplementedException();
        public static T MaxValue<T>() => throw new NotImplementedException();

        public static T PositiveInfinity<T>() => throw new NotImplementedException();
        public static T NegativeInfinity<T>() => throw new NotImplementedException();
        public static T Epsilon<T>() => throw new NotImplementedException();

        public static T Add<T>(T self, T other) => throw new NotImplementedException(); 

        //public static T Add<T>(T self, T other) => throw new NotImplementedException(); 
        public static T Subtract<T>(T self, T other) => throw new NotImplementedException();
        public static T Multiply<T>(T self, T other) => throw new NotImplementedException();
        public static T Divide<T>(T self, T other) => throw new NotImplementedException();
        public static T Modulo<T>(T self, T other) => throw new NotImplementedException();

        public static bool LessThan<T>(T self, T other) => throw new NotImplementedException();
        public static bool GreaterThan<T>(T self, T other) => throw new NotImplementedException();
        public static bool Equals<T>(T self, T other) => throw new NotImplementedException();
        public static bool NotEquals<T>(T self, T other) => throw new NotImplementedException();
        public static bool LessThanOrNotEquals<T>(T self, T other) => throw new NotImplementedException();
        public static bool GreaterThanOrNotEquals<T>(T self, T other) => throw new NotImplementedException();

        public static bool IsInfinity<T>(T self) => throw new NotImplementedException();
        public static bool IsNaN<T>(T self) => throw new NotImplementedException();
        public static T Negate<T>(T self) => throw new NotImplementedException();
        public static T Reciprocal<T>(T self) => throw new NotImplementedException();

        public static T Lesser<T>(T self, T other) => throw new NotImplementedException();
        public static T Greater<T>(T self, T other) => throw new NotImplementedException();
    }
    */

    public class TypedConstants<T>
    {
        public static T One;
        public static T MinValue;
        public static T MaxValue;
        public static T Zero;
        public static T NaN;
        public static T Epsilon;
        public static T PositiveInfinity;
        public static T NegativeInfinity;
    }

    public static class Constants
    {
        public const double DegreesToRadians = 2 * Math.PI / 360.0;
        public const double RadiansToDegrees = 360.0 / (2 * Math.PI);
        public const float Tolerance = Single.Epsilon;
    }

    /*
     * - First, generate the operators from the functions. Easy.
     * - Next, generate the functions for "IVector". Anything that is a vector has the same stuff.
     * - Next, generate the functions of "INumber". Same thing as above.
     * - Next, generate the intrinsic operations.
     * - Plato says, if you tell me this type is a number, then not a problem. I trust you and I can work with that.
     * - What would be fun would be to be able to do things like constrain it, etc.
     * - HOWEVER: having fun is not a priority.
     * - So: as soon as something is called a "Number" then it has "NumberOps" static functions generated for it.
     * - Where NumberOps is a partial library. Can a partial stretch across multiple projects? Nope.
     * - I can live with that for now.
     */

    /*
    public static partial class Intrinsics
    {
        public static float Add(this float self, float other) => self + other;
        public static float Subtract(this float self, float other) => self - other;
        public static float Multiply(this float self, float other) => self * other;
        public static float Divide(this float self, float other) => self / other;   
        public static float Modulo(this float self, float other) => self % other;
        public static bool LessThan(this float self, float other) => self < other;
        public static bool GreaterThan(this float self, float other) => self > other;
        public static bool Equals(this float self, float other) => self == other;
        public static bool NotEquals(this float self, float other) => self != other;
        public static bool LessThanOrEquals(this float self, float other) => self <= other;
        public static bool GreaterThanOrEquals(this float self, float other) => self >= other;
        public static float Negate(this float self) => -self;
        public static float Reciprocal(this float self) => 1f / self;
 
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Abs(this float x) => (float)Math.Abs(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Acos(this float x) => (float)Math.Acos(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Asin(this float x) => (float)Math.Asin(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Atan(this float x) => (float)Math.Atan(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Cos(this float x) => (float)Math.Cos(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Cosh(this float x) => (float)Math.Cosh(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Exp(this float x) => (float)Math.Exp(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Log(this float x) => (float)Math.Log(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Log10(this float x) => (float)Math.Log10(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sin(this float x) => (float)Math.Sin(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sinh(this float x) => (float)Math.Sinh(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sqrt(this float x) => (float)Math.Sqrt(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Tan(this float x) => (float)Math.Tan(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Tanh(this float x) => (float)Math.Tanh(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Sign(this float x) => x > 0 ? 1 : x < 0 ? -1 : 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Magnitude(this float x) => x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float MagnitudeSquared(this float x) => x * x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Inverse(this float x) => (float)1 / x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Truncate(this float x) => (float)Math.Truncate(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Ceiling(this float x) => (float)Math.Ceiling(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Floor(this float x) => (float)Math.Floor(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Round(this float x) => (float)System.Math.Round(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float ToRadians(this float x) => (float)(x * Constants.DegreesToRadians);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float ToDegrees(this float x) => (float)(x * Constants.RadiansToDegrees);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Distance(this float v1, float v2) => (v1 - v2).Abs();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsInfinity(this float v) => float.IsInfinity(v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNaN(this float v) => float.IsNaN(v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool AlmostEquals(this float v1, float v2, float tolerance = Constants.Tolerance) => (v2 - v1).AlmostZero(tolerance);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool AlmostZero(this float v, float tolerance = Constants.Tolerance) => v.Abs() < tolerance;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Smoothstep(this float v) => v * v * (3 - 2 * v);
    }


    public static class VectorExtensions
    {
        public static TScalar SumComponents<T, TScalar>(this IVector<T, TScalar> self)
        {
            var r = self.GetComponent(0);
            for (var i = 1; i < self.NumComponents; ++i)
                r = Intrinsics.Add(r, self.GetComponent(i));
            return r;
        }
    }
    */

}