using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A simple wrapper around the built-in <c>float</c> type,
    /// forwarding all arithmetic and common methods to <c>float</c>.
    /// The Plato math intrinsics live in <see cref="NumberIntrinsics"/> as extension methods
    /// (all-extension-methods runtime: struct = field + constructor + operators + conversions).
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

        /// <summary>The implicit Number -> Angle conversion as a member, so the generated
        /// conversion forwarder (declared in stdlib-legacy/intrinsics.plato) has a target -
        /// same pattern as Integer.Number.</summary>
        public Angle Angle { [MethodImpl(AggressiveInlining)] get => new(Value); }

        public static Number MinValue = float.MinValue;
        public static Number MaxValue = float.MaxValue;
        public static Number Zero = 0;
        public static Number One = 1;

        // The mathematical constants the forward stdlib declares as `_`-receiver intrinsics
        // (`Pi(_: Number): Number`, stdlib/intrinsics-scalars.library.plato). A `_` receiver is
        // Plato's type-level idiom, so a moved extension body re-qualifies the bare name to a
        // STATIC read on the wrapper (`Ara3D.Geometry.Number.Tau`) — the same shape as the
        // limits above. See NumberIntrinsics for the value-receiver shape (`0.0.Pi`).
        public static Number Pi = MathF.PI;
        public static Number Tau = MathF.Tau;
        public static Number E = MathF.E;
        public static Number Epsilon = float.Epsilon;
    }

    /// <summary>
    /// Plato math intrinsics for <see cref="Number"/>, as extension methods (the
    /// all-extension-methods runtime). Call sites are unchanged: <c>x.Abs()</c> binds to an
    /// extension exactly as it did to the former instance method.
    /// </summary>
    public static class NumberIntrinsics
    {
        //-------------------------------------------------------------------------------
        // Math Intrinsic Functions
        //-------------------------------------------------------------------------------

        /// <summary>
        /// The absolute value of a single-precision floating-point number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Abs(this Number self) => MathF.Abs(self.Value);

        /// <summary>
        /// The angle whose cosine is the specified number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Angle Acos(this Number self) => MathF.Acos(self.Value);

        /// <summary>
        /// The angle whose hyperbolic cosine is the specified number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Angle Acosh(this Number self) => MathF.Acosh(self.Value);

        /// <summary>
        /// The angle whose sine is the specified number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Angle Asin(this Number self) => MathF.Asin(self.Value);

        /// <summary>
        /// The angle whose hyperbolic sine is the specified number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Angle Asinh(this Number self) => MathF.Asinh(self.Value);

        /// <summary>
        /// The angle whose tangent is the specified number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Angle Atan(this Number self) => MathF.Atan(self.Value);

        /// <summary>
        /// The angle whose tangent is the quotient of this number with another.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Angle Atan2(this Number self, Number x) => MathF.Atan2(self.Value, x);

        /// <summary>
        /// The angle whose hyperbolic tangent is the specified number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Angle Atanh(this Number self) => MathF.Atanh(self.Value);

        //-------------------------------------------------------------------------------
        // Radians trigonometric kernel
        //
        // The scalar half of the trig surface: radians in, radians out, no Angle in any
        // signature. Since the intrinsics-are-primitive-only rule (Plato 709be97) these are
        // what `foundation/intrinsics.library.plato` declares, and the public Angle-typed
        // surface is ordinary Plato in `foundation/angle-trig.library.plato` delegating here.
        // The inverse family carries the `Radians` suffix because the Angle-returning
        // overloads above share the receiver type and differ only in return type, which C#
        // cannot overload on.
        //-------------------------------------------------------------------------------

        /// <summary>The cosine of an angle given in radians.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Cos(this Number self) => MathF.Cos(self.Value);

        /// <summary>The sine of an angle given in radians.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Sin(this Number self) => MathF.Sin(self.Value);

        /// <summary>The tangent of an angle given in radians.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Tan(this Number self) => MathF.Tan(self.Value);

        /// <summary>The hyperbolic cosine of the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Cosh(this Number self) => MathF.Cosh(self.Value);

        /// <summary>The hyperbolic sine of the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Sinh(this Number self) => MathF.Sinh(self.Value);

        /// <summary>The hyperbolic tangent of the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Tanh(this Number self) => MathF.Tanh(self.Value);

        /// <summary>The angle in radians whose cosine is the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number AcosRadians(this Number self) => MathF.Acos(self.Value);

        /// <summary>The angle in radians whose hyperbolic cosine is the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number AcoshRadians(this Number self) => MathF.Acosh(self.Value);

        /// <summary>The angle in radians whose sine is the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number AsinRadians(this Number self) => MathF.Asin(self.Value);

        /// <summary>The angle in radians whose hyperbolic sine is the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number AsinhRadians(this Number self) => MathF.Asinh(self.Value);

        /// <summary>The angle in radians whose tangent is the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number AtanRadians(this Number self) => MathF.Atan(self.Value);

        /// <summary>The angle in radians whose tangent is the quotient of this value with another.</summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Atan2Radians(this Number self, Number x) => MathF.Atan2(self.Value, x);

        /// <summary>The angle in radians whose hyperbolic tangent is the value.</summary>
        [MethodImpl(AggressiveInlining)] public static Number AtanhRadians(this Number self) => MathF.Atanh(self.Value);

        /// <summary>
        /// The largest value that compares less than the value.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number BitDecrement(this Number self) => MathF.BitDecrement(self.Value);

        /// <summary>
        /// The smallest value that compares greater than the value.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number BitIncrement(this Number self) => MathF.BitIncrement(self.Value);

        /// <summary>
        /// The cube root of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Cbrt(this Number self) => MathF.Cbrt(self.Value);

        /// <summary>
        /// The smallest integral value that is greater than or equal to the specified single-precision floating-point number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Ceiling(this Number self) => MathF.Ceiling(self.Value);

        /// <summary>
        /// A value with the magnitude of this number and the sign of another number.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number CopySign(this Number self, Number y) => MathF.CopySign(self.Value, y);

        /// <summary>
        /// Returns e raised to the specified power.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Exp(this Number self) => MathF.Exp(self.Value);

        /// <summary>
        /// The largest integral value less than or equal to this value.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Floor(this Number self) => MathF.Floor(self.Value);

        /// <summary>
        /// Returns (Value * y) + z, rounded as one ternary operation.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number FusedMultiplyAdd(this Number self, Number y, Number z) => MathF.FusedMultiplyAdd(self.Value, y, z);

        /// <summary>
        /// The remainder resulting from the division of by another number.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number IEEERemainder(this Number self, Number y) => MathF.IEEERemainder(self.Value, y);

        /// <summary>
        /// The base 2 integer logarithm of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number ILogB(this Number self) => MathF.ILogB(self.Value);

        /// <summary>
        /// The logarithm of the number in the base.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Log(this Number self, Number newBase) => MathF.Log(self.Value, newBase);

        /// <summary>
        /// The natural (base e) logarithm of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number NaturalLog(this Number self) => MathF.Log(self.Value);

        /// <summary>
        /// The base 10 logarithm of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Log10(this Number self) => MathF.Log10(self.Value);

        /// <summary>
        /// The base 2 logarithm of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Log2(this Number self) => MathF.Log2(self.Value);

        /// <summary>
        /// The larger of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Max(this Number self, Number other) => MathF.Max(self.Value, other);

        /// <summary>
        /// The larger magnitude of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number MaxMagnitude(this Number self, Number other) => MathF.MaxMagnitude(self.Value, other);

        /// <summary>
        /// The smaller of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Min(this Number self, Number other) => MathF.Min(self.Value, other);

        /// <summary>
        /// The smaller magnitude of two single-precision floating-point numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number MinMagnitude(this Number self, Number other) => MathF.MinMagnitude(self.Value, other);

        /// <summary>
        /// The number raised to the specified power.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Pow(this Number self, Number power) => MathF.Pow(self.Value, power);

        /// <summary>
        /// Clamp number within the range of two numbers.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Clamp(this Number self, Number a, Number b) => MathF.Max(a, MathF.Min(self.Value, b));

        /// <summary>
        /// The reciprocal of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Reciprocal(this Number self) => 1f / self.Value;

        /// <summary>
        /// An estimate of the reciprocal of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number ReciprocalEstimate(this Number self) => MathF.ReciprocalEstimate(self.Value);

        /// <summary>
        /// An estimate of the reciprocal square root of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number ReciprocalSqrtEstimate(this Number self) => MathF.ReciprocalSqrtEstimate(self.Value);

        /// <summary>
        /// Rounds the value to an integer using the specified rounding convention.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number RoundToZero(this Number self, int digits = 0) => MathF.Round(self.Value, digits, MidpointRounding.ToZero);

        /// <summary>
        /// Rounds the value to an integer using the specified rounding convention.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number RoundAwayFromZero(this Number self, int digits = 0) => MathF.Round(self.Value, digits, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Rounds the value to the nearest integral value, rounding midpoint values to the nearest even number.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Round(this Number self, int digits = 0) => MathF.Round(self.Value, digits);

        /// <summary>
        /// Returns x multiplied by 2 raised to the power of n, computed efficiently.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number ScaleB(this Number self, Integer n) => MathF.ScaleB(self.Value, n);

        /// <summary>
        /// An integer that indicates the sign of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Integer Sign(this Number self) => MathF.Sign(self.Value);

        /// <summary>
        /// The square root of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Sqrt(this Number self) => MathF.Sqrt(self.Value);

        /// <summary>
        /// The square root of the number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Square(this Number self) => self.Value * self.Value;

        /// <summary>
        /// Calculates the integral part of the single-precision floating-point number.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public static Number Truncate(this Number self) => MathF.Truncate(self.Value);

        [MethodImpl(AggressiveInlining)] public static Number ReciprocalSquareRootEstimate(this Number self) => MathF.ReciprocalSqrtEstimate(self.Value);

        // TODO: Figure out why these aren't being provided by Plato

        // The value-receiver shape of the `_`-receiver constants (stdlib documents `0.0.Pi` as
        // reading the constant with the receiver's value ignored). Under --scalar=float the
        // receiver is the erased primitive, and an extension method does not apply the
        // float -> Number implicit conversion to its receiver, so both receiver types are
        // declared. Statics only (no generated forwarder competes: the emitter never generates
        // primitive forwarders for `_`-receiver members).
        [MethodImpl(AggressiveInlining)] public static Number Pi(this Number _) => Number.Pi;
        [MethodImpl(AggressiveInlining)] public static Number Tau(this Number _) => Number.Tau;
        [MethodImpl(AggressiveInlining)] public static Number E(this Number _) => Number.E;
        [MethodImpl(AggressiveInlining)] public static Number Epsilon(this Number _) => Number.Epsilon;
        [MethodImpl(AggressiveInlining)] public static float Pi(this float _) => MathF.PI;
        [MethodImpl(AggressiveInlining)] public static float Tau(this float _) => MathF.Tau;
        [MethodImpl(AggressiveInlining)] public static float E(this float _) => MathF.E;
        [MethodImpl(AggressiveInlining)] public static float Epsilon(this float _) => float.Epsilon;

        // The V1 stdlib generated Pow2/Pow3 as instance members of Number; the forward stdlib
        // declares neither, so Cubic/Quadratic below lose their receiver. Declared as extensions
        // so the V1 instance members still win where they exist.
        [MethodImpl(AggressiveInlining)] public static Number Pow2(this Number self) => self.Value * self.Value;
        [MethodImpl(AggressiveInlining)] public static Number Pow3(this Number self) => self.Value * self.Value * self.Value;

        public static Number Cubic(this Number self, Number a, Number b, Number c, Number d) => a.Pow3() * self + b.Pow2() * self + c * self + d;
        public static Number Linear(this Number self, Number a, Number b) => a * self + b;
        public static Number Quadratic(this Number self, Number a, Number b, Number c) => a.Pow2() * self + b * self + c;

        // Forward-stdlib scalar obligations (intrinsics-scalars.library.plato). Exact `int`
        // returns for Compare/Hash: the scalar-erased Comparable/Hashable obligations demand the
        // erased type (see the same note on Angle.Compare/Hash).
        [MethodImpl(AggressiveInlining)] public static int Compare(this Number a, Number b) => a.Value.CompareTo(b.Value);
        [MethodImpl(AggressiveInlining)] public static int Hash(this Number self) => self.Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public static Boolean IsNaN(this Number self) => float.IsNaN(self.Value);
        [MethodImpl(AggressiveInlining)] public static Boolean IsInfinite(this Number self) => float.IsInfinity(self.Value);
        [MethodImpl(AggressiveInlining)] public static Boolean IsFinite(this Number self) => float.IsFinite(self.Value);
        [MethodImpl(AggressiveInlining)] public static Integer ToInteger(this Number self) => (int)self.Value;
    }
}
