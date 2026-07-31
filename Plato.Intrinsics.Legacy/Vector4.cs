using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;
using SNVector4 = System.Numerics.Vector4;

namespace Ara3D.Geometry
{
    [DataContract]
    public partial struct Vector4 
    {
        // Fields

        [DataMember] public readonly SNVector4 Value;

        // Constructor

        [MethodImpl(AggressiveInlining)]
        public Vector4(SNVector4 v) => Value = v;

        [MethodImpl(AggressiveInlining)]
        public Vector4(Number x, Number y, Number z, Number w) => Value = new(x, y, z, w);

        [MethodImpl(AggressiveInlining)]
        public Vector4(Number x) => Value = new(x);

        //-------------------------------------------------------------------------------------
        // Indexer
        //-------------------------------------------------------------------------------------

        // Properties

        public Number X
        {
            [MethodImpl(AggressiveInlining)]
            get => Value.X;
        }

        public Number Y
        {
            [MethodImpl(AggressiveInlining)]
            get => Value.Y;
        }

        public Number Z
        {
            [MethodImpl(AggressiveInlining)]
            get => Value.Z;
        }

        public Number W
        {
            [MethodImpl(AggressiveInlining)]
            get => Value.W;
        }

        public const int Count = 4;

        // Immutable "setters"

        [MethodImpl(AggressiveInlining)]
        public Vector4 WithX(Number x)
            => new(x, Y, Z, W);

        [MethodImpl(AggressiveInlining)]
        public Vector4 WithY(Number y)
            => new(X, y, Z, W);

        [MethodImpl(AggressiveInlining)]
        public Vector4 WithZ(Number z)
            => new(X, Y, z, W);

        [MethodImpl(AggressiveInlining)]
        public Vector4 WithW(Number w)
            => new(X, Y, Z, w);

        // Implicit casts 

        [MethodImpl(AggressiveInlining)]
        public static Vector4 FromSystem(SNVector4 v) => Unsafe.As<SNVector4, Vector4>(ref v);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNVector4(Vector4 v) => v.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Vector4(SNVector4 v) => FromSystem(v);

        [MethodImpl(AggressiveInlining)]
        public static unsafe implicit operator Vector128<float>(Vector4 v) => *(Vector128<float>*)&v;

        [MethodImpl(AggressiveInlining)]
        public static unsafe implicit operator Vector4(Vector128<float> v) => *(Vector4*)&v;

        // Static operators  

        /// <summary>
        /// Adds two Vector4D instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator +(Vector4 left, Vector4 right) => left.Value + right.Value;

        /// <summary>
        /// Subtracts the right Vector4D from the left Vector4D.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator -(Vector4 left, Vector4 right) => left.Value - right.Value;

        /// <summary>
        /// Multiplies two Vector4D instances element-wise.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, Vector4 right) => left.Value * right.Value;

        /// <summary>
        /// Multiplies a Vector4D by a scalar.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator *(Vector4 left, Number scalar) => left.Value * scalar;

        /// <summary>
        /// Multiplies a scalar by a Vector4D.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator *(Number scalar, Vector4 right) => scalar * right.Value;

        /// <summary>
        /// Divides the left Vector4D by the right Vector4D element-wise.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator /(Vector4 left, Vector4 right) => left.Value / right.Value;

        /// <summary>
        /// Divides a Vector4D by a scalar.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator /(Vector4 left, Number scalar) => left.Value / scalar;
        
        /// <summary>
        /// Pairwise modulo operator. 
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator %(Vector4 left, Vector4 right)
            => new(left.X % right.X, left.Y % right.Y, left.Z % right.Z, left.W % right.W);

        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator %(Vector4 left, Number scalar) 
            => left % new Vector4(scalar);

        /// <summary>
        /// Negates the specified Vector4D.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator -(Vector4 value) => -value.Value;
        
        /// <summary>
        /// Returns the dot product of two <see cref="Vector4"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Dot(Vector4 right) => SNVector4.Dot(Value, right);

