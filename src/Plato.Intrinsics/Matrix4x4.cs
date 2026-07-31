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

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateRotationX(Angle angle)
            => SNMatrix4x4.CreateRotationX(angle);

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateRotationY(Angle angle)
            => SNMatrix4x4.CreateRotationY(angle);

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateRotationZ(Angle angle)
            => SNMatrix4x4.CreateRotationZ(angle);

        // --------------------------------------------------------------------------------
        // Decompose, Determinant, Transpose, etc. (common instance methods)
        // --------------------------------------------------------------------------------

        public Vector3 Translation { [MethodImpl(AggressiveInlining)] get => Sys.Translation; }

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithTranslation(Vector3 translation)
        {
            var matrix = Sys;
            matrix.Translation = translation;
            return matrix;
        }

        /// <summary>
        /// Attempts to extract scale, rotation (as a <see cref="Quaternion"/>),
        /// and translation from this matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public (Vector3, Quaternion, Vector3, Boolean) Decompose()
        {
            var success = SNMatrix4x4.Decompose(Sys, out var scl, out var rot, out var trans);
            return (trans, rot, scl, success);
        }

        public Quaternion Rotation { [MethodImpl(AggressiveInlining)] get => Decompose().Item2; }

        [MethodImpl(AggressiveInlining)] public Number Determinant() => Sys.GetDeterminant();

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

        /// <summary>
        /// Creates a rotation matrix from a quaternion.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateFromQuaternion(Quaternion quaternion)
            => SNMatrix4x4.CreateFromQuaternion(quaternion);

        /// <summary>
        /// Creates a rotation matrix from yaw, pitch, and roll values (in radians).
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateFromYawPitchRoll(Angle yaw, Angle pitch, Angle roll)
            => SNMatrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);

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

        /// <summary>
        /// Creates a matrix that reflects coordinates about a specified plane.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateReflection(Plane value)
            => SNMatrix4x4.CreateReflection(value.Sys);

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

        // Exact `int`/`float` signatures: these discharge the (scalar-erased) MatrixLike
        // obligations the forward-stdlib generation declares on the Matrix4x4 partial, and
        // interface implementation demands the exact erased types.

        [MethodImpl(AggressiveInlining)] public int Hash() => Sys.GetHashCode();

        [MethodImpl(AggressiveInlining)] public int RowCount() => 4;

        [MethodImpl(AggressiveInlining)] public int ColumnCount() => 4;

        [MethodImpl(AggressiveInlining)]
        public float ElementAt(int row, int column)
        {
            // Straight off the declared rows: no System.Numerics round-trip, and the row/column
            // order is the declaration's own (RowN holds row N; component c is column c).
            var r = row switch
            {
                0 => Row1, 1 => Row2, 2 => Row3, 3 => Row4,
                _ => throw new ArgumentOutOfRangeException($"row {row} is outside a 4x4 matrix"),
            };
            return column switch
            {
                0 => r.X, 1 => r.Y, 2 => r.Z, 3 => r.W,
                _ => throw new ArgumentOutOfRangeException($"column {column} is outside a 4x4 matrix"),
            };
        }
    }
}