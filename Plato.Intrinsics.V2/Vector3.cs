using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;
using SNVector3 = System.Numerics.Vector3;

namespace Ara3D.Geometry
{
    [DataContract]
    public readonly partial struct Vector3 
    {
        // Fields

        [DataMember] public readonly SNVector3 Value;

        // Constructor

        [MethodImpl(AggressiveInlining)]
        public Vector3(SNVector3 v) => Value = v;

        [MethodImpl(AggressiveInlining)]
        public Vector3(Number x, Number y, Number z) => Value = new(x, y, z);

        [MethodImpl(AggressiveInlining)]
        public Vector3(Number x) => Value = new(x);

        // Properties

        public Number X { [MethodImpl(AggressiveInlining)] get => Value.X; }

        public Number Y { [MethodImpl(AggressiveInlining)] get => Value.Y; }

        public Number Z { [MethodImpl(AggressiveInlining)] get => Value.Z; }
   
        public const int Count = 3;

        // Immutable "setters"

        [MethodImpl(AggressiveInlining)]
        public Vector3 WithX(Number x)
            => new(x, Y, Z);

        [MethodImpl(AggressiveInlining)]
        public Vector3 WithY(Number y)
            => new(X, y, Z);

        [MethodImpl(AggressiveInlining)]
        public Vector3 WithZ(Number z)
            => new(X, Y, z);

        // Implicit casts 

        [MethodImpl(AggressiveInlining)]
        public static Vector3 FromSystem(SNVector3 v) => Unsafe.As<SNVector3, Vector3>(ref v);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNVector3(Vector3 v) => v.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Vector3(SNVector3 v) => FromSystem(v);
        
        // Static operators  

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator +(Vector3 left, Vector3 right) => left.Value + right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator -(Vector3 left, Vector3 right) => left.Value - right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator *(Vector3 left, Vector3 right) => left.Value * right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator *(Vector3 left, Number scalar) => left.Value * scalar;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator *(Number scalar, Vector3 right) => scalar * right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator /(Vector3 left, Vector3 right) => left.Value / right.Value;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator /(Vector3 left, Number scalar) => left.Value / scalar;

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator %(Vector3 left, Vector3 right) => new(left.X % right.X, left.Y % right.Y, left.Z % right.Z);

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator %(Vector3 left, Number scalar) 
            => left % new Vector3(scalar);

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator -(Vector3 value) => -value.Value;
        
        /// <summary>
        /// Returns the Euclidean distance between two <see cref="Vector3"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Distance(Vector3 value2) => SNVector3.Distance(Value, value2);

        /// <summary>
        /// Returns the squared Euclidean distance between two <see cref="Vector3"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number DistanceSquared(Vector3 value2) => SNVector3.DistanceSquared(Value, value2);

        /// <summary>
        /// Returns a vector that clamps each element of the <see cref="Vector3"/> between the corresponding elements of the minimum and maximum vectors.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Clamp(Vector3 min, Vector3 max) => SNVector3.Clamp(Value, min, max);

        /// <summary>
        /// Returns a normalized version of the specified <see cref="Vector3"/>.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public Vector3 Normalize() => SNVector3.Normalize(Value);

        /// <summary>
        /// Returns the length of the <see cref="Vector3"/>.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public Number Length() => Value.Length();

        /// <summary>
        /// Returns the squared length of the <see cref="Vector3"/>.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public Number LengthSquared() => Value.LengthSquared();

        /// <summary>
        /// Returns a vector that is the reflection of the specified vector off a plane defined by the specified normal.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Reflect(Vector3 normal) => SNVector3.Reflect(Value, normal);

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each element.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public Vector3 Abs() => SNVector3.Abs(Value);

        /// <summary>
        /// Returns the square root of each element.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public Vector3 Sqrt() => SNVector3.SquareRoot(Value);

        /// <summary>
        /// Transforms by a 4x4 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Transform(Matrix4x4 matrix) => SNVector3.Transform(Value, matrix);

        /// <summary>
        /// Transforms by a quaternion rotation.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Transform(Quaternion rotation) => SNVector3.Transform(Value, rotation);

        /// <summary>
        /// Transforms a normal vector by a 4x4 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 TransformNormal(Matrix4x4 matrix) => SNVector3.TransformNormal(Value, matrix);
        
        /// <summary>
        /// Returns the maximum of two <see cref="Vector3"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Max(Vector3 value2) => SNVector3.Max(Value, value2);

        /// <summary>
        /// Returns the minimum of two <see cref="Vector3"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Min(Vector3 value2) => SNVector3.Min(Value, value2);

        /// <summary>
        /// Returns the dot product of this and another <see cref="Vector3"/>.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Dot(Vector3 right) => SNVector3.Dot(Value, right);

        /// <summary>
        /// Returns the cross product of this and another <see cref="Vector3"/>.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector3 Cross(Vector3 right) => SNVector3.Cross(Value, right);

        /// <summary>
        /// Returns the dot product of two <see cref="Vector3"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Number Dot(Vector3 left, Vector3 right) => SNVector3.Dot(left, right);

        /// <summary>
        /// Returns the dot product of two <see cref="Vector3"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector3 Cross(Vector3 left, Vector3 right) => SNVector3.Cross(left, right);
    }

    // NOTE: the Dot/Cross extension methods (for Vector3 and System.Numerics.Vector3
    // receivers) that used to live here are now generated by the Plato compiler in
    // Plato.Generated\_Vector3.g.cs (class Vector3Extensions); they forward to the
    // instance Dot/Cross above, matching the Vector2 intrinsic pattern.
    public static class Vector3_Extensions
    {
        [MethodImpl(AggressiveInlining)]
        public static Vector3 AnyPerpendicular(this Vector3 v)
        {
            // Constructed directly (not Vector3.UnitZ): the generated UnitZ member's shape
            // varies by emitter style (--methods makes it a static method), and this handwritten
            // file must compile against every style.
            if (v.AlmostZero())
                return new Vector3(0, 0, 1);

            // Choose the smallest component to avoid degeneracy
            if (Math.Abs(v.X) < Math.Abs(v.Y))
            {
                return Math.Abs(v.X) < Math.Abs(v.Z)
                    ? new Vector3(0, -v.Z, v.Y).Normalize()
                    : new Vector3(-v.Y, v.X, 0).Normalize();
            }

            return Math.Abs(v.Y) < Math.Abs(v.Z)
                ? new Vector3(v.Z, 0, -v.X).Normalize()
                : new Vector3(-v.Y, v.X, 0).Normalize();
        }

    }
}
