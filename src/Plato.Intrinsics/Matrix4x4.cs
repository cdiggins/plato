using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SNMatrix4x4 = System.Numerics.Matrix4x4;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// Behaviour-only partial for the GENERATED <c>Matrix4x4</c> struct (plato-365).
    ///
    /// The shape is the stdlib declaration (`stdlib/foundation/matrices.types.plato`): four
    /// <c>Number4</c> ROWS, so element (r, c) is component c of row r. System.Numerics is an
    /// implementation detail of the bodies below, reached through <see cref="Sys"/>.
    ///
    /// ROW-WISE conversion, and why it is written out longhand: System.Numerics.Matrix4x4 names its
    /// elements M11..M44 and its constructor takes them in row-major order, so `Row1.X .. Row4.W`
    /// maps position-for-position onto that constructor with no index arithmetic to get wrong. The
    /// M-names appear ONLY here, inside Plato.Intrinsics — the writer must never learn them
    /// (plato-365: that would re-create the invisible-primitiveness this issue deletes).
    ///
    /// Nothing here may declare a field, a constructor, a `Row*`, a `WithRow*`, or an equality
    /// override: the generated partial has them, and a second copy is a duplicate-member error.
    /// </summary>
    public partial struct Matrix4x4
    {
        /// <summary>This matrix as a System.Numerics matrix: the same sixteen floats, row by row.</summary>
        internal SNMatrix4x4 Sys
        {
            [MethodImpl(AggressiveInlining)]
            get => new SNMatrix4x4(
                Row1.X, Row1.Y, Row1.Z, Row1.W,
                Row2.X, Row2.Y, Row2.Z, Row2.W,
                Row3.X, Row3.Y, Row3.Z, Row3.W,
                Row4.X, Row4.Y, Row4.Z, Row4.W);
        }

        /// <summary>The inverse of <see cref="Sys"/>.</summary>
        [MethodImpl(AggressiveInlining)]
        internal static Matrix4x4 FromSys(SNMatrix4x4 m)
            => new Matrix4x4(
                new Number4(m.M11, m.M12, m.M13, m.M14),
                new Number4(m.M21, m.M22, m.M23, m.M24),
                new Number4(m.M31, m.M32, m.M33, m.M34),
                new Number4(m.M41, m.M42, m.M43, m.M44));

        // The intrinsic bridge. Public because every handwritten body in this project converts
        // across it; generated code never names SNMatrix4x4.

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNMatrix4x4(Matrix4x4 m) => m.Sys;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Matrix4x4(SNMatrix4x4 m) => FromSys(m);


        // --------------------------------------------------------------------------------
        // Operators (forward to System.Numerics)
        // --------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator +(Matrix4x4 value1, Matrix4x4 value2)
            => value1.Sys + value2.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator -(Matrix4x4 value1, Matrix4x4 value2)
            => value1.Sys - value2.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 value1, Matrix4x4 value2)
            => value1.Sys * value2.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 value1, Number f)
            => value1.Sys * f;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator *(Number f, Matrix4x4 value1)
            => value1.Sys * f;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator /(Matrix4x4 value1, Number f)
            => value1.Sys * f.ReciprocalEstimate();

        // Unary negation and scalar modulo: the generated struct fills its unimplemented
        // Negative()/Modulo(scalar) obligations by forwarding to these operators, so the
        // handwritten runtime has to declare them. Both are component-wise.

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator -(Matrix4x4 value)
            => -value.Sys;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator %(Matrix4x4 value, Number scalar)
            => new Matrix4x4(
                new Number4(value.Row1.X % scalar, value.Row1.Y % scalar, value.Row1.Z % scalar, value.Row1.W % scalar),
                new Number4(value.Row2.X % scalar, value.Row2.Y % scalar, value.Row2.Z % scalar, value.Row2.W % scalar),
                new Number4(value.Row3.X % scalar, value.Row3.Y % scalar, value.Row3.Z % scalar, value.Row3.W % scalar),
                new Number4(value.Row4.X % scalar, value.Row4.Y % scalar, value.Row4.Z % scalar, value.Row4.W % scalar));

        // Transform-on-the-left. The stdlib declares `Multiply(matrix, self)` as an explicit
        // alias for `Transform(self, matrix)` (stdlib/intrinsics-vectors.library.plato).

        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator *(Matrix4x4 matrix, Vector2 self)
            => self.Transform(matrix);

        [MethodImpl(AggressiveInlining)]
        public static Vector3 operator *(Matrix4x4 matrix, Vector3 self)
            => self.Transform(matrix);

        [MethodImpl(AggressiveInlining)]
        public static Vector4 operator *(Matrix4x4 matrix, Vector4 self)
            => self.Transform(matrix);

        // --------------------------------------------------------------------------------
        // Example "Create*" static methods (forwarded)
        // --------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateTranslation(Vector3 position)
            => SNMatrix4x4.CreateTranslation(position);

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateScale(Number scale)
            => SNMatrix4x4.CreateScale(scale);

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateScale(Number xScale, Number yScale, Number zScale)
            => SNMatrix4x4.CreateScale(xScale, yScale, zScale);

        /// <summary>Per-axis scale. Declared by the forward stdlib as
        /// <c>CreateScale(_: Matrix4x4, scales: Number3)</c>; Matrix3x2 already had the
        /// vector-valued twin, Matrix4x4 did not.</summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateScale(Vector3 scales)
            => SNMatrix4x4.CreateScale(scales);

        // CreateRotationX/Y/Z are NOT here: the forward stdlib has reference bodies for them
        // (matrices-ops.library.plato), so the generated partial declares them.

        // --------------------------------------------------------------------------------
        // Decompose, Determinant, Transpose, etc. (common instance methods)
        // --------------------------------------------------------------------------------

        // `Translation` and `Rotation` are NOT here either — same reason, and the generated
        // pair reads the declared rows rather than round-tripping through System.Numerics.

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithTranslation(Vector3 translation)
        {
            var matrix = Sys;
            matrix.Translation = translation;
            return matrix;
        }

        /// <summary>
        /// Attempts to extract scale, rotation (as a <see cref="Quaternion"/>), and translation
        /// from this matrix. The forward stdlib declares `Decompose` returning a Plato
        /// `Tuple4`, and the generated `Rotation` reads `.X1` off it, so an instance method
        /// returning a C# ValueTuple would shadow the generated extension with an
        /// incompatible shape. Named `DecomposeSys` to stay out of that overload set.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public (Vector3, Quaternion, Vector3, Boolean) DecomposeSys()
        {
            var success = SNMatrix4x4.Decompose(Sys, out var scl, out var rot, out var trans);
            return (trans, rot, scl, success);
        }

        // `Determinant()` is NOT here: the forward stdlib has a reference body for it. An instance
        // method shadows that extension and returns the WRAPPER (Number) where the generated one
        // returns float, which makes call sites that mix the two ambiguous (CS0172).

        [MethodImpl(AggressiveInlining)] public Matrix4x4 Transpose() => SNMatrix4x4.Transpose(Sys);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 Lerp(Matrix4x4 matrix2, Number amount)
            => SNMatrix4x4.Lerp(Sys, matrix2.Sys, amount);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 Invert()
        {
            var success = SNMatrix4x4.Invert(Sys, out var result);
            if (!success) throw new InvalidOperationException("Non-invertible matrix    ");
            return result;
        }

        [MethodImpl(AggressiveInlining)] public bool CanInvert() => SNMatrix4x4.Invert(Sys, out var _);

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreatePerspectiveFieldOfView(Number fieldOfView, Number aspectRatio, Number nearPlane, Number farPlane)
            => SNMatrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlane, farPlane);

        /// <summary>
        /// Creates a spherical billboard that rotates around a specified object position.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3 cameraForwardVector)
            => SNMatrix4x4.CreateBillboard(
                objectPosition, cameraPosition, cameraUpVector, cameraForwardVector);

        /// <summary>
        /// Creates a cylindrical billboard that rotates around a specified axis.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 rotateAxis, Vector3 cameraForwardVector, Vector3 objectForwardVector)
            => SNMatrix4x4.CreateConstrainedBillboard(objectPosition, cameraPosition, rotateAxis, cameraForwardVector, objectForwardVector);

        /// <summary>
        /// Creates a rotation matrix from a specified axis and angle.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateFromAxisAngle(Vector3 axis, Angle angle)
            => SNMatrix4x4.CreateFromAxisAngle(axis, angle);

        /// <summary>
        /// Creates a rotation matrix from a specified axis and angle.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateFromAxisAngleWithPivot(Vector3 axis, Angle angle, Vector3 pivot)
            => CreateFromAxisAngle(axis, angle).WithPivot(pivot);

        // CreateFromQuaternion / CreateFromYawPitchRoll are NOT here: the forward stdlib has
        // reference bodies for both, so the generated partial declares them.

        /// <summary>
        /// Creates a view matrix for a camera looking at a target.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
            => SNMatrix4x4.CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateOrthographic(Number width, Number height, Number zNearPlane, Number zFarPlane)
            => SNMatrix4x4.CreateOrthographic(width, height, zNearPlane, zFarPlane);

        /// <summary>
        /// Creates a customized orthographic projection matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateOrthographicOffCenter(Number left, Number right, Number bottom, Number top, Number zNearPlane, Number zFarPlane)
            => SNMatrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane);

        /// <summary>
        /// Creates a perspective projection matrix based on a width and height.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreatePerspective(Number width, Number height, Number nearPlaneDistance, Number farPlaneDistance)
            => SNMatrix4x4.CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance);

        /// <summary>
        /// Creates a customized perspective projection matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreatePerspectiveOffCenter(Number left, Number right, Number bottom, Number top, Number nearPlaneDistance, Number farPlaneDistance)
            => SNMatrix4x4.CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance);

        // CreateReflection is NOT here: same reason (reference body in the forward stdlib).

        /// <summary>
        /// Creates a matrix that flattens geometry into a specified plane as if casting a shadow from a light source.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateShadow(Vector3 lightDirection, Plane plane)
            => SNMatrix4x4.CreateShadow(lightDirection, plane.Sys);

        /// <summary>
        /// Creates a world matrix with the specified parameters.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
            => SNMatrix4x4.CreateWorld(position, forward, up);

        //==

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithPivot(Vector3 pivot)
            => CreateTranslation(pivot) * this * CreateTranslation(-pivot);

        // Exact `int` signature: this discharges the (scalar-erased) Hashable obligation the
        // forward-stdlib generation declares on the Matrix4x4 partial, and interface
        // implementation demands the exact erased type.
        //
        // RowCount / ColumnCount / ElementAt used to live here for the same reason. They moved
        // to the declaration (`stdlib/foundation/matrices-dense.library.plato`, plato-321) and
        // were deleted here in the same change: with bodies on both sides the generated partial
        // and this one declare the same member, which is CS0111 (plato-375).

        [MethodImpl(AggressiveInlining)] public int Hash() => Sys.GetHashCode();
    }
}