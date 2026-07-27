using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SNMatrix4x4 = System.Numerics.Matrix4x4;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    [DataContract]
    public partial struct Matrix4x4
    {
        [DataMember] public readonly SNMatrix4x4 Value;

        // --------------------------------------------------------------------------------
        // Constructor
        // --------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4(SNMatrix4x4 matrix) => Value = matrix;

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4(Vector4 row1, Vector4 row2, Vector4 row3, Vector4 row4)
            : this(row1.X, row1.Y, row1.Z, row1.W,
                row2.X, row2.Y, row2.Z, row2.W,
                row3.X, row3.Y, row3.Z, row3.W,
                row4.X, row4.Y, row4.Z, row4.W)
        { }

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4(
            Number m11, Number m12, Number m13, Number m14,
            Number m21, Number m22, Number m23, Number m24,
            Number m31, Number m32, Number m33, Number m34,
            Number m41, Number m42, Number m43, Number m44)
            => Value = new SNMatrix4x4(m11, m12, m13, m14,
                m21, m22, m23, m24,
                m31, m32, m33, m34,
                m41, m42, m43, m44);

        // --------------------------------------------------------------------------------
        // Convert to/from System.Numerics.Matrix4x4
        // --------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 FromSystem(SNMatrix4x4 sysMat)
            => Unsafe.As<SNMatrix4x4, Matrix4x4>(ref sysMat);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Matrix4x4(SNMatrix4x4 m) 
            => FromSystem(m);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNMatrix4x4(Matrix4x4 m) 
            => m.Value;

        //-------------------------------------------------------------------------------------
        // Properties
        //-------------------------------------------------------------------------------------

        public Vector4 Row1 { [MethodImpl(AggressiveInlining)] get => new(Value.M11, Value.M12, Value.M13, Value.M14); }
        public Vector4 Row2 { [MethodImpl(AggressiveInlining)] get => new(Value.M21, Value.M22, Value.M23, Value.M24); }
        public Vector4 Row3 { [MethodImpl(AggressiveInlining)] get => new(Value.M31, Value.M32, Value.M33, Value.M34); }
        public Vector4 Row4 { [MethodImpl(AggressiveInlining)] get => new(Value.M41, Value.M42, Value.M43, Value.M44); }

        //-------------------------------------------------------------------------------------
        // Immutable "setters"
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithRow1(Vector4 v) => new(v, Row2, Row3, Row4);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithRow2(Vector4 v) => new(Row1, v, Row3, Row4);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithRow3(Vector4 v) => new(Row1, Row2, v, Row4);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithRow4(Vector4 v) => new(Row1, Row2, Row3, v);

        // --------------------------------------------------------------------------------
        // Operators (forward to System.Numerics)
        // --------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator +(Matrix4x4 value1, Matrix4x4 value2)
            => value1.Value + value2.Value;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator -(Matrix4x4 value1, Matrix4x4 value2)
            => value1.Value - value2.Value;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 value1, Matrix4x4 value2)
            => value1.Value * value2.Value;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator *(Matrix4x4 value1, Number f)
            => value1.Value * f;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator *(Number f, Matrix4x4 value1)
            => value1.Value * f;

        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 operator /(Matrix4x4 value1, Number f)
            => value1.Value * f.ReciprocalEstimate();

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

        public Vector3 Translation { [MethodImpl(AggressiveInlining)] get => Value.Translation; }

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 WithTranslation(Vector3 translation)
        {
            var matrix = Value;
            matrix.Translation = translation;
            return matrix;
        }

        /// <summary>
        /// Attempts to extract scale, rotation (as a <see cref="Quaternion"/>),
        /// and translation from Value matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public (Vector3, Quaternion, Vector3, Boolean) Decompose()
        {
            var success = SNMatrix4x4.Decompose(Value, out var scl, out var rot, out var trans);
            return (trans, rot, scl, success);
        }

        public Quaternion Rotation { [MethodImpl(AggressiveInlining)] get => Decompose().Item2; }

        [MethodImpl(AggressiveInlining)] public Number Determinant() => Value.GetDeterminant();

        [MethodImpl(AggressiveInlining)] public Matrix4x4 Transpose() => SNMatrix4x4.Transpose(Value);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 Lerp(Matrix4x4 matrix2, Number amount)
            => SNMatrix4x4.Lerp(this.Value, matrix2.Value, amount);

        [MethodImpl(AggressiveInlining)]
        public Matrix4x4 Invert()
        {
            var success = SNMatrix4x4.Invert(Value, out var result);
            if (!success) throw new InvalidOperationException("Non-invertible matrix    ");
            return result;
        }

        [MethodImpl(AggressiveInlining)] public bool CanInvert() => SNMatrix4x4.Invert(Value, out var _);

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
            => SNMatrix4x4.CreateReflection(value);

        /// <summary>
        /// Creates a matrix that flattens geometry into a specified plane as if casting a shadow from a light source.
        /// </summary>
        [MethodImpl(AggressiveInlining)]
        public static Matrix4x4 CreateShadow(Vector3 lightDirection, Plane plane)
            => SNMatrix4x4.CreateShadow(lightDirection, plane);

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

    }
}