        /// <summary>
        /// Returns the Euclidean distance between two <see cref="Vector4"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number Distance(Vector4 value2) => SNVector4.Distance(Value, value2);

        /// <summary>
        /// Returns the squared Euclidean distance between two <see cref="Vector4"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Number DistanceSquared(Vector4 value2) => SNVector4.DistanceSquared(Value, value2);

        /// <summary>
        /// Returns a vector that clamps each element of the <see cref="Vector4"/> between the corresponding elements of the minimum and maximum vectors.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector4 Clamp(Vector4 min, Vector4 max) => SNVector4.Clamp(Value, min, max);

        /// <summary>
        /// Returns a normalized version of the specified <see cref="Vector4"/>.
        /// </summary>
        public Vector4 Normalize
        {
            [MethodImpl(AggressiveInlining)] get => SNVector4.Normalize(Value);
        }

        /// <summary>
        /// Returns the length of the <see cref="Vector4"/>.
        /// </summary>
        public Number Length
        {
            [MethodImpl(AggressiveInlining)] get => Value.Length();
        }

        /// <summary>
        /// Returns the squared length of the <see cref="Vector4"/>.
        /// </summary>
        public Number LengthSquared
        {
            [MethodImpl(AggressiveInlining)] get => Value.LengthSquared();
        }

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each element in the specified <see cref="Vector4"/>.
        /// </summary>
        public Vector4 Abs
        {
            [MethodImpl(AggressiveInlining)] get => SNVector4.Abs(Value);
        }

        /// <summary>
        /// Returns the square root of each element in the specified <see cref="Vector4"/>.
        /// </summary>
        public Vector4 Sqrt
        {
            [MethodImpl(AggressiveInlining)] get => SNVector4.SquareRoot(Value);
        }

        /// <summary>
        /// Transforms a <see cref="Vector4"/> by a 4x4 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector4 Transform(Matrix4x4 matrix) => SNVector4.Transform(Value, matrix);

        /// <summary>
        /// Transforms a <see cref="Vector4"/> by a quaternion rotation.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector4 Transform(Quaternion rotation) => SNVector4.Transform(Value, rotation);
        
        /// <summary>
        /// Returns the maximum of two <see cref="Vector4"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector4 Max(Vector4 value2) => SNVector4.Max(Value, value2);

        /// <summary>
        /// Returns the minimum of two <see cref="Vector4"/> instances.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public Vector4 Min(Vector4 value2) => SNVector4.Min(Value, value2);
        
        public Vector4 XYWZ => new(X, Y, W, Z);
        public Vector4 XYZW => new(X, Y, Z, W);
        public Vector4 XWYZ => new(X, W, Y, Z);
        public Vector4 XWZY => new(X, W, Z, Y);
        public Vector4 XZYW => new(X, Z, Y, W);
        public Vector4 XZWY => new(X, Z, W, Y);

        public Vector4 YXZW => new(Y, X, Z, W);
        public Vector4 YXWZ => new(Y, X, W, Z);
        public Vector4 YZXW => new(Y, Z, X, W);
        public Vector4 YZWX => new(Y, Z, W, X);
        public Vector4 YWXZ => new(Y, W, X, Z);
        public Vector4 YWZX => new(Y, W, Z, X);

        public Vector4 ZXYW => new(Z, X, Y, W);
        public Vector4 ZXWY => new(Z, X, W, Y);
        public Vector4 ZYXW => new(Z, Y, X, W);
        public Vector4 ZYWX => new(Z, Y, W, X);
        public Vector4 ZWXY => new(Z, W, X, Y);
        public Vector4 ZWYX => new(Z, W, Y, X);

        public Vector4 WXZY => new(W, X, Z, Y);
        public Vector4 WYZX => new(W, Y, Z, X);
        public Vector4 WYXZ => new(W, Y, X, Z);
        public Vector4 WZXY => new(W, Z, X, Y);
        public Vector4 WZYX => new(W, Z, Y, X);
    }
}
