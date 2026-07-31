using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;
using SNQuaternion = System.Numerics.Quaternion;

namespace Ara3D.Geometry
{
    [DataContract]
    public partial struct Quaternion
    {
        // Fields 

        [DataMember] public readonly SNQuaternion Value;

        // Constructor

        [MethodImpl(AggressiveInlining)]
        public Quaternion(SNQuaternion v) => Value = v;

        [MethodImpl(AggressiveInlining)]
        public Quaternion(Number x, Number y, Number z, Number w) => Value = new(x, y, z, w);

        // Properties

        public Number X
        {
            [MethodImpl(AggressiveInlining)] get => Value.X;
        }

        public Number Y
        {
            [MethodImpl(AggressiveInlining)] get => Value.Y;
        }

        public Number Z
        {
            [MethodImpl(AggressiveInlining)] get => Value.Z;
        }

        public Number W
        {
            [MethodImpl(AggressiveInlining)] get => Value.W;
        }

        // Immutable "setters"

        [MethodImpl(AggressiveInlining)]
        public Quaternion WithX(Number x)
            => new(x, Y, Z, W);

        [MethodImpl(AggressiveInlining)]
        public Quaternion WithY(Number y)
            => new(X, y, Z, W);

        [MethodImpl(AggressiveInlining)]
        public Quaternion WithZ(Number z)
            => new(X, Y, z, W);

        [MethodImpl(AggressiveInlining)]
        public Quaternion WithW(Number w)
            => new(X, Y, Z, w);

        // Implicit casts 

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNQuaternion(Quaternion v)
            => v.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Quaternion(SNQuaternion v)
            => Unsafe.As<SNQuaternion, Quaternion>(ref v);

        // Operators

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator +(Quaternion a, Quaternion b)
            => a.Value + b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator -(Quaternion a, Quaternion b)
            => a.Value - b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator -(Quaternion a)
            => -a.Value;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator *(Quaternion a, Quaternion b)
            => a.Value * b.Value;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator *(Quaternion a, Number scalar)
            => a.Value * scalar;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator /(Quaternion a, Quaternion b)
            => a.Value / b.Value;

        // Forwarded static methods

        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromAxisAngle(Vector3 axis, Angle angle)
            => SNQuaternion.CreateFromAxisAngle(axis, angle);

        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
            => SNQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll);

        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromRotationMatrix(Matrix4x4 matrix)
            => SNQuaternion.CreateFromRotationMatrix(matrix);

        //==
        // Static methods converted into instance methods 

        [MethodImpl(AggressiveInlining)]
        public Quaternion Concatenate(Quaternion value2)
            => SNQuaternion.Concatenate(Value, value2.Value);

        [MethodImpl(AggressiveInlining)]
        public Number Dot(Quaternion quaternion2)
            => SNQuaternion.Dot(Value, quaternion2.Value);

        [MethodImpl(AggressiveInlining)]
        public Quaternion Lerp(Quaternion quaternion2, Number amount)
            => SNQuaternion.Lerp(Value, quaternion2.Value, amount);

        [MethodImpl(AggressiveInlining)]
        public Quaternion Slerp(Quaternion quaternion2, Number amount)
            => SNQuaternion.Slerp(Value, quaternion2.Value, amount);

        // Properties

        public Number Length
        {
            [MethodImpl(AggressiveInlining)] get => Value.Length();
        }

        public Number LengthSquared
        {
            [MethodImpl(AggressiveInlining)] get => Value.LengthSquared();
        }

        public Quaternion Normalize
        {
            [MethodImpl(AggressiveInlining)] get => SNQuaternion.Normalize(Value);
        }

        public Quaternion Conjugate
        {
            [MethodImpl(AggressiveInlining)] get => SNQuaternion.Conjugate(Value);
        }

        public Quaternion Inverse
        {
            [MethodImpl(AggressiveInlining)] get => SNQuaternion.Inverse(Value);
        }
    }

    public static partial class QuaternionExtensions2
    {
        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromAxisAngle(this Quaternion _, Vector3 axis, Angle angle)
            => SNQuaternion.CreateFromAxisAngle(axis, angle);

        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromYawPitchRoll(this Quaternion _, Angle yaw, Angle pitch, Angle roll)
            => SNQuaternion.CreateFromYawPitchRoll(yaw, pitch, roll);

        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromRotationMatrix(this Quaternion _, Matrix4x4 matrix)
            => SNQuaternion.CreateFromRotationMatrix(matrix);
    }
}