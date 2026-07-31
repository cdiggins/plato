using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SNPlane = System.Numerics.Plane;

namespace Ara3D.Geometry
{
    /// <summary>
    /// A lightweight wrapper around <see cref="System.Numerics.Plane"/>.
    /// </summary>
    [DataContract]
    public partial struct Plane
    {
        // --------------------------------------------------------------------
        // Fields
        // --------------------------------------------------------------------
        
        [DataMember] public readonly SNPlane Value;

        // --------------------------------------------------------------------
        // Properties
        // --------------------------------------------------------------------

        public Vector3 Normal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.Normal;
        }

        public Number D
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value.D;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane WithNormal(Vector3 normal)
            => new(normal, D);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane WithD(Number d)
            => new(Normal, d);

        // --------------------------------------------------------------------
        // Constructors
        // --------------------------------------------------------------------

        /// <summary>
        /// Creates a new <see cref="Plane"/> with the specified normal and distance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(Vector3 normal, Number d)
            => Value = new(normal, d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(SNPlane plane)
            => Value = plane;

        // --------------------------------------------------------------------
        // Internal: Convert to/from Plane
        // --------------------------------------------------------------------

        /// <summary>
        /// Converts a <see cref="Plane"/> to this wrapper.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane FromSystem(SNPlane plane)
            => Unsafe.As<SNPlane, Plane>(ref plane);

        // --------------------------------------------------------------------
        // Implicit conversions
        // --------------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SNPlane(Plane plane)
            => plane.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Plane(SNPlane plane)
            => FromSystem(plane);

        // --------------------------------------------------------------------
        // Static Methods (forwarded to Plane)
        // --------------------------------------------------------------------

        /// <summary>
        /// Creates a <see cref="Plane"/> from three vertices.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane CreateFromVertices(Vector3 point1, Vector3 point2, Vector3 point3)
            => SNPlane.CreateFromVertices(point1, point2, point3);

        /// <summary>
        /// Returns the dot product of a plane and a 4D vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Dot(Vector4 value)
            => SNPlane.Dot(Value, value);

        /// <summary>
        /// Returns the dot product of a plane's normal with a 3D coordinate, plus the plane's D value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DotCoordinate(Vector3 value)
            => SNPlane.DotCoordinate(Value, value);

        /// <summary>
        /// Returns the dot product of a plane's normal with a 3D normal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float DotNormal(Vector3 value)
            => SNPlane.DotNormal(Value, value);

        /// <summary>
        /// Returns a copy of the plane with a normal of length of 1.
        /// </summary>
        public Plane Normalize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SNPlane.Normalize(Value);
        }

        /// <summary>
        /// Transforms by a rotation (Quaternion).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane Transform(Quaternion rotation)
            => SNPlane.Transform(Value, rotation);

        /// <summary>
        /// Transforms by a 4x4 matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane Transform(Matrix4x4 matrix)
            => SNPlane.Transform(Value, matrix);
    }
}
