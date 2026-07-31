using System.Runtime.CompilerServices;

namespace Ara3D.Geometry
{
    /// <summary>
    /// The Plato.Intrinsics.V2 shared sources call a handful of extension methods that normally
    /// come from the GENERATED library (which PlatoTests deliberately does not compile).
    /// IntrinsicsV2SurfaceTests only reflects over the V2 surface, never executes these, so
    /// minimal stand-ins are enough to compile. If a future V2 change calls a new generated
    /// extension, the build error lands here: add another one-liner.
    /// </summary>
    internal static class IntrinsicsV2TestShims
    {
        public static bool AlmostZero(this Vector3 v) => v.Value.LengthSquared() < 1e-12f;
    }

    // ------------------------------------------------------------------------------------------
    // plato-365: generated SHAPE stand-ins.
    //
    // Angle and Plane are no longer CSharpWriter.PrimitiveTypes entries, so the writer now emits
    // their fields and constructors from the stdlib declaration and the V2 file supplies only
    // behaviour. These partials reproduce exactly the generated shape (field names, field types,
    // constructor signature) so that compiling V2 alone in this assembly still type-checks the
    // handwritten bodies against it. THIS IS THE COMPILE GATE for the V2 side of plato-365: if a
    // body reaches for a member the generated struct does not have, it fails here rather than a
    // thousand generated files downstream.
    //
    // Keep in sync with the declarations:
    //   Angle  — stdlib/foundation/quantities-geometric.types.plato
    //   Plane  — stdlib/geometry/lines-planes.types.plato
    // ------------------------------------------------------------------------------------------

    public partial struct Angle
    {
        public readonly float Radians;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Angle(float radians) => Radians = radians;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Angle WithRadians(float radians) => new Angle(radians);

        // The single-field implicit pair the writer emits for any one-field struct.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(Angle self) => self.Radians;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Angle(float value) => new Angle(value);
    }

    public partial struct Plane
    {
        public readonly Direction3D Normal;
        public readonly float Distance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(Direction3D normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane WithNormal(Direction3D normal) => new Plane(normal, Distance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane WithDistance(float distance) => new Plane(Normal, distance);
    }

    public partial struct Quaternion
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Quaternion WithX(float x) => new Quaternion(x, Y, Z, W);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Quaternion WithY(float y) => new Quaternion(X, y, Z, W);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Quaternion WithZ(float z) => new Quaternion(X, Y, z, W);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Quaternion WithW(float w) => new Quaternion(X, Y, Z, w);
    }

    public partial struct Matrix3x2
    {
        public readonly Number2 Row1;
        public readonly Number2 Row2;
        public readonly Number2 Row3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x2(Number2 row1, Number2 row2, Number2 row3)
        {
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix3x2 WithRow1(Number2 v) => new Matrix3x2(v, Row2, Row3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix3x2 WithRow2(Number2 v) => new Matrix3x2(Row1, v, Row3);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix3x2 WithRow3(Number2 v) => new Matrix3x2(Row1, Row2, v);
    }

    public partial struct Matrix4x4
    {
        public readonly Number4 Row1;
        public readonly Number4 Row2;
        public readonly Number4 Row3;
        public readonly Number4 Row4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(Number4 row1, Number4 row2, Number4 row3, Number4 row4)
        {
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
            Row4 = row4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix4x4 WithRow1(Number4 v) => new Matrix4x4(v, Row2, Row3, Row4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix4x4 WithRow2(Number4 v) => new Matrix4x4(Row1, v, Row3, Row4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix4x4 WithRow3(Number4 v) => new Matrix4x4(Row1, Row2, v, Row4);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public Matrix4x4 WithRow4(Number4 v) => new Matrix4x4(Row1, Row2, Row3, v);
    }

    /// <summary>Stand-in for the generated <c>Number2</c> (`stdlib/foundation/vectors-tuples.types.plato`),
    /// including the Vector2 bridge the writer emits for it.</summary>
    public partial struct Number2
    {
        public readonly float X;
        public readonly float Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Number2(float x, float y) { X = x; Y = y; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector2(Number2 self) => new Vector2(self.X, self.Y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Number2(Vector2 value) => new Number2(value.X, value.Y);
    }

    /// <summary>Stand-in for the generated <c>Number4</c>, including the Vector4 bridge.</summary>
    public partial struct Number4
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public readonly float W;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Number4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector4(Number4 self) => new Vector4(self.X, self.Y, self.Z, self.W);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Number4(Vector4 value) => new Number4(value.X, value.Y, value.Z, value.W);
    }

    /// <summary>Stand-in for the generated <c>Direction3D</c> (a unit <c>Vector3D</c>), including
    /// the implicit bridge pair the writer emits for it (CSharpConcreteTypeWriter
    /// .IntrinsicVectorBridges). Only the conversions matter to the V2 bodies.</summary>
    public partial struct Direction3D
    {
        public readonly Vector3 Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Direction3D(Vector3 value) => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Vector3(Direction3D self) => self.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Direction3D(Vector3 value) => new Direction3D(value);
    }
}
