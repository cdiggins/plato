using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// This is a wrapper around Vector256&lt;float&gt; that provides a more user-friendly API.
    /// Note that the Vector256 class can be found in the Runtime.Intrinsics namespace, and
    /// has some design difference from Vector2, Vector3, and Vector4. One of the more notable
    /// differences is that a Vector8 is sometimes intended to be used as a bit-mask  
    /// and there are a number of bit-oriented functions around them. The intent of the
    /// Vector256 type was as a wrapper around SIMD operations, and less as a general-purpose vector type.
    /// In the end, we decided to put it in the same namespace, and expose a similar API as Vector4.  
    /// </summary>
    [DataContract]
    public partial struct Vector8
    {
        [DataMember] public readonly Vector256<float> Value;

        //-------------------------------------------------------------------------------------
        // Constructors
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Vector8(Vector256<float> value) => Value = value;

        [MethodImpl(AggressiveInlining)]
        public Vector8(Number scalar) => Value = Vector256.Create(scalar.Value);

        [MethodImpl(AggressiveInlining)]
        public Vector8(Number f0, Number f1, Number f2, Number f3, Number f4, Number f5, Number f6, Number f7)
            => Value = Vector256.Create(f0, f1, f2, f3, f4, f5, f6, f7);

        [MethodImpl(AggressiveInlining)]
        public Vector8(Vector4 lower, Vector4 upper) => Value = Vector256.Create(lower, upper);

        //-------------------------------------------------------------------------------------
        // Implicit operators 
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Vector256<float>(Vector8 value) => value.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Vector8(Vector256<float> value) => new(value);

        //-------------------------------------------------------------------------------------
        // Properties
        //-------------------------------------------------------------------------------------

        public Number X0
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(0);
        }

        public Number X1
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(1);
        }

        public Number X2
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(2);
        }

        public Number X3
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(3);
        }

        public Number X4
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(4);
        }

        public Number X5
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(5);
        }

        public Number X6
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(6);
        }

        public Number X7
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetElement(7);
        }

        public Vector4 Lower
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetLower();
        }

        public Vector4 Upper
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetUpper();
        }

        public const int Count = 8;

        [MethodImpl(AggressiveInlining)]
        public Vector8 WithLower(Vector4 lower)
            => new(lower, Upper);

        [MethodImpl(AggressiveInlining)]
        public Vector8 WithUpper(Vector4 upper)
            => new(Lower, upper);

        //-------------------------------------------------------------------------------------
        // Operator Overloads
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator +(Vector8 left, Vector8 right) => Vector256.Add(left.Value, right.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator -(Vector8 left, Vector8 right) => Vector256.Subtract(left.Value, right.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator *(Vector8 left, Vector8 right) => Vector256.Multiply(left.Value, right.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator *(Vector8 left, Number scalar) => Vector256.Multiply(left.Value, scalar);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator *(Number scalar, Vector8 right) => Vector256.Multiply(scalar, right.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator /(Vector8 left, Vector8 right) => Vector256.Divide(left.Value, right.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator /(Vector8 left, Number scalar) => Vector256.Divide(left.Value, scalar);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator %(Vector8 left, Vector8 right)
            => new(left.Lower % right.Lower, left.Upper % right.Upper);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator %(Vector8 left, Number scalar) => left % new Vector8(scalar);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator /(Number scalar, Vector8 right) =>
            Vector256.Divide(new Vector8(scalar), right.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator -(Vector8 value) => Vector256.Negate(value.Value);

        //-------------------------------------------------------------------------------------
        // Bitwise functions
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Vector8 AndNot(Vector8 b) => Vector256.AndNot(Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator &(Vector8 a, Vector8 b) => Vector256.BitwiseAnd(a.Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator |(Vector8 a, Vector8 b) => Vector256.BitwiseOr(a.Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator ~(Vector8 a) => Vector256.OnesComplement(a.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator ^(Vector8 a, Vector8 b) => Vector256.Xor(a.Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public Vector8 ConditionalSelect(Vector8 a, Vector8 b) =>
            Vector256.ConditionalSelect(Value, a.Value, b.Value);

        //-------------------------------------------------------------------------------------
        // Comparison operators 
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator <(Vector8 a, Vector8 b) => Vector256.LessThan(a.Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator <=(Vector8 a, Vector8 b) => Vector256.LessThanOrEqual(a.Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator >(Vector8 a, Vector8 b) => Vector256.GreaterThan(a.Value, b.Value);

        [MethodImpl(AggressiveInlining)]
        public static Vector8 operator >=(Vector8 a, Vector8 b) => Vector256.GreaterThanOrEqual(a.Value, b.Value);

        //-------------------------------------------------------------------------------------
        // Comparison functions
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Vector8 Max(Vector8 other) => Vector256.Max(Value, other.Value);

        [MethodImpl(AggressiveInlining)]
        public Vector8 Min(Vector8 other) => Vector256.Min(Value, other.Value);

        //-------------------------------------------------------------------------------------
        // Basic math functions 
        //-------------------------------------------------------------------------------------

        public Vector8 Abs
        {
            [MethodImpl(AggressiveInlining)] get => Vector256.Abs(Value);
        }

        [MethodImpl(AggressiveInlining)]
        public Number Dot(Vector8 other) => Vector256.Dot(Value, other.Value);

        /// <summary>Reciprocal (1/x) of each element</summary>
        public Vector8 Reciprocal
        {
            [MethodImpl(AggressiveInlining)] get => Avx.Reciprocal(Value);
        }

        /// <summary>Approximate reciprocal of the square root of each element: 1 / sqrt(x)</summary>
        public Vector8 ReciprocalSqrtEstimate
        {
            [MethodImpl(AggressiveInlining)] get => Avx.ReciprocalSqrt(Value);
        }

        public Vector8 Sqrt
        {
            [MethodImpl(AggressiveInlining)] get => Vector256.Sqrt(Value);
        }

        /// <summary>Square each element</summary>
        public Vector8 Sqr
        {
            [MethodImpl(AggressiveInlining)] get => this * this;
        }

        public Number Sum
        {
            [MethodImpl(AggressiveInlining)] get => Vector256.Sum(Value);
        }

        public Number FirstElement
        {
            [MethodImpl(AggressiveInlining)] get => Vector256.ToScalar(Value);
        }

        //-------------------------------------------------------------------------------------
        // Pseudo-mutation operators 
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Vector8 WithElement(Integer i, Number f) => Vector256.WithElement(Value, i, f);

        //-------------------------------------------------------------------------------------
        // Minimum and maximum elements
        //-------------------------------------------------------------------------------------

        public Number MinElement
        {
            [MethodImpl(AggressiveInlining)]
            get
            {
                var minHalf = Lower.Min(Upper);
                var shuffled = minHalf.ZWXY;
                var reduced = minHalf.Min(shuffled);
                reduced = reduced.Min(reduced.YXWZ);
                return reduced.X;
            }
        }

        public Number MaxElement
        {
            [MethodImpl(AggressiveInlining)]
            get
            {
                var maxHalf = Lower.Max(Upper);
                var shuffled = maxHalf.ZWXY;
                var reduced = maxHalf.Max(shuffled);
                reduced = reduced.Max(reduced.YXWZ);
                return reduced.X;
            }
        }
    }
}