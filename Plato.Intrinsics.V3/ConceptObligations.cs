using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;
using SNVector2 = System.Numerics.Vector2;
using SNVector3 = System.Numerics.Vector3;
using SNVector4 = System.Numerics.Vector4;
using SNQuaternion = System.Numerics.Quaternion;
using SNPlane = System.Numerics.Plane;

namespace Ara3D.Geometry
{
    // ============================================================================
    // V3 concept obligations: members the generated v3 concept interfaces require
    // of the handwritten primitive structs, beyond what the V2-derived intrinsic
    // files already provide.
    // ============================================================================

    public partial struct Vector2
    {
        public float Magnitude { [MethodImpl(AggressiveInlining)] get => Value.Length(); }
        [MethodImpl(AggressiveInlining)] public int Hash() => Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public Vector2 Lerp(Vector2 b, float t) => SNVector2.Lerp(Value, b, t);
        [MethodImpl(AggressiveInlining)] public float MagnitudeSquared() => Value.LengthSquared();
        [MethodImpl(AggressiveInlining)] public Vector2 Zero() => new(0f, 0f);
        [MethodImpl(AggressiveInlining)] public Vector2 One() => new(1f, 1f);
        [MethodImpl(AggressiveInlining)] public Vector2 MinValue() => new(float.MinValue, float.MinValue);
        [MethodImpl(AggressiveInlining)] public Vector2 MaxValue() => new(float.MaxValue, float.MaxValue);
    }

    public partial struct Vector3
    {
        public float Magnitude { [MethodImpl(AggressiveInlining)] get => Value.Length(); }
        [MethodImpl(AggressiveInlining)] public int Hash() => Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public Vector3 Lerp(Vector3 b, float t) => SNVector3.Lerp(Value, b, t);
        [MethodImpl(AggressiveInlining)] public float MagnitudeSquared() => Value.LengthSquared();
        [MethodImpl(AggressiveInlining)] public Vector3 Zero() => new(0f, 0f, 0f);
        [MethodImpl(AggressiveInlining)] public Vector3 One() => new(1f, 1f, 1f);
        [MethodImpl(AggressiveInlining)] public Vector3 MinValue() => new(float.MinValue, float.MinValue, float.MinValue);
        [MethodImpl(AggressiveInlining)] public Vector3 MaxValue() => new(float.MaxValue, float.MaxValue, float.MaxValue);
    }

