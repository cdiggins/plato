using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;
using SNVector2 = System.Numerics.Vector2;

namespace Ara3D.Geometry
{
    [DataContract]
    public readonly partial struct Vector2 
    {
        // Fields

        [DataMember] public readonly SNVector2 Value;

        // Constructor

        [MethodImpl(AggressiveInlining)]
        public Vector2(Number x, Number y) => Value = new(x, y);

        [MethodImpl(AggressiveInlining)]
        public Vector2(Number x) => Value = new(x);

        [MethodImpl(AggressiveInlining)]
        public Vector2(SNVector2 x) => Value = x;

        //-------------------------------------------------------------------------------------
        // Properties
        //-------------------------------------------------------------------------------------

        public Number X { [MethodImpl(AggressiveInlining)] get => Value.X; }
        public Number Y { [MethodImpl(AggressiveInlining)] get => Value.Y; }

        public const int Count = 2;

        //-------------------------------------------------------------------------------------
        // Immutable "setters"
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Vector2 WithX(Number x) => new(x, Y);

        [MethodImpl(AggressiveInlining)]
        public Vector2 WithY(Number y) => new(X, y);

        // Implicit casts 
        
        [MethodImpl(AggressiveInlining)]
        public static Vector2 FromSystem(SNVector2 v) => Unsafe.As<SNVector2, Vector2>(ref v);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNVector2(Vector2 v) => v.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Vector2(SNVector2 v) => FromSystem(v);

        // Static operators  

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator +(Vector2 left, Vector2 right) => left.Value + right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator -(Vector2 left, Vector2 right) => left.Value - right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator *(Vector2 left, Vector2 right) => left.Value * right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator *(Vector2 left, Number scalar) => left.Value * scalar;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator *(Number scalar, Vector2 right) => scalar * right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator /(Vector2 left, Vector2 right) => left.Value / right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator /(Vector2 left, Number scalar) => left.Value / scalar;

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator %(Vector2 left, Vector2 right) => new(left.X % right.X, left.Y % right.Y);

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator %(Vector2 left, Number scalar) => left % new Vector2(scalar);

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator -(Vector2 value) => -value.Value;

        /// <summary>
        /// Returns the dot product of two <see cref="Vector2"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Dot(Vector2 right) => SNVector2.Dot(Value, right);

        /// <summary>
        /// Returns the Euclidean distance between two <see cref="Vector2"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Distance(Vector2 value2) => SNVector2.Distance(Value, value2);

        /// <summary>
        /// Returns the squared Euclidean distance between two <see cref="Vector2"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number DistanceSquared(Vector2 value2) => SNVector2.DistanceSquared(Value, value2);

        /// <summary>
        /// Returns a vector that clamps each element of the <see cref="Vector2"/> between the corresponding elements of the minimum and maximum vectors.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Clamp(Vector2 min, Vector2 max) => SNVector2.Clamp(Value, min, max);

        /// <summary>
        /// Returns a normalized version of the specified <see cref="Vector2"/>.
        /// </summary>
        public Vector2 Normalize
        {
            [MethodImpl(AggressiveInlining)] get => SNVector2.Normalize(Value);
        }

        /// <summary>
        /// Returns the length of the <see cref="Vector2"/>.
        /// </summary>
        public Number Length
        {
            [MethodImpl(AggressiveInlining)] get => Value.Length();
        }

        /// <summary>
        /// Returns the squared length of the <see cref="Vector2"/>.
        /// </summary>
        public Number LengthSquared
        {
            [MethodImpl(AggressiveInlining)]
            get =>  Value.LengthSquared();
        }

        /// <summary>
        /// Returns a vector that is the reflection of the specified vector off a plane defined by the specified normal.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Reflect(Vector2 normal) => SNVector2.Reflect(Value, normal);

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each element in the specified <see cref="Vector2"/>.
        /// </summary>
        public Vector2 Abs
        {
            [MethodImpl(AggressiveInlining)] get => SNVector2.Abs(Value);
        }

        /// <summary>
        /// Returns the square root of each element in the specified <see cref="Vector2"/>.
        /// </summary>
        public Vector2 Sqrt
        {
            [MethodImpl(AggressiveInlining)] get => SNVector2.SquareRoot(Value);
        }

        /// <summary>
        /// Transforms a <see cref="Vector2"/> by a 3x2 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Transform(Matrix3x2 matrix) => SNVector2.Transform(Value, matrix);

        /// <summary>
        /// Transforms a <see cref="Vector2"/> by a 4x4 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Transform(Matrix4x4 matrix) => SNVector2.Transform(Value, matrix);

        /// <summary>
        /// Transforms a <see cref="Vector2"/> by a quaternion rotation.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Transform(Quaternion rotation) => SNVector2.Transform(Value, rotation);

        /// <summary>
        /// Transforms a normal vector by a 3x2 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 TransformNormal(Matrix3x2 matrix) => SNVector2.TransformNormal(Value, matrix);

        /// <summary>
        /// Transforms a normal vector by a 4x4 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 TransformNormal(Matrix4x4 matrix) => SNVector2.TransformNormal(Value, matrix);

        /// <summary>
        /// Returns the maximum of two <see cref="Vector2"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Max(Vector2 value2) => SNVector2.Max(Value, value2);

        /// <summary>
        /// Returns the minimum of two <see cref="Vector2"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector2 Min(Vector2 value2) => SNVector2.Min(Value, value2);

        /// <summary>
        /// Returns the minimum of two <see cref="Vector2"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static float Dot(Vector2 value1, Vector2 value2) 
            => SNVector2.Dot(value1, value2);

    }
}
