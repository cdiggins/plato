using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using static System.Runtime.CompilerServices.MethodImplOptions;
using SNQuaternion = System.Numerics.Quaternion;

namespace Ara3D.Geometry
{
    /// <summary>
    /// Behaviour-only partial for the GENERATED <c>Quaternion</c> struct (plato-365).
    ///
    /// The shape is the stdlib declaration (`stdlib/foundation/rotations.types.plato`): the four
    /// Numbers X, Y, Z, W, component-for-component with System.Numerics.Quaternion. The rotation
    /// algebra below still runs on System.Numerics, reached through <see cref="Sys"/>.
    ///
    /// Nothing here may declare a field, a constructor, an X/Y/Z/W, a `With*`, or an equality
    /// override: the generated partial has them, and a second copy is a duplicate-member error.
    /// </summary>
    public partial struct Quaternion
    {
        /// <summary>This rotation as a System.Numerics quaternion: the same four floats.</summary>
        internal SNQuaternion Sys
        {
            [MethodImpl(AggressiveInlining)]
            get => new SNQuaternion(X, Y, Z, W);
        }

        /// <summary>The inverse of <see cref="Sys"/>.</summary>
        [MethodImpl(AggressiveInlining)]
        internal static Quaternion FromSys(SNQuaternion q)
            => new Quaternion(q.X, q.Y, q.Z, q.W);

        // Forward-stdlib Hashable obligation; exact `int` return (see Angle.Hash).
        [MethodImpl(AggressiveInlining)] public int Hash() => Sys.GetHashCode();

        // The intrinsic bridge. Public because every handwritten body in this project converts
        // across it; generated code never names SNQuaternion.

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNQuaternion(Quaternion v) => v.Sys;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Quaternion(SNQuaternion v) => FromSys(v);

        // Operators

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator +(Quaternion a, Quaternion b)
            => a.Sys + b.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator -(Quaternion a, Quaternion b)
            => a.Sys - b.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator -(Quaternion a)
            => -a.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator *(Quaternion a, Quaternion b)
            => a.Sys * b.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator *(Quaternion a, Number scalar)
            => a.Sys * scalar;

        [MethodImpl(AggressiveInlining)]
        public static Quaternion operator /(Quaternion a, Quaternion b)
            => a.Sys / b.Sys;

        // Rotation-on-the-left. The stdlib declares `Multiply(rotation, self)` as an explicit
        // alias for `Transform(self, rotation)` (stdlib/intrinsics-vectors.library.plato).

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator *(Quaternion rotation, Vector2 self)
            => self.Transform(rotation);

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator *(Quaternion rotation, Vector3 self)
            => self.Transform(rotation);

        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator *(Quaternion rotation, Vector4 self)
            => self.Transform(rotation);

        // Forwarded static methods

        [MethodImpl(AggressiveInlining)]
        public static Quaternion CreateFromAxisAngle(Vector3 axis, Angle angle)
            => SNQuaternion.CreateFromAxisAngle(axis, angle);

        // CreateFromYawPitchRoll / CreateFromRotationMatrix are NOT here: the forward stdlib has
        // reference bodies for both, so the generated partial declares them. The extension-method
        // twins below (receiver-taking, legacy-generation shape) are a different overload set and
        // do not collide.

        //==
        // Static methods converted into instance methods 

        [MethodImpl(AggressiveInlining)]
        public Quaternion Concatenate(Quaternion value2)
            => SNQuaternion.Concatenate(Sys, value2.Sys);

        [MethodImpl(AggressiveInlining)]
        public Number Dot(Quaternion quaternion2)
            => SNQuaternion.Dot(Sys, quaternion2.Sys);

        [MethodImpl(AggressiveInlining)]
        public Quaternion Lerp(Quaternion quaternion2, Number amount)
            => SNQuaternion.Lerp(Sys, quaternion2.Sys, amount);

        // The `float`-amount Lerp is NOT here: the forward stdlib has a reference body for it, so
        // the generated partial declares it with the scalar-erased signature.

        [MethodImpl(AggressiveInlining)]
        public Quaternion Slerp(Quaternion quaternion2, Number amount)
            => SNQuaternion.Slerp(Sys, quaternion2.Sys, amount);

        // Properties

        // `Length()` is NOT here: the forward stdlib has a reference body for it (sqrt of the
        // component sum of squares), so the generated partial declares it.

        [MethodImpl(AggressiveInlining)] public Number LengthSquared() => Sys.LengthSquared();

        [MethodImpl(AggressiveInlining)] public Quaternion Normalize() => SNQuaternion.Normalize(Sys);

        [MethodImpl(AggressiveInlining)] public Quaternion Conjugate() => SNQuaternion.Conjugate(Sys);

        [MethodImpl(AggressiveInlining)] public Quaternion Inverse() => SNQuaternion.Inverse(Sys);
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