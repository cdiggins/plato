using System.Runtime.CompilerServices;
using SNPlane = System.Numerics.Plane;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// Behaviour-only partial for the GENERATED <c>Plane</c> struct (plato-365).
    ///
    /// The shape is the stdlib declaration (`stdlib/geometry/lines-planes.types.plato`):
    /// <c>Normal: Direction3D</c> plus <c>Distance: Number</c>, Hesse normal form
    /// (https://en.wikipedia.org/wiki/Hesse_normal_form). System.Numerics is now an
    /// IMPLEMENTATION DETAIL of the bodies below rather than the type itself, reached through
    /// <see cref="Sys"/>.
    ///
    /// SIGN CONVENTION — the reason this conversion is a named, commented helper rather than a bit
    /// cast. Plato's Hesse form is `Dot(Normal, p) == Distance`; System.Numerics.Plane's form is
    /// `Dot(Normal, p) + D == 0`. So <c>D == -Distance</c>. The wrapper this file replaces
    /// exposed `Distance => Value.D` with no negation, which disagreed with the declaration it was
    /// supposedly implementing — exactly the silent wrong bits plato-365 exists to make impossible.
    ///
    /// Nothing here may declare a field, a constructor, a `With*`, or an equality override: the
    /// generated partial has them, and a second copy is a duplicate-member error.
    /// </summary>
    public partial struct Plane
    {
        /// <summary>This plane as a System.Numerics plane. Same four floats, with the Hesse
        /// distance negated into System.Numerics' `D` (see the type remarks).</summary>
        /// <remarks>Internal, not a public implicit conversion: System.Numerics must not be
        /// reachable from the generated code, only from handwritten intrinsic bodies (the
        /// Matrix4x4 reflection/shadow factories are the other caller).</remarks>
        internal SNPlane Sys
        {
            [MethodImpl(AggressiveInlining)]
            get => new SNPlane(((Vector3)Normal).Value, -Distance);
        }

        /// <summary>The inverse of <see cref="Sys"/>.</summary>
        [MethodImpl(AggressiveInlining)]
        internal static Plane FromSys(SNPlane p)
            => new Plane(new Vector3(p.Normal), -p.D);

        // --------------------------------------------------------------------
        // Intrinsic surface — stdlib/geometry/intrinsics-planes.library.plato
        // --------------------------------------------------------------------

        /// <summary>A copy of the plane whose normal has length 1.</summary>
        [MethodImpl(AggressiveInlining)]
        public Plane Normalize()
            => FromSys(SNPlane.Normalize(Sys));

        /// <summary>The plane through three points. Declared with an ignored receiver of the
        /// result type (`CreateFromVertices(_: Plane, ...)`), so it emits as a static.</summary>
        [MethodImpl(AggressiveInlining)]
        public static Plane CreateFromVertices(Vector3 point1, Vector3 point2, Vector3 point3)
            => FromSys(SNPlane.CreateFromVertices(point1, point2, point3));

        /// <summary>The dot product of the plane's four coefficients with a 4D vector.</summary>
        [MethodImpl(AggressiveInlining)]
        public float Dot(Vector4 value)
            => SNPlane.Dot(Sys, value);

        /// <summary>The signed distance from a point to the plane, measured along the normal:
        /// `Dot(Normal, p) - Distance`.</summary>
        [MethodImpl(AggressiveInlining)]
        public float DotCoordinate(Vector3 value)
            => SNPlane.DotCoordinate(Sys, value);

        /// <summary>The dot product of the plane's normal with a direction.</summary>
        [MethodImpl(AggressiveInlining)]
        public float DotNormal(Vector3 value)
            => SNPlane.DotNormal(Sys, value);

        /// <summary>Transformed by a rotation.</summary>
        [MethodImpl(AggressiveInlining)]
        public Plane Transform(Quaternion rotation)
            => FromSys(SNPlane.Transform(Sys, rotation));

        /// <summary>Transformed by a 4x4 matrix.</summary>
        [MethodImpl(AggressiveInlining)]
        public Plane Transform(Matrix4x4 matrix)
            => FromSys(SNPlane.Transform(Sys, matrix));

        /// <summary>Hashable obligation; exact `int` return (see Angle.Hash).</summary>
        [MethodImpl(AggressiveInlining)]
        public int Hash()
            => Sys.GetHashCode();
    }
}
