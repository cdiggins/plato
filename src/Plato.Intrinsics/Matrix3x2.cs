using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using SNMatrix3x2 = System.Numerics.Matrix3x2;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Ara3D.Geometry
{
    /// <summary>
    /// Behaviour-only partial for the GENERATED <c>Matrix3x2</c> struct (plato-365).
    ///
    /// The shape is the stdlib declaration (`stdlib/foundation/matrices.types.plato`): three
    /// <c>Number2</c> ROWS — a 2x2 linear part plus a translation row. System.Numerics is an
    /// implementation detail of the bodies below, reached through <see cref="Sys"/>.
    ///
    /// ROW-WISE conversion, written out longhand for the same reason as Matrix4x4: the M-names
    /// (M11..M32) appear ONLY inside Plato.Intrinsics, never in the writer.
    ///
    /// Nothing here may declare a field, a constructor, a `Row*`, a `WithRow*`, or an equality
    /// override: the generated partial has them, and a second copy is a duplicate-member error.
    /// </summary>
    public partial struct Matrix3x2
    {
        /// <summary>This matrix as a System.Numerics matrix: the same six floats, row by row.</summary>
        internal SNMatrix3x2 Sys
        {
            [MethodImpl(AggressiveInlining)]
            get => new SNMatrix3x2(
                Row1.X, Row1.Y,
                Row2.X, Row2.Y,
                Row3.X, Row3.Y);
        }

        /// <summary>The inverse of <see cref="Sys"/>.</summary>
        [MethodImpl(AggressiveInlining)]
        internal static Matrix3x2 FromSystem(SNMatrix3x2 m)
            => new Matrix3x2(
                new Number2(m.M11, m.M12),
                new Number2(m.M21, m.M22),
                new Number2(m.M31, m.M32));

        // The intrinsic bridge. Public because every handwritten body in this project converts
        // across it; generated code never names SNMatrix3x2.

        [MethodImpl(AggressiveInlining)]
        public static implicit operator SNMatrix3x2(Matrix3x2 mat) => mat.Sys;

        [MethodImpl(AggressiveInlining)]
        public static implicit operator Matrix3x2(SNMatrix3x2 sysMat) => FromSystem(sysMat);

        // -------------------------------------------------------------------------------
        // Operators
        // -------------------------------------------------------------------------------

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator +(Matrix3x2 value1, Matrix3x2 value2)
            => FromSystem(value1.Sys + value2.Sys);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator -(Matrix3x2 value1, Matrix3x2 value2)
            => FromSystem(value1.Sys - value2.Sys);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator *(Matrix3x2 value1, Matrix3x2 value2)
            => FromSystem(value1.Sys * value2.Sys);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator *(Matrix3x2 value1, Number scalar)
            => FromSystem(value1.Sys * scalar);

        // Transform-on-the-left. The stdlib declares `Multiply(matrix, self)` as an explicit
        // alias for `Transform(self, matrix)` (stdlib/intrinsics-vectors.library.plato).
        [MethodImpl(AggressiveInlining)]
        public static Vector2 operator *(Matrix3x2 matrix, Vector2 self)
            => self.Transform(matrix);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator *(Number scalar, Matrix3x2 value1)
            => value1 * scalar;

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator /(Matrix3x2 value1, Number scalar)
            => value1 * (1f / scalar);

        // Unary negation and scalar modulo: the generated struct fills its unimplemented
        // Negative()/Modulo(scalar) obligations by forwarding to these operators, so the
        // handwritten runtime has to declare them. Both are component-wise.

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator -(Matrix3x2 value)
            => FromSystem(-value.Sys);

        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 operator %(Matrix3x2 value, Number scalar)
            => new Matrix3x2(
                new Number2(value.Row1.X % scalar, value.Row1.Y % scalar),
                new Number2(value.Row2.X % scalar, value.Row2.Y % scalar),
                new Number2(value.Row3.X % scalar, value.Row3.Y % scalar));

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

        // The Angle-taking `CreateRotation(angle)` is NOT here: the forward stdlib has a
        // reference body for it (`CreateRotation(_: Matrix3x2, angle: Angle)`), so the generated
        // partial declares it. The Number overloads above serve the legacy generation, which
        // spells the parameter `radians: Number`.
        [MethodImpl(AggressiveInlining)]
        public static Matrix3x2 CreateRotation(Angle angle, Vector2 centerPoint)
            => SNMatrix3x2.CreateRotation(angle, centerPoint);

        // -------------------------------------------------------------------------------
        // Other useful static methods: Invert, Lerp, etc.
        // -------------------------------------------------------------------------------
        
        // `Invert()` is NOT here: the forward stdlib declares it returning Tuple2<Matrix3x2,
        // Boolean> and has a reference body for it. An instance method returning a C# ValueTuple
        // shadows that generated extension and gives call sites a tuple with no .X0/.X1.

        [MethodImpl(AggressiveInlining)]
        public Matrix3x2 Lerp(Matrix3x2 matrix2, Number amount)
            => FromSystem(SNMatrix3x2.Lerp(Sys, matrix2.Sys, amount));

        // -------------------------------------------------------------------------------
        // Instance methods
        // -------------------------------------------------------------------------------

        /// <summary>
        /// Gets the determinant of this 3x2 matrix.
        /// </summary>
        [MethodImpl(AggressiveInlining)] public float Determinant() => Sys.GetDeterminant();

        // Exact `int`/`float` signatures: these discharge the (scalar-erased) MatrixLike
        // obligations the forward-stdlib generation declares on the Matrix3x2 partial, and
        // interface implementation demands the exact erased types.

        [MethodImpl(AggressiveInlining)] public int Hash() => Sys.GetHashCode();

        [MethodImpl(AggressiveInlining)] public int RowCount() => 3;

        [MethodImpl(AggressiveInlining)] public int ColumnCount() => 2;

        [MethodImpl(AggressiveInlining)]
        public float ElementAt(int row, int column)
        {
            // Straight off the declared rows: no System.Numerics round-trip.
            var r = row switch
            {
                0 => Row1, 1 => Row2, 2 => Row3,
                _ => throw new ArgumentOutOfRangeException($"row {row} is outside a 3x2 matrix"),
            };
            return column switch
            {
                0 => r.X, 1 => r.Y,
                _ => throw new ArgumentOutOfRangeException($"column {column} is outside a 3x2 matrix"),
            };
        }
    }
}
