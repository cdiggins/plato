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
