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
        
        public Number Radians
        {
            [MethodImpl(AggressiveInlining)]
            get => Value; 
        }
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

        // -------------------------------------------------------------------------------
        // Trigonometric Functions
        // https://en.wikipedia.org/wiki/Trigonometric_functions
        // -------------------------------------------------------------------------------

        /// <summary>
        /// Cosine 
        /// </summary>
        public Number Cos { [MethodImpl(AggressiveInlining)] get => MathF.Cos(Value); }

        /// <summary>
        /// Hyperbolic cosine.
        /// </summary>
        public Number Cosh { [MethodImpl(AggressiveInlining)] get => MathF.Cosh(Value); }

        /// <summary>
        /// Sine
        /// </summary>
        public Number Sin { [MethodImpl(AggressiveInlining)] get => MathF.Sin(Value); }

        /// <summary>
        /// Sine and cosine.
        /// </summary>
        public (Number Sin, Number Cos) SinCos { [MethodImpl(AggressiveInlining)] get => MathF.SinCos(Value); }

        /// <summary>
        /// Hyperbolic sine
        /// </summary>
        public Number Sinh { [MethodImpl(AggressiveInlining)] get => MathF.Sinh(Value); }

        /// <summary>
        /// The tangent.
        /// </summary>
        public Number Tan { [MethodImpl(AggressiveInlining)] get => MathF.Tan(Value); }

        /// <summary>
        /// The hyperbolic tangent. 
        /// </summary>
        public Number Tanh { [MethodImpl(AggressiveInlining)] get => MathF.Tanh(Value); }
    }
}
