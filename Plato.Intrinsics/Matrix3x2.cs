using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SNMatrix3x2 = System.Numerics.Matrix3x2;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    [DataContract]
    public partial struct Matrix3x2 
    {
        // -------------------------------------------------------------------------------
        // Fields (layout must match System.Numerics.Matrix3x2)
        // -------------------------------------------------------------------------------

        [DataMember] public readonly SNMatrix3x2 Value;

        // -------------------------------------------------------------------------------
        // Constructors
        // -------------------------------------------------------------------------------
        
        [MethodImpl(AggressiveInlining)]
        public Matrix3x2(SNMatrix3x2 v) 
            => Value = v;

        [MethodImpl(AggressiveInlining)]
        public Matrix3x2(
            Number m11, Number m12,
            Number m21, Number m22,
            Number m31, Number m32)
            => Value = new(m11, m12, m21, m22, m31, m32);
        
        [MethodImpl(AggressiveInlining)]
        public Matrix3x2(
            Vector2 row1,
            Vector2 row2,
            Vector2 row3)
            => Value = new(row1.X, row1.Y, row2.X,row2.Y, row3.X, row3.Y);

        //-------------------------------------------------------------------------------------
        // Properties
        //-------------------------------------------------------------------------------------

        public Vector2 Row1 { [MethodImpl(AggressiveInlining)] get => new(Value.M11, Value.M12); }
        public Vector2 Row2 { [MethodImpl(AggressiveInlining)] get => new(Value.M21, Value.M22); }
        public Vector2 Row3 { [MethodImpl(AggressiveInlining)] get => new(Value.M31, Value.M32); }

        public Number M11 => Value.M11;
        public Number M12 => Value.M12;
        public Number M21 => Value.M21;
        public Number M22 => Value.M22;
        public Number M31 => Value.M31;
        public Number M32 => Value.M32;
        //-------------------------------------------------------------------------------------
        // Immutable "setters"
        //-------------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public Matrix3x2 WithRow1(Vector2 v) => new(v, Row2, Row3);

        [MethodImpl(AggressiveInlining)]
        public Matrix3x2 WithRow2(Vector2 v) => new(Row1, v, Row3);

        [MethodImpl(AggressiveInlining)]
        public Matrix3x2 WithRow3(Vector2 v) => new(Row1, Row2, v);

        // -------------------------------------------------------------------------------
        // Convert to/from System.Numerics.Matrix3x2 
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 FromSystem(SNMatrix3x2 sysMat)
            => Unsafe.As<SNMatrix3x2, Matrix3x2>(ref sysMat);

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNMatrix3x2(Matrix3x2 mat)
            => mat.Value;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Matrix3x2(SNMatrix3x2 sysMat)
            => FromSystem(sysMat);

        // -------------------------------------------------------------------------------
        // Operators
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator +(Matrix3x2 value1, Matrix3x2 value2)
            => FromSystem(value1.Value + value2.Value);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator -(Matrix3x2 value1, Matrix3x2 value2)
            => FromSystem(value1.Value - value2.Value);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator *(Matrix3x2 value1, Matrix3x2 value2)
            => FromSystem(value1.Value * value2.Value);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator *(Matrix3x2 value1, Number scalar)
            => FromSystem(value1.Value * scalar);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator *(Number scalar, Matrix3x2 value1)
            => value1 * scalar;

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator /(Matrix3x2 value1, Number scalar)
            => value1 * (1f / scalar);

        // -------------------------------------------------------------------------------
        // Common 2D transform creation methods (forwarded)
        // -------------------------------------------------------------------------------
        
        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateTranslation(Number xPosition, Number yPosition)
            => SNMatrix3x2.CreateTranslation(xPosition, yPosition);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateTranslation(Vector2 position)
            => SNMatrix3x2.CreateTranslation(position);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateScale(Number scale)
            => SNMatrix3x2.CreateScale(scale);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateScale(Number xScale, Number yScale)
            => SNMatrix3x2.CreateScale(xScale, yScale);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateScale(Vector2 scales)
            => SNMatrix3x2.CreateScale(scales);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateScale(Number xScale, Number yScale, Vector2 centerPoint)
            => SNMatrix3x2.CreateScale(xScale, yScale, centerPoint);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateRotation(Number radians)
            => SNMatrix3x2.CreateRotation(radians);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateRotation(Number radians, Vector2 centerPoint)
            => SNMatrix3x2.CreateRotation(radians, centerPoint);

        // -------------------------------------------------------------------------------
        // Other useful static methods: Invert, Lerp, etc.
        // -------------------------------------------------------------------------------
        
        public (Matrix3x2, Boolean) Invert
        {
            [MethodImpl(AggressiveInlining)]
            get
            {
                var success = SNMatrix3x2.Invert(Value, out var result);
                return (result, success);
            }
        }

        [MethodImpl(AggressiveInlining)]
        public Matrix3x2 Lerp(Matrix3x2 matrix2, Number amount)
            => FromSystem(SNMatrix3x2.Lerp(Value, matrix2.Value, amount));

        // -------------------------------------------------------------------------------
        // Instance methods
        // -------------------------------------------------------------------------------

        /// <summary>
        /// Gets the determinant of this 3x2 matrix.
        /// </summary>
        public Number Determinant
        {
            [MethodImpl(AggressiveInlining)] get => Value.GetDeterminant();
        }
    }
}