    public partial struct Vector4
    {
        public float Magnitude { [MethodImpl(AggressiveInlining)] get => Value.Length(); }
        [MethodImpl(AggressiveInlining)] public int Hash() => Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public Vector4 Lerp(Vector4 b, float t) => SNVector4.Lerp(Value, b, t);
        [MethodImpl(AggressiveInlining)] public float MagnitudeSquared() => Value.LengthSquared();
        [MethodImpl(AggressiveInlining)] public Vector4 Zero() => new(0f, 0f, 0f, 0f);
        [MethodImpl(AggressiveInlining)] public Vector4 One() => new(1f, 1f, 1f, 1f);
        [MethodImpl(AggressiveInlining)] public Vector4 MinValue() => new(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
        [MethodImpl(AggressiveInlining)] public Vector4 MaxValue() => new(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
    }

    public partial struct Quaternion
    {
        public float Magnitude { [MethodImpl(AggressiveInlining)] get => Value.Length(); }
        [MethodImpl(AggressiveInlining)] public Quaternion Lerp(Quaternion b, float t) => SNQuaternion.Lerp(Value, b.Value, t);
    }

    public partial struct Angle
    {
        [MethodImpl(AggressiveInlining)] public int Compare(Angle b) => Value.CompareTo(b.Value);
        [MethodImpl(AggressiveInlining)] public int Hash() => Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public Angle Lerp(Angle b, float t) => Value + (b.Value - Value) * t;
    }

    public partial struct Matrix3x2
    {
        public int RowCount { [MethodImpl(AggressiveInlining)] get => 3; }
        [MethodImpl(AggressiveInlining)] public int ColumnCount() => 2;

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator -(Matrix3x2 m) => new System.Numerics.Matrix3x2(-m.Value.M11, -m.Value.M12, -m.Value.M21, -m.Value.M22, -m.Value.M31, -m.Value.M32);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator %(Matrix3x2 m, float s)
            => new System.Numerics.Matrix3x2(m.Value.M11 % s, m.Value.M12 % s, m.Value.M21 % s, m.Value.M22 % s, m.Value.M31 % s, m.Value.M32 % s);
        [MethodImpl(AggressiveInlining)]
        public float ElementAt(int row, int column) => row switch
        {
            0 => column == 0 ? Value.M11 : Value.M12,
            1 => column == 0 ? Value.M21 : Value.M22,
            _ => column == 0 ? Value.M31 : Value.M32,
        };
    }

    public partial struct Matrix4x4
    {
        public int RowCount { [MethodImpl(AggressiveInlining)] get => 4; }
        [MethodImpl(AggressiveInlining)] public int ColumnCount() => 4;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator -(Matrix4x4 m) => new System.Numerics.Matrix4x4(
            -m.Value.M11, -m.Value.M12, -m.Value.M13, -m.Value.M14,
            -m.Value.M21, -m.Value.M22, -m.Value.M23, -m.Value.M24,
            -m.Value.M31, -m.Value.M32, -m.Value.M33, -m.Value.M34,
            -m.Value.M41, -m.Value.M42, -m.Value.M43, -m.Value.M44);

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator %(Matrix4x4 m, float s) => new System.Numerics.Matrix4x4(
            m.Value.M11 % s, m.Value.M12 % s, m.Value.M13 % s, m.Value.M14 % s,
            m.Value.M21 % s, m.Value.M22 % s, m.Value.M23 % s, m.Value.M24 % s,
            m.Value.M31 % s, m.Value.M32 % s, m.Value.M33 % s, m.Value.M34 % s,
            m.Value.M41 % s, m.Value.M42 % s, m.Value.M43 % s, m.Value.M44 % s);
        public float ElementAt(int row, int column) => (row, column) switch
        {
            (0, 0) => Value.M11, (0, 1) => Value.M12, (0, 2) => Value.M13, (0, 3) => Value.M14,
            (1, 0) => Value.M21, (1, 1) => Value.M22, (1, 2) => Value.M23, (1, 3) => Value.M24,
            (2, 0) => Value.M31, (2, 1) => Value.M32, (2, 2) => Value.M33, (2, 3) => Value.M34,
            _ => column switch { 0 => Value.M41, 1 => Value.M42, 2 => Value.M43, _ => Value.M44 },
        };
    }

    public partial struct Plane
    {
        /// <summary>The plane's signed distance from the origin (v3 field name; wraps System.Numerics.Plane.D).</summary>
        public float Distance { [MethodImpl(AggressiveInlining)] get => Value.D; }

        /// <summary>The closest point on the plane to the given point (orthogonal projection).</summary>
        [MethodImpl(AggressiveInlining)]
        public Point3D ClosestPoint(Point3D point)
        {
            var p = new SNVector3(point.X, point.Y, point.Z);
            var d = SNPlane.DotCoordinate(Value, p);
            var q = p - Value.Normal * d;
            return new Point3D(q.X, q.Y, q.Z);
        }
    }

    // ============================================================================
    // Wrapper-receiver bridges the generated code does not emit: call sites where
    // a Number/Integer-typed intermediate receives a scalar-erased member.
    // ============================================================================

    public static class WrapperReceiverBridges
    {
        [MethodImpl(AggressiveInlining)] public static int Compare(this Number a, float b) => a.Value.CompareTo(b);
        [MethodImpl(AggressiveInlining)] public static int Hash(this Number x) => x.Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public static float Inverse(this Number x) => 1f / x.Value;
        [MethodImpl(AggressiveInlining)] public static float Lerp(this Number a, float b, float t) => a.Value + (b - a.Value) * t;
        [MethodImpl(AggressiveInlining)] public static float MaxValue(this Number _) => float.MaxValue;
        [MethodImpl(AggressiveInlining)] public static float MinValue(this Number _) => float.MinValue;
        [MethodImpl(AggressiveInlining)] public static float One(this Number _) => 1f;
        [MethodImpl(AggressiveInlining)] public static float Zero(this Number _) => 0f;
        [MethodImpl(AggressiveInlining)] public static float Pow2(this Number x) => x.Value * x.Value;
        [MethodImpl(AggressiveInlining)] public static float Pow3(this Number x) => x.Value * x.Value * x.Value;
        [MethodImpl(AggressiveInlining)] public static float ToNumber(this Number x) => x.Value;

        [MethodImpl(AggressiveInlining)] public static int BitwiseNot(this Integer x) => ~x.Value;
        [MethodImpl(AggressiveInlining)] public static int BitwiseXor(this Integer a, int b) => a.Value ^ b;
        [MethodImpl(AggressiveInlining)] public static int Compare(this Integer a, int b) => a.Value.CompareTo(b);
        [MethodImpl(AggressiveInlining)] public static int Hash(this Integer x) => x.Value.GetHashCode();
        [MethodImpl(AggressiveInlining)] public static int MaxValue(this Integer _) => int.MaxValue;
        [MethodImpl(AggressiveInlining)] public static int MinValue(this Integer _) => int.MinValue;
        [MethodImpl(AggressiveInlining)] public static int One(this Integer _) => 1;
        [MethodImpl(AggressiveInlining)] public static int Zero(this Integer _) => 0;
        [MethodImpl(AggressiveInlining)] public static int ShiftLeft(this Integer a, int b) => a.Value << b;
        [MethodImpl(AggressiveInlining)] public static int ShiftRight(this Integer a, int b) => a.Value >> b;
        [MethodImpl(AggressiveInlining)] public static int ToInteger(this Integer x) => x.Value;
    }

    // ============================================================================
    // Existential-field cascade: v3 types with concept-typed fields (Curve2D /
    // Curve3D / ParametricSurface). The emitter instantiates the concept with
    // Self = the containing type, so the containing type must itself satisfy the
    // concept. These explicit implementations forward to the stored curve/surface.
    // ============================================================================

    public partial struct CoonsPatch : Curve3D<CoonsPatch>
    {
        [MethodImpl(AggressiveInlining)] Point3D Procedural<float, Point3D>.Eval(float t) => Bottom.Eval(t);
    }

    public partial struct ExtrudedSurface : Curve3D<ExtrudedSurface>
    {
        [MethodImpl(AggressiveInlining)] Point3D Procedural<float, Point3D>.Eval(float t) => Profile.Eval(t);
    }

    public partial struct RuledSurface : Curve3D<RuledSurface>
    {
        [MethodImpl(AggressiveInlining)] Point3D Procedural<float, Point3D>.Eval(float t) => Start.Eval(t);
    }

    public partial struct TubeSurface : Curve3D<TubeSurface>
    {
        [MethodImpl(AggressiveInlining)] Point3D Procedural<float, Point3D>.Eval(float t) => Path.Eval(t);
    }

    public partial struct SweptSolid : Curve3D<SweptSolid>
    {
        [MethodImpl(AggressiveInlining)] Point3D Procedural<float, Point3D>.Eval(float t) => Path.Eval(t);
    }

    public partial struct SurfaceOfRevolution : Curve2D<SurfaceOfRevolution>
    {
        [MethodImpl(AggressiveInlining)] Point2D Procedural<float, Point2D>.Eval(float t) => Profile.Eval(t);
    }

    public partial struct SweptSurface : Curve2D<SweptSurface>, Curve3D<SweptSurface>
    {
        [MethodImpl(AggressiveInlining)] Point2D Procedural<float, Point2D>.Eval(float t) => Profile.Eval(t);
        [MethodImpl(AggressiveInlining)] Point3D Procedural<float, Point3D>.Eval(float t) => Path.Eval(t);
    }

    public partial struct TrimmedSurface : ParametricSurface<TrimmedSurface>
    {
        [MethodImpl(AggressiveInlining)] Point3D Procedural<UvCoordinate, Point3D>.Eval(UvCoordinate uv) => Base.Eval(uv);
        [MethodImpl(AggressiveInlining)] bool ParametricSurface<TrimmedSurface>.ClosedU() => Base.ClosedU();
        [MethodImpl(AggressiveInlining)] bool ParametricSurface<TrimmedSurface>.ClosedV() => Base.ClosedV();
    }
}
