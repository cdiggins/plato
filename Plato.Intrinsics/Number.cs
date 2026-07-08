using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>float</c> type, 
    /// forwarding all arithmetic and common methods to <c>float</c>.
    /// </summary>
    [DataContract]
    public partial struct Number 
    {
        // -------------------------------------------------------------------------------
        // Field (the wrapped float)
        // -------------------------------------------------------------------------------
        
        [DataMember] public readonly float Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Number(float value) => Value = value;

        // -------------------------------------------------------------------------------
        // Convert to/from float
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Number FromSystem(float f) => new(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Number(float f) => FromSystem(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator float(Number n) => n.Value;
        
        // -------------------------------------------------------------------------------
        // Operators (forward to float)
        // -------------------------------------------------------------------------------
        [MethodImpl(AggressiveInlining)]
        public static Number operator +(Number a, Number b)
            => a.Value + b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator -(Number a, Number b)
            => a.Value - b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator *(Number a, Number b)
            => a.Value * b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator %(Number a, Number b)
            => a.Value % b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator /(Number a, Number b)
            => a.Value / b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Number operator -(Number n)
            => -n.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <(Number a, Number b)
            => a.Value < b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <=(Number a, Number b)
            => a.Value <= b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >(Number a, Number b)
            => a.Value > b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >=(Number a, Number b)
            => a.Value >= b.Value;

        [MethodImpl(AggressiveInlining)]
        public Integer CompareTo(Number other)
            => Value.CompareTo(other.Value);

        //-------------------------------------------------------------------------------
        // Math Intrinsic Functions
        //-------------------------------------------------------------------------------

        /// <summary>
        /// The absolute value of a single-precision floating-point number.
        /// </summary>
        public Number Abs
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Abs(Value);
        }

        /// <summary>
        /// The angle whose cosine is the specified number.
        /// </summary>
        public Angle Acos
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Acos(Value);
        }

        /// <summary>
        /// The angle whose hyperbolic cosine is the specified number.
        /// </summary>
        public Angle Acosh
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Acosh(Value);
        }

        /// <summary>
        /// The angle whose sine is the specified number.
        /// </summary>
        public Angle Asin
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Asin(Value);
        }

        /// <summary>
        /// The angle whose hyperbolic sine is the specified number.
        /// </summary>
        public Angle Asinh
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Asinh(Value);
        }

        /// <summary>
        /// The angle whose tangent is the specified number.
        /// </summary>
        public Angle Atan
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Atan(Value);
        }

        /// <summary>
        /// The angle whose tangent is the quotient of this number with another.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Angle Atan2(Number x) => MathF.Atan2(Value, x);

        /// <summary>
        /// The angle whose hyperbolic tangent is the specified number.
        /// </summary>
        public Angle Atanh
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Atanh(Value);
        }

        /// <summary>
        /// The largest value that compares less than the value.
        /// </summary>
        public Number BitDecrement
        {
            [MethodImpl(AggressiveInlining)] get => MathF.BitDecrement(Value);
        }

        /// <summary>
        /// The smallest value that compares greater than the value.
        /// </summary>
        public Number BitIncrement
        {
            [MethodImpl(AggressiveInlining)] get => MathF.BitIncrement(Value);
        }

        /// <summary>
        /// The cube root of the number.
        /// </summary>
        public Number Cbrt
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Cbrt(Value);
        }

        /// <summary>
        /// The smallest integral value that is greater than or equal to the specified single-precision floating-point number.
        /// </summary>
        public Number Ceiling
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Ceiling(Value);
        }

        /// <summary>
        /// A value with the magnitude of this number and the sign of another number.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number CopySign(Number y) => MathF.CopySign(Value, y);

        /// <summary>
        /// Returns e raised to the specified power.
        /// </summary>
        public Number Exp
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Exp(Value);
        }

        /// <summary>
        /// The largest integral value less than or equal to this value.
        /// </summary>
        public Number Floor
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Floor(Value);
        }

        /// <summary>
        /// Returns (Value * y) + z, rounded as one ternary operation.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number FusedMultiplyAdd(Number y, Number z) => MathF.FusedMultiplyAdd(Value, y, z);

        /// <summary>
        /// The remainder resulting from the division of by another number.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number IEEERemainder(Number y) => MathF.IEEERemainder(Value, y);

        /// <summary>
        /// The base 2 integer logarithm of the number.
        /// </summary>
        public Number ILogB
        {
            [MethodImpl(AggressiveInlining)] get => MathF.ILogB(Value);
        }

        /// <summary>
        /// The logarithm of the number in the base.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Log(Number newBase) => MathF.Log(Value, newBase);

        /// <summary>
        /// The natural (base e) logarithm of the number.
        /// </summary>
        public Number NaturalLog
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Log(Value);
        }

        /// <summary>
        /// The base 10 logarithm of the number.
        /// </summary>
        public Number Log10
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Log10(Value);
        }

        /// <summary>
        /// The base 2 logarithm of the number.
        /// </summary>
        public Number Log2
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Log2(Value);
        }

        /// <summary>
        /// The larger of two single-precision floating-point numbers.
        /// </summary>  
        [MethodImpl(AggressiveInlining)]
        public Number Max(Number other) => MathF.Max(Value, other);

        /// <summary>
        /// The larger magnitude of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number MaxMagnitude(Number other) => MathF.MaxMagnitude(Value, other);

        /// <summary>
        /// The smaller of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Min(Number other) => MathF.Min(Value, other);

        /// <summary>
        /// The smaller magnitude of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number MinMagnitude(Number other) => MathF.MinMagnitude(Value, other);

        /// <summary>
        /// The number raised to the specified power.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Pow(Number power) => MathF.Pow(Value, power);

        /// <summary>
        /// Clamp number within the range of two numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Clamp(Number a, Number b) => MathF.Max(a, MathF.Min(Value, b));

        /// <summary>
        /// The reciprocal of the number.
        /// </summary>
        public Number Reciprocal
        {
            [MethodImpl(AggressiveInlining)] get => 1f / Value;
        }

        /// <summary>
        /// An estimate of the reciprocal of the number.
        /// </summary>
        public Number ReciprocalEstimate
        {
            [MethodImpl(AggressiveInlining)] get => MathF.ReciprocalEstimate(Value);
        }

        /// <summary>
        /// An estimate of the reciprocal square root of the number.
        /// </summary>
        public Number ReciprocalSqrtEstimate
        {
            [MethodImpl(AggressiveInlining)] get => MathF.ReciprocalSqrtEstimate(Value);
        }

        /// <summary>
        /// Rounds the value to an integer using the specified rounding convention.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number RoundToZero(int digits = 0) => MathF.Round(Value, digits, MidpointRounding.ToZero);

        /// <summary>
        /// Rounds the value to an integer using the specified rounding convention.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number RoundAwayFromZero(int digits = 0) => MathF.Round(Value, digits, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Rounds the value to the nearest integral value, rounding midpoint values to the nearest even number.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Round(int digits = 0) => MathF.Round(Value, digits);

        /// <summary>
        /// Returns x multiplied by 2 raised to the power of n, computed efficiently.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number ScaleB(Integer n) => MathF.ScaleB(Value, n);

        /// <summary>
        /// An integer that indicates the sign of the number.
        /// </summary>
        public Integer Sign
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Sign(Value);
        }

        /// <summary>
        /// The square root of the number.
        /// </summary>
        public Number Sqrt
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Sqrt(Value);
        }

        /// <summary>
        /// The square root of the number.
        /// </summary>
        public Number Square
        {
            [MethodImpl(AggressiveInlining)]
            get => Value * Value;
        }

        /// <summary>
        /// Calculates the integral part of the single-precision floating-point number.
        /// </summary>
        public Number Truncate
        {
            [MethodImpl(AggressiveInlining)] get => MathF.Truncate(Value);
        }

        public Number ReciprocalSquareRootEstimate
        {
            [MethodImpl(AggressiveInlining)] get => MathF.ReciprocalSqrtEstimate(Value);
        }

        public static Number MinValue = float.MinValue;
        public static Number MaxValue = float.MaxValue;
        public static Number Zero = 0;
        public static Number One = 1;

        // TODO: Figure out why these aren't being provided by Plato

        public Number Cubic(Number a, Number b, Number c, Number d) => a.Pow3 * this + b.Pow2 * this + c * this + d;
        public Number Linear(Number a, Number b) => a * this + b;
        public Number Quadratic(Number a, Number b, Number c) => a.Pow2 * this + b * this + c;

    }
}
