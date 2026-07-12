using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A value type that represents angles internally as radians.
    /// Separating angles from numbers, makes working with them easier, and
    /// less prone to unit-based errors.
    /// </summary>
    [DataContract]
    public partial struct Angle 
    {
        // -------------------------------------------------------------------------------
        // Field (the wrapped float)
        // -------------------------------------------------------------------------------

        [DataMember] public readonly float Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Angle(float value) 
            => Value = value;

        // -------------------------------------------------------------------------------
        // Convert to/from float
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Angle(float f) => new(f);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator float(Angle n) => n.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Angle(Number x) => new(x);
        
        [MethodImpl(AggressiveInlining)]
        public Number Radians() => Value;
        // -------------------------------------------------------------------------------
        // Operators (forward to float)
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Angle operator +(Angle a, Angle b)
            => new(a.Value + b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator -(Angle a, Angle b)
            => new(a.Value - b.Value);

        [Obsolete("This method is illegal and should not be used.", true)]
        public static Angle operator *(Angle a, Angle b)
            => throw new Exception("Multiplying two angles is not well-defined");

        [Obsolete("This method is illegal and should not be used.", true)]
        public static Angle operator /(Angle a, Angle b)
            => throw new Exception("Dividing two angles is not well-defined");

        [MethodImpl(AggressiveInlining)]
        public static Angle operator *(Angle a, Number x)
            => new(a.Value * x);

        [MethodImpl(AggressiveInlining)]
        public static Angle operator *(Number x, Angle a)
            => x * a.Value;

        [MethodImpl(AggressiveInlining)]
        public static Angle operator /(Angle a, Number x)
            => a.Value / x;

        [MethodImpl(AggressiveInlining)]
        public static Angle operator -(Angle n)
            => -n.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <(Angle a, Angle b)
            => a.Value < b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator <=(Angle a, Angle b)
            => a.Value <= b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >(Angle a, Angle b)
            => a.Value > b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Boolean operator >=(Angle a, Angle b)
            => a.Value >= b.Value;

        // -------------------------------------------------------------------------------
        // IComparable / IComparable<Angle> Implementation
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Integer CompareTo(Angle other)
            => Value.CompareTo(other.Value);

    }

    /// <summary>
    /// Behavioural intrinsics for <see cref="Angle"/> as extension methods — the all-extension-methods
    /// runtime (M5 / consolidation plan C3). Fields, constructors, operators, conversions,
    /// <see cref="Angle.Radians"/> and <see cref="Angle.CompareTo"/> stay on the struct; see
    /// docs/plato-struct-surface.md. The trigonometric functions
    /// (https://en.wikipedia.org/wiki/Trigonometric_functions) move here.
    /// </summary>
    public static class AngleIntrinsics
    {
        /// <summary>Cosine.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Cos(this Angle self) => MathF.Cos(self.Value);

        /// <summary>Hyperbolic cosine.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Cosh(this Angle self) => MathF.Cosh(self.Value);

        /// <summary>Sine.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Sin(this Angle self) => MathF.Sin(self.Value);

        /// <summary>Sine and cosine.</summary>
        [MethodImpl(AggressiveInlining)] public static (Number Sin, Number Cos) SinCos(this Angle self) => MathF.SinCos(self.Value);

        /// <summary>Hyperbolic sine.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Sinh(this Angle self) => MathF.Sinh(self.Value);

        /// <summary>The tangent.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Tan(this Angle self) => MathF.Tan(self.Value);

        /// <summary>The hyperbolic tangent.</summary>
        [MethodImpl(AggressiveInlining)] public static Number Tanh(this Angle self) => MathF.Tanh(self.Value);
    }
}